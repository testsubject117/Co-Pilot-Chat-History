$paths = @(
    'S:\',
    'S:\ShadowCopy1',
    'S:\ShadowCopy2',
    'S:\ShadowCopy4'
)

foreach ($p in $paths) {
    if (Test-Path $p) {
        Write-Host "===== Searching $p ====="
        Get-ChildItem -Path $p -Directory -Recurse -Force -ErrorAction SilentlyContinue |
            Where-Object {
                $_.Name -match 'VisStudTests|AMIOffice|Repos|Projects|Source'
            } |
            Select-Object FullName
    }
}