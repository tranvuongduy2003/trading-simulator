---
artifact_type: plan
artifact_version: 1
id: plan-20260525-203000-virtual-cash-story-1
title: Virtual Cash Balance — Story 1 (See available cash)
slug: virtual-cash-story-1
filename_template: 20260525-203000-virtual-cash-story-1.md
created_at: 2026-05-25T20:30:00+07:00
updated_at: 2026-05-25T21:30:00+07:00
status: active
owner: engineering
tags: [plan, implementation, trading-simulator, wallet, cash, us-03, story-1]
related_spec: docs/specs/20260525-201500-virtual-cash-balance.md
related_plans: []
prd_refs: [PRD §5.1 US-03, PRD §6.1 FR-1.3, PRD §6.6 FR-6.2, PRD §7.3, PRD §8.1]
tech_refs: [Tech §5.2.1, Tech §6, Tech §8.1, Tech §15.1, Tech §17.3]
db_refs: [DB §4.2 wallets, DB §5 invariants, DB §6.2]
github:
  repo: tranvuongduy2003/trading-simulator
  epic_issue: 33
  story_issue_ids: [34]
  last_synced_at: 2026-05-25T20:30:00+07:00
search_index:
  keywords: [wallet, virtual cash, available balance, top bar, AppLayout, GET api wallet, GetMyWalletQuery, useWalletQuery, trading-page, formatUsd, skeleton, 500 error, BR-02, BR-05, EC-01, EC-05, US-03 story-1]
  bounded_contexts: [Trading]
  task_count: 6
---

# Implementation Plan: Virtual Cash Balance — Story 1

