# Technical Design Document

**Product Name:** Real-time Stock Trading Simulator
**Document Version:** 1.0
**Status:** Draft
**Last Updated:** May 22, 2026
**Document Owner:** Technical Architect
**Companion Document:** PRD.md (v1.0)

---

## 1. Document Control

| Field | Value |
|-------|-------|
| Document Type | Technical Design Document |
| Scope | MVP (v1.0) |
| Audience | Engineering team |
| Deployment Target | Local development environment only |

### 1.1 Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-05-22 | Technical Architect | Initial version aligned with PRD v1.0 |

---

## 2. Architectural Overview

### 2.1 Architectural Goals

- Enforce a clear separation of concerns through Clean Architecture so that business rules are independent of frameworks, databases, and delivery mechanisms.
- Model the trading domain explicitly using Domain-Driven Design to keep business invariants close to the data they protect.
- Separate the write path (commands) from the read path (queries) using CQRS to allow each side to evolve independently.
- Decouple order intake from order matching using a Channel-based producer-consumer pipeline to keep API responses fast and matching deterministic.
- Run the entire system locally with a single command using .NET Aspire as the orchestration layer.

### 2.2 Architectural Style

The solution combines four complementary styles:

1. **Clean Architecture** — concentric dependency rule, with the Domain at the center.
2. **Domain-Driven Design (Tactical)** — aggregates, entities, value objects, domain events.
3. **CQRS** — distinct command and query pipelines sharing a single source of truth.
4. **Producer-Consumer Pipeline** — bounded in-memory channel between the API and the matching engine.

### 2.3 High-Level Logical View

```
                ┌──────────────────────────────────────────────────┐
                │                  Web Frontend                    │
                │            (React 19, single-page app)           │
                └──────────────────────────────────────────────────┘
                                  │            ▲
                          REST / SignalR (WebSocket)
                                  ▼            │
                ┌──────────────────────────────────────────────────┐
                │                    API Service                   │
                │   (ASP.NET Core, REST endpoints + SignalR hub)   │
                └──────────────────────────────────────────────────┘
                                  │            ▲
                          Channel (in-process)
                                  ▼            │
                ┌──────────────────────────────────────────────────┐
                │              Matching Engine Service             │
                │   (Worker Service, single-threaded matching)     │
                └──────────────────────────────────────────────────┘
                                  │            ▲
                                  ▼            │
                ┌──────────────────────────────────────────────────┐
                │           Infrastructure (DB, Cache)             │
                │         PostgreSQL, Redis, File logs             │
                └──────────────────────────────────────────────────┘
```

### 2.4 Process Model

The system consists of two long-running .NET processes:

- **API Service** — handles HTTP requests, hosts the SignalR hub, accepts orders and forwards them to the matching engine over a Channel.
- **Matching Engine Service** — a Worker Service that owns the in-memory order book, performs matching, and persists trades.

For the MVP, both processes communicate through an in-process Channel. The matching engine is therefore co-located with the API in the same logical boundary. A future evolution may split them across process boundaries using a message broker; the Channel abstraction is designed to allow that swap with minimal domain changes.

---

## 3. Clean Architecture Layering

### 3.1 Layer Definitions

The codebase is divided into four layers, each with a strict, one-directional dependency rule.

#### 3.1.1 Domain Layer (Core)

- Contains aggregates, entities, value objects, domain events, domain services, and domain exceptions.
- Has zero external dependencies — no references to EF Core, ASP.NET Core, MediatR, or any other framework.
- Pure C# only.

#### 3.1.2 Application Layer

- Contains use cases expressed as CQRS handlers (command handlers and query handlers).
- Defines interfaces (ports) for any external concerns it needs (repositories, unit of work, time provider, channel writer).
- Depends only on the Domain layer.
- No knowledge of HTTP, databases, or messaging technology.

#### 3.1.3 Infrastructure Layer

- Provides concrete implementations of the interfaces defined by the Application layer.
- Contains EF Core DbContext and configurations, repository implementations, Redis client wrappers, channel adapters, and the file logger configuration.
- Depends on Application and Domain.

