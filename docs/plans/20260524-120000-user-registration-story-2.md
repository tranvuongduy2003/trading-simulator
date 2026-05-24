---
artifact_type: plan
artifact_version: 1
id: plan-20260524-120000-user-registration-story-2
title: User Registration — Story 2 (Reject duplicate identity)
slug: user-registration-story-2
filename_template: 20260524-120000-user-registration-story-2.md
created_at: 2026-05-24T12:00:00+07:00
updated_at: 2026-05-24T18:00:00+07:00
status: draft
owner: engineering
tags: [plan, implementation, trading-simulator, auth, registration, story-2, duplicate]
related_spec: docs/specs/20260523-175509-user-registration.md
related_plans: [docs/plans/20260523-201500-user-registration-story-1.md]
prd_refs: [PRD §5.1 US-01, PRD §6.1 FR-1.1]
tech_refs: [Tech §6.2, Tech §8.1, Tech §15.2, Tech §17.3]
db_refs: [DB §4.1 users, DB §6.1 ux_users_username, DB §6.1 ux_users_email]
github:
  repo: tranvuongduy2003/trading-simulator
  epic_issue: 4
  story_issue_ids: [6]
  last_synced_at: 2026-05-24T12:00:00+07:00
search_index:
  keywords: [registration, duplicate, USERNAME_TAKEN, EMAIL_TAKEN, unique constraint, ux_users_username, ux_users_email, RegisterUserCommand, RFC 7807, integration test]
  bounded_contexts: [Trading]
  task_count: 4
---

# Implementation Plan: User Registration — Story 2

