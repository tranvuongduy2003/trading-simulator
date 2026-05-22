# Database Design Document

**Product Name:** Real-time Stock Trading Simulator
**Document Version:** 1.0
**Status:** Draft
**Last Updated:** May 22, 2026
**Document Owner:** Technical Architect / Database Administrator
**Companion Documents:** PRD.md (v1.0), TECHNICAL.md (v1.0)

---

## 1. Document Control

| Field | Value |
|-------|-------|
| Document Type | Database Design Document |
| Scope | MVP (v1.0) |
| Database Engine | PostgreSQL 17 |
| Auxiliary Store | Redis 7 |
| Audience | Engineering team |

### 1.1 Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-05-22 | Technical Architect / DBA | Initial version aligned with PRD v1.0 and TECHNICAL v1.0 |

### 1.2 Relationship to Other Documents

This document is a direct derivative of the domain model defined in `TECHNICAL.md` Section 5. Every persistent table in this design maps to one or more aggregates declared there. Where this document and `TECHNICAL.md` appear to diverge, `TECHNICAL.md` is the source of truth for domain semantics; this document is the source of truth for storage representation.

---

## 2. Design Principles

The database design follows these principles, in order of priority when conflicts arise:

1. **Aggregate-aligned storage.** Each aggregate root corresponds to one primary table. Owned entities are stored in their own tables but are accessed and modified only through their aggregate root.
2. **Single source of truth.** PostgreSQL holds authoritative state for all aggregates. Redis holds only derived data (snapshots, projections, caches) that can be rebuilt from PostgreSQL at any time.
3. **Strong constraints at the database layer.** Invariants that can be expressed declaratively (non-null, non-negative, unique, foreign key, enum range) are enforced by the database, not only by the application.
4. **Optimistic concurrency by default.** Aggregates that may be modified concurrently carry a row version column. Pessimistic locking is used only where measurements show it is necessary.
5. **Append-only where possible.** Trades and domain events are never updated or deleted. Append-only tables simplify reasoning and enable straightforward historical queries.
6. **Index for the query.** Indexes are designed against the read patterns identified in the PRD and the query handlers identified in `TECHNICAL.md`. Indexes are added with intent, not defensively.
7. **Time in UTC.** All timestamps are stored as `TIMESTAMPTZ` and represent UTC instants. Display-time conversion is the responsibility of the client.
8. **Monetary precision.** All monetary values use `NUMERIC(18, 4)` to avoid floating-point error. Quantities use `BIGINT` because the PRD specifies whole-share trading.

---

## 3. Logical Schema Overview

### 3.1 Entity-Relationship Summary

```
                ┌──────────────┐         1:1        ┌──────────────┐
                │    users     │ ─────────────────▶ │   wallets    │
                └──────┬───────┘                    └──────────────┘
                       │ 1:1
                       ▼
                ┌──────────────┐        1:N         ┌──────────────┐
                │  portfolios  │ ─────────────────▶ │   holdings   │
                └──────────────┘                    └──────────────┘

                ┌──────────────┐
                │   symbols    │
                └──────┬───────┘
                       │
        ┌──────────────┼──────────────┐
        │              │              │
        ▼              ▼              ▼
  ┌──────────┐   ┌──────────┐   ┌──────────────┐
  │  orders  │   │  trades  │   │  candlesticks │
  └────┬─────┘   └────┬─────┘   └──────────────┘
       │              │
       │              │
       └──────────────┘
       Trades reference two orders (buy and sell)

                ┌────────────────────┐
                │  user_sessions     │  (auth)
                └────────────────────┘

                ┌────────────────────┐
                │ portfolio_resets   │  (audit / cooldown)
                └────────────────────┘
```

### 3.2 Table Inventory

| Table | Aggregate / Purpose | Mutability |
|-------|---------------------|------------|
| `users` | User aggregate root | Mutable |
| `wallets` | Wallet entity (owned by User) | Mutable |
| `portfolios` | Portfolio aggregate root | Mutable |
| `holdings` | Holding entity (owned by Portfolio) | Mutable |
| `symbols` | Reference data for tradable instruments | Mostly static |
| `orders` | Order aggregate root | Mutable until terminal state |
| `trades` | Trade aggregate root | Append-only |
| `candlesticks` | Derived OHLCV bars | Append-only / upsert in current bar |
| `user_sessions` | Authentication sessions | Mutable |
| `portfolio_resets` | Audit log for reset feature, enforces cooldown | Append-only |

### 3.3 Schema Organization

All application tables live in a single schema named `trading`. The `public` schema is reserved for PostgreSQL extensions only. EF Core migration history lives in its own table within the `trading` schema.

