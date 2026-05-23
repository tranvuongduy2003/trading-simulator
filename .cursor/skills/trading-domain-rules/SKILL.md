---
name: trading-domain-rules
description: Enforces Trading bounded-context DDD rules for aggregates, value objects, domain events, and invariants. Use when implementing or changing code in TradingSimulator.Domain, domain unit tests, or any pure domain logic (entities, value objects, domain services, domain exceptions).
---

# Trading Domain Rules

Source of truth: `docs/TECHNICAL.md` §5, `docs/DATABASE.md` §4–7. PRD business semantics are reflected in those sections.

## Layer constraints

- **Zero external dependencies** — no EF Core, ASP.NET Core, MediatR, Redis, or channels.
- **Pure C#** — business rules live here, not in handlers or DbContext.
- Single bounded context: **Trading**.

## Project layout (conventional)

```
Domain/
├── Users/           User, Wallet
├── Portfolios/      Portfolio, Holding
├── Orders/          Order, enums, state machine
├── Trades/          Trade
├── Common/          Money, Price, Quantity, Symbol, typed IDs
├── Events/          domain events
├── Services/        OrderValidationService, MatchingService
└── Exceptions/
```

## Aggregates (enforce in code + tests)

### User (`UserId`)

- Owns **Wallet** (`total`, `reserved`; available = total − reserved).
- **Invariants:** available ≥ 0; reserved ≤ total; unique username/email at creation (repository check in app layer).
- **Behaviors:** reserve/release funds; settle on buy fill (decrement reserved + total); credit on sell fill; reset wallet.

### Portfolio (`PortfolioId`, 1:1 `UserId`)

- Owns **Holding** per symbol (`total_quantity`, `reserved_quantity`, `average_price`).
- **Invariants:** available quantity ≥ 0; reserved ≤ total; weighted-average price on buy fills.
- **Behaviors:** reserve/release quantity; add on buy fill; reduce on sell fill; reset.

### Order (`OrderId`)

- **Invariants:** `filled ≤ original`; limit has price, market has no price; terminal states immutable.
- **Status:** `Pending` → `PartiallyFilled` | `Filled` | `Cancelled` | `Rejected` (terminal: Filled, Cancelled, Rejected).
- **Behaviors:** `ApplyFill`, `Cancel` (Pending/PartiallyFilled only), `Reject` (Pending only).

### Trade (`TradeId`)

- **Immutable** after creation; distinct buy/sell order IDs; execution price = **maker** (resting) order price.

## Value objects

| Type | Rules |
|------|--------|
| Typed IDs (`UserId`, `OrderId`, …) | Non-empty GUID |
| `Username` | Length + allowed chars |
| `EmailAddress` | Valid email |
| `Symbol` | Uppercase, 1–5 chars (DB allows 8 for FK) |
| `Money` | Non-negative balances; 4 decimal places |
| `Price` | Positive; ≤ 4 decimals |
| `Quantity` | Positive integer (whole shares) |

Prefer records/factory methods that validate on construction; invalid input throws typed domain exceptions.

## Domain events (raise from aggregates; dispatch in Application after save)

`UserRegistered`, `OrderPlaced`, `OrderMatched`, `OrderCancelled`, `OrderRejected`, `TradeExecuted`, `WalletDebited`, `WalletCredited`, `HoldingUpdated`, `PortfolioReset`.

## Domain services

| Service | Responsibility | Must NOT |
|---------|----------------|----------|
| `OrderValidationService` | Price/type/quantity consistency | Check wallet or holdings |
| `MatchingService` | Price-time-priority on in-memory book; returns trades + order updates | Persist or touch IO |

## Transactional boundaries (domain perspective)

- **One aggregate = consistency unit** for a single transactional command when possible.
- Cross-aggregate effects (matching touches Order, Trade, 2× User, 2× Portfolio) are orchestrated in Application with one Unit of Work; domain stays ignorant of persistence.

## Persistence alignment (do not reference DB; mirror rules)

Enum storage (`SMALLINT`) — keep domain enums aligned:

| Domain | DB |
|--------|-----|
| Buy / Sell | 0 / 1 |
| Limit / Market | 0 / 1 |
| Pending … Rejected | 0–4 |

DB also enforces: wallet/holding reserve ≤ total; order filled ≤ original; limit price NOT NULL; terminal `terminated_at` set. State-machine transitions and maker price remain **application/domain tests**, not only DB checks.

## Anti-patterns

- Framework attributes or `DbContext` in Domain.
- Fund/holding checks inside `OrderValidationService`.
- IO, channels, or DTOs in Domain.
- Mutating `Trade` after creation.
- Allowing transitions from terminal order states.

## Tests (when adding domain code)

Prioritize: order state machine (valid + invalid), wallet reserve/release/settle, portfolio weighted-average updates. Skip trivial value-object-only tests.

## Reference

Detailed tables and state diagram: [domain-reference.md](domain-reference.md)
