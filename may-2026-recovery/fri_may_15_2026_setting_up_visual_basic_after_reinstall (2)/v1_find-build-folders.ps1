Get-ChildItem -Path 'S:\Users\CapnKirk\source\repos' -Directory -Recurse -Force -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -match '22|23|24|25|26|27|28|29|30|31' } |
    Select-Object FullName