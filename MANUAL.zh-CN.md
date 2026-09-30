# ACMD（Advanced CMD）使用操作手册

## 1. 功能说明

ACMD 为 Windows CMD 提供常用命令简写，并让 `ping`、`tracert`、`nslookup`、
`pathping` 等可直接接受 HTTP 或 HTTPS 网址。按下 Enter 后，程序仅提取网址中的
主机名，再调用 Windows 自带命令。

常规cmd命令,只支持输入域名，不能带http协议

```cmd
ping xxx.com
tracert 123.com
nslookup abc.cn
```

安装acmd之后，对应实际执行的命令如下：(支持直接将网址粘贴到cmd命令行中)

```cmd
p https://xxx.com/aaa/ccc/jsidaoijd
t https://123.com/login
n https://abc.cn/1.html
```

## 2. 安装

1. 从项目的 Releases 页面下载 `acmd.exe`。
2. 将文件复制到系统目录 `C:\Windows\System32`。
3. 在任意 CMD 中执行：

   ```cmd
   acmd.exe install
   ```

4. 待安装完成，重新打开一个 CMD 窗口。

   ```cmd
   start
   ```
   
## 3. 日常使用

直接输入网址，不要使用 Markdown 反引号，或双引号：

```cmd
p https://baidu.com
```
<img width="506" height="308" alt="image" src="https://github.com/user-attachments/assets/1c5052db-455c-4de6-a754-2fcc9a63d82c" />

```cmd
t wd https://www.baidu.com
```
<img width="483" height="422" alt="image" src="https://github.com/user-attachments/assets/41a96cbd-6e24-4221-8e9f-99d5a63dfdae" />

```cmd
n https://www.baidu.com
```
<img width="445" height="188" alt="image" src="https://github.com/user-attachments/assets/ccba66c0-fab8-4aff-a7f4-d7b76e08adcf" />

## 4. 命令简写汇总

| 简写  | 实际命令 | 案例 | 拓展 |
| --- | --- | --- | --- |
| `p` | `ping` | `p www.baidu.com` | `p t baidu.com` -> `ping -t baidu.com`<br>`p https://github.com/user/repo` -> `ping github.com` |
| `t` | `tracert` | `t www.baidu.com` | `t dw baidu.com` -> `tracert -d -w 1 baidu.com`<br>`t wd baidu.com` -> `tracert -w 1 -d baidu.com` |
| `n` | `nslookup` | `n www.baidu.com` | `n https://example.com/path` -> `nslookup example.com` |
| `a` | `arp` | `a -a` | 原生 `arp` 参数可直接传入。 |
| `s` | `ssh` | `s user@192.168.1.1` | 例如：`s -p 2222 user@host`。 |
| `c` | `curl` | `c https://example.com` | `c c` -> `curl cip.cc`<br>`c i` -> `curl ipinfo.io` |
| `cc` | `curl cip.cc` | `cc` | 快速查询 `cip.cc` 的 IP 与归属地信息。 |
| `ci` | `curl ipinfo.io` | `ci` | 快速查询 `ipinfo.io` 的 IP 元数据信息。 |
| `f` | `ftp` | `f ftp.example.com` | 原生 `ftp` 参数可直接传入。 |
| `m` | `mstsc` | `m 192.168.1.1` | `m 192.168.1.1` -> `mstsc /v:192.168.1.1:3389`<br>`m 192.168.1.1:53389` -> `mstsc /v:192.168.1.1:53389` |
| `pa` | `pathping` | `pa www.baidu.com` | `pa https://example.com/path` -> `pathping example.com` |
| `te` | `telnet` | `te 192.168.1.1 23` | 需先启用 Windows Telnet Client 可选功能。 |
| `i` | `ipconfig` | `i` | `i a` -> `ipconfig /all`<br>`i f` -> `ipconfig /flushdns` |
| `g` | `getmac` | `g /v` | 原生 `getmac` 参数可直接传入。 |
| `ne` | `netsh` | `ne interface ip show config` | 原生 `netsh` 上下文与参数可直接传入。 |
| `r` | `route` | `r p` -> `route print` | `r p 4` -> `route print -4`<br>`r p 6` -> `route print -6`<br>`r a 223.5.5.5 32 192.168.1.1` -> `route add 223.5.5.5 mask 255.255.255.255 192.168.1.1`<br>`r d 223.5.5.5 32 192.168.1.1` -> `route delete 223.5.5.5 mask 255.255.255.255 192.168.1.1` |
| `nb` | `nbtstat` | `nb -n` | 原生 `nbtstat` 参数可直接传入。 |

路由添加和删除的格式为
`r <a|d> <目标地址> <CIDR 前缀> <网关>`。ACMD 会将合法 IPv4 前缀（`0` 至
`32`）转换为 `route` 所需的子网掩码。

## 5. 版本与启动信息

在 CMD 中执行以下命令可显示 ACMD 当前版本、版权和项目地址：

```cmd
acmd -v
```

执行 `acmd install` 后，每次新开 CMD 窗口都会在提示符前自动显示这组信息。

## 6. 更新升级

执行以下命令可比对本机版本和 GitHub 最新 Release：

```cmd
acmd update
```

检测到新版本时，ACMD 会下载并校验最新 `acmd.exe`，随后请求一次 UAC 管理员授权以
替换程序。替换完成后会自动打开新的 CMD 窗口。程序安装在
`C:\Windows\System32` 时，升级必须通过该 UAC 授权。

## 7. 卸载acmd

任意cmd窗口执行：

```cmd
acmd.exe uninstall
```

然后重新打开 CMD。该操作只移除 ACMD 添加的宏。

## 8. 构建与测试（开发者）

在 Windows PowerShell 中运行：

```powershell
.\build.ps1
.\test.ps1
```

项目使用 Windows 自带的 .NET Framework C# 编译器，不依赖第三方包。

## 9. 注意事项

- 仅转换以 `http://` 或 `https://` 开头的参数。
- `ping`、`tracert`、`nslookup`、`pathping` 会提取 HTTP/HTTPS URL 的主机名；
  `c`（`curl`）保留完整 URL。
- URL 的路径、查询参数、片段、账号信息和端口不会传给网络命令。
- `acmd.exe` 会直接启动 `System32` 中的原生命令，不使用 `cmd /c`。
