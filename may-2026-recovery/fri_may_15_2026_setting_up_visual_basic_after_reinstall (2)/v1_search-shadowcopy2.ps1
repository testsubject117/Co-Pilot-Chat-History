Get-ChildItem -Path "S:\ShadowCopy2" -Directory -Recurse -Force -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -eq 'VisStudTests' } |
    Select-Object FullName