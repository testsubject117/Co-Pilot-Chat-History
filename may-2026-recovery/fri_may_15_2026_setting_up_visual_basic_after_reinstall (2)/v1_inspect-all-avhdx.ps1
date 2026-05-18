Get-ChildItem -Path $vhdfolder -Filter *.avhdx | ForEach-Object {
    Write-Host "===== $($_.Name) ====="
    try {
        Get-VHD -Path $_.FullName | Select-Object Path, VhdType, VhdFormat, FileSize, Size, ParentPath
    }
    catch {
        Write-Host "ERROR: $($_.Exception.Message)"
    }
}