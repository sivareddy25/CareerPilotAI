$root = Split-Path -Parent $PSScriptRoot
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:ASPNETCORE_URLS = if ($env:API_URL) { $env:API_URL } else { 'http://localhost:5080' }
$env:Database__ConnectionString = "Host=localhost;Port=5432;Database=careerpilot;Username=careerpilot;Password=$env:POSTGRES_PASSWORD"
dotnet run --project "$root/src/backend/CareerPilot.Api/CareerPilot.Api.csproj" --no-launch-profile
