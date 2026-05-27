---
artifact_type: plan
artifact_version: 1
id: plan-20260528-003204-portfolio-reset-story-5
title: Portfolio Reset - Story 5 (Consistent data everywhere after reset)
slug: portfolio-reset-story-5
filename_template: 20260528-003204-portfolio-reset-story-5.md
created_at: 2026-05-28T00:32:04+07:00
updated_at: 2026-05-28T18:00:00+07:00
status: complete
owner: engineering
tags: [plan, implementation, trading-simulator, portfolio-reset, tanstack-query, signalr, us-04, story-5]
related_spec: docs/specs/20260525-251500-portfolio-reset.md
related_plans:
  - docs/plans/20260525-260000-portfolio-reset-story-1.md
  - docs/plans/20260527-210000-portfolio-reset-story-2.md
  - docs/plans/20260527-214600-portfolio-reset-story-3.md
  - docs/plans/20260527-231500-portfolio-reset-story-4.md
prd_refs: [PRD SS5.1 US-04, PRD SS6.1 FR-1.4, PRD SS6.6 FR-6.2, PRD SS7.1, PRD SS7.4, PRD SS8.1]
tech_refs: [Tech SS5.4 PortfolioResetEvent, Tech SS8.1 Portfolio endpoints, Tech SS9.2 SignalR user group, Tech SS11 Frontend, Tech SS17.3 API integration tests]
db_refs: [DB SS4.2 wallets, DB SS4.3 portfolios, DB SS4.5 orders, DB SS4.6 trades, DB SS10.4 Portfolio Reset]
github:
  repo: tranvuongduy2003/trading-simulator
  epic_issue: 43
  story_issue_ids: [48]
  last_synced_at: 2026-05-28T00:32:04+07:00
search_index:
  keywords:
    [
      portfolio reset,
      story 5,
      query invalidation,
      tanstack query,
      wallet refetch,
      read-your-writes,
      signalr balance updated,
      order cancellation notified,
      multi-tab stale,
      virtual cash card,
      wallet top bar chip,
      EC-10,
      EC-11,
    ]
  bounded_contexts: [Trading]
  task_count: 6
---

# Implementation Plan: Portfolio Reset - Story 5