| Field | Value |
|-------|--------|
| Spec | `docs/specs/20260525-201500-virtual-cash-balance.md` (§2 Story 1) |
| GitHub story | [#34 — See how much cash I can trade with](https://github.com/tranvuongduy2003/trading-simulator/issues/34) |
| Epic | [#33 — Virtual cash balance display (US-03)](https://github.com/tranvuongduy2003/trading-simulator/issues/33) |
| Depends on | US-01 registration (wallet row + $100k), US-02 login (session + protected routes) — **already shipped** |
| Status | Automation complete — manual sign-off pending |
| Tasks | 6 |
| Branch | `feature/virtual-cash-story-1` |
| Aspire impact | No |
| Schema impact | No |
| Test levels | Domain (reuse) · API integration (new focused suite) · Manual UI |
| ADRs required | ADR-004 (top-bar available cash) |
| GitHub | Synced 2026-05-25 — see §GitHub Links |

## Executive summary

Story 1 (US-03) requires the authenticated trading view to show **available virtual cash** prominently, with correct USD formatting, non-blocking loading in the cash area, and honest error handling when `GET /api/wallet` fails. **Product confirmed (spec §13 Q1):** available cash also appears in the **app top bar** on every authenticated route (compact chip), aligned with PRD §8.1. The vertical slice **already exists** end-to-end: `GetMyWalletQuery` + `WalletEndpoint`, registration/login integration tests asserting **$100,000** balances, and `trading-page.tsx` with a “Virtual cash” card using `formatUsd` (two decimals). This plan **hardens** Story 1: dedicated wallet API tests (including **500**), decouples wallet UI state from portfolio fetch failures, shared `useWalletQuery`, dashboard `VirtualCashCard`, top-bar chip in `AppLayout`, and manual verification. Stories 2–4 stay out of scope.

## Goals and non-goals

**Goals**

- G1: Story 1 acceptance criteria pass with traceable automated tests (`GetMyWalletTests` or equivalent).
- G2: Cash card loads independently — portfolio slow/failed does not hide wallet or block the whole trading grid.
- G3: On wallet **500**, show spec copy and **no** numeric balance (EC-05).
- G4: Display **available** as primary figure; **$100,000.00** for new users (EC-01, BR-05).
- G5: USD display uses exactly **two** decimal places (BR-02 read path via `formatUsd`).
- G6: **Top bar** shows compact **available** cash on all `AppLayout` routes (Trading, Portfolio, Orders) via shared wallet query — no duplicate fetch.

**Non-goals**

- NG1: Story 2 — reserved/total breakdown UX emphasis beyond existing secondary line (top bar shows **available** only, not total/reserved).
- NG2: Story 3 — cross-user TanStack cache hardening (logout/login B) — separate issue #36.
- NG3: Story 4 — refetch-on-focus / read-your-writes polish — issue #37.
- NG4: SignalR wallet push, order placement reserves.
- NG5: New tables/migrations, matching engine, Redis wallet projections.
- NG6: Full PRD §8.1 terminal (symbol, last price, daily change in top bar) — placeholders OK; **cash chip only** for Story 1.

## Traceability matrix

| Spec story | Plan task(s) | Test evidence |
|------------|--------------|---------------|
| Story 1 — prominent available cash | Task 2, 3, 4 | Manual: trading card + top bar chip show available |
| PRD §8.1 top-bar cash (Q1 = Yes) | Task 4 | Manual: chip visible on Portfolio/Orders routes |
| Story 1 — new user $100k / reserved 0 | Task 1 | `GetMyWallet_AfterRegister_Returns100kAvailable` (or reuse pattern from `RegisterUserTests`) |
| Story 1 — two decimal USD display | Task 3, 5 | `formatUsd` + manual visual check |
| Story 1 — wallet loading skeleton | Task 2 | Manual: skeleton in cash card only; holdings may load separately |
| Story 1 — API 500 no fake balance | Task 1, 2 | `GetMyWallet_WhenReadFails_Returns500`; UI shows error, no `$0` |
| BR-02 available = total − reserved | Task 1 | Handler already computes; assert in integration test |
| BR-05 / BR-06 | Task 1 | Register + wallet GET asserts USD 100k virtual |

## Architecture impact

```text
┌─────────────────┐     useWalletQuery (queryKey ['wallet'])
│ AppLayout       │ ──┐
│ WalletTopBarChip│   │  GET /api/wallet   ┌─────────────────────┐
└─────────────────┘   ├──────────────────► │ WalletEndpoint      │
┌─────────────────┐     │                  │  → GetMyWalletQuery │
│ TradingPage     │ ──┘                  │  → WalletReadRepo     │
│ VirtualCashCard │ ◄── WalletResponse     └─────────────────────┘
└─────────────────┘
       │ portfolio query (isolated from wallet UI)
       ▼
```

| Layer | Change summary |
|-------|----------------|
| Domain | **REUSE** — `Wallet.AvailableBalance`, `UserRegisterTests` initial cash |
| Application | **REUSE** — `GetMyWalletQueryHandler`; optional **none** unless 500 mapping gap found |
| Infrastructure | **CREATE** test fake `ThrowOnWalletReadRepository` only in integration tests |
| Api | **MODIFY** — add `Produces 500` on wallet endpoint if missing; no handler logic change expected |
| MatchingEngine | None |
| web/ | **CREATE** `use-wallet-query`, `virtual-cash-card`, `wallet-top-bar-chip`; **MODIFY** `trading-page.tsx`, `app-layout.tsx` |
| AppHost | None |

## Data & migration plan

| Artifact | Action | DB reference |
|----------|--------|--------------|
| EF migration | **None** | — |
| Redis keys | **None** | DB §12 (wallet not in Redis) |
| Book recovery | N/A | — |

## Open questions

| # | Question | Source | Answer | Status |
|---|----------|--------|--------|--------|
| 1 | Top-bar wallet before full terminal? | Spec §13 | **Yes** — compact available-cash chip in `AppLayout` header on all authenticated routes; dashboard card retained | ✅ Answered |
| 2 | Reuse scattered wallet tests vs new `GetMyWalletTests` class? | Code review | **Dedicated class** for Story 1 traceability; keep existing register/login tests unchanged | ✅ Answered |
| 3 | Portfolio failure should hide holdings only? | Spec §4a loading | **Yes** — wallet card renders on its own query state | ✅ Answered |

## Risks and mitigations

| Risk | Likelihood | Impact | Mitigation | Owner task |
|------|------------|--------|------------|------------|
| Coupled loading hides cash when portfolio fails | M | M | Split query pending/error UI per card | Task 2 |
| Showing `$0` during load | L | H | Skeleton until `walletQuery.isSuccess`; never render card with zero as placeholder | Task 2 |
| Duplicate wallet test maintenance | L | L | New tests assert Story 1 AC only; don’t duplicate full register flows | Task 1 |
| `useSession` + `['wallet']` key divergence | L | M | Shared `useWalletQuery` for layout + trading; session uses `['auth','session']` — document in Task 6 manual | Task 6 |
| Top bar crowded on narrow desktop | L | L | Compact chip: short label + amount; truncate; hide label below `md` if needed | Task 4 |

## Prerequisites

- [x] US-01 / US-02 shipped (`wallets` table, session auth)
- [x] `GET /api/wallet` implemented
- [ ] Spec status may remain draft — proceed per issue #34 scope
- [ ] Local Aspire stack for manual checklist

## File structure (planned)

```text
tests/TradingSimulator.Api.IntegrationTests/
  Users/GetMyWalletTests.cs                    CREATE
  Users/Fakes/ThrowOnWalletReadRepository.cs CREATE

src/Api/Endpoints/WalletEndpoint.cs            MODIFY (Produces 500)

web/src/features/trading/
  hooks/use-wallet-query.ts                    CREATE
  components/virtual-cash-card.tsx             CREATE
  components/wallet-top-bar-chip.tsx             CREATE
  pages/trading-page.tsx                       MODIFY

web/src/layouts/app-layout.tsx                 MODIFY

web/src/lib/format.ts                          REUSE
docs/memory/decisions.md                       MODIFY (ADR-004)
contracts/openapi/api.v1.yaml                  MODIFY (if 500 missing)
```

## Authorization, session, and domain notes

- **Session model:** Cookie session; `GET /api/wallet` requires auth (`RequireAuthorization`). Story 1 assumes user already reached trading view via login/register.
- **Domain rules (must not violate):**
  - BR-02: `availableBalance` = `totalBalance - reservedBalance` (computed in `GetMyWalletQueryHandler`, not stored).
  - BR-05: New users **$100,000** total, **$0** reserved.
  - BR-06: Virtual simulation money — label “Virtual cash” in UI.
- **Story 1 does not** implement BR-07 cross-user cache clearing (Story 3).

## Progress tracker

### Task 1: Add Story 1 wallet API integration tests

| Attribute | Value |
|-----------|--------|
| Spec story | Story 1 |
| Depends on | None |
| Estimated complexity | M |
| Parent story issue | #34 |

#### Objective

Automated proof that `GET /api/wallet` satisfies Story 1 happy and failure paths: **200** with correct balances for a new user, **401** without session, **500** when read repository throws, with RFC 7807 `INTERNAL_ERROR`.

#### Implementation notes

- **CREATE** `GetMyWalletTests` using existing `UsersApiFixture` + Testcontainers pattern from `RegisterUserTests` / `LoginUserTests`.
- Happy path: register (or login) → GET wallet → assert `AvailableBalance`, `TotalBalance` = **100000**, `ReservedBalance` = **0**, `Currency` = **USD**, `available` = `total - reserved`.
- **CREATE** `ThrowOnWalletReadRepository` implementing `IWalletReadRepository` that throws on `GetByUserIdAsync`; register via `fixture.CreateFactory(ConfigureThrowOnWalletRead)`.
- Assert **500** + `code` **INTERNAL_ERROR**; response body must not deserialize as wallet.
- Do **not** remove existing wallet assertions in register/login tests (regression safety).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `tests/Api.IntegrationTests/Users/GetMyWalletTests.cs` | Story 1 API AC |
| CREATE | `tests/Api.IntegrationTests/Users/Fakes/ThrowOnWalletReadRepository.cs` | Simulate read failure |
| REUSE | `tests/Api.IntegrationTests/Users/RegisterUserTests.cs` | Fixture/helpers pattern |
| REUSE | `src/Application/Users/Queries/GetMyWalletQueryHandler.cs` | No change expected |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `GetMyWallet_WithoutSession_Returns401` | `GetMyWalletTests.cs` |
| Integration | `GetMyWallet_AfterRegister_Returns100kBalances` | `GetMyWalletTests.cs` |
| Integration | `GetMyWallet_WhenReadFails_Returns500_INTERNAL_ERROR` | `GetMyWalletTests.cs` |

#### Acceptance criteria

- [x] All three integration tests pass under Docker Testcontainers
- [x] Existing Users integration suite still green
- [x] `availableBalance` equals `totalBalance - reservedBalance` on 200 response

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | PRD FR-1.3, FR-6.2; Tech §6, §8.1; DB §4.2 |
| Async matching | N/A (read-only) |
| PostgreSQL authoritative | Read via `WalletReadRepository` |
| Redis projection | N/A |
| RFC 7807 errors | 500 `INTERNAL_ERROR` |
| SignalR | N/A |
| Aspire | None |
| ADR needed? | No |

#### Risk

None — test-only plus test fake.

---

### Task 2: Isolate wallet loading and error UI on trading view

| Attribute | Value |
|-----------|--------|
| Spec story | Story 1 |
| Depends on | Task 1 |
| Estimated complexity | M |
| Parent story issue | #34 |

#### Objective

Wallet card has its own loading skeleton and error message; portfolio query pending/error does not suppress cash display or block the entire account grid.

#### Implementation notes

- Replace combined `isLoading` / `hasError` on `trading-page.tsx` with **per-query** state for wallet vs portfolio.
- Wallet area: `walletQuery.isPending` → skeleton inside cash card bounds only; `walletQuery.isError` → spec error copy (`role="alert"`); **do not** render balance figures on error.
- Holdings card: independent skeleton/error (optional muted error) so rest of page remains usable.
- Keep `queryKey: ['wallet']` and `staleTime: 30_000` per frontend rules.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `web/src/features/trading/pages/trading-page.tsx` | Decouple query UI states (card extraction in Task 3) |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Wallet loads while portfolio pending — cash skeleton visible, page header visible | Aspire |
| Manual | Simulate portfolio 500 (if possible) — wallet still shows if wallet 200 | Aspire |

#### Acceptance criteria

- [x] Loading skeleton appears only in virtual cash area, not full-page blocking grid
- [x] Wallet **500** shows error text; no `$0.00` or stale numbers
- [x] `yarn --cwd web build` passes

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | PRD §8.1 layout; spec §4a loading |
| RFC 7807 errors | Map `ApiError` for wallet query |
| Aspire | None |

#### Risk

Low — UI-only; verify no layout shift regressions.

---

### Task 3: Shared useWalletQuery and VirtualCashCard

| Attribute | Value |
|-----------|--------|
| Spec story | Story 1 |
| Depends on | Task 2 |
| Estimated complexity | M |
| Parent story issue | #34 |

#### Objective

Single TanStack Query hook (`useWalletQuery`) powers wallet reads; `VirtualCashCard` on the trading page shows **Virtual cash**, large **available**, and secondary total/reserved with two-decimal USD.

#### Implementation notes

- **CREATE** `useWalletQuery` — `queryKey: ['wallet']`, `queryFn: authApi.getWallet`, `staleTime: 30_000`, `normalizeWallet` on success. Replace inline `useQuery` in `trading-page.tsx`.
- **CREATE** `VirtualCashCard` — props from query state: `isPending`, `isError`, `wallet` (normalized). Move error copy constant to shared module if top bar reuses it.
- Apply design-system tokens: `Card`, `tabular-nums`, `text-muted-foreground` for secondary line.
- **REUSE** `formatUsd` — two decimal places.
- Primary emphasis on **available**; total/reserved remain secondary (Story 2 refines copy later).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `web/src/features/trading/hooks/use-wallet-query.ts` | Shared wallet query |
| CREATE | `web/src/features/trading/components/virtual-cash-card.tsx` | Dashboard cash card |
| MODIFY | `web/src/features/trading/pages/trading-page.tsx` | Use hook + card |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | New account shows **$100,000.00** available on trading card | Aspire |

#### Acceptance criteria

- [x] Card matches spec labels (“Virtual cash”, “Available to trade”)
- [x] Amounts display as `$X,XXX.XX` (two decimals)
- [x] `yarn --cwd web lint` passes

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| design-system.mdc | shadcn Card, tabular nums, no raw hex |
| frontend.mdc | Server state in Query only; same `queryKey` for layout reuse in Task 4 |

#### Risk

None — refactor for clarity.

---

### Task 4: Top-bar available cash chip (PRD §8.1, spec Q1)

| Attribute | Value |
|-----------|--------|
| Spec story | Story 1 |
| Depends on | Task 3 |
| Estimated complexity | M |
| Parent story issue | #34 |

#### Objective

On every authenticated `AppLayout` screen, the header shows a compact **available** virtual cash figure (e.g. **$100,000.00**) so users see spendable cash without navigating to Trading.

#### Implementation notes

- **CREATE** `WalletTopBarChip` — uses `useWalletQuery` (deduped with trading page).
- **MODIFY** `app-layout.tsx` — mount chip in header between `<nav>` and `<UserMenu>`: layout `flex items-center gap-2`.
- **Display:** Short label “Cash” (visible `sm:` and up) + `formatUsd(availableBalance)` in `tabular-nums`; `aria-label` e.g. “Available virtual cash {amount}”.
- **Loading:** inline `Skeleton` (~`h-5 w-24`), not full header block.
- **Error:** show em dash or “Unavailable” — **no** numeric placeholder (EC-05). Do not duplicate the long trading-page error paragraph in the header.
- **Auth:** render only when `useAuthStore` status is `authenticated` (same as `UserMenu`).
- Header already uses `viewTransitionName: 'site-header'` — chip lives inside isolated header; no extra VT.
- Symbol / last price / daily change remain out of scope (NG6).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `web/src/features/trading/components/wallet-top-bar-chip.tsx` | Compact header cash |
| MODIFY | `web/src/layouts/app-layout.tsx` | Insert chip in top bar |
| MODIFY | `docs/memory/decisions.md` | ADR-004 |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Chip shows **$100,000.00** on Trading, Portfolio, Orders after login | Aspire |
| Manual | Navigate Portfolio → Trading — single wallet fetch (Network tab, optional) | Aspire |
| Manual | Wallet API failure — chip shows no fake balance | Aspire |

#### Acceptance criteria

- [x] Available cash visible in top bar on all main nav routes
- [x] Two-decimal USD formatting matches dashboard card
- [x] Loading skeleton in chip area only; user menu still usable
- [x] ADR-004 recorded in `docs/memory/decisions.md`
- [x] `yarn --cwd web build` passes

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD §8.1 | Top bar includes available cash (other top-bar fields deferred) |
| design-system.mdc | Tabular nums, semantic tokens, `aria-label` on compact readout |

#### Risk

Low — header width; test at 1280px per PRD.

---

### Task 5: OpenAPI and contract alignment for wallet errors

| Attribute | Value |
|-----------|--------|
| Spec story | Story 1 (API contract) |
| Depends on | Task 1 |
| Estimated complexity | S |
| Parent story issue | #34 |

#### Objective

Committed OpenAPI documents `GET /api/wallet` **401**, **404**, and **500** responses; `yarn --cwd web api:verify` passes.

#### Implementation notes

- **MODIFY** `WalletEndpoint` — `.ProducesProblem(StatusCodes.Status500InternalServerError)` (matches Auth/Users endpoints).
- Run `yarn --cwd web api:export` and commit `contracts/openapi/api.v1.yaml` only.
- Follow `openapi-contract-sync` skill checklist.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `src/Api/Endpoints/WalletEndpoint.cs` | OpenAPI metadata |
| MODIFY | `contracts/openapi/api.v1.yaml` | Exported contract |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| CI | `yarn --cwd web api:verify` | — |

#### Acceptance criteria

- [x] `api:verify` green locally
- [x] Wallet path lists 500 response in YAML

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| api-guidelines.mdc | RFC 7807 on errors |

#### Risk

None.

---

### Task 6: Polish, regression, and manual sign-off

| Attribute | Value |
|-----------|--------|
| Spec story | Story 1 \| Polish |
| Depends on | Tasks 1–5 |
| Estimated complexity | S |
| Parent story issue | #34 |

#### Objective

Full regression run, manual Story 1 checklist on Aspire, update tracking docs; ready for PR to `main` closing #34.

#### Implementation notes

- Run `dotnet test` on Users + new wallet tests; `yarn --cwd web lint` + `build`.
- Manual checklist (operator):
  1. Register new user → trading view → **$100,000.00** available on **dashboard card** and **top bar**, reserved **$0.00** on card.
  2. Open **Portfolio** and **Orders** → top bar still shows same available amount.
  3. Refresh page with valid session → same balances within 2 s (card + chip).
  4. Throttle network (DevTools) → cash skeleton in card and chip; page usable.
  5. (Optional) Stop API / break DB → trading card error message; top bar shows no fake balance.
- Note: `useSession` uses `['auth','session']`; display uses `useWalletQuery` (`['wallet']`) — both hit `GET /api/wallet`; TanStack dedupes concurrent mounts.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `docs/memory/current-status.md` | Post-build status |
| MODIFY | `docs/CHANGELOG.md` | Impl entry after `/build` |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | Full `TradingSimulator.Api.IntegrationTests` Users suite | — |
| Manual | Story 1 checklist above | — |

#### Acceptance criteria

- [x] All automated tests green (**54** Users Testcontainers; excludes `RegisterUserSessionTests` — local Postgres)
- [ ] Manual checklist signed off by operator (see §Manual UI checklist)
- [x] No regression to login/register/logout flows (included in Users suite)
- [x] Spec §13 Q1 implemented (top-bar chip + ADR-004)

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| Epic regression | BR-02, BR-05, BR-06 unchanged |

#### Risk

None.

## Manual UI checklist (operator)

Run on Aspire (`feature/virtual-cash-story-1`). Sign off when all pass.

1. Register new user → **Trading** → dashboard card and top bar show **$100,000.00** available; card shows reserved **$0.00**.
2. Open **Portfolio** and **Orders** → top bar shows the same available amount.
3. Refresh with valid session → balances unchanged within ~2 s (card + chip).
4. DevTools network throttle → skeleton in cash card and chip only; page remains usable.
5. (Optional) Break wallet API → trading card shows error copy; top bar shows **Unavailable** (no fake balance).

## Reference files

| File | Why open it |
|------|-------------|
| `docs/specs/20260525-201500-virtual-cash-balance.md` | Story 1 AC source |
| `web/src/features/trading/pages/trading-page.tsx` | Current combined loading/error |
| `src/Application/Users/Queries/GetMyWalletQueryHandler.cs` | Available balance computation |
| `src/Api/Endpoints/WalletEndpoint.cs` | Route + OpenAPI |
| `tests/Api.IntegrationTests/Users/RegisterUserTests.cs` | Wallet 200 + $100k pattern |
| `tests/Api.IntegrationTests/Users/Fakes/ThrowOnCreateSessionStore.cs` | Test double pattern for 500 |
| `web/src/lib/format.ts` | `formatUsd` two-decimal rule |
| `web/src/layouts/app-layout.tsx` | Top bar insertion point |
| `docs/plans/20260525-180000-user-login-story-4.md` | Plan template + GitHub sync style |

## Implementation details (for /build)

### Backend (likely unchanged)

- `GetMyWalletQuery` → `GetMyWalletQueryHandler` uses `ICurrentUserAccessor` + `IWalletReadRepository`.
- `WalletResponse` fields: `totalBalance`, `reservedBalance`, `availableBalance` (decimal JSON).
- **404** `WALLET_NOT_FOUND` if row missing — UI should treat as error (spec: defect, not empty zero).

### Test fake sketch

```csharp
// ThrowOnWalletReadRepository : IWalletReadRepository
public Task<WalletReadModel?> GetByUserIdAsync(...) =>
    throw new InvalidOperationException("Simulated wallet read failure");
```

Register with `services.AddScoped<IWalletReadRepository, ThrowOnWalletReadRepository>()` in test factory configure callback.

### Frontend state matrix

| walletQuery | portfolioQuery | Cash card | Holdings card |
|-------------|----------------|-----------|---------------|
| pending | any | Skeleton | Own skeleton |
| error | any | Error alert, no amounts | Optional error |
| success | pending | Show balances | Skeleton |
| success | error | Show balances | Error or empty table |

### Error copy (spec)

> Could not load account data. Try refreshing or sign in again.

Keep existing string in `VirtualCashCard` (shared constant optional for chip error label).

### Top bar chip (Q1 = Yes)

```tsx
// WalletTopBarChip — sketch
const { data, isPending, isError } = useWalletQuery()
// pending → <Skeleton className="h-5 w-24" />
// error → <span className="text-muted-foreground text-sm">Unavailable</span>
// success → <span className="tabular-nums">{formatUsd(normalizeWallet(data).availableBalance)}</span>
```

## Verification matrix (plan-level)

| Spec AC | Verified by |
|---------|-------------|
| Authenticated trading view shows available USD | Task 3–4 manual |
| Top bar available cash (Q1) | Task 4 manual + ADR-004 |
| New user $100k / $0 reserved | Task 1 integration |
| Two decimal USD display | Task 3–4 `formatUsd` + manual |
| Non-blocking cash skeleton | Task 2–4 manual |
| GET wallet 500 → error, no fabricated balance | Task 1 integration + Task 2–4 UI |
| EC-01 new user all-available $100k | Task 1 |
| EC-05 API 500 | Task 1 + Task 2–4 |

## Rollback / recovery

- **Code:** revert branch `feature/virtual-cash-story-1`
- **DB:** N/A
- **Redis:** N/A

## Deferred work (Plan B)

- Story 2 plan: `docs/plans/<ts>-virtual-cash-story-2.md` — total/reserved breakdown UX (#35)
- Story 3: cross-user wallet cache (#36)
- Story 4: refetch-on-focus / read-your-writes (#37)
- SignalR wallet push when orders ship
- PRD §8.1 full top bar (AAPL symbol, last price, daily change)

## GitHub Links

| Local ref | Issue # | Type | Title | URL |
|-----------|---------|------|-------|-----|
| spec Story 1 | 34 | Story | US-03 / Story 1: See how much cash I can trade with | https://github.com/tranvuongduy2003/trading-simulator/issues/34 |
| spec epic | 33 | Epic | Spec: Virtual cash balance display (US-03) | https://github.com/tranvuongduy2003/trading-simulator/issues/33 |
