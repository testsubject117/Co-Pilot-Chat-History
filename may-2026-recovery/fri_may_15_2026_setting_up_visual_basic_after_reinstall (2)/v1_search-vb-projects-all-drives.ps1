Get-PSDrive -PSProvider FileSystem | ForEach-Object {
    $root = $_.Root
    Write-Host "===== Searching $root ====="
    Get-ChildItem -Path $root -Recurse -Force -ErrorAction SilentlyContinue -File |
        Where-Object {
            $_.Extension -in '.sln','.vbproj','.vb' -or
            $_.Name -match 'amioffice|VisStudTests'
        } |
        Select-Object FullName
}