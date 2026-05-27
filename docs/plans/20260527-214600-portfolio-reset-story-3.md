---
artifact_type: plan
artifact_version: 1
id: plan-20260527-214600-portfolio-reset-story-3
title: Portfolio Reset - Story 3 (Cancel open orders and clear activity history)
slug: portfolio-reset-story-3
filename_template: 20260527-214600-portfolio-reset-story-3.md
created_at: 2026-05-27T21:46:00+07:00
updated_at: 2026-05-27T22:23:00+07:00
status: draft
owner: engineering
tags: [plan, implementation, trading-simulator, portfolio-reset, orders, history, us-04, story-3]
related_spec: docs/specs/20260525-251500-portfolio-reset.md
related_plans: [docs/plans/20260527-210000-portfolio-reset-story-2.md, docs/plans/20260525-260000-portfolio-reset-story-1.md]
prd_refs: [PRD SS6.1 FR-1.4, PRD SS6.5 FR-5.1, PRD SS6.5 FR-5.2, PRD SS6.6 FR-6.4, PRD SS7.1, PRD SS7.3]
tech_refs: [Tech SS5.2.3 Order, Tech SS6 CQRS Design, Tech SS7 Producer-Consumer Pipeline, Tech SS8.1 API Service, Tech SS9.2 SignalR groups, Tech SS10.5 Read projections, Tech SS16 Error handling]
db_refs: [DB SS4.5 orders, DB SS4.6 trades, DB SS4.10 portfolio_resets, DB SS6.5 ix_orders_user_status, DB SS6.6 trade history indexes, DB SS10.4 Portfolio Reset]
github:
  repo: tranvuongduy2003/trading-simulator
  epic_issue: 43
  story_issue_ids: [46]
  last_synced_at: 2026-05-27T21:46:00+07:00
search_index:
  keywords: [portfolio reset, story 3, cancel open orders, clear order history, clear trade history, reset cutoff, order book convergence, reset in progress, user-scoped history, reset orchestration]
  bounded_contexts: [Trading]
  task_count: 5
---

# Implementation Plan: Portfolio Reset - Story 3

| Field | Value |
|-------|--------|
| Spec | `docs/specs/20260525-251500-portfolio-reset.md` |
| Status | DRAFT |
| Tasks | 5 |
| Branch | `feature/portfolio-reset-story-3` |
| Aspire impact | No topology change |
| Schema impact | No migration |
| Test levels | Api.IntegrationTests + manual UI |
| ADRs required | ADR-007: reset history visibility strategy |
| GitHub | Synced 2026-05-27T21:46:00+07:00 - see GitHub Links |

## Executive summary

Story 3 completes the reset write path started in Stories 1-2 by adding open-order cancellation and user-scoped activity-history clearing behavior. The implementation will atomically cancel the current user's open orders (Pending/PartiallyFilled), release reserved balances/quantities through the reset transaction, and persist a reset cutoff instant used by order/trade history reads. Because the current codebase does not yet expose `/api/orders/open`, `/api/orders/history`, or `/api/trades`, this plan includes a minimal read-side slice for those endpoints with reset-aware filtering to satisfy acceptance criteria. Matching-engine and real-time updates are delivered via Application realtime ports and existing SignalR groups, with explicit non-goal of global tape mutation.

## Goals and non-goals

**Goals**
- G1: Reset cancels all current-user open orders and sets `status = Cancelled` + `terminated_at`.
- G2: Reserved wallet/holding amounts tied to open orders are released before wallet reset to keep invariants valid.
- G3: `GET /api/orders/open`, `GET /api/orders/history`, `GET /api/trades` return empty first page immediately after reset for the same user.
- G4: Order-book/user notifications converge quickly; no canceled user order remains matchable after reset commit.

**Non-goals**
- NG1: Changing global market tape retention or deleting `trades` rows.
- NG2: Implementing Story 4 cooldown enforcement (`RESET_COOLDOWN_ACTIVE`).
- NG3: Multi-symbol behavior (still AAPL-only MVP).
- NG4: New infrastructure or broker/outbox architecture.

## Traceability matrix

