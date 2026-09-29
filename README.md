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

## Version And Startup Banner

Run the following command to show the installed ACMD version and project URL:

```cmd
acmd -v
```

After `acmd install`, every newly opened CMD window also displays this
information before the prompt.

## Update

Run this command to compare the installed version with the latest GitHub
Release:

```cmd
acmd update
```

When a newer `acmd.exe` is available, ACMD downloads and validates it, then
requests one UAC approval to replace the executable. After the replacement, it
opens a new CMD window automatically. The updater needs administrator approval
when ACMD is installed in `C:\Windows\System32`.

## Complete Shortcut Reference

Open a **new** CMD window after installation, then use the following shortcuts.
For `ping`, `tracert`, `nslookup`, and `pathping`, an HTTP/HTTPS URL is reduced
to its host name automatically.

| Shortcut | Actual command | Example | Extensions |
| --- | --- | --- | --- |
| `p` | `ping` | `p www.baidu.com` | `p t baidu.com` -> `ping -t baidu.com`<br>`p https://github.com/user/repo` -> `ping github.com` |
| `t` | `tracert` | `t www.baidu.com` | `t dw baidu.com` -> `tracert -d -w 1 baidu.com`<br>`t wd baidu.com` -> `tracert -w 1 -d baidu.com` |
| `n` | `nslookup` | `n www.baidu.com` | `n https://example.com/path` -> `nslookup example.com` |
| `a` | `arp` | `a -a` | Pass any native `arp` option directly. |
| `s` | `ssh` | `s user@192.168.1.1` | Pass SSH options directly, for example `s -p 2222 user@host`. |
| `c` | `curl` | `c https://example.com` | `c c` -> `curl cip.cc`<br>`c i` -> `curl ipinfo.io` |
| `f` | `ftp` | `f ftp.example.com` | Pass any native `ftp` option directly. |
| `m` | `mstsc` | `m 192.168.1.1` | `m 192.168.1.1` -> `mstsc /v:192.168.1.1:3389`<br>`m 192.168.1.1:53389` -> `mstsc /v:192.168.1.1:53389` |
| `pa` | `pathping` | `pa www.baidu.com` | `pa https://example.com/path` -> `pathping example.com` |
| `te` | `telnet` | `te 192.168.1.1 23` | Requires the Windows Telnet Client optional feature. |
| `i` | `ipconfig` | `i` | `i a` -> `ipconfig /all`<br>`i f` -> `ipconfig /flushdns` |
| `g` | `getmac` | `g /v` | Pass any native `getmac` option directly. |
| `ne` | `netsh` | `ne interface ip show config` | Pass any native `netsh` context and command directly. |
| `r` | `route` | `r p` -> `route print` | `r p 4` -> `route print -4`<br>`r p 6` -> `route print -6`<br>`r a 223.5.5.5 32 192.168.1.1` -> `route add 223.5.5.5 mask 255.255.255.255 192.168.1.1`<br>`r d 223.5.5.5 32 192.168.1.1` -> `route delete 223.5.5.5 mask 255.255.255.255 192.168.1.1` |
| `nb` | `nbtstat` | `nb -n` | Pass any native `nbtstat` option directly. |

Route add/delete uses the format `r <a|d> <destination> <CIDR prefix>
<gateway>`. ACMD converts a valid IPv4 prefix from `0` through `32` into the
corresponding `route` subnet mask.

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
installation steps above. Version `0.1.3` adds the version command and CMD
startup banner.

## Scope and behavior

- The listed shortcuts expand only in newly opened CMD windows after installation.
- Only `ping`, `tracert`, `nslookup`, and `pathping` convert `http://` and
  `https://` arguments; `c` keeps complete URLs for `curl`.
- The host is extracted using the Windows .NET URI parser. Paths, queries,
  fragments, credentials, and ports are not passed to the networking command.
- The program calls the native executable from `System32` directly; it does not
  invoke `cmd /c`, so URL text is never interpreted as shell syntax.