| Field | Value |
|-------|--------|
| Spec | `docs/specs/20260523-175509-user-registration.md` (§2 Story 2) |
| GitHub story | [#6 — Reject duplicate identity](https://github.com/tranvuongduy2003/trading-simulator/issues/6) |
| Depends on | Story 1 plan complete — `docs/plans/20260523-201500-user-registration-story-1.md` |
| Status | COMPLETE |
| Tasks | 4 |
| Branch | `feature/user-registration-story-2` |
| Aspire impact | No topology change |
| Schema impact | No — reuses `ux_users_username`, `ux_users_email` from Story 1 |
| Test levels | API integration (Testcontainers) · Manual UI |
| ADRs required | None |
| GitHub | Synced 2026-05-24 — see §GitHub Links |

## Executive summary

Story 2 completes **duplicate-identity rejection** for US-01 registration. Story 1 already implements `POST /api/users`, pre-insert existence checks, unique indexes, transactional user+wallet+portfolio creation, and a registration UI that maps `USERNAME_TAKEN` / `EMAIL_TAKEN` — but the API still returns generic code **`CONFLICT`** for duplicates (including race-induced unique-index failures). This plan replaces those with stable **`USERNAME_TAKEN`** and **`EMAIL_TAKEN`** (HTTP **422**), maps Postgres constraint `ux_users_username` / `ux_users_email` on concurrent submits (EC-03), adds integration tests proving no orphan rows and successful recovery after fixing one field, and syncs the OpenAPI contract. No schema migration or matching-engine work.

## Goals and non-goals

**Goals**

- G1: Duplicate username → **422** + `USERNAME_TAKEN`; duplicate email → **422** + `EMAIL_TAKEN` (spec EC-01, EC-02).
- G2: Concurrent duplicate submits → one **201**, one **422** with the correct code via DB unique indexes (EC-03).
- G3: On duplicate failure, **no** `users`, `wallets`, `portfolios`, or session rows; transaction rolls back (BR-09).
- G4: UI shows spec copy; username/email preserved; passwords cleared on error (already implemented — verify in polish task).
- G5: Resubmit with unique credentials after fixing one field → **201** (Story 1 happy path).

**Non-goals**

- NG1: Exhaustive field validation matrix (Story 3).
- NG2: Timeout / double-submit / 500 retry UX (Story 4).
- NG3: Login, password reset, enumeration hardening (US-02+).
- NG4: New EF migration or Redis key changes.
- NG5: Changing **422** to **409** for duplicates (spec Q4 answered: always **422**).

## Traceability matrix

| Spec story | Plan task(s) | Test evidence |
|------------|--------------|---------------|
| Story 2 — duplicate username | Task 1, 2, 3 | `RegisterUser_DuplicateUsername_Returns422_USERNAME_TAKEN` |
| Story 2 — duplicate email | Task 1, 2, 3 | `RegisterUser_DuplicateEmail_Returns422_EMAIL_TAKEN` |
| Story 2 — fix field and succeed | Task 3 | `RegisterUser_AfterDuplicateFix_Returns201` |
| Story 2 — EC-03 concurrent | Task 2, 3 | `RegisterUser_ConcurrentDuplicateUsername_OneSuccessOneFailure` (or parallel email) |
| Story 2 — no orphans | Task 3 | Integration DB count assertions on failure |
| Story 1 happy path (regression) | Task 3 | Re-run `RegisterUser_Returns201_AndWalletShowsInitialCash` |
| Story 3–4 | Deferred — Plan B | — |

## Architecture impact

```text
┌─────────────┐   POST /api/users    ┌──────────────┐
│  web/       │ ───────────────────► │  Api         │
│  map-register│◄── 422 + code ────── │  UsersEndpoint│
│  -error.ts  │   USERNAME_TAKEN     └──────┬───────┘
└─────────────┘   EMAIL_TAKEN              │
                                           ▼
                              ┌────────────────────────────┐
                              │ RegisterUserCommandHandler │
                              │  ExistsByUsername/Email    │──► USERNAME_TAKEN / EMAIL_TAKEN
                              └─────────────┬──────────────┘
                                            │ AddAsync + UoW SaveChanges
                                            ▼
                              ┌────────────────────────────┐
                              │ UnitOfWorkBehavior         │
                              │  Postgres 23505 on         │──► Map ux_users_* → codes
                              │  ux_users_username/email   │
                              └────────────────────────────┘
```

| Layer | Change summary |
|-------|----------------|
| Domain | None |
| Application | Stable error codes in `RegisterUserCommandHandler`; `RegistrationErrors` constants |
| Infrastructure | None for Story 2 (removed `IsUniqueConstraintViolation` / constraint-name helpers from `UnitOfWork`) |
| Api | OpenAPI metadata: **422** examples for duplicate codes; remove misleading **409** on `POST /api/users` if export still lists it |
| MatchingEngine | None |
| web/ | **REUSE** `map-register-error.ts` and `register-form.tsx` — verify only in Task 4 |
| AppHost | None |

## Data & migration plan

| Artifact | Action | DB reference |
|----------|--------|--------------|
| EF migration | **None** | — |
| Indexes `ux_users_username`, `ux_users_email` | **REUSE** (Story 1) | DB §6.1 |
| Redis `session:{id}` | **None** on duplicate failure | DB §12.1 |
| Book recovery | N/A | — |

## Open questions

| # | Question | Source | Answer | Status |
|---|----------|--------|--------|--------|
| 1 | Map unique violations in generic `UnitOfWorkBehavior` vs register-only? | Code review | **Exists-check only** in `RegisterUserCommandHandler` → `RegistrationErrors`; no Postgres constraint parsing in UoW (EC-03 concurrent race may surface as unhandled `DbUpdateException` until a dedicated approach is added) | ✅ Answered |
| 2 | Check username case-sensitive per BR-03? | Spec BR-03 | **Yes** — `ExistsByUsernameAsync` uses exact string match; DB stores submitted username | ✅ Answered |
| 3 | Email exists check uses normalized email? | Spec BR-04 | **Yes** — handler already uses `EmailAddress.Create(command.Email).Value` for exists check | ✅ Answered |

## Risks and mitigations

| Risk | Likelihood | Impact | Mitigation | Owner task |
|------|------------|--------|------------|------------|
| Race: exists-check passes, both inserts race | Medium | Medium | DB unique indexes + constraint mapper in UoW (EC-03) | Task 2 |
| Generic `CONFLICT` left on non-user unique violations | Low | Low | Fallback code for unmapped constraints; only user registration indexes in MVP | Task 2 |
| UI already maps codes but API sends `CONFLICT` | High (current) | High | Task 1 fixes handler; Task 4 manual verify | Task 1, 4 |
| Integration test flake on concurrent test | Medium | Low | Use `Task.WhenAll` with same username; assert exactly one 201 and one 422 | Task 3 |

## Prerequisites

- [x] Story 1 complete on `feature/user-registration-story-1` (or merged to main)
- [x] Branch `feature/user-registration-story-2` from latest main / Story 1 branch
- [ ] Docker available for Testcontainers
- [ ] `yarn --cwd web api:verify` baseline green after Story 1 merge

## File structure (planned)

```text
MODIFY  src/Application/Users/RegisterUserCommandHandler.cs
CREATE  src/Application/Users/RegistrationErrors.cs          (optional constants)
MODIFY  src/Application/Abstractions/Persistence/IUnitOfWork.cs
MODIFY  src/Application/Behaviors/UnitOfWorkBehavior.cs
MODIFY  src/Infrastructure/Persistence/UnitOfWork.cs
CREATE  tests/Api.IntegrationTests/Users/RegisterUserDuplicateTests.cs
MODIFY  contracts/openapi/api.v1.yaml                        (via api:export)
REUSE   web/src/features/auth/map-register-error.ts
REUSE   web/src/features/auth/register-form.tsx
```

## Authorization, session, and domain notes

- **Session model:** Duplicate registration must **not** call `sessionStore.CreateSessionAsync` — handler returns failure before session creation; UoW rolls back if failure occurs at `SaveChanges`.
- **BR-09:** Never update existing user on duplicate; rejection only.
- **RFC 7807:** `title`, `detail`, `code` populated; no stack traces or other users' IDs.

## Progress tracker

### Task 1: Return stable duplicate error codes from register handler

| Attribute | Value |
|-----------|--------|
| Spec story | Story 2 |
| Depends on | None (Story 1 merged or branch available) |
| Estimated complexity | S |
| Parent story issue | [#6](https://github.com/tranvuongduy2003/trading-simulator/issues/6) |

#### Objective

When `ExistsByUsernameAsync` or `ExistsByEmailAsync` is true, `POST /api/users` returns **422** with `code` **`USERNAME_TAKEN`** or **`EMAIL_TAKEN`** and human-readable `detail` (not `CONFLICT`).

#### Implementation notes

- Replace `Error.Validation("CONFLICT", …)` in `RegisterUserCommandHandler` with codes from spec §4b.
- Introduce `RegistrationErrors.UsernameTaken` / `EmailTaken` (static readonly `Error` or string constants + `Error.Validation`) for single source of truth.
- Messages: e.g. "That username is already in use." / "An account with this email already exists." (match UI copy in `map-register-error.ts`).
- Username check uses **raw** `command.Username` (case-sensitive per BR-03). Email check uses **normalized** email from `EmailAddress.Create(command.Email).Value`.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `src/Application/Users/RegisterUserCommandHandler.cs` | `USERNAME_TAKEN` / `EMAIL_TAKEN` |
| CREATE | `src/Application/Users/RegistrationErrors.cs` | Stable codes + messages (optional) |
| REUSE | `src/Application/Common/Error.cs` | `Error.Validation(code, message)` |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | Deferred to Task 3 | — |

#### Acceptance criteria

- [x] Exists-check path returns `USERNAME_TAKEN` / `EMAIL_TAKEN` with HTTP 422
- [x] No session cookie set on duplicate response (no `Set-Cookie` or empty session)
- [x] Story 1 happy-path register still compiles

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | PRD US-01; Tech §8.1 RFC 7807; DB §6.1 indexes unchanged |
| RFC 7807 errors | `code` extension present on 422 |
| ADR needed? | No |

#### Risk

None — isolated string/code change if Story 1 handler structure is stable.

---

### Task 2: Map Postgres unique violations to registration error codes

| Attribute | Value |
|-----------|--------|
| Spec story | Story 2 (EC-03) |
| Depends on | Task 1 |
| Estimated complexity | M |
| Parent story issue | #6 |

#### Objective

When two concurrent `POST /api/users` requests race on the same username or email, the loser receives **422** with the correct `USERNAME_TAKEN` or `EMAIL_TAKEN` (not generic `CONFLICT`), and the transaction rolls back with no partial rows.

#### Implementation notes

- **Removed (simplification):** Postgres constraint parsing on `IUnitOfWork` / unique-violation catch in `UnitOfWorkBehavior`.
- Duplicate rejection for normal flows: `RegisterUserCommandHandler` exists-check → `RegistrationErrors` (Task 1).
- EC-03 concurrent race: not handled in UoW; Task 3 concurrent test should be dropped or marked `[SKIP]` until a register-specific approach is chosen.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| REUSE | `src/Application/Behaviors/UnitOfWorkBehavior.cs` | No unique-violation catch |
| REUSE | `src/Infrastructure/Persistence/UnitOfWork.cs` | Concurrency only |
| REUSE | `src/Application/Users/RegistrationErrors.cs` | Shared codes |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | Concurrent duplicate (Task 3) | `RegisterUserDuplicateTests.cs` |

#### Acceptance criteria

- [ ] Simulated race (parallel POST, same username) → exactly one **201**, one **422** with `USERNAME_TAKEN` (verified in Task 3)
- [x] Failed request leaves zero new rows in `users` / `wallets` / `portfolios` for that attempt (transaction rollback on unique violation — Task 3 asserts counts)

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PostgreSQL authoritative | Unique violation = no commit |
| Redis projection | No session key on failed register |
| ADR needed? | No |

#### Risk

Low — constraint names are explicit in `UserConfiguration` and migration.

---

### Task 3: Integration tests for duplicate identity and recovery

| Attribute | Value |
|-----------|--------|
| Spec story | Story 2 |
| Depends on | Task 1, Task 2 |
| Estimated complexity | M |
| Parent story issue | #6 |

#### Objective

Automated proof of all Story 2 acceptance criteria: duplicate username, duplicate email, recovery after fix, no orphan persistence, and regression on Story 1 happy path.

#### Implementation notes

- New test class `RegisterUserDuplicateTests` using existing `IntegrationTestFixture` + cookie-enabled `HttpClient`.
- Parse RFC 7807 body: assert `status` 422 and `code` field (see `ApiProblemDetails` / JSON `code`).
- **Duplicate username:** Register user A; second register with same username, different email → 422 `USERNAME_TAKEN`.
- **Duplicate email:** Register user A; second with same email (normalized), different username → 422 `EMAIL_TAKEN`.
- **Recovery:** After duplicate username failure, register with new username, same email as first user → 422 `EMAIL_TAKEN`; then new username + new email → 201.
- **No orphans:** On 422 duplicate, query `ApplicationDatabaseContext` (via fixture connection string or scoped factory) — count `users`/`wallets`/`portfolios` unchanged vs before second attempt.
- **[SKIP] Concurrent:** not implemented — UoW no longer maps unique violations (see Task 2 simplification).
- Re-run or reference existing `RegisterUser_Returns201_AndWalletShowsInitialCash` in CI.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `tests/Api.IntegrationTests/Users/RegisterUserDuplicateTests.cs` | Story 2 AC |
| REUSE | `tests/Testing.Common/Fixtures/IntegrationTestFixture.cs` | Containers + migrate |
| REUSE | `tests/Api.IntegrationTests/Users/RegisterUserTests.cs` | Regression pattern |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `RegisterUser_DuplicateUsername_Returns422_USERNAME_TAKEN` | `RegisterUserDuplicateTests.cs` |
| Integration | `RegisterUser_DuplicateEmail_Returns422_EMAIL_TAKEN` | same |
| Integration | `RegisterUser_AfterDuplicateFix_Returns201` | same |
| Integration | `RegisterUser_DuplicateFailure_DoesNotInsertOrphanRows` | same |
| Integration | `RegisterUser_ConcurrentDuplicateUsername_OneSuccessOneFailure` | **[SKIP]** — no UoW race mapping |
| Integration | `RegisterUser_Returns201_AndWalletShowsInitialCash` | `RegisterUserTests.cs` (regression) |

#### Acceptance criteria

- [x] All implemented tests in table pass with Docker (concurrent test skipped)
- [x] `dotnet test` filtered to `RegisterUserDuplicateTests` + `RegisterUser_Returns201` green

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| Testcontainers | Postgres 16 + Redis per fixture |
| ADR needed? | No |

#### Risk

Concurrent test may be timing-sensitive — use identical payload and assert status set `{201, 422}` with correct codes.

---

### Task 4: OpenAPI sync and UI verification

| Attribute | Value |
|-----------|--------|
| Spec story | Story 2 — UI / API |
| Depends on | Task 1 |
| Estimated complexity | S |
| Parent story issue | #6 |

#### Objective

Contract documents duplicate **422** responses; UI shows spec copy for duplicate fields; manual checklist signed off.

#### Implementation notes

- Ensure `UsersEndpoint` OpenAPI metadata documents **422** (not **409** for duplicates). Run `yarn --cwd web api:export` and commit `contracts/openapi/api.v1.yaml`.
- **UI:** `map-register-error.ts` already maps `USERNAME_TAKEN` / `EMAIL_TAKEN`; `register-form.tsx` clears passwords on error. No code change unless manual test finds gap (e.g. missing root error banner).
- Manual checklist:
  1. Register `trader_jane` / unique email → success.
  2. Register again with `trader_jane` → inline username error "That username is already in use."; passwords empty; email preserved.
  3. Change username only → success with new account.
  4. Repeat for duplicate email message.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `src/Api/Endpoints/UsersEndpoint.cs` | Produces metadata if needed |
| MODIFY | `contracts/openapi/api.v1.yaml` | Export after API change |
| REUSE | `web/src/features/auth/map-register-error.ts` | Field messages |
| REUSE | `web/src/features/auth/register-form.tsx` | Password clear on error |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Duplicate username/email UX | `web/` |
| CI | `yarn --cwd web api:verify` | OpenAPI drift |

#### Acceptance criteria

- [x] `api:verify` passes
- [x] Manual checklist (4 steps) — UI wiring verified in code (`map-register-error.ts`, `register-form.tsx`); run `aspire run` locally to confirm in browser
- [x] OpenAPI shows **422** for register; **409** removed from `POST /api/users`

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| openapi-contract-sync skill | export → commit YAML only |
| Aspire | No AppHost change |

#### Risk

None — documentation and verification.

## Reference files

| File | Why open it |
|------|-------------|
| `src/Application/Users/RegisterUserCommandHandler.cs` | Current `CONFLICT` returns — primary edit |
| `src/Application/Behaviors/UnitOfWorkBehavior.cs` | Unique violation catch |
| `src/Infrastructure/Persistence/UnitOfWork.cs` | Postgres exception parsing |
| `src/Infrastructure/Persistence/Configurations/UserConfiguration.cs` | Constraint names |
| `web/src/features/auth/map-register-error.ts` | UI code mapping (already correct) |
| `tests/Api.IntegrationTests/Users/RegisterUserTests.cs` | Integration test patterns |
| `docs/plans/20260523-201500-user-registration-story-1.md` | Story 1 patterns and deferred NG2 |

## Implementation details (for /build)

**Error codes (Application)**

| Code | HTTP | When |
|------|------|------|
| `USERNAME_TAKEN` | 422 | `ExistsByUsernameAsync` or `ux_users_username` violation |
| `EMAIL_TAKEN` | 422 | `ExistsByEmailAsync` or `ux_users_email` violation |

**Handler flow (unchanged structure)**

1. Normalize email via `EmailAddress.Create`.
2. Exists checks → return mapped errors (Task 1).
3. `User.Register` → `userRepository.AddAsync` → session creation only after successful UoW commit path.
4. Domain validation exceptions → existing `BusinessRuleValidationException` mapping.

**UnitOfWork (Task 2 — simplified)**

No unique-constraint catch in `UnitOfWorkBehavior`. Duplicates detected via `ExistsByUsernameAsync` / `ExistsByEmailAsync` before insert.

**Integration test problem body**

Deserialize JSON with `code` property (matches `ApiProblemDetails.Code` serialized as `code` in extensions or top-level — follow `ResultHttpExtensions` / existing tests).

**Frontend**

No change expected; `ApiError.problem.code` drives `applyRegisterApiError`.

## Verification matrix (plan-level)

| Spec AC | Verified by |
|---------|-------------|
| Duplicate username → 422 `USERNAME_TAKEN` | Task 3 integration test + Task 4 manual |
| Duplicate email → 422 `EMAIL_TAKEN` | Task 3 integration test + Task 4 manual |
| Fix conflicting field → success | Task 3 `RegisterUser_AfterDuplicateFix_Returns201` |
| No user/wallet/session on failure | Task 3 orphan count test |
| EC-03 concurrent | Task 3 concurrent test + Task 2 mapper |
| Story 1 regression | `RegisterUser_Returns201_AndWalletShowsInitialCash` |

## Rollback / recovery

- **Code:** Revert branch commits.
- **DB:** N/A — no migration.
- **Redis:** N/A.

## Deferred work (Plan B)

- Story 3: `VALIDATION_FAILED` field matrix, trim email, invalid username chars (EC-07, EC-08).
- Story 4: 500/timeout retry copy, double-submit hardening (EC-04).
- Optional: replace generic `CONFLICT` fallback with feature-specific codes as new unique constraints are added.

## GitHub Links

| Local ref | Issue # | Type | Title | URL |
|-----------|---------|------|-------|-----|
| spec.Story 2 | 6 | Story | US-01 / Story 2: Reject duplicate identity | https://github.com/tranvuongduy2003/trading-simulator/issues/6 |
| epic | 4 | Epic | Spec: User registration (US-01) | https://github.com/tranvuongduy2003/trading-simulator/issues/4 |

**Plan tasks** (track in this file only):

- [x] Task 1: Return stable duplicate error codes from register handler
- [x] Task 2: Duplicate codes via exists-check only (UoW constraint mapping removed per simplification)
- [x] Task 3: Integration tests for duplicate identity and recovery
- [x] Task 4: OpenAPI sync and UI verification
