---
artifact_type: plan
artifact_version: 1
id: plan-20260530-140000-live-order-book-depth-story-1
title: Live Order Book Depth — Story 1 (initial depth table)
slug: live-order-book-depth-story-1
filename_template: 20260530-140000-live-order-book-depth-story-1.md
created_at: 2026-05-30T14:00:00+07:00
updated_at: 2026-05-30T18:00:00+07:00
status: implemented
owner: engineering
tags: [plan, implementation, trading-simulator, market-data, order-book, depth, us-06]
related_spec: docs/specs/20260530-002008-live-order-book-depth.md
related_plans: []
prd_refs: [PRD §5.2 US-06, PRD §6.2 FR-2.1, PRD §7.1, PRD §8.1, PRD §8.2]
tech_refs: [Tech §8.2, Tech §9, Tech §10.5, Tech §11]
db_refs: [DB §12.1, DB §14.2]
github:
  repo: tranvuongduy2003/trading-simulator
  epic_issue: 69
  story_issue_ids: [70]
  last_synced_at: 2026-05-30T14:00:00+07:00
search_index:
  keywords: [order book depth, depth table, GetOrderBookSnapshot, depth=10, orderCount, aggregation, AAPL, trading view, left panel, TopOfBookStrip, TanStack Query]
  bounded_contexts: [Trading]
  task_count: 5
---

# Implementation Plan: Live Order Book Depth — Story 1

