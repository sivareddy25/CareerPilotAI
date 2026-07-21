#!/usr/bin/env bash
set -euo pipefail
source "$(dirname "$0")/common.sh"
ensure_env
ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS="$API_URL" \
  Database__ConnectionString="Host=localhost;Port=5432;Database=careerpilot;Username=careerpilot;Password=${POSTGRES_PASSWORD}" \
  dotnet run --project "$BACKEND_PROJECT" --no-launch-profile