#### 3.1.4 Presentation Layer

- Contains the two host projects: API Service and Matching Engine Service.
- Composition root: wires up dependencies, configures middleware, exposes endpoints.
- Depends on Application and Infrastructure.

### 3.2 Dependency Rule

Dependencies always point inward.

```
Presentation  ─►  Infrastructure  ─►  Application  ─►  Domain
                                                          ▲
                                  (Infrastructure also references Domain)
```

The Domain layer never references anything outside itself. The Application layer never references the Infrastructure layer; instead, it declares interfaces that the Infrastructure layer implements (Dependency Inversion).

---

## 4. Solution Structure

### 4.1 Repository Layout

```
trading-simulator/
├── TradingSimulator.slnx
├── README.md
├── .github/
│   └── workflows/
│       └── ci.yml
├── src/
│   ├── AppHost/
│   │   └── TradingSimulator.AppHost.csproj
│   ├── ServiceDefaults/
│   │   └── TradingSimulator.ServiceDefaults.csproj
│   ├── Api/
│   │   └── TradingSimulator.Api.csproj
│   ├── MatchingEngine/
│   │   └── TradingSimulator.MatchingEngine.csproj
│   ├── Application/
│   │   └── TradingSimulator.Application.csproj
│   ├── Domain/
│   │   └── TradingSimulator.Domain.csproj
│   ├── Infrastructure/
│   │   └── TradingSimulator.Infrastructure.csproj
│   └── Contracts/
│       └── TradingSimulator.Contracts.csproj
├── tests/
│   ├── Domain.UnitTests/
│   ├── MatchingEngine.UnitTests/
│   └── Api.IntegrationTests/
└── web/
    └── (React 19 + Vite app — not part of .slnx)
```

### 4.2 Project Responsibilities

| Project | Layer | Responsibility |
|---------|-------|----------------|
| AppHost | Orchestration | .NET Aspire host that declares and wires all resources (Postgres, Redis, services, frontend). |
| ServiceDefaults | Cross-cutting | Aspire service defaults shared by all .NET services. |
| Api | Presentation | HTTP endpoints, SignalR hub, request validation, authentication. |
| MatchingEngine | Presentation | Worker service hosting the order book and matching loop. |
| Application | Application | Command and query handlers, application-level interfaces, validators. |
| Domain | Domain | Aggregates, entities, value objects, domain events, domain rules. |
| Infrastructure | Infrastructure | EF Core, repositories, Redis adapters, channel adapters, logging configuration. |
| Contracts | Shared | DTOs, request/response models, SignalR message contracts shared between API and clients. |

### 4.3 Solution File Format

The solution uses the XML-based `.slnx` format. The `.slnx` file declares all projects under `src/` and `tests/` but does not include the `web/` frontend, which is built and run independently by Aspire as a Yarn-managed Vite app.

---

## 5. Domain Model (DDD)

The domain model is derived directly from the PRD. All business rules, invariants, and state transitions described below are enforced inside the Domain layer.

### 5.1 Bounded Context

The MVP defines a single bounded context: **Trading**. All aggregates, value objects, and domain events described in this section belong to this context.

### 5.2 Aggregates

#### 5.2.1 User Aggregate

- **Root:** `User`
- **Identity:** `UserId` (strongly-typed)
- **Entities owned:** `Wallet`
- **Value objects used:** `Username`, `EmailAddress`, `PasswordHash`, `Money`

**State:**
- Identity, username, email, password hash, account creation timestamp.
- A `Wallet` containing total cash balance and reserved cash.

**Invariants:**
- Username must be unique within the context (enforced via repository check at creation).
- Email must be a valid email format.
- The wallet's available balance (total minus reserved) must never be negative.
- Reserved balance must never exceed total balance.

**Behaviors:**
- Reserve funds for a pending buy order.
- Release reserved funds when an order is cancelled or partially cancelled.
- Settle funds when an order is filled (move from reserved into spent — i.e., decrement both reserved and total).
- Credit funds when a sell order is filled.
- Reset wallet to its initial state (used by portfolio reset feature).

#### 5.2.2 Portfolio Aggregate

