# Phase 19 --- Developer Experience (DX), One-Click Setup & Local Development Environment

## Objective

Design and implement a production-grade Developer Experience (DX) for
CareerPilot AI so that any developer can clone the repository and start
the complete application with minimal effort.

The goal is **Clone → Run → Application Ready**.

A developer should only need to:

``` bash
git clone <repository-url>
cd CareerPilotAI
./run.sh
```

or

``` powershell
./run.ps1
```

or simply press **Run/Debug** inside VS Code, Visual Studio or Rider.

Everything else must happen automatically.

------------------------------------------------------------------------

## Requirements

### Environment Validation

Validate automatically:

-   .NET SDK 9
-   Node.js
-   npm
-   Angular CLI (or local CLI)
-   Git
-   PostgreSQL
-   Docker (optional)
-   Ollama
-   Playwright

Stop gracefully with clear instructions if anything is missing.

### Dependency Restoration

Automatically execute:

-   dotnet restore
-   npm install

### Configuration

Generate missing development configuration files without overwriting
existing ones.

### Database

Automatically:

-   Create database
-   Apply EF Core migrations
-   Seed lookup data
-   Seed Local User

### Ollama

Verify installation and running service.

Automatically download missing AI models.

### Playwright

Automatically install browser binaries if missing.

### Startup

Automatically start:

-   Backend API
-   Angular
-   Supporting services

Wait until healthy.

### Browser

Automatically open:

-   Angular
-   Swagger
-   Health endpoint

### Startup Dashboard

Display status for:

-   Environment
-   Database
-   Backend
-   Angular
-   Ollama
-   AI Models
-   Playwright
-   Local User
-   Swagger

### Cross Platform

Provide:

-   run.sh
-   run.ps1
-   run.bat

### Docker

Support both Native and Docker development.

### IDE Integration

Support:

-   VS Code
-   Visual Studio
-   Rider

Run/Debug should start everything automatically.

### Utility Scripts

Provide scripts for:

-   Clean
-   Restore
-   Build
-   Test
-   Format
-   Reset Database
-   Create Migration
-   Apply Migration
-   Seed
-   Backend Only
-   Frontend Only
-   Start Everything

### Logging

Structured startup logging with friendly error messages.

### Documentation

Rewrite README.md with:

-   Prerequisites
-   Installation
-   One-click startup
-   Docker
-   AI setup
-   Database
-   Troubleshooting
-   FAQ

------------------------------------------------------------------------

## Quality Requirements

-   Clean Architecture
-   SOLID
-   DRY
-   KISS
-   Production-ready
-   Cross-platform
-   Zero build warnings
-   Zero analyzer warnings
-   Zero compiler warnings

------------------------------------------------------------------------

## Acceptance Criteria

-   Clone repository
-   Execute one command
-   Everything starts automatically
-   Dependencies restored
-   Database initialized
-   Migrations applied
-   Local user seeded
-   Ollama ready
-   AI models ready
-   Playwright ready
-   Backend running
-   Frontend running
-   Browser opened
-   Swagger available
-   VS Code works
-   Visual Studio works
-   Rider works
-   Docker mode works
-   Native mode works
-   README complete

------------------------------------------------------------------------

## Deliverables

-   Startup scripts
-   Environment validation
-   Dependency bootstrap
-   Database bootstrap
-   Ollama bootstrap
-   Playwright bootstrap
-   VS Code configuration
-   Visual Studio configuration
-   Rider compatibility
-   Docker support
-   Utility scripts
-   Startup dashboard
-   Updated README
-   Summary of modified files

------------------------------------------------------------------------

## Final Goal

A developer should be able to clone the repository, run a single command
or press Run in their IDE, and have the entire CareerPilot AI
application---including backend, frontend, PostgreSQL, Ollama, AI models
and Playwright---fully configured and running with almost no manual
intervention.
