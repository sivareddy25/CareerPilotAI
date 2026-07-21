#!/usr/bin/env bash
set -euo pipefail
source "$(dirname "$0")/common.sh"
dotnet clean "$BACKEND_SOLUTION"
rm -rf "$FRONTEND_DIR/.angular/cache" "$FRONTEND_DIR/dist"
success 'Build output and Angular cache removed; dependencies and database were preserved.'
