Get-ChildItem -Path "R:\" -Directory -Recurse -Force -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -eq 'VisStudTests' } |
    Select-Object FullName