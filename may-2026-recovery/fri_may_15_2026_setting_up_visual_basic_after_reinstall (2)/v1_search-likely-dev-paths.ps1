$paths = @(
    'S:\Users',
    'S:\Temp',
    'S:\MainMenu'
)

foreach ($p in $paths) {
    if (Test-Path $p) {
        Write-Host "Searching $p"
        Get-ChildItem -Path $p -Directory -Recurse -Force -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -match 'VisStudTests|AMIOffice|Repos|Source|Projects' } |
            Select-Object FullName
    }
}