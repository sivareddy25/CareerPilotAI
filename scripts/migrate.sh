#!/usr/bin/env bash
set -euo pipefail
source "$(dirname "$0")/common.sh"
require_command dotnet 'Install the .NET 9 SDK.'
ensure_env
dotnet tool restore
Database__ConnectionString="Host=localhost;Port=5432;Database=careerpilot;Username=careerpilot;Password=${POSTGRES_PASSWORD}" \
  dotnet ef database update --project "$ROOT_DIR/src/backend/CareerPilot.Infrastructure" --startup-project "$ROOT_DIR/src/backend/CareerPilot.Api"
success 'Database migrations applied.'