---

## 4. Table Specifications

For each table, this section describes columns, types, nullability, defaults, and the meaning of each field. Indexes, constraints, and relationships are specified separately in Sections 5–7.

### 4.1 `users`

Represents the User aggregate root.

| Column | Type | Nullable | Default | Description |
|--------|------|----------|---------|-------------|
| `id` | `UUID` | No | — | Primary key. Strongly-typed `UserId`. |
| `username` | `VARCHAR(32)` | No | — | Unique display name. |
| `email` | `VARCHAR(254)` | No | — | Unique email address. |
| `password_hash` | `VARCHAR(255)` | No | — | Hashed password including algorithm identifier and salt. |
| `created_at` | `TIMESTAMPTZ` | No | `now()` | Account creation timestamp. |
| `updated_at` | `TIMESTAMPTZ` | No | `now()` | Last modification timestamp. |
| `row_version` | `BIGINT` | No | `1` | Optimistic concurrency token. |

### 4.2 `wallets`

Represents the Wallet entity owned by a User. One wallet per user.

| Column | Type | Nullable | Default | Description |
|--------|------|----------|---------|-------------|
| `user_id` | `UUID` | No | — | Primary key and foreign key to `users.id`. |
| `total_balance` | `NUMERIC(18,4)` | No | — | Total cash balance. |
| `reserved_balance` | `NUMERIC(18,4)` | No | `0` | Cash currently reserved by open buy orders. |
| `currency` | `CHAR(3)` | No | `'USD'` | ISO 4217 currency code. Constant for MVP. |
| `updated_at` | `TIMESTAMPTZ` | No | `now()` | Last modification timestamp. |
| `row_version` | `BIGINT` | No | `1` | Optimistic concurrency token. |

Available balance is computed at read time as `total_balance - reserved_balance` and is never stored.

### 4.3 `portfolios`

Represents the Portfolio aggregate root. One portfolio per user.

| Column | Type | Nullable | Default | Description |
|--------|------|----------|---------|-------------|
| `id` | `UUID` | No | — | Primary key. Strongly-typed `PortfolioId`. |
| `user_id` | `UUID` | No | — | Foreign key to `users.id`. Unique. |
| `created_at` | `TIMESTAMPTZ` | No | `now()` | Creation timestamp. |
| `updated_at` | `TIMESTAMPTZ` | No | `now()` | Last modification timestamp. |
| `row_version` | `BIGINT` | No | `1` | Optimistic concurrency token. |

### 4.4 `holdings`

Represents a Holding entity owned by a Portfolio. One row per (portfolio, symbol) pair.

| Column | Type | Nullable | Default | Description |
|--------|------|----------|---------|-------------|
| `portfolio_id` | `UUID` | No | — | Foreign key to `portfolios.id`. |
| `symbol` | `VARCHAR(8)` | No | — | Foreign key to `symbols.code`. |
| `total_quantity` | `BIGINT` | No | `0` | Total shares owned. |
| `reserved_quantity` | `BIGINT` | No | `0` | Shares currently reserved by open sell orders. |
| `average_price` | `NUMERIC(18,4)` | No | `0` | Weighted average purchase price. |
| `updated_at` | `TIMESTAMPTZ` | No | `now()` | Last modification timestamp. |

The composite key is `(portfolio_id, symbol)`. Available quantity is computed at read time as `total_quantity - reserved_quantity`.

### 4.5 `symbols`

Reference data for tradable instruments. For MVP this table contains a single row (`AAPL`).

| Column | Type | Nullable | Default | Description |
|--------|------|----------|---------|-------------|
| `code` | `VARCHAR(8)` | No | — | Primary key. Symbol code (e.g., `AAPL`). |
| `name` | `VARCHAR(128)` | No | — | Full instrument name. |
| `is_active` | `BOOLEAN` | No | `TRUE` | Whether the symbol is currently tradable. |
| `tick_size` | `NUMERIC(18,4)` | No | `0.01` | Minimum price increment. |
| `created_at` | `TIMESTAMPTZ` | No | `now()` | Record creation timestamp. |

### 4.6 `orders`

Represents the Order aggregate root.

