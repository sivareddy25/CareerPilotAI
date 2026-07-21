#!/usr/bin/env bash
set -euo pipefail
source "$(dirname "$0")/common.sh"
dotnet test "$BACKEND_SOLUTION" --no-restore
npm test --prefix "$FRONTEND_DIR" -- --run