| Spec story | Plan task(s) | Test evidence |
|------------|--------------|---------------|
| Story 3 open orders become zero | Task 2, 4 | `ResetPortfolio_WhenUserHasOpenOrders_OpenOrdersEndpointReturnsEmpty` |
| Story 3 no ghost liquidity | Task 3, 5 | Integration test with realtime/order-book assertion + manual checklist |
| Story 3 order + trade history first page empty | Task 1, 4 | `GetMyOrderHistory_AfterReset_ReturnsEmpty`, `GetMyTradeHistory_AfterReset_ReturnsEmpty` |
| Story 3 no-op when no open orders/history | Task 2, 4 | `ResetPortfolio_WithoutOpenOrdersAndHistory_Succeeds` |
| Story 3 counterparty order remains | Task 2, 4 | `ResetPortfolio_CancelsOnlyCurrentUserOpenOrders` |

## Architecture impact

```text
web (reset button + tabs)
  -> POST /api/portfolio/reset
  -> GET /api/orders/open | /api/orders/history | /api/trades
         |
Api Endpoints -> MediatR handlers (Application)
         |
PortfolioResetWriteRepository (Infrastructure, PostgreSQL)
  - cancel user open orders
  - release reservations
  - reset wallet + holdings
  - append portfolio_resets row
         |
Realtime publisher (user:{userId}, market:{symbol}) after commit
```

| Layer | Change summary |
|-------|----------------|
| Domain | Optional order-status constants helper only; no new aggregate behavior required for this slice |
| Application | Add order/trade query handlers + reset orchestration abstractions |
| Infrastructure | Extend reset repository with order cancellation/release logic and read repositories for orders/trades |
| Api | Add three read endpoints + response contracts |
| MatchingEngine | No host-level change; reset publishes cancellation/removal notifications |
| web/ | No new feature work required; uses existing query invalidation in Story 5 plan |
| AppHost | None |

## Data and migration plan

| Artifact | Action | DB reference |
|----------|--------|--------------|
| EF migration | None | Schema already has `orders`, `trades`, `portfolio_resets` |
| PostgreSQL write path | Extend reset transaction order | DB SS10.4 |
| Redis keys | Update/refresh order-book projection through existing publisher flow | DB SS12 |
| History strategy | Read-side cutoff by latest `portfolio_resets.reset_at` per user | DB SS4.10 |

## Open questions

| # | Question | Source | Answer | Status |
|---|----------|--------|--------|--------|
| 1 | Clear history by deleting rows or by read cutoff? | Spec SS13 Q1 | Use read cutoff; keep immutable market records | ✅ |
| 2 | Should reset create synthetic cancellation history entries while history is hidden? | Story 3 wording | No for MVP; cutoff hides all prior rows | ✅ |
| 3 | How to prove 500 ms convergence locally without flaky tests? | PRD SS7.1 | Keep automated bounded polling + manual Aspire timing check | ⏳ |

## Risks and mitigations

| Risk | Likelihood | Impact | Mitigation | Owner task |
|------|------------|--------|------------|------------|
| Missing order/trade read stack causes oversized scope | H | H | Add minimal read contracts/endpoints only for required paths | Task 1 |
| Reset writes bypass domain cancellation semantics | M | M | Centralize SQL update rules in one repository + explicit tests for release math | Task 2 |
| Realtime convergence assertions flake | M | M | Use polling helper with timeout; keep strict checks in manual checklist | Task 5 |
| Counterparty order accidentally canceled | L | H | Filter updates by `user_id` and add dedicated test | Task 4 |

## Prerequisites

- [ ] Story 2 branch baseline available (wallet/holdings/audit row reset already implemented)
- [ ] Docker/Testcontainers ready for integration tests
- [ ] Existing reset endpoint contract preserved (`POST /api/portfolio/reset` 200 payload)
- [ ] No pending schema changes needed for orders/trades tables

## File structure (planned)

```text
src/
  Contracts/
    Orders/
      OpenOrderDto.cs
      OrderHistoryItemDto.cs
      OrderHistoryResponse.cs
    Trades/
      TradeHistoryItemDto.cs
      TradeHistoryResponse.cs
  Application/
    Abstractions/Persistence/
      IOrderReadRepository.cs
      ITradeReadRepository.cs
      IPortfolioResetWriteRepository.cs              MODIFY
    Orders/Queries/
      GetMyOpenOrdersQuery.cs
      GetMyOpenOrdersQueryHandler.cs
      GetMyOrderHistoryQuery.cs
      GetMyOrderHistoryQueryHandler.cs
    Trades/Queries/
      GetMyTradeHistoryQuery.cs
      GetMyTradeHistoryQueryHandler.cs
    Portfolios/Commands/
      ResetPortfolioCommandHandler.cs                MODIFY
  Infrastructure/
    Persistence/Repositories/
      OrderReadRepository.cs
      TradeReadRepository.cs
      PortfolioResetWriteRepository.cs               MODIFY
    DependencyInjection.cs                           MODIFY
  Api/
    Endpoints/
      OrdersEndpoint.cs
      TradesEndpoint.cs
      PortfolioEndpoint.cs                           MODIFY (response codes if needed)
tests/
  Api.IntegrationTests/
    Orders/
      GetMyOpenOrdersTests.cs
      GetMyOrderHistoryTests.cs
    Trades/
      GetMyTradeHistoryTests.cs
    Portfolios/
      ResetPortfolioTests.cs                         MODIFY
      PortfolioResetTestHelpers.cs                   MODIFY
docs/memory/decisions.md                             MODIFY (ADR-007)
```

