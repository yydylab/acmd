# ACMD（Advanced CMD）使用操作手册

## 1. 功能说明

ACMD 为 Windows CMD 提供常用命令简写，并让 `ping`、`tracert`、`nslookup`、
`pathping` 可直接接受 HTTP 或 HTTPS 网址。按下 Enter 后，程序仅提取网址中的
主机名，再调用 Windows 自带命令。

```cmd
ping https://github.com/chrisant996/clink
tracert https://123.com/admin
nslookup https://abc.cn/dhihsihdi2992
```

对应实际执行的命令是：

```cmd
ping github.com
tracert 123.com
nslookup abc.cn
```

## 2. 安装

1. 从项目的 Releases 页面下载 `acmd.exe`。
2. 以管理员身份打开 CMD 或 PowerShell。
3. 将文件复制到系统目录：

   ```cmd
   copy acmd.exe C:\Windows\System32\acmd.exe
   ```

   在 PowerShell 中可使用：

   ```powershell
   Copy-Item .\acmd.exe C:\Windows\System32\acmd.exe
   ```

4. 在任意 CMD 中执行：

   ```cmd
   acmd.exe install
   ```

5. 关闭当前 CMD，并重新打开一个 CMD 窗口。

安装仅写入当前用户的
`HKCU\Software\Microsoft\Command Processor\AutoRun`。

## 3. 命令简写

| 简写 | 实际命令 | 简写 | 实际命令 |
| --- | --- | --- | --- |
| `p` | `ping` | `t` | `tracert` |
| `n` | `nslookup` | `a` | `arp` |
| `s` | `ssh` | `c` | `curl` |
| `f` | `ftp` | `m` | `mstsc` |
| `pa` | `pathping` | `te` | `telnet` |
| `i` | `ipconfig` | `g` | `getmac` |
| `ne` | `netsh` | `r` | `route` |
| `nb` | `nbtstat` |  |  |

特殊简写：

```cmd
i -f
i /f
r p 4
```

分别执行 `ipconfig /flushdns`、`ipconfig /flushdns` 和 `route print -4`。
`r p 6` 对应执行 `route print -6`。

## 4. 日常使用

直接输入网址，不要使用 Markdown 反引号：

```cmd
ping https://example.com/login
```

请使用上面的形式，而非：

```cmd
ping `https://example.com/login`
```

CMD 不将反引号视为引号字符，后者会导致 DNS 查询失败。

带查询参数的网址包含 `&` 时请用英文双引号包住完整网址：

```cmd
ping "https://example.com/path?a=1&b=2"
```

## 5. 卸载

执行：

```cmd
acmd.exe uninstall
```

然后重新打开 CMD。该操作只移除 ACMD 添加的宏。

## 6. 构建与测试

在 Windows PowerShell 中运行：

```powershell
.\build.ps1
.\test.ps1
```

项目使用 Windows 自带的 .NET Framework C# 编译器，不依赖第三方包。

## 7. 注意事项

- 仅转换以 `http://` 或 `https://` 开头的参数。
- `ping`、`tracert`、`nslookup`、`pathping` 会提取 HTTP/HTTPS URL 的主机名；
  `c`（`curl`）保留完整 URL。
- URL 的路径、查询参数、片段、账号信息和端口不会传给网络命令。
- `acmd.exe` 会直接启动 `System32` 中的原生命令，不使用 `cmd /c`。
