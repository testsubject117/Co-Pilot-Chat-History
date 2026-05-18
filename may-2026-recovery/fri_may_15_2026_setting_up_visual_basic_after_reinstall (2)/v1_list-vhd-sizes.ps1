$vhdfolder = 'C:\ProgramData\Microsoft\Windows\Virtual Hard Disks'
Get-ChildItem -Path $vhdfolder -File | Where-Object { $_.Extension -in '.vhd','.vhdx','.avhdx' } |
Select-Object Name, Length, @{Name='SizeGB';Expression={[math]::Round($_.Length / 1GB, 3)}} |
Sort-Object Length -Descending | Format-Table -AutoSize