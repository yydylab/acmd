$ErrorActionPreference = 'Stop'

& .\build.ps1

$cases = @(
    @{ Input = @('ping', 'https://xxx.com/login'); Expected = 'ping xxx.com' },
    @{ Input = @('tracert', '-d', '-w', '1', 'https://123.com/admin'); Expected = 'tracert -d -w 1 123.com' },
    @{ Input = @('nslookup', 'https://abc.cn/dhihsihdi2992'); Expected = 'nslookup abc.cn' },
    @{ Input = @('ping', 'https://[2001:db8::1]/health'); Expected = 'ping [2001:db8::1]' },
    @{ Input = @('ping', 'example.com'); Expected = 'ping example.com' },
    @{ Input = @('pathping', 'https://example.com/trace'); Expected = 'pathping example.com' },
    @{ Input = @('ping', 't', 'https://example.com/status'); Expected = 'ping -t example.com' },
    @{ Input = @('tracert', 'dw', 'https://example.com/admin'); Expected = 'tracert -d -w 1 example.com' },
    @{ Input = @('tracert', 'wd', 'https://example.com/admin'); Expected = 'tracert -w 1 -d example.com' },
    @{ Input = @('ipconfig', 'a'); Expected = 'ipconfig /all' },
    @{ Input = @('ipconfig', 'f'); Expected = 'ipconfig /flushdns' },
    @{ Input = @('route', 'p'); Expected = 'route print' },
    @{ Input = @('route', 'p', '4'); Expected = 'route print -4' },
    @{ Input = @('route', 'p', '6'); Expected = 'route print -6' },
    @{ Input = @('route', 'a', '223.5.5.5', '32', '192.168.1.1'); Expected = 'route add 223.5.5.5 mask 255.255.255.255 192.168.1.1' },
    @{ Input = @('route', 'd', '10.0.0.0', '24', '192.168.1.1'); Expected = 'route delete 10.0.0.0 mask 255.255.255.0 192.168.1.1' },
    @{ Input = @('curl', 'c'); Expected = 'curl cip.cc' },
    @{ Input = @('curl', 'i'); Expected = 'curl ipinfo.io' },
    @{ Input = @('curl-cip'); Expected = 'curl cip.cc' },
    @{ Input = @('curl-ipinfo'); Expected = 'curl ipinfo.io' },
    @{ Input = @('curl', 'https://example.com/path'); Expected = 'curl https://example.com/path' },
    @{ Input = @('mstsc', '192.168.1.1'); Expected = 'mstsc /v:192.168.1.1:3389' },
    @{ Input = @('mstsc', '192.168.1.1:53389'); Expected = 'mstsc /v:192.168.1.1:53389' }
)

foreach ($case in $cases) {
    $actual = (& .\acmd.exe normalize @($case.Input)).Trim()
    if ($actual -ne $case.Expected) {
        throw "Expected '$($case.Expected)', got '$actual'."
    }
    Write-Host "PASS $actual"
}

$banner = (& .\acmd.exe -v) -join "`n"
foreach ($expected in @('acmd v0.1.5.0', 'Copyright (c) 2026 yydylab', 'https://github.com/yydylab/acmd')) {
    if (-not $banner.Contains($expected)) {
        throw "Version banner does not contain '$expected'."
    }
}
Write-Host "PASS version banner"

# Install/Uninstall must add and remove every current macro without changing
# the user's existing CMD AutoRun entry (for example, Clink).
$commandProcessorKey = 'HKCU:\Software\Microsoft\Command Processor'
$beforeAutoRun = [string](Get-ItemProperty -Path $commandProcessorKey -ErrorAction SilentlyContinue).AutoRun
& .\acmd.exe install
$installedAutoRun = [string](Get-ItemProperty -Path $commandProcessorKey).AutoRun
foreach ($macro in @('doskey cc=', 'doskey ci=')) {
    if ($installedAutoRun -notmatch [regex]::Escape($macro)) {
        throw "Install did not register $macro."
    }
}
& .\acmd.exe uninstall
$afterAutoRun = [string](Get-ItemProperty -Path $commandProcessorKey -ErrorAction SilentlyContinue).AutoRun
if ($afterAutoRun -cne $beforeAutoRun) {
    throw 'Uninstall did not restore the prior AutoRun setting.'
}
Write-Host "PASS cc/ci macro install and uninstall"