## Authorization, session, and domain notes

- **Session model:** Cookie session; all new read endpoints require authorization.
- **Route protection:** owner-scoped reads only (`currentUserAccessor.UserId`).
- **Domain rules to preserve:** BR-04 (user-scoped history empty, market tape unchanged), BR-07 (cancel releases reservations), BR-08 (engine/book updated).

## Progress tracker

### Task 1: Add order/trade read slice with reset-aware filters

| Attribute | Value |
|-----------|--------|
| Spec story | Story 3 |
| Depends on | None |
| Estimated complexity | L |
| Parent story issue | #46 |

#### Objective

Introduce minimal `/api/orders/open`, `/api/orders/history`, and `/api/trades` query pipeline so Story 3 acceptance can be enforced and verified through API-level behavior.

#### Implementation notes

- Create Contracts DTOs for open orders and history items.
- Add read repositories with NoTracking projections and bounded first-page defaults.
- Apply reset cutoff filter: ignore rows with `created_at`/`executed_at` earlier than latest `portfolio_resets.reset_at` for that user.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `src/Api/Endpoints/OrdersEndpoint.cs` | Open/history routes |
| CREATE | `src/Api/Endpoints/TradesEndpoint.cs` | Trade history route |
| CREATE | `src/Application/Orders/Queries/*` | Query contracts + handlers |
| CREATE | `src/Application/Trades/Queries/*` | Query contracts + handlers |
| CREATE | `src/Infrastructure/Persistence/Repositories/OrderReadRepository.cs` | Order reads |
| CREATE | `src/Infrastructure/Persistence/Repositories/TradeReadRepository.cs` | Trade reads |
| MODIFY | `src/Infrastructure/DependencyInjection.cs` | Register new read repositories |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `GetMyOpenOrders_WithSession_ReturnsOnlyCurrentUser` | `tests/Api.IntegrationTests/Orders/GetMyOpenOrdersTests.cs` |
| Integration | `GetMyOrderHistory_AfterReset_ReturnsEmpty` | `tests/Api.IntegrationTests/Orders/GetMyOrderHistoryTests.cs` |
| Integration | `GetMyTradeHistory_AfterReset_ReturnsEmpty` | `tests/Api.IntegrationTests/Trades/GetMyTradeHistoryTests.cs` |

#### Acceptance criteria

- [x] Three endpoints return 200 for authenticated user and 401 when unauthenticated.
- [x] First page after reset is empty for order/trade history.
- [x] Endpoints are owner-scoped and do not leak other users' rows.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | PRD SS6.5/6.6, Tech SS6, DB SS6.5/6.6 |
| PostgreSQL authoritative | Direct EF projections from DB |
| RFC 7807 errors | 401/404 patterns align with existing API |
| ADR needed? | Covered by ADR-007 |

#### Risk

Moderate: new read surface in a codebase that currently has only wallet/portfolio reads.

### Task 2: Extend reset transaction to cancel open orders and release reservations

| Attribute | Value |
|-----------|--------|
| Spec story | Story 3 |
| Depends on | Task 1 |
| Estimated complexity | L |
| Parent story issue | #46 |

#### Objective

Complete DB SS10.4 steps 1-2 before existing wallet/holdings reset so reset never leaves open orders or stale reservations for the current user.

#### Implementation notes

- In `ResetForUserAsync`, load user open orders (`status in (0,1)`), mark canceled (`status=3`, `terminated_at=resetAt`, `updated_at=resetAt`).
- Release reservations:
  - Buy orders release wallet reserved by remaining quantity * price (for limit orders).
  - Sell orders release holding reserved quantity for remaining shares.