- **Root:** `Portfolio`
- **Identity:** `PortfolioId` (1:1 with `UserId`)
- **Entities owned:** `Holding`
- **Value objects used:** `Symbol`, `Quantity`, `Price`

**State:**
- A collection of `Holding` entities, each describing the quantity and average purchase price for a single symbol.
- A reserved-quantity field per holding tracks shares committed to open sell orders.

**Invariants:**
- A holding's available quantity (total minus reserved) must never be negative.
- A holding's reserved quantity must never exceed its total quantity.
- Average purchase price is recalculated on each buy fill using the weighted-average method.

**Behaviors:**
- Reserve a quantity of a symbol for a pending sell order.
- Release reserved quantity on cancellation.
- Add to a holding on a buy fill (updating quantity and average price).
- Reduce a holding on a sell fill.
- Reset portfolio to its initial state.

#### 5.2.3 Order Aggregate

- **Root:** `Order`
- **Identity:** `OrderId`
- **Value objects used:** `Symbol`, `Price`, `Quantity`, `OrderSide`, `OrderType`, `OrderStatus`

**State:**
- Identity, owner (`UserId`), symbol, side (Buy or Sell), type (Limit or Market), price (nullable for Market), original quantity, filled quantity, status, creation timestamp, last-update timestamp.

**Invariants:**
- Filled quantity must never exceed original quantity.
- Price must be present for Limit orders and absent for Market orders.
- An order in a terminal state (Filled, Cancelled, Rejected) cannot transition to any other state.

**State Machine:**

```
                     ┌────────────┐
                     │  Pending   │
                     └────┬───────┘
                          │
              ┌───────────┼────────────┐
              ▼           ▼            ▼
       ┌──────────┐ ┌───────────┐ ┌──────────┐
       │PartFilled│ │   Filled  │ │Cancelled │
       └────┬─────┘ └───────────┘ └──────────┘
            │
       ┌────┴────┐
       ▼         ▼
   ┌────────┐ ┌─────────┐
   │Filled  │ │Cancelled│
   └────────┘ └─────────┘
```

Terminal states: `Filled`, `Cancelled`, `Rejected`.

**Behaviors:**
- Apply a fill (increment filled quantity, transition status as appropriate).
- Cancel (only allowed in Pending or PartiallyFilled states).
- Reject (only allowed from Pending; carries a reason).

#### 5.2.4 Trade Aggregate

- **Root:** `Trade`
- **Identity:** `TradeId`
- **Value objects used:** `Symbol`, `Price`, `Quantity`

**State:**
- Identity, symbol, buy order ID, sell order ID, buyer user ID, seller user ID, execution price, quantity, execution timestamp.

**Invariants:**
- A Trade is immutable once created.
- Buy and sell order IDs must reference distinct orders.
- Execution price equals the price of the resting (maker) order.

**Behaviors:**
- None beyond construction. Trades are created and never modified.

### 5.3 Value Objects

| Value Object | Purpose | Validation |
|--------------|---------|------------|
| `UserId`, `OrderId`, `TradeId`, `PortfolioId` | Strongly-typed identifiers | Non-empty GUID |
| `Username` | User's display name | Length, allowed characters |
| `EmailAddress` | Email | Valid email format |
| `PasswordHash` | Hashed password | Non-empty, hash format |
| `Symbol` | Tradable instrument | Uppercase, 1–5 characters |
| `Money` | Cash amount with currency | Non-negative for balances, 4 decimal places |
| `Price` | Order or trade price | Positive, up to 4 decimals |
| `Quantity` | Number of shares | Positive integer |
| `OrderSide` | Buy or Sell | Enum |
| `OrderType` | Limit or Market | Enum |
| `OrderStatus` | Lifecycle status | Enum |

### 5.4 Domain Events

Domain events are raised by aggregates and dispatched by the Application layer after persistence succeeds.

