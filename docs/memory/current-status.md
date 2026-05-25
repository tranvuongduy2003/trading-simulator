# Current Status

Last updated: 2026-05-25
Owner: @tranvuongduy2003

## Active Focus

- Story 4 implementation complete on `feature/user-registration-story-4` — open PR and run manual register UI checklist

## Latest Completed

- spec: user login US-02 (`docs/specs/20260525-103709-user-login.md`)
- impl: user registration story 4 (Tasks 1–4) — transient UX, integration tests, OpenAPI **500**, regression (18 Testcontainers tests green)
- Plan: `docs/plans/20260525-095103-user-registration-story-4.md` (COMPLETE)
- impl: user registration story 3 (Tasks 1–4) — validation, API contract, client onBlur, OpenAPI polish

## In Progress

- None.

## Next Up

- Manual UI sign-off (register form checklist in Story 4 plan Task 4)
- Open PR: `feature/user-registration-story-4` → `main`
- Merge `feature/user-registration-story-3` if not already on `main`

## Blockers

- None.

## Known Issues

- MSB3277: EF Core Relational 10.0.7 vs 10.0.8 — warning only.
- Api integration tests require Docker (Testcontainers).
- Rare concurrent register race may return **500** instead of **422** (EC-03 MVP: client submit guard + DB indexes; no Postgres constraint mapper).
- `RegisterUserSessionTests` uses `WebApplicationFactory` without Testcontainers (needs local Postgres on :5432).

## Session Start Checklist

- [x] Story 4 plan tasks 1–4 complete on `feature/user-registration-story-4`
- [ ] PR opened and CI green
- [ ] Manual register UI checklist (Story 4 Task 4)

## Story 4 — EC-03 note

Double-submit: **client** `submittingRef` + `isPending` guard; DB `ux_users_username` / `ux_users_email` backstop. Extreme concurrent race may still return **500** (acceptable for MVP; see `known-issues.md`).
