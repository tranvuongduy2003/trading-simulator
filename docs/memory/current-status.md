# Current Status

Last updated: 2026-05-25
Owner: @tranvuongduy2003

## Active Focus

- User login story 3 automation complete on `feature/user-login-story-3` — open PR → `main` (GitHub #24); manual UI checklist pending operator
- PR for `feature/user-login-story-2` → `main` (GitHub #23 — Story 2 automation complete)

## Latest Completed

- impl: user login story 3 **Tasks 1–5** (automation) — persistence tests, `SessionStore` hardening, session-expired + cookies UX; **38** Users Testcontainers tests, `api:verify`, `yarn build` green
- impl: user login story 2 **Tasks 1–4** — invalid-credentials HTTP tests, UI code audit, `api:verify`, 33 Users Testcontainers tests green
- plan: user login story 3 (`docs/plans/20260525-170000-user-login-story-3.md`) — build tasks done; manual sign-off pending
- impl: user login story 1 **Task 6** — OpenAPI export/verify, 28 Users integration tests green (register + login)

## In Progress

- Operator: Story 3 manual UI checklist (plan §Manual UI checklist) on Aspire
- Manual Story 2 UI checklist on Aspire (plan §Task 3) — operator before merge
- PR for `feature/user-login-story-1` → `main` (not opened yet)

## Next Up

- Open PR `feature/user-login-story-3` → `main` (closes #24 when merged)
- Operator: Story 3 manual UI checklist (6 steps in plan)
- Open PR `feature/user-login-story-2` → `main` (closes #23 when merged)
- Epic #21 Stories 4–5 (logout, login validation hardening)

## Blockers

- None for code/CI.

## Known Issues

- MSB3277: EF Core Relational 10.0.7 vs 10.0.8 — warning only.
- Api integration tests require Docker (Testcontainers).
- `RegisterUserSessionTests` uses `WebApplicationFactory` without Testcontainers (needs local Postgres on :5432) — excluded from CI-style regression run.
- Rare concurrent register race may return **500** instead of **422** (EC-03 MVP).

## Session Start Checklist

- [x] Login story 3 Tasks 1–5 automation on `feature/user-login-story-3`
- [ ] Manual Story 3 UI checklist (plan §Manual UI checklist)
- [ ] PR `feature/user-login-story-3` opened and CI green
- [x] Login story 2 Tasks 1–4 on `feature/user-login-story-2`
- [ ] Manual Story 2 UI checklist (Task 3)
- [ ] PR opened and CI green
- [x] Login story 1 Tasks 1–6 (automation) on `feature/user-login-story-1`
- [ ] Manual login UI checklist (Story 1 Task 6)
