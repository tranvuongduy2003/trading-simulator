---
artifact_type: plan
artifact_version: 1
id: plan-20260525-230000-virtual-cash-story-3
title: Virtual Cash Balance — Story 3 (Session-private wallet)
slug: virtual-cash-story-3
filename_template: 20260525-230000-virtual-cash-story-3.md
created_at: 2026-05-25T23:00:00+07:00
updated_at: 2026-05-25T23:45:00+07:00
status: completed
owner: engineering
tags: [plan, implementation, trading-simulator, wallet, session, privacy, us-03, story-3]
related_spec: docs/specs/20260525-201500-virtual-cash-balance.md
related_plans: [docs/plans/20260525-203000-virtual-cash-story-1.md, docs/plans/20260525-220000-virtual-cash-story-2.md]
prd_refs: [PRD §5.1 US-03, PRD §7.3, PRD §8.1]
tech_refs: [Tech §5.2.1, Tech §6, Tech §8.1, Tech §15.1, Tech §17.3]
db_refs: [DB §4.2 wallets, DB §5 invariants]
github:
  repo: tranvuongduy2003/trading-simulator
  epic_issue: 33
  story_issue_ids: [36]
  last_synced_at: 2026-05-25T23:00:00+07:00
search_index:
  keywords: [wallet privacy, session scope, BR-07, UNAUTHORIZED, 401, TanStack Query cache, EC-03, EC-04, cross-user, GetMyWallet, ICurrentUserAccessor, ProtectedRoute, useWalletQuery, removeQueries]
  bounded_contexts: [Trading]
  task_count: 5
---

# Implementation Plan: Virtual Cash Balance — Story 3

