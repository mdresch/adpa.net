# 🤖 ADPA AI Agent Handover Guide

Purpose: Enable an AI coding agent to start contributing immediately with clear context, priorities, and safe operating procedures.

Status: Current • Updated for .NET 8 LTS and Blazor decision

---

## 1) Project Snapshot

- Runtime: .NET 8.0 (LTS)
- App type: ASP.NET Core Web API + SignalR; Blazor Server frontend approved (to be added)
- Key folders:
  - `Controllers/` – API endpoints
  - `Services/` – Domain logic (analytics, reporting, intelligence, workflow, security, etc.)
  - `Data/` – EF Core DbContexts and repositories
  - `Hubs/` – SignalR hubs (`ProcessingHub.cs`)
  - `Middleware/` – Global exception handling
  - `wwwroot/` – Static demo UI (to be replaced by Blazor)
  - `docs/` – Decisions, plans, and guides (start with `REVIEW_SUMMARY_README.md`)
- Core decisions (approved):
  - Frontend: Blazor Server (MudBlazor) on .NET 8 LTS
  - Real‑time: SignalR
  - Large files: Server‑side streaming → storage → background processing
  - Cost/time: Blazor chosen over Next.js (see `EXECUTIVE_SUMMARY.md`)

---

## 2) Environment & Run

Prerequisites:
- .NET 8.0 SDK (LTS)

Build & run (dev):
```bash
dotnet restore
dotnet build
dotnet run
```

Dev URLs:
- API Swagger: https://localhost:7050/swagger
- SignalR Hub: wss://localhost:7050/hubs/processing
- Static demo: https://localhost:7050

Configuration:
- `appsettings.json` / `appsettings.Development.json`
- Connection string key: `ConnectionStrings:DefaultConnection`

---

## 3) Key Endpoints & Realtime

Core:
- `GET /api/health` – Health/version
- `GET /api/data` – List processed data
- `POST /api/data` – Submit data to process
- `POST /api/data/process-pending` – Trigger queued processing

Documents:
- `POST /api/documents/upload` – Upload file (multipart/form-data)
- `GET /api/documents` – List documents
- `GET /api/documents/{id}` – Get one

SignalR:
- Hub: `/hubs/processing`
- Used for progress/status updates during processing

Explore the full surface at Swagger: `/swagger`.

---

## 4) Priority Roadmap (next 2–4 weeks)

Week 1–2 (Foundation & Core UI):
- Create `ADPA.Web` Blazor Server project (see `BLAZOR_IMPLEMENTATION_GUIDE.md`)
- Layout + navigation + auth wiring (MudBlazor)
- Document upload component (progress + streaming to storage)
- Live status via SignalR

Week 3–4 (Analytics & Management):
- Analytics dashboard with charts and KPIs
- Document list/details/viewer
- Basic security interfaces (policies, monitoring, audit)

Reference: `ROADMAP.md`, `BLAZOR_IMPLEMENTATION_GUIDE.md`.

---

## 5) Contribution Workflow

Branching:
- Default branch: `main`
- Use prefixes: `feature/<desc>`, `fix/<desc>`, `chore/<desc>`
- Keep PRs small; one review approval required
- Conventional commits encouraged: `feat: add document viewer`

Quality gates (aim for):
- Build/test pass locally
- No new critical analyzer warnings
- Basic unit/component test if feasible

Docs:
- Update `/docs` if changing decisions/architecture
- Link PRs to relevant docs or create new notes

---

## 6) Coding Standards & Safety

General:
- Prefer clear, verbose, maintainable code
- Use dependency injection; avoid singletons unless stateless
- Keep long‑running work out of controllers and SignalR hubs (use background workers)

DTOs & contracts:
- Centralize shared contracts; version APIs; validate inputs

Security:
- Respect auth policies; never weaken middleware in production
- Sanitize logs; avoid logging secrets or PII
- Apply CORS policies thoughtfully (SignalR policy present)

Performance:
- Async I/O; streaming for uploads; response compression; caching when safe

---

## 7) Data & Processing

EF Core:
- DbContext: `AdpaEfDbContext`
- Repositories in `Data/Repositories/`

Processing pattern (approved):
1) Upload stream → persistent storage (e.g., Azure Blob)
2) Enqueue processing job
3) Background worker processes and emits progress
4) SignalR pushes updates to connected clients

Note: Avoid processing heavy files in controller request scope.

---

## 8) Observability

Logging:
- Centralized via ASP.NET Core logging
- Global exception middleware: `Middleware/GlobalExceptionMiddleware.cs`

Recommended (future):
- Application Insights + OpenTelemetry (traces/metrics)
- Structured logs (request IDs, correlation IDs)

---

## 9) Where to Start (AI Agent Tasks)

Good first issues:
1) Add Azure Blob abstraction and stream uploads there (config‑driven)
2) Introduce a background worker (`IHostedService`) + simple in‑memory queue
3) Wire processing progress to `ProcessingHub` with throttling
4) Scaffold `ADPA.Web` (Blazor Server) with MudBlazor and add FileUpload component
5) Add simple analytics endpoint and chart on the dashboard

Stretch:
- Add OpenAPI annotations and response types for key endpoints
- Implement basic auth guardrails (JWT validation pipeline integration)

---

## 10) Knowledge Base

Read these first (in `/docs`):
- `REVIEW_SUMMARY_README.md` – Index and navigation
- `EXECUTIVE_SUMMARY.md` – Why Blazor and .NET 8 LTS
- `ARCHITECTURAL_ASSESSMENT.md` – Risk register, feature analysis
- `BLAZOR_IMPLEMENTATION_GUIDE.md` – Step‑by‑step Blazor setup
- `DECISION_FAQ.md` – Sourced answers for common questions

---

## 11) Guardrails for AI Changes

- Never commit secrets or connection strings; use user secrets or env vars
- Preserve existing indentation and formatting; minimize unrelated diffs
- Do not remove security headers/middleware without explicit approval
- Keep endpoints backward compatible unless versioned
- Add small, focused tests where practical

---

## 12) Contacts & Ownership

- Code owners: see repository settings (or ask maintainers)
- Design authority: decisions tracked in `/docs`
- Raise questions: open a draft PR with a brief context summary

---

With these guidelines and references, an AI agent can begin delivering value immediately—start with the upload → queue → process → SignalR loop and the Blazor shell. Happy shipping!

