# ADPA (.NET 8 LTS) – Automated Document Processing & Analytics

ADPA is an enterprise document processing and analytics platform built on ASP.NET Core (.NET 8 LTS). It provides APIs and a real‑time dashboard for uploading, processing, and monitoring documents at scale.

---

## Current Features (as of this commit)

- .NET 8.0 (LTS) Web API (clean architecture, DI, logging)
- 20+ controllers and 30+ services for core processing (per internal docs)
- Background/async processing pipeline (queued work, non‑blocking requests)
- Health and diagnostics endpoints
- SignalR hub for real‑time processing status (`Hubs/ProcessingHub.cs`)
- CORS and standard ASP.NET security middleware
- Static HTML/JS demo dashboard (to be replaced by Blazor)

---

## Approved Decisions

- Frontend: Blazor Server on .NET 8 LTS (single, type‑safe .NET stack)
- Real‑time: SignalR for live status and notifications
- UI Library: MudBlazor for enterprise‑grade components
- Large files: Server‑side streaming to storage, background processing
- Cost/Timeline: Chosen for lower TCO and faster delivery vs Next.js

See supporting docs in `/docs`:
- `EXECUTIVE_SUMMARY.md`
- `ARCHITECTURAL_ASSSESSMENT.md`
- `TECHNOLOGY_COMPARISON_SUMMARY.md`
- `BLAZOR_IMPLEMENTATION_GUIDE.md`
- `DECISION_FAQ.md`

---

## Upcoming Features (approved roadmap)

Weeks 1–4 (Foundation & Core):
- Blazor Server app shell (layout, nav, auth)
- Document upload with progress, streaming to storage
- Real‑time processing status via SignalR
- Initial dashboard with live stats

Weeks 5–8 (Analytics & Security):
- Analytics dashboard (charts, KPIs, trends)
- Document list, details, and viewer
- Security interfaces: policies, monitoring, audit

Weeks 9–12 (Administration & Polish):
- Admin tools (users, settings)
- Performance optimizations, testing (bUnit/Playwright)
- Deployment pipeline and run‑books

For details, see `BLAZOR_IMPLEMENTATION_GUIDE.md` and `ROADMAP.md`.

---

## Tech Stack

- Backend: ASP.NET Core (.NET 8 LTS), SignalR, EF Core (planned)
- Frontend (approved): Blazor Server with MudBlazor
- Storage/Infra (recommended): Azure App Service, Azure Storage/Service Bus, Application Insights

---

## Quick Start (API)

Prerequisites:
- .NET 8.0 SDK (LTS): `https://dotnet.microsoft.com/download`

Build & Run:
```bash
dotnet restore
dotnet build
dotnet run
```

Default URLs:
- HTTP: `http://localhost:5050`
- HTTPS: `https://localhost:7050`

---

## Key API Endpoints

Base URL (dev):
- HTTP: `http://localhost:5050`
- HTTPS: `https://localhost:7050`

Core:
- `GET /api/health` — Service health and version
- `GET /api/data` — List processed data requests
- `GET /api/data/{id}` — Get a specific request by ID
- `POST /api/data` — Submit data for processing
- `POST /api/data/process-pending` — Manually trigger processing of pending data

Realtime:
- `GET /hubs/processing` — SignalR hub for processing status (WebSocket)

Tip: Swagger/OpenAPI is recommended for discoverability (see `docs/INFRASTRUCTURE_SUCCESS.md`).

---

## Swagger/OpenAPI (Quick Setup)

Add interactive API docs at `/swagger`.

1) Install package:
```bash
dotnet add package Swashbuckle.AspNetCore
```

2) Update `Program.cs`:
```csharp
// Register
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Enable
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// For production environments that allow it, you can enable:
// app.UseSwagger();
// app.UseSwaggerUI();

app.Run();
```

Open: `https://localhost:7050/swagger`

---

## Documentation

All decision records and implementation details are tracked in `/docs`:
- Decision summary: `docs/EXECUTIVE_SUMMARY.md`
- Architecture: `docs/ARCHITECTURAL_ASSESSMENT.md`
- Comparison: `docs/TECHNOLOGY_COMPARISON_SUMMARY.md`
- Implementation: `docs/BLAZOR_IMPLEMENTATION_GUIDE.md`
- FAQ: `docs/DECISION_FAQ.md`

---

## Contributing and Branching

Workflow (GitHub Flow‑style):
- Default branch: `main`
- Create feature branches from `main` using prefixes:
  - `feature/<short-description>` for new features
  - `fix/<short-description>` for bug fixes
  - `chore/<short-description>` for tooling/infra
- Keep PRs small and focused; link to related issue if applicable.
- Use Conventional Commit messages where practical (e.g., `feat: add document viewer`).
- Require at least one review approval before merging.
- CI should pass (build + basic tests) before merge.

Release tips:
- Use tags for notable releases (e.g., `v0.1.0`).
- Keep `/docs` decision records updated when architectural choices change.

---

## License

Proprietary – Internal use only (update this section if a license is chosen).
