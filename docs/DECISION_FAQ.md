# ❓ ADPA Frontend Decision FAQ (Blazor vs Next.js)

Status: Final • Audience: Stakeholders & Tech Leads • Length: 1 page

---

## 1) Why Blazor instead of Next.js for ADPA?
- ADPA is an internal, document‑processing dashboard with real‑time status and deep .NET integration. Blazor keeps a single, type‑safe stack and enables direct use of .NET libraries and SignalR without a second frontend stack.
- Next.js shines for public, SEO‑heavy sites and content publishing. That’s not ADPA’s core need.

Sources:
- Microsoft Docs – Blazor overview: `https://learn.microsoft.com/aspnet/core/blazor/`
- SignalR overview: `https://learn.microsoft.com/aspnet/core/signalr/introduction`

## 2) Can Blazor scale to thousands of users?
Yes—with the right architecture (pooled connections, background processing, caching). For higher scale and reduced connection density, combine Blazor Server with SSR and defer interactivity, or use Blazor WebAssembly for specific surfaces.

Sources:
- Blazor Server hosting & scale: `https://learn.microsoft.com/aspnet/core/blazor/hosting-models#blazor-server`
- ASP.NET Core performance best practices: `https://learn.microsoft.com/aspnet/core/performance/`

## 3) Are there SEO concerns?
ADPA is an internal dashboard; SEO is not a priority. If marketing/SEO becomes important, run a separate Next.js static site while keeping the core app in Blazor.

Sources:
- Next.js SEO/SSG: `https://nextjs.org/learn/seo/introduction-to-seo`
- Blazor prerendering: `https://learn.microsoft.com/aspnet/core/blazor/components/prerendering-and-streaming`

## 4) What about real‑time updates and notifications?
SignalR is first‑class in ASP.NET Core and integrates directly with Blazor Server. No separate WebSocket stack or Socket.io is required.

Sources:
- SignalR hub basics: `https://learn.microsoft.com/aspnet/core/signalr/hubs`

## 5) How do we handle large file uploads and processing?
Use server‑side streaming to storage (e.g., Azure Blob Storage), then process via background workers. Practical limits are governed by server settings, network, and storage throughput—not browser constraints.

Sources:
- Upload files in ASP.NET Core: `https://learn.microsoft.com/aspnet/core/mvc/models/file-uploads`
- Azure Blob Storage .NET: `https://learn.microsoft.com/azure/storage/blobs/storage-quickstart-blobs-dotnet`

## 6) Is Microsoft committed to Blazor?
Yes. Blazor is part of ASP.NET Core and continues to receive active investment. Use .NET 8 LTS for production stability.

Sources:
- .NET support policy (LTS vs STS): `https://learn.microsoft.com/dotnet/core/porting/versioning-sdk-msbuild-vs-tools#support`
- .NET release schedule: `https://learn.microsoft.com/dotnet/core/versions/`

## 7) What’s the migration/rollback path if we ever need Next.js?
Keep clean API boundaries and shared DTOs in a `.Shared` assembly. If requirements shift to SEO/public, spin up a Next.js frontend that consumes the same APIs while retaining the .NET backend and domain logic.

Sources:
- ASP.NET Core Web API best practices: `https://learn.microsoft.com/aspnet/core/fundamentals/apis`

## 8) What are the key risks and mitigations?
- Connection density (Blazor Server): scale‑out, circuit resiliency, background jobs
- Global latency: regional hosting + CDN; prerender and defer interactivity
- Long‑running work: queues + workers (IHostedService/Azure Functions)
- Vendor lock‑in: API‑first design; shared contracts; portable domain logic

See also: Risk Register in `ARCHITECTURAL_ASSESSMENT.md`.


