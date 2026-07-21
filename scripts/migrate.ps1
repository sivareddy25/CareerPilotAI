$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
if (-not (Test-Path "$root/.env")) { Copy-Item "$root/.env.example" "$root/.env" }
Get-Content "$root/.env" | Where-Object { $_ -match '^[A-Za-z_][A-Za-z0-9_]*=' } | ForEach-Object { $parts = $_ -split '=', 2; [Environment]::SetEnvironmentVariable($parts[0], $parts[1], 'Process') }
dotnet tool restore
$env:Database__ConnectionString = "Host=localhost;Port=5432;Database=careerpilot;Username=careerpilot;Password=$env:POSTGRES_PASSWORD"
dotnet ef database update --project "$root/src/backend/CareerPilot.Infrastructure" --startup-project "$root/src/backend/CareerPilot.Api"