| Event | Raised By | Triggered When |
|-------|-----------|----------------|
| `UserRegisteredEvent` | User | A user completes registration. |
| `OrderPlacedEvent` | Order | A new order is created and persisted. |
| `OrderMatchedEvent` | Order | An order receives a fill (full or partial). |
| `OrderCancelledEvent` | Order | An order is cancelled by user or system. |
| `OrderRejectedEvent` | Order | An order fails validation at the engine. |
| `TradeExecutedEvent` | Trade | A trade is created from a successful match. |
| `WalletDebitedEvent` | User | Funds settle from a buy order fill. |
| `WalletCreditedEvent` | User | Funds credit from a sell order fill. |
| `HoldingUpdatedEvent` | Portfolio | A holding's quantity changes. |
| `PortfolioResetEvent` | Portfolio | A user resets their portfolio. |

### 5.5 Domain Services

Some operations span multiple aggregates and do not belong to any single one. These are modeled as domain services in the Domain layer:

- **OrderValidationService** — verifies that a proposed order is internally consistent (price/type/quantity rules). It does not check funds or holdings, which belong to the Wallet and Portfolio aggregates respectively.
- **MatchingService** — encapsulates the price-time-priority matching algorithm. It operates on an in-memory order book representation and produces a list of trades plus updated order states. It has no persistence concerns.

### 5.6 Aggregate Boundaries and Transactional Consistency

Each aggregate is the unit of transactional consistency. A single command may modify exactly one aggregate transactionally; modifications to other aggregates are coordinated through domain events handled in the Application layer.

For matching, which inherently touches multiple aggregates (Order, Trade, two Users, two Portfolios), the Application layer orchestrates the persistence within a single Unit of Work to keep the writes atomic at the database level, while domain events drive any further reactions (notifications, projections).

---

## 6. CQRS Design

### 6.1 Pipeline

Both commands and queries flow through a mediator. Cross-cutting behaviors are implemented as pipeline behaviors:

```
Request ─► Validation ─► Logging ─► Handler ─► Response
```

### 6.2 Commands

Commands represent intent to change state. They return either a result identifier or a void/result wrapper.

| Command | Triggered By | Modifies |
|---------|--------------|----------|
| `RegisterUserCommand` | API (POST /users) | User aggregate |
| `LoginUserCommand` | API (POST /auth/login) | (Read-side, but kept on the command pipeline due to side effects) |
| `PlaceOrderCommand` | API (POST /orders) | Wallet or Portfolio (reservation) + Order |
| `CancelOrderCommand` | API (DELETE /orders/{id}) | Order + Wallet/Portfolio (release) |
| `ResetPortfolioCommand` | API (POST /portfolio/reset) | User, Portfolio, Orders |
| `ProcessMatchCommand` | Matching engine internal | Order(s), Trade, Wallet(s), Portfolio(s) |

### 6.3 Queries

Queries read state and never modify it. Each query returns a DTO defined in the Contracts project.

| Query | Returns |
|-------|---------|
| `GetMyWalletQuery` | Cash balance breakdown |
| `GetMyPortfolioQuery` | Holdings, valuation |
| `GetMyOpenOrdersQuery` | Active orders |
| `GetMyOrderHistoryQuery` | Paginated past orders |
| `GetMyTradeHistoryQuery` | Paginated trades |
| `GetOrderBookSnapshotQuery` | Top N levels of bids and asks |
| `GetRecentTradesQuery` | Most recent N trades (trade tape) |
| `GetCandlesticksQuery` | OHLCV bars for a given interval |

### 6.4 Read vs Write Models

The MVP uses the same database for both reads and writes, but the models are distinct:

- The write side uses the Domain aggregates.
- The read side uses dedicated DTOs and projects directly via the persistence layer, bypassing aggregate reconstruction for efficiency.

This separation allows the read model to be denormalized or moved to a different store in the future without changing the write model.

### 6.5 Validators

Every command has a validator. Validation runs as a pipeline behavior before the handler executes. Validation failures result in a structured failure response and never reach the handler.

### 6.6 Pipeline Behaviors

- **Logging Behavior** — logs every command and query at entry and exit, including duration.
- **Validation Behavior** — runs all validators for the request type and short-circuits on failure.
- **Unit of Work Behavior** — wraps command execution in a transaction and commits on success.

