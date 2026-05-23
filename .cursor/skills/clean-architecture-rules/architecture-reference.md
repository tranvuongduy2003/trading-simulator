# Architecture Reference

## High-level processes

- **Api:** REST + SignalR; enqueues orders to Incoming Order Channel after persist.
- **MatchingEngine:** single-threaded match loop; rebuilds book from DB on startup.

## Repository ports (Application)

- `IUserRepository`
- `IPortfolioRepository`
- `IOrderRepository`
- `ITradeRepository`
- `IUnitOfWork`

## DB schema organization

- Schema: `trading` (not `public` for app tables).
- Append-only: `trades`, `portfolio_resets`.
- Denormalized on `trades`: `buyer_user_id`, `seller_user_id` for history queries.

## Transaction scopes (Application orchestrates)

| Operation | Single transaction includes |
|-----------|----------------------------|
| Place order | Reserve wallet/holding + insert Pending order |
| Match step | Update orders + insert trade + both wallets + both holdings + candlestick upsert |
| Cancel | Terminal order + release reservation |
| Portfolio reset | Cancel opens + release reservations + reset wallet + clear holdings + `portfolio_resets` row |

Matching runs **after** place-order commit via channel.

## Error boundaries

- Domain violations → typed exceptions/results → handler maps to structured API errors (no leak).
- Unhandled → global middleware, generic client response.
- Matching engine: catch per message; reject order if possible; do not stop loop.

## Future seams (do not implement in MVP)

Outbox, message broker instead of in-process Channel, separate read store, multi-symbol sharding.
