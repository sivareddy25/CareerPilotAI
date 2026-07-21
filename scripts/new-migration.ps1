param([Parameter(Mandatory = $true)][string]$Name)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
dotnet tool restore
dotnet ef migrations add $Name --project "$root/src/backend/CareerPilot.Infrastructure" --startup-project "$root/src/backend/CareerPilot.Api"
