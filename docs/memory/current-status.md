# Current Status

Last updated: 2026-05-25
Owner: @tranvuongduy2003

## Active Focus

- None — Story 3 implementation complete on `feature/user-registration-story-3`; ready for PR.

## Latest Completed

- impl: user registration story 3 (Tasks 1–4) — validation, API contract, client onBlur, OpenAPI polish
- Plan: `docs/plans/20260525-120000-user-registration-story-3.md`

## In Progress

- None.

## Next Up

- Open PR from `feature/user-registration-story-3` → `main`
- Story 4 plan when Story 3 merges ([#8](https://github.com/tranvuongduy2003/trading-simulator/issues/8))

## Blockers

- None.

## Known Issues

- MSB3277: EF Core Relational 10.0.7 vs 10.0.8 — warning only.
- Api integration tests require Docker (Testcontainers).
- Duplicate register concurrent race on unique index not mapped in UoW (Story 2).
- `RegisterUserSessionTests` uses `WebApplicationFactory` without Testcontainers (needs local Postgres on :5432).

## Session Start Checklist

- [x] Story 3 plan tasks 1–4 complete on `feature/user-registration-story-3`
- [ ] PR opened and CI green
- [ ] Optional browser spot-check of register form (onBlur, confirm-password, server errors)
