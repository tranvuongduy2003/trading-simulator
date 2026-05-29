---
artifact_type: plan
artifact_version: 1
id: plan-20260530-210000-live-order-book-depth-story-2
title: Live Order Book Depth — Story 2 (real-time depth updates)
slug: live-order-book-depth-story-2
filename_template: 20260530-210000-live-order-book-depth-story-2.md
created_at: 2026-05-30T21:00:00+07:00
updated_at: 2026-05-30T21:00:00+07:00
status: approved
owner: engineering
tags: [plan, implementation, trading-simulator, market-data, order-book, depth, signalr, us-06]
related_spec: docs/specs/20260530-002008-live-order-book-depth.md
related_plans: [docs/plans/20260530-140000-live-order-book-depth-story-1.md, docs/plans/20260529-203000-best-bid-ask-story-2.md]
prd_refs: [PRD §5.2 US-06, PRD §6.2 FR-2.1, PRD §7.1, PRD §8.2]
tech_refs: [Tech §9, Tech §10.5, Tech §11]
db_refs: [DB §12.1, DB §14.2]
github:
  repo: tranvuongduy2003/trading-simulator
  epic_issue: 69
  story_issue_ids: [71]
  last_synced_at: 2026-05-30T21:00:00+07:00
search_index:
  keywords: [order book depth, OrderBookUpdated, setQueryData, SignalR, reconnect, level removal, ghost row, orderCount, TanStack Query, AAPL, realtime, hub payload, BR-08]
  bounded_contexts: [Trading]
  task_count: 5
---

# Implementation Plan: Live Order Book Depth — Story 2

