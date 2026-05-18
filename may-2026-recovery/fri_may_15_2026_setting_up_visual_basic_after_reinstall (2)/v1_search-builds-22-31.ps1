$paths = @(
    'S:\',
    'S:\ShadowCopy1',
    'S:\ShadowCopy2',
    'S:\ShadowCopy4'
)

$wanted = '22','23','24','25','26','27','28','29','30','31'

foreach ($p in $paths) {
    if (Test-Path $p) {
        Write-Host "===== Searching $p ====="
        Get-ChildItem -Path $p -Directory -Recurse -Force -ErrorAction SilentlyContinue |
            Where-Object {
                $_.Name -in $wanted -or $_.Name -match 'Build(22|23|24|25|26|27|28|29|30|31)'
            } |
            Select-Object FullName
    }
}