# CareerPilot AI System Architecture

CareerPilot AI is built as a production-grade enterprise SaaS application following **Clean Architecture**, **Domain-Driven Design (DDD)**, and **CQRS (Command Query Responsibility Segregation)** principles.

---

## 1. System Architecture Layers

```
                          ┌───────────────────────────┐
                          │   Angular 22 Single Page  │
                          │   Application (Frontend)  │
                          └─────────────┬─────────────┘
                                        │ REST / JSON (HTTPS)
                                        ▼
                          ┌───────────────────────────┐
                          │   CareerPilot.Api         │
                          │   (ASP.NET Core 9 API)    │
                          └─────────────┬─────────────┘
                                        │ MediatR CQRS
                                        ▼
                          ┌───────────────────────────┐
                          │   CareerPilot.Application │
                          │   (CQRS Commands/Queries) │
                          └─────────────┬─────────────┘
                                        │ Interfaces & Aggregates
                   ┌────────────────────┴────────────────────┐
                   ▼                                         ▼
    ┌───────────────────────────┐             ┌───────────────────────────┐
    │   CareerPilot.Domain      │             │ CareerPilot.Infrastructure│
    │   (Aggregates, Value Objs)│             │ (EF Core, Redis, Providers)│
    └───────────────────────────┘             └─────────────┬─────────────┘
                                                            │ EF Core / SQL
                                                            ▼
                                              ┌───────────────────────────┐
                                              │ PostgreSQL 16 & Redis 7   │
                                              └───────────────────────────┘
```

### Layer Responsibilities

1. **Domain Layer (`CareerPilot.Domain`)**:
   - Zero external dependencies.
   - Contains Aggregate Roots (`User`, `ResumeDocument`, `ResumeVersion`, `Job`, `Company`, `SavedJob`, `SearchProfile`, `JobSyncLog`, `SavedJob`), Value Objects (`Location`, `SalaryRange`, `TemplateConfiguration`, `MatchScores`), and Domain Enums.

2. **Application Layer (`CareerPilot.Application`)**:
   - Contains CQRS Commands, Queries, Handlers, DTOs, and Interfaces (`IJobRepository`, `IJobNormalizationService`, `IJobSynchronizationService`, `IJobProvider`, `IJwtTokenGenerator`, `IUserContext`).

3. **Infrastructure Layer (`CareerPilot.Infrastructure`)**:
   - Implements abstractions: EF Core Npgsql DbContext (`ApplicationDbContext`), Repositories, Redis Caching, Provider Connectors (`Greenhouse`, `Lever`, `Ashby`, `Workday`, `SmartRecruiters`, `Company Career Pages`).

4. **API Layer (`CareerPilot.Api`)**:
   - ASP.NET Core 9 Web API exposing RESTful endpoints with API Versioning, JWT Bearer Auth, Rate Limiting, OpenAPI / Swagger documentation, and Health Check probes (`/healthz`).

5. **Frontend Web App (`src/frontend`)**:
   - Angular 22 standalone components architecture with Angular Signals (`signal`, `computed`), SCSS design system tokens, CSS custom variables, and Microsoft Fluent 2 aesthetics.

---

## 2. Key Modules & Subsystems

1. **Design System Infrastructure**: Microsoft Fluent 2 design tokens, color palette, typography scale, spacing, elevation, border-radius, micro-animations, and reusable components (`Button`, `Card`, `Input`, `Select`, `Badge`, `Chip`, `Modal`, `Toast`, `Spinner`, `Pagination`, `PageHeader`, `Breadcrumb`).
2. **Resume Engine & Versioning**: PDF/DOCX multi-format parser, JSON Resume schema serializer, version control engine (branch, rollback, compare), and HTML5/SCSS template engine (Classic, Modern, Executive, Minimal).
3. **Job Aggregation & Ingestion Engine**: Connector-based provider pipeline (`IJobProvider`), SHA256 content hashing, normalization service, duplicate detection, and automated deactivation.
4. **Advanced Job Search & Filtering**: Multi-column search, 15+ filter facets, custom sorting, saved jobs bookmarks, and reusable search profiles.