---

## 7. Producer-Consumer Pipeline

### 7.1 Purpose

The Channel-based pipeline decouples the API's order intake from the matching engine's processing loop. This achieves three goals:

1. **Fast API responses** — the API returns as soon as the order is persisted and enqueued, without waiting for matching to complete.
2. **Deterministic matching** — orders are matched in a strict, single-threaded sequence inside the engine.
3. **Backpressure** — a bounded channel applies natural backpressure when the engine cannot keep up, slowing down the producer side rather than running out of memory.

### 7.2 Channel Topology

Two channels exist in the MVP:

- **Incoming Order Channel** — bounded; written by API command handlers, read by the matching engine loop.
- **Outgoing Trade Event Channel** — unbounded; written by the matching engine, read by multiple consumers (persistence sink, notification sink, projection sink).

### 7.3 Producer Side (API)

When a `PlaceOrderCommand` handler completes its work (persisting the order in Pending state and reserving funds/holdings), it writes an order intake message to the Incoming Order Channel. The handler then returns to the API client.

Cancellation requests follow the same channel to preserve ordering: the engine processes places and cancels in the order they were submitted.

### 7.4 Consumer Side (Matching Engine)

The matching engine runs a single background loop that reads from the Incoming Order Channel:

- For a place message: locate the order in memory (or load it), pass it to the matching service against the in-memory order book, and produce zero or more trade events.
- For a cancel message: remove the order from the order book and update its status.

The matching loop is intentionally single-threaded for the core matching step. Concurrency exists only at the channel boundaries.

### 7.5 Backpressure and Capacity

The Incoming Order Channel is bounded with a configurable capacity. When the channel is full, writers wait until space becomes available. This protects the engine from being overwhelmed and protects the host from unbounded memory growth.

### 7.6 Graceful Shutdown

On shutdown, the channel is completed (no more writes accepted) and the matching engine drains all remaining messages before exiting. This ensures no orders are lost between intake and matching.

### 7.7 Recovery Model

On startup, the matching engine rebuilds the in-memory order book from the persisted state of all open orders. The Channel itself is in-memory and ephemeral; durability comes from the database, not the channel.

---

## 8. Service Boundaries

### 8.1 API Service

**Responsibilities:**
- Expose REST endpoints for all PRD user actions (registration, login, order placement, cancellation, queries).
- Host a SignalR hub for real-time updates (order book changes, trade tape, user-specific notifications).
- Authenticate requests and identify the calling user.
- Validate inputs at the boundary (basic shape validation; deeper validation happens in command validators).
- Forward command intent into the Incoming Order Channel where appropriate.

**Endpoints (logical grouping):**

| Group | Endpoints |
|-------|-----------|
| Auth | Register, Login, Logout |
| Wallet | Get wallet |
| Portfolio | Get portfolio, Reset portfolio |
| Orders | Place order, Cancel order, Get open orders, Get order history |
| Trades | Get my trade history, Get recent market trades |
| Market Data | Get order book snapshot, Get candlesticks |

### 8.2 Matching Engine Service

**Responsibilities:**
- Maintain the in-memory order book for the single supported symbol.
- Consume from the Incoming Order Channel.
- Execute the price-time-priority matching algorithm.
- Produce trade events into the Outgoing Trade Event Channel.
- Persist resulting state changes (order updates, trade records, wallet and portfolio updates) atomically.
- Rebuild order book state on startup from persisted open orders.

### 8.3 Simulated Liquidity Component

Hosted inside the Matching Engine Service, this component generates background orders to keep the order book populated, addressing the dependency identified in the PRD. It places limit orders around a target price using a configurable distribution and refreshes them periodically.

This component is internal to the engine; it does not produce real user activity and its orders are clearly marked at the data level.

---

## 9. Real-Time Notifications

### 9.1 Transport

Real-time updates use SignalR over WebSocket. The hub is hosted in the API service.

### 9.2 Channels (Hub Groups)

| Group | Subscribers | Messages |
|-------|-------------|----------|
| Market (per symbol) | All connected clients | Order book updates, last trade price, trade tape entries |
| User (per user ID) | The owning user | Order fill notifications, order cancellation confirmations, balance updates |