| Column | Type | Nullable | Default | Description |
|--------|------|----------|---------|-------------|
| `id` | `UUID` | No | — | Primary key. Strongly-typed `OrderId`. |
| `user_id` | `UUID` | No | — | Foreign key to `users.id`. |
| `symbol` | `VARCHAR(8)` | No | — | Foreign key to `symbols.code`. |
| `side` | `SMALLINT` | No | — | `0` = Buy, `1` = Sell. |
| `type` | `SMALLINT` | No | — | `0` = Limit, `1` = Market. |
| `price` | `NUMERIC(18,4)` | Yes | — | Limit price. NULL for market orders. |
| `original_quantity` | `BIGINT` | No | — | Quantity requested at placement. |
| `filled_quantity` | `BIGINT` | No | `0` | Quantity matched so far. |
| `status` | `SMALLINT` | No | `0` | `0` = Pending, `1` = PartiallyFilled, `2` = Filled, `3` = Cancelled, `4` = Rejected. |
| `rejection_reason` | `VARCHAR(255)` | Yes | — | Populated only when `status = Rejected`. |
| `is_simulated` | `BOOLEAN` | No | `FALSE` | Marks orders generated by the simulated liquidity component. |
| `created_at` | `TIMESTAMPTZ` | No | `now()` | Placement timestamp. Defines time priority for matching. |
| `updated_at` | `TIMESTAMPTZ` | No | `now()` | Last modification timestamp. |
| `terminated_at` | `TIMESTAMPTZ` | Yes | — | Timestamp when the order reached a terminal state. |
| `row_version` | `BIGINT` | No | `1` | Optimistic concurrency token. |

### 4.7 `trades`

Represents the Trade aggregate root. Append-only.

| Column | Type | Nullable | Default | Description |
|--------|------|----------|---------|-------------|
| `id` | `BIGSERIAL` | No | — | Primary key. Monotonically increasing. |
| `external_id` | `UUID` | No | `gen_random_uuid()` | Stable public identifier. |
| `symbol` | `VARCHAR(8)` | No | — | Foreign key to `symbols.code`. |
| `buy_order_id` | `UUID` | No | — | Foreign key to `orders.id`. |
| `sell_order_id` | `UUID` | No | — | Foreign key to `orders.id`. |
| `buyer_user_id` | `UUID` | No | — | Foreign key to `users.id`. Denormalized for query performance. |
| `seller_user_id` | `UUID` | No | — | Foreign key to `users.id`. Denormalized for query performance. |
| `price` | `NUMERIC(18,4)` | No | — | Execution price (price of the resting order). |
| `quantity` | `BIGINT` | No | — | Quantity matched in this trade. |
| `executed_at` | `TIMESTAMPTZ` | No | `now()` | Execution timestamp. |

The buyer and seller user IDs are denormalized into this table to support user trade-history queries without joining through orders.

### 4.8 `candlesticks`

Derived OHLCV aggregates per symbol per time bucket. Maintained by the matching engine in response to trades.

| Column | Type | Nullable | Default | Description |
|--------|------|----------|---------|-------------|
| `symbol` | `VARCHAR(8)` | No | — | Foreign key to `symbols.code`. |
| `interval` | `SMALLINT` | No | — | `0` = 1m, `1` = 5m, `2` = 15m, `3` = 1h. |
| `bucket_start` | `TIMESTAMPTZ` | No | — | Start of the time bucket. |
| `open_price` | `NUMERIC(18,4)` | No | — | First trade price in the bucket. |
| `high_price` | `NUMERIC(18,4)` | No | — | Highest trade price in the bucket. |
| `low_price` | `NUMERIC(18,4)` | No | — | Lowest trade price in the bucket. |
| `close_price` | `NUMERIC(18,4)` | No | — | Last trade price in the bucket. |
| `volume` | `BIGINT` | No | `0` | Total quantity traded in the bucket. |
| `trade_count` | `INTEGER` | No | `0` | Number of trades in the bucket. |
| `updated_at` | `TIMESTAMPTZ` | No | `now()` | Last update timestamp. |

The composite key is `(symbol, interval, bucket_start)`. Candlestick rows are derivable from `trades` and could be rebuilt by replay if lost.

### 4.9 `user_sessions`

Authentication sessions backed by the database. Sessions may also be cached in Redis; PostgreSQL remains authoritative.

| Column | Type | Nullable | Default | Description |
|--------|------|----------|---------|-------------|
| `id` | `UUID` | No | — | Primary key. Session identifier. |
| `user_id` | `UUID` | No | — | Foreign key to `users.id`. |
| `created_at` | `TIMESTAMPTZ` | No | `now()` | Session creation timestamp. |
| `expires_at` | `TIMESTAMPTZ` | No | — | Session expiration timestamp. |
| `last_seen_at` | `TIMESTAMPTZ` | No | `now()` | Last activity timestamp. |
| `revoked_at` | `TIMESTAMPTZ` | Yes | — | Set when the session is explicitly invalidated. |

### 4.10 `portfolio_resets`

