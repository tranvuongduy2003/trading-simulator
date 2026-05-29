---
artifact_type: spec
artifact_version: 1
id: spec-20260530-002008-live-order-book-depth
title: Live Order Book Depth
slug: live-order-book-depth
filename_template: 20260530-002008-live-order-book-depth.md
created_at: 2026-05-30T00:20:08+07:00
updated_at: 2026-05-30T00:20:08+07:00
status: draft
owner: product
tags: [spec, feature, trading-simulator, market-data, order-book, depth, liquidity, AAPL]
related_plan: docs/plans/20260530-140000-live-order-book-depth-story-1.md
related_specs: [docs/specs/20260529-010501-best-bid-ask.md]
github_epic_issue: 69
github_story_issues: [70, 71, 72, 73]
prd_refs: [PRD §5.2 US-06, PRD §6.2 FR-2.1, PRD §7.1, PRD §8.1, PRD §8.2, PRD §11.2]
tech_refs: [Tech §8.2, Tech §9, Tech §10.5, Tech §11]
db_refs: [DB §12.1, DB §14.2]
search_index:
  keywords: [order book, depth, liquidity, bid, ask, price level, aggregation, order count, AAPL, market data, signalr, snapshot, market group, left panel]
  bounded_contexts: [Trading]
  user_personas: [Aspiring Trader, Curious Learner]
---