### 9.3 Message Flow

The Outgoing Trade Event Channel feeds a notification consumer that publishes the appropriate messages to the SignalR hub. The hub then fans them out to subscribed clients.

### 9.4 Backplane

For the MVP, a single API process hosts the hub and no SignalR backplane is required. Redis is reserved for caching and read-side projections only.

---

## 10. Persistence Strategy

### 10.1 Storage Choices

| Store | Use |
|-------|-----|
| PostgreSQL | Source of truth for all aggregates (users, orders, trades, holdings, wallets). |
| Redis | Order book snapshots for fast read queries, candlestick aggregates, trade tape buffer. |
| File system | Application logs. |

This document does not specify table schemas; those are defined in a separate database design artifact.

### 10.2 Repositories

The Application layer defines repository interfaces per aggregate root:

- `IUserRepository`
- `IPortfolioRepository`
- `IOrderRepository`
- `ITradeRepository`

The Infrastructure layer implements these against EF Core. Repositories operate on whole aggregates only; partial-aggregate access is not exposed.

### 10.3 Unit of Work

A `IUnitOfWork` interface in the Application layer represents a transactional boundary. Command handlers obtain a Unit of Work, mutate one or more aggregates through their repositories, and commit. The Infrastructure implementation maps this to an EF Core transaction.

### 10.4 Concurrency Control

Wallets, portfolios, and orders carry a row version. Updates use optimistic concurrency. On conflict, the affected command retries up to a small, bounded number of times before failing with a structured error.

### 10.5 Read Projections

For high-frequency reads (order book snapshot, trade tape), the matching engine pushes denormalized projections into Redis after each state change. Query handlers prefer Redis projections when available and fall back to PostgreSQL otherwise.

---

## 11. Frontend Architecture

The frontend is intentionally minimal. It exists to demonstrate the platform's capabilities, not as a primary engineering deliverable.

### 11.1 Stack

- **React 19** with TypeScript.
- **Vite** as the build tool and dev server.
- **TailwindCSS** for styling.
- **shadcn/ui** for component primitives.
- **Zustand** for client-side state.
- **TanStack Query** for REST data fetching and caching.
- **react-hook-form + zod** for form state and validation.
- **@microsoft/signalr** for real-time updates.
- **lightweight-charts** for candlestick rendering.

### 11.2 Structure

```
web/
├── src/
│   ├── app/                  (top-level routing, providers)
│   ├── components/           (shared UI components)
│   ├── features/
│   │   ├── auth/
│   │   ├── trading/          (order form, order book, trade tape)
│   │   ├── portfolio/
│   │   └── orders/           (open orders, order history)
│   ├── lib/                  (API client, SignalR client, utilities)
│   ├── store/                (Zustand stores)
│   └── types/                (shared types, zod schemas)
├── index.html
├── package.json
└── vite.config.ts
```

### 11.3 State Management

- **Server state** — managed by TanStack Query. All REST queries and mutations go through it.
- **Real-time state** — SignalR messages update either TanStack Query cache (via `queryClient.setQueryData`) or Zustand stores depending on the data shape.
- **Client state** — Zustand for UI-only concerns (selected timeframe, panel visibility, form drafts).

### 11.4 Validation

Every form uses react-hook-form with a zod schema. The same zod schemas are used to type the form values and to validate before submission.

### 11.5 Testing

The frontend has no automated tests. Manual verification is acceptable for the MVP.

---

## 12. Orchestration with .NET Aspire

### 12.1 Role of AppHost

The Aspire AppHost project is the single entry point for running the system locally. It declares all required resources and how they connect.

### 12.2 Declared Resources

- A PostgreSQL container with a persistent data volume and PgAdmin.
- A Redis container with Redis Commander for inspection.
- The API service project.
- The Matching Engine service project.
- The React frontend (`web/`), declared as a Vite app with Yarn (`AddViteApp` + `WithYarn`).

### 12.3 Wiring

Aspire injects connection strings and service endpoints automatically:

