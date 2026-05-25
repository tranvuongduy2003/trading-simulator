# Current Status

Last updated: 2026-05-25
Owner: @tranvuongduy2003

## Active Focus

- User login story 5 **automation complete** on `feature/user-login-story-5` — open PR → `main` (closes #26); manual UI checklist pending operator
- User login story 4 **automation complete** on `feature/user-login-story-4` — open PR → `main` (closes #25); manual UI checklist pending operator
- User login story 3 automation complete on `feature/user-login-story-3` — open PR → `main` (GitHub #24); manual UI checklist pending operator
- PR for `feature/user-login-story-2` → `main` (GitHub #23 — Story 2 automation complete)

## Latest Completed

- impl: user login story 5 **Tasks 1–5** — validation 422, transient tests, login UI errors, OpenAPI **422** on login, `api:verify`, **51** Users Testcontainers tests green
- impl: user login story 5 **Task 4** — login Zod email rules, `map-login-error` 422 fields, `onBlur` form
- impl: user login story 5 **Task 3** — `LoginUserTransientFailureTests` (3), `ThrowOnCreateSessionStore`; 23 login integration tests green
- impl: user login story 5 **Task 2** — `LoginUserValidationTests` (5 tests), `AssertValidationFailedAsync` on login helpers
- impl: user login story 5 **Task 1** — `LoginUserCommandValidator`, `LoginValidationMessages`, `Password.ForCredentialVerification`, handler split
- plan: user login story 5 (`docs/plans/20260525-190000-user-login-story-5.md`) — validation 422, transient 500, double-submit
- impl: user login story 4 **Tasks 1–5** — logout API/UI, **5** `LogoutUserTests`, OpenAPI + `api:verify`, `UserLoggedOut` log; **43** Users Testcontainers tests
- impl: user login story 4 **Task 4** — EC-07 guards in `useSession` + `ProtectedRoute`; `yarn build` green
- impl: user login story 4 **Task 3** — `useLogout`, header user menu in `AppLayout`; `yarn lint` + `yarn build` green
- impl: user login story 4 **Task 2** — logout edge-case tests (401 unauthenticated, double logout, Redis key removed); **5** `LogoutUserTests` green
- impl: user login story 4 **Task 1** — `POST /api/auth/logout`, `RevokeSessionAsync`, `LogoutUserTests` (2 tests green)
- plan: user login story 4 (`docs/plans/20260525-180000-user-login-story-4.md`) — logout API, revoke session, user menu
- impl: user login story 3 **Tasks 1–5** (automation) — persistence tests, `SessionStore` hardening, session-expired + cookies UX; **38** Users Testcontainers tests, `api:verify`, `yarn build` green
- impl: user login story 2 **Tasks 1–4** — invalid-credentials HTTP tests, UI code audit, `api:verify`, 33 Users Testcontainers tests green
- plan: user login story 3 (`docs/plans/20260525-170000-user-login-story-3.md`) — build tasks done; manual sign-off pending
- impl: user login story 1 **Task 6** — OpenAPI export/verify, 28 Users integration tests green (register + login)

## In Progress

- Operator: Story 3 manual UI checklist (plan §Manual UI checklist) on Aspire
- Manual Story 2 UI checklist on Aspire (plan §Task 3) — operator before merge
- PR for `feature/user-login-story-1` → `main` (not opened yet)

## Next Up

- Open PR `feature/user-login-story-4` → `main` (closes #25 when merged)
- Manual: Story 4 checklist (plan §Manual UI checklist + EC-07 back/two-tab on Aspire)
- Open PR `feature/user-login-story-3` → `main` (closes #24 when merged)
- Operator: Story 3 manual UI checklist (6 steps in plan)
- Open PR `feature/user-login-story-2` → `main` (closes #23 when merged)
- Open PR `feature/user-login-story-5` → `main` (closes #26 when merged)
- Manual: Story 5 UI checklist (plan §Manual UI checklist) on Aspire

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
