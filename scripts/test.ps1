$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
dotnet test "$root/src/backend/CareerPilotAI.sln" --no-restore
npm test --prefix "$root/src/frontend" -- --run