Audit log for the portfolio reset feature. Used both for history and for enforcing the 24-hour cooldown described in the PRD.

| Column | Type | Nullable | Default | Description |
|--------|------|----------|---------|-------------|
| `id` | `BIGSERIAL` | No | — | Primary key. |
| `user_id` | `UUID` | No | — | Foreign key to `users.id`. |
| `reset_at` | `TIMESTAMPTZ` | No | `now()` | Time of reset. |
| `reason` | `VARCHAR(64)` | Yes | — | Optional reason classification (e.g., `user_initiated`). |

---

## 5. Relationships

### 5.1 Foreign Key Summary

| From | To | On Delete | Notes |
|------|----|-----------|----|
| `wallets.user_id` | `users.id` | `RESTRICT` | A user cannot be deleted while a wallet exists. |
| `portfolios.user_id` | `users.id` | `RESTRICT` | Same rationale. |
| `holdings.portfolio_id` | `portfolios.id` | `CASCADE` | Holdings cannot outlive their portfolio. |
| `holdings.symbol` | `symbols.code` | `RESTRICT` | Symbols are never deleted while holdings exist. |
| `orders.user_id` | `users.id` | `RESTRICT` | Preserve order history. |
| `orders.symbol` | `symbols.code` | `RESTRICT` | Preserve referential integrity. |
| `trades.buy_order_id` | `orders.id` | `RESTRICT` | Trades are immutable historical record. |
| `trades.sell_order_id` | `orders.id` | `RESTRICT` | Same. |
| `trades.buyer_user_id` | `users.id` | `RESTRICT` | Same. |
| `trades.seller_user_id` | `users.id` | `RESTRICT` | Same. |
| `trades.symbol` | `symbols.code` | `RESTRICT` | Same. |
| `candlesticks.symbol` | `symbols.code` | `RESTRICT` | Reference data integrity. |
| `user_sessions.user_id` | `users.id` | `CASCADE` | Sessions disappear with the user. |
| `portfolio_resets.user_id` | `users.id` | `RESTRICT` | Preserve audit trail. |

### 5.2 Aggregate Ownership

Although foreign keys exist between aggregates, write access at the application level is restricted to aggregate roots only. This is enforced by repository design, not by the database. The database enforces only referential integrity.

### 5.3 Cardinality Notes

- A user has exactly one wallet and exactly one portfolio.
- A portfolio has at most one holding per symbol.
- A trade references exactly two orders, of opposite sides, both for the same symbol.
- Each candlestick row is uniquely identified by `(symbol, interval, bucket_start)`.

---

## 6. Indexing Strategy

Indexes are designed against the query handlers in `TECHNICAL.md` Section 6.3 and the read patterns expected from the PRD.

### 6.1 `users`

| Index | Columns | Type | Purpose |
|-------|---------|------|---------|
| `pk_users` | `id` | B-tree (PK) | Primary key. |
| `ux_users_username` | `username` | Unique B-tree | Login lookup, uniqueness enforcement. |
| `ux_users_email` | `email` | Unique B-tree | Email uniqueness, password recovery (future). |

### 6.2 `wallets`

| Index | Columns | Type | Purpose |
|-------|---------|------|---------|
| `pk_wallets` | `user_id` | B-tree (PK) | Primary key, single-row access by user. |

No other indexes; wallets are always accessed by user.

### 6.3 `portfolios`

| Index | Columns | Type | Purpose |
|-------|---------|------|---------|
| `pk_portfolios` | `id` | B-tree (PK) | Primary key. |
| `ux_portfolios_user` | `user_id` | Unique B-tree | One portfolio per user. |

### 6.4 `holdings`

| Index | Columns | Type | Purpose |
|-------|---------|------|---------|
| `pk_holdings` | `portfolio_id, symbol` | B-tree (PK) | Composite primary key. |

The primary key already serves the only access pattern (look up a specific holding by portfolio and symbol).

### 6.5 `orders`

This is the most query-intensive table; indexes are chosen carefully.

| Index | Columns | Type | Purpose |
|-------|---------|------|---------|
| `pk_orders` | `id` | B-tree (PK) | Primary key. |
| `ix_orders_user_status` | `user_id, status, created_at DESC` | B-tree | User-facing queries: open orders, order history. |
| `ix_orders_active_book` | `symbol, side, price, created_at` | Partial B-tree | Order book reconstruction. Partial: `WHERE status IN (0, 1)`. |
| `ix_orders_terminated` | `terminated_at DESC` | Partial B-tree | History queries by time. Partial: `WHERE terminated_at IS NOT NULL`. |

