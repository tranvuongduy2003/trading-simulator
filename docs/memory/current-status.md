# Current Status

Last updated: 2026-05-25
Owner: @tranvuongduy2003

## Active Focus

- PR for `feature/user-login-story-2` → `main` (GitHub #23 — Story 2 automation complete)

## Latest Completed

- impl: user login story 2 **Tasks 1–4** — invalid-credentials HTTP tests, UI code audit, `api:verify`, 33 Users Testcontainers tests green
- plan: user login story 2 (`docs/plans/20260525-160000-user-login-story-2.md`) — automation complete
- impl: user login story 1 **Task 6** — OpenAPI export/verify, 28 Users integration tests green (register + login)
- impl: user login story 1 **Tasks 1–5** — API, handler, HTTP tests, login UI
- plan: user login story 1 (`docs/plans/20260525-150000-user-login-story-1.md`) — all build tasks done

## In Progress

- Manual Story 2 UI checklist on Aspire (plan §Task 3) — operator before merge
- PR for `feature/user-login-story-1` → `main` (not opened yet)

## Next Up

- Open PR `feature/user-login-story-2` → `main` (closes #23 when merged)
- Operator: Story 2 manual UI checklist (plan §Task 3)
- Epic #21 Stories 3–5 (session expiry, logout, login validation hardening)

## Blockers

- None for code/CI.

## Known Issues

- MSB3277: EF Core Relational 10.0.7 vs 10.0.8 — warning only.
- Api integration tests require Docker (Testcontainers).
- `RegisterUserSessionTests` uses `WebApplicationFactory` without Testcontainers (needs local Postgres on :5432) — excluded from CI-style regression run.
- Rare concurrent register race may return **500** instead of **422** (EC-03 MVP).

## Session Start Checklist

- [x] Login story 2 Tasks 1–4 on `feature/user-login-story-2`
- [ ] Manual Story 2 UI checklist (Task 3)
- [ ] PR opened and CI green
- [x] Login story 1 Tasks 1–6 (automation) on `feature/user-login-story-1`
- [ ] Manual login UI checklist (Story 1 Task 6)
