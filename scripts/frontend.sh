#!/usr/bin/env bash
set -euo pipefail
source "$(dirname "$0")/common.sh"
npm start --prefix "$FRONTEND_DIR" -- --host 0.0.0.0
