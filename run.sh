#!/usr/bin/env bash
# CareerPilot AI one-click local bootstrap. Usage: ./run.sh [--docker] [--no-browser]
set -euo pipefail
ROOT_DIR="$(cd "$(dirname "$0")" && pwd)"
source "$ROOT_DIR/scripts/common.sh"

mode=native
open_browser=true
for argument in "$@"; do
  case "$argument" in
    --docker) mode=docker ;;
    --no-browser) open_browser=false ;;
    -h|--help) echo 'Usage: ./run.sh [--docker] [--no-browser]'; exit 0 ;;
    *) fail "Unknown option: $argument" ;;
  esac
done

dashboard() {
  printf '\n\033[1;36mCareerPilot AI startup dashboard\033[0m\n'
  printf '  %-14s %s\n' 'Environment' '✓ .NET 9, Node/npm, Git'
  printf '  %-14s %s\n' 'Database' '✓ PostgreSQL migrations and seed handled by API startup'
  printf '  %-14s %s\n' 'Backend' "✓ $API_URL"
  printf '  %-14s %s\n' 'Angular' "✓ $WEB_URL"
  printf '  %-14s %s\n' 'Ollama' "✓ http://localhost:11434"
  printf '  %-14s %s\n' 'AI model' "✓ $OLLAMA_MODEL"
  printf '  %-14s %s\n' 'Playwright' '✓ Chromium installed'
  printf '  %-14s %s\n\n' 'Swagger' "$API_URL/swagger"
}

ensure_env
require_command git 'Install Git: https://git-scm.com/downloads'
require_command dotnet 'Install the .NET 9 SDK: https://dotnet.microsoft.com/download/dotnet/9.0'
require_command npm 'Install Node.js 20 or newer: https://nodejs.org/'
dotnet --list-sdks | grep -q '^9\.' || fail 'The .NET 9 SDK is required. Install it, then rerun ./run.sh.'

if [[ "$mode" == docker ]]; then
  require_command docker 'Install Docker Desktop, then rerun ./run.sh --docker.'
  info 'Starting the complete Docker development stack…'
  docker compose --env-file "$ROOT_DIR/.env" -f "$ROOT_DIR/docker-compose.yml" up --build -d
  wait_for_url 'http://localhost:8080/health' 'Backend API' 120 || fail 'Docker API did not start. Run docker compose logs backend-api.'
  wait_for_url 'http://localhost' 'Angular' 60 || fail 'Docker web app did not start. Run docker compose logs frontend-web.'
  API_URL='http://localhost:8080'; WEB_URL='http://localhost'; dashboard
  if [[ "$open_browser" == true ]]; then open_url "$WEB_URL"; open_url "$API_URL/swagger"; open_url "$API_URL/health"; fi
  info 'Docker services are running. Follow logs with: docker compose logs -f'
  exit 0
fi

require_command docker 'Native mode uses Docker for PostgreSQL and Redis. Install Docker Desktop or use ./run.sh --docker.'
"$ROOT_DIR/scripts/restore.sh"
info 'Starting PostgreSQL and Redis containers…'
docker compose --env-file "$ROOT_DIR/.env" -f "$ROOT_DIR/docker-compose.yml" up -d postgres redis
for ((i=1; i<=45; i++)); do
  if docker compose --env-file "$ROOT_DIR/.env" -f "$ROOT_DIR/docker-compose.yml" exec -T postgres pg_isready -U careerpilot -d careerpilot >/dev/null 2>&1; then break; fi
  [[ "$i" -eq 45 ]] && fail 'PostgreSQL did not become ready. Run docker compose logs postgres.'
  sleep 1
done

require_command ollama 'Install Ollama from https://ollama.com/download, start it, then rerun ./run.sh.'
wait_for_url 'http://localhost:11434/api/tags' 'Ollama' 10 || fail 'Ollama is installed but not running. Start the Ollama service and rerun ./run.sh.'
if ! ollama list | awk 'NR > 1 { print $1 }' | grep -qx "$OLLAMA_MODEL"; then
  info "Downloading the local AI model $OLLAMA_MODEL (first run only)…"
  ollama pull "$OLLAMA_MODEL"
fi

info 'Installing Playwright Chromium when needed…'
npm exec --prefix "$FRONTEND_DIR" --yes playwright -- install chromium

info 'Starting API and Angular development servers…'
"$ROOT_DIR/scripts/backend.sh" >"$ROOT_DIR/.careerpilot-api.log" 2>&1 & api_pid=$!
"$ROOT_DIR/scripts/frontend.sh" >"$ROOT_DIR/.careerpilot-web.log" 2>&1 & web_pid=$!
cleanup() { kill "$api_pid" "$web_pid" 2>/dev/null || true; }
trap cleanup EXIT INT TERM

wait_for_url "$API_URL/health" 'Backend API' 60 || { tail -n 40 "$ROOT_DIR/.careerpilot-api.log"; fail 'API did not start. See .careerpilot-api.log.'; }
wait_for_url "$WEB_URL" 'Angular' 60 || { tail -n 40 "$ROOT_DIR/.careerpilot-web.log"; fail 'Angular did not start. See .careerpilot-web.log.'; }
dashboard
if [[ "$open_browser" == true ]]; then
  open_url "$WEB_URL"; open_url "$API_URL/swagger"; open_url "$API_URL/health"
fi
info 'Development servers are running. Press Ctrl+C to stop them; logs are in .careerpilot-*.log.'
wait "$api_pid" "$web_pid"
