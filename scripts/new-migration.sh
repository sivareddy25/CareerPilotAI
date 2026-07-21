#!/usr/bin/env bash
set -euo pipefail
source "$(dirname "$0")/common.sh"
[[ $# -eq 1 ]] || fail 'Usage: ./scripts/new-migration.sh <MigrationName>'
dotnet tool restore
dotnet ef migrations add "$1" --project "$ROOT_DIR/src/backend/CareerPilot.Infrastructure" --startup-project "$ROOT_DIR/src/backend/CareerPilot.Api"
