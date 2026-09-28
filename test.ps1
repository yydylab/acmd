$ErrorActionPreference = 'Stop'

& .\build.ps1

$cases = @(
    @{ Input = @('ping', 'https://xxx.com/login'); Expected = 'ping xxx.com' },
    @{ Input = @('tracert', '-d', '-w', '1', 'https://123.com/admin'); Expected = 'tracert -d -w 1 123.com' },
    @{ Input = @('nslookup', 'https://abc.cn/dhihsihdi2992'); Expected = 'nslookup abc.cn' },
    @{ Input = @('ping', 'https://[2001:db8::1]/health'); Expected = 'ping [2001:db8::1]' },
    @{ Input = @('ping', 'example.com'); Expected = 'ping example.com' }
)

foreach ($case in $cases) {
    $actual = (& .\UrlCmd.exe normalize @($case.Input)).Trim()
    if ($actual -ne $case.Expected) {
        throw "Expected '$($case.Expected)', got '$actual'."
    }
    Write-Host "PASS $actual"
}
