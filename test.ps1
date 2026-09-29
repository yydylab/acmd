$ErrorActionPreference = 'Stop'

& .\build.ps1

$cases = @(
    @{ Input = @('ping', 'https://xxx.com/login'); Expected = 'ping xxx.com' },
    @{ Input = @('tracert', '-d', '-w', '1', 'https://123.com/admin'); Expected = 'tracert -d -w 1 123.com' },
    @{ Input = @('nslookup', 'https://abc.cn/dhihsihdi2992'); Expected = 'nslookup abc.cn' },
    @{ Input = @('ping', 'https://[2001:db8::1]/health'); Expected = 'ping [2001:db8::1]' },
    @{ Input = @('ping', 'example.com'); Expected = 'ping example.com' },
    @{ Input = @('pathping', 'https://example.com/trace'); Expected = 'pathping example.com' },
    @{ Input = @('ipconfig', '-f'); Expected = 'ipconfig /flushdns' },
    @{ Input = @('ipconfig', '/f'); Expected = 'ipconfig /flushdns' },
    @{ Input = @('route', 'p', '4'); Expected = 'route print -4' },
    @{ Input = @('route', 'p', '6'); Expected = 'route print -6' },
    @{ Input = @('curl', 'https://example.com/path'); Expected = 'curl https://example.com/path' }
)

foreach ($case in $cases) {
    $actual = (& .\acmd.exe normalize @($case.Input)).Trim()
    if ($actual -ne $case.Expected) {
        throw "Expected '$($case.Expected)', got '$actual'."
    }
    Write-Host "PASS $actual"
}
