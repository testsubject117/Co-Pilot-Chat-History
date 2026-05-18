Get-PSDrive -PSProvider FileSystem | ForEach-Object {
    Get-ChildItem -Path $_.Root -Recurse -Force -ErrorAction SilentlyContinue -File |
        Where-Object { $_.Extension -in '.vhd','.vhdx','.avhdx' } |
        Select-Object -ExpandProperty DirectoryName
} | Sort-Object -Unique