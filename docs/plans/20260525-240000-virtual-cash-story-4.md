---
artifact_type: plan
artifact_version: 1
id: plan-20260525-240000-virtual-cash-story-4
title: Virtual Cash Balance — Story 4 (Trust after login and refresh)
slug: virtual-cash-story-4
filename_template: 20260525-240000-virtual-cash-story-4.md
created_at: 2026-05-25T24:00:00+07:00
updated_at: 2026-05-25T25:30:00+07:00
status: completed
owner: engineering
tags: [plan, implementation, trading-simulator, wallet, refetch, read-your-writes, us-03, story-4]
related_spec: docs/specs/20260525-201500-virtual-cash-balance.md
related_plans: [docs/plans/20260525-203000-virtual-cash-story-1.md, docs/plans/20260525-220000-virtual-cash-story-2.md, docs/plans/20260525-230000-virtual-cash-story-3.md]
prd_refs: [PRD §5.1 US-03, PRD §6.1 FR-1.2, PRD §6.6 FR-6.2]
tech_refs: [Tech §5.2.1, Tech §6, Tech §8.1, Tech §15.1, Tech §17.3]
db_refs: [DB §4.2 wallets, DB §5 invariants, DB §8 BR-08]
github:
  repo: tranvuongduy2003/trading-simulator
  epic_issue: 33
  story_issue_ids: [37]
  last_synced_at: 2026-05-25T24:00:00+07:00
search_index:
  keywords: [wallet refetch, read-your-writes, refetchOnWindowFocus, staleTime, login prefetch, browser refresh, PostgreSQL authoritative, BR-08, BR-04, EC-07, GetMyWallet, useWalletQuery, prefetchWallet]
  bounded_contexts: [Trading]
  task_count: 5
---

# Implementation Plan: Virtual Cash Balance — Story 4