Rationale:
- `ix_orders_user_status` covers the user's open-orders view and is used for paginated history with status filters.
- `ix_orders_active_book` is a partial index that contains only orders eligible for matching. It is small (bounded by open-order count) and supports efficient order book rebuilding on engine startup.

### 6.6 `trades`

| Index | Columns | Type | Purpose |
|-------|---------|------|---------|
| `pk_trades` | `id` | B-tree (PK) | Primary key. |
| `ux_trades_external_id` | `external_id` | Unique B-tree | Public lookup by external ID. |
| `ix_trades_symbol_time` | `symbol, executed_at DESC` | B-tree | Trade tape (recent trades per symbol). |
| `ix_trades_buyer_time` | `buyer_user_id, executed_at DESC` | B-tree | User trade history (buy side). |
| `ix_trades_seller_time` | `seller_user_id, executed_at DESC` | B-tree | User trade history (sell side). |
| `ix_trades_buy_order` | `buy_order_id` | B-tree | Find all fills for an order. |
| `ix_trades_sell_order` | `sell_order_id` | B-tree | Find all fills for an order. |

### 6.7 `candlesticks`

| Index | Columns | Type | Purpose |
|-------|---------|------|---------|
| `pk_candlesticks` | `symbol, interval, bucket_start` | B-tree (PK) | Composite primary key; also serves chart queries by symbol/interval ordered by time. |

### 6.8 `user_sessions`

| Index | Columns | Type | Purpose |
|-------|---------|------|---------|
| `pk_user_sessions` | `id` | B-tree (PK) | Primary key. |
| `ix_user_sessions_user` | `user_id` | B-tree | List sessions for a user (logout-all flows). |
| `ix_user_sessions_expires` | `expires_at` | B-tree | Background cleanup of expired sessions. |

### 6.9 `portfolio_resets`

| Index | Columns | Type | Purpose |
|-------|---------|------|---------|
| `pk_portfolio_resets` | `id` | B-tree (PK) | Primary key. |
| `ix_portfolio_resets_user_time` | `user_id, reset_at DESC` | B-tree | Enforce 24h cooldown by looking up the most recent reset. |

### 6.10 Index Hygiene

- All indexes are created `CONCURRENTLY` in production-style environments to avoid blocking writes. For local development this is not required.
- Indexes that prove unused under realistic workloads should be removed; this is reviewed during performance work.
- No covering indexes are introduced in the MVP. They are considered only after measurement.

---

## 7. Constraints

### 7.1 Check Constraints

The following invariants are enforced at the database layer.

| Table | Constraint | Rule |
|-------|------------|------|
| `wallets` | `ck_wallets_total_non_negative` | `total_balance >= 0` |
| `wallets` | `ck_wallets_reserved_non_negative` | `reserved_balance >= 0` |
| `wallets` | `ck_wallets_reserved_le_total` | `reserved_balance <= total_balance` |
| `holdings` | `ck_holdings_total_non_negative` | `total_quantity >= 0` |
| `holdings` | `ck_holdings_reserved_non_negative` | `reserved_quantity >= 0` |
| `holdings` | `ck_holdings_reserved_le_total` | `reserved_quantity <= total_quantity` |
| `holdings` | `ck_holdings_avg_price_non_negative` | `average_price >= 0` |
| `orders` | `ck_orders_quantity_positive` | `original_quantity > 0` |
| `orders` | `ck_orders_filled_non_negative` | `filled_quantity >= 0` |
| `orders` | `ck_orders_filled_le_original` | `filled_quantity <= original_quantity` |
| `orders` | `ck_orders_side_range` | `side IN (0, 1)` |
| `orders` | `ck_orders_type_range` | `type IN (0, 1)` |
| `orders` | `ck_orders_status_range` | `status IN (0, 1, 2, 3, 4)` |
| `orders` | `ck_orders_limit_has_price` | `(type = 0 AND price IS NOT NULL AND price > 0) OR (type = 1 AND price IS NULL)` |
| `orders` | `ck_orders_terminal_has_terminated_at` | `(status IN (2, 3, 4)) = (terminated_at IS NOT NULL)` |
| `trades` | `ck_trades_quantity_positive` | `quantity > 0` |
| `trades` | `ck_trades_price_positive` | `price > 0` |
| `trades` | `ck_trades_orders_distinct` | `buy_order_id <> sell_order_id` |
| `trades` | `ck_trades_users_distinct` | `buyer_user_id <> seller_user_id` |
| `candlesticks` | `ck_candlesticks_prices_positive` | `open_price > 0 AND high_price > 0 AND low_price > 0 AND close_price > 0` |
| `candlesticks` | `ck_candlesticks_high_low` | `high_price >= low_price` |
| `candlesticks` | `ck_candlesticks_volume_non_negative` | `volume >= 0` |
| `symbols` | `ck_symbols_tick_size_positive` | `tick_size > 0` |

