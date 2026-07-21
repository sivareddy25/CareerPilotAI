# CareerPilot AI

CareerPilot AI is a local-first job-search workspace built with ASP.NET Core 9, Angular, PostgreSQL, Redis, Ollama, and Playwright.

## One-click local startup

```bash
git clone <repository-url>
cd CareerPilotAI
./run.sh
```

On Windows, run `./run.ps1` (or `run.bat`). The first run creates `.env` from `.env.example`, restores .NET and npm packages, starts PostgreSQL and Redis, verifies Ollama, downloads `llama3.2:3b`, installs Playwright Chromium, applies EF Core migrations, seeds the local user, starts the API and Angular, waits for both, and opens the application, Swagger, and health page.

Press `Ctrl+C` to stop the native API and Angular processes. PostgreSQL and Redis remain available for the next run.

## Prerequisites

- Git
- [.NET SDK 9](https://dotnet.microsoft.com/download/dotnet/9.0)
- Node.js 20+ and npm
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) for the managed PostgreSQL and Redis services
- [Ollama](https://ollama.com/download), running locally for native mode

`run.sh` and `run.ps1` check these prerequisites and stop with an actionable message when one is unavailable. The scripts use the repository-local Angular CLI and restore the repository-local EF CLI tool; no global Angular or EF installation is required.

## URLs and local user

| Service | URL |
| --- | --- |
| Angular application | http://localhost:4200 |
| API health | http://localhost:5080/health |
| Swagger | http://localhost:5080/swagger |
| Ollama | http://localhost:11434 |

The API starts in `Local` hosting mode in development. It applies pending migrations and creates the configured local user automatically. The default identity is `local.user@careerpilot.internal`; it is intended only for local development.

## Docker mode

To run the application itself in containers:

```bash
./run.sh --docker
# Windows: ./run.ps1 -Docker
```

Docker mode exposes the web app on port 80, API on port 8080, PostgreSQL on 5432, Redis on 6379, and Ollama on 11434. It also downloads the configured `OLLAMA_MODEL` into the persistent Ollama volume. Copy and adjust `.env` before running if the default development credentials conflict with your machine. Never commit `.env`.

## Configuration

`.env.example` documents local secrets and endpoints. Bootstrap only creates `.env` when it is missing; existing configuration is never overwritten. The API development connection uses the Docker-managed database:

```text
Host=localhost;Port=5432;Database=careerpilot;Username=careerpilot;Password=<POSTGRES_PASSWORD from .env>
```

For a native PostgreSQL installation, update `src/backend/CareerPilot.Api/appsettings.Development.json` or provide `Database__ConnectionString` in your shell.

## Utility commands

| Command | Purpose |
| --- | --- |
| `./scripts/restore.sh` | Restore .NET and npm dependencies |
| `./scripts/build.sh` | Restore and build with .NET warnings treated as errors |
| `./scripts/test.sh` | Run backend and frontend tests |
| `./scripts/format.sh` | Format C# and frontend source |
| `./scripts/migrate.sh` | Apply EF Core migrations |
| `./scripts/new-migration.sh Name` | Create an EF Core migration |
| `./scripts/backend.sh` | Run only the API |
| `./scripts/frontend.sh` | Run only Angular |
| `./scripts/clean.sh` | Remove build output and Angular cache |
| `./scripts/reset-db.sh` | Recreate Docker development database volumes (destructive) |

PowerShell equivalents are provided for restore, API-only, frontend-only, and full startup.

## IDEs

VS Code has **CareerPilot: Start everything** and **CareerPilot: Start Docker** tasks in `.vscode/tasks.json`; the API debug profile depends on the full startup task. Visual Studio and Rider discover the existing `CareerPilotAI.sln` and API `launchSettings.json`; use the `http` profile after starting dependencies with the one-click script.

## Troubleshooting

- **Port already in use:** stop the service using 4200, 5080, 5432, 6379, or 11434, or override `API_URL`/`WEB_URL` before startup.
- **Ollama is not running:** launch Ollama once, then rerun the command. Confirm with `ollama list`.
- **Docker database will not start:** run `docker compose logs postgres`; `./scripts/reset-db.sh` recreates the local database volume and loses local data.
- **Browser download fails:** rerun `npm exec --prefix src/frontend --yes playwright -- install chromium` after checking your network/proxy settings.
- **Startup fails:** inspect `.careerpilot-api.log` or `.careerpilot-web.log`. They contain the structured backend and Angular output.

## FAQ

**Does startup overwrite my settings?** No. It only creates `.env` if it does not exist.

**Do I need a global Angular CLI?** No. The frontend’s local CLI is used.

**Can I skip Docker?** Docker mode runs the full stack. Native mode currently uses Docker only for the supported PostgreSQL and Redis dependencies; the API, Angular, and Ollama run on your host.