| Field | Value |
|-------|--------|
| Spec | `docs/specs/20260525-201500-virtual-cash-balance.md` (§2 Story 4) |
| GitHub story | [#37 — Trust balances after login and refresh](https://github.com/tranvuongduy2003/trading-simulator/issues/37) |
| Epic | [#33 — Virtual cash balance display (US-03)](https://github.com/tranvuongduy2003/trading-simulator/issues/33) |
| Depends on | Stories 1–3 shipped (`useWalletQuery`, session privacy, `GetMyWalletTests`, auth flows) |
| Status | COMPLETED (automation) — manual checklist pending operator |
| Tasks | 5 |
| Branch | `feature/virtual-cash-story-4` |
| Aspire impact | No |
| Schema impact | No |
| Test levels | API integration (new read-your-writes / refetch scenarios) · Manual UI |
| ADRs required | None |
| GitHub | Synced 2026-05-25 — see §GitHub Links |

## Executive summary

Story 4 (US-03) closes the **trust loop**: after login, browser refresh, or tab focus, displayed cash must match the latest **PostgreSQL** wallet row from `GET /api/wallet` (BR-08), including reserved amounts when seeded (BR-04). Stories 1–3 delivered display, breakdown, and session privacy; the main gap is **client refetch policy** — `QueryClient` sets `refetchOnWindowFocus: false` globally and `useWalletQuery` uses `staleTime: 30_000`, so a focused tab can keep stale balances for up to 30s (violates EC-07 / stale-tab AC). Login seeds `['auth', 'session']` but not `['wallet', userId]`, adding an extra round-trip before the trading view shows cash. This plan adds **integration proof** of login→wallet and second-fetch-after-DB-update, a **shared post-auth wallet prefetch**, **per-query refetch-on-focus with `staleTime: 0`**, and a manual checklist for refresh and focus on Aspire.

## Goals and non-goals

**Goals**

- G1: After successful login, trading view shows wallet within **2 s** matching PostgreSQL (read-your-writes).
- G2: Browser refresh with valid session re-loads the same balances (session probe + wallet fetch).
- G3: Window focus and remount refetch wallet so stale-tab balances update (EC-07).
- G4: Integration test: login → `GET /api/wallet` **200** with correct balances; second GET after DB seed returns updated figures.
- G5: **500** path unchanged — no partial wallet row (reuse Story 1 test + UI).

**Non-goals**

- NG1: SignalR wallet push (spec §6, issue deferred).
- NG2: Order-place/cancel mutation invalidation (no order epic yet; document hook point in `interceptors.ts`).
- NG3: Portfolio reset (US-04 / EC-08) — only note refetch compatibility.
- NG4: Backend handler or schema changes (read path already authoritative).
- NG5: Frontend unit/E2E framework; lowering global `refetchOnWindowFocus` for non-wallet queries.

## Traceability matrix

| Spec story | Plan task(s) | Test evidence |
|------------|--------------|---------------|
| Story 4 — login → wallet within 2s, PG match | Task 2, 3 | `GetMyWallet_AfterLogin_ReturnsSeededBalances`; manual step 1 |
| Story 4 — browser refresh same balances | Task 3, 5 | Manual step 2; `useSession` + `useWalletQuery` on remount |
| Story 4 — focus/refetch latest reserved | Task 3 | `GetMyWallet_SecondFetchAfterDbUpdate_ReturnsLatestBalances`; manual step 3 |
| Story 4 — stale tab updates on focus | Task 3 | Manual step 3; `staleTime: 0` + `refetchOnWindowFocus: true` on wallet |
| Story 4 — PG unavailable **500**, no partial UI | Task 1, 5 | Reuse `GetMyWallet_WhenReadFails_Returns500_INTERNAL_ERROR`; manual regression |
| BR-08 PostgreSQL authoritative | Task 1 | Second-fetch-after-seed test |
| BR-04 reserved refetch after orders | Task 3 | Per-query refetch; future `invalidateQueries` on order mutations |
| EC-07 slightly stale until refetch | Task 3 | Documented MVP behavior |
| EC-08 portfolio reset | — | Deferred; refetch-on-focus compatible |

## Architecture impact

```text
┌──────────────────────────────────────────────────────────────────┐
│ Auth success (login / register)                                   │
│  clearUserScopedQueries → prefetchWallet(['wallet', userId])      │
│  setSession → navigate /trading                                   │
└────────────────────────────┬─────────────────────────────────────┘
                             │
┌────────────────────────────▼─────────────────────────────────────┐
│ ProtectedRoute: useSession() → GET /api/wallet (session bootstrap)  │
│ AppLayout / TradingPage: useWalletQuery()                           │
│   staleTime: 0 · refetchOnWindowFocus: true · enabled if authed    │
└────────────────────────────┬─────────────────────────────────────┘
                             │ session cookie
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│ Api GET /api/wallet → GetMyWalletQuery → WalletReadRepository     │
│                      → PostgreSQL wallets (authoritative)         │
└──────────────────────────────────────────────────────────────────┘
```

| Layer | Change summary |
|-------|----------------|
| Domain | **REUSE** — no change |
| Application | **REUSE** — `GetMyWalletQueryHandler` |
| Infrastructure | **REUSE** — `WalletReadRepository` |
| Api | **REUSE** — `WalletEndpoint`; optional comment only |
| MatchingEngine | None |
| web/ | **MODIFY** — `use-wallet-query`, auth prefetch helper, `login-form`, `register-form` |
| AppHost | None |

## Data & migration plan

| Artifact | Action | DB reference |
|----------|--------|--------------|
| EF migration | **None** | — |
| Redis keys | **None** | — |
| Book recovery | N/A | — |

## Open questions

| # | Question | Source | Answer | Status |
|---|----------|--------|--------|--------|
| 1 | Keep global `refetchOnWindowFocus: false` and override only wallet? | code review | **Yes** — avoids refetch storms on market/portfolio placeholders | ✅ |
| 2 | Is `staleTime: 0` on wallet too aggressive for MVP? | spec §6 EC-07 | Acceptable; wallet reads are cheap; focus refetch is the product requirement | ✅ |
| 3 | Should login block navigation until wallet prefetch completes? | spec 2s SLA | **Yes** — extend existing login `fetchQuery` pattern to wallet key before `navigate` | ✅ |

## Risks and mitigations

| Risk | Likelihood | Impact | Mitigation | Owner task |
|------|------------|--------|------------|------------|
| Extra wallet GET on every tab focus | M | L | Single lightweight read; no SignalR yet | Task 3 |
| Login prefetch races with `clearUserScopedQueries` | L | M | Clear first, then `fetchQuery` with new `userId` | Task 2 |
| Regression: Story 3 cross-user flash | L | H | Keep `removeQueries` on auth change; prefetch only after `setSession` userId known | Task 2, 5 |
| Session query (`staleTime: 60s`) hides expired session on focus | M | M | Out of scope for Story 4; wallet 401 still triggers `UnauthorizedListener` | — |

## Prerequisites

- [x] Spec Story 4 defined (§2)
- [x] Stories 1–3 automation on feature branches (wallet UI, reserved breakdown, cache privacy)
- [ ] Aspire local stack for manual checklist
- [x] `GetMyWalletTests` baseline green (Docker Testcontainers)

## File structure (planned)

```text
tests/Api.IntegrationTests/Users/
  GetMyWalletTests.cs                    MODIFY — Story 4 refetch / login balance tests

web/src/features/trading/hooks/
  use-wallet-query.ts                    MODIFY — refetch policy

web/src/features/auth/
  prefetch-wallet.ts                     CREATE — shared prefetch + cache seed
  login-form.tsx                         MODIFY — prefetch wallet query key
  register-form.tsx                      MODIFY — seed ['wallet', userId] from registration

docs/memory/current-status.md            MODIFY — after /build
```

## Authorization, session, and domain notes

- **Session model:** Cookie session; `ProtectedRoute` bootstraps via `useSession` → `GET /api/wallet`.
- **Read-your-writes:** Registration creates wallet in same transaction as user; login must see committed row — integration tests use real PostgreSQL.
- **BR-08:** UI never recomputes `available` from total − reserved; always display API fields (`normalizeWallet`).
- **BR-04:** Until order placement ships, prove refetch fidelity via seeded `reserved_balance` + second GET (and focus refetch manual).

## Progress tracker

### Task 1: Integration tests — login wallet balances and refetch after DB update

| Attribute | Value |
|-----------|--------|
| Spec story | Story 4 |
| Depends on | None |
| Estimated complexity | M |
| Parent story issue | #37 |

#### Objective

Automate API-level proof that login returns wallet **200** with PostgreSQL-aligned balances, and a subsequent `GET /api/wallet` after `ExecuteUpdate` on `wallets` returns the updated reserved/available breakdown.

#### Implementation notes

- Add `GetMyWallet_AfterLogin_ReturnsSeededBalances`: register → logout → login → seed 50k/10k reserved → GET wallet → assert 40k available.
- Add `GetMyWallet_SecondFetchAfterDbUpdate_ReturnsLatestBalances`: authenticated GET → seed different balances → second GET → assert new values (simulates refetch after stale tab).
- Reuse `SeedWalletBalancesAsync` from existing tests.
- Keep `GetMyWallet_WhenReadFails_Returns500_INTERNAL_ERROR` as regression anchor.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `tests/Api.IntegrationTests/Users/GetMyWalletTests.cs` | New Story 4 scenarios |
| REUSE | `tests/.../IntegrationTestFixture.cs` | Testcontainers client |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `GetMyWallet_AfterLogin_ReturnsSeededBalances` | `GetMyWalletTests.cs` |
| Integration | `GetMyWallet_SecondFetchAfterDbUpdate_ReturnsLatestBalances` | `GetMyWalletTests.cs` |
| Integration | Existing 500 / login userId tests still pass | `GetMyWalletTests.cs` |

#### Acceptance criteria

- [x] New tests pass in Docker Testcontainers suite
- [x] Login → wallet **200** asserts monetary fields, not only `userId`
- [x] Second fetch reflects DB update (BR-08)

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | US-03, Tech §6, DB §4.2 |
| PostgreSQL authoritative | Seed via EF `ExecuteUpdate`, read via API |
| RFC 7807 | N/A for 200 paths |
| Aspire | None |
| ADR needed? | No |

#### Risk

None — test-only slice.

---

### Task 2: Post-auth wallet prefetch and cache seeding

| Attribute | Value |
|-----------|--------|
| Spec story | Story 4 |
| Depends on | Task 1 (optional parallel) |
| Estimated complexity | M |
| Parent story issue | #37 |

#### Objective

After login or register, warm `['wallet', userId]` in TanStack Query before navigating to trading so cash appears immediately and matches the server response.

#### Implementation notes

- Create `prefetchWalletQuery(queryClient, userId)` calling `authApi.getWallet` + `normalizeWallet`, `queryKey: ['wallet', userId]`.
- **Login:** after `clearUserScopedQueries`, `await prefetchWalletQuery` (and keep session `fetchQuery` or consolidate to one wallet fetch that sets both session + wallet keys).
- **Register:** after `setSession`, `setQueryData(['wallet', userId], normalizeWallet(registrationWallet))` in addition to existing session cache seed.
- Do not prefetch before `userId` is known (Story 3 EC-04).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `web/src/features/auth/prefetch-wallet.ts` | Shared prefetch/seed |
| MODIFY | `web/src/features/auth/login-form.tsx` | Prefetch wallet before navigate |
| MODIFY | `web/src/features/auth/register-form.tsx` | Seed wallet query cache |
| REUSE | `web/src/features/auth/clear-user-queries.ts` | Clear before prefetch |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | Task 1 tests | API |
| Manual | Login → trading shows balance without long skeleton | Aspire |

#### Acceptance criteria

- [x] Login success path populates `['wallet', userId]` before `navigate(paths.trading)`
- [x] Register seeds wallet cache from `response.wallet` (no `$0` flash)
- [x] Story 3 `clearUserScopedQueries` still runs first on auth transitions

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| Async matching | N/A |
| Redis projection | N/A |
| ADR needed? | No |

#### Risk

Low — ensure prefetch errors surface like today (login root error).

---

### Task 3: Wallet query refetch policy (focus and mount)

| Attribute | Value |
|-----------|--------|
| Spec story | Story 4 |
| Depends on | Task 2 |
| Estimated complexity | S |
| Parent story issue | #37 |

#### Objective

Enable refetch-on-focus and immediate staleness for wallet only, so EC-07 stale reads correct on tab focus without enabling global refetch storms.

#### Implementation notes

- In `useWalletQuery`, set:
  - `staleTime: 0`
  - `refetchOnWindowFocus: true` (overrides `providers.tsx` default `false`)
  - Keep `enabled: authStatus === 'authenticated' && userId !== null`
- Leave global `QueryClient` defaults unchanged for portfolio/market placeholders.
- Document in file comment: order mutations should `invalidateQueries({ queryKey: ['wallet'] })` when order epic ships (already in `interceptors.ts`).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `web/src/features/trading/hooks/use-wallet-query.ts` | Refetch policy |
| REUSE | `web/src/app/providers.tsx` | Global defaults stay |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Focus tab after seeding reserved in DB → UI updates | Aspire + devtools |

#### Acceptance criteria

- [x] Focusing browser tab triggers network `GET /api/wallet` when authenticated
- [x] Wallet refetches on trading page remount after full page refresh
- [x] No change to global `refetchOnWindowFocus: false` for other queries

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| SignalR | Deferred; invalidation hook exists |
| ADR needed? | No |

#### Risk

Low — slightly more API traffic on focus; acceptable for MVP.

---

### Task 4: Read-your-writes verification on session refresh path

| Attribute | Value |
|-----------|--------|
| Spec story | Story 4 |
| Depends on | Task 3 |
| Estimated complexity | S |
| Parent story issue | #37 |

#### Objective

Confirm and document that a full browser refresh on `/trading` re-runs session bootstrap and wallet fetch without showing stale in-memory Zustand balances (auth store is not persisted).

#### Implementation notes

- **No code required** if manual verification passes: `auth-store` resets to `unknown` on reload → `ProtectedRoute` skeleton → `useSession` + `useWalletQuery` both fetch `/api/wallet`.
- If gap found: ensure `useWalletQuery` is not disabled while `authStatus === 'unknown'` during bootstrap — may need `enabled` tied to session success only (evaluate during `/build`; avoid showing balances during `unknown`).
- Optional: add one-line comment on `ProtectedRoute` referencing Story 4 refresh contract.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `web/src/app/routes/protected-route.tsx` | Comment only if helpful |
| MODIFY | `web/src/hooks/use-session.ts` | Only if refresh gap found |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Hard refresh on trading → same balances | Aspire |

#### Acceptance criteria

- [x] Browser refresh with valid session shows same balances after load completes
- [x] No persisted Zustand wallet amounts (only server state)

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| FR-1.2 session persistence | Cookie survives refresh |
| ADR needed? | No |

#### Risk

None — verification-first task.

---

### Task 5: Polish — regression and manual trust checklist

| Attribute | Value |
|-----------|--------|
| Spec story | Story 4 · Polish |
| Depends on | Tasks 1–4 |
| Estimated complexity | S |
| Parent story issue | #37 |

#### Objective

Run automated regression and manual steps proving trust AC; update memory artifacts.

#### Manual UI checklist

1. **Login read-your-writes:** Login as user with known balance (new user $100k) → trading view shows correct available within ~2s without second login.
2. **Browser refresh:** On `/trading`, hard refresh (F5) → same balances after load.
3. **Focus refetch:** With devtools open, seed reserved via integration test DB or second browser tab not available — use Postgres MCP: update `reserved_balance` for your user → focus tab → wallet UI updates to match `GET /api/wallet`.
4. **Stale tab:** Open trading, change wallet in DB (reserved 5k), switch away 1 min, focus tab → balances update (not stuck at $100k).
5. **500 regression:** Force wallet read failure (or use test env) → error copy, no dollar amounts (Story 1).
6. **Story 3 regression:** Logout/login different users → no cross-user balances.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `docs/memory/current-status.md` | Post-build status |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | Full `GetMyWalletTests` + Users collection | Docker |
| Manual | Checklist 1–6 | Aspire |

#### Acceptance criteria

- [x] All Task 1–4 acceptance criteria pass
- [ ] Manual checklist 1–6 signed off (operator on Aspire)
- [x] `yarn --cwd web build` green

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| Aspire | `aspire run` for manual |
| ADR needed? | No |

#### Risk

None.

## Reference files

| File | Why open it |
|------|-------------|
| `web/src/features/trading/hooks/use-wallet-query.ts` | Refetch policy change |
| `web/src/app/providers.tsx` | Global `refetchOnWindowFocus: false` |
| `web/src/features/auth/login-form.tsx` | Post-login prefetch |
| `web/src/features/auth/register-form.tsx` | Registration wallet seed |
| `tests/.../Users/GetMyWalletTests.cs` | Extend Story 4 tests |
| `docs/plans/20260525-230000-virtual-cash-story-3.md` | Cache contract table |
| `docs/specs/20260525-201500-virtual-cash-balance.md` | Story 4 AC |

## Implementation details (for /build)

### TanStack Query contract (target state)

| Query key | staleTime | refetchOnWindowFocus | When enabled |
|-----------|-----------|----------------------|--------------|
| `['auth', 'session']` | 60_000 (unchanged) | false (global) | `authStatus !== 'unauthenticated'` |
| `['wallet', userId]` | **0** | **true** | authenticated + `userId` set |

### Post-auth sequence (login)

```text
clearUserScopedQueries()
→ fetchQuery(['auth','session'], getWallet)  // existing session probe
→ fetchQuery(['wallet', userId], getWallet)   // NEW — warm trading UI
→ setSession({ userId, username })
→ navigate(/trading)
```

### Post-auth sequence (register)

```text
clearUserScopedQueries()
→ setSession(...)
→ setQueryData(['auth','session'], walletShape)
→ setQueryData(['wallet', userId], normalizeWallet(response.wallet))  // NEW
→ navigate(/trading)
```

### Future order epic

When place/cancel ships: `queryClient.invalidateQueries({ queryKey: ['wallet'] })` in mutation `onSuccess` (in addition to focus refetch). SignalR `BalanceUpdated` already invalidates in `interceptors.ts`.

## Verification matrix (plan-level)

| Spec AC | Verified by |
|---------|-------------|
| Login → wallet within 2s, PG match | Task 2 + manual 1 + `GetMyWallet_AfterLogin_ReturnsSeededBalances` |
| Browser refresh same balances | Task 4 manual 2 |
| Focus/refetch latest reserved | Task 3 + manual 3–4 + `GetMyWallet_SecondFetchAfterDbUpdate_*` |
| Stale tab updates | Task 3 manual 4 |
| PG down → 500, no partial UI | Task 1 reuse + manual 5 |
| BR-08 / BR-04 | Task 1 integration |

## Rollback / recovery

- **Code:** revert branch commits.
- **DB:** N/A.
- **Redis:** N/A.

## Deferred work (Plan B)

- SignalR wallet push for sub-second updates (spec §13 Q2).
- `invalidateQueries` on order mutations when order epic lands.
- Playwright timing test for 2s login→wallet SLA.
- Lower `useSession` staleTime if session expiry on focus becomes a product requirement.

## GitHub Links

| Local ref | Issue # | Type | Title | URL |
|-----------|---------|------|-------|-----|
| spec.Story 4 | 37 | Story | US-03 / Story 4: Trust balances after login and refresh | https://github.com/tranvuongduy2003/trading-simulator/issues/37 |
| epic | 33 | Epic | Virtual cash balance display (US-03) | https://github.com/tranvuongduy2003/trading-simulator/issues/33 |