- Apply this before wallet force-reset and holdings delete to preserve consistent transition semantics.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `src/Application/Abstractions/Persistence/IPortfolioResetWriteRepository.cs` | Expose cancellation result metadata if needed |
| MODIFY | `src/Infrastructure/Persistence/Repositories/PortfolioResetWriteRepository.cs` | Add cancellation + release logic |
| MODIFY | `tests/Api.IntegrationTests/Portfolios/PortfolioResetTestHelpers.cs` | Seed open orders + trades |
| MODIFY | `tests/Api.IntegrationTests/Portfolios/ResetPortfolioTests.cs` | Reset cancel behavior tests |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `ResetPortfolio_WhenUserHasOpenOrders_CancelsPendingAndPartiallyFilled` | `ResetPortfolioTests.cs` |
| Integration | `ResetPortfolio_CancelsOnlyCurrentUserOpenOrders` | `ResetPortfolioTests.cs` |
| Integration | `ResetPortfolio_ReleasesReservationsBeforeWalletReset` | `ResetPortfolioTests.cs` |

#### Acceptance criteria

- [x] Current-user open orders become canceled.
- [x] Counterparty orders remain unchanged.
- [x] Wallet and holdings remain invariant-safe throughout transaction.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| Async matching | API write path persists cancellation intent; no synchronous matching |
| PostgreSQL authoritative | One transaction in UoW |
| Redis projection | Handed off by realtime/publisher task |
| ADR needed? | No |

#### Risk

High: reservation release math can drift if not aligned with order remainder semantics.

### Task 3: Publish reset cancellation notifications and projection refresh signals

| Attribute | Value |
|-----------|--------|
| Spec story | Story 3 |
| Depends on | Task 2 |
| Estimated complexity | M |
| Parent story issue | #46 |

#### Objective

Ensure cancellation effects are observable in user and market streams after reset commit, avoiding ghost liquidity.

#### Implementation notes

- Reuse `IRealtimeNotificationPublisher` and existing cancellation message contracts.
- Publish one user cancellation notification per canceled order, plus market order-book update trigger for AAPL.
- Keep publication post-commit (existing command flow) to avoid phantom events on rollback.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `src/Application/Portfolios/Commands/ResetPortfolioCommandHandler.cs` | Trigger notification publisher with cancellation results |
| REUSE | `src/Api/Realtime/SignalRRealtimeNotificationPublisher.cs` | Existing adapter |
| MODIFY | `tests/Api.IntegrationTests/Portfolios/ResetPortfolioTests.cs` | Verify notification side effects where feasible |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `ResetPortfolio_PublishesOrderCancellationNotifications` | `ResetPortfolioTests.cs` |
| Manual | Reset with active open orders updates open-order panel quickly | Aspire checklist |

#### Acceptance criteria

- [x] User receives reset-driven cancellation events equivalent to manual cancel.
- [x] Market/order-book view no longer includes user canceled orders after convergence window.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| SignalR | `user:{userId}` and `market:{symbol}` groups |
| RFC 7807 errors | Unchanged |
| Aspire | None |
| ADR needed? | No |

#### Risk

Medium: without full matching engine order-book pipeline in place, some convergence checks remain manual.

### Task 4: History visibility validation and edge-case hardening

| Attribute | Value |
|-----------|--------|
| Spec story | Story 3 |
| Depends on | Task 2 |
| Estimated complexity | M |
| Parent story issue | #46 |

#### Objective

Cover edge/failure paths from Story 3 with repeatable integration tests and finalize reset-aware history behavior.

#### Implementation notes

- Add tests for no-open-order/no-history reset success.
- Add partially-filled order scenario where remainder is canceled and hidden history still returns empty.
- Confirm global-market semantics by proving another user's history remains visible to that user.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `tests/Api.IntegrationTests/Portfolios/ResetPortfolioTests.cs` | Story 3 edge tests |
| MODIFY | `tests/Api.IntegrationTests/Orders/GetMyOrderHistoryTests.cs` | Cutoff behavior |
| MODIFY | `tests/Api.IntegrationTests/Trades/GetMyTradeHistoryTests.cs` | Cutoff behavior |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `ResetPortfolio_WithoutOpenOrdersAndHistory_Succeeds` | `ResetPortfolioTests.cs` |
| Integration | `ResetPortfolio_WithPartiallyFilledOrder_HistoryHiddenForCurrentUser` | `ResetPortfolioTests.cs` |
| Integration | `ResetPortfolio_DoesNotHideCounterpartyOwnHistory` | `GetMyTradeHistoryTests.cs` |

