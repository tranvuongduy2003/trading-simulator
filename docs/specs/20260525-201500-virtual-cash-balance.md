---
artifact_type: spec
artifact_version: 1
id: spec-20260525-201500-virtual-cash-balance
title: Virtual Cash Balance Display
slug: virtual-cash-balance
filename_template: 20260525-201500-virtual-cash-balance.md
created_at: 2026-05-25T20:15:00+07:00
updated_at: 2026-05-25T20:45:00+07:00
status: draft
owner: product
tags: [spec, feature, trading-simulator, wallet, cash, balance, portfolio, us-03]
related_plan: docs/plans/20260525-240000-virtual-cash-story-4.md
related_plans: [docs/plans/20260525-203000-virtual-cash-story-1.md, docs/plans/20260525-220000-virtual-cash-story-2.md, docs/plans/20260525-230000-virtual-cash-story-3.md, docs/plans/20260525-240000-virtual-cash-story-4.md]
related_specs: [docs/specs/20260523-175509-user-registration.md, docs/specs/20260525-103709-user-login.md]
github_epic_issue: 33
github_story_issues: [34, 35, 36, 37]
prd_refs: [PRD §5.1 US-03, PRD §6.1 FR-1.3, PRD §6.6 FR-6.2, PRD §7.3, PRD §7.4, PRD §8.1, PRD §10.1]
tech_refs: [Tech §5.2.1 User/Wallet, Tech §6 GetMyWalletQuery, Tech §8.1 Wallet endpoint, Tech §9.2 user group, Tech §15.1]
db_refs: [DB §4.2 wallets, DB §5 invariants, DB §6.2 wallets indexes]
search_index:
  keywords: [wallet, cash, balance, available, reserved, total, virtual cash, USD, trading dashboard, GetMyWallet, session, read-your-writes, FR-6.2, US-03]
  bounded_contexts: [Trading]
  user_personas: [Aspiring Trader, Authenticated User]
---