### 7.2 Unique Constraints

| Table | Constraint | Columns |
|-------|------------|---------|
| `users` | `ux_users_username` | `username` |
| `users` | `ux_users_email` | `email` |
| `portfolios` | `ux_portfolios_user` | `user_id` |
| `holdings` | (PK) | `(portfolio_id, symbol)` |
| `trades` | `ux_trades_external_id` | `external_id` |
| `candlesticks` | (PK) | `(symbol, interval, bucket_start)` |

### 7.3 Not Null

Every column in this design specifies its nullability explicitly. Columns documented as non-nullable are enforced as `NOT NULL` in the schema.

### 7.4 Application-Enforced Invariants

Some invariants cannot be expressed declaratively and remain the responsibility of the application:

- Order state transitions follow the state machine defined in `TECHNICAL.md` Section 5.2.3.
- Trade execution price equals the price of the resting order (maker).
- Average holding price uses weighted-average computation on buy fills.
- Portfolio reset cooldown of 24 hours is enforced by application logic against `portfolio_resets`.
- Reserved amounts on wallets and holdings reconcile exactly with open-order quantities.

These invariants are protected by tests at the domain and integration levels.

---

## 8. Data Types and Conventions

### 8.1 Identifiers

- All aggregate root identifiers are `UUID` (v7 preferred for time-ordered locality). They are generated at the application layer to keep identity ownership with the domain.
- Auxiliary append-only tables (`trades`, `portfolio_resets`) use `BIGSERIAL` for the internal primary key and a separate `UUID` external identifier where needed.

### 8.2 Monetary Values

All monetary values are stored as `NUMERIC(18, 4)`. The application uses a `Money` value object that enforces precision boundaries. Currency is implicitly USD for the MVP and stored explicitly in `wallets.currency` for forward compatibility.

### 8.3 Quantities

All share quantities are stored as `BIGINT`. Fractional shares are out of scope for the MVP.

### 8.4 Enumerations

Enumerations are stored as `SMALLINT` with check constraints on the allowed range. The mapping between numeric values and names is documented in this file and in the Domain layer. Numeric values are append-only: once assigned, a value is never reused for a different meaning.

### 8.5 Timestamps

All timestamp columns use `TIMESTAMPTZ` and store UTC. Server time is assumed to be UTC. `created_at` and `updated_at` columns are populated by the application; the database provides `DEFAULT now()` as a safety net.

### 8.6 Strings

- `VARCHAR` is used with explicit length limits derived from the domain (e.g., `Username` length matches the `Username` value object).
- All strings are stored as UTF-8 (PostgreSQL default).

### 8.7 Naming

- Table names are plural snake_case (`orders`, `user_sessions`).
- Column names are snake_case (`created_at`, `row_version`).
- Primary keys are named `pk_<table>`.
- Foreign keys are named `fk_<table>_<column>`.
- Unique indexes are named `ux_<table>_<columns>`.
- Non-unique indexes are named `ix_<table>_<columns>`.
- Check constraints are named `ck_<table>_<rule>`.

---

## 9. Concurrency Control

### 9.1 Optimistic Concurrency

Tables that may be updated concurrently carry a `row_version BIGINT` column. The application increments this value on every update and includes it in the `WHERE` clause. A zero-affected-rows result indicates a concurrency conflict, which is surfaced to the command handler as a typed exception.

This applies to `users`, `wallets`, `portfolios`, and `orders`.

### 9.2 Append-Only Tables

`trades`, `candlesticks` (effectively upsert with monotonic field updates), and `portfolio_resets` are not subject to optimistic concurrency because they are not updated in place by competing actors.

### 9.3 Pessimistic Locking

Pessimistic locks (`SELECT ... FOR UPDATE`) are used inside the matching engine for the brief critical section where a wallet or holding must be modified atomically with the trade insertion. The scope is intentionally narrow.

### 9.4 Isolation Level

The default isolation level is `READ COMMITTED`. Specific operations that require stricter guarantees (e.g., reservation against wallet balance during order placement) wrap their work in a transaction at `REPEATABLE READ` or use explicit row locks.

### 9.5 Retry Policy

Concurrency exceptions are retried with bounded attempts and small randomized backoff at the command-handler boundary. After exhausting retries, the command fails with a structured error.

---

## 10. Transactional Boundaries

### 10.1 Order Placement

A single transaction covers:

