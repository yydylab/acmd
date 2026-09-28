# UrlCmd

[中文操作手册](MANUAL.zh-CN.md) | [Release 下载](../../releases)

`UrlCmd.exe` lets `ping`, `tracert`, and `nslookup` accept an HTTP or HTTPS URL in
CMD. Before the Windows networking command runs, it replaces the URL with its
host name and keeps the rest of the arguments intact.

Examples:

```text
ping https://xxx.com/login
tracert https://123.com/admin
nslookup https://abc.cn/dhihsihdi2992
```

They execute as:

```text
ping xxx.com
tracert 123.com
nslookup abc.cn
```

## Build

Run the following from PowerShell in this directory:

```powershell
.\build.ps1
.\test.ps1
```

The build uses the built-in .NET Framework 4 C# compiler and produces a single
framework-dependent `UrlCmd.exe`. No third-party dependency is used.

## Install

1. Copy `UrlCmd.exe` to `C:\Windows\System32\UrlCmd.exe`.
2. Open a CMD window and run `UrlCmd.exe install`.
3. Open a **new** CMD window.

The installer preserves the current user's existing `CMD` AutoRun setting
 and appends three `doskey` macros. It changes only
`HKCU\Software\Microsoft\Command Processor\AutoRun`, so it does not require
administrator rights and does not affect other users.

Windows does not automatically run arbitrary EXE files merely because they are
placed in `System32`; the one-time `install` command is required to enable the
current user's CMD integration.

To remove the integration:

```text
UrlCmd.exe uninstall
```

## Release

Download `UrlCmd.exe` from the [Releases](../../releases) page, then follow the
installation steps above. Version `0.1.0` is the first public release.

## Scope and behavior

- Supported commands: `ping`, `tracert`, `nslookup`.
- Only `http://` and `https://` arguments are converted.
- The host is extracted using the Windows .NET URI parser. Paths, queries,
  fragments, credentials, and ports are not passed to the networking command.
- The program calls the native executable from `System32` directly; it does not
  invoke `cmd /c`, so URL text is never interpreted as shell syntax.
