Get-PSDrive -PSProvider FileSystem | ForEach-Object {
    $root = $_.Root
    Write-Host "===== Searching $root ====="
    Get-ChildItem -Path $root -Directory -Recurse -Force -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -match 'VisStudTests|amioffice|repos|CapnKirk' } |
        Select-Object FullName
}