| Field | Value |
|-------|--------|
| Spec | `docs/specs/20260525-201500-virtual-cash-balance.md` (§2 Story 3) |
| GitHub story | [#36 — Only see my own balance when signed in](https://github.com/tranvuongduy2003/trading-simulator/issues/36) |
| Epic | [#33 — Virtual cash balance display (US-03)](https://github.com/tranvuongduy2003/trading-simulator/issues/33) |
| Depends on | Story 1–2 shipped (`useWalletQuery`, `VirtualCashCard`, `GetMyWalletQuery`, US-02 session) |
| Status | COMPLETED (automation); manual UI checklist pending operator |
| Tasks | 5 |
| Branch | `feature/virtual-cash-story-3` |
| Aspire impact | No |
| Schema impact | No |
| Test levels | API integration (new privacy scenarios) · Manual UI |
| ADRs required | None |
| GitHub | Synced 2026-05-25 — see §GitHub Links |

## Executive summary

Story 3 (US-03) ensures **wallet reads are private to the signed-in session**: `GET /api/wallet` returns only the caller’s row (BR-07, Tech §15.1), unauthenticated calls get **401** `UNAUTHORIZED`, and the React client must **not flash another user’s cached balances** when user B logs in after user A on the same browser (EC-04). The backend path is largely correct (`ICurrentUserAccessor`, `RequireAuthorization`, no `userId` query param). This plan **proves** ownership with new integration tests and **hardens** TanStack Query cache behavior on login/logout/session expiry so trading UI and the top-bar cash chip never render stale wallet numbers.

## Goals and non-goals

**Goals**

- G1: Authenticated `GET /api/wallet` returns `userId` matching the session user (happy path AC).
- G2: User B login after user A on the same cookie jar never surfaces A’s balances in UI (EC-04).
- G3: Unauthenticated `GET /api/wallet` → **401** with stable `code` `UNAUTHORIZED`; protected routes redirect to login.
- G4: Session expired mid-view → **401**, auth cleared, **no numeric balances** on trading view or top bar (EC-03).
- G5: Logout and login transitions **purge** wallet (and portfolio) query cache — not only `invalidateQueries`.

**Non-goals**

- NG1: Story 4 — refetch-on-focus / read-your-writes polish (#37).
- NG2: Story 1–2 display formatting, reserved breakdown emphasis.
- NG3: New `userId` query parameter, admin wallet APIs, or SignalR wallet push.
- NG4: Re-implementing US-02 login/logout/session store (reuse; only wallet-specific cache rules).
- NG5: Frontend unit/E2E framework; schema/matching/Redis changes.

## Traceability matrix

| Spec story | Plan task(s) | Test evidence |
|------------|--------------|---------------|
| Story 3 — userId matches session | Task 1 | `GetMyWallet_AfterLogin_UserIdMatchesSession`; existing register test |
| Story 3 — user B after user A (no stale cache) | Task 2, 3, 4 | `GetMyWallet_AfterSecondUserLogin_ReturnsSecondUserWalletOnly`; manual EC-04 |
| Story 3 — unauthenticated 401 | Task 1 | `GetMyWallet_WithoutSession_Returns401_UNAUTHORIZED` (extend existing test) |
| Story 3 — session expired, no balances | Task 4, 5 | `SessionPersistence_ExpiredSession_Returns401` (reuse); manual EC-03 |
| BR-07 session-scoped wallet | Task 1 | Handler uses `ICurrentUserAccessor` only — no code change expected |
| EC-03 session expired mid-view | Task 4, 5 | Manual + `UnauthorizedListener` / wallet `isError` |
| EC-04 same browser user switch | Task 2, 3 | Integration + manual |

## Architecture impact

```text
┌─────────────────────────────────────────────────────────────┐
│ Browser (single cookie jar)                                  │
│  Login/Logout ──► removeQueries(['wallet']) / clear on 401   │
│  useWalletQuery(['wallet', userId]) ──► GET /api/wallet        │
│  VirtualCashCard / WalletTopBarChip ──► hide $ if !match/401  │
└────────────────────────────┬────────────────────────────────┘
                             │ session cookie
                             ▼
┌─────────────────────────────────────────────────────────────┐
│ Api: WalletEndpoint.RequireAuthorization()                   │
│  → GetMyWalletQueryHandler(ICurrentUserAccessor → userId)    │
│  → WalletReadRepository.GetByUserIdAsync(sessionUserId)      │
└────────────────────────────┬────────────────────────────────┘
                             ▼
                    PostgreSQL wallets (pk user_id)
```

| Layer | Change summary |
|-------|----------------|
| Domain | **REUSE** — no change |
| Application | **REUSE** — `GetMyWalletQueryHandler` + `ICurrentUserAccessor` |
| Infrastructure | **REUSE** — `WalletReadRepository` filters by `userId` |
| Api | **MODIFY** — `WalletEndpoint` drops `RequireAuthorization()` so handler returns RFC 7807 `UNAUTHORIZED` (ASP.NET auth returned empty 401) |
| MatchingEngine | None |
| web/ | **MODIFY** auth transitions, `use-wallet-query`, trading/top-bar wallet display guards |
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
| 1 | Is `invalidateQueries` on login enough for EC-04? | Code review | **No** — `invalidateQueries` keeps prior `data` during refetch; use `removeQueries` before session switch and user-scoped `queryKey` | ✅ Answered |
| 2 | Should wallet query key include `userId`? | TanStack best practice | **Yes** — `['wallet', userId]` prevents cache collision; `removeQueries({ queryKey: ['wallet'] })` still clears all wallet queries | ✅ Answered |
| 3 | Defense-in-depth UI guard if cache bug regresses? | Spec risk | **Yes** — do not render amounts unless `wallet.userId === authStore.userId` | ✅ Answered |

## Risks and mitigations

| Risk | Likelihood | Impact | Mitigation | Owner task |
|------|------------|--------|------------|------------|
| Stale wallet flash on login (EC-04) | M | H | `removeQueries` + user-scoped key + UI userId guard | Task 2, 3, 4 |
| `useWalletQuery` runs while logged out, repopulates cache | L | M | `enabled: status === 'authenticated'` | Task 3 |
| Integration test order-dependent shared DB | L | L | Unique emails per run; single client cookie sequence | Task 1 |
| SignalR interceptors still use `['wallet']` prefix | L | L | Partial key match still works; document in Task 2 | Task 2 |

## Prerequisites

- [x] US-01 / US-02 shipped (session, `GET /api/wallet`)
- [x] Virtual cash Story 1–2 automation on branches (or merged to `main`)
- [ ] Local Aspire for manual EC-03 / EC-04 checklist
- [x] Branch `feature/virtual-cash-story-3` from latest `main` (or Story 2 branch if not merged)

## File structure (planned)

```text
tests/TradingSimulator.Api.IntegrationTests/
  Users/GetMyWalletTests.cs                         MODIFY (privacy tests)

web/src/features/auth/
  clear-user-queries.ts                             CREATE (shared purge helper)
  login-form.tsx                                    MODIFY
  register-form.tsx                                 MODIFY
  use-logout.ts                                     REUSE (already removeQueries)

web/src/features/trading/
  hooks/use-wallet-query.ts                         MODIFY
  pages/trading-page.tsx                            MODIFY
  components/virtual-cash-card.tsx                  MODIFY (optional guard props)
  components/wallet-top-bar-chip.tsx                MODIFY

web/src/lib/signalr/interceptors.ts                 MODIFY (query key comment/prefix only if needed)

src/Application/Users/Queries/GetMyWalletQueryHandler.cs   REUSE
src/Api/Endpoints/WalletEndpoint.cs                          REUSE
```

## Authorization, session, and domain notes

- **Session model:** HttpOnly cookie; `SessionAuthenticationHandler` sets `ICurrentUserAccessor.UserId`.
- **Route protection:** `ProtectedRoute` + `useSession` bootstrap; unauthenticated → `/login`.
- **API:** `GET /api/wallet` has **no** `userId` route/query parameter — ownership from session only (spec §7).
- **BR-07:** Handler must never accept a client-supplied user id; repository lookup uses accessor only.
- **401 global path:** `unauthorizedResponseInterceptor` dispatches `api:unauthorized`; `UnauthorizedListener` clears `queryClient` and navigates to login with `session-expired` when previously authenticated.
- **Logout path:** `useLogout` already `removeQueries` for `wallet`, `portfolio`, `auth/session` — keep aligned with login/register.

## Progress tracker

### Task 1: Prove API wallet ownership and 401 contract

| Attribute | Value |
|-----------|--------|
| Spec story | Story 3 |
| Depends on | None |
| Estimated complexity | S |
| Parent story issue | #36 |

#### Objective

Automated tests document that wallet reads are session-bound: correct `userId` on success, **401** `UNAUTHORIZED` without session, and after user B logs in on the same client, wallet data belongs to B only (not A’s seeded balances).

#### Implementation notes

- Extend `GetMyWalletTests` — do not duplicate full register flows unnecessarily.
- Add helper to assert wallet problem `code` for 401 (mirror `LoginUserTestHelpers` pattern).
- **Two-user test:** register/login user A → seed distinct `total_balance` (e.g. 50_000) → logout → register/login user B → `GET /api/wallet` → assert `userId` is B and balances are B’s default 100k (not A’s 50k).
- **Login-without-logout variant (optional second fact):** login A → wallet → login B with same client → wallet `userId` is B (session replacement).
- Reuse `SeedWalletBalancesAsync` from existing `GetMyWalletTests`.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `tests/.../Users/GetMyWalletTests.cs` | New facts + 401 code assert |
| REUSE | `tests/.../Users/GetMyWalletTests.cs` | `SeedWalletBalancesAsync` |
| REUSE | `src/Application/Users/Queries/GetMyWalletQueryHandler.cs` | Ownership via accessor |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `GetMyWallet_WithoutSession_Returns401_UNAUTHORIZED` | `GetMyWalletTests.cs` |
| Integration | `GetMyWallet_AfterSecondUserLogin_ReturnsSecondUserWalletOnly` | `GetMyWalletTests.cs` |
| Integration | `GetMyWallet_AfterLogin_UserIdMatchesSession` (or fold into above) | `GetMyWalletTests.cs` |

#### Acceptance criteria

- [x] No session → **401** and problem `code` `UNAUTHORIZED`
- [x] User A seeded wallet ≠ user B wallet after B’s session established on same `HttpClient`
- [x] `userId` in 200 body matches registered/logged-in user
- [x] All new tests pass in Users Testcontainers collection

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | Tech §15.1, DB §4.2 |
| Async matching | N/A |
| PostgreSQL authoritative | Read path only |
| Redis projection | N/A |
| RFC 7807 errors | `UNAUTHORIZED` stable code |
| SignalR | N/A |
| Aspire | None |
| ADR needed? | No |

#### Risk

None — tests only; may expose existing backend bug if two-user test fails.

---

### Task 2: Purge wallet cache on auth user change

| Attribute | Value |
|-----------|--------|
| Spec story | Story 3 |
| Depends on | Task 1 (tests define expected behavior) |
| Estimated complexity | S |
| Parent story issue | #36 |

#### Objective

Login and register flows **remove** prior user’s wallet/portfolio cache before establishing the new session, matching logout behavior and spec §6 stale-UI handling.

#### Implementation notes

- **CREATE** `clearUserScopedQueries(queryClient)` — `removeQueries` for `['wallet']`, `['portfolio']`, and optionally `['auth', 'session']` when replacing user.
- **login-form.tsx:** call purge **before** `setSession` / wallet fetch; then `fetchQuery` session wallet; then navigate.
- **register-form.tsx:** purge before `setQueryData` / navigation (register always new user).
- Keep `useLogout.finalizeLogout` using same helper for one code path.
- Do **not** rely on `invalidateQueries` alone for user switch (EC-04).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `web/src/features/auth/clear-user-queries.ts` | Shared cache purge |
| MODIFY | `web/src/features/auth/login-form.tsx` | Purge on successful login |
| MODIFY | `web/src/features/auth/register-form.tsx` | Purge on successful register |
| MODIFY | `web/src/features/auth/use-logout.ts` | Call shared helper |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Login B after A — no flash of A’s amount | Aspire |
| Integration | Task 1 two-user test | `GetMyWalletTests.cs` |

#### Acceptance criteria

- [x] Successful login calls `removeQueries` for wallet (not only invalidate)
- [x] Successful register clears prior wallet cache before navigation
- [x] Logout still clears wallet cache (regression)

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | Spec §6 EC-04 |
| RFC 7807 | N/A |
| Aspire | None |
| ADR needed? | No |

#### Risk

Low — auth-only files; verify public routes still bootstrap session without infinite loader (`providers.tsx` guard).

---

### Task 3: User-scoped wallet query hook

| Attribute | Value |
|-----------|--------|
| Spec story | Story 3 |
| Depends on | Task 2 |
| Estimated complexity | S |
| Parent story issue | #36 |

#### Objective

`useWalletQuery` only runs for the authenticated user and keys cache by `userId`, so TanStack Query cannot serve user A’s wallet entry to user B’s session.

#### Implementation notes

- Read `userId` from `useAuthStore`.
- `queryKey: ['wallet', userId]` with `enabled: status === 'authenticated' && !!userId`.
- `staleTime` unchanged (30s).
- Update any `invalidateQueries`/`removeQueries` call sites to use prefix `['wallet']` (already partial-match friendly).
- **Do not** store wallet in Zustand (frontend rule).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `web/src/features/trading/hooks/use-wallet-query.ts` | Scoped key + enabled |
| MODIFY | `web/src/lib/signalr/interceptors.ts` | Confirm invalidation prefix still valid |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | React Query devtools / network: new key after login B | Aspire |

#### Acceptance criteria

- [x] Wallet query disabled when `authStatus !== 'authenticated'`
- [x] Query key includes current `userId`
- [x] No duplicate fetch storms on navigation

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | Tech §11 TanStack Query |
| ADR needed? | No |

#### Risk

None — isolated hook change.

---

### Task 4: Hide balances on 401 and userId mismatch

| Attribute | Value |
|-----------|--------|
| Spec story | Story 3 |
| Depends on | Task 3 |
| Estimated complexity | M |
| Parent story issue | #36 |

#### Objective

Trading view and top-bar chip **never show numeric balances** when wallet fetch fails with **401**, session is invalid, or API `userId` does not match the auth store (defense in depth).

#### Implementation notes

- Add small helper `canDisplayWallet(wallet, sessionUserId)` in `wallet-display.ts` or next to hook.
- **trading-page.tsx:** pass `wallet` to card only when `walletQuery.isSuccess && canDisplayWallet(...)`.
- **wallet-top-bar-chip.tsx:** same guard; keep skeleton on pending; “Unavailable” on error (no amounts).
- On **401**, `ApiError` + global listener should clear session; local UI must not render last successful `data` if status flipped to error — with Task 3 key purge this is unlikely; guard anyway.
- **401 on wallet while authenticated:** treat as error state (no tabular nums), rely on redirect from `UnauthorizedListener`.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `web/src/features/trading/wallet-display.ts` | `canDisplayWallet` helper |
| MODIFY | `web/src/features/trading/pages/trading-page.tsx` | Guarded render |
| MODIFY | `web/src/features/trading/components/wallet-top-bar-chip.tsx` | Guarded render |
| MODIFY | `web/src/features/trading/components/virtual-cash-card.tsx` | Optional: accept `showBalances` flag |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Expire session (revoke in DB or wait) — trading shows no $ | Aspire |
| Manual | Force 401 on wallet — redirect + no balances | Aspire |
| Integration | Reuse `SessionPersistence_ExpiredSession_Returns401` | existing |

#### Acceptance criteria

- [x] EC-03: expired session → no numeric balances on trading page or top bar (code guard; manual Aspire pending)
- [x] Wallet `userId` ≠ `authStore.userId` → card/chip show error/empty, not amounts
- [x] Protected route still redirects unauthenticated users to login

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | Spec §4a 401 flow |
| RFC 7807 | 401 handling |
| ADR needed? | No |

#### Risk

Low — UI-only; avoid blocking loading skeleton indefinitely.

---

### Task 5: Polish — regression smoke and manual privacy checklist

| Attribute | Value |
|-----------|--------|
| Spec story | Story 3 · Polish |
| Depends on | Tasks 1–4 |
| Estimated complexity | S |
| Parent story issue | #36 |

#### Objective

Confirm no regressions to Stories 1–2 and complete manual verification of EC-03 and EC-04 on Aspire.

#### Implementation notes

- Run Users Testcontainers suite (wallet + session tests).
- Run `yarn --cwd web build`.
- Run `yarn --cwd web api:verify` if OpenAPI touched.
- Execute manual checklist below; comment results on GitHub #36 when done.

#### Manual UI checklist

1. **EC-04:** Register/login user A → note available cash → logout → register/login user B → trading view shows B’s balance (typically $100,000.00), never A’s amount.
2. **EC-04 variant:** Login A → login B without logout (if supported) → same as above.
3. **EC-03:** While on trading view, revoke session (logout other tab or DB revoke) → refresh or trigger wallet fetch → redirect login, session-expired message, **no** dollar amounts visible during transition.
4. **Unauthenticated:** Open `/trading` logged out → redirect to login; direct `GET /api/wallet` returns 401 (browser devtools).
5. **Regression:** Story 1 skeleton/error paths; Story 2 reserved breakdown still correct.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `docs/memory/current-status.md` | After `/build` completes |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | Full Users collection green | CI/local Docker |
| Manual | Checklist above | Aspire |

#### Acceptance criteria

- [x] All Task 1–4 acceptance criteria still pass
- [ ] Manual checklist steps 1–5 signed off (operator on Aspire)
- [x] No regression in `GetMyWalletTests` (including Story 2 reserved cases)

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| Aspire | `aspire run` for manual |
| ADR needed? | No |

#### Risk

None — verification only.

## Reference files

| File | Why open it |
|------|-------------|
| `src/Application/Users/Queries/GetMyWalletQueryHandler.cs` | BR-07 ownership implementation |
| `src/Api/Endpoints/WalletEndpoint.cs` | `RequireAuthorization`, route shape |
| `tests/.../Users/GetMyWalletTests.cs` | Extend privacy tests |
| `tests/.../Users/LogoutUserTests.cs` | Logout → wallet 401 pattern |
| `tests/.../Users/SessionPersistenceTests.cs` | Expired session 401 |
| `web/src/features/auth/login-form.tsx` | Cache purge insertion point |
| `web/src/features/auth/use-logout.ts` | Existing `removeQueries` |
| `web/src/app/providers.tsx` | `UnauthorizedListener` |
| `web/src/hooks/use-session.ts` | Session bootstrap + 401 clear |
| `docs/plans/20260525-203000-virtual-cash-story-1.md` | Prior wallet UI patterns |

## Implementation details (for /build)

### Backend

- `GetMyWalletQueryHandler` returns `Error.Unauthorized("UNAUTHORIZED", ...)` when `currentUserAccessor.UserId` is null.
- `WalletEndpoint` maps via `ToHttpResult()` → 401 problem+json.
- **Deviation:** removed `.RequireAuthorization()` on `GET /api/wallet` so unauthenticated calls reach the handler (middleware-only 401 had no problem body / `UNAUTHORIZED` code). Session still required via `ICurrentUserAccessor`.
- If Task 1 fails on two-user test, inspect `LoginUserCommandHandler` session replacement and `WalletReadRepository` filter — not Story 3 UI.

### Frontend cache contract

| Event | Query action |
|-------|----------------|
| Logout success | `removeQueries(['wallet'])`, `['portfolio']`, `['auth','session']` |
| Login success | **removeQueries** before new session, then fetch |
| Register success | **removeQueries** before seeding session cache |
| 401 while authenticated | `queryClient.clear()` (existing) |
| Wallet query key | `['wallet', userId]` |

### UI display rule

```text
show numeric balances IFF
  walletQuery.isSuccess
  AND wallet.userId === authStore.userId
  AND NOT (walletQuery.error is 401)
```

### Error codes

- Wallet unauthenticated: **401**, `code`: `UNAUTHORIZED` (spec §4b).

## Verification matrix (plan-level)

| Spec AC | Verified by |
|---------|-------------|
| Authenticated wallet `userId` matches account | Task 1 integration + register test |
| User B after A — no A cached balances | Task 1 API + Task 2–4 UI + manual EC-04 |
| Unauthenticated → 401 + login redirect | Task 1 + `ProtectedRoute` + manual |
| Session expired → 401, clear auth, no balances | Task 4 + `SessionPersistenceTests` + manual EC-03 |
| BR-07 | Task 1 + handler review |
| No `userId` query param | Code review `WalletEndpoint` |

## Rollback / recovery

- **Code:** revert branch commits.
- **DB:** N/A.
- **Redis:** N/A.

## Deferred work (Plan B)

- Story 4: refetch-on-focus and read-your-writes (#37).
- E2E Playwright for EC-04 flash detection.
- Structured audit log when wallet `userId` mismatch detected client-side (debug-only).

## GitHub Links

| Local ref | Issue # | Type | Title | URL |
|-----------|---------|------|-------|-----|
| spec.Story 3 | 36 | Story | US-03 / Story 3: Only see my own balance when signed in | https://github.com/tranvuongduy2003/trading-simulator/issues/36 |
| epic | 33 | Epic | Virtual cash balance display (US-03) | https://github.com/tranvuongduy2003/trading-simulator/issues/33 |
