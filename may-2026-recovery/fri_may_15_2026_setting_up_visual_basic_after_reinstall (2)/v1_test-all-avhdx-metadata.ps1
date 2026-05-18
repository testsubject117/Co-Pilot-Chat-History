$vhdfolder = 'C:\ProgramData\Microsoft\Windows\Virtual Hard Disks'
Get-ChildItem -Path $vhdfolder -Filter *.avhdx | ForEach-Object {
    Write-Host "===== $($_.Name) ====="
    try {
        Get-VHD -Path $_.FullName | Format-List Path,VhdFormat,VhdType,FileSize,Size,ParentPath
    }
    catch {
        Write-Host "ERROR: $($_.Exception.Message)"
    }
    Write-Host ""
}