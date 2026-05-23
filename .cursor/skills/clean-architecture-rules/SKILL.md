---
name: clean-architecture-rules
description: Enforces Clean Architecture layering, dependency direction, and project boundaries for the trading simulator solution. Use when adding projects, references, repositories, hosts (Api, MatchingEngine), Infrastructure, or wiring DI across layers.
---

# Clean Architecture Rules

Sources: `docs/TECHNICAL.md` §2–4, §10; `docs/DATABASE.md` §2–3.

## Dependency rule (non-negotiable)

```
Presentation → Infrastructure → Application → Domain
                      └──────────────────────────┘
```

- **Domain:** no outward references.
- **Application:** only Domain; declares ports (`IUserRepository`, `IUnitOfWork`, `IOrderChannelWriter`, time, etc.).
- **Infrastructure:** implements Application ports; may reference Domain for mappings.
- **Presentation** (Api, MatchingEngine, AppHost): composition root; references Application + Infrastructure.

Never reference Infrastructure from Application.

## Solution map

| Project | Layer | References |
|---------|-------|------------|
| `TradingSimulator.Domain` | Domain | — |
| `TradingSimulator.Application` | Application | Domain |
| `TradingSimulator.Infrastructure` | Infrastructure | Application, Domain |
| `TradingSimulator.Api` | Presentation | Application, Infrastructure, ServiceDefaults |
| `TradingSimulator.MatchingEngine` | Presentation | Application, Infrastructure, ServiceDefaults |
| `TradingSimulator.Contracts` | Shared DTOs | No domain logic |
| `TradingSimulator.AppHost` | Orchestration | Aspire resources |

Frontend `web/` is outside `.slnx`.

## Layer responsibilities

### Domain

Aggregates, value objects, domain events, domain services, domain exceptions. Pure C#.

### Application

- CQRS commands/queries + handlers
- FluentValidation (or equivalent) per command
- MediatR pipeline behaviors: validation, logging, unit of work
- Repository and infrastructure **interfaces** only

### Infrastructure

- EF Core `DbContext`, configurations, migrations (`trading` schema)
- Repository implementations (whole aggregates only)
- Redis adapters, channel adapters, logging sinks
- Maps DB `SMALLINT` enums ↔ domain enums

### Presentation

- Minimal endpoints / worker loops
- Auth, SignalR hub, global exception → RFC 7807
- `Program.cs` / `Add*` extension composition only wires services

## Persistence patterns

- **PostgreSQL** = source of truth; **Redis** = rebuildable projections (order book snapshot, trade tape, candles, session cache).
- One primary table per aggregate root; owned entities in child tables (`wallets`, `holdings`).
- Repositories load/save **entire aggregate roots** — no partial-aggregate APIs.
- `IUnitOfWork` wraps command transactions (EF Core transaction in Infrastructure).
- Optimistic concurrency: `row_version` on `users`, `wallets`, `portfolios`, `orders`; bounded retries at handler boundary.
- Queries: project to Contracts DTOs; prefer Redis when fresh, else PostgreSQL; use `AsNoTracking` for reads.

## Where logic belongs

| Concern | Layer |
|---------|--------|
| Invariants, state transitions | Domain |
| Use case orchestration, authorization checks | Application handlers |
| HTTP shape, cookies, antiforgery | Api |
| SQL, indexes, Redis keys | Infrastructure |
| Matching loop, channel consume | MatchingEngine host + Application commands |

## Composition root checklist

- Register Infrastructure via `AddInfrastructure(...)` (or per-area extensions), not inline in every host.
- Share `ServiceDefaults` for health, logging, discovery.
- Aspire AppHost injects connection strings; apps read config only (no Aspire client in Domain/Application).

## Anti-patterns

- Domain → Infrastructure reference.
- EF entities used as domain aggregates without explicit mapping.
- Business rules in controllers or `DbContext.SaveChanges` overrides.
- Shared mutable static state across requests.
- Cross-store transactions (Postgres + Redis); write Redis **after** commit.

## Reference

Layer diagram and transactional boundaries: [architecture-reference.md](architecture-reference.md)