> GitHub epic: [#69 Spec: Live Order Book Depth (US-06)](https://github.com/tranvuongduy2003/trading-simulator/issues/69)

# Feature: Live Order Book Depth
> Status: DRAFT  |  Date: 2026-05-30
> PRD: PRD §5.2 US-06, §6.2 FR-2.1, §7.1, §8.1, §8.2
> Tech: Tech §8.2, §9, §10.5, §11
> DB: DB §12.1, §14.2
> Owner: Product

## 1. Problem & Solution

**Problem:** A trader on the main trading view can see best bid and ask (US-05) but cannot scan **how much liquidity** rests at each price level. Without a depth ladder, it is hard to judge whether the market is thin or thick before placing limit orders.

**Solution:** Display the **live order book depth** for **AAPL**: up to **10** price levels on the bid and ask sides (PRD FR-2.1), each showing **price**, **aggregated quantity** (whole shares), and **number of orders** at that level. The table loads from the market snapshot on first paint and stays current via real-time order-book updates (PRD §7.1: ≤ **500 ms** from book change to UI).

**Persona:** Aspiring Trader or Curious Learner using the local simulator (PRD §4).

**Smallest valuable version:** A **depth table** in the trading view’s **left panel zone** (PRD §8.1) with bid and ask columns/sections — **not** the trade tape (US-07), candlestick chart (US-08), or order placement form (US-10+). The existing **top-of-book strip** (US-05) may remain above or beside the depth table; this feature adds the multi-level ladder only.

## 2. User Stories & Acceptance Criteria

### Story 1: See order book depth when I open the trading view
> As a **logged-in user**, I want to **see multiple bid and ask price levels for AAPL**, so that **I can gauge how liquidity is distributed away from the touch**.

**Happy path:**
- GIVEN the simulated book has at least **3** bid levels (e.g. **150.20**, **150.15**, **150.10** USD) and **3** ask levels (e.g. **150.30**, **150.35**, **150.40**) with known aggregated quantities and order counts → WHEN I land on the main trading view → THEN within **2 s** I see a **depth table** titled or labeled for **AAPL** showing up to **10** bid rows and up to **10** ask rows.
- GIVEN a bid level at **150.25** with aggregate quantity **300** shares across **2** open orders → WHEN the depth table renders that row → THEN I see **price 150.25**, **quantity 300**, and **order count 2** (labels such as “Price”, “Size”, “Orders” or equivalent accessible text — not color alone).
- GIVEN bids are sorted **highest price first** (best bid at top of bid section) and asks **lowest price first** (best ask at top of ask section) → WHEN I scan the table → THEN level order matches standard exchange depth conventions (PRD §13 glossary: order book sorted by price).

**Failure / edge path:**
- GIVEN I am not authenticated → WHEN I attempt to open the trading view → THEN I am redirected to login and **no** depth levels are shown.
- GIVEN the order book snapshot request fails (network or **5xx**) → WHEN the trading view loads → THEN the depth area shows a clear error with **Retry** (e.g. “Order book unavailable — Retry”) and **no fabricated** price levels or quantities.

---

### Story 2: Depth table updates in real time
> As a **logged-in user**, I want **the depth table to update automatically**, so that **I see liquidity change without refreshing the page**.

**Happy path:**
- GIVEN I am viewing **10** bid and ask levels → WHEN a new limit order adds a level or changes aggregate size at an existing price (place, cancel, or match) → THEN the affected row(s) update within **500 ms** (PRD §7.1) without a full page reload.
- GIVEN a level’s aggregate quantity drops to **0** because all orders at that price filled or cancelled → WHEN the book projection updates → THEN that price row **disappears** from the depth table and remaining levels shift (no ghost row with quantity **0**).

**Failure / edge path:**
- GIVEN the real-time connection is disconnected for more than **5 s** → WHEN I remain on the trading view → THEN the depth panel shows a **Reconnecting…** indicator (or shares the market strip badge) and, on reconnect, the table matches the latest snapshot within **2 s**.
- GIVEN a book update payload contains fewer than **10** levels on one side → WHEN the UI applies it → THEN only the provided levels are shown (no padding with zeros).

---

### Story 3: Understand an empty or one-sided book in the depth panel
> As a **logged-in user**, I want **clear messaging when bids, asks, or both are missing**, so that **I am not misled by empty rows or stale levels**.

**Happy path:**
- GIVEN the book has bids and asks at multiple levels → WHEN I view the depth table → THEN both sides show live rows (Story 1).

**Failure / edge path:**
- GIVEN there are **no** open bids for **AAPL** → WHEN the depth panel loads or updates → THEN the bid section shows empty-state copy (e.g. **“No bids”**) and **no** bid price rows; the ask section still shows if present.
- GIVEN there are **no** open asks → WHEN the depth panel loads or updates → THEN the ask section shows **“No asks”** and the bid section still shows if present.
- GIVEN there are **no** bids **and** no asks → WHEN I view the depth panel → THEN both sections show empty-state copy (e.g. **“No market — waiting for liquidity”**) and **no** placeholder prices; simulated liquidity is expected to repopulate under normal operation (PRD §11.2).
- GIVEN I log out → WHEN I log in again → THEN the depth table does not flash prices from the prior session’s cache (consistent with US-05 cache purge behavior).

---

### Story 4: Depth panel fits the trading layout
> As a **logged-in user**, I want **the depth table in the expected trading layout zone**, so that **I can read book and (future) chart/order panels together on desktop**.

**Happy path:**
- GIVEN my viewport is at least **1280px** wide (PRD §8.3 desktop target) → WHEN I open the trading view → THEN the depth table appears in the **left panel zone** of the trading layout (PRD §8.1), with **AAPL** visible in page chrome, and numeric columns use **tabular** alignment for prices and quantities (PRD §8.2).
- GIVEN bid and ask rows use green and red accent tones → WHEN I view any row → THEN each side is also identified by text (“Bid” / “Ask” or column headers), not color alone (PRD §7.5).

**Failure / edge path:**
- GIVEN viewport width is between **768px** and **1279px** (tablet) → WHEN I open the trading view → THEN the depth table remains usable in a **stacked** layout (depth above or below other panels) without horizontal clipping of columns.
- GIVEN the depth table is loading → WHEN the first snapshot has not arrived → THEN I see a **skeleton** or “Loading order book…” in the left zone — not a table of zeros.

## 3. Domain & Business Rules

```
BR-01: Depth levels are aggregated by price for open orders on AAPL with status Pending or Partially Filled. Example: two buy orders at 150.25 for 100 and 200 shares → one bid level 150.25, quantity 300, orderCount 2.
BR-02: Default depth N = 10 price levels per side (PRD FR-2.1). Fewer than N levels exist when the book is thin; never invent levels.
BR-03: Bid levels are ordered descending by price (best bid first). Ask levels ascending by price (best ask first). Example: bids 150.30, 150.25, 150.20 top to bottom; asks 150.35, 150.40, 150.45 top to bottom.
BR-04: Quantities are whole shares (integers); prices use up to 4 decimal places (NUMERIC(18,4) semantics, PRD FR-3.1).
BR-05: Depth read model reflects matching engine state after commits; UI may lag briefly but converges within PRD §7.1 (≤ 500 ms) under nominal local load (async matching: API accept → engine match → projection → SignalR).
BR-06: Only AAPL is supported; non-AAPL symbol requests are rejected at the API boundary (MVP single-symbol constraint).
BR-07: Simulated liquidity orders count toward aggregation and orderCount like user orders; they are not visually distinguished in the depth table for MVP.
BR-08: Top-of-book strip (US-05) and depth table must show consistent best bid/ask and sizes derived from the same snapshot or hub payload (no divergent sources).
```

## 4. UI Behavior **or** API Contract

### 4a. UI Behavior

```
Screen: Main trading view — left panel (Order book depth)
- Arrival: User sees “Order book” or equivalent heading, symbol AAPL, bid section and ask section (side-by-side or stacked per layout), up to 10 levels each.
- Action: User scrolls within panel if future levels exceed viewport (MVP: 10 rows fit without scroll on desktop).
- Loading: Skeleton rows or spinner in left zone until first snapshot succeeds.
- Empty: “No bids” / “No asks” / “No market — waiting for liquidity” per Story 3; no zero-price filler rows.
- Error: Inline retry in depth panel; does not block wallet or activity tabs unless a global banner is chosen.
- Real-time: Rows add, update, or remove on OrderBookUpdated within 500 ms; optional subtle row highlight on change (no jarring flash per PRD §8.2).

Screen: Main trading view — coexistence with US-05 strip
- Top-of-book strip may sit above depth or in a summary row; values must match first bid/ask row of depth table when both visible.
```

### 4b. API Contract

- **Endpoint(s):** `GET /api/market/orderbook?symbol=AAPL&depth=10` — returns top **N** levels per side (default **10** if depth omitted; client **should** pass `depth=10` explicitly for clarity).
- **Request / response (sketch):**
  ```json
  {
    "symbol": "AAPL",
    "bids": [
      { "price": 150.25, "quantity": 300, "orderCount": 2 },
      { "price": 150.20, "quantity": 100, "orderCount": 1 }
    ],
    "asks": [
      { "price": 150.30, "quantity": 150, "orderCount": 1 },
      { "price": 150.35, "quantity": 200, "orderCount": 3 }
    ],
    "updatedAt": "2026-05-30T00:00:00Z"
  }
  ```
- **Errors:** `400` invalid symbol; `401` unauthenticated session; `500`/`503` with RFC 7807 `application/problem+json` — UI shows retry, no partial fake book.
- **Auth:** Authenticated session required (consistent with US-05 MVP decision).
- **Idempotency:** Read-only — not required.
- **Pagination / filtering:** N/A (single symbol, fixed depth cap).

## 5. Data & Storage Impact

| Concern | Answer |
|---|---|
| PostgreSQL tables / columns | **None** new. Levels aggregated from open `orders` via partial index `ix_orders_active_book` when Redis miss (DB §4, §14.2). |
| Redis keys / projections | **`orderbook:{symbol}:snapshot`** — stores top-N levels; depth UI consumes same projection as US-05 (DB §12.1). |
| Matching / channel behavior | Place/cancel/match commits → engine updates in-memory book → writes Redis snapshot (depth **10**) → notification publishes `OrderBookUpdated` with bid/ask arrays (Tech §7–§9). |
| Migration needed | **No** |
| Rebuild strategy if Redis cleared | Snapshot rebuilt from PostgreSQL active orders on read fallback or engine recovery (DB §12.2). |

## 6. Real-Time & Consistency

- **SignalR events:** `OrderBookUpdated` on group `market:AAPL` — payload includes `symbol`, `bids[]`, `asks[]` (each level: price, quantity; orderCount if included in contract), `updatedAt`. Client replaces depth table data from payload (same TanStack Query cache key as snapshot is acceptable if US-05 pattern continues).
- **Read-your-writes:** After place/cancel, API returns order status before matching finishes; depth may update twice (resting order appears, then size changes after match). User sees command result first; depth converges on engine publish (async matching invariant).
- **Stale UI handling:** On hub reconnect, refetch `GET /api/market/orderbook?symbol=AAPL&depth=10` and resubscribe to `market:AAPL`. Show last known levels with reconnect badge until refetch completes; clear cache on logout.

## 7. Security & Privacy (MVP)

- **Authn / Authz:** Session cookie; depth snapshot and hub subscription for authenticated trading routes only. Market data is shared across users (simulation); no per-user book partition.
- **Sensitive fields:** No PII in market payloads.
- **Threat surface:** Client-side display tampering does not affect matching; server/engine state is authoritative. Optional rate limits on market reads are not required for local MVP.

## 8. Observability (local MVP)

| Signal | What to emit |
|---|---|
| Structured logs | Snapshot served (symbol, bidLevelCount, askLevelCount, cache hit/miss); hub publish for book update — avoid logging full depth at info level |
| Traces | Span on snapshot query and hub fan-out (ServiceDefaults) |
| Metrics | `minimal for MVP` |
| Audit | N/A |

## 9. Edge Cases

```
EC-01: Empty book → both sections empty-state; no rows.
EC-02: One-sided book → missing side empty-state only.
EC-03: Fewer than 10 levels → show only existing levels.
EC-04: Partial fill reduces level quantity but level remains → row quantity updates.
EC-05: Last order at a price level cancelled → row removed.
EC-06: User market order sweeps multiple levels → multiple rows decrease or disappear within 500 ms.
EC-07: Simulated liquidity refresh changes several levels at once → table batch-updates from single hub message.
EC-08: Session expired → redirect login; depth cleared with cache purge.
EC-09: Redis unavailable → API PostgreSQL fallback; higher latency acceptable; consistent snapshot or error.
EC-10: Engine down / no updates for extended period → optional “Market data delayed” after threshold (e.g. 10 s); minimum is reconnect + refetch (align with US-05 EC-08).
EC-11: Crossed book (best bid ≥ best ask) → should not persist; if snapshot returns crossed levels, display as returned and log warning (engine invariant).
EC-12: Duplicate rapid hub messages → UI shows latest snapshot; no duplicate rows for same price.
```

## 10. Dependencies, Risks, Decision Triggers

- **Depends on:** US-02 (session); US-05 (order book snapshot API, SignalR `OrderBookUpdated`, market query cache patterns); matching engine + Redis projection pipeline (Tech §8.2, §10.5); simulated liquidity (PRD §11.2) for non-empty default experience.
- **Impacts:** US-09 (real-time without refresh — partially satisfied here); US-10–13 (order form may later pre-fill from clicked level — out of scope); US-07/08 layout zones in same trading view.
- **External services:** PostgreSQL, Redis, Aspire-hosted API and Matching Engine.
- **Key risk:** US-05 implementation may ship before depth UI — ensure hub payload includes **full N levels**, not only best bid/ask, or depth table will not satisfy FR-2.1.
- **Decision triggers:** User-configurable depth N > 10 → ADR + API contract change. Cumulative depth visualization → new spec. Public anonymous depth → ADR + security review.

## 11. Assumptions

- **Backend snapshot and SignalR payload already carry up to 10 levels** per side (US-05 infrastructure); US-06 is primarily **frontend depth panel** plus explicit `depth=10` query parameter on the client.
- Depth table lives in **left panel** per PRD §8.1; center (chart) and right (order form) remain placeholders until their specs ship.
- **Top-of-book strip** (US-05) remains; depth does not replace it unless product later consolidates (would need ADR).
- **orderCount** is exposed in API contract and shown in UI (FR-2.1 explicit).
- Tablet stacked layout is acceptable for MVP; phone layout out of scope (PRD §8.3).
- Price formatting: **2–4** decimal places in UI; quantities as integers with thousands separators optional.

## 12. Out of Scope

- Trade tape (US-07), candlestick chart (US-08), last trade / daily change in header (FR-2.2).
- Clicking a depth row to pre-fill order price (order form not in this spec).
- Cumulative depth histogram, heatmap, or level-2 historical data.
- User-configurable depth N, scrolling beyond 10 levels, or multiple symbols.
- Separate public/unauthenticated depth endpoint.
- Message broker, transactional outbox, fractional shares, production CD, horizontal scaling (global MVP exclusions).

## 13. Open Questions

| # | Question | Source | Answer | Status |
|---|---|---|---|---|
| 1 | Should bid and ask appear as two columns side-by-side or as a single combined ladder (asks above, bids below)? | PRD §8.1 layout | **Side-by-side** bid and ask tables on desktop ≥1280px; **stacked** on tablet | ✅ |
| 2 | Show `orderCount` column for MVP or only price + size? | FR-2.1 | **Show all three** (price, quantity, orderCount) | ✅ |
| 3 | Share one TanStack Query cache with US-05 top-of-book or separate key? | Tech §11 | **Same cache** (`market/orderbook/AAPL`) — single snapshot feeds strip + depth | ✅ |
| 4 | Subtle row flash on quantity change? | PRD §8.2 | **Optional** — allowed if subtle; not required for MVP acceptance | ⏳ Deferred |
