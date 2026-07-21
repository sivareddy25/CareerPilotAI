$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
dotnet clean "$root/src/backend/CareerPilotAI.sln"
Remove-Item "$root/src/frontend/.angular/cache", "$root/src/frontend/dist" -Recurse -Force -ErrorAction SilentlyContinue
