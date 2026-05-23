---
name: cqrs-handler-pattern
description: Defines CQRS command/query handlers, MediatR pipeline behaviors, validators, read models, and concurrency retry for TradingSimulator.Application. Use when adding commands, queries, handlers, FluentValidation validators, or pipeline behaviors.
---

# CQRS Handler Pattern

Sources: `docs/TECHNICAL.md` §6; `docs/DATABASE.md` §9–10.

## Pipeline

```
Request → Validation → Logging → Handler → Response
              ↑ fail fast (no handler)
Commands also: Unit of Work behavior wraps handler in transaction
```

Register behaviors in order: **Validation** before **Logging** before **UnitOfWork** (UoW innermost around handler).

## Commands (write)

| Command | Trigger | Primary aggregates |
|---------|---------|-------------------|
| `RegisterUserCommand` | POST /users | User |
| `LoginUserCommand` | POST /auth/login | Session side effects |
| `PlaceOrderCommand` | POST /orders | Wallet or Portfolio + Order; then channel enqueue |
| `CancelOrderCommand` | DELETE /orders/{id} | Order + release reservation |
| `ResetPortfolioCommand` | POST /portfolio/reset | User, Portfolio, Orders |
| `ProcessMatchCommand` | MatchingEngine internal | Order, Trade, Users, Portfolios |

Commands return result id or typed result wrapper — not domain entities.

## Queries (read)

Never mutate state. Return DTOs from **Contracts**:

`GetMyWallet`, `GetMyPortfolio`, `GetMyOpenOrders`, `GetMyOrderHistory`, `GetMyTradeHistory`, `GetOrderBookSnapshot`, `GetRecentTrades`, `GetCandlesticks`.

Handlers:

- Project via Infrastructure read stores / EF queries with **NoTracking**.
- Prefer Redis projections when available; fallback to PostgreSQL.
- Enforce resource ownership (current user only).

## Handler structure

```csharp
// Command handler sketch
public sealed class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, PlaceOrderResult>
{
    public async Task<PlaceOrderResult> Handle(PlaceOrderCommand request, CancellationToken ct)
    {
        // 1. Load aggregates via repositories
        // 2. Domain methods (reserve, create order)
        // 3. UoW commits in pipeline behavior OR explicit commit per team convention
        // 4. Side effects after success: domain events, channel write
    }
}
```

- Handlers depend on **interfaces** only.
- Map domain exceptions to `Result` / application errors — no stack traces to clients.
- Dispatch domain events **after** persistence succeeds.

## Validators

- Every **command** has a FluentValidation (or equivalent) validator.
- Validation runs in pipeline **before** handler; failures → structured response, handler not invoked.
- Boundary (Api) does shape validation; business validation in Application validators + Domain.

## Unit of Work behavior

- Applies to **commands** only (not queries).
- Opens transaction, calls handler, commits on success, rolls back on failure.
- `DbUpdateConcurrencyException` → bounded retry with jitter at behavior or handler boundary; then structured failure.

## Read vs write models

| Side | Model |
|------|--------|
| Write | Domain aggregates via repositories |
| Read | Contracts DTOs, denormalized SQL/Redis, no aggregate reconstruction required |

## Channel integration (commands)

- `PlaceOrderCommand` / `CancelOrderCommand`: enqueue **after** successful DB commit.
- `ProcessMatchCommand`: invoked from MatchingEngine consumer, not from Api HTTP.

## Authorization

- All commands/queries except register/login require authenticated user.
- Handlers verify `UserId` owns targeted orders, wallet, portfolio.

## Anti-patterns

- Queries that call `SaveChanges`.
- Handlers injecting `DbContext` directly (use repositories).
- Skipping validator for new commands.
- Returning domain entities from API-facing handlers.
- Enqueueing channel before commit.

## Reference

Full command/query list and retry policy: [cqrs-reference.md](cqrs-reference.md)
