---
artifact_type: plan
artifact_version: 1
id: plan-20260530-230000-live-order-book-depth-story-4
title: Live Order Book Depth — Story 4 (depth panel trading layout)
slug: live-order-book-depth-story-4
filename_template: 20260530-230000-live-order-book-depth-story-4.md
created_at: 2026-05-30T23:00:00+07:00
updated_at: 2026-05-30T23:45:00+07:00
status: approved
owner: engineering
tags: [plan, implementation, trading-simulator, market-data, order-book, depth, layout, us-06, prd-8-1]
related_spec: docs/specs/20260530-002008-live-order-book-depth.md
related_plans:
  - docs/plans/20260530-140000-live-order-book-depth-story-1.md
  - docs/plans/20260530-210000-live-order-book-depth-story-2.md
  - docs/plans/20260530-220000-live-order-book-depth-story-3.md
prd_refs: [PRD §5.2 US-06, PRD §8.1, PRD §8.2, PRD §8.3, PRD §7.5]
tech_refs: [Tech §11]
db_refs: []
github:
  repo: tranvuongduy2003/trading-simulator
  epic_issue: 69
  story_issue_ids: [73]
  last_synced_at: 2026-05-30T23:00:00+07:00
search_index:
  keywords: [trading layout, left panel, depth table, PRD 8.1, xl breakpoint, 1280px, tablet stack, AAPL chrome, tabular-nums, bid ask labels, placeholder chart, placeholder order form, BR-08, TopOfBookStrip, OrderBookDepthPanel]
  bounded_contexts: [Trading]
  task_count: 5
---

# Implementation Plan: Live Order Book Depth — Story 4

