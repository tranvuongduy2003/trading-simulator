# Current Status

Last updated: 2026-05-25
Owner: @tranvuongduy2003

## Active Focus

- Virtual cash balance **Story 1** automation complete on `feature/virtual-cash-story-1` — open PR → `main` (closes #34); operator manual UI checklist pending (plan §Manual UI checklist)
- User login story 5 **automation complete** on `feature/user-login-story-5` — open PR → `main` (closes #26); manual UI checklist pending operator
- User login story 4 **automation complete** on `feature/user-login-story-4` — open PR → `main` (closes #25); manual UI checklist pending operator
- User login story 3 automation complete on `feature/user-login-story-3` — open PR → `main` (GitHub #24); manual UI checklist pending operator
- PR for `feature/user-login-story-2` → `main` (GitHub #23 — Story 2 automation complete)

## Latest Completed

- impl: virtual cash story 1 **Tasks 1–6** (automation) — wallet API tests, trading UI, top-bar chip, OpenAPI **500**; **54** Users Testcontainers + `api:verify` + `yarn build` green
- impl: virtual cash story 1 **Task 5** — `WalletEndpoint` `ProducesProblem(500)`, `api:export` + `api:verify` green
- impl: virtual cash story 1 **Task 4** — `WalletTopBarChip` in `AppLayout`; ADR-004 in `decisions.md`
- impl: virtual cash story 1 **Task 3** — `useWalletQuery`, `VirtualCashCard`, `wallet-display.ts`
- impl: virtual cash story 1 **Task 2** — per-query wallet/portfolio loading and error UI on `trading-page.tsx`
- impl: virtual cash story 1 **Task 1** — `GetMyWalletTests` (3), `ThrowOnWalletReadRepository`
- plan: virtual cash balance story 1 (`docs/plans/20260525-203000-virtual-cash-story-1.md`)
- spec: virtual cash balance US-03 (`docs/specs/20260525-201500-virtual-cash-balance.md`)
- impl: user login story 5 **Tasks 1–5** — validation 422, transient tests, login UI errors, OpenAPI **422** on login, `api:verify`, **51** Users Testcontainers tests green
- impl: user login story 4 **Tasks 1–5** — logout API/UI, **5** `LogoutUserTests`, OpenAPI + `api:verify`; **43** Users Testcontainers tests
- impl: user login story 3 **Tasks 1–5** (automation) — **38** Users Testcontainers tests, `api:verify`, `yarn build` green

## In Progress

- Operator: virtual cash story 1 manual UI checklist (plan §Manual UI checklist) on Aspire
- Operator: Story 3 manual UI checklist (login plan §Manual UI checklist) on Aspire
- Manual Story 2 UI checklist on Aspire (login plan §Task 3) — operator before merge
- PR for `feature/user-login-story-1` → `main` (not opened yet)

## Next Up

- Open PR `feature/virtual-cash-story-1` → `main` (closes #34 when merged)
- Manual: virtual cash story 1 checklist (5 steps in plan)
- Open PR `feature/user-login-story-4` → `main` (closes #25 when merged)
- Open PR `feature/user-login-story-3` → `main` (closes #24 when merged)
- Open PR `feature/user-login-story-2` → `main` (closes #23 when merged)
- Open PR `feature/user-login-story-5` → `main` (closes #26 when merged)

## Blockers

- None for code/CI.

## Known Issues

- MSB3277: EF Core Relational 10.0.7 vs 10.0.8 — warning only.
- Api integration tests require Docker (Testcontainers).
- `RegisterUserSessionTests` uses `WebApplicationFactory` without Testcontainers (needs local Postgres on :5432) — excluded from CI-style regression run.
- Rare concurrent register race may return **500** instead of **422** (EC-03 MVP).

## Session Start Checklist

- [x] Virtual cash story 1 Tasks 1–6 automation on `feature/virtual-cash-story-1`
- [ ] Manual virtual cash story 1 UI checklist (plan §Manual UI checklist)
- [ ] PR `feature/virtual-cash-story-1` opened and CI green
- [x] Login story 3 Tasks 1–5 automation on `feature/user-login-story-3`
- [ ] Manual Story 3 UI checklist (plan §Manual UI checklist)
- [ ] PR `feature/user-login-story-3` opened and CI green
