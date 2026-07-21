$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
if (-not (Test-Path "$root/.env")) { Copy-Item "$root/.env.example" "$root/.env" }
& docker compose --env-file "$root/.env" -f "$root/docker-compose.yml" rm -s -f postgres
& docker volume rm careerpilot_postgres-data 2>$null
& docker compose --env-file "$root/.env" -f "$root/docker-compose.yml" up -d postgres redis
Write-Host 'Database volume recreated. Start the application to apply migrations and seed local data.'
