# Trading Domain Reference

## Order state machine

```
                 Pending
        ┌──────────┼──────────┐
        ▼          ▼          ▼
  PartiallyFilled  Filled  Cancelled
        │
   Filled / Cancelled
```

Terminal: `Filled`, `Cancelled`, `Rejected`.

## Domain events

| Event | Aggregate | When |
|-------|-----------|------|
| `UserRegisteredEvent` | User | Registration completes |
| `OrderPlacedEvent` | Order | New order persisted |
| `OrderMatchedEvent` | Order | Full or partial fill |
| `OrderCancelledEvent` | Order | User/system cancel |
| `OrderRejectedEvent` | Order | Engine validation failure |
| `TradeExecutedEvent` | Trade | Match creates trade |
| `WalletDebitedEvent` | User | Buy fill settlement |
| `WalletCreditedEvent` | User | Sell fill credit |
| `HoldingUpdatedEvent` | Portfolio | Holding quantity changes |
| `PortfolioResetEvent` | Portfolio | User reset |

## Application-enforced invariants (test in domain + integration)

- Maker price on trade equals resting order price.
- Wallet/holding reservations match open buy/sell orders.
- Portfolio reset 24h cooldown (via `portfolio_resets` audit).
- Order state machine transitions only as documented.

## PRD-derived business rules (MVP)

Product IDs refer to [`docs/PRD.md`](../../docs/PRD.md). See [`docs/TRACEABILITY.md`](../../docs/TRACEABILITY.md) for full mapping.

| PRD | Rule |
|-----|------|
| FR-1.3 | Initial virtual cash USD 100,000 (`Trading:InitialVirtualCash`) |
| FR-1.4 | Portfolio reset ≤ once per 24h; cancel open orders, release reserves, restore cash/holdings, clear trade history |
| FR-3.1–3.2 | Limit and market orders; whole shares; market unfilled remainder cancelled (IOC) |
| FR-3.3 | Reserve cash (buys) and shares (sells) at placement |
| FR-4.1–4.3 | Price-time priority; partial fills; execution at maker (resting) price |
| §3.3 | Single symbol `AAPL` only |

- User owns only their orders, wallet, portfolio.
