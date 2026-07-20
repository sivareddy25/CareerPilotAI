# CareerPilot AI

**AI-powered job application automation platform.**

CareerPilot AI helps candidates run their job search as a managed pipeline rather than a
pile of browser tabs: sourcing roles, tailoring application material per posting, submitting
through automated browser flows, and tracking every application's state end to end.

> **Status — Phase 1: Structural scaffold.**
> This repository currently contains the solution topology only. There is deliberately
> **no** business logic, authentication, persistence, browser automation, or AI integration
> in the tree yet. Each of those arrives in its own phase against the boundaries established
> here. See [`docs/03-Roadmap.md`](docs/03-Roadmap.md).

---

## Table of contents

- [Architecture](#architecture)
- [Technology stack](#technology-stack)
- [Folder structure](#folder-structure)
- [How to run](#how-to-run)
- [Development workflow](#development-workflow)

---

## Architecture

The backend follows **Clean Architecture**. The organising rule is that *source-code
dependencies point inward only* — outer layers know about inner layers, never the reverse.

```
        ┌─────────────────────────────────────────────┐
        │                CareerPilot.Api              │  HTTP, DI composition root
        └───────────────┬──────────────┬──────────────┘
                        │              │
                        ▼              ▼
        ┌──────────────────────┐  ┌──────────────────────────┐
        │ CareerPilot.         │  │ CareerPilot.             │
        │ Application          │◀─│ Infrastructure           │
        │ use cases, ports     │  │ adapters: EF, Redis,     │
        └──────────┬───────────┘  │ Playwright, AI clients   │
                   │              └──────────────────────────┘
                   ▼
        ┌──────────────────────┐
        │  CareerPilot.Domain  │  entities, value objects, rules
        └──────────┬───────────┘
                   ▼
        ┌──────────────────────┐
        │  CareerPilot.Shared  │  dependency-free primitives
        └──────────────────────┘
```

### Layer responsibilities

| Project | Depends on | Holds | Must never hold |
| --- | --- | --- | --- |
| **CareerPilot.Shared** | *nothing* | `Result<T>`, error codes, paging envelopes, constants | Anything domain-specific |
| **CareerPilot.Domain** | Shared | Entities, value objects, domain events, invariants | EF attributes, HTTP types, DI |
| **CareerPilot.Application** | Domain, Shared | Commands, queries, handlers, pipeline behaviors, **port interfaces** | Concrete I/O, SQL, SDK clients |
| **CareerPilot.Infrastructure** | Application | EF Core, Postgres, Redis, Playwright, AI SDKs — **implementations of Application ports** | Business rules |
| **CareerPilot.Api** | Application, Infrastructure | Endpoints, middleware, DI wiring, configuration | Business rules, data access |

### Why the dependency arrows point this way

**Infrastructure depends on Application, not the other way round.** This is the dependency
inversion that makes the architecture worth having. `Application` declares *ports* —
`IApplicationDbContext`, `IJobBoardAutomationClient`, `IResumeGenerator` — as interfaces it
owns. `Infrastructure` supplies the *adapters*. The consequence: swapping Postgres for
another store, or Playwright for a vendor API, is an Infrastructure edit that cannot
propagate inward. Use cases stay testable without a database or a browser.

**Api references Infrastructure.** Strictly, the HTTP layer should not know about adapters.
It does here for one narrowly-scoped reason: something has to be the *composition root* that
binds ports to adapters at startup, and `Program.cs` is the pragmatic place. The discipline
that keeps this honest is that `Api` may call `AddInfrastructure()` and **nothing else** from
that assembly — no repository types, no `DbContext`, in any endpoint. If you want this
mechanically enforced rather than agreed by convention, an architecture-test project can
assert it once the testing phase lands.

**Shared sits beneath Domain.** `Shared` is for primitives with genuinely no business
meaning — result types, pagination, error codes — and it carries **zero** dependencies. Its
position at the bottom of the graph is what stops it degrading into the usual "misc dumping
ground": anything that needs to reference a domain concept structurally cannot live there.

**CQRS-ready, not CQRS-implemented.** `Application/Features/` is laid out for vertical
slices (one folder per use case, each owning its command/query, handler, validator, and
response), and `Application/Behaviors/` is reserved for cross-cutting pipeline concerns —
validation, logging, transactions. No mediator library is installed in Phase 1. That is
deliberate: the dispatcher choice (MediatR is no longer free for commercial use, so
alternatives are worth weighing) is a decision to make when the first handler is written,
not before. The folder layout does not depend on which one wins.

**Minimal APIs over controllers.** `Api/Endpoints/` is structured for endpoint modules
grouped by feature. Minimal APIs are the .NET 9 default, allocate less per request, and map
cleanly onto vertical slices. Swapping to controllers later is contained to this project.

### Frontend architecture

Angular 22, standalone components throughout, **zoneless** change detection with signals as
the state primitive. No `zone.js` in the dependency tree — rendering is driven by signal
graph invalidation, which is both faster and far easier to reason about than monkey-patched
async change detection.

`core/` is for singletons loaded once at bootstrap; `shared/` for stateless reusable
presentation; `features/` for lazily-routed business areas; `layouts/` for the chrome that
wraps routed views.

`guards/`, `interceptors/`, `services/`, and `models/` live *inside* `core/` rather than at
the app root. All four are singleton, bootstrap-time concerns, so nesting them states that
relationship structurally — and it keeps the app root to four entries that each answer a
different question: *loaded once* (`core`), *reused* (`shared`), *routed* (`features`),
*wraps routes* (`layouts`). A flat root works at small scale but blurs that distinction as
the feature count grows.

---

## Technology stack

### Backend
| Concern | Choice |
| --- | --- |
| Runtime | .NET 9 |
| API | ASP.NET Core Web API (minimal APIs) |
| Architecture | Clean Architecture, CQRS-ready |
| ORM | EF Core *(Phase 2)* |
| Database | PostgreSQL *(Phase 2)* |
| Cache | Redis *(Phase 2)* |

### Frontend
| Concern | Choice |
| --- | --- |
| Framework | Angular 22 |
| Components | Standalone (no NgModules) |
| State | Signals, zoneless change detection |
| Styling | SCSS |
| Language | TypeScript 6 |

### Infrastructure
| Concern | Choice |
| --- | --- |
| Containers | Docker (multi-stage builds) |
| Orchestration | Docker Compose (local) |
| CI | GitHub Actions *(Phase 2)* |

---

## Folder structure

```
CareerPilotAI/
├── .github/
│   └── workflows/                  # CI pipelines (Phase 2)
├── docker/
│   ├── docker-compose.yml          # local stack: api, web, postgres, redis
│   ├── Dockerfile.api              # multi-stage .NET build → aspnet runtime
│   └── Dockerfile.web              # multi-stage Angular build → nginx
├── docs/
│   ├── 01-Vision.md                08-AI.md
│   ├── 02-Architecture.md          09-Deployment.md
│   ├── 03-Roadmap.md               10-Testing.md
│   ├── 04-Database.md
│   ├── 05-Backend.md
│   ├── 06-Frontend.md
│   └── 07-Automation.md
├── scripts/                        # developer tooling scripts
└── src/
    ├── backend/
    │   ├── CareerPilotAI.sln
    │   ├── CareerPilot.Api/
    │   │   ├── Configuration/      # strongly-typed options binding
    │   │   ├── Endpoints/          # minimal-API endpoint modules
    │   │   ├── Extensions/         # IServiceCollection / WebApplication helpers
    │   │   └── Middleware/         # exception handling, correlation IDs
    │   ├── CareerPilot.Application/
    │   │   ├── Abstractions/
    │   │   │   ├── Messaging/      # ICommand, IQuery, handler contracts
    │   │   │   ├── Persistence/    # repository + unit-of-work ports
    │   │   │   └── Services/       # ports for AI, automation, email
    │   │   ├── Behaviors/          # validation, logging, transaction pipeline
    │   │   ├── Common/
    │   │   │   ├── Mapping/
    │   │   │   └── Validation/
    │   │   └── Features/           # vertical slices, one folder per use case
    │   ├── CareerPilot.Domain/
    │   │   ├── Abstractions/       # Entity, AggregateRoot, IDomainEvent
    │   │   ├── Entities/
    │   │   ├── Enums/
    │   │   ├── Events/
    │   │   ├── Exceptions/
    │   │   └── ValueObjects/
    │   ├── CareerPilot.Infrastructure/
    │   │   ├── Ai/                 # LLM client adapters
    │   │   ├── Automation/         # Playwright browser automation
    │   │   ├── Caching/            # Redis
    │   │   ├── Identity/           # auth provider integration
    │   │   ├── Messaging/          # queues, outbox, email
    │   │   ├── Persistence/
    │   │   │   ├── Configurations/ # EF entity type configurations
    │   │   │   ├── Migrations/
    │   │   │   └── Repositories/
    │   │   └── Services/
    │   └── CareerPilot.Shared/
    │       ├── Constants/
    │       ├── Errors/
    │       ├── Extensions/
    │       ├── Pagination/
    │       └── Results/
    └── frontend/
        └── src/
            ├── app/
            │   ├── core/               # bootstrap-time singletons
            │   │   ├── guards/         # route guards
            │   │   ├── interceptors/   # HTTP interceptors
            │   │   ├── models/         # TypeScript contracts
            │   │   └── services/       # injectable services
            │   ├── features/           # lazily-routed business areas
            │   ├── layouts/            # shell / chrome components
            │   └── shared/             # reusable presentational components
            └── styles.scss
```

Empty folders are held in git by `.gitkeep` files; delete them as real code lands.

---

## How to run

### Prerequisites

| Tool | Version | Check |
| --- | --- | --- |
| .NET SDK | 9.0+ | `dotnet --version` |
| Node.js | 20.19+ / 22.12+ / 24+ | `node --version` |
| Angular CLI | 22+ | `ng version` |
| Docker Desktop | current | `docker --version` |

### Backend

```bash
cd src/backend
dotnet restore
dotnet build
dotnet run --project CareerPilot.Api
```

The API listens on the ports in `CareerPilot.Api/Properties/launchSettings.json`.
Verify it is alive:

```bash
curl http://localhost:<port>/health          # -> Healthy
```

OpenAPI is exposed in Development at `/openapi/v1.json`.

### Frontend

```bash
cd src/frontend
npm install
npm start                                     # http://localhost:4200
```

### Full stack via Docker

```bash
cd docker
docker compose up --build
```

| Service | URL |
| --- | --- |
| Web | http://localhost:4200 |
| API | http://localhost:8080 |
| Postgres | `localhost:5432` |
| Redis | `localhost:6379` |

Postgres and Redis run but are not yet consumed by the API — they are provisioned ahead of
Phase 2 so the topology is settled before persistence lands.

---

## Development workflow

### Branching

```
main                 always releasable
  └── feature/<slice>    one vertical slice per branch
  └── fix/<summary>
  └── chore/<summary>
```

### Commits

[Conventional Commits](https://www.conventionalcommits.org/) — `feat:`, `fix:`, `chore:`,
`docs:`, `refactor:`, `test:`. This keeps changelog generation mechanical later.

### Adding a backend feature

Work outward, not inward — the dependency graph is also the implementation order:

1. **Domain** — model the concept. Entities and value objects that enforce their own invariants.
2. **Application** — add the vertical slice under `Features/<Area>/<UseCase>/`: command or
   query, handler, validator, response. If it needs I/O, declare a **port interface** under
   `Abstractions/` — do not reach for a concrete client.
3. **Infrastructure** — implement that port. Register it in the layer's DI extension.
4. **Api** — expose an endpoint module under `Endpoints/` that dispatches the use case and
   maps the result to HTTP.

If a step forces you to reference outward — Application needing an EF type, Domain needing
`HttpContext` — that is the architecture reporting a modelling problem, not an obstacle to
work around.

### Adding a frontend feature

1. Contracts in `core/models/`.
2. Data access in `core/services/` (typed, signal-exposing).
3. UI in `features/<area>/` as standalone components, lazily loaded via `app.routes.ts`.
4. Promote to `shared/` only on the *second* consumer, not in anticipation of one.

### Before pushing

```bash
dotnet build src/backend/CareerPilotAI.sln    # zero warnings
cd src/frontend && npm run build
```

### Documentation

Architectural decisions belong in `docs/`, not only in commit messages. If a change alters a
boundary, update the relevant numbered document in the same PR.
