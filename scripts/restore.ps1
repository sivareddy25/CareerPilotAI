$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
dotnet restore "$root/src/backend/CareerPilotAI.sln"
npm ci --prefix "$root/src/frontend"
