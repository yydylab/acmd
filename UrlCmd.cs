using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.Win32;

internal static class Acmd
{
    private const string CommandProcessorKey = @"Software\Microsoft\Command Processor";
    private const string MacroMarker = "acmd.exe\" run ping $*";
    private const string LegacyMacroMarker = "doskey ping=\"";

    private static readonly IDictionary<string, string> Aliases =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "p", "ping" },
            { "t", "tracert" },
            { "n", "nslookup" },
            { "a", "arp" },
            { "s", "ssh" },
            { "c", "curl" },
            { "f", "ftp" },
            { "m", "mstsc" },
            { "pa", "pathping" },
            { "te", "telnet" },
            { "i", "ipconfig" },
            { "g", "getmac" },
            { "ne", "netsh" },
            { "r", "route" },
            { "nb", "nbtstat" }
        };

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
                Console.WriteLine("ACMD is already installed for the current user.");
                return 0;
            }

            current = RemoveLegacyMacroGroup(current);
            string updated = string.IsNullOrWhiteSpace(current) ? macro : current + " & " + macro;
            key.SetValue("AutoRun", updated, RegistryValueKind.String);
        }

        Console.WriteLine("Installed. Open a new CMD window to use ACMD shortcuts.");
        return 0;
    }

    private static int Uninstall()
    {
        using (RegistryKey key = Registry.CurrentUser.CreateSubKey(CommandProcessorKey))
        {
            string current = key.GetValue("AutoRun", string.Empty, RegistryValueOptions.DoNotExpandEnvironmentNames) as string ?? string.Empty;
            int markerIndex = current.IndexOf(MacroMarker, StringComparison.OrdinalIgnoreCase);
            if (markerIndex < 0)
            {
                Console.WriteLine("ACMD is not installed for the current user.");
                return 0;
            }

            string updated = RemoveMacroGroup(current, MacroMarker);

            if (string.IsNullOrEmpty(updated))
                key.DeleteValue("AutoRun", false);
            else
                key.SetValue("AutoRun", updated, RegistryValueKind.String);
        }

        Console.WriteLine("Uninstalled. New CMD windows will no longer load ACMD macros.");
        return 0;
    }

    private static string BuildMacro(string executable)
    {
        string quotedExecutable = QuoteForCmd(executable);
        return string.Join(" & ", Aliases.Select(alias =>
            "doskey " + alias.Key + "=" + quotedExecutable + " run " + alias.Value + " $*"));
    }

    private static int Run(string[] args)
    {
        if (args.Length == 0 || !IsSupportedCommand(args[0]))
        {
            Console.Error.WriteLine("ACMD only runs supported Windows network commands.");
            return 1;
        }

        string command = args[0].ToLowerInvariant();
        string[] normalized = TransformArguments(command, args.Skip(1).ToArray());
        string commandPath = FindCommandPath(command);

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
            Console.Error.WriteLine("Usage: acmd.exe normalize <command> [arguments]");
            return 1;
        }

        Console.WriteLine(args[0].ToLowerInvariant() + " " +
            string.Join(" ", TransformArguments(args[0], args.Skip(1).ToArray()).Select(QuoteForProcess)));
        return 0;
    }

    private static bool IsSupportedCommand(string command)
    {
        return Aliases.Values.Contains(command, StringComparer.OrdinalIgnoreCase);
    }

    private static string[] TransformArguments(string command, string[] arguments)
    {
        if (string.Equals(command, "ipconfig", StringComparison.OrdinalIgnoreCase) && arguments.Length > 0)
        {
            if (string.Equals(arguments[0], "a", StringComparison.OrdinalIgnoreCase))
                return new[] { "/all" }.Concat(arguments.Skip(1)).ToArray();

            if (string.Equals(arguments[0], "-f", StringComparison.OrdinalIgnoreCase)
                || string.Equals(arguments[0], "/f", StringComparison.OrdinalIgnoreCase))
            {
                return new[] { "/flushdns" }.Concat(arguments.Skip(1)).ToArray();
            }
        }

        if (string.Equals(command, "ping", StringComparison.OrdinalIgnoreCase)
            && arguments.Length > 0
            && string.Equals(arguments[0], "t", StringComparison.OrdinalIgnoreCase))
        {
            return new[] { "-t" }.Concat(arguments.Skip(1).Select(NormalizeArgument)).ToArray();
        }

        if (string.Equals(command, "tracert", StringComparison.OrdinalIgnoreCase)
            && arguments.Length > 0)
        {
            if (string.Equals(arguments[0], "dw", StringComparison.OrdinalIgnoreCase))
                return new[] { "-d", "-w", "1" }.Concat(arguments.Skip(1).Select(NormalizeArgument)).ToArray();

            if (string.Equals(arguments[0], "wd", StringComparison.OrdinalIgnoreCase))
                return new[] { "-w", "1", "-d" }.Concat(arguments.Skip(1).Select(NormalizeArgument)).ToArray();
        }

        if (string.Equals(command, "curl", StringComparison.OrdinalIgnoreCase) && arguments.Length > 0)
        {
            if (string.Equals(arguments[0], "c", StringComparison.OrdinalIgnoreCase))
                return new[] { "cip.cc" }.Concat(arguments.Skip(1)).ToArray();

            if (string.Equals(arguments[0], "i", StringComparison.OrdinalIgnoreCase))
                return new[] { "ipinfo.io" }.Concat(arguments.Skip(1)).ToArray();
        }

        if (string.Equals(command, "mstsc", StringComparison.OrdinalIgnoreCase)
            && arguments.Length > 0
            && IsIpv4Endpoint(arguments[0]))
        {
            string endpoint = arguments[0].IndexOf(':') < 0 ? arguments[0] + ":3389" : arguments[0];
            return new[] { "/v:" + endpoint }.Concat(arguments.Skip(1)).ToArray();
        }

        if (string.Equals(command, "route", StringComparison.OrdinalIgnoreCase))
        {
            if (arguments.Length >= 2
                && string.Equals(arguments[0], "p", StringComparison.OrdinalIgnoreCase)
                && (arguments[1] == "4" || arguments[1] == "6"))
            {
                return new[] { "print", "-" + arguments[1] }.Concat(arguments.Skip(2)).ToArray();
            }

            if (arguments.Length >= 4
                && (string.Equals(arguments[0], "a", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(arguments[0], "d", StringComparison.OrdinalIgnoreCase)))
            {
                string mask;
                if (TryGetIpv4Mask(arguments[2], out mask))
                {
                    string action = string.Equals(arguments[0], "a", StringComparison.OrdinalIgnoreCase)
                        ? "add"
                        : "delete";
                    return new[] { action, arguments[1], "mask", mask, arguments[3] }
                        .Concat(arguments.Skip(4))
                        .ToArray();
                }
            }
        }

        if (ShouldNormalizeUrls(command))
            return arguments.Select(NormalizeArgument).ToArray();

        return arguments;
    }

    private static bool IsIpv4Endpoint(string value)
    {
        string host = value;
        int portIndex = value.IndexOf(':');
        if (portIndex >= 0)
        {
            host = value.Substring(0, portIndex);
            int port;
            if (!int.TryParse(value.Substring(portIndex + 1), out port) || port < 1 || port > 65535)
                return false;
        }

        IPAddress address;
        return IPAddress.TryParse(host, out address) && address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
    }

    private static bool TryGetIpv4Mask(string prefixText, out string mask)
    {
        mask = null;
        int prefix;
        if (!int.TryParse(prefixText, out prefix) || prefix < 0 || prefix > 32)
            return false;

        uint value = prefix == 0 ? 0u : uint.MaxValue << (32 - prefix);
        mask = string.Join(".", new[]
        {
            (value >> 24) & 255,
            (value >> 16) & 255,
            (value >> 8) & 255,
            value & 255
        });
        return true;
    }

    private static bool ShouldNormalizeUrls(string command)
    {
        return string.Equals(command, "ping", StringComparison.OrdinalIgnoreCase)
            || string.Equals(command, "tracert", StringComparison.OrdinalIgnoreCase)
            || string.Equals(command, "nslookup", StringComparison.OrdinalIgnoreCase)
            || string.Equals(command, "pathping", StringComparison.OrdinalIgnoreCase);
    }

    private static string FindCommandPath(string command)
    {
        string filename = command + ".exe";
        string systemPath = Path.Combine(Environment.SystemDirectory, filename);
        if (File.Exists(systemPath))
            return systemPath;

        if (string.Equals(command, "ssh", StringComparison.OrdinalIgnoreCase))
        {
            string openSshPath = Path.Combine(Environment.SystemDirectory, "OpenSSH", filename);
            if (File.Exists(openSshPath))
                return openSshPath;
        }

        return filename;
    }

    private static string RemoveMacroGroup(string current, string marker)
    {
        int markerIndex = current.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (markerIndex < 0)
            return current;

        int segmentStart = current.LastIndexOf(" & ", markerIndex, StringComparison.Ordinal);
        segmentStart = segmentStart < 0 ? 0 : segmentStart + 3;
        int segmentEnd = current.IndexOf(" & ", markerIndex);
        if (segmentEnd < 0)
            segmentEnd = current.Length;

        if (string.Equals(marker, MacroMarker, StringComparison.OrdinalIgnoreCase))
        {
            string executable = Process.GetCurrentProcess().MainModule.FileName;
            string macro = BuildMacro(executable);
            segmentEnd = markerIndex + macro.Length;
        }

        string updated = current.Remove(segmentStart, segmentEnd - segmentStart).Trim();
        return updated.EndsWith("&", StringComparison.Ordinal)
            ? updated.Substring(0, updated.Length - 1).TrimEnd()
            : updated;
    }

    private static string RemoveLegacyMacroGroup(string current)
    {
        int start = current.IndexOf(LegacyMacroMarker, StringComparison.OrdinalIgnoreCase);
        if (start < 0)
            return current;

        int lastMacro = current.IndexOf("doskey nslookup=", start, StringComparison.OrdinalIgnoreCase);
        int end = lastMacro < 0 ? -1 : current.IndexOf(" & ", lastMacro);
        if (end < 0)
            end = current.Length;

        int segmentStart = current.LastIndexOf(" & ", start, StringComparison.Ordinal);
        segmentStart = segmentStart < 0 ? 0 : segmentStart + 3;
        string updated = current.Remove(segmentStart, end - segmentStart).Trim();
        return updated.EndsWith("&", StringComparison.Ordinal)
            ? updated.Substring(0, updated.Length - 1).TrimEnd()
            : updated;
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
        Console.WriteLine("ACMD (Advanced CMD) - CMD network command shortcuts.");
        Console.WriteLine("  acmd.exe install");
        Console.WriteLine("  acmd.exe uninstall");
        Console.WriteLine("  acmd.exe normalize ping https://example.com/path");
    }
}