#### Acceptance criteria

- [x] All Story 3 failure/edge paths have deterministic integration coverage.
- [x] No test depends on wall-clock sleeps; polling helper only.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | BR-04, BR-07, BR-08 |
| PostgreSQL authoritative | Verified by direct reads |
| Async matching | No direct match execution in tests |
| ADR needed? | No |

#### Risk

Low once repository and read filters are complete.

### Task 5: Polish, ADR, and manual verification closure

| Attribute | Value |
|-----------|--------|
| Spec story | Polish |
| Depends on | Task 1,2,3,4 |
| Estimated complexity | S |
| Parent story issue | #46 |

#### Objective

Finalize documentation, run regression subset, and record manual checklist outcomes for Story 3.

#### Implementation notes

- Add ADR-007 documenting read-cutoff strategy for "history cleared" without deleting trade records.
- Run focused integration suites for portfolio reset, orders, and trades endpoints.
- Execute manual Aspire checklist on tabs (Open Orders, Order History, Trade History, Holdings).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `docs/memory/decisions.md` | ADR-007 |
| MODIFY | `docs/memory/current-status.md` | completion/next-up update |
| MODIFY | `docs/CHANGELOG.md` | plan entry |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `dotnet test` reset/order/trade subsets | `tests/Api.IntegrationTests/*` |
| Manual | Tabs show empty state after reset | Aspire |

#### Acceptance criteria

- [x] ADR-007 captured and linked.
- [x] Integration subset green.
- [x] Manual checklist completed or explicitly handed off to operator.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | Traceability matrix complete |
| SignalR | Convergence notes documented |
| Aspire | Manual operator validation |
| ADR needed? | Yes - ADR-007 |

#### Risk

None - isolated to verification/documentation.

## Reference files

| File | Why open it |
|------|-------------|
| `src/Application/Portfolios/Commands/ResetPortfolioCommandHandler.cs` | Current reset orchestrator |
| `src/Infrastructure/Persistence/Repositories/PortfolioResetWriteRepository.cs` | Current Story 2 write path |
| `tests/Api.IntegrationTests/Portfolios/ResetPortfolioTests.cs` | Existing reset test baseline |
| `src/Infrastructure/Persistence/Configurations/OrderConfiguration.cs` | Order status and indexes |
| `src/Infrastructure/Persistence/Configurations/TradeConfiguration.cs` | Trade index model |
| `docs/specs/20260525-251500-portfolio-reset.md` | Story 3 acceptance and edge cases |

## Implementation details (for /build)

- Keep `ResetPortfolioCommand` under UoW behavior.
- Introduce repository methods to compute latest reset cutoff once and apply to both order-history and trade-history queries.
- Prefer pagination contract with default page size 25 for history endpoints (align PRD FR-5.2/FR-6.4).
- For open orders, filter `status IN (0,1)` and `created_at >= latestResetAt` if cutoff exists.
- For trade history, filter `(buyer_user_id = user OR seller_user_id = user)` and `executed_at >= latestResetAt`.
- Do not delete from `trades` table; preserve market integrity.

## Verification matrix (plan-level)

| Spec AC | Verified by |
|---------|-------------|
| Open orders zero after reset | Task 2 + open orders integration test |
| No ghost liquidity | Task 3 integration/manual convergence check |
| Order/trade history empty first page | Task 1/4 integration tests |
| No-op reset with no data | Task 4 integration test |
| Counterparty order remains | Task 2/4 integration test |

## Rollback / recovery

- **Code:** revert Story 3 commits.
- **DB:** no schema rollback needed; data effects can be re-tested with fresh seeded users.
- **Redis:** clear projections and rebuild from PostgreSQL if stale.

## Deferred work (Plan B)

- Story 4 cooldown enforcement and user-facing eligibility endpoint.
- Story 5 frontend query invalidation polish and richer realtime payloads.
- Matching-engine dedicated cancellation channel implementation once order pipeline is introduced.

## GitHub Links

| Local ref | Issue # | Type | Title | URL |
|-----------|---------|------|-------|-----|
| `spec.Story 3` | `#46` | Story | US-04 / Story 3: Cancel open orders and clear activity history | [#46](https://github.com/tranvuongduy2003/trading-simulator/issues/46) |
| `epic` | `#43` | Epic | Spec: Portfolio reset (US-04) | [#43](https://github.com/tranvuongduy2003/trading-simulator/issues/43) |

