# ACMD (Advanced CMD) User Guide

[中文操作手册](MANUAL.zh-CN.md) | [Download Releases](../../releases)

## 1. Features

ACMD provides shortcuts for common Windows CMD commands. It lets `ping`,
`tracert`, `nslookup`, and `pathping` accept HTTP or HTTPS URLs directly.
After you press Enter, ACMD extracts only the host name from the URL and then
runs the Windows built-in command.

Standard CMD commands support domain names only; they do not accept an HTTP URL:

```cmd
ping xxx.com
tracert 123.com
nslookup abc.cn
```

After installing ACMD, the following commands run with URL host extraction.
You can paste URLs directly into the CMD command line:

```cmd
p https://xxx.com/aaa/ccc/jsidaoijd
t https://123.com/login
n https://abc.cn/1.html
```

## 2. Installation

1. Download `acmd.exe` from the project's Releases page.
2. Copy the file to `C:\Windows\System32`.
3. Run the following command in any CMD window:

   ```cmd
   acmd.exe install
   ```

4. When installation is complete, open a new CMD window:

   ```cmd
   start
   ```

## 3. Everyday Use

Enter URLs directly. Do not use Markdown backticks or double quotes:

```cmd
p https://baidu.com
```

<img width="506" height="308" alt="ACMD ping URL example" src="https://github.com/user-attachments/assets/1c5052db-455c-4de6-a754-2fcc9a63d82c" />

```cmd
t wd https://www.baidu.com
```

<img width="483" height="422" alt="ACMD tracert URL example" src="https://github.com/user-attachments/assets/41a96cbd-6e24-4221-8e9f-99d5a63dfdae" />

```cmd
n https://www.baidu.com
```

<img width="445" height="188" alt="ACMD nslookup URL example" src="https://github.com/user-attachments/assets/ccba66c0-fab8-4aff-a7f4-d7b76e08adcf" />

## 4. Command Shortcut Summary

| Shortcut | Actual command | Example | Extension |
| --- | --- | --- | --- |
| `p` | `ping` | `p www.baidu.com` | `p t baidu.com` -> `ping -t baidu.com`<br>`p https://github.com/user/repo` -> `ping github.com` |
| `t` | `tracert` | `t www.baidu.com` | `t dw baidu.com` -> `tracert -d -w 1 baidu.com`<br>`t wd baidu.com` -> `tracert -w 1 -d baidu.com` |
| `n` | `nslookup` | `n www.baidu.com` | `n https://example.com/path` -> `nslookup example.com` |
| `a` | `arp` | `a -a` | Pass native `arp` options directly. |
| `s` | `ssh` | `s user@192.168.1.1` | For example: `s -p 2222 user@host`. |
| `c` | `curl` | `c https://example.com` | `c c` -> `curl cip.cc`<br>`c i` -> `curl ipinfo.io` |
| `cc` | `curl cip.cc` | `cc` | Quickly query IP and location information from `cip.cc`. |
| `ci` | `curl ipinfo.io` | `ci` | Quickly query IP metadata from `ipinfo.io`. |
| `f` | `ftp` | `f ftp.example.com` | Pass native `ftp` options directly. |
| `m` | `mstsc` | `m 192.168.1.1` | `m 192.168.1.1` -> `mstsc /v:192.168.1.1:3389`<br>`m 192.168.1.1:53389` -> `mstsc /v:192.168.1.1:53389` |
| `pa` | `pathping` | `pa www.baidu.com` | `pa https://example.com/path` -> `pathping example.com` |
| `tp` | `tcping` | `tp 192.168.1.200 3389` | `tp 192.168.1.200` -> `tcping 192.168.1.200 22`.<br>Requires [tcping.exe](https://github.com/pouriyajamshidi/tcping) to be installed and available in `PATH` or `System32`. |
| `te` | `telnet` | `te 192.168.1.1 23` | Enable the Windows Telnet Client optional feature first. |
| `i` | `ipconfig` | `i` | `i a` -> `ipconfig /all`<br>`i f` -> `ipconfig /flushdns` |
| `ia` | `ipconfig /all` | `ia` | Quickly show complete configuration details for all network adapters. |
| `if` | `ipconfig /flushdns` | `if` | Quickly clear the local DNS resolver cache. |
| `g` | `getmac` | `g /v` | Pass native `getmac` options directly. |
| `ne` | `netsh` | `ne interface ip show config` | Pass native `netsh` contexts and options directly. |
| `r` | `route` | `r p` -> `route print` | `r p 4` -> `route print -4`<br>`r p 6` -> `route print -6`<br>`r a 223.5.5.5 32 192.168.1.1` -> `route add 223.5.5.5 mask 255.255.255.255 192.168.1.1`<br>`r d 223.5.5.5 32 192.168.1.1` -> `route delete 223.5.5.5 mask 255.255.255.255 192.168.1.1` |
| `rp` | `route print` | `rp` | `rp4` -> `route print -4`<br>`rp6` -> `route print -6` |
| `nb` | `nbtstat` | `nb -n` | Pass native `nbtstat` options directly. |

The route add/delete syntax is
`r <a|d> <destination> <CIDR prefix> <gateway>`. ACMD converts a valid IPv4
prefix from `0` through `32` to the subnet mask required by `route`.

## 5. Version And Startup Information

Run this command in CMD to display the ACMD version, copyright, and project
URL:

```cmd
acmd -v
```

After running `acmd install`, this information is displayed before the prompt
in every newly opened CMD window.

## 6. Updates

Run this command to compare the local version with the latest GitHub Release:

```cmd
acmd update
```

When a newer version is available, ACMD downloads and validates the latest
`acmd.exe`, then requests one UAC approval to replace the program. Once the
replacement finishes, it opens a new CMD window automatically. Updating an
ACMD installation in `C:\Windows\System32` requires that UAC approval.

## 7. Uninstall ACMD

Run this command in any CMD window:

```cmd
acmd.exe uninstall
```

Then open a new CMD window. This removes only macros added by ACMD.

## 8. Build And Test (Developers)

Run the following in Windows PowerShell:

```powershell
.\build.ps1
.\test.ps1
```

The project uses the Windows built-in .NET Framework C# compiler and has no
third-party dependencies.

## 9. Notes

- Only arguments that begin with `http://` or `https://` are converted.
- `ping`, `tracert`, `nslookup`, and `pathping` extract the host name from an
  HTTP/HTTPS URL. `c` (`curl`) preserves the full URL.
- URL paths, query parameters, fragments, credentials, and ports are not
  passed to network commands.
- `acmd.exe` starts native commands from `System32` directly and does not use
  `cmd /c`.
