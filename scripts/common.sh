#!/usr/bin/env bash
# Shared, side-effect-free helpers for the POSIX developer commands.
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
BACKEND_PROJECT="$ROOT_DIR/src/backend/CareerPilot.Api/CareerPilot.Api.csproj"
BACKEND_SOLUTION="$ROOT_DIR/src/backend/CareerPilotAI.sln"
FRONTEND_DIR="$ROOT_DIR/src/frontend"
API_URL="${API_URL:-http://localhost:5080}"
WEB_URL="${WEB_URL:-http://localhost:4200}"
OLLAMA_MODEL="${OLLAMA_MODEL:-llama3.2:3b}"

info() { printf '\033[0;36m[CareerPilot]\033[0m %s\n' "$*"; }
success() { printf '\033[0;32m[CareerPilot]\033[0m %s\n' "$*"; }
warn() { printf '\033[0;33m[CareerPilot]\033[0m %s\n' "$*" >&2; }
fail() { printf '\033[0;31m[CareerPilot]\033[0m %s\n' "$*" >&2; exit 1; }
require_command() { command -v "$1" >/dev/null 2>&1 || fail "Missing $1. $2"; }
wait_for_url() {
  local url="$1" name="$2" attempts="${3:-45}"
  for ((i=1; i<=attempts; i++)); do
    if curl --fail --silent --output /dev/null "$url"; then success "$name is ready: $url"; return 0; fi
    sleep 1
  done
  return 1
}
ensure_env() {
  if [[ ! -f "$ROOT_DIR/.env" ]]; then
    cp "$ROOT_DIR/.env.example" "$ROOT_DIR/.env"
    success "Created .env from .env.example (existing configuration is never overwritten)."
  fi
  set -a; source "$ROOT_DIR/.env"; set +a
}
open_url() { command -v open >/dev/null && open "$1" || command -v xdg-open >/dev/null && xdg-open "$1" || true; }