- Both .NET services receive Postgres and Redis connection strings.
- The frontend receives the API endpoint URL as an environment variable for Vite.
- The API service receives the Matching Engine endpoint (if needed for health checks).

### 12.4 Local Workflow

A developer runs the AppHost project. Aspire starts all containers and services in the correct order based on declared dependencies, exposes endpoints, and provides a dashboard for logs and resource state. Docker Compose is not used.

### 12.5 ServiceDefaults

The ServiceDefaults project provides shared configuration applied to all .NET services: default logging, default health checks, default service discovery wiring. Each service project references ServiceDefaults and calls its extension method during startup.

---

## 13. Configuration Management

### 13.1 Sources

Configuration is layered in the following order (later overrides earlier):

1. `appsettings.json` — defaults.
2. `appsettings.Development.json` — local overrides.
3. Environment variables — supplied by Aspire at runtime.
4. User secrets — for any developer-specific values.

### 13.2 Key Configuration Areas

- Database connection (supplied by Aspire).
- Redis connection (supplied by Aspire).
- Channel capacity and behavior options.
- Initial virtual cash amount.
- Portfolio reset cooldown duration.
- Simulated liquidity parameters.
- Logging levels.

Defaults below implement [`PRD.md`](PRD.md) unless overridden by environment or `appsettings.Development.json`. See [`TRACEABILITY.md`](TRACEABILITY.md) for full FR/US mapping.

| Key | PRD | Default |
|-----|-----|---------|
| `Session:CookieName` | Auth (§15) | `TradingSimulator.Session` |
| `Session:ExpirationHours` | Auth (§15) | `24` |
| `Concurrency:MaxRetryAttempts` | §10.4 optimistic concurrency | `3` |
| `Concurrency:BaseDelayMilliseconds` | §10.4 optimistic concurrency | `25` |
| `Channels:IncomingOrderCapacity` | NFR throughput | `1000` |
| `Trading:InitialVirtualCash` | FR-1.3 | `100000` |
| `Trading:PortfolioResetCooldownMinutes` | FR-1.4 (24 hours) | `1440` |
| `Trading:SimulatedLiquidity:*` | PRD §11.2 dependency | enabled; refresh interval configurable |

---

## 14. Logging

### 14.1 Approach

The MVP uses basic structured logging. Logs are written to the console (captured by the Aspire dashboard) and to rotating files on disk.

### 14.2 Standard Fields

Every log entry includes:

- Timestamp.
- Log level.
- Service name.
- Message.
- Structured properties relevant to the event.

### 14.3 What Gets Logged

- Application lifecycle events (startup, shutdown).
- Every command and query, with duration and outcome.
- Order placements, matches, cancellations, and rejections.
- Exceptions with full stack traces.
- Channel state changes (drain on shutdown, capacity warnings).

### 14.4 Log Levels

- **Trace/Debug** — disabled by default; available for deep investigation.
- **Information** — normal operations.
- **Warning** — recoverable anomalies (concurrency retries, validation failures).
- **Error** — unhandled exceptions, persistence failures.
- **Critical** — fatal failures that require restart.

Advanced observability (tracing, metrics, dashboards) is intentionally out of scope.

---

## 15. Security

### 15.1 Authentication

The API uses cookie-based authentication with a server-issued session identifier. Sessions are stored server-side in Redis with a configurable expiration.

### 15.2 Password Storage

Passwords are hashed using a modern password-hashing algorithm with a per-user salt. Plaintext passwords are never stored or logged.

### 15.3 Authorization

All endpoints except registration and login require an authenticated session. Resource ownership is enforced at the application layer: a user can only read and modify their own orders, wallet, and portfolio.

### 15.4 Transport

Local development runs over HTTPS using developer certificates provisioned by Aspire.

### 15.5 Input Handling

All inputs cross a validation layer before reaching domain logic. Anti-forgery protection is applied to state-changing endpoints. Output encoding is applied at the frontend boundary.

---

## 16. Error Handling

### 16.1 Domain Errors

Domain rule violations are expressed as typed domain exceptions or result objects. Command handlers translate these into structured error responses without leaking internal details.

