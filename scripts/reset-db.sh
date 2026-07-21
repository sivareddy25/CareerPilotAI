#!/usr/bin/env bash
set -euo pipefail
source "$(dirname "$0")/common.sh"
ensure_env
require_command docker 'Resetting the managed development database requires Docker.'
docker compose --env-file "$ROOT_DIR/.env" -f "$ROOT_DIR/docker-compose.yml" rm -s -f postgres
docker volume rm careerpilot_postgres-data >/dev/null 2>&1 || true
docker compose --env-file "$ROOT_DIR/.env" -f "$ROOT_DIR/docker-compose.yml" up -d postgres redis
info 'Database volume recreated. Start the application to apply migrations and seed local data.'
