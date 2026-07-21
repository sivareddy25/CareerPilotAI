#!/usr/bin/env bash
set -euo pipefail
source "$(dirname "$0")/common.sh"
require_command dotnet 'Install the .NET 9 SDK: https://dotnet.microsoft.com/download/dotnet/9.0'
require_command npm 'Install Node.js 20 or newer: https://nodejs.org/'
info 'Restoring .NET packages…'
dotnet restore "$BACKEND_SOLUTION"
info 'Installing frontend packages from the lock file…'
npm ci --prefix "$FRONTEND_DIR"
success 'Dependencies restored.'
