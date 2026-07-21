$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
& "$PSScriptRoot/restore.ps1"
dotnet build "$root/src/backend/CareerPilotAI.sln" --no-restore -warnaserror
npm run build --prefix "$root/src/frontend"
