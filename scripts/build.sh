#!/usr/bin/env bash
set -euo pipefail
source "$(dirname "$0")/common.sh"
"$ROOT_DIR/scripts/restore.sh"
dotnet build "$BACKEND_SOLUTION" --no-restore -warnaserror
npm run build --prefix "$FRONTEND_DIR"
success 'Build completed without warnings.'
