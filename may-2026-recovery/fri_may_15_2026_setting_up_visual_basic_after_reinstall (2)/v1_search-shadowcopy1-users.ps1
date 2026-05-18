Get-ChildItem 'S:\ShadowCopy1\Users' -Directory -Recurse -Force -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -match 'CapnKirk|amioffice|VisStudTests|repos' } |
    Select-Object FullName