> GitHub epic: [#33 Spec: Virtual cash balance display (US-03)](https://github.com/tranvuongduy2003/trading-simulator/issues/33)

# Feature: Virtual Cash Balance Display
> Status: DRAFT  |  Date: 2026-05-25
> PRD: PRD §5.1 US-03, §6.1 FR-1.3, §6.6 FR-6.2, §7.3–7.4, §8.1, §10.1
> Tech: Tech §5.2.1, §6, §8.1, §9.2, §15.1
> DB: DB §4.2, §5, §6.2
> Owner: Product

## 1. Problem & Solution

**Problem:** After registering or logging in, a user cannot confidently know how much virtual USD they can spend on the next order. Without a clear, trustworthy cash display, limit and market buy flows (US-10–US-15) feel opaque and error-prone.

**Solution:** On the authenticated main trading view, show the user’s virtual cash with **available** balance as the primary figure they trade against, plus **total** and **reserved** breakdown per FR-6.2. Balances come from the authoritative wallet in PostgreSQL; only the signed-in user may read their own wallet.

**Persona:** Authenticated trader (Aspiring Trader) using the local **AAPL** simulator after US-01 registration and/or US-02 login.

**Smallest valuable version:** Read-only cash display on the trading dashboard + protected `GET /api/wallet` + loading/error/session handling + balances that match server state after login and on manual refresh. Real-time SignalR wallet pushes, top-bar compact wallet chip, and portfolio reset (US-04) are Phase 2 / separate stories.

## 2. User Stories & Acceptance Criteria

### Story 1: See how much cash I can trade with
> As a **user**, I want to **see my available virtual cash balance prominently**, so that **I know how much I can use for the next buy order**.

**Happy path:**
- GIVEN I am authenticated and on the main trading view → WHEN the wallet loads within **2 s** (local MVP) → THEN I see a labeled **Virtual cash** (or equivalent) area showing **available** balance formatted as USD (e.g. **$100,000.00** for a new account).
- GIVEN I registered today and have no open buy orders → WHEN the wallet loads → THEN **available** equals **$100,000.00**, **total** equals **$100,000.00**, and **reserved** equals **$0.00** (FR-1.3 initial capital).
- GIVEN amounts are stored as `NUMERIC(18,4)` → WHEN displayed in the UI → THEN money shows **two** decimal places for USD; quantities elsewhere remain whole shares.

**Failure / edge path:**
- GIVEN wallet data is still loading → WHEN I view the trading screen → THEN I see a non-blocking loading state (skeleton or inline placeholder) in the cash area without layout jump that hides the rest of the page.
- GIVEN `GET /api/wallet` fails with a non-auth error (e.g. **500**) → WHEN the trading view renders → THEN I see a human-readable error (e.g. “Could not load account data. Try refreshing or sign in again.”) and no fabricated balance figures.

---

### Story 2: Understand total versus reserved cash
> As a **user**, I want to **see total and reserved cash alongside available**, so that **I understand why my spendable amount is lower when I have open buy orders**.

**Happy path:**
- GIVEN my wallet has **total** **$50,000.0000**, **reserved** **$10,000.0000** → WHEN I view the cash area → THEN **available** displays **$40,000.00** and secondary copy shows **Total $50,000.00 · Reserved $10,000.00** (or equivalent clear labels).
- GIVEN **reserved** is **$0.00** → WHEN I view the cash area → THEN reserved is still visible as **$0.00** or concise copy that none is tied up in open buys (no hidden reserved state).

**Failure / edge path:**
- GIVEN the API returns inconsistent numbers where `availableBalance` ≠ `totalBalance - reservedBalance` (data bug) → WHEN the UI renders → THEN the UI still shows the three fields returned by the API and does not invent a fourth figure; operators treat this as a defect against BR-03.

---

### Story 3: Only see my own balance when signed in
> As a **user**, I want **my cash balance to be private to my session**, so that **no one else can read my wallet through the app**.

**Happy path:**
- GIVEN I am authenticated as user A → WHEN `GET /api/wallet` runs → THEN the response `userId` matches my account and balances reflect **my** wallet only (never another user’s **$100,000** default unless that is actually my balance).
- GIVEN I just logged in as user B who previously traded → WHEN the trading view loads → THEN I do **not** briefly see user A’s cached balance from a prior session (client must not show stale wallet data across users).

**Failure / edge path:**
- GIVEN I am not authenticated (no valid session cookie) → WHEN the client calls `GET /api/wallet` → THEN HTTP **401** with stable `code` `UNAUTHORIZED`; the trading route redirects to login (or shows session-expired messaging per US-02).
- GIVEN my session expired → WHEN I open the trading view → THEN wallet fetch returns **401**, local auth state clears, and I am prompted to sign in again without displaying numeric balances.

---

### Story 4: Trust balances after login and refresh
> As a **user**, I want **the displayed cash to match the server after I sign in or refresh**, so that **I can place orders with confidence**.

**Happy path:**
- GIVEN I log in successfully → WHEN the trading view loads → THEN wallet data loads without requiring a second login and shows my current PostgreSQL balances (read-your-writes within **2 s**).
- GIVEN I am on the trading view and press browser refresh → WHEN the page reloads with a valid session → THEN the same balances reappear (session persistence per FR-1.2).
- GIVEN I return to the trading tab after placing or cancelling a buy order in a later release → WHEN the wallet query refetches (focus, interval, or post-mutation invalidation) → THEN **available**, **total**, and **reserved** reflect the latest reserved amounts for open buys (BR-04). *Until order placement ships, manual refresh must show the same values as `GET /api/wallet`.*

**Failure / edge path:**
- GIVEN I had the trading view open before a trade filled elsewhere (second tab) → WHEN I focus the tab or refresh → THEN balances update to the server state on the next successful wallet fetch (no permanent stale **$100,000** if reserved cash changed).
- GIVEN PostgreSQL is temporarily unavailable → WHEN `GET /api/wallet` runs → THEN HTTP **500** and the UI error path from Story 1; no partial wallet row is shown.

## 3. Domain & Business Rules

```
BR-01: One wallet per user. Wallet rows are keyed by user_id; a user never has two wallet records (DB §5, Tech §5.2.1).

BR-02: Available cash = total_balance − reserved_balance. Available is computed at read time, not stored (DB §4.2). It is the amount the user can allocate to new buy orders (PRD FR-3.3, FR-6.2).

BR-03: Wallet invariants: total_balance ≥ 0, reserved_balance ≥ 0, reserved_balance ≤ total_balance, and available ≥ 0 always (Tech §5.2.1, PRD §7.3). Violations are system defects, not user-facing states.

BR-04: Reserved balance increases when cash is reserved for open buy orders and decreases on cancel or fill settlement (Tech §5.2.1). US-03 display must reflect reservations once order placement exists; until then reserved stays 0 for new users.

BR-05: Initial virtual cash for new registrations is USD 100,000.0000 with reserved 0 (PRD FR-1.3, registration spec BR-02). Currency is USD only for MVP (`wallets.currency` = USD).

BR-06: Virtual cash is non-redeemable simulation money (PRD §3.2). UI copy may remind users it is not real money where appropriate; no withdrawal or transfer flows.

BR-07: Wallet reads are scoped to the authenticated session user. Cross-user wallet access is forbidden at the application layer (Tech §15.1).

BR-08: PostgreSQL is authoritative for wallet balances; Redis does not store wallet projections for MVP (Tech §3, DB §4.2). Clearing Redis does not change displayed cash after refetch.
```

## 4. UI Behavior **or** API Contract

### 4a. UI Behavior

```
Screen: Main trading view (authenticated)
- Arrival: User lands after login/register redirect or navigates to /trading (or equivalent). Cash area visible without extra navigation (PRD §7.4 — portfolio/cash reachable within 2 clicks; on-dashboard display satisfies US-03 MVP).
- Primary display: "Virtual cash" card on trading view showing available balance large (tabular numerals), subtitle "Available to trade".
- Top bar (all authenticated routes): compact chip with **available** cash (PRD §8.1, spec §13 Q1 ✅) — label "Cash" + formatted amount; loading skeleton / no fake balance on error.
- Secondary display: Total and reserved on one line or tooltip-friendly secondary text (FR-6.2).
- Loading: Skeleton or inline placeholder in the cash card while wallet (and optionally portfolio) queries are pending; do not show $0.00 as if real.
- Empty: N/A for wallet (every authenticated user has a wallet row). If API returns 404 for wallet (defect), show error state, not empty zero balance.
- Error: Non-401 failures → destructive text + suggestion to refresh or sign in again. 401 → session flow (redirect login / session expired banner per login spec).
- Real-time (Phase 1): No SignalR requirement for US-03; refetch on mount, window focus, and after auth-changing mutations is sufficient. Future: user-group balance push when order stories ship.
```

```
Screen: Registration success / Login success (transition)
- After 201 register or 200 login, trading view must load wallet and show starting **$100,000.00** available for a new account without an extra manual step.
```

### 4b. API Contract

- **Endpoint:** `GET /api/wallet`
- **Auth:** Session required (cookie). Public routes: register/login only (Tech §15.1).
- **Response 200 (JSON sketch):**
  ```json
  {
    "userId": "<uuid>",
    "username": "trader_jane",
    "currency": "USD",
    "totalBalance": "100000.0000",
    "reservedBalance": "0.0000",
    "availableBalance": "100000.0000"
  }
  ```
  Monetary fields may serialize as string or number; client normalizes to decimal for display.
- **Errors:**
  - **401** `UNAUTHORIZED` — missing, expired, or revoked session.
  - **500** `INTERNAL_ERROR` — infrastructure failure; no wallet body.
- **Idempotency:** Read-only; safe to repeat. Used as session probe in auth flows (login spec).
- **Pagination:** N/A (single wallet row per user).

## 5. Data & Storage Impact

| Concern | Answer |
|---|---|
| PostgreSQL tables / columns | **Read** `wallets` (`user_id`, `total_balance`, `reserved_balance`, `currency`, `updated_at`, `row_version`). No new columns for US-03. |
| Redis keys / projections | **None** for wallet in MVP. |
| Matching / channel behavior | **None** for read path. Writes to `wallets` occur on order place/cancel/match (future order stories); US-03 consumes results via GET. |
| Migration needed | **No** if `wallets` table already deployed (registration). |
| Rebuild strategy if Redis cleared | **N/A** — wallet not in Redis. |

Cross-check: `pk_wallets` on `user_id` (DB §6.2); check constraints on non-negative balances (DB §7).

## 6. Real-Time & Consistency

- **SignalR events:** Not required for US-03 Phase 1. Tech §9.2 user group may later emit balance updates on fill/cancel; defer to order/portfolio stories. MVP uses HTTP refetch.
- **Read-your-writes:** After login/register, first `GET /api/wallet` on the trading view must return current PostgreSQL balances within **2 s** — not zero, not another user’s data.
- **Stale UI handling:** On session user change (logout/login as different user), clear wallet query cache. On tab focus or manual refresh, refetch wallet. When order placement ships, invalidate wallet query after successful place/cancel mutations.

## 7. Security & Privacy (MVP)

- **Authn / Authz:** Session identifies user; wallet query returns only the caller’s row (BR-07).
- **Sensitive fields:** Balances are not secrets but are personal financial simulation data — never log full wallet responses at info level in shared logs.
- **Threat surface:** Guessing another user’s `userId` must not change wallet results; no `userId` query parameter on GET. Session fixation mitigated by login spec; US-03 relies on existing session model.

## 8. Observability (local MVP)

| Signal | What to emit |
|---|---|
| Structured logs | Optional debug on wallet read failures; log `userId` only at debug. No balance values required in MVP logs. |
| Traces | Span on `GetMyWallet` handler if ServiceDefaults enabled — minimal for MVP. |
| Metrics | `minimal for MVP` |
| Audit | N/A (read-only) |

## 9. Edge Cases

```
EC-01: New user, no trades, no open orders → available = total = $100,000.00, reserved = $0.00.

EC-02: Open limit buy reserved $5,000 → total unchanged, reserved $5,000, available reduced by $5,000 (display all three).

EC-03: Session expired mid-view → wallet GET 401 → clear auth, redirect login, hide balances.

EC-04: User B logs in after User A on same browser → no display of A’s balances after B’s session is established.

EC-05: Wallet API 500 → error message, no fake zeros.

EC-06: Very large balance within NUMERIC(18,4) → UI formats without scientific notation; no overflow in display layer.

EC-07: Concurrent wallet read during matching settlement → READ COMMITTED may show slightly stale reserved until refetch; acceptable for MVP if refetch on focus (document in plan).

EC-08: Portfolio reset (US-04, future) → wallet returns to $100,000 total, $0 reserved; US-03 display must update after refetch when that feature exists.
```

## 10. Dependencies, Risks, Decision Triggers

- **Depends on:** US-01 Registration (wallet row + initial capital), US-02 Login (session + protected routes). `wallets` schema and `GET /api/wallet` contract.
- **Impacts:** US-10–US-15 order placement (users compare available cash to order cost); US-15 validation messaging; US-23 portfolio valuation (cash component).
- **External services:** PostgreSQL (authoritative); session store (auth gate only).
- **Key risk:** Client shows cached wallet from a previous user or stale TanStack Query data after logout — mitigated by cache clear on auth transitions (Story 3).
- **Decision triggers:** If cash moves to top bar only (PRD §8.1), update layout spec and ensure still within 2-click rule — add to `docs/memory/decisions.md`.

## 11. Assumptions

- **Confirmed (product):** Main trading view is the primary surface for US-03 MVP (not only the future Holdings tab).
- **Confirmed (product):** `GET /api/wallet` already exists in the API contract; US-03 is end-to-end correctness and UX, not inventing a new resource.
- **Assumed:** Phase 1 does not require SignalR wallet events; HTTP refetch is acceptable (PRD real-time targets apply to market data and fills, not strictly to cash read on first delivery).
- **Assumed:** Registration/login flows already navigate to trading view; US-03 tightens display and cross-user cache rules.

## 12. Out of Scope

- Placing, cancelling, or matching orders (US-10–US-19) — only display impact when reserves exist.
- Holdings detail, unrealized P&L, total portfolio value (US-20, US-21, US-23) — may appear adjacent on trading view but not part of US-03 acceptance.
- Portfolio reset UI and behavior (US-04).
- Real-time SignalR push for wallet-only updates (Phase 2 enhancement).
- Full PRD §8.1 top bar (AAPL symbol, last price, daily change) — Story 1 adds **available cash chip only** per Q1 ✅.
- Multi-currency, fractional shares, multi-symbol.
- Global MVP exclusions: message broker, outbox, production CD, horizontal scaling.

## 13. Open Questions

| # | Question | Source | Answer | Status |
|---|---|---|---|---|
| 1 | Should available cash also appear in the top bar before the full terminal layout ships? | PRD §8.1 | **Yes** — compact available-cash chip in `AppLayout` on all authenticated routes (Story 1); full terminal top bar (symbol, price) still deferred | ✅ |
| 2 | After order placement ships, is refetch-on-mutation enough or is SignalR wallet push required for NFR latency? | Tech §9.2 | — | ⏳ Deferred to order epic |
