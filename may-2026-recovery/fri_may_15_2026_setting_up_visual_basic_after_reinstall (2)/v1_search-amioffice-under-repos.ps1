Get-ChildItem -Path 'S:\Users\CapnKirk\source\repos' -Directory -Recurse -Force -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -match 'amioffice|VisStudTests' } |
    Select-Object FullName