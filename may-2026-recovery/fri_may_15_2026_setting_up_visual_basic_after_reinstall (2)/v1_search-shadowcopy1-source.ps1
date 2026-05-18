Get-ChildItem 'S:\ShadowCopy1\Users\CapnKirk\source' -Directory -Recurse -Force -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -match 'VisStudTests|amioffice|repos|22|23|24|25|26|27|28|29|30|31' } |
    Select-Object FullName