| Field | Value |
|-------|--------|
| Spec | `docs/specs/20260530-002008-live-order-book-depth.md` |
| Status | APPROVED |
| Tasks | 5 |
| Branch | `feature/live-order-book-depth-story-4` |
| Aspire impact | No |
| Schema impact | No |
| Test levels | Manual UI only (MVP) |
| ADRs required | None |
| GitHub | Synced 2026-05-30 — Story [#73](https://github.com/tranvuongduy2003/trading-simulator/issues/73); Epic [#69](https://github.com/tranvuongduy2003/trading-simulator/issues/69) |

## Executive summary

Deliver **Story 4** of US-06: reshape the main **trading view** into the PRD §8.1 **desktop workspace** (left = order book depth, center = chart placeholder, right = order form placeholder) at **≥1280px**, with a **stacked** layout on tablet (**768px–1279px**) that keeps depth tables readable (no clipped columns). Stories 1–3 already provide depth data, live updates, empty states, `tabular-nums`, and **Bids** / **Asks** text labels — this plan is **frontend layout and chrome only** (no API, SignalR, or persistence changes). **TopOfBookStrip** stays in the **left zone** above depth so BR-08 remains visible; wallet summary moves **below** the workspace grid so the left column is book-focused.

## Goals and non-goals

**Goals**

- G1: At **xl (1280px)+**, depth panel occupies the **left column** of a three-zone grid; center and right show labeled MVP placeholders.
- G2: **AAPL** is visible in trading **page chrome** (workspace header), not only footer or card subtitles.
- G3: Tablet (**md–lg**, 768–1279px) stacks zones vertically; depth tables remain usable (`min-w-0`, horizontal scroll on table container if needed).
- G4: While `useOrderBookQuery` is pending, **skeleton** (or explicit loading copy) appears **only in the left zone** — never a table of zeros.
- G5: Manual verification at **1280px** and **~768px**; BR-08 strip touch matches depth row 0 when both visible.

**Non-goals** (this plan will not do)

- NG1: Real candlestick chart (US-08) or functional order form (US-10+) — placeholders only.
- NG2: PRD top bar last trade / daily change (FR-2.2) — future spec.
- NG3: Phone layout (<768px) — out of scope per PRD §8.3.
- NG4: Backend, Redis, SignalR, or TanStack Query key changes.
- NG5: AppHost / `VITE_*` changes.

## Traceability matrix

| Spec story | Plan task(s) | Test evidence |
|------------|--------------|---------------|
| Story 4 — depth panel trading layout | Tasks 1–5 | Manual UI checklist (1280px + ~768px) |
| Stories 1–3 (regression) | Task 5 | Manual: live depth, empty copy, reconnect badge |
| BR-08 | Task 5 | Manual: strip vs depth row 0 |

## Architecture impact

```text
┌──────────────────────────────────────────────────────────────────────────────┐
│  AppLayout (unchanged header; footer still shows AAPL · MVP shell)              │
│  TradingPage                                                                  │
│    TradingWorkspaceHeader  — symbol AAPL + page title (NEW)                   │
│    TradingWorkspaceGrid    — xl: 3 columns | md–lg: stack (NEW)               │
│      LeftZone              — TopOfBookStrip + OrderBookDepthPanel (MOVED)       │
│      CenterZone            — ChartPlaceholder (NEW)                           │
│      RightZone             — OrderFormPlaceholder (NEW)                         │
│    Below grid              — VirtualCashCard (optional) + PortfolioActivityTabs │
└──────────────────────────────────────────────────────────────────────────────┘
         Data unchanged: useOrderBookQuery → shared cache → strip + depth
```

| Layer | Change summary |
|-------|----------------|
| Domain | None |
| Application | None |
| Infrastructure | None |
| Api | None |
| MatchingEngine | None |
| web/ | `TradingWorkspace*` layout components; refactor `trading-page.tsx`; minor depth panel/table responsive classes |
| AppHost | None |

## Data & migration plan

| Artifact | Action | DB reference |
|----------|--------|--------------|
| EF migration | None | — |
| Redis keys | None | — |
| Book recovery | N/A | — |

## Open questions

| # | Question | Source | Answer | Status |
|---|----------|--------|--------|--------|
| 1 | Tailwind breakpoint for PRD 1280px: `xl` (1280) or custom `min-[1280px]`? | PRD §8.3 | Use **`xl:`** (Tailwind default 1280px) unless visual QA shows mismatch | ✅ |
| 2 | Top-of-book strip: left zone vs global top bar? | Spec §4a coexistence | **Left zone**, stacked above depth (BR-08) | ✅ |
| 3 | VirtualCashCard placement after grid? | PRD §8.1 (wallet in holdings) | **Below** workspace grid, full width; not in left column | ✅ |
| 4 | Promote spec from `draft` before merge? | Spec status | Implement against AC; product can approve in parallel | ⏳ Deferred |

## Risks and mitigations

| Risk | Likelihood | Impact | Mitigation | Owner task |
|------|------------|--------|------------|------------|
| `md:max-w-2xl` on trading page blocks wide layout | High | High | Remove in Task 2; use grid `min-w-0` on columns | Task 2 |
| Depth tables clip on tablet | Medium | Medium | `min-w-0` on left column; reuse `Table` `overflow-x-auto` | Task 3 |
| Placeholder columns steal focus from depth | Low | Low | Muted placeholder styling; left column fixed width ~280–320px at xl | Task 1 |
| Stories 1–3 branches not on `main` | Medium | Medium | Rebase Story 4 branch onto latest US-06 work before `/build` | Prerequisites |

## Prerequisites

- [ ] US-06 Stories 1–3 merged or available on branch base (`OrderBookDepthPanel`, empty states, reconnect badge)
- [ ] Aspire local stack runs (`aspire run` or env-doctor)
- [ ] `yarn --cwd web lint` and `yarn --cwd web build` green on base branch

## File structure (planned)

```text
web/src/features/trading/
  components/
    trading-workspace-header.tsx       CREATE  AAPL chrome + title
    trading-workspace-grid.tsx         CREATE  xl 3-col / md stack slots
    trading-workspace-placeholders.tsx CREATE  chart + order form shells
  pages/
    trading-page.tsx                   MODIFY  compose workspace layout
web/src/features/market/components/
  order-book-depth-panel.tsx           MODIFY  optional min-h / zone class
  order-book-depth-table.tsx           REUSE   tabular-nums + Bids/Asks (verify AC)
```

## Authorization, session, and domain notes

- **Session model:** Unchanged — `ProtectedRoute` on `/trading`.
- **Route protection:** No new routes.
- **Domain rules:** **BR-08** only — strip and depth must continue to read the same `useOrderBookQuery` snapshot; do not split query keys or add Zustand book state.

## Progress tracker

### Task 1: Skeleton — trading workspace grid and placeholders

| Attribute | Value |
|-----------|--------|
| Spec story | Story 4 |
| Depends on | None |
| Estimated complexity | M |
| Parent story issue | [#73](https://github.com/tranvuongduy2003/trading-simulator/issues/73) |

#### Objective

`TradingPage` renders a **three-zone workspace** at `xl` with visible **Chart** and **Order form** placeholders, and a dedicated **left slot** (initially empty or with a “Order book zone” stub) so `/build` can verify layout at 1280px before wiring depth.

#### Implementation notes

- Add `TradingWorkspaceGrid` with named slots: `left`, `center`, `right` (children composition, not boolean props).
- Grid CSS (desktop): e.g. `xl:grid xl:grid-cols-[minmax(17rem,20rem)_1fr_minmax(16rem,18rem)] xl:gap-4 xl:items-start`.
- Below `xl`: `flex flex-col gap-4` (stacked zones).
- Placeholders: muted `Card` with title + one-line “Coming soon” — no hex colors; semantic tokens only.
- Add `TradingWorkspaceHeader` with **AAPL** badge (`Badge` or `span` with `aria-label="Symbol AAPL"`) beside “Trading” title.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `web/src/features/trading/components/trading-workspace-grid.tsx` | Responsive grid + slots |
| CREATE | `web/src/features/trading/components/trading-workspace-placeholders.tsx` | Chart / order placeholders |
| CREATE | `web/src/features/trading/components/trading-workspace-header.tsx` | AAPL chrome |
| MODIFY | `web/src/features/trading/pages/trading-page.tsx` | Use grid; stub left slot |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | 1280px → three columns visible | `web/` |
| Manual | 768px → zones stack vertically | `web/` |

#### Acceptance criteria

- [x] At **≥1280px**, left / center / right zones are distinct columns.
- [x] Below **1280px**, zones stack without horizontal page scroll from grid.
- [x] **AAPL** visible in workspace header.
- [x] `yarn --cwd web build` passes.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | PRD §8.1, §8.3; Tech §11 |
| Async matching | N/A |
| PostgreSQL authoritative | N/A |
| Redis projection | N/A |
| RFC 7807 errors | N/A |
| SignalR | N/A |
| Aspire | None |
| ADR needed? | No |

#### Risk

Low — layout-only; no data path changes.

---

### Task 2: Mount market panels in left zone (strip + depth)

| Attribute | Value |
|-----------|--------|
| Spec story | Story 4 |
| Depends on | Task 1 |
| Estimated complexity | M |
| Parent story issue | [#73](https://github.com/tranvuongduy2003/trading-simulator/issues/73) |

#### Objective

**TopOfBookStrip** and **OrderBookDepthPanel** live in the **left column** only; remove the narrow `md:max-w-2xl` wrapper that constrains book UI. Loading skeleton and error/retry remain in the left zone (no zero rows while pending).

#### Implementation notes

- Move existing `orderBookQuery` wiring into left slot: strip first, depth second (`flex flex-col gap-4`).
- Delete or relocate the old single-column market stack.
- Ensure pending state: `OrderBookDepthPanel` `isPending` shows skeleton inside left zone (already does — verify after grid move).
- **VirtualCashCard**: render **below** `TradingWorkspaceGrid`, not inside left column (aligns with PRD wallet in holdings area).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `web/src/features/trading/pages/trading-page.tsx` | Left zone = strip + depth |
| REUSE | `web/src/features/market/components/top-of-book-strip.tsx` | Unchanged props |
| REUSE | `web/src/features/market/components/order-book-depth-panel.tsx` | Mount in left zone |
| REUSE | `web/src/features/market/hooks/use-order-book-query.ts` | Single cache (BR-08) |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Loading → skeleton in **left** column only; center/right placeholders static | Aspire |
| Manual | Error → retry in depth panel; no fabricated rows | Aspire |

#### Acceptance criteria

- [x] Depth table is in **left panel zone** at 1280px+.
- [x] Snapshot pending shows **skeleton**, not zero-qty rows.
- [x] Strip + depth both visible in left zone; same query instance.
- [x] `yarn --cwd web lint` passes.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | PRD §8.1; BR-08 |
| Async matching | N/A |
| PostgreSQL authoritative | N/A |
| Redis projection | N/A |
| RFC 7807 errors | N/A |
| SignalR | Unchanged (Stories 1–2) |
| Aspire | None |
| ADR needed? | No |

#### Risk

Medium — regression on market UX if query wiring duplicated; keep one `useOrderBookQuery()` in `TradingPage`.

---

### Task 3: Tablet responsive polish (768px–1279px)

| Attribute | Value |
|-----------|--------|
| Spec story | Story 4 |
| Depends on | Task 2 |
| Estimated complexity | S |
| Parent story issue | [#73](https://github.com/tranvuongduy2003/trading-simulator/issues/73) |

#### Objective

On tablet widths, stacked layout keeps **Price / Size / Orders** columns readable without clipping; bid/ask tables remain side-by-side at `lg` within the left zone where spec Q1 allows.

#### Implementation notes

- Apply `min-w-0` to left grid column and depth card root.
- Confirm `OrderBookDepthPanel` inner layout: `flex flex-col gap-4 lg:grid lg:grid-cols-2` (bid/ask side-by-side from `lg` = 1024px) — acceptable inside stacked page at 768px+.
- Optional: `text-xs` table density on `<md` only if columns still clip after `overflow-x-auto`.
- Avoid `overflow-hidden` on ancestors of `Table` (kills horizontal scroll).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `web/src/features/trading/components/trading-workspace-grid.tsx` | `min-w-0`, gap tuning |
| MODIFY | `web/src/features/market/components/order-book-depth-panel.tsx` | Zone width classes if needed |
| REUSE | `web/src/components/ui/table.tsx` | `overflow-x-auto` container |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | ~768px width — no clipped table headers; horizontal scroll OK | DevTools |
| Manual | ~1024px — bid/ask tables side-by-side inside depth card | DevTools |

#### Acceptance criteria

- [x] At **768–1279px**, depth panel usable (no permanent column clip).
- [x] Page does not require horizontal scroll for entire layout (table-internal scroll OK).

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | PRD §8.3 |
| Async matching | N/A |
| PostgreSQL authoritative | N/A |
| Redis projection | N/A |
| RFC 7807 errors | N/A |
| SignalR | N/A |
| Aspire | None |
| ADR needed? | No |

#### Risk

Low — CSS-only.

---

### Task 4: Accessibility audit — tabular nums and Bid/Ask labels

| Attribute | Value |
|-----------|--------|
| Spec story | Story 4 |
| Depends on | Task 2 |
| Estimated complexity | S |
| Parent story issue | [#73](https://github.com/tranvuongduy2003/trading-simulator/issues/73) |

#### Objective

Confirm Story 4 happy-path **WCAG** items: numeric columns use **`tabular-nums`**; sides identified by **text** (“Bids” / “Asks” and column headers), not color alone (PRD §7.5, §8.2).

#### Implementation notes

- **Already implemented** in `order-book-depth-table.tsx` (`Bids`/`Asks` headings, `tabular-nums` on price/size/orders). Task is verify + small gaps only.
- If strip cells lack `tabular-nums`, add to `top-of-book-strip.tsx` value cells.
- Ensure table `aria-label` remains (`${sideLabel} depth levels`).
- Optional: add visible **“Bid”** / **“Ask”** screen-reader-only prefix on price cells only if audit finds color-only identification — prefer existing headings.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| REUSE | `web/src/features/market/components/order-book-depth-table.tsx` | Verify labels |
| MODIFY | `web/src/features/market/components/top-of-book-strip.tsx` | `tabular-nums` if missing |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Keyboard focus order: header → strip → depth → tabs | Aspire |
| Manual | Disable color in DevTools — sides still identifiable by text | Browser |

#### Acceptance criteria

- [x] Price and quantity columns use **tabular** alignment.
- [x] **Bid** and **Ask** sides identifiable without relying on green/red alone.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | PRD §7.5, §8.2; `design-system.mdc` |
| Async matching | N/A |
| PostgreSQL authoritative | N/A |
| Redis projection | N/A |
| RFC 7807 errors | N/A |
| SignalR | N/A |
| Aspire | None |
| ADR needed? | No |

#### Risk

None — verification-first.

---

### Task 5: Polish — manual checklist, BR-08, build/lint

| Attribute | Value |
|-----------|--------|
| Spec story | Story 4 · Polish |
| Depends on | Tasks 1–4 |
| Estimated complexity | S |
| Parent story issue | [#73](https://github.com/tranvuongduy2003/trading-simulator/issues/73) |

#### Objective

Sign off Story 4 AC at **1280px** and **~768px**; confirm Stories 1–3 behaviors unchanged; document operator checklist in plan.

#### Implementation notes

- Remove stale copy on `TradingPage` (“chart modules will mount here”) if redundant with placeholders.
- Run `yarn --cwd web lint`, `yarn --cwd web build`, `yarn --cwd web api:verify` (no API change expected; sanity check).
- Update `docs/memory/current-status.md` after `/build` (not in `/plan` session unless user requests).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `web/src/features/trading/pages/trading-page.tsx` | Copy cleanup |
| REUSE | `web/src/features/trading/components/portfolio-activity-tabs.tsx` | Bottom panel unchanged |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Full checklist below | Aspire |

#### Acceptance criteria

- [x] All Story 4 spec AC pass (layout, chrome, tablet, loading).
- [x] BR-08: strip best bid/ask === depth first row per side.
- [x] Stories 1–3: live update, empty copy, reconnect badge still work.
- [x] `yarn --cwd web lint` + `yarn --cwd web build` green.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | PRD §8.1–§8.3 |
| Async matching | N/A |
| PostgreSQL authoritative | N/A |
| Redis projection | N/A |
| RFC 7807 errors | N/A |
| SignalR | Regression check |
| Aspire | `aspire run` manual |
| ADR needed? | No |

#### Risk

None — verification.

## Reference files

| File | Why open it |
|------|-------------|
| `web/src/features/trading/pages/trading-page.tsx` | Current single-column layout to replace |
| `web/src/features/market/components/order-book-depth-panel.tsx` | Left-zone mount + skeleton |
| `web/src/features/market/components/order-book-depth-table.tsx` | Bids/Asks + tabular-nums |
| `web/src/layouts/app-layout.tsx` | Global chrome; footer AAPL |
| `docs/PRD.md` §8.1 | Zone definitions |
| `docs/plans/20260530-140000-live-order-book-depth-story-1.md` | Deferred layout note (NG3) now in scope |

## Implementation details (for /build)

- **Breakpoint:** PRD desktop **1280px** maps to Tailwind **`xl:`** (`min-width: 80rem`). Tablet stack uses default/mobile-first layout below `xl`.
- **Grid proportions:** Left ~280–320px fixed minmax at `xl`; center `1fr`; right ~260–288px — tune in Task 1 for density without crowding depth.
- **Query wiring:** Keep exactly one `useOrderBookQuery()` in `TradingPage`; pass `isPending` / `isError` / `data` to both strip and depth — do not create a second hook instance.
- **Loading copy:** Existing skeleton in `OrderBookDepthPanel` satisfies “skeleton or Loading order book…” — optional addition of visible “Loading order book…” text above skeleton if product prefers explicit string (not required if skeleton present).
- **Placeholders:** `ChartPlaceholder` / `OrderFormPlaceholder` — static text only; no `lightweight-charts` import in this story.
- **BR-08 check:** `deriveTopOfBook(orderBookQuery.data)` first bid/ask must match `snapshot.bids[0]` / `snapshot.asks[0]` when levels exist (manual compare at 1280px).

## Verification matrix (plan-level)

| Spec AC | Verified by |
|---------|-------------|
| ≥1280px depth in left zone | Task 2 manual + Task 5 |
| AAPL in chrome | Task 1 header + Task 5 |
| Tabular alignment | Task 4 + Task 5 |
| Bid/Ask text labels | Task 4 + Task 5 |
| Tablet stacked, no clip | Task 3 + Task 5 |
| Loading skeleton, not zeros | Task 2 + Task 5 |
| BR-08 | Task 5 manual |

## Manual UI checklist (operator)

- [ ] `aspire run` — log in, open `/trading`
- [ ] **1280px** viewport: left = strip + depth; center = chart placeholder; right = order placeholder; **AAPL** in page header
- [ ] **1280px**: prices/qty align in columns (`tabular-nums`); headings say **Bids** / **Asks**
- [ ] **1280px**: strip touch matches depth row 1 per side (BR-08)
- [ ] Throttle network — depth left zone shows **skeleton**, not `0` rows
- [ ] **~768px** viewport: zones stack; depth readable; scroll table horizontally only if needed
- [ ] Place order / hub update — depth still updates (Story 2 regression)
- [ ] Empty book — Story 3 copy still shows in depth tables
- [ ] `yarn --cwd web lint` && `yarn --cwd web build`

## Rollback / recovery

- **Code:** Revert branch `feature/live-order-book-depth-story-4`
- **DB:** N/A
- **Redis:** N/A

## Deferred work (Plan B)

- AppLayout top bar: last trade, daily change, symbol next to logo (FR-2.2 / PRD §8.1 top bar)
- Functional chart center (US-08) and order form right (US-10+)
- Zustand-persisted panel widths / collapse
- Playwright visual regression at 1280/768 (out of MVP test policy)

## GitHub Links

| Local ref | Issue # | Type | Title | URL |
|-----------|---------|------|-------|-----|
| spec Story 4 | 73 | Story | US-06 / Story 4: Depth panel trading layout | https://github.com/tranvuongduy2003/trading-simulator/issues/73 |
| spec epic | 69 | Epic | Spec: Live Order Book Depth (US-06) | https://github.com/tranvuongduy2003/trading-simulator/issues/69 |
