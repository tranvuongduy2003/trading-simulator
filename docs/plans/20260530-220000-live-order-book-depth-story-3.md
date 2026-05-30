---
artifact_type: plan
artifact_version: 1
id: plan-20260530-220000-live-order-book-depth-story-3
title: Live Order Book Depth — Story 3 (empty and one-sided depth panel)
slug: live-order-book-depth-story-3
filename_template: 20260530-220000-live-order-book-depth-story-3.md
created_at: 2026-05-30T22:00:00+07:00
updated_at: 2026-05-30T23:30:00+07:00
status: approved
owner: engineering
tags: [plan, implementation, trading-simulator, market-data, order-book, depth, empty-book, us-06]
related_spec: docs/specs/20260530-002008-live-order-book-depth.md
related_plans:
  - docs/plans/20260530-140000-live-order-book-depth-story-1.md
  - docs/plans/20260530-210000-live-order-book-depth-story-2.md
  - docs/plans/20260530-010000-best-bid-ask-story-4.md
prd_refs: [PRD §5.2 US-06, PRD §8.2, PRD §11.2]
tech_refs: [Tech §9, Tech §11]
db_refs: [DB §12.1]
github:
  repo: tranvuongduy2003/trading-simulator
  epic_issue: 69
  story_issue_ids: [72]
  last_synced_at: 2026-05-30T22:00:00+07:00
search_index:
  keywords: [order book depth, empty book, one-sided, no bids, no asks, no market, liquidity state, OrderBookDepthPanel, OrderBookDepthTable, logout cache purge, OrderBookUpdated, EC-01, EC-02, EC-08, BR-02, AAPL]
  bounded_contexts: [Trading]
  task_count: 4
---

# Implementation Plan: Live Order Book Depth — Story 3

