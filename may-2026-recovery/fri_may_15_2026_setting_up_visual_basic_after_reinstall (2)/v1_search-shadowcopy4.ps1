Get-ChildItem -Path "S:\ShadowCopy4" -Directory -Recurse -Force -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -eq 'VisStudTests' } |
    Select-Object FullName