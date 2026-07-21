$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
dotnet format "$root/src/backend/CareerPilotAI.sln" --no-restore
npm exec --prefix "$root/src/frontend" prettier -- --write src
