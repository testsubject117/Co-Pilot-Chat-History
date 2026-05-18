Get-ChildItem -Path 'S:\Users\CapnKirk\source' -Directory -Recurse -Force -ErrorAction SilentlyContinue |
    Select-Object FullName