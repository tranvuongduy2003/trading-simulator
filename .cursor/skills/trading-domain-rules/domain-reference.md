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

- Virtual cash wallet; initial balance from configuration.
- Whole-share quantities only.
- Limit and market orders; single symbol (`AAPL`) for MVP.
- User owns only their orders, wallet, portfolio.
- Portfolio reset cancels open orders, releases reservations, restores initial wallet, clears holdings; cooldown enforced.