1. Reserve funds on the wallet (buy) or reserve quantity on the holding (sell).
2. Insert the order in `Pending` status.
3. Commit.

The matching is performed asynchronously after this commit succeeds.

### 10.2 Order Matching

A single transaction covers, for each match step that produces a trade:

1. Update the maker order (`filled_quantity`, possibly `status`, possibly `terminated_at`).
2. Update the taker order (same).
3. Insert the trade.
4. Update the buyer's wallet (decrement total and reserved).
5. Update the seller's wallet (increment total).
6. Update the buyer's holding (increment quantity, recompute average price).
7. Update the seller's holding (decrement total and reserved).
8. Update or insert the relevant candlestick rows.
9. Commit.

If any step fails, the entire transaction rolls back and the order remains in its prior state. The matching loop logs the failure and continues with the next message.

### 10.3 Order Cancellation

A single transaction covers:

1. Update the order to `Cancelled` with `terminated_at`.
2. Release the reserved funds or quantity on the wallet or holding.
3. Commit.

### 10.4 Portfolio Reset

A single transaction covers:

1. Cancel all open orders for the user.
2. Release all reservations.
3. Set wallet to its initial state.
4. Remove all holdings for the user.
5. Insert a row into `portfolio_resets`.
6. Commit.

---

## 11. Migration Strategy

### 11.1 Tooling

EF Core migrations manage schema changes. Migrations are checked into source control and applied automatically on service startup in development.

### 11.2 Conventions

- Each migration is small and focused on a single concern.
- Migrations are additive whenever possible. Destructive changes are split across multiple migrations to allow safe rollback during development.
- Reference data for `symbols` is seeded by a migration, not by application code.

### 11.3 Initial Schema

The initial migration creates all tables, indexes, constraints, and seeds the `symbols` table with the single MVP symbol `AAPL`.

---

## 12. Redis Usage

Redis holds derived, rebuildable data only. PostgreSQL remains the source of truth.

### 12.1 Key Patterns

| Pattern | Purpose | TTL |
|---------|---------|-----|
| `orderbook:{symbol}:snapshot` | Latest top-N levels of the order book, serialized. | None (overwritten on every change). |
| `trades:{symbol}:recent` | List of recent trades (capped). | None (capped list). |
| `candles:{symbol}:{interval}:{bucket}` | Cached in-progress candlestick. | Until bucket close + grace. |
| `session:{session_id}` | Session lookup cache. | Matches session expiration. |

### 12.2 Consistency Model

Redis projections are written after the corresponding PostgreSQL transaction commits. If a write to Redis fails, the system continues; the next event will overwrite stale state. On Redis loss, projections are rebuilt from PostgreSQL on demand.

### 12.3 No Cross-Store Transactions

The system does not attempt to make PostgreSQL and Redis writes atomic. Redis is treated as a cache, not a system of record.

---

## 13. Backup and Retention

Local-only deployment. Backup and retention policies are out of scope for the MVP and are not enforced. Developers may reset the local database at any time. The `portfolio_resets` table provides an in-app audit trail of resets initiated by users.

---

## 14. Performance Considerations

### 14.1 Expected Hot Paths

- Insert into `orders` (every order placement).
- Update of `orders` (every fill or cancellation).
- Insert into `trades` (every match).
- Update of `wallets` and `holdings` (every match).

### 14.2 Mitigations Built Into This Design

- `ix_orders_active_book` is a partial index, keeping it small and fast.
- Trades have denormalized buyer and seller user IDs to avoid joins in history queries.
- Read-heavy paths (order book snapshot, trade tape, candlesticks) are served from Redis.
- Optimistic concurrency avoids long-held row locks on the highest-contention rows.

### 14.3 Measurement

Performance tuning is driven by measurement, not speculation. Query plans (`EXPLAIN ANALYZE`) are reviewed for hot paths before and after schema changes. The MVP does not commit to specific tuning beyond what this design already includes.

---

## 15. Advanced Topics (Reference Only)

This section documents techniques that are deliberately out of scope for the MVP but represent natural evolutions of this design. They are listed here so that the current schema does not paint itself into a corner.

### 15.1 Partitioning

#### Time-based partitioning of `trades`

Once the trade volume grows beyond what a single table can comfortably index, `trades` can be partitioned by `executed_at` using PostgreSQL declarative partitioning, typically with monthly or weekly partitions. Benefits include:

- Smaller per-partition indexes.
- Fast detachment of old partitions for archival.
- More efficient time-range queries via partition pruning.

The current schema is compatible with this evolution: `executed_at` is `NOT NULL` and is the natural partition key. The primary key would change from `id` to `(executed_at, id)` to satisfy partitioning constraints.