| Field | Value |
|-------|--------|
| Spec | `docs/specs/20260530-002008-live-order-book-depth.md` |
| Status | IMPLEMENTED |
| Tasks | 5 / 5 |
| Branch | `feature/live-order-book-depth-story-1` |
| Aspire impact | No — reuses existing Api, PostgreSQL, Redis |
| Schema impact | No |
| Test levels | Api.IntegrationTests; Manual UI |
| ADRs required | None |
| GitHub | Synced 2026-05-30 — Story [#70](https://github.com/tranvuongduy2003/trading-simulator/issues/70); Epic [#69](https://github.com/tranvuongduy2003/trading-simulator/issues/69) |

## Executive summary

Deliver **Story 1** of US-06: authenticated users see a **multi-level order book depth table** for **AAPL** on the main trading view within **2 s**, loaded from `GET /api/market/orderbook?symbol=AAPL&depth=10`. The read path is **already implemented** for US-05 (`GetOrderBookSnapshotQuery`, Redis `orderbook:AAPL:snapshot`, PostgreSQL aggregation with `orderCount`). This plan is primarily **frontend**: depth panel UI, explicit `depth=10` on the client, shared TanStack Query cache with `TopOfBookStrip`, loading skeleton, and retryable error state. One **integration test** proves multi-level sort and price-level aggregation (BR-01). SignalR live refresh, per-side empty copy, and full PRD three-column layout are deferred to Stories 2–4 ([#71](https://github.com/tranvuongduy2003/trading-simulator/issues/71)–[#73](https://github.com/tranvuongduy2003/trading-simulator/issues/73)).

## Goals and non-goals

**Goals**

- G1: Trading view shows bid and ask depth tables (up to **10** rows per side) with **Price**, **Size**, and **Orders** columns and accessible labels.
- G2: Client requests `GET /api/market/orderbook?symbol=AAPL&depth=10` via existing `useOrderBookQuery` cache key `['market', 'orderbook', 'AAPL']` (feeds strip + depth per spec §13 Q3).
- G3: Prove aggregation and sort with Api integration tests; manual Aspire checklist for 2 s load and BR-08 strip/depth consistency.

**Non-goals** (this plan will not do)

- NG1: SignalR `OrderBookUpdated` handling or ≤500 ms live refresh (**Story 2**, #71).
- NG2: “No bids” / “No asks” / “No market” empty-state copy in depth panel (**Story 3**, #72) — depth may render zero rows until Story 3; strip keeps US-05 Story 4 behavior.
- NG3: Full PRD §8.1 three-column desktop layout (left / center chart / right form) (**Story 4**, #73) — Story 1 adds a functional depth block on `TradingPage`, not the final 1280px grid.
- NG4: Matching engine changes, new PostgreSQL tables, or Redis write path changes.
- NG5: Row-click pre-fill, cumulative depth, configurable N, multi-symbol.

## Traceability matrix

| Spec story | Plan task(s) | Test evidence |
|------------|--------------|---------------|
| Story 1 — see depth on trading view | Tasks 1–5 | `GetOrderBookSnapshot_*` (existing + new multi-level/aggregation); manual trading view checklist |
| Stories 2–4 | — | Deferred (#71–#73) |

## Architecture impact

```text
┌─────────────────────────────────────────────────────────────────────────┐
│  web/ TradingPage                                                        │
│    useOrderBookQuery()  ──GET /api/market/orderbook?symbol=AAPL&depth=10│
│         │                    (shared queryKey)                           │
│         ├─► TopOfBookStrip (US-05) — deriveTopOfBook(bids[0], asks[0])   │
│         └─► OrderBookDepthPanel (NEW) — render bids[] / asks[] rows        │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│  Api: MarketEndpoint → GetOrderBookSnapshotQuery (depth default 10)      │
│  Infrastructure: OrderBookSnapshotReadRepository                         │
│    Redis GET orderbook:AAPL:snapshot → else PG GroupBy price (BR-01)   │
└─────────────────────────────────────────────────────────────────────────┘
         Story 2+ only: SignalR OrderBookUpdated → setQueryData (same key)
```

| Layer | Change summary |
|-------|----------------|
| Domain | None |
| Application | None (reuse `GetOrderBookSnapshotQuery` / handler) |
| Infrastructure | None (aggregation + sort already in `OrderBookSnapshotReadRepository`) |
| Api | None (endpoint + `depth` query param exist) |
| MatchingEngine | None |
| web/ | `getOrderBook` adds `depth=10`; `OrderBookDepthPanel` + table subcomponents; mount on `trading-page`; optional `order-book-depth-display.ts` formatters |
| AppHost | None |

## Data & migration plan

| Artifact | Action | DB reference |
|----------|--------|--------------|
| EF migration | None | — |
| Redis keys | Read-only `orderbook:AAPL:snapshot` | DB §12.1 |
| Book recovery | N/A — PG fallback on cache miss | DB §12.2, §14.2 |

## Open questions

| # | Question | Source | Answer | Status |
|---|----------|--------|--------|--------|
| 1 | Side-by-side vs stacked bid/ask layout? | Spec §13 Q1 | Side-by-side ≥`lg`; stacked below | ✅ (implement in Task 2) |
| 2 | Shared TanStack cache with US-05? | Spec §13 Q3 | Same `['market', 'orderbook', 'AAPL']` | ✅ |
| 3 | Row flash on quantity change? | Spec §13 Q4 | Optional; not required for Story 1 AC | ⏳ Deferred (Story 2) |
| 4 | Depth panel empty copy in Story 1? | Story 1 vs Story 3 | Show **rows only** when levels exist; no Story 3 marketing copy yet | ✅ |

## Risks and mitigations

| Risk | Likelihood | Impact | Mitigation | Owner task |
|------|------------|--------|------------|------------|
| Strip and depth diverge (BR-08) | Low | Medium | Both use same snapshot; `deriveTopOfBook` uses `bids[0]`/`asks[0]`; manual + integration asserts first levels | Task 4 |
| Client omits `depth=10` and gets wrong cap | Low | Low | Task 1 adds explicit query param; integration test uses `?depth=10` | Task 1, 3 |
| No multi-level integration coverage today | Medium | Medium | Task 3 seeds ≥3 levels per side + aggregation case | Task 3 |
| Trading page layout still single column | Medium | Low | Accept for Story 1; Story 4 refines zones | — |

## Prerequisites

- [x] US-05 Story 1–4 implemented on feature branches (snapshot API, strip, SignalR, empty strip copy)
- [ ] Spec promoted to `approved` (currently `draft` — implement against AC; confirm with product if blocking)
- [ ] Aspire local stack runs (`aspire run` or env-doctor)
- [ ] Merge or rebase US-05 market work onto `main` / story branch before `/build` if not already on `main`

## File structure (planned)

```text
web/src/features/market/
  api.ts                              MODIFY  depth=10 query param
  components/
    top-of-book-strip.tsx             REUSE   loading/error patterns
    order-book-depth-panel.tsx        CREATE  card + skeleton/error/retry
    order-book-depth-table.tsx        CREATE  bid/ask tables
  order-book-depth-display.ts         CREATE  format price/qty/count (optional)
  hooks/use-order-book-query.ts       REUSE   no key change
web/src/features/trading/pages/
  trading-page.tsx                    MODIFY  mount depth panel
tests/Api.IntegrationTests/Market/
  GetOrderBookSnapshotTests.cs        MODIFY  multi-level + aggregation tests
  MarketTestHelpers.cs                MODIFY  seed helpers if needed
```

## Authorization, session, and domain notes

- **Session model:** Cookie session; `ProtectedRoute` redirects unauthenticated users to login (`web/src/app/routes/protected-route.tsx`).
- **Route protection:** `/trading` under `ProtectedRoute`; `useOrderBookQuery` has `enabled: authStatus === 'authenticated'`.
- **Domain rules (must not violate):**
  - BR-01: Levels aggregated by price; `orderCount` = count of open orders at price.
  - BR-02: Max **10** levels per side (`depth` param).
  - BR-03: Bids desc, asks asc (server-enforced; UI displays array order as returned).
  - BR-04: Integer quantities; prices up to 4 decimals (`formatPrice` / tabular-nums).
  - BR-06: AAPL only — already enforced in handler.
  - BR-08: Best bid/ask in strip must match first depth row when both visible.

## Progress tracker

### Task 1: Skeleton — depth API param and panel shell on trading view

| Attribute | Value |
|-----------|--------|
| Spec story | Story 1 |
| Depends on | None |
| Estimated complexity | S |
| Parent story issue | [#70](https://github.com/tranvuongduy2003/trading-simulator/issues/70) |

#### Objective

End-to-end slice: trading page mounts an **Order book depth** panel that shares `useOrderBookQuery`, shows loading skeleton and error+Retry (no fabricated rows), and calls `GET /api/market/orderbook?symbol=AAPL&depth=10`.

#### Implementation notes

- Extend `getOrderBook(symbol, signal)` to append `depth=10` in `URLSearchParams`.
- Add `OrderBookDepthPanel` with props mirroring strip: `isPending`, `isError`, `snapshot`, `onRetry` — body can be placeholder text until Task 2.
- Mount panel on `TradingPage` below or beside `TopOfBookStrip` (simple vertical stack acceptable for Story 1).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `web/src/features/market/api.ts` | `depth=10` query param |
| CREATE | `web/src/features/market/components/order-book-depth-panel.tsx` | Shell: title AAPL, skeleton, error, retry |
| MODIFY | `web/src/features/trading/pages/trading-page.tsx` | Wire panel to `orderBookQuery` |
| REUSE | `web/src/features/market/hooks/use-order-book-query.ts` | Same query key |
| REUSE | `web/src/features/market/components/top-of-book-strip.tsx` | Error copy pattern (`MARKET_LOAD_ERROR_MESSAGE`) |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Logged in → panel shows skeleton then data or error | `web/` |
| Integration | Existing auth/empty tests still pass (no regression) | `GetOrderBookSnapshotTests.cs` |

#### Acceptance criteria

- [x] Network tab shows `GET .../orderbook?symbol=AAPL&depth=10` on trading view load
- [x] Unauthenticated `/trading` redirects to login (no depth fetch)
- [x] Panel shows skeleton while pending; error + Retry on failure without numeric rows

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | FR-2.1 depth 10; Tech §10.5 read path; DB §12.1 |
| Async matching | N/A — read-only |
| PostgreSQL authoritative | Fallback unchanged |
| Redis projection | Read-only |
| RFC 7807 errors | Reuse Api client / suppressErrorToast |
| SignalR | N/A Story 1 |
| Aspire | None |
| ADR needed? | No |

#### Risk

None — isolated UI shell; backend unchanged.

---

### Task 2: Depth table — bid/ask rows with accessible columns

| Attribute | Value |
|-----------|--------|
| Spec story | Story 1 |
| Depends on | Task 1 |
| Estimated complexity | M |
| Parent story issue | [#70](https://github.com/tranvuongduy2003/trading-simulator/issues/70) |

#### Objective

Render up to **10** bid and **10** ask rows with **Price**, **Size**, and **Orders** column headers; bids highest-first and asks lowest-first (trust API order); side-by-side on `lg+`, stacked on smaller viewports; `text-bid` / `text-ask` with text labels “Bids” / “Asks”.

#### Implementation notes

- Use shadcn `Table` (`web/src/components/ui/table.tsx`); `tabular-nums` on numeric cells.
- Reuse `formatPrice` from `@/features/market/format-price` for prices; integer quantity formatting (optional thousands separator).
- EC-03: map `snapshot.bids` / `snapshot.asks` directly — no padding rows.
- Do **not** add Story 3 empty-state banners in depth panel.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `web/src/features/market/components/order-book-depth-table.tsx` | Side table component |
| CREATE | `web/src/features/market/order-book-depth-display.ts` | Row format helpers (optional) |
| MODIFY | `web/src/features/market/components/order-book-depth-panel.tsx` | Compose tables |
| REUSE | `web/src/features/market/format-price.ts` | Price display |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | 3+ levels per side visible; columns labeled; bid green / ask red with text headers | `web/` |

#### Acceptance criteria

- [x] Depth table shows price, quantity, orderCount per row when snapshot has levels
- [x] At most 10 rows per side
- [x] Column headers are visible text (not color-only)
- [x] Layout: side-by-side bids/asks at `lg`, stacked below `lg`

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | PRD §8.2 tabular alignment; BR-03 display order |
| design-system.mdc | `text-bid` / `text-ask`, Skeleton, Card |
| frontend.mdc | No Zustand for server book state |

#### Risk

Low — presentational; snapshot shape already typed.

---

### Task 3: Integration tests — multi-level sort and aggregation

| Attribute | Value |
|-----------|--------|
| Spec story | Story 1 |
| Depends on | Task 1 |
| Estimated complexity | M |
| Parent story issue | [#70](https://github.com/tranvuongduy2003/trading-simulator/issues/70) |

#### Objective

Automated proof of BR-01, BR-02, BR-03: snapshot returns multiple sorted levels and aggregates quantity + `orderCount` at the same price.

#### Implementation notes

- Add `GetOrderBookSnapshot_WithMultipleBidAndAskLevels_ReturnsSortedTopTen` — seed ≥3 bids (e.g. 150.10, 150.15, 150.20) and ≥3 asks; assert desc/asc order and `?depth=10`.
- Add `GetOrderBookSnapshot_AggregatesOrdersAtSamePrice` — two bids at 150.25 (100 + 200) → single level quantity 300, orderCount 2.
- Extend `MarketTestHelpers` if a reusable multi-seed helper reduces duplication.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `tests/Api.IntegrationTests/Market/GetOrderBookSnapshotTests.cs` | New facts |
| MODIFY | `tests/Api.IntegrationTests/Market/MarketTestHelpers.cs` | Optional seed helpers |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | Multi-level sort | `GetOrderBookSnapshotTests.cs` |
| Integration | Same-price aggregation | `GetOrderBookSnapshotTests.cs` |
| Integration | `depth=10` with >10 seeded levels returns cap 10 | `GetOrderBookSnapshotTests.cs` (optional) |

#### Acceptance criteria

- [x] All new tests pass with Docker Testcontainers
- [x] Existing `GetOrderBookSnapshot_*` tests unchanged (green)

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | BR-01–03; DB §14.2 `ix_orders_active_book` |
| backend-testing.mdc | No domain mocks |

#### Risk

None — tests only.

---

### Task 4: BR-08 consistency — strip touch matches depth first row

| Attribute | Value |
|-----------|--------|
| Spec story | Story 1 |
| Depends on | Task 2 |
| Estimated complexity | S |
| Parent story issue | [#70](https://github.com/tranvuongduy2003/trading-simulator/issues/70) |

#### Objective

Document and verify that `TopOfBookStrip` best bid/ask prices and sizes match the first row of the depth table from the same snapshot.

#### Implementation notes

- Code path already aligned: `deriveTopOfBook` uses `snapshot.bids[0]` / `asks[0]`.
- Add integration test `GetOrderBookSnapshot_FirstLevelsMatchTopOfBookSemantics` OR extend seeded test with assertions on first level vs expected best prices.
- Manual checklist item: compare strip vs depth first row after seeding.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `tests/Api.IntegrationTests/Market/GetOrderBookSnapshotTests.cs` | BR-08 assertion |
| REUSE | `web/src/features/market/top-of-book-display.ts` | `deriveTopOfBook` |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | First bid/ask levels are best prices | `GetOrderBookSnapshotTests.cs` |
| Manual | Strip and depth first rows match | `web/` |

#### Acceptance criteria

- [x] Integration test proves highest bid and lowest ask are index 0
- [ ] Manual: strip prices equal depth row 1 prices when both visible

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| Spec | BR-08 |

#### Risk

None.

---

### Task 5: Polish — manual checklist, lint/build, OpenAPI verify

| Attribute | Value |
|-----------|--------|
| Spec story | Story 1 \| Polish |
| Depends on | Tasks 1–4 |
| Estimated complexity | S |
| Parent story issue | [#70](https://github.com/tranvuongduy2003/trading-simulator/issues/70) |

#### Objective

Ship-ready Story 1: `yarn lint`, `yarn build`, `api:verify` (no contract change expected), manual Aspire checklist, update memory only if product approves spec.

#### Implementation notes

- OpenAPI already documents `depth` — run `yarn --cwd web api:verify` to confirm no drift.
- No OpenAPI export unless Api surface changes.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| — | CI scripts | lint/build/verify |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Full Story 1 AC matrix below | Aspire |
| Integration | Full `GetOrderBookSnapshotTests` suite | Docker |

#### Acceptance criteria

- [ ] `dotnet test tests/Api.IntegrationTests` market tests green (Docker not running locally — compile verified)
- [x] `yarn --cwd web lint` and `yarn --cwd web build` green
- [ ] Manual checklist (§Manual UI checklist) signed off by operator

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| openapi-contract-sync | verify only |
| core.mdc | No XML docs; no Trading prefix |

#### Risk

None.

---

## Reference files

| File | Why open it |
|------|-------------|
| `docs/plans/20260529-120000-best-bid-ask-story-1.md` | Prior market snapshot plan pattern |
| `web/src/features/market/components/top-of-book-strip.tsx` | Loading/error/retry UX |
| `web/src/features/trading/pages/trading-page.tsx` | Mount point |
| `src/Infrastructure/Persistence/Repositories/OrderBookSnapshotReadRepository.cs` | BR-01 aggregation |
| `tests/Api.IntegrationTests/Market/GetOrderBookSnapshotTests.cs` | Extend tests |
| `docs/specs/20260530-002008-live-order-book-depth.md` | Story 1 AC |

## Implementation details (for /build)

**HTTP:** No Api changes. Client must call `/api/market/orderbook?symbol=AAPL&depth=10`.

**DTO (existing):** `OrderBookLevelResponse { price, quantity, orderCount }`; arrays sorted server-side.

**React:**

- Keep `queryKey: ['market', 'orderbook', 'AAPL']`.
- Pass full `OrderBookSnapshotResponse` to `OrderBookDepthPanel`; strip keeps `deriveTopOfBook`.
- Error message: reuse `MARKET_LOAD_ERROR_MESSAGE` from `top-of-book-display.ts` or extract shared `market-load-error.ts` if duplication annoys lint.
- Auth: no change — `ProtectedRoute` + query `enabled`.

**Integration seeding:** Use `MarketTestHelpers.SeedOpenBidAsync` / `SeedOpenAskAsync` at distinct prices; for aggregation, call seed twice at same price before `ClearOrderBookSnapshotCacheAsync`.

**Deferred hub work (Story 2):** `order-book-from-realtime.ts` already maps hub payloads — depth panel will pick up live updates when Story 2 wires `setQueryData` (no Task 1–5 change required if key unchanged).

## Verification matrix (plan-level)

| Spec AC | Verified by |
|---------|-------------|
| 3+ bid/ask levels within 2 s | Task 3 integration + manual Aspire |
| Row shows price, qty 300, orderCount 2 | Task 3 aggregation test + manual |
| Bid desc / ask asc | Task 3 sort test |
| Unauthenticated → login, no depth | `ProtectedRoute` + existing `GetOrderBookSnapshot_RequiresAuthentication` |
| Snapshot failure → error + Retry, no fake rows | Task 1 panel + manual |
| EC-03 fewer than 10 levels | Task 2 maps arrays only + manual thin book |
| BR-08 strip vs depth | Task 4 |

## Manual UI checklist

Run on Aspire with authenticated user and seeded book (≥3 levels per side):

- [ ] Navigate to `/trading` — depth panel title mentions **AAPL**; data within **2 s**
- [ ] Bid section shows highest price at top; ask section lowest at top
- [ ] Columns **Price**, **Size**, **Orders** (or equivalent) readable with screen reader / visible headers
- [ ] Strip best bid/ask match first depth rows (prices and sizes)
- [ ] Stop Api — panel shows error + **Retry**; no placeholder prices
- [ ] Log out — redirected; log in — no stale depth from prior session (cache purge via `clear-user-queries.ts`)
- [ ] `yarn lint` + `yarn build` green

## Rollback / recovery

- **Code:** revert branch commits
- **DB:** N/A
- **Redis:** N/A for Story 1 reads

## Deferred work (Plan B)

- Story 2 plan: `docs/plans/<timestamp>-live-order-book-depth-story-2.md` — SignalR depth refresh (#71)
- Story 3: per-side empty states in depth panel (#72)
- Story 4: PRD §8.1 three-column trading layout (#73)
- Optional row highlight on `OrderBookUpdated` (spec §13 Q4)

## GitHub Links

| Local ref | Issue # | Type | Title | URL |
|-----------|---------|------|-------|-----|
| spec Story 1 | 70 | Story | US-06 / Story 1: See order book depth on trading view | https://github.com/tranvuongduy2003/trading-simulator/issues/70 |
| spec epic | 69 | Epic | US-06 epic | https://github.com/tranvuongduy2003/trading-simulator/issues/69 |
