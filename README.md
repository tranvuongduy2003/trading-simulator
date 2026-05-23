# Real-time Stock Trading Simulator

Local-only MVP for simulating AAPL trading with virtual USD. Orchestrated by [.NET Aspire](https://aspire.dev).

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Aspire CLI](https://aspire.dev) 13.3+
- [Node.js 22 LTS](https://nodejs.org/) and [Yarn](https://yarnpkg.com/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (PostgreSQL, Redis containers)

## First-time setup

From the repository root (Windows PowerShell):

```powershell
.\scripts\Setup-Environments.ps1
```

Or manually:

```bash
# Restore backend
dotnet restore TradingSimulator.slnx

# Install frontend
yarn --cwd web install

# Optional local env overrides
cp .env.example .env
cp web/.env.example web/.env

# HTTPS dev certificate (create + trust; also done by Setup-Environments.ps1)
dotnet dev-certs https
dotnet dev-certs https --trust
```

## Run locally

Start the full stack from the AppHost (recommended):

```bash
dotnet run --project src/AppHost/TradingSimulator.AppHost.csproj
```

Or with the Aspire CLI:

```bash
aspire run --project src/AppHost/TradingSimulator.AppHost.csproj
```

The Aspire dashboard shows logs, health, and endpoints for PostgreSQL (with PgAdmin), Redis (with Redis Commander), API, matching engine, and the Vite frontend.

**HTTPS (default under AppHost):** API at `https://localhost:8000` (HTTP `http://localhost:8001`), Vite at `https://localhost:5000`. Aspire injects the ASP.NET dev certificate for both; run `dotnet dev-certs https --trust` once if the browser warns. `VITE_API_URL` is set to the API HTTPS endpoint automatically.

To run **Vite alone** with HTTPS, copy `web/.env.example` to `web/.env`, set `VITE_DEV_HTTPS=1`, then `yarn --cwd web dev`.

## Solution layout

```
src/
  AppHost/           Aspire orchestration (topology source of truth)
  ServiceDefaults/   Shared logging, health, service discovery
  Api/               REST + SignalR host
  MatchingEngine/    Matching worker host
  Application/       CQRS handlers and ports
  Domain/            Pure domain model
  Infrastructure/    EF Core, Redis, adapters
  Contracts/           DTOs and hub messages
tests/               Unit and integration test projects
web/                 React 19 + Vite frontend (outside .slnx)
```

## Configuration

Layering (later wins): `appsettings.json` → `appsettings.Development.json` → Aspire-injected env → user secrets.

Key areas live under the `Trading` section in service `appsettings.json` (initial cash, channel capacity, simulated liquidity, reset cooldown).

Frontend reads `VITE_API_URL` (injected by AppHost when running under Aspire).

### Local connection strings (AppHost source of truth)

| Resource | AppHost | Connection key / URI |
|----------|---------|----------------------|
| PostgreSQL | `AddPostgres("postgres")` + `AddDatabase("trading")` | `ConnectionStrings__trading` / `POSTGRES_URI` in `.env.example` |
| Redis | `AddRedis("cache")` | `ConnectionStrings__cache` / `REDIS_HOST:REDIS_PORT` |

Password: `Parameters:postgres-password` in `src/AppHost/appsettings.Development.json` (default `postgres`). Host ports are pinned in `AppHost.cs` (5432, 6379) for MCP and tooling.

**Secrets in git:** Copy `.env.example` → `.env`, `web/.env.example` → `web/.env`, and `.mcp.json.example` → `.mcp.json` (local only; see [`.gitignore`](.gitignore)).

## CI

GitHub Actions builds the .NET solution, runs tests, and builds the frontend on every push/PR to `main`. See [`.github/workflows/ci.yml`](.github/workflows/ci.yml).

## Tooling

| Stack | Format | Lint |
|-------|--------|------|
| .NET | `dotnet format TradingSimulator.slnx` (SDK built-in) | `Microsoft.CodeAnalysis.NetAnalyzers` (warnings as errors in `src/`) |
| Web | `yarn --cwd web format` | `yarn --cwd web lint` |

Central package versions: [`Directory.Packages.props`](Directory.Packages.props). Shared MSBuild properties: [`Directory.Build.props`](Directory.Build.props). Editor settings: [`.editorconfig`](.editorconfig).

Frontend design tokens: [`web/src/styles/globals.css`](web/src/styles/globals.css) (shadcn/ui + `--color-bid` / `--color-ask`). Add components with `yarn --cwd web exec shadcn add <name>`.

## Docs

- [`docs/PRD.md`](docs/PRD.md) — product scope
- [`docs/TECHNICAL.md`](docs/TECHNICAL.md) — architecture
- [`docs/DATABASE.md`](docs/DATABASE.md) — schema design
