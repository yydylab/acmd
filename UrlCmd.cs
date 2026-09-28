using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.Win32;

internal static class UrlCmd
{
    private const string CommandProcessorKey = @"Software\Microsoft\Command Processor";
    private const string MacroMarker = "UrlCmd.exe\" run ping $*";

    private static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return 1;
        }

        switch (args[0].ToLowerInvariant())
        {
            case "install":
                return Install();
            case "uninstall":
                return Uninstall();
            case "run":
                return Run(args.Skip(1).ToArray());
            case "normalize":
                return Normalize(args.Skip(1).ToArray());
            default:
                PrintUsage();
                return 1;
        }
    }

    private static int Install()
    {
        string executable = Process.GetCurrentProcess().MainModule.FileName;
        string macro = BuildMacro(executable);

        using (RegistryKey key = Registry.CurrentUser.CreateSubKey(CommandProcessorKey))
        {
            string current = key.GetValue("AutoRun", string.Empty, RegistryValueOptions.DoNotExpandEnvironmentNames) as string ?? string.Empty;
            if (current.IndexOf(MacroMarker, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Console.WriteLine("UrlCmd is already installed for the current user.");
                return 0;
            }

            string updated = string.IsNullOrWhiteSpace(current) ? macro : current + " & " + macro;
            key.SetValue("AutoRun", updated, RegistryValueKind.String);
        }

        Console.WriteLine("Installed. Open a new CMD window to use URL cleanup.");
        return 0;
    }

    private static int Uninstall()
    {
        string macro = BuildMacro(Process.GetCurrentProcess().MainModule.FileName);
        using (RegistryKey key = Registry.CurrentUser.CreateSubKey(CommandProcessorKey))
        {
            string current = key.GetValue("AutoRun", string.Empty, RegistryValueOptions.DoNotExpandEnvironmentNames) as string ?? string.Empty;
            int markerIndex = current.IndexOf(macro, StringComparison.OrdinalIgnoreCase);
            if (markerIndex < 0)
            {
                Console.WriteLine("UrlCmd is not installed for the current user.");
                return 0;
            }

            int segmentStart = current.LastIndexOf(" & ", markerIndex, StringComparison.Ordinal);
            segmentStart = segmentStart < 0 ? 0 : segmentStart + 3;
            int segmentEnd = markerIndex + macro.Length;

            string updated = current.Remove(segmentStart, segmentEnd - segmentStart);
            updated = updated.Trim();
            if (updated.EndsWith("&", StringComparison.Ordinal))
                updated = updated.Substring(0, updated.Length - 1).TrimEnd();

            if (string.IsNullOrEmpty(updated))
                key.DeleteValue("AutoRun", false);
            else
                key.SetValue("AutoRun", updated, RegistryValueKind.String);
        }

        Console.WriteLine("Uninstalled. New CMD windows will no longer load UrlCmd macros.");
        return 0;
    }

    private static string BuildMacro(string executable)
    {
        string quotedExecutable = QuoteForCmd(executable);
        return string.Join(" & ", new[]
        {
            "doskey ping=" + quotedExecutable + " run ping $*",
            "doskey tracert=" + quotedExecutable + " run tracert $*",
            "doskey nslookup=" + quotedExecutable + " run nslookup $*"
        });
    }

    private static int Run(string[] args)
    {
        if (args.Length == 0 || !IsSupportedCommand(args[0]))
        {
            Console.Error.WriteLine("UrlCmd only runs ping, tracert, or nslookup.");
            return 1;
        }

        string command = args[0].ToLowerInvariant();
        string[] normalized = args.Skip(1).Select(NormalizeArgument).ToArray();
        string commandPath = Path.Combine(Environment.SystemDirectory, command + ".exe");

        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = commandPath,
                Arguments = string.Join(" ", normalized.Select(QuoteForProcess)),
                UseShellExecute = false
            };

            using (Process child = Process.Start(startInfo))
            {
                child.WaitForExit();
                return child.ExitCode;
            }
        }
        catch (Exception error)
        {
            Console.Error.WriteLine("Unable to start {0}: {1}", command, error.Message);
            return 1;
        }
    }

    private static int Normalize(string[] args)
    {
        if (args.Length == 0 || !IsSupportedCommand(args[0]))
        {
            Console.Error.WriteLine("Usage: UrlCmd.exe normalize <ping|tracert|nslookup> [arguments]");
            return 1;
        }

        Console.WriteLine(args[0].ToLowerInvariant() + " " +
            string.Join(" ", args.Skip(1).Select(NormalizeArgument).Select(QuoteForProcess)));
        return 0;
    }

    private static bool IsSupportedCommand(string command)
    {
        return string.Equals(command, "ping", StringComparison.OrdinalIgnoreCase)
            || string.Equals(command, "tracert", StringComparison.OrdinalIgnoreCase)
            || string.Equals(command, "nslookup", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeArgument(string argument)
    {
        string candidate = argument.Trim().Trim('"');
        Uri uri;
        if (Uri.TryCreate(candidate, UriKind.Absolute, out uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            && !string.IsNullOrEmpty(uri.Host))
        {
            string host = uri.Host.Trim('[', ']');
            IPAddress address;
            if (uri.HostNameType == UriHostNameType.IPv6 && IPAddress.TryParse(host, out address))
                return "[" + address + "]";
            return host;
        }

        return argument;
    }

    private static string QuoteForCmd(string value)
    {
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }

    // Implements the Windows argv quoting rules used by CreateProcessW.
    private static string QuoteForProcess(string value)
    {
        if (value.Length > 0 && value.IndexOfAny(new[] { ' ', '\t', '"' }) < 0)
            return value;

        var result = new System.Text.StringBuilder("\"");
        int slashes = 0;
        foreach (char character in value)
        {
            if (character == '\\')
            {
                slashes++;
            }
            else if (character == '"')
            {
                result.Append('\\', slashes * 2 + 1);
                result.Append('"');
                slashes = 0;
            }
            else
            {
                result.Append('\\', slashes);
                result.Append(character);
                slashes = 0;
            }
        }
        result.Append('\\', slashes * 2);
        result.Append('"');
        return result.ToString();
    }

    private static void PrintUsage()
    {
        Console.WriteLine("UrlCmd - remove web URL paths before ping, tracert, and nslookup.");
        Console.WriteLine("  UrlCmd.exe install");
        Console.WriteLine("  UrlCmd.exe uninstall");
        Console.WriteLine("  UrlCmd.exe normalize ping https://example.com/path");
    }
}
