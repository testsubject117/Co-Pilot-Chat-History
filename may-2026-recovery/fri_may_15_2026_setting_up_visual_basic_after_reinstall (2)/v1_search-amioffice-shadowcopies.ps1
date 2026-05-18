'S:\ShadowCopy1','S:\ShadowCopy2','S:\ShadowCopy4' | ForEach-Object {
    Write-Host "Searching $_"
    Get-ChildItem -Path $_ -Directory -Recurse -Force -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -match 'AMIOffice|VisStudTests' } |
        Select-Object FullName
}