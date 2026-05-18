$vhdfolder = 'C:\ProgramData\Microsoft\Windows\Virtual Hard Disks'

Get-ChildItem -Path $vhdfolder -File | Where-Object { $_.Extension -in '.vhd','.vhdx','.avhdx' } | ForEach-Object {
    $path = $_.FullName
    $bytes = New-Object byte[] 16
    $fs = $null
    try {
        $fs = [System.IO.File]::Open($path, 'Open', 'Read', 'ReadWrite')
        $read = $fs.Read($bytes, 0, $bytes.Length)
        $hex = ($bytes[0..($read-1)] | ForEach-Object { $_.ToString('X2') }) -join ' '
        $ascii = -join ($bytes[0..($read-1)] | ForEach-Object {
            if ($_ -ge 32 -and $_ -le 126) { [char]$_ } else { '.' }
        })

        [PSCustomObject]@{
            Name   = $_.Name
            Length = $_.Length
            Hex16  = $hex
            Ascii  = $ascii
        }
    }
    catch {
        [PSCustomObject]@{
            Name   = $_.Name
            Length = $_.Length
            Hex16  = 'ERROR'
            Ascii  = $_.Exception.Message
        }
    }
    finally {
        if ($fs) { $fs.Dispose() }
    }
} | Format-Table -AutoSize