# Current Status

Last updated: 2026-05-25
Owner: @tranvuongduy2003

## Active Focus

- User login story 1 — **implementation complete** on `feature/user-login-story-1`; manual UI sign-off + PR next

## Latest Completed

- impl: user login story 1 **Task 6** — OpenAPI export/verify, 28 Users integration tests green (register + login)
- impl: user login story 1 **Tasks 1–5** — API, handler, HTTP tests, login UI
- plan: user login story 1 (`docs/plans/20260525-150000-user-login-story-1.md`) — all build tasks done

## In Progress

- Manual Story 1 UI checklist (plan §Manual UI checklist) — operator on Aspire
- PR for `feature/user-login-story-1` → `main` (not opened yet)

## Next Up

- Run manual login checklist (7 steps in plan Task 6)
- Open PR: `feature/user-login-story-1` → `main`
- Epic #21 Stories 2–5 (invalid credentials UX, session expiry, logout, validation)

## Blockers

- None for code/CI.

## Known Issues

- MSB3277: EF Core Relational 10.0.7 vs 10.0.8 — warning only.
- Api integration tests require Docker (Testcontainers).
- `RegisterUserSessionTests` uses `WebApplicationFactory` without Testcontainers (needs local Postgres on :5432) — excluded from CI-style regression run.
- Rare concurrent register race may return **500** instead of **422** (EC-03 MVP).

## Session Start Checklist

- [x] Login story 1 Tasks 1–6 (automation) on `feature/user-login-story-1`
- [ ] Manual login UI checklist (Task 6)
- [ ] PR opened and CI green
