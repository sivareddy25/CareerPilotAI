# CareerPilot AI API Documentation

Base URL: `http://localhost:8080/api/v1`

---

## Endpoint Sitemap

### Authentication & User Account
- `POST /api/v1/auth/register` — Create new user account
- `POST /api/v1/auth/login` — Authenticate and receive JWT token
- `GET /api/v1/auth/me` — Retrieve current authenticated user profile
- `PUT /api/v1/auth/profile` — Update profile info
- `PUT /api/v1/auth/password` — Change security password

### Job Ingestion & Aggregation
- `GET /api/v1/jobs` — Query paged, normalized job postings
- `GET /api/v1/jobs/{id:guid}` — Fetch position details
- `POST /api/v1/jobs/synchronize` — Trigger multi-provider ATS ingestion sync
- `POST /api/v1/jobs/import` — Import custom job payload
- `GET /api/v1/companies` — List hiring companies

### Advanced Job Search & Profiles
- `GET /api/v1/jobs/search` — Multi-column search & 15+ filter facets
- `GET /api/v1/jobs/saved` — List user saved jobs
- `POST /api/v1/jobs/{id:guid}/save` — Bookmark job
- `DELETE /api/v1/jobs/{id:guid}/save` — Remove bookmark
- `GET /api/v1/search-profiles` — List saved search presets
- `POST /api/v1/search-profiles` — Create search profile preset
- `DELETE /api/v1/search-profiles/{id:guid}` — Remove search profile preset

### Executive Dashboard & Notifications
- `GET /api/v1/dashboard/overview` — Get SaaS executive hub metrics, upcoming interviews, and activity feed
- `GET /api/v1/notifications` — List active in-app notifications
- `PUT /api/v1/notifications/{id:guid}/read` — Mark notification as read

### Health Checks
- `GET /healthz` — System liveness check
- `GET /readyz` — Database & Redis readiness check