| Field | Value |
|-------|--------|
| Spec | `docs/specs/20260525-251500-portfolio-reset.md` (Story 5) |
| GitHub story | [#48 — Consistent data everywhere after reset](https://github.com/tranvuongduy2003/trading-simulator/issues/48) |
| Epic | [#43 — Portfolio reset (US-04)](https://github.com/tranvuongduy2003/trading-simulator/issues/43) |
| Depends on | Stories 1–4 shipped (reset POST, wallet/holdings write, order cancel + history cutoff, cooldown + eligibility) |
| Status | COMPLETE (automation); manual UI checklist pending operator |
| Tasks | 6 |
| Branch | `feature/portfolio-reset-story-5` |
| Aspire impact | No |
| Schema impact | No |
| Test levels | Manual UI (primary) · optional Api.IntegrationTests regression only |
| ADRs required | ADR-008 (post-reset client cache strategy) |
| GitHub | Synced 2026-05-28 — see GitHub Links |

## Executive summary

Story 5 closes the portfolio-reset UX loop on the **web client**: after a successful `POST /api/portfolio/reset`, every portfolio-related surface must show post-reset figures within **2 s** without logout. Backend work is already in place from Stories 2–3 (`ResetPortfolioCommandHandler` publishes `BalanceUpdated` and per-order `OrderCancellationNotified`; integration tests assert the publisher). The remaining gap is **TanStack Query cache coordination** — `use-reset-portfolio.ts` still has a Story 5 TODO, the success toast still defers holdings/orders to a “later release”, `['portfolio']` is not user-scoped and uses a 30s `staleTime` (breaks EC-11 multi-tab), and there are **no frontend hooks** yet for `GET /api/orders/open`, `/api/orders/history`, or `/api/trades` despite OpenAPI/codegen existing. This plan adds a shared invalidation helper, seeds wallet cache from the **200 response body** (authoritative, not client-fabricated), aligns portfolio/orders/trades queries with virtual-cash Story 4 refetch policy, extends the SignalR query bridge for trades, and mounts a minimal tabbed activity panel on the trading page so acceptance criteria for empty post-reset tabs are observable.

## Goals and non-goals

**Goals**

- G1: After reset success, `VirtualCashCard` and `WalletTopBarChip` show **$100,000.00** available within **2 s** (US-03, ADR-004).
- G2: Invalidate/refetch `wallet`, `portfolio`, `orders/open`, `orders/history`, and `trades` query prefixes on reset success; no pre-reset figures after refetch completes.
- G3: Rely on existing SignalR user-group messages; client bridge invalidates matching query keys (no new hub methods).
- G4: On wallet refetch **500** after successful reset, show US-03 error state — do not hardcode $100k in JSX.
- G5: Multi-tab stale holdings/cash corrected on window focus via per-query `refetchOnWindowFocus` (EC-11, compatible with virtual cash Story 4).

**Non-goals**

- NG1: New SignalR event types or backend reset transaction changes (Stories 2–3 own BR-08/BR-11).
- NG2: Full PRD §8.1 trading terminal layout (order book, chart, order form) — only the **bottom activity tabs** slice needed for Story 5 AC.
- NG3: Automated frontend tests (MVP policy).
- NG4: Order placement UI (EC-10 documented for manual verification when place-order ships).
- NG5: Changing global `refetchOnWindowFocus` in `providers.tsx`.

## Traceability matrix

| Spec story | Plan task(s) | Test evidence |
|------------|--------------|---------------|
| Cash card + top bar $100k within 2s | Task 2, 6 | Manual steps 1–2; reset response seeds `['wallet', userId]` |
| Invalidate wallet, portfolio, orders, trades | Task 1, 2, 3, 4 | Manual step 3; devtools Query cache inspection |
| SignalR cancel/balance on `user:{userId}` | Task 6 | REUSE `ResetPortfolio_PublishesOrderCancellationNotifications`; manual hub debug |
| Wallet refetch 500 → error, no fabricated $100k | Task 2, 6 | Manual step 4; no `100000` literals in components |
| Multi-tab focus → post-reset data | Task 3, 6 | Manual step 5; portfolio `staleTime: 0` + `refetchOnWindowFocus` |
| EC-10 place order after reset | — | Manual note when order form exists |
| EC-11 multi-tab stale | Task 3, 6 | Manual step 5 |

## Architecture impact

```text
User confirms reset (AppLayout / ResetPortfolioDialog)
  -> POST /api/portfolio/reset (200 + wallet snapshot)     [existing Api]
  -> onSuccess:
       seedWalletQueryData from response.wallet
       invalidatePortfolioPanels(queryClient)             [NEW web helper]
  -> parallel refetch:
       GET /api/wallet | /portfolio | /orders/open | /orders/history | /trades
  -> VirtualCashCard + WalletTopBarChip + Trading tabs re-render

Parallel path (already live):
  ResetPortfolioCommandHandler
    -> NotifyOrderCancellationAsync (per cancelled order)
    -> NotifyBalanceUpdatedAsync
    -> PublishOrderBookUpdatedAsync
  -> SignalR user:{userId}
  -> createSimulationHubQueryBridge invalidates overlapping keys
```

| Layer | Change summary |
|-------|----------------|
| Domain | **REUSE** — no change |
| Application | **REUSE** — `ResetPortfolioCommandHandler` realtime publishes |
| Infrastructure | **REUSE** — reset write + read repositories |
| Api | **REUSE** — endpoints from Stories 2–4 |
| MatchingEngine | **REUSE** — book updates via existing publisher |
| web/ | **MODIFY/CREATE** — query keys, invalidation, hooks, trading tabs, SignalR bridge, reset hook |
| AppHost | None |

## Data and migration plan

| Artifact | Action | DB reference |
|----------|--------|--------------|
| EF migration | None | — |
| Redis keys | None (client refetch only) | DB SS12 |
| Book recovery | N/A | Tech SS7 |

## Open questions

| # | Question | Source | Answer | Status |
|---|----------|--------|--------|--------|
| 1 | Is `POST` 200 `wallet` snapshot sufficient for immediate $100k display if `GET /api/wallet` refetch fails? | Story 5 failure AC | Yes — 200 body is server-authoritative; show US-03 error only when **no** successful wallet payload in cache (do not hardcode 100k in JSX) | ✅ |
| 2 | Mount bottom tabs on `TradingPage` vs separate `OrdersPage`? | PRD SS8.1 vs current routes | Minimal tabbed section on **TradingPage** (Holdings tab reuses existing card content) | ✅ |
| 3 | User-scope `['portfolio']` → `['portfolio', userId]`? | Story 3 privacy pattern | Yes — match `['wallet', userId]` | ✅ |

## Risks and mitigations

| Risk | Likelihood | Impact | Mitigation | Owner task |
|------|------------|--------|------------|------------|
| Invalidation prefix too narrow (misses keyed queries) | M | H | Central `portfolioPanelQueryKeys` + prefix invalidation | Task 1 |
| `setQueryData` masks failed refetch while showing wrong user | L | H | Scope keys with `userId`; `canDisplayWallet` guard unchanged | Task 2 |
| SignalR bridge omits `trades` | M | M | Add `['trades']` invalidation on cancel/balance | Task 6 |
| Bottom panel scope creep | M | M | Tab UI read-only tables + empty states only | Task 5 |
| Portfolio 30s staleTime blocks EC-11 | H | M | Align with wallet: `staleTime: 0`, `refetchOnWindowFocus: true` | Task 3 |

## Prerequisites

- [ ] Stories 1–4 merged or available on branch baseline (`feature/portfolio-reset-story-4` or `main`)
- [ ] `aspire run` / env-doctor green
- [ ] `yarn --cwd web build` passes on baseline
- [ ] Operator can seed user with open orders + history for manual reset walkthrough

## File structure (planned)

```text
web/src/
  lib/
    query-keys.ts                              CREATE  portfolio panel key factories
  features/
    portfolio-reset/
      invalidate-portfolio-panels.ts           CREATE
      use-reset-portfolio.ts                   MODIFY
    trading/
      hooks/
        use-portfolio-query.ts                 CREATE
      components/
        portfolio-activity-tabs.tsx            CREATE  open/history/trades/holdings tabs
      pages/
        trading-page.tsx                       MODIFY
    orders/
      api.ts                                   CREATE
      hooks/
        use-open-orders-query.ts               CREATE
        use-order-history-query.ts             CREATE
    trades/
      api.ts                                   CREATE
      hooks/
        use-trade-history-query.ts             CREATE
  lib/signalr/
    interceptors.ts                            MODIFY  trades invalidation
  features/auth/
    clear-user-queries.ts                      MODIFY  orders/trades prefixes
docs/memory/
  decisions.md                                 MODIFY  ADR-008
```

## Authorization, session, and domain notes

- **Session model:** Cookie session unchanged; all reads require authentication.
- **Route protection:** Reset and panel reads only when `authStatus === 'authenticated'`.
- **Domain rules:** BR-11 notifications already emitted server-side; client must not assume SignalR delivery — always invalidate on POST success.
- **Read-your-writes:** Prefer reset **200 wallet snapshot** for immediate UI, then confirm via `GET /api/wallet` refetch (virtual cash Story 4 policy).

## Progress tracker

### Task 1: Skeleton — shared query keys and post-reset invalidation

| Attribute | Value |
|-----------|--------|
| Spec story | Story 5 |
| Depends on | None |
| Estimated complexity | S |
| Parent story issue | #48 |

#### Objective

Introduce a single module defining portfolio-panel TanStack Query keys and an `invalidatePortfolioPanels` helper; wire `useResetPortfolio` `onSuccess` to call it so reset success immediately marks all panel queries stale (observable in React Query Devtools) even before UI surfaces mount.

#### Implementation notes

- Keys (prefix-friendly for `invalidateQueries`):
  - `['wallet', userId]`
  - `['portfolio', userId]`
  - `['orders', 'open', userId]`
  - `['orders', 'history', userId]`
  - `['trades', userId]`
- `invalidatePortfolioPanels(queryClient, userId)` calls `invalidateQueries` with each prefix.
- Keep existing eligibility invalidation.
- Remove Story 5 TODO comment once wired.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `web/src/lib/query-keys.ts` | Key factories |
| CREATE | `web/src/features/portfolio-reset/invalidate-portfolio-panels.ts` | Invalidation helper |
| MODIFY | `web/src/features/portfolio-reset/use-reset-portfolio.ts` | Call helper on success |
| REUSE | `web/src/features/trading/hooks/use-wallet-query.ts` | Wallet key shape |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Reset success → devtools shows invalidation for wallet/portfolio/orders/trades keys | `web/` |

#### Acceptance criteria

- [x] Successful reset triggers invalidation for all five panel prefixes
- [x] No change to error/cooldown paths
- [x] `yarn --cwd web lint` passes

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech | US-04 Story 5; Tech SS11 |
| PostgreSQL authoritative | Invalidation triggers refetch, not local fabrication |
| SignalR | N/A this task |
| ADR needed? | No |

#### Risk

None — isolated wiring.

---

### Task 2: Wallet read-your-writes from reset response

| Attribute | Value |
|-----------|--------|
| Spec story | Story 5 (cash surfaces) |
| Depends on | Task 1 |
| Estimated complexity | S |
| Parent story issue | #48 |

#### Objective

Within **2 s** of reset success, `VirtualCashCard` and `WalletTopBarChip` show **$100,000.00** available using the **200 response wallet snapshot**, then converge on `GET /api/wallet` after invalidation refetch.

#### Implementation notes

- Reuse `seedWalletQueryData` from `prefetch-wallet.ts` with `response.wallet` mapped to `WalletResponse` shape (`userId` from auth store).
- Call `invalidatePortfolioPanels` after seeding so refetch still runs.
- Update success toast copy — remove “later release” deferral; state holdings/orders/history refresh now.
- **Failure path:** never assign literal `100000` in JSX; if refetch errors and cache empty, `WALLET_LOAD_ERROR_MESSAGE` displays (US-03).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `web/src/features/portfolio-reset/use-reset-portfolio.ts` | Seed + invalidate |
| REUSE | `web/src/features/auth/prefetch-wallet.ts` | `seedWalletQueryData` |
| REUSE | `web/src/features/trading/components/virtual-cash-card.tsx` | Display |
| REUSE | `web/src/features/trading/components/wallet-top-bar-chip.tsx` | Display |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Reset depleted wallet → chip + card show $100,000.00 without reload | `web/` |

#### Acceptance criteria

- [x] Top bar and virtual cash card update immediately after successful reset
- [x] Refetch still issued; no hardcoded $100k in components
- [x] Success toast reflects full panel refresh

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD | US-03 + US-04 Story 5 |
| ADR-004 | Top-bar chip reuses `useWalletQuery` |
| Virtual cash Story 4 | Compatible `staleTime: 0` + focus refetch |

#### Risk

Low — uses existing normalize/ display guards.

---

### Task 3: User-scoped portfolio query and multi-tab refetch

| Attribute | Value |
|-----------|--------|
| Spec story | Story 5 (holdings); EC-11 |
| Depends on | Task 1 |
| Estimated complexity | S |
| Parent story issue | #48 |

#### Objective

Holdings on the trading view show **zero AAPL** after reset and stale tabs recover on focus without a full logout.

#### Implementation notes

- Extract `usePortfolioQuery` mirroring `useWalletQuery`: `queryKey: ['portfolio', userId]`, `staleTime: 0`, `refetchOnWindowFocus: true`, `authApi.getPortfolio`.
- Replace inline `useQuery` in `trading-page.tsx`.
- Update `clear-user-queries.ts` if needed (prefix `['portfolio']` already removes all).
- Post-reset: invalidation refetch shows empty holdings / zero available quantity.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `web/src/features/trading/hooks/use-portfolio-query.ts` | Portfolio hook |
| MODIFY | `web/src/features/trading/pages/trading-page.tsx` | Use hook |
| MODIFY | `web/src/features/auth/clear-user-queries.ts` | Confirm orders/trades prefixes (Task 6 may finish) |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Two tabs: reset in tab A → focus tab B → holdings show 0 | `web/` |

#### Acceptance criteria

- [x] Holdings table shows 0 available AAPL after reset refetch
- [x] Focused stale tab refetches portfolio without 30s stale window

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| EC-11 | Multi-tab focus refetch |
| Redis projection | N/A — PG read |

#### Risk

Key migration from `['portfolio']` — ensure no stale callers remain.

---

### Task 4: Orders and trades query hooks

| Attribute | Value |
|-----------|--------|
| Spec story | Story 5 (activity queries) |
| Depends on | Task 1 |
| Estimated complexity | M |
| Parent story issue | #48 |

#### Objective

Add typed API clients and TanStack Query hooks for owner-scoped open orders, order history, and trade history using generated OpenAPI types where available.

#### Implementation notes

- Endpoints: `GET /api/orders/open`, `GET /api/orders/history`, `GET /api/trades` (already in `api-schema.ts`).
- Hooks: `useOpenOrdersQuery`, `useOrderHistoryQuery`, `useTradeHistoryQuery` with user-scoped keys matching Task 1.
- Same refetch policy as wallet: `staleTime: 0`, `refetchOnWindowFocus: true`, `enabled` when authenticated.
- Do not duplicate server state in Zustand.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `web/src/features/orders/api.ts` | HTTP wrappers |
| CREATE | `web/src/features/orders/hooks/use-open-orders-query.ts` | Open orders |
| CREATE | `web/src/features/orders/hooks/use-order-history-query.ts` | Order history |
| CREATE | `web/src/features/trades/api.ts` | HTTP wrappers |
| CREATE | `web/src/features/trades/hooks/use-trade-history-query.ts` | Trade history |
| REUSE | `web/src/generated/api-schema.ts` | Types/paths |
| REUSE | `src/Contracts/Orders/*.cs`, `src/Contracts/Trades/*.cs` | Field shapes |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Hooks return data before reset, empty arrays after reset | `web/` |

#### Acceptance criteria

- [x] Hooks fetch successfully on trading view mount
- [x] Keys align with `invalidatePortfolioPanels` prefixes
- [x] `yarn --cwd web build` passes

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| api-guidelines | Owner-scoped reads |
| ADR-007 | History cutoff — empty after reset |

#### Risk

None — read-only client layer.

---

### Task 5: Trading activity tabs (minimal panel)

| Attribute | Value |
|-----------|--------|
| Spec story | Story 5 (bottom panel empty states) |
| Depends on | Task 3, 4 |
| Estimated complexity | M |
| Parent story issue | #48 |

#### Objective

On the trading view, render tabbed **Open Orders**, **Order History**, **Trade History**, and **Holdings** so post-reset empty states are visible without navigating away.

#### Implementation notes

- `PortfolioActivityTabs` using shadcn `Tabs`.
- Holdings tab: reuse existing holdings table content (extract from `TradingPage` if needed).
- Other tabs: compact tables or list rows; `isPending` skeletons; empty copy (“No open orders”, “No orders yet”, “No trades yet”).
- No order placement/cancel UI in this task.
- Page composes wallet card + activity tabs grid per PRD direction (full book/chart still out of scope).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `web/src/features/trading/components/portfolio-activity-tabs.tsx` | Tabbed panel |
| MODIFY | `web/src/features/trading/pages/trading-page.tsx` | Compose panel |
| REUSE | `web/src/components/ui/tabs.tsx` | UI primitive |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | After reset, all four tabs show empty/zero within 2s | `web/` |

#### Acceptance criteria

- [x] Open orders count 0 after reset
- [x] Order history and trade history first page empty after reset
- [x] Holdings tab shows zero AAPL available
- [x] No stale pre-reset rows remain after refetch completes

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD SS8.1 | Bottom panel slice only |
| design-system | Tabs + table density |

#### Risk

Scope creep into full terminal layout — keep tabs read-only.

---

### Task 6: Polish — SignalR bridge, session cleanup, ADR-008, manual checklist

| Attribute | Value |
|-----------|--------|
| Spec story | Polish |
| Depends on | Task 2, 5 |
| Estimated complexity | S |
| Parent story issue | #48 |

#### Objective

Align realtime invalidation with panel keys, document cache strategy, extend logout cache purge, and complete manual verification for SignalR + multi-tab + wallet error path.

#### Implementation notes

- `interceptors.ts`: on `onBalanceUpdated` / `onOrderCancellationNotified`, also `invalidateQueries({ queryKey: ['trades'] })`.
- `clear-user-queries.ts`: `removeQueries` for `['orders']` and `['trades']` prefixes.
- ADR-008: post-reset client strategy — seed from 200 + invalidate/refetch; SignalR as secondary.
- Confirm backend tests still green: `ResetPortfolio_PublishesOrderCancellationNotifications` (no new server tests required).
- Run `yarn --cwd web lint && yarn --cwd web build`.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `web/src/lib/signalr/interceptors.ts` | Trades invalidation |
| MODIFY | `web/src/features/auth/clear-user-queries.ts` | Logout purge |
| MODIFY | `docs/memory/decisions.md` | ADR-008 |
| REUSE | `tests/Api.IntegrationTests/Portfolios/ResetPortfolioTests.cs` | SignalR publisher regression |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | REUSE existing reset realtime tests | `ResetPortfolioTests.cs` |
| Manual | Checklist below | `web/` |

#### Acceptance criteria

- [x] SignalR cancel/balance messages invalidate same keys as POST success
- [x] Logout clears orders/trades cache
- [x] ADR-008 recorded
- [ ] Manual checklist completed on Aspire (operator — plan §Manual UI checklist)

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| BR-11 | PortfolioResetEvent → notifications (server done) |
| Virtual cash Story 4 | Focus refetch compatible |
| ADR needed? | ADR-008 |

#### Risk

None — polish only.

---

## Reference files

| File | Why open it |
|------|-------------|
| `web/src/features/portfolio-reset/use-reset-portfolio.ts` | Story 5 TODO at invalidation site |
| `web/src/features/trading/hooks/use-wallet-query.ts` | Refetch policy template |
| `web/src/lib/signalr/interceptors.ts` | Hub → Query bridge |
| `src/Application/Portfolios/Commands/ResetPortfolioCommandHandler.cs` | Balance/cancel publishes |
| `tests/Api.IntegrationTests/Portfolios/ResetPortfolioTests.cs` | Realtime + reset regression |
| `docs/plans/20260525-240000-virtual-cash-story-4.md` | Refetch-on-focus precedent |
| `docs/plans/20260527-214600-portfolio-reset-story-3.md` | History cutoff + API reads |

## Implementation details (for /build)

### Post-reset client flow

1. `resetPortfolio()` returns `PortfolioResetResponse` with `wallet`, `resetAt`, `nextEligibleAt`.
2. `saveNextEligibleAt` (existing).
3. `seedWalletQueryData(queryClient, userId, mapResetWalletToWalletResponse(response.wallet, userId))`.
4. `await invalidatePortfolioPanels(queryClient, userId)` — use `void` + `queryClient.refetchQueries` if eager refresh needed for 2s AC; prefer `invalidateQueries` then rely on mounted observers to refetch.
5. Toast: “Portfolio reset. You're starting fresh with $100,000.”

### Query key contract (single source)

```ts
// web/src/lib/query-keys.ts — illustrative
export const queryKeys = {
  wallet: (userId: string) => ['wallet', userId] as const,
  portfolio: (userId: string) => ['portfolio', userId] as const,
  ordersOpen: (userId: string) => ['orders', 'open', userId] as const,
  ordersHistory: (userId: string) => ['orders', 'history', userId] as const,
  trades: (userId: string) => ['trades', userId] as const,
}
```

### SignalR (no server change)

| Hub method | Client interceptor | Invalidated keys |
|------------|-------------------|------------------|
| `BalanceUpdated` | `onBalanceUpdated` | `wallet`, `portfolio`, `trades` (add) |
| `OrderCancellationNotified` | `onOrderCancellationNotified` | `orders/open`, `orders/history`, `trades` (add) |

### Wallet 500 failure path

- Do not catch reset success and force display $100k without `response.wallet`.
- If `seedWalletQueryData` ran but subsequent `getWallet` refetch returns 500, `useWalletQuery` `isError` shows `WALLET_LOAD_ERROR_MESSAGE` — acceptable because user should retry; optional follow-up: keep seeded data until refetch succeeds (document in ADR-008).

### EC-10 (manual, when order form exists)

After reset refetch completes, place buy → available balance reflects $100k minus reservation.

## Verification matrix (plan-level)

| Spec AC | Verified by |
|---------|-------------|
| Cash card + chip $100k < 2s | Task 2 manual + seed from 200 |
| All panel queries invalidated | Task 1 devtools + Task 5 manual |
| SignalR user notifications | Task 6 manual + existing integration test |
| Wallet refetch 500 → error | Task 6 manual (simulate Api failure) |
| Multi-tab focus | Task 3 + Task 6 manual |
| EC-10 | Manual when order UI ships |
| No regression Stories 1–4 | Run `ResetPortfolioTests` subset |

## Manual UI checklist (Aspire)

1. Login as user with depleted cash, AAPL holdings, open orders, and history rows.
2. Reset portfolio → within 2s top-bar chip and virtual cash card show **$100,000.00** available.
3. Open Orders / Order History / Trade History tabs → empty; Holdings → 0 AAPL.
4. (Optional) Break `GET /api/wallet` temporarily → confirm error state, no JSX hardcode $100k.
5. Two browser tabs: reset in tab A → focus tab B → cash/holdings/orders match post-reset after refetch.
6. DevTools: confirm SignalR `BalanceUpdated` / `OrderCancellationNotified` while connected.

## Rollback / recovery

- **Code:** revert `feature/portfolio-reset-story-5` branch.
- **DB:** N/A — no migrations.
- **Redis:** N/A — client-only changes.

## Deferred work (Plan B)

- Full trading terminal layout (order book, chart, order form) and EC-10 automation.
- `setQueryData` for portfolio/orders from reset response (only wallet returned today).
- E2E Playwright suite for reset panel consistency.

## GitHub Links

| Local ref | Issue # | Type | Title | URL |
|-----------|---------|------|-------|-----|
| spec Story 5 | 48 | Story | US-04 / Story 5: Consistent data everywhere after reset | https://github.com/tranvuongduy2003/trading-simulator/issues/48 |
| epic | 43 | Epic | Spec: Portfolio reset (US-04) | https://github.com/tranvuongduy2003/trading-simulator/issues/43 |
