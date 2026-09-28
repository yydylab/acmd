# UrlCmd 使用操作手册

## 1. 功能说明

UrlCmd 让 Windows CMD 中的 `ping`、`tracert`、`nslookup` 可直接接受 HTTP 或
HTTPS 网址。按下 Enter 后，程序仅提取网址中的主机名，并调用 Windows 自带的
网络命令。

```cmd
ping https://github.com/chrisant996/clink
tracert -d -w 1 https://123.com/admin
nslookup https://abc.cn/dhihsihdi2992
```

对应实际执行的命令是：

```cmd
ping github.com
tracert -d -w 1 123.com
nslookup abc.cn
```

## 2. 安装

1. 从项目的 Releases 页面下载 `UrlCmd.exe`。
2. 以管理员身份打开 CMD 或 PowerShell。
3. 将文件复制到系统目录：

   ```cmd
   copy UrlCmd.exe C:\Windows\System32\UrlCmd.exe
   ```

   在 PowerShell 中可使用：

   ```powershell
   Copy-Item .\UrlCmd.exe C:\Windows\System32\UrlCmd.exe
   ```

4. 在任意 CMD 中执行：

   ```cmd
   UrlCmd.exe install
   ```

5. 关闭当前 CMD，并重新打开一个 CMD 窗口。

安装仅写入当前用户的
`HKCU\Software\Microsoft\Command Processor\AutoRun`。已有的 Clink AutoRun
配置会被保留。

## 3. 日常使用

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

## 4. 卸载

执行：

```cmd
UrlCmd.exe uninstall
```

然后重新打开 CMD。该操作只移除 UrlCmd 添加的宏，并保留原有的 Clink 或其他
AutoRun 设置。

## 5. 构建与测试

在 Windows PowerShell 中运行：

```powershell
.\build.ps1
.\test.ps1
```

项目使用 Windows 自带的 .NET Framework C# 编译器，不依赖第三方包。

## 6. 注意事项

- 仅转换以 `http://` 或 `https://` 开头的参数。
- 仅支持 `ping`、`tracert`、`nslookup`。
- URL 的路径、查询参数、片段、账号信息和端口不会传给网络命令。
- `UrlCmd.exe` 会直接启动 `System32` 中的原生命令，不使用 `cmd /c`。