| Field | Value |
|-------|--------|
| Spec | `docs/specs/20260530-002008-live-order-book-depth.md` |
| Status | APPROVED |
| Tasks | 5 |
| Branch | `feature/live-order-book-depth-story-2` |
| Aspire impact | No — reuses existing Api hub + read path |
| Schema impact | No |
| Test levels | Api.IntegrationTests (SignalR depth); Manual UI |
| ADRs required | None |
| GitHub | Synced 2026-05-30 — Story [#71](https://github.com/tranvuongduy2003/trading-simulator/issues/71); Epic [#69](https://github.com/tranvuongduy2003/trading-simulator/issues/69) |

## Executive summary

Deliver **Story 2** of US-06: the **depth table** updates in place when the order book projection changes, without a page reload. US-05 Story 2 already wires `OrderBookUpdated` → `setQueryData` on `['market','orderbook','AAPL']`, and Story 1 mounts `OrderBookDepthPanel` on that same query — so the vertical slice is mostly **complete**. This plan closes the gaps: **reconnect badge on the depth panel**, **hub payload parity** (`orderCount` + no zero-qty levels), **client-side normalization** (EC-12 dedupe), **integration tests** for multi-level hub updates and level removal, and a **manual checklist** proving ≤500 ms depth refresh and BR-08 strip/depth consistency after live updates.

## Goals and non-goals

**Goals**

- G1: Depth rows add, update, or remove within **500 ms** when `OrderBookUpdated` arrives (shared TanStack cache; no `invalidateQueries` on live messages).
- G2: Zero-quantity levels never render (server omits them; client filters defensively).
- G3: Hub disconnect **>5 s** shows **Reconnecting…** on depth panel (or shared badge); reconnect refetches snapshot within **2 s** (reuse US-05 reconnect path).
- G4: Hub payload carries **`orderCount`** per level so the Orders column stays accurate after live updates (BR-08 parity with HTTP snapshot).
- G5: Integration test proves multi-level hub payload and level removal; manual Aspire checklist for depth-specific AC.

**Non-goals** (this plan will not do)

- NG1: Per-side empty-state copy in depth panel (**Story 3**, #72).
- NG2: PRD §8.1 three-column layout (**Story 4**, #73).
- NG3: Matching engine auto-publish on every match (engine consumer hook remains future work; tests use `IOrderBookMarketDataNotifier`).
- NG4: Optional row flash on quantity change (spec §13 Q4 — deferred).
- NG5: New REST endpoints or PostgreSQL schema changes.

## Traceability matrix

| Spec story | Plan task(s) | Test evidence |
|------------|--------------|---------------|
| Story 2 — real-time depth updates | Tasks 1–5 | `OrderBookUpdated_*` depth tests; manual checklist |
| Story 1 regression | Task 5 | Existing `GetOrderBookSnapshot_*` suite |
| Stories 3–4 | — | Deferred (#72, #73) |

## Architecture impact

```text
┌─────────────────────────────────────────────────────────────────────────┐
│  web/ TradingPage                                                        │
│    useOrderBookQuery()  ── shared key ['market','orderbook','AAPL']      │
│         │                                                                │
│         ├─► TopOfBookStrip — deriveTopOfBook(bids[0], asks[0])           │
│         └─► OrderBookDepthPanel — render bids[] / asks[] (Story 1)       │
│                                                                          │
│  useSimulationHub + interceptors.ts                                      │
│    onOrderBookUpdated → mapOrderBookUpdatedToSnapshot → setQueryData     │
│    on reconnected → invalidateQueries (refetch depth=10)                 │
└─────────────────────────────────────────────────────────────────────────┘
                                    ▲
                                    │ SignalR OrderBookUpdated (depth 10)
┌───────────────────────────────────┴─────────────────────────────────────┐
│  OrderBookMarketDataNotifier — read snapshot depth=10 → publish          │
│  (extend OrderBookLevelMessage with orderCount)                          │
└─────────────────────────────────────────────────────────────────────────┘
```

| Layer | Change summary |
|-------|----------------|
| Domain | None |
| Application | `OrderBookMarketDataNotifier.MapLevel` includes `OrderCount` |
| Infrastructure | None (read repo unchanged) |
| Api | None (hub fan-out unchanged) |
| MatchingEngine | None |
| web/ | Depth reconnect badge; mapper filters/dedupes; optional shared badge component |
| Contracts | `OrderBookLevelMessage` adds `OrderCount` |
| AppHost | None |

## Data & migration plan

| Artifact | Action | DB reference |
|----------|--------|--------------|
| EF migration | None | — |
| Redis keys | Read-only `orderbook:AAPL:snapshot` | DB §12.1 |
| SignalR contract | Add `OrderCount` to `OrderBookLevelMessage` | Tech §9 |
| Book recovery | N/A | DB §12.2 |

## Open questions

| # | Question | Source | Answer | Status |
|---|----------|--------|--------|--------|
| 1 | Reconnect badge on depth vs strip only? | Spec Story 2 failure AC | **Both panels** show badge (same `useMarketConnectionStatus` prop) | ✅ |
| 2 | Hub missing `orderCount` — show 0 or preserve prior? | US-05 Story 2 mapper | **Extend contract** — publish `orderCount` from read model (BR-08) | ✅ |
| 3 | Client filter zero-qty if server sends them? | EC-05 | **Filter in mapper** before `setQueryData` | ✅ |
| 4 | Duplicate price in hub array (EC-12)? | Spec §9 | **Dedupe by price** — last occurrence wins in mapper | ✅ |
| 5 | Row flash on change? | Spec §13 Q4 | **Deferred** — not required for Story 2 AC | ⏳ |

## Risks and mitigations

| Risk | Likelihood | Impact | Mitigation | Owner task |
|------|------------|--------|------------|------------|
| Depth already updates but untested | Medium | Medium | Task 1 manual smoke + Task 3 integration test | Task 1, 3 |
| `orderCount` shows 0 after every hub push | High | Medium | Task 2 extends `OrderBookLevelMessage` | Task 2 |
| Reconnect badge only on strip | Medium | Low | Task 1 wires badge to depth panel header | Task 1 |
| SignalR contract drift (C# vs TS) | Low | Medium | Task 2 updates both; run `api:verify` if OpenAPI touched (SignalR-only — manual sync) | Task 2 |
| Engine not auto-publishing in dev | High | Low | Manual test via notifier seed + integration tests | Task 5 |

## Prerequisites

- [x] US-06 Story 1 implemented (`OrderBookDepthPanel`, `depth=10`, integration tests) — branch `feature/live-order-book-depth-story-1`
- [x] US-05 Story 2 implemented (`setQueryData`, reconnect refetch, `OrderBookMarketDataNotifier`) — on `main` or feature branches
- [ ] Story 1 PR merged or rebased onto story-2 branch before `/build`
- [ ] Aspire local stack runs
- [ ] Spec `status: draft` — implement against AC; product approval not blocking for engineering plan

## File structure (planned)

```text
src/Contracts/Realtime/
  OrderBookUpdatedMessage.cs                 MODIFY  OrderCount on OrderBookLevelMessage
src/Application/Market/
  OrderBookMarketDataNotifier.cs             MODIFY  map OrderCount
web/src/
  types/realtime.ts                          MODIFY  orderCount on OrderBookLevel
  features/market/
    order-book-from-realtime.ts              MODIFY  map + filter + dedupe
    components/order-book-depth-panel.tsx    MODIFY  reconnect badge
  features/trading/pages/trading-page.tsx    MODIFY  pass showReconnectingBadge to depth
tests/Api.IntegrationTests/Market/
  OrderBookUpdatedSignalRTests.cs            MODIFY  multi-level + level removal tests
```

## Authorization, session, and domain notes

- **Session model:** Hub + snapshot require authenticated session (unchanged).
- **Route protection:** `/trading` behind `ProtectedRoute`; hub starts when `authStatus === 'authenticated'`.
- **Domain rules (must not violate):**
  - BR-05: Client applies hub message immediately via `setQueryData`; server publish reads latest projection (`NotifyOrderBookChangedAsync`).
  - BR-08: Strip touch and depth row 0 must match after every cache update (HTTP or hub).
  - BR-02: Render only provided levels — no zero padding (EC-03).
  - EC-05: Levels with aggregate quantity 0 must not appear in UI arrays.

## Progress tracker

### Task 1: Skeleton — depth panel live wiring and reconnect badge

| Attribute | Value |
|-----------|--------|
| Spec story | Story 2 |
| Depends on | None |
| Estimated complexity | S |
| Parent story issue | [#71](https://github.com/tranvuongduy2003/trading-simulator/issues/71) |

#### Objective

Confirm the depth table **reactively updates** from the existing `setQueryData` bridge (no page reload), and surface **Reconnecting…** on the depth panel when the hub is disconnected **>5 s** (same signal as the strip).

#### Implementation notes

- Pass `showReconnectingBadge` from `TradingPage` into `OrderBookDepthPanel` (mirror `TopOfBookStrip` header badge pattern).
- Optional: extract a tiny `MarketReconnectBadge` in `web/src/features/market/components/` if duplication triggers lint — keep inline if ≤10 lines duplicated.
- Do **not** add a second hub subscription or query key.
- Smoke: after Task 1, hub `setQueryData` should re-render depth without code changes to interceptors (verify in manual step).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `web/src/features/market/components/order-book-depth-panel.tsx` | Reconnect badge in card header |
| MODIFY | `web/src/features/trading/pages/trading-page.tsx` | Pass `showReconnectingBadge` |
| REUSE | `web/src/features/market/hooks/use-market-connection-status.ts` | 5 s threshold |
| REUSE | `web/src/lib/signalr/interceptors.ts` | Existing `setQueryData` |
| REUSE | `web/src/hooks/use-simulation-hub.ts` | Reconnect refetch |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Depth row count changes after notifier push without reload | Aspire |
| Manual | Stop Api 6 s — badge on **depth** panel; last rows visible | Aspire |

#### Acceptance criteria

- [x] Depth panel header shows **Reconnecting…** after 5 s hub disconnect (matches strip behavior)
- [x] During disconnect, last depth rows remain visible (not cleared to skeleton)
- [x] On Api restart, depth refetches and badge clears within **2 s** (manual — operator)

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD §7.1 | Live updates via cache, not full refetch |
| SignalR | Reuse US-05 reconnect refetch |
| ADR needed? | No |

#### Risk

None — UI wiring only.

---

### Task 2: Hub payload parity — orderCount and client normalization

| Attribute | Value |
|-----------|--------|
| Spec story | Story 2 |
| Depends on | Task 1 |
| Estimated complexity | M |
| Parent story issue | [#71](https://github.com/tranvuongduy2003/trading-simulator/issues/71) |

#### Objective

`OrderBookUpdated` payloads match HTTP snapshot semantics: each level includes **`orderCount`**, excludes **zero-quantity** rows, and **dedupes** duplicate prices (EC-12) before updating TanStack cache.

#### Implementation notes

- Extend `OrderBookLevelMessage` to `(decimal Price, decimal Quantity, int OrderCount)` in Contracts.
- Update `OrderBookMarketDataNotifier.MapLevel` to pass `level.OrderCount`.
- Update `web/src/types/realtime.ts` — add `orderCount: number` to `OrderBookLevel`.
- Refactor `mapOrderBookUpdatedToSnapshot`:
  - Map `orderCount` from hub level (default `0` only if field absent for backward compat during rollout).
  - Filter `quantity > 0`.
  - Dedupe by price per side (keep last entry when duplicates exist).
  - Preserve sort order from server (bids desc, asks asc — do not re-sort client-side unless tests prove hub unsorted).
- No OpenAPI change (SignalR-only); keep TS/C# records aligned manually.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `src/Contracts/Realtime/OrderBookUpdatedMessage.cs` | Add OrderCount |
| MODIFY | `src/Application/Market/OrderBookMarketDataNotifier.cs` | Map OrderCount |
| MODIFY | `web/src/types/realtime.ts` | Type parity |
| MODIFY | `web/src/features/market/order-book-from-realtime.ts` | Map, filter, dedupe |
| REUSE | `src/Infrastructure/Persistence/Repositories/OrderBookSnapshotReadRepository.cs` | Source of orderCount |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Unit (optional) | Pure mapper: filters qty 0, dedupes price | `order-book-from-realtime.ts` — manual only for MVP unless trivial vitest added (skip unless requested) |
| Integration | Hub message includes orderCount > 0 | Task 3 |

#### Acceptance criteria

- [x] After hub update, Orders column shows non-zero counts when aggregation has multiple orders
- [x] Levels with quantity 0 never render after mapper runs
- [x] Duplicate price in payload → single row in UI

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| BR-08 | Hub levels match HTTP snapshot fields |
| Contracts | Keep `ISimulationHubClient` payload in sync |
| ADR needed? | No |

#### Risk

Low — additive field on SignalR record; existing clients ignore extra JSON fields.

---

### Task 3: Integration tests — multi-level depth and level removal via hub

| Attribute | Value |
|-----------|--------|
| Spec story | Story 2 |
| Depends on | Task 2 |
| Estimated complexity | M |
| Parent story issue | [#71](https://github.com/tranvuongduy2003/trading-simulator/issues/71) |

#### Objective

Automated proof that subscribed clients receive **full depth arrays** (not touch-only) and that a **removed price level** disappears from the hub payload after book state changes.

#### Implementation notes

- Extend `OrderBookUpdatedSignalRTests.cs`:
  1. **`OrderBookUpdated_PublishesMultipleBidLevels`** — seed bids at 150.20, 150.25, 150.30; notify; assert payload has ≥3 bids sorted desc with correct prices.
  2. **`OrderBookUpdated_RemovedLevelNotInPayload`** — seed bid at 150.25; notify; cancel/remove that order (use existing seed helpers + status update or cancel flow); notify; assert no bid at 150.25 and quantity levels all > 0.
  3. **`OrderBookUpdated_IncludesOrderCountOnAggregatedLevel`** — two bids same price → orderCount 2, quantity sum in payload.
- Reuse `MarketTestHelpers.WaitUntilAsync` — no `Thread.Sleep`.
- Assert `message.Bids.All(b => b.Quantity > 0)` after Task 2 server-side guarantees.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `tests/Api.IntegrationTests/Market/OrderBookUpdatedSignalRTests.cs` | New facts |
| MODIFY | `tests/Api.IntegrationTests/Market/MarketTestHelpers.cs` | Helper to cancel/remove seeded order if needed |
| REUSE | `tests/Api.IntegrationTests/Market/GetOrderBookSnapshotTests.cs` | Seed patterns |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | Multi-level bids in hub payload | `OrderBookUpdatedSignalRTests.cs` |
| Integration | Removed level absent after cancel | `OrderBookUpdatedSignalRTests.cs` |
| Integration | Aggregated orderCount in hub | `OrderBookUpdatedSignalRTests.cs` |

#### Acceptance criteria

- [x] All new tests pass with Docker Testcontainers
- [x] Existing `OrderBookUpdated_AfterTouchChange_ReceivesNewBestBid` still passes

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| Tech §17 | Real hub, not mocked publisher |
| EC-04–EC-07 | Level removal test covers EC-05 |
| ADR needed? | No |

#### Risk

Cancel/remove helper may need new test utility — keep scoped to market helpers.

---

### Task 4: BR-08 live consistency — strip touch matches depth row 0 after hub update

| Attribute | Value |
|-----------|--------|
| Spec story | Story 2 |
| Depends on | Tasks 2–3 |
| Estimated complexity | S |
| Parent story issue | [#71](https://github.com/tranvuongduy2003/trading-simulator/issues/71) |

#### Objective

After a hub-driven cache update, **top-of-book strip** prices/sizes equal **first depth row** on each side (same snapshot object in React).

#### Implementation notes

- Code path already shares one `orderBookQuery.data` — verify `deriveTopOfBook` uses `bids[0]`/`asks[0]` after mapper update.
- Add integration assertion in Task 3 test: hub payload `bids[0]` matches HTTP `GET /api/market/orderbook?depth=10` first bid after same seed + notify.
- Manual checklist item: change touch via hub → compare strip vs depth row 1.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `tests/Api.IntegrationTests/Market/OrderBookUpdatedSignalRTests.cs` | Hub vs HTTP parity assertion |
| REUSE | `web/src/features/market/top-of-book-display.ts` | `deriveTopOfBook` |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | Hub first level matches HTTP snapshot touch | `OrderBookUpdatedSignalRTests.cs` |
| Manual | Strip and depth first rows match after live update | Aspire |

#### Acceptance criteria

- [x] Integration test compares hub message to REST snapshot for same DB state
- [ ] Manual: strip bid price === depth bid row 1 price after notifier push (operator)

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| BR-08 | Single cache source |

#### Risk

None.

---

### Task 5: Polish — manual checklist, regression, lint/build

| Attribute | Value |
|-----------|--------|
| Spec story | Story 2 \| Polish |
| Depends on | Tasks 1–4 |
| Estimated complexity | S |
| Parent story issue | [#71](https://github.com/tranvuongduy2003/trading-simulator/issues/71) |

#### Objective

Story 2 ship-ready: full market test suite green, frontend lint/build, manual Aspire checklist signed off, GitHub #71 task checklist updated on PR.

#### Implementation notes

- Run `dotnet test tests/Api.IntegrationTests --filter "FullyQualifiedName~OrderBookUpdated|FullyQualifiedName~GetOrderBookSnapshot"`.
- Run `yarn --cwd web lint`, `yarn --cwd web build`, `yarn --cwd web api:verify`.
- No OpenAPI export expected unless Contracts HTTP DTOs changed (they do not).
- Update `docs/memory/current-status.md` on merge.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| — | CI scripts | Verification |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | Full OrderBook + SignalR market tests | Docker |
| Manual | §Manual UI checklist below | Aspire |

#### Acceptance criteria

- [x] All Story 2 AC in verification matrix covered (automation)
- [x] Story 1 snapshot tests still pass
- [ ] Manual checklist signed off by operator

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| Story 3/4 | No empty-state copy or layout grid changes |
| ADR needed? | No |

#### Risk

None.

---

## Reference files

| File | Why open it |
|------|-------------|
| `docs/plans/20260529-203000-best-bid-ask-story-2.md` | Prior realtime pattern (setQueryData, reconnect) |
| `docs/plans/20260530-140000-live-order-book-depth-story-1.md` | Depth panel + shared cache |
| `web/src/lib/signalr/interceptors.ts` | Hub → cache bridge |
| `web/src/features/market/order-book-from-realtime.ts` | Mapper to extend |
| `src/Application/Market/OrderBookMarketDataNotifier.cs` | Publish depth=10 |
| `tests/Api.IntegrationTests/Market/OrderBookUpdatedSignalRTests.cs` | Extend tests |
| `web/src/features/market/components/order-book-depth-panel.tsx` | Reconnect badge target |

## Implementation details (for /build)

**Already done (do not re-implement):**

- `createSimulationHubQueryBridge` calls `setQueryData(['market','orderbook','AAPL'], snapshot)` on `onOrderBookUpdated`.
- `useSimulationHub` on `reconnected`: resubscribe + `invalidateQueries` for order book key.
- `OrderBookMarketDataNotifier` reads snapshot at **depth 10** before publish.
- `OrderBookDepthPanel` renders `snapshot.bids` / `snapshot.asks` from `useOrderBookQuery`.

**Task 2 contract change:**

```csharp
public sealed record OrderBookLevelMessage(decimal Price, decimal Quantity, int OrderCount);
```

**Mapper sketch (frontend):**

- Input: `OrderBookUpdatedMessage`
- Per side: filter `quantity > 0` → dedupe by `price` (Map or reverse-reduce, last wins) → map to `OrderBookLevelResponse`
- Output: full replacement snapshot (not merge) — duplicate hub messages naturally “latest wins” via full replace (EC-12)

**500 ms AC:** No server timer; local verification = hub push + React commit. Integration tests use 2 s timeout bounds.

**Fewer than 10 levels (EC-03):** `OrderBookDepthTable` maps array as-is — no padding rows (Story 1 behavior; verify unchanged).

## Verification matrix (plan-level)

| Spec AC | Verified by |
|---------|-------------|
| Row updates within 500 ms on place/cancel/match | Task 1 manual + Task 3 integration (payload refresh) |
| Qty 0 level disappears | Task 2 mapper filter + Task 3 removal test |
| Disconnect >5 s → Reconnecting + reconnect ≤2 s snapshot | Task 1 manual |
| Fewer than 10 levels — no padding | Task 2 manual thin book + Story 1 regression |
| BR-08 strip vs depth after live update | Task 4 |
| EC-12 duplicate messages — latest wins | Task 2 full replace + dedupe |

## Manual UI checklist

Run on Aspire with authenticated user and seeded book (≥3 levels per side):

- [ ] Open `/trading` — depth shows ≥3 rows per side from initial snapshot
- [ ] DevTools: WebSocket to `/hubs/simulation` connected
- [ ] Trigger `NotifyOrderBookChangedAsync` (integration test DB state or future engine) — depth rows update **without** page reload or wallet skeleton flash
- [ ] Remove/cancel only order at a price — that **row disappears** (not qty 0)
- [ ] Strip best bid/ask match depth **first row** after live update
- [ ] Stop Api **6+ seconds** — **Reconnecting…** on strip **and** depth panel; rows still visible
- [ ] Restart Api — depth matches refetched snapshot within **2 s**; badges clear
- [ ] Thin book (<10 levels) — no empty placeholder rows
- [ ] `yarn --cwd web lint` + `yarn --cwd web build` green

## Rollback / recovery

- **Code:** Revert branch `feature/live-order-book-depth-story-2`
- **DB:** N/A
- **Redis:** N/A — read-only
- **Contracts:** If `OrderCount` added, revert is backward-compatible for HTTP clients; SignalR clients must deploy with mapper update

## Deferred work (Plan B)

- Story 3: per-side empty states in depth panel (#72)
- Story 4: PRD §8.1 three-column layout (#73)
- Matching engine auto-publish after Redis snapshot write (orders epic)
- Optional row highlight on quantity change (spec §13 Q4)
- Vitest unit tests for `order-book-from-realtime.ts` if frontend test policy changes

## GitHub Links

| Local ref | Issue # | Type | Title | URL |
|-----------|---------|------|-------|-----|
| spec Story 2 | 71 | Story | US-06 / Story 2: Depth table updates in real time | https://github.com/tranvuongduy2003/trading-simulator/issues/71 |
| spec epic | 69 | Epic | Spec: Live Order Book Depth (US-06) | https://github.com/tranvuongduy2003/trading-simulator/issues/69 |
| spec Story 1 | 70 | Story | US-06 / Story 1: See order book depth on trading view | https://github.com/tranvuongduy2003/trading-simulator/issues/70 |
