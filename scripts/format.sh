#!/usr/bin/env bash
set -euo pipefail
source "$(dirname "$0")/common.sh"
dotnet format "$BACKEND_SOLUTION" --no-restore
npm exec --prefix "$FRONTEND_DIR" prettier -- --write src
