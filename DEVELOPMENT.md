# Developer Onboarding & Local Setup Guide

Welcome to CareerPilot AI. Follow these instructions to run and test the application locally.

---

## 1. Prerequisites
- **.NET 9 SDK**: Version 9.0+
- **Node.js**: Version 20.x or higher (`npm v10+`)
- **Docker & Docker Compose**: (Optional, for containerized database/redis stack)
- **PostgreSQL 16** & **Redis 7** (if running locally outside Docker)

---

## 2. Running Backend (.NET 9)

```bash
cd src/backend
dotnet restore CareerPilotAI.sln
dotnet build CareerPilotAI.sln
dotnet run --project CareerPilot.Api/CareerPilot.Api.csproj
```
The Web API will launch at `http://localhost:5000` (or `http://localhost:8080`). Swagger documentation is available at `http://localhost:5000/swagger`.

---

## 3. Running Frontend (Angular 22)

```bash
cd src/frontend
npm install
npm run start
```
The Angular Web Application will launch at `http://localhost:4200`.

---

## 4. Running full stack with Docker Compose

```bash
docker-compose up --build
```
Access points:
- Angular Web App: `http://localhost`
- ASP.NET Core API: `http://localhost:8080`
- PostgreSQL: `localhost:5432`
- Redis: `localhost:6379`