### 16.2 API Error Responses

The API returns problem details (RFC 7807) for all error responses, including:

- A machine-readable error code.
- A human-readable message.
- Additional structured fields for validation failures.

### 16.3 Unexpected Errors

Unhandled exceptions are caught by a global middleware, logged in full, and returned to the client as a generic error response without sensitive details.

### 16.4 Matching Engine Resilience

The matching engine catches exceptions per channel message. A failure handling one order does not stop the loop. The failing message is logged and, if possible, the order is moved to a Rejected state.

---

## 17. Testing Strategy

Testing is selective and focused on areas where correctness is non-trivial or business-critical. Tests are written where they add real value, not as a coverage exercise.

### 17.1 Domain Unit Tests

Aggregates with non-trivial invariants and state machines are tested:

- Order state transitions (valid and invalid).
- Wallet reserve/release/settle behavior.
- Portfolio holding updates including weighted average price.

Pure value objects with trivial logic are not separately tested.

### 17.2 Matching Engine Unit Tests

The matching service is tested thoroughly because correctness is critical:

- Price-time priority is respected.
- Partial fills behave correctly.
- Market orders consume liquidity correctly.
- Market orders with insufficient liquidity cancel the remainder.
- Limit buy at or above best ask matches immediately.
- Limit sell at or below best bid matches immediately.

### 17.3 API Integration Tests

A small set of end-to-end happy-path and key-failure-path tests, using Testcontainers to provision a real PostgreSQL and Redis. These verify that the layers wire together correctly:

- Register and log in.
- Place a limit order that does not match.
- Place opposing orders that match and produce a trade.
- Cancel an open order.
- Place an order with insufficient funds and receive a rejection.

### 17.4 Frontend Tests

None. Frontend correctness is verified manually.

### 17.5 What Is Not Tested

- Trivial getters, setters, and pass-through code.
- EF Core configuration (covered implicitly by integration tests).
- Third-party libraries.

---

## 18. Continuous Integration

### 18.1 CI Only, No CD

The project has no deployment pipeline. CI runs on every push and pull request.

### 18.2 CI Pipeline

A single GitHub Actions workflow executes:

1. Check out the repository.
2. Set up the .NET 10 SDK.
3. Restore dependencies.
4. Build the solution.
5. Run unit tests.
6. Run integration tests (Testcontainers will spin up the required containers).
7. Publish test results as an artifact.

The frontend is built but not tested in CI.

### 18.3 Branch Strategy

A trunk-based workflow with short-lived feature branches and pull requests targeting `main`. CI must pass before merge.

---

## 19. Local Development Workflow

### 19.1 Prerequisites

- .NET 10 SDK.
- Node.js LTS and Yarn (see `web/yarn.lock`).
- Docker Desktop running.

### 19.2 First-Time Setup

1. Clone the repository.
2. Restore .NET dependencies.
3. Install frontend dependencies under `web/`.
4. Trust the development certificate.

### 19.3 Running the System

A developer launches the Aspire AppHost project. Aspire starts PostgreSQL, Redis, the API service, the Matching Engine service, and the frontend dev server. The Aspire dashboard provides a unified view of all logs and resource state.

### 19.4 Database Migrations

EF Core migrations are applied automatically on service startup in the development environment. A separate command is available to generate new migrations during development.

---

## 20. Open Questions and Future Considerations

The following items are deferred beyond the MVP and are listed here only to document architectural seams that have been designed to accommodate them.

- **Outbox Pattern** — replacing direct event publication with a transactional outbox to guarantee at-least-once delivery to downstream consumers.
- **Message Broker** — replacing the in-process Channel with RabbitMQ or a similar broker to allow the matching engine to run as a fully independent process.
- **Advanced Observability** — distributed tracing, metrics, and dashboards.
- **Read-Side Separation** — moving the read model to a dedicated store or maintaining a separate projection database.
- **Horizontal Scaling** — the current design assumes a single matching engine instance per symbol. Multi-symbol scaling would require partitioning by symbol.

These items are out of scope for v1.0 and are not addressed by this document.

---

*End of Document*