#### Time-based partitioning of `orders`

Closed orders (terminal states) are heavier than open orders but never modified. A second-stage evolution could partition `orders` by `created_at` once the table becomes a hotspot. The partial index `ix_orders_active_book` continues to apply, but only against partitions containing active orders.

#### Time-based partitioning of `candlesticks`

Less urgent because candlesticks are inherently bounded per bucket, but the same approach applies if multi-symbol expansion (Section 15.3) is pursued.

### 15.2 Hypertables and Time-Series Stores

For multi-symbol, multi-year deployments, replacing `trades` and `candlesticks` with a time-series extension (TimescaleDB hypertables) or an external store (InfluxDB, ClickHouse) is a natural step. The trade-off is operational complexity vs query and storage efficiency for time-bucketed analytics.

### 15.3 Sharding

Sharding becomes relevant only at scales that the MVP will not reach. Two reasonable sharding axes for this domain:

- **By symbol.** Each symbol's orders, trades, and candlesticks live on a dedicated shard. Matching engines also become per-symbol. User accounts (users, wallets, portfolios) remain on a shared shard or are sharded by user ID.
- **By user.** Orders and trades are co-located with the user. This favors per-user queries at the cost of order book queries, which become cross-shard.

The current design favors a future "by symbol" approach: there is no implicit assumption that orders for different symbols share storage, and reference data is small and replicable.

### 15.4 Outbox Pattern

The MVP publishes domain events in process. To guarantee at-least-once delivery to downstream consumers (e.g., notification services, projection workers, an external message broker), a transactional outbox can be introduced.

The outbox would be a single table:

| Column | Type | Description |
|--------|------|-------------|
| `id` | `BIGSERIAL` | Primary key, dispatch ordering. |
| `aggregate_type` | `VARCHAR(64)` | Aggregate that produced the event. |
| `aggregate_id` | `UUID` | Identifier of the producing aggregate. |
| `event_type` | `VARCHAR(128)` | Event class name. |
| `payload` | `JSONB` | Serialized event payload. |
| `occurred_at` | `TIMESTAMPTZ` | Domain time of the event. |
| `processed_at` | `TIMESTAMPTZ` | Dispatcher time. NULL while pending. |

Domain event inserts share the same transaction as the aggregate write. A background dispatcher reads pending rows, publishes them, and marks them processed. Indexes on `processed_at IS NULL` and `(processed_at, id)` keep the dispatcher fast.

### 15.5 Idempotency

Two distinct idempotency concerns exist in this domain:

- **API-level idempotency.** Clients may retry `PlaceOrderCommand` due to network failure. A nullable `client_order_id` column on `orders` with a unique partial index (`WHERE client_order_id IS NOT NULL`) provides natural deduplication. The MVP does not require this, but the column can be added without restructuring.
- **Event-consumer idempotency.** Downstream consumers of domain events must tolerate duplicate delivery. A dedicated `processed_events` table per consumer, keyed by event ID, is the standard pattern.

### 15.6 Read-Side Separation (CQRS Evolution)

The current design uses a single PostgreSQL instance for both writes and reads. A future evolution could:

- Maintain materialized views (or PostgreSQL `MATERIALIZED VIEW`s with scheduled refresh) for heavy read patterns.
- Project to a dedicated read store (a separate PostgreSQL instance, a search index, or a denormalized document store).
- Drive projections from the outbox (Section 15.4) for eventual consistency.

### 15.7 Event Sourcing for Orders

Orders are a natural candidate for event sourcing: their state is a function of a small set of events (placed, matched, partially filled, cancelled, rejected). The current design persists the current state. A future evolution could store the events and derive state on demand or via snapshots. Trade-offs include richer auditability at the cost of read complexity.

### 15.8 Audit and Compliance

The MVP records lifecycle events through column timestamps and the trades append-only table. A more rigorous audit posture would introduce a per-table change-data-capture stream or an explicit audit table for sensitive operations.

### 15.9 Connection Pool Sizing

For the local-only MVP this is non-critical. At scale, connection pool sizing per service becomes a tuning parameter that interacts with the matching engine's transactional load and the read services' concurrency. PgBouncer or similar pooling tiers may be introduced.

### 15.10 Vacuum and Bloat Management

PostgreSQL's MVCC produces table and index bloat under high-update workloads. The `orders` and `wallets` tables are particularly susceptible. Standard mitigations include tuned autovacuum settings, fill-factor adjustments, and periodic `REINDEX CONCURRENTLY`. None of these are required for the MVP.

---

*End of Document*
