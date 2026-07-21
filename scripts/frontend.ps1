$root = Split-Path -Parent $PSScriptRoot
npm start --prefix "$root/src/frontend" -- --host 0.0.0.0
