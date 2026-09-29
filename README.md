# ACMD (Advanced CMD)

[中文操作手册](MANUAL.zh-CN.md) | [Release 下载](../../releases)

`acmd.exe` adds practical Windows CMD shortcuts and lets `ping`, `tracert`,
`nslookup`, and `pathping` accept an HTTP or HTTPS URL. Before those network
commands run, it replaces the URL with its host name.

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
framework-dependent `acmd.exe`. No third-party dependency is used.

## Shortcuts

| Shortcut | Command |
| --- | --- |
| `p` | `ping` |
| `t` | `tracert` |
| `n` | `nslookup` |
| `a` | `arp` |
| `s` | `ssh` |
| `c` | `curl` |
| `f` | `ftp` |
| `m` | `mstsc` |
| `pa` | `pathping` |
| `te` | `telnet` |
| `i` | `ipconfig` |
| `g` | `getmac` |
| `ne` | `netsh` |
| `r` | `route` |
| `nb` | `nbtstat` |

Additional forms: `i -f` and `i /f` run `ipconfig /flushdns`; `r p 4` runs
`route print -4` (and `r p 6` runs `route print -6`).

## Install

1. Copy `acmd.exe` to `C:\Windows\System32\acmd.exe`.
2. Open a CMD window and run `acmd.exe install`.
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
acmd.exe uninstall
```

## Release

Download `acmd.exe` from the [Releases](../../releases) page, then follow the
installation steps above. Version `0.1.1` is the first ACMD release.

## Scope and behavior

- The listed shortcuts expand only in newly opened CMD windows after installation.
- Only `ping`, `tracert`, `nslookup`, and `pathping` convert `http://` and
  `https://` arguments; `c` keeps complete URLs for `curl`.
- The host is extracted using the Windows .NET URI parser. Paths, queries,
  fragments, credentials, and ports are not passed to the networking command.
- The program calls the native executable from `System32` directly; it does not
  invoke `cmd /c`, so URL text is never interpreted as shell syntax.
