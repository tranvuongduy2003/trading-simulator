# PRD Traceability (MVP v1.0)

Maps product requirements in [`PRD.md`](PRD.md) to engineering artifacts. Use this when writing specs, plans, or tests — cite **user story IDs** (`US-xx`), **functional requirement IDs** (`FR-x.y`), and technical sections (`Tech §N`, `DB §N`).

**Authority:** Product behavior → `PRD.md`. Implementation design → `TECHNICAL.md`. Storage → `DATABASE.md`.

---

## Document roles

| Document | Contents |
|----------|----------|
| [`PRD.md`](PRD.md) | Personas, user stories, functional/non-functional requirements, UI layout, acceptance criteria |
| [`TECHNICAL.md`](TECHNICAL.md) | Architecture, domain model, CQRS, channels, API/SignalR, configuration |
| [`DATABASE.md`](DATABASE.md) | Tables, constraints, indexes, Redis keys |
| This file | Cross-reference index only — no new requirements |

---

## MVP acceptance (PRD §10.1)

A user can register, log in, place limit and market orders on both sides, see matches in real time, view portfolio, and cancel open orders. All **Must** user stories in PRD §5 are in scope.

---

## User stories → engineering

### Account (PRD §5.1, §6.1)

| Story | Priority | Product refs | Technical | Storage |
|-------|----------|--------------|-----------|---------|
| US-01 Register | Must | FR-1.1 | Tech §8.1 Auth, §6 commands | DB §4 `users` |
| US-02 Login | Must | FR-1.2 | Tech §8.1, §15 Security | DB §4 `user_sessions` |
| US-03 Cash balance | Must | FR-1.3, FR-6.2 | Tech §5.2.1 Wallet | DB §4 `wallets` |
| US-04 Portfolio reset | Should | FR-1.4 | Tech §6 `ResetPortfolioCommand` | DB §4 `portfolio_resets` |

### Market data (PRD §5.2, §6.2)

| Story | Priority | Product refs | Technical | Storage |
|-------|----------|--------------|-----------|---------|
| US-05 Best bid/ask | Must | FR-2.1, FR-2.5 | Tech §9 SignalR, §8 market queries | DB §12 Redis book |
| US-06 Order book depth | Must | FR-2.1 | Tech §8.2 engine, §9 | DB §12 |
| US-07 Trade tape | Must | FR-2.3 | Tech §9 | DB §12 tape |
| US-08 Candlestick chart | Should | FR-2.4 | Tech §8 market `GetCandlesticks` | DB §12 candles |
| US-09 Real-time updates | Must | FR-2.*, NFR §7.1 | Tech §9 SignalR | — |

### Orders (PRD §5.3–5.4, §6.3–6.5)

| Story | Priority | Product refs | Technical | Storage |
|-------|----------|--------------|-----------|---------|
| US-10–11 Limit buy/sell | Must | FR-3.1, FR-4.* | Tech §5.2.3, §7, §8 | DB §4 `orders` |
| US-12–13 Market buy/sell | Must | FR-3.2 (IOC remainder) | Tech §5.2.3, `MatchingService` | DB §4 `orders` |
| US-14 Confirmation | Should | FR-3.4 | Frontend only | — |
| US-15 Validation / reserves | Must | FR-3.3 | Tech §5.2.1–2, command validators | DB constraints |
| US-16 Open orders | Must | FR-5.1 | Tech §6 queries | DB §4 `orders` |
| US-17 Order history | Must | FR-5.2 | Tech §6 queries (page size 25) | DB indexes §7 |
| US-18 Cancel | Must | FR-5.3 | Tech §7 channel cancel | DB §4 `orders` |
| US-19 Fill notifications | Must | FR-5.4 | Tech §9 user group | — |

### Portfolio (PRD §5.5, §6.6)

| Story | Priority | Product refs | Technical | Storage |
|-------|----------|--------------|-----------|---------|
| US-20 Holdings | Must | FR-6.1 | Tech §5.2.2 | DB §4 `holdings` |
| US-21 Unrealized P&L | Must | FR-6.1 | Queries + last price | — |
| US-22 Trade history | Must | FR-6.4 | Tech §6 `GetMyTradeHistory` | DB §4 `trades` |
| US-23 Total portfolio value | Should | FR-6.3 | Query composition | — |

---

## Functional requirements → domain rules

| PRD | Rule (summary) | Enforced in |
|-----|----------------|-------------|
| FR-1.3 | Initial cash USD 100,000 | `Trading:InitialVirtualCash` (Tech §13) |
| FR-1.4 | Reset at most once per 24h; full reset semantics | `PortfolioResetCooldownMinutes` = 1440; `ResetPortfolioCommand` |
| FR-3.2 | Market order unfilled remainder cancelled (IOC) | `MatchingService` (Tech §5, matching skill) |
| FR-3.3 | Reserve cash/shares at placement | User + Portfolio aggregates |
| FR-4.1 | Price-time priority | `MatchingService` |
| FR-4.2 | Partial fills; limit remainder rests, market remainder cancelled | Order aggregate + matcher |
| FR-4.3 | Trade at resting (maker) price | Trade aggregate invariant |
| FR-7.3 | Non-negative balances; conservation of cash | DB checks + domain |

---

## UI layout (PRD §8.1)

| PRD zone | Implementation guidance |
|----------|-------------------------|
| Top bar | Logo, AAPL, last price, daily change, user menu — `design-system.mdc`, `web/` trading route |
| Left | Order book depth | `features/trading` |
| Center | Candlestick + timeframe (1m, 5m, 15m, 1h) | `features/trading` + lightweight-charts |
| Right | Limit / market order form | `features/orders` |
| Bottom tabs | Open orders, order history, trade history, holdings | `features/orders`, `features/portfolio` |

Desktop-first (≥1280px); tablet stack acceptable (≥768px). Mobile out of scope (PRD §8.3).

---

## Configuration defaults (PRD-aligned)

| Setting | PRD source | Default in repo |
|---------|------------|-----------------|
| `Trading:InitialVirtualCash` | FR-1.3 | `100000` |
| `Trading:PortfolioResetCooldownMinutes` | FR-1.4 (24h) | `1440` |
| Order book depth levels | FR-2.1 (default 10) | TBD in read API / Redis projection |
| Trade tape size | FR-2.3 (default 50) | TBD in read API / Redis |
| Order history page size | FR-5.2 | `25` (query handler default when implemented) |
| Symbol | §3.3 Non-goals | `AAPL` only |

---

## Out of scope (PRD §3.3, §10.2)

Do not implement without explicit approval: multi-symbol, real money, stop/trailing/iceberg orders, native mobile, social/leaderboards, derivatives, news feeds, message broker, transactional outbox (see `core.mdc`).

---

*Last synced with PRD v1.0 — May 22, 2026.*