| Field | Value |
|-------|--------|
| Spec | `docs/specs/20260530-002008-live-order-book-depth.md` |
| Status | APPROVED (implementation complete) |
| Tasks | 4 |
| Branch | `feature/live-order-book-depth-story-3` |
| Aspire impact | No |
| Schema impact | No |
| Test levels | Api.IntegrationTests (optional SignalR empty payload); Manual UI |
| ADRs required | None |
| GitHub | Synced 2026-05-30 — Story [#72](https://github.com/tranvuongduy2003/trading-simulator/issues/72); Epic [#69](https://github.com/tranvuongduy2003/trading-simulator/issues/69) |

## Executive summary

Deliver **Story 3** of US-06: when the **AAPL** depth panel receives empty `bids` and/or `asks` arrays (HTTP snapshot or SignalR `OrderBookUpdated`), each side shows **explicit empty-state copy** instead of blank tables or fabricated rows. Stories 1–2 already render multi-level depth from the shared TanStack Query cache; US-05 Story 4 already centralizes liquidity copy in `top-of-book-display.ts` and purges `['market','orderbook','AAPL']` on logout. This plan adds **depth-specific empty UI** in `OrderBookDepthTable` / `OrderBookDepthPanel`, reuses the same message strings for consistency, optionally proves **real-time empty transitions** via one SignalR integration test, and closes with a **manual Aspire checklist**.

**Definition of done:** All Story 3 acceptance criteria pass; depth empty copy matches strip semantics; Stories 1–2 regression (live rows, reconnect, level removal) unchanged.

## Goals and non-goals

**Goals**

- G1: No open bids → bid depth section shows **“No bids”**, no price rows; ask section still shows live rows when present (EC-02, BR-02).
- G2: No open asks → ask section shows **“No asks”**; bid section unchanged when present.
- G3: Empty book (both sides) → **both** depth sections show **“No market — waiting for liquidity”**; no placeholder prices or zero-qty rows.
- G4: Real-time hub/snapshot with empty side arrays updates depth empty states without stale opposite-side rows.
- G5: Logout → re-login does not flash prior session depth prices (reuse US-05 cache purge).
- G6: Manual Aspire checklist signed off.

**Non-goals** (this plan will not do)

- NG1: Simulated liquidity seeding (PRD §11.2 / engine).
- NG2: Backend API or hub contract changes (empty arrays already valid).
- NG3: PRD §8.1 three-column layout (**Story 4**, #73).
- NG4: New PostgreSQL schema, Redis keys, or matching-engine changes.
- NG5: Frontend Vitest unit-test harness (MVP manual UI per `core.mdc`).

## Traceability matrix

| Spec story | Plan task(s) | Test evidence |
|------------|--------------|---------------|
| Story 3 — empty / one-sided depth | Tasks 1–4 | Optional `OrderBookUpdated_EmptyPayload_*`; manual checklist |
| Story 1–2 regression | Task 4 | Existing `GetOrderBookSnapshot_*`, `OrderBookUpdated_*`; manual two-sided book |
| Story 4 (layout) | — | Deferred (#73) |

## Architecture impact

```text
┌─────────────────────────────────────────────────────────────────────────┐
│  web/ TradingPage                                                        │
│    useOrderBookQuery()  ── shared key ['market','orderbook','AAPL']      │
│         │                                                                │
│         ├─► TopOfBookStrip — deriveTopOfBook + US-05 Story 4 empty UX   │
│         └─► OrderBookDepthPanel (MODIFY)                                 │
│               deriveBookLiquidityState(deriveTopOfBook(snapshot))        │
│               OrderBookDepthTable ×2 — per-side empty copy or rows       │
│                                                                          │
│  logout → clearUserScopedQueries → removeQueries market/orderbook/AAPL   │
│  SignalR OrderBookUpdated → setQueryData (full snapshot replace)         │
└─────────────────────────────────────────────────────────────────────────┘
                                    ▲
                                    │ empty bids[] / asks[] valid payloads
┌───────────────────────────────────┴─────────────────────────────────────┐
│  Api: GET /api/market/orderbook — unchanged (US-05)                      │
└─────────────────────────────────────────────────────────────────────────┘
```

| Layer | Change summary |
|-------|----------------|
| Domain | None |
| Application | None |
| Infrastructure | None |
| Api | None (optional SignalR integration test only) |
| MatchingEngine | None |
| web/ | `order-book-depth-display.ts`, `order-book-depth-table.tsx`, `order-book-depth-panel.tsx` |
| AppHost | None |

## Data & migration plan

| Artifact | Action | DB reference |
|----------|--------|--------------|
| EF migration | None | — |
| Redis keys | None (empty sides already valid in `orderbook:AAPL:snapshot`) | DB §12.1 |
| SignalR contract | None | Tech §9 |
| Book recovery | N/A | DB §12.2 |

**Payload contract (unchanged):** `OrderBookSnapshotResponse` / `OrderBookUpdatedMessage` use empty `bids` / `asks` arrays. Client must not synthesize levels (BR-02).

## Open questions

| # | Question | Source | Answer | Status |
|---|----------|--------|--------|--------|
| 1 | Fully empty book: per-side “No bids”/“No asks” or global message in both sections? | Spec Story 3 AC | **Both sections** show **“No market — waiting for liquidity”** when `bids` and `asks` are empty; one-sided uses side-specific copy | ✅ |
| 2 | Empty side: hide table headers or show headers + empty message? | Spec §4a | Keep **Bid/Ask heading + column headers**; replace tbody with a single empty-state row or muted paragraph (no fake price cells) | ✅ |
| 3 | Panel-level banner when fully empty (like strip)? | US-05 Story 4 pattern | **Optional** — per-section copy satisfies AC; skip duplicate banner unless layout needs it | ✅ |
| 4 | Logout cache purge — new work? | Spec Story 3 AC | **Already implemented** in `clear-user-queries.ts` (US-05 Story 4); Task 4 verifies depth panel benefits | ✅ |

## Risks and mitigations

| Risk | Likelihood | Impact | Mitigation | Owner task |
|------|------------|--------|------------|------------|
| Copy drift vs top-of-book strip | Medium | Medium | Reuse `EMPTY_BOOK_MESSAGE` and `formatTopOfBookSidePrice` / exported side labels from `top-of-book-display.ts` | Task 1 |
| Stale opposite side after one-sided hub push | Low | High | `setQueryData` replaces full snapshot (Story 2); manual cancel-all-bids step | Task 4 |
| Empty tbody looks like loading bug | Medium | Low | Explicit muted copy with `aria-live="polite"` on depth panel | Task 2 |
| Flash prior session prices on re-login | Low | High | Verify `removeQueries` on logout; depth `isPending` until fresh fetch | Task 4 |

## Prerequisites

- [x] Stories 1–2 implemented (`OrderBookDepthPanel`, SignalR bridge, reconnect badge)
- [x] US-05 Story 4 liquidity helpers + logout purge on base branch
- [ ] Aspire local stack runs (`aspire run` or env-doctor)
- [ ] Api integration tests pass locally (Docker for Testcontainers)

## File structure (planned)

```text
web/src/features/market/
  order-book-depth-display.ts           MODIFY — depth side empty message helper
  components/order-book-depth-table.tsx MODIFY — empty state when levels.length === 0
  components/order-book-depth-panel.tsx MODIFY — pass liquidity state / empty flags
  top-of-book-display.ts                REUSE — deriveBookLiquidityState, EMPTY_BOOK_MESSAGE
web/src/features/auth/
  clear-user-queries.ts                 REUSE — verify purge key unchanged
tests/Api.IntegrationTests/Market/
  OrderBookUpdatedSignalRTests.cs       MODIFY (optional) — empty payload clears both sides
```

## Authorization, session, and domain notes

- **Session model:** Unauthenticated users redirect to login; depth never renders (Story 1).
- **Domain rules:** BR-02 — never invent price levels when a side is empty; render copy only.
- **Logout:** `clearUserScopedQueries` already removes `['market','orderbook','AAPL']`; depth and strip share cache — one purge covers both.
- **Consistency:** Depth first rows must still match strip best bid/ask when both sides have levels (BR-08 — regression only).

## Progress tracker

### Task 1: Depth empty-state helpers and table skeleton

| Attribute | Value |
|-----------|--------|
| Spec story | Story 3 |
| Depends on | None (Stories 1–2 + US-05 Story 4 on base branch) |
| Estimated complexity | S |
| Parent story issue | #72 |

#### Objective

Add pure helpers for depth-side empty copy and wire **one-sided empty UI** in `OrderBookDepthTable` so an API snapshot with empty `bids` or `asks` shows **“No bids”** / **“No asks”** instead of a blank table — end-to-end observable on trading view.

#### Implementation notes

- Add `getDepthSideEmptyMessage(bids, asks, side): string | null` in `order-book-depth-display.ts`:
  - Returns `null` when the side has `levels.length > 0`.
  - When both sides empty → `EMPTY_BOOK_MESSAGE` for **both** bid and ask sections.
  - When only this side empty → `'No bids'` or `'No asks'` (reuse `formatTopOfBookSidePrice(null, side)` or export shared constants from `top-of-book-display.ts`).
- Derive `BookLiquidityState` via existing `deriveBookLiquidityState(deriveTopOfBook(snapshot))` in panel (do not duplicate logic).
- `OrderBookDepthTable`: when `levels.length === 0`, render muted empty copy (e.g. `<p className="text-muted-foreground text-sm">…</p>`) **instead of** an empty `<TableBody>`; do not render placeholder price rows.
- Keep column headers and side label (“Bids” / “Asks”) for accessibility.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `web/src/features/market/order-book-depth-display.ts` | `getDepthSideEmptyMessage` |
| MODIFY | `web/src/features/market/components/order-book-depth-table.tsx` | Empty side UI |
| MODIFY | `web/src/features/market/components/order-book-depth-panel.tsx` | Derive liquidity state; pass empty message to tables |
| REUSE | `web/src/features/market/top-of-book-display.ts` | `deriveTopOfBook`, `deriveBookLiquidityState`, `EMPTY_BOOK_MESSAGE` |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Reset book / fresh user → empty snapshot → both sections show global message | `web/` |
| Manual | Seed bid only → “No asks” on ask side; bid rows visible | `web/` |

#### Acceptance criteria

- [x] Empty `bids` and `asks` → both depth sections show **“No market — waiting for liquidity”**; no price rows
- [x] Bid-only snapshot → bid rows + ask section **“No asks”**
- [x] Ask-only snapshot → symmetric
- [x] Two-sided snapshot still renders live rows (happy path regression)

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | PRD §5.2 US-06; PRD §8.2 text labels; DB §12.1 read-only |
| Async matching | N/A |
| PostgreSQL authoritative | N/A |
| Redis projection | N/A |
| RFC 7807 errors | N/A |
| SignalR | N/A this task |
| Aspire | None |
| ADR needed? | No |

#### Risk

None — isolated frontend display logic.

---

### Task 2: Fully empty book UX and panel polish

| Attribute | Value |
|-----------|--------|
| Spec story | Story 3 |
| Depends on | Task 1 |
| Estimated complexity | S |
| Parent story issue | #72 |

#### Objective

Refine fully empty and loading/error coexistence: depth panel empty states are visually distinct from skeleton and error states; `aria-live="polite"` announces empty transitions; no duplicate or conflicting copy when reconnect badge is visible.

#### Implementation notes

- Ensure `OrderBookDepthPanel` does not render depth tables during `isPending` or `isError` (existing guard — verify no flash of empty copy before load).
- When `snapshot` succeeds with both arrays empty, both tables show global message (Task 1 helper).
- Match strip typography: muted `text-sm` for empty copy; side headings retain bid/ask color + text label.
- Do **not** add a third panel-level banner if both sections already show `EMPTY_BOOK_MESSAGE` (avoid triple repetition with strip banner — acceptable to show strip + depth both with global message per product consistency).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `web/src/features/market/components/order-book-depth-panel.tsx` | Loading/empty/error guards, a11y |
| MODIFY | `web/src/features/market/components/order-book-depth-table.tsx` | Typography / `aria-live` on empty block |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Hard refresh on empty book → skeleton then empty copy (not zeros) | `web/` |
| Manual | API error → retry still works; empty state not shown during error | `web/` |

#### Acceptance criteria

- [x] Loading shows skeleton only — not empty copy
- [x] Fully empty book: no placeholder prices (`0.00`, `—` in price column)
- [x] Empty copy uses accessible text, not color alone (side headings remain)

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | PRD §7.5 a11y |
| SignalR | N/A |
| Aspire | None |
| ADR needed? | No |

#### Risk

Low — visual polish only.

---

### Task 3: Real-time empty transitions (SignalR)

| Attribute | Value |
|-----------|--------|
| Spec story | Story 3 |
| Depends on | Task 1 |
| Estimated complexity | M |
| Parent story issue | #72 |

#### Objective

Prove depth empty states update when `OrderBookUpdated` delivers empty side arrays (portfolio reset or notifier publish), without stale rows on the cleared side.

#### Implementation notes

- Client path already replaces full snapshot via `mapOrderBookUpdatedToSnapshot` → `setQueryData` (Story 2). Task 1 empty UI should react automatically — this task adds **automated proof** and fixes any gap (e.g. defensive filter leaving ghost levels).
- Optional integration test `OrderBookUpdated_EmptyPayload_ClearsBothSides`: subscribe to hub, publish snapshot with `bids: []`, `asks: []` via `IOrderBookMarketDataNotifier`, assert HTTP/cache read returns empty arrays (mirror existing depth hub tests in `OrderBookUpdatedSignalRTests.cs`).
- Optional: `OrderBookUpdated_OneSidedPayload_ClearsOppositeSide` — publish bid-only after two-sided seed; assert `asks` empty in subsequent GET.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `tests/Api.IntegrationTests/Market/OrderBookUpdatedSignalRTests.cs` | Empty / one-sided hub payload tests |
| REUSE | `web/src/lib/signalr/interceptors.ts` | Full snapshot replace |
| REUSE | `tests/Api.IntegrationTests/Market/MarketTestHelpers.cs` | Notifier + snapshot helpers |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `OrderBookUpdated_EmptyPayload_ClearsBothSides` | `OrderBookUpdatedSignalRTests.cs` |
| Integration | `OrderBookUpdated_BidOnlyPayload_EmptyAsks` (optional) | same |
| Manual | Reset portfolio → depth shows empty copy on both sides within 2 s | `web/` |

#### Acceptance criteria

- [x] Hub message with empty arrays updates TanStack cache (integration test passes)
- [ ] Manual: live cancel all orders on one side → depth empty copy on that side; other side unchanged

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | Tech §9 SignalR; EC-01, EC-02 |
| Async matching | Empty book after reset uses existing notification path |
| SignalR | Full snapshot replace — no merge that preserves stale side |
| Aspire | None |
| ADR needed? | No |

#### Risk

Integration test flakiness — reuse Story 2 hub test patterns (deterministic notifier, not engine timing).

---

### Task 4: Logout verification, manual checklist, regression

| Attribute | Value |
|-----------|--------|
| Spec story | Story 3 — Polish |
| Depends on | Tasks 1–3 |
| Estimated complexity | S |
| Parent story issue | #72 |

#### Objective

Verify logout cache purge prevents depth price flash on re-login; run manual Aspire checklist; confirm strip/depth messaging consistency and Stories 1–2 regression.

#### Implementation notes

- Confirm `clear-user-queries.ts` includes `['market','orderbook','AAPL']` — **no code change expected** (US-05 Story 4).
- Manual: login → view depth with prices → logout → login → depth shows skeleton/empty/fresh fetch, **no flash** of prior bid/ask rows.
- Manual: two-sided book → both panels show rows; strip best matches depth row 1 (BR-08).
- Manual: reconnect badge + empty book — badge visible; empty copy still correct after reconnect refetch.
- Run `yarn --cwd web lint`, `yarn --cwd web build`, `dotnet test` on Market integration filters.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| REUSE | `web/src/features/auth/clear-user-queries.ts` | Verify purge key |
| REUSE | `web/src/features/trading/pages/trading-page.tsx` | Shared query wiring |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | Re-run `GetOrderBookSnapshot_Aapl_ReturnsEmptySnapshot`, `BidOnly`, `AskOnly` | `GetOrderBookSnapshotTests.cs` |
| Manual | Full Story 3 checklist (below) | `web/` |

#### Acceptance criteria

- [x] Logout → re-login: no stale depth prices (`clear-user-queries.ts` purge verified; manual sign-off pending)
- [x] `yarn lint` + `yarn build` green
- [x] Existing market integration tests pass (Docker) — 7 `OrderBookUpdated_*`, 11 `GetOrderBookSnapshot_*`
- [ ] Manual checklist complete (operator on Aspire)

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | EC-08 session expired → login redirect + cleared depth |
| SignalR | Reconnect refetch preserves empty state when book empty |
| Aspire | Manual on local stack |
| ADR needed? | No |

#### Risk

None — verification task.

---

## Manual UI checklist

Run on Aspire (`aspire run`) as logged-in user on `/trading`:

1. **Empty book:** Reset portfolio or use fresh empty snapshot → depth bid and ask sections both show **“No market — waiting for liquidity”**; strip shows matching empty UX; no numeric price rows in depth tables.
2. **Bid-only:** Seed one buy limit → bid depth rows visible; ask section **“No asks”**; strip ask side **“No asks”**.
3. **Ask-only:** Seed one sell limit → symmetric.
4. **Two-sided:** Seed bids and asks → both depth sections show rows; first bid/ask prices match strip.
5. **Live transition:** Cancel all bids → bid section switches to **“No bids”** without page reload; asks remain.
6. **Logout purge:** With visible depth prices, logout → login → no flash of old prices before new fetch.
7. **Reconnect:** Disconnect network >5 s → **Reconnecting…** badge on depth panel; reconnect on empty book → empty copy restored.

## Reference files

| File | Why open it |
|------|-------------|
| `web/src/features/market/top-of-book-display.ts` | Liquidity state + shared empty copy (US-05 Story 4) |
| `web/src/features/market/components/top-of-book-strip.tsx` | Strip empty UX pattern to mirror |
| `web/src/features/market/components/order-book-depth-table.tsx` | Primary empty UI target |
| `web/src/features/market/components/order-book-depth-panel.tsx` | Panel wiring |
| `docs/plans/20260530-010000-best-bid-ask-story-4.md` | Prior empty-book plan |
| `tests/Api.IntegrationTests/Market/GetOrderBookSnapshotTests.cs` | Empty / one-sided API facts |
| `tests/Api.IntegrationTests/Market/OrderBookUpdatedSignalRTests.cs` | Hub test patterns (Task 3) |

## Implementation details (for /build)

- **Liquidity derivation:** `const display = deriveTopOfBook(snapshot)` then `deriveBookLiquidityState(display)` — same as strip. For per-side empty message, prefer checking `snapshot.bids.length` / `snapshot.asks.length` directly in `getDepthSideEmptyMessage` to avoid off-by-one with partial levels.
- **Copy strings (must match US-05):** `"No bids"`, `"No asks"`, `"No market — waiting for liquidity"`.
- **Fully empty rule:** When `bids.length === 0 && asks.length === 0`, return `EMPTY_BOOK_MESSAGE` for **both** sides (not side-specific labels).
- **Table structure:** Prefer empty message **below** column headers inside the table card, or a single `<TableRow><TableCell colSpan={3}>…</TableCell></TableRow>` — avoid empty tbody with no explanation.
- **No backend changes:** `GetOrderBookSnapshot_Aapl_ReturnsEmptySnapshot`, `BidOnly`, `AskOnly` already assert empty arrays.
- **Logout:** `clearUserScopedQueries` line 9 already purges market orderbook — verify only.

## Verification matrix (plan-level)

| Spec AC | Verified by |
|---------|-------------|
| Happy path — both sides show rows | Task 1 regression + Task 4 manual #4 |
| No bids → “No bids”, asks if present | Task 1 manual + `GetOrderBookSnapshot_AskOnly_*` API |
| No asks → “No asks” | Task 1 manual + `GetOrderBookSnapshot_BidOnly_*` API |
| Both empty → global message, no placeholders | Task 1–2 manual #1 |
| Logout → no flash | Task 4 manual #6 + `clear-user-queries.ts` |
| Real-time empty update | Task 3 integration + manual #5 |

## Rollback / recovery

- **Code:** revert branch commits
- **DB:** N/A
- **Redis:** N/A

## Deferred work (Plan B)

- Story 4 — PRD §8.1 three-column trading layout (#73)
- Vitest unit tests for `getDepthSideEmptyMessage` if frontend test harness is adopted later
- Simulated liquidity seeding for non-empty default cold start (epic / PRD §11.2)

## GitHub Links

| Local ref | Issue # | Type | Title | URL |
|-----------|---------|------|-------|-----|
| spec.Story 3 | 72 | Story | US-06 / Story 3: Empty or one-sided depth panel | https://github.com/tranvuongduy2003/trading-simulator/issues/72 |
| epic | 69 | Epic | Spec: Live Order Book Depth (US-06) | https://github.com/tranvuongduy2003/trading-simulator/issues/69 |
