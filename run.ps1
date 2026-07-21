[CmdletBinding()]
param(
    [switch]$Docker,
    [switch]$NoBrowser
)

$ErrorActionPreference = 'Stop'
$Root = $PSScriptRoot
$ApiUrl = if ($env:API_URL) { $env:API_URL } else { 'http://localhost:5080' }
$WebUrl = if ($env:WEB_URL) { $env:WEB_URL } else { 'http://localhost:4200' }
$OllamaModel = if ($env:OLLAMA_MODEL) { $env:OLLAMA_MODEL } else { 'llama3.2:3b' }
function Info([string]$Message) { Write-Host "[CareerPilot] $Message" -ForegroundColor Cyan }
function Fail([string]$Message) { throw "[CareerPilot] $Message" }
function Require([string]$Command, [string]$Help) { if (-not (Get-Command $Command -ErrorAction SilentlyContinue)) { Fail "Missing $Command. $Help" } }
function Wait-Url([string]$Url, [string]$Name) { 1..60 | ForEach-Object { try { Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 2 | Out-Null; Info "$Name is ready: $Url"; return $true } catch { Start-Sleep -Seconds 1 } }; return $false }

if (-not (Test-Path "$Root/.env")) { Copy-Item "$Root/.env.example" "$Root/.env"; Info 'Created .env from .env.example.' }
Get-Content "$Root/.env" | Where-Object { $_ -match '^[A-Za-z_][A-Za-z0-9_]*=' } | ForEach-Object { $parts = $_ -split '=', 2; [Environment]::SetEnvironmentVariable($parts[0], $parts[1], 'Process') }
Require git 'Install Git: https://git-scm.com/downloads'
Require dotnet 'Install the .NET 9 SDK: https://dotnet.microsoft.com/download/dotnet/9.0'
Require npm 'Install Node.js 20 or newer: https://nodejs.org/'
if (-not (dotnet --list-sdks | Select-String '^9\.')) { Fail 'The .NET 9 SDK is required.' }

if ($Docker) {
  Require docker 'Install Docker Desktop, then rerun .\run.ps1 -Docker.'
  & docker compose --env-file "$Root/.env" -f "$Root/docker-compose.yml" up --build -d
  if (-not (Wait-Url 'http://localhost:8080/health' 'Backend API')) { Fail 'Docker API did not start. Run docker compose logs backend-api.' }
  if (-not (Wait-Url 'http://localhost' 'Angular')) { Fail 'Docker web app did not start. Run docker compose logs frontend-web.' }
  Write-Host "`nCareerPilot AI startup dashboard`n  Database: ready`n  Backend: http://localhost:8080`n  Angular: http://localhost`n  Ollama: http://localhost:11434`n  Swagger: http://localhost:8080/swagger`n"
  if (-not $NoBrowser) { Start-Process 'http://localhost'; Start-Process 'http://localhost:8080/swagger'; Start-Process 'http://localhost:8080/health' }
  exit $LASTEXITCODE
}
Require docker 'Native mode uses Docker for PostgreSQL and Redis. Install Docker Desktop or use -Docker.'
& "$Root/scripts/restore.ps1"
& docker compose --env-file "$Root/.env" -f "$Root/docker-compose.yml" up -d postgres redis
for ($i = 1; $i -le 45; $i++) {
  & docker compose --env-file "$Root/.env" -f "$Root/docker-compose.yml" exec -T postgres pg_isready -U careerpilot -d careerpilot *> $null
  if ($LASTEXITCODE -eq 0) { break }
  if ($i -eq 45) { Fail 'PostgreSQL did not become ready. Run docker compose logs postgres.' }
  Start-Sleep -Seconds 1
}
Require ollama 'Install Ollama from https://ollama.com/download and start it.'
if (-not (Wait-Url 'http://localhost:11434/api/tags' 'Ollama')) { Fail 'Ollama is not running.' }
if (-not (ollama list | Select-String ([regex]::Escape($OllamaModel)))) { Info "Downloading $OllamaModel…"; ollama pull $OllamaModel }
Push-Location "$Root/src/frontend"; npm exec --yes playwright -- install chromium; Pop-Location
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:ASPNETCORE_URLS = $ApiUrl
$env:Database__ConnectionString = "Host=localhost;Port=5432;Database=careerpilot;Username=careerpilot;Password=$env:POSTGRES_PASSWORD"
$api = Start-Process dotnet -ArgumentList 'run', '--project', "$Root/src/backend/CareerPilot.Api/CareerPilot.Api.csproj", '--no-launch-profile' -WorkingDirectory $Root -PassThru -RedirectStandardOutput "$Root/.careerpilot-api.log" -RedirectStandardError "$Root/.careerpilot-api-error.log" -NoNewWindow
$web = Start-Process npm -ArgumentList 'start', '--prefix', "$Root/src/frontend", '--', '--host', '0.0.0.0' -WorkingDirectory $Root -PassThru -RedirectStandardOutput "$Root/.careerpilot-web.log" -RedirectStandardError "$Root/.careerpilot-web-error.log" -NoNewWindow
try {
  if (-not (Wait-Url "$ApiUrl/health" 'Backend API')) { Fail 'API did not start; see .careerpilot-api.log.' }
  if (-not (Wait-Url $WebUrl 'Angular')) { Fail 'Angular did not start; see .careerpilot-web.log.' }
  Write-Host "`nCareerPilot AI startup dashboard`n  Database: migrations and local user seed ready`n  Backend: $ApiUrl`n  Angular: $WebUrl`n  Ollama: http://localhost:11434 ($OllamaModel)`n  Playwright: Chromium ready`n  Swagger: $ApiUrl/swagger`n"
  if (-not $NoBrowser) { Start-Process $WebUrl; Start-Process "$ApiUrl/swagger"; Start-Process "$ApiUrl/health" }
  Wait-Process -Id @($api.Id, $web.Id)
} finally { Stop-Process -Id $api.Id, $web.Id -ErrorAction SilentlyContinue }
