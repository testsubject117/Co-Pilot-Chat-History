Get-PSDrive -PSProvider FileSystem | ForEach-Object {
    $root = $_.Root
    Write-Host "===== Searching $root ====="
    Get-ChildItem -Path $root -Directory -Recurse -Force -ErrorAction SilentlyContinue |
        Where-Object {
            $_.Name -match '^22$|^23$|^24$|^25$|^26$|^27$|^28$|^29$|^30$|^31$|Build22|Build23|Build24|Build25|Build26|Build27|Build28|Build29|Build30|Build31'
        } |
        Select-Object FullName
}