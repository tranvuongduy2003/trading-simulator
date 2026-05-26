# Current Status

Last updated: 2026-05-25

Owner: @tranvuongduy2003

## Active Focus

- Portfolio reset **Story 1** automation **complete** on `feature/portfolio-reset-story-1` — operator manual UI checklist + PR → `main` (closes #44)
- Virtual cash balance **Story 4** automation complete on `feature/virtual-cash-story-4` — open PR → `main` (closes #37); operator manual UI checklist pending (plan §Manual UI checklist)
- Virtual cash balance **Story 3** automation complete on `feature/virtual-cash-story-3` — open PR → `main` (closes #36); operator manual UI checklist pending (plan §Manual UI checklist)
- Virtual cash balance **Story 2** automation complete on `feature/virtual-cash-story-2` — open PR → `main` (closes #35); operator manual UI checklist pending (plan §Manual UI checklist)
- Virtual cash balance **Story 1** automation complete on `feature/virtual-cash-story-1` — open PR → `main` (closes #34); operator manual UI checklist pending (plan §Manual UI checklist)

## Latest Completed

- impl: portfolio reset story 1 **Tasks 1–5** (automation) — POST stub, dialog, 409 guard, eligibility UX, ADR-005, **13** wallet/reset integration tests + `api:verify` + `yarn build`; manual UI checklist pending; branch `feature/portfolio-reset-story-1`
- impl: portfolio reset story 1 **Task 4** — success toast, sessionStorage cooldown, disabled menu (`feature/portfolio-reset-story-1`)
- impl: portfolio reset story 1 **Task 3** — in-flight guard 409, error mapping, loading UX (`feature/portfolio-reset-story-1`)
- impl: portfolio reset story 1 **Task 2** — five consequence bullets, dismiss without POST (`feature/portfolio-reset-story-1`)
- impl: portfolio reset story 1 **Task 1** — POST stub, menu + dialog shell, integration tests, OpenAPI (`feature/portfolio-reset-story-1`)
- plan: portfolio reset story 1 — confirm before reset (`docs/plans/20260525-260000-portfolio-reset-story-1.md`, GitHub #44)
- spec: portfolio reset US-04 (`docs/specs/20260525-251500-portfolio-reset.md`)
- impl: virtual cash story 4 **Tasks 1–5** (automation) — login/refetch API tests, post-auth wallet prefetch, `staleTime: 0` + `refetchOnWindowFocus` on wallet query, refresh-path comment, **10** `GetMyWalletTests` + `yarn build` green
- plan: virtual cash balance story 4 — trust after login and refresh (`docs/plans/20260525-240000-virtual-cash-story-4.md`, GitHub #37)
- impl: virtual cash story 3 **Tasks 1–5** (automation) — privacy API tests, cache purge on auth change, user-scoped wallet query, display guards, **8** `GetMyWalletTests` + **59** Users Testcontainers + `yarn build` green
- plan: virtual cash balance story 3 — session-private wallet (`docs/plans/20260525-230000-virtual-cash-story-3.md`, GitHub #36)

## In Progress

- Operator: portfolio reset story 1 manual UI checklist (plan §Manual UI checklist) on Aspire
- Operator: virtual cash story 4 manual UI checklist (plan §Manual UI checklist) on Aspire
- Operator: virtual cash story 3 manual UI checklist (plan §Manual UI checklist) on Aspire
- Operator: virtual cash story 2 manual UI checklist (plan §Manual UI checklist) on Aspire
- Operator: virtual cash story 1 manual UI checklist (plan §Manual UI checklist) on Aspire

## Next Up

- Open PR `feature/portfolio-reset-story-1` → `main` (closes #44 when merged)
- Manual: portfolio reset story 1 checklist (7 steps in plan)
- Open PR `feature/virtual-cash-story-4` → `main` (closes #37 when merged)
- Manual: virtual cash story 4 checklist (6 steps in plan)
- Open PR `feature/virtual-cash-story-3` → `main` (closes #36 when merged)
- Open PR `feature/virtual-cash-story-2` → `main` (closes #35 when merged)
- Open PR `feature/virtual-cash-story-1` → `main` (closes #34 when merged)

## Blockers

- None for code/CI.

## Known Issues

- MSB3277: EF Core Relational 10.0.7 vs 10.0.8 — warning only.
- Api integration tests require Docker (Testcontainers).
- `RegisterUserSessionTests` uses `WebApplicationFactory` without Testcontainers (needs local Postgres on :5432) — excluded from CI-style regression run.
- Rare concurrent register race may return **500** instead of **422** (EC-03 MVP).

## Session Start Checklist

- [x] Virtual cash story 4 Tasks 1–5 automation on `feature/virtual-cash-story-4`
- [ ] Manual virtual cash story 4 UI checklist (plan §Manual UI checklist)
- [ ] PR `feature/virtual-cash-story-4` opened and CI green
- [x] Virtual cash story 3 Tasks 1–5 automation on `feature/virtual-cash-story-3`
- [ ] Manual virtual cash story 3 UI checklist (plan §Manual UI checklist)
- [ ] PR `feature/virtual-cash-story-3` opened and CI green
