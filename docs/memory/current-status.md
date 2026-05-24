# Current Status

Last updated: 2026-05-24
Owner: @tranvuongduy2003

## Active Focus

- User registration Story 2 — duplicate identity (`USERNAME_TAKEN` / `EMAIL_TAKEN`).

## Latest Completed

- Story 2 plan complete (`feature/user-registration-story-2`): duplicate `USERNAME_TAKEN` / `EMAIL_TAKEN`, integration tests, OpenAPI 422 (no 409 on register)

## In Progress

- None.

## Next Up

- Open PR for Story 2 (`create-pr`) or merge after review
- Story 3 plan (validation matrix) when ready

## Blockers

- None.

## Known Issues

- MSB3277: EF Core Relational 10.0.7 vs 10.0.8 — warning only.
- Api integration tests require Docker (Testcontainers).
- Duplicate register (exists-check) returns `USERNAME_TAKEN` / `EMAIL_TAKEN`; concurrent race on unique index is not mapped in UoW.

## Session Start Checklist

- [ ] Read `docs/plans/20260524-120000-user-registration-story-2.md`
- [ ] Branch `feature/user-registration-story-2` from main or Story 1 branch
- [ ] Story 1 integration tests green before starting Task 1
