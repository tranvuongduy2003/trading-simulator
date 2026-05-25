---
artifact_type: plan
artifact_version: 1
id: plan-20260525-095103-user-registration-story-4
title: User Registration — Story 4 (Recover from transient failures)
slug: user-registration-story-4
filename_template: 20260525-095103-user-registration-story-4.md
created_at: 2026-05-25T09:51:03+07:00
updated_at: 2026-05-25T16:00:00+07:00
status: complete
owner: engineering
tags: [plan, implementation, trading-simulator, auth, registration, story-4, transient, retry, double-submit]
related_spec: docs/specs/20260523-175509-user-registration.md
related_plans: [docs/plans/20260523-201500-user-registration-story-1.md, docs/plans/20260524-120000-user-registration-story-2.md, docs/plans/20260525-120000-user-registration-story-3.md]
prd_refs: [PRD §5.1 US-01, PRD §6.1 FR-1.1, PRD §7.4]
tech_refs: [Tech §6.2, Tech §8.1, Tech §15.2, Tech §15.3, Tech §17.3]
db_refs: [DB §4.1 users, DB §6.1 ux_users_username, DB §6.1 ux_users_email, DB §4.9 user_sessions, DB §12.1 session cache, DB §12.2]
github:
  repo: tranvuongduy2003/trading-simulator
  epic_issue: 4
  story_issue_ids: [8]
  last_synced_at: 2026-05-25T10:30:00+07:00
search_index:
  keywords: [registration, transient, retry, INTERNAL_ERROR, double-submit, EC-03, EC-04, EC-09, EC-10, exists-check, RegisterUserCommand, submit guard, Redis session]
  bounded_contexts: [Trading]
  task_count: 4
---

# Implementation Plan: User Registration — Story 4

| Field | Value |
|-------|--------|
| Spec | `docs/specs/20260523-175509-user-registration.md` (§2 Story 4) |
| GitHub story | [#8 — Recover from transient failures](https://github.com/tranvuongduy2003/trading-simulator/issues/8) |
| Depends on | Stories 1–3 complete — plans through `docs/plans/20260525-120000-user-registration-story-3.md` |
| Status | COMPLETE |
| Tasks | 4 |
| Branch | `feature/user-registration-story-4` |
| Aspire impact | No topology change |
| Schema impact | No |
| Test levels | API integration (Testcontainers) · Manual UI |
| ADRs required | None |
| GitHub | Synced 2026-05-25 — see §GitHub Links |

## Executive summary

Story 4 closes the **ambiguous-failure** gap for US-01 registration: retry after timeout, PostgreSQL outages, Redis cache failures after commit, and accidental double-submit. Stories 1–3 already provide atomic registration, `ExistsByUsername` / `ExistsByEmail` → `USERNAME_TAKEN` / `EMAIL_TAKEN`, validation, and password clearing on error. This plan **does not** add a Postgres unique-violation mapper or `UnitOfWorkBehavior` constraint parsing — EC-03 is handled by a **client submit guard** (prevent a second in-flight request) plus existing handler exists-checks and DB unique indexes as the correctness backstop (a rare concurrent race may still return **500**; that is acceptable for MVP). Work is limited to transient-error UX (**500** / network), integration tests (retry-after-success, simulated **500**, Redis EC-10, wallet invariant under parallel submit), OpenAPI **500** documentation, and regression polish.

## Goals and non-goals

**Goals**

- G1: Double-submit prevented in UI; at most one funded account per identity (EC-03 via client guard + DB indexes; BR-09).
- G2: Retry after successful register with same username/email → **422** `USERNAME_TAKEN` / `EMAIL_TAKEN` via exists-check (EC-04, BR-09) — **no backend mapper**.
- G3: Infrastructure failure on register → **500** `INTERNAL_ERROR` with generic client detail; no password in logs (EC-09).
- G4: Redis session cache write failure after PG commit → still **201**, cookie set, `GET /api/wallet` works via PG session (EC-10).
- G5: Registration UI shows retry message on **500** / network error; submit re-enables; username/email preserved; passwords cleared (spec §4a).
- G6: Integration tests prove wallet invariants and failure paths without constraint-name mapping code.

**Non-goals**

- NG1: `Idempotency-Key` header (spec §4b).
- NG2: Rate limiting / CAPTCHA.
- NG3: Login redirect polish beyond existing “Log in” link (US-02).
- NG4: **Postgres unique-violation mapper**, `IRegisterUserUniqueViolationMapper`, or register-scoped `UnitOfWorkBehavior` catch (explicitly out — simplify flow).
- NG5: New EF migration or matching-engine work.

## Traceability matrix

| Spec story | Plan task(s) | Test evidence |
|------------|--------------|---------------|
| Story 4 — single intentional submit | Task 4 (regression) | `RegisterUser_Returns201_AndWalletShowsInitialCash` |
| Story 4 — double-click / concurrent (EC-03) | Task 1, 2 | Client submit guard (manual) · `RegisterUser_ParallelSameUsername_AtMostOneWallet` |
| Story 4 — retry after success (EC-04) | Task 2 | `RegisterUser_RetrySameCredentials_Returns422_NotSecondWallet` |
| Story 4 — PostgreSQL down (EC-09) | Task 2 | `RegisterUser_WhenPersistenceFails_Returns500_INTERNAL_ERROR` |
| Story 4 — Redis fail after commit (EC-10) | Task 2 | `RegisterUser_WhenRedisCacheWriteFails_StillReturns201_AndWalletWorks` |
| Story 4 — 500 / network UI | Task 1, 4 | Manual UI checklist |
| Story 4 — observability | Task 3 | Log review + optional `Activity` on register |
| Stories 1–3 regression | Task 4 | Duplicate + validation + happy-path suites |

## Architecture impact

```text
┌─────────────┐  POST (once)        ┌──────────────┐
│  web/       │ ──────────────────► │ UsersEndpoint │
│ register-   │  submit guard       └──────┬───────┘
│ form.tsx    │◄── 500 / 422 TAKEN       │
└─────────────┘                         ▼
                         ┌──────────────────────────────┐
                         │ RegisterUserCommandHandler   │
                         │  ExistsByUsername/Email      │──► 422 TAKEN (retry)
                         │  UoW commit (atomic)         │
                         └──────────────┬───────────────┘
                                        ▼
                         ┌──────────────────────────────┐
                         │ PostCommitSessionCacheBehavior│
                         │  Redis fail → warn only       │──► EC-10, still 201
                         └──────────────────────────────┘

Rare concurrent race (no mapper): second SaveChanges may → 500; DB still ≤1 wallet.
```

| Layer | Change summary |
|-------|----------------|
| Domain | **REUSE** — BR-01 unchanged |
| Application | **REUSE** `RegisterUserCommandHandler` exists-checks — no new persistence ports |
| Infrastructure | **REUSE** — no Npgsql constraint mapper |
| Api | **MODIFY** `UsersEndpoint` OpenAPI **500**; **REUSE** `ExceptionHandlingMiddleware` |
| MatchingEngine | None |
| web/ | **MODIFY** `map-register-error.ts`, `register-form.tsx`, optional toast suppress on register |
| AppHost | None |

## Data & migration plan

| Artifact | Action | DB reference |
|----------|--------|--------------|
| EF migration | **None** | — |
| `ux_users_username`, `ux_users_email` | **REUSE** — correctness backstop only (no app mapper) | DB §6.1 |
| `user_sessions` | **REUSE** — authoritative on EC-10 | DB §4.9, §12.2 |
| Redis `session:{id}` | **REUSE** — best-effort after commit | DB §12.1 |

## Open questions

| # | Question | Source | Answer | Status |
|---|----------|--------|--------|--------|
| 1 | Postgres unique-violation mapper? | User / Story 2 | **No** — exists-check + client submit guard + DB indexes; rare race may **500** | ✅ |
| 2 | Simulate EC-09 with test double? | Test design | **Yes** — fake `IUserRepository` throws on `AddAsync` | ✅ |
| 3 | Toast + form alert on 500? | UX | **Form Alert only**; `suppressErrorToast` on register POST | ✅ |
| 4 | Block double-click before `isPending`? | Client | **Yes** — `useRef` submit guard | ✅ |

## Risks and mitigations

| Risk | Likelihood | Impact | Mitigation | Owner task |
|------|------------|--------|------------|------------|
| Concurrent race returns **500** instead of **422** | Low | Low | Accept for MVP; DB prevents second wallet; integration test asserts wallet count | Task 2 |
| Client guard bypassed (devtools) | Low | Low | DB unique indexes still cap accounts | — |
| Real Postgres outage → **500** | — | — | Expected EC-09; retry UX | Task 1 |

## Prerequisites

- [x] Stories 1–3 merged or on branch base
- [x] Branch `feature/user-registration-story-4` from latest `main`
- [x] Docker for Testcontainers
- [x] `yarn --cwd web api:verify` green on base

## File structure (planned)

```text
MODIFY  src/Api/Endpoints/UsersEndpoint.cs
MODIFY  contracts/openapi/api.v1.yaml                    (via api:export)
CREATE  tests/Api.IntegrationTests/Users/RegisterUserTransientFailureTests.cs
MODIFY  tests/Testing.Common/Fixtures/IntegrationTestWebApplicationFactory.cs  (optional test services hook)
MODIFY  web/src/features/auth/map-register-error.ts
MODIFY  web/src/features/auth/register-form.tsx
MODIFY  web/src/features/auth/api.ts                      (optional suppressErrorToast)
MODIFY  web/src/lib/api/types.ts
MODIFY  web/src/lib/api/interceptors.ts
```

## Authorization, session, and domain notes

- **BR-01 / BR-09:** Unchanged — atomic register; retry hits exists-check.
- **EC-03:** Primary mitigation is **client** (one in-flight submit). DB indexes prevent duplicate wallets if two requests still land.
- **EC-04:** Second submit after success uses **exists-check** (Story 2) — no new server code.
- **EC-10:** Already implemented in `PostCommitSessionCacheBehavior` — verify with test double.

## Progress tracker

### Task 1: Registration transient-error UX and double-submit guard

| Attribute | Value |
|-----------|--------|
| Spec story | Story 4 |
| Depends on | None |
| Estimated complexity | S |
| Parent story issue | #8 |

#### Objective

End-to-end slice: users get retry guidance on **500** / network failure; rapid double-click cannot fire two register requests from the form.

#### Implementation notes

- `applyRegisterApiError`: **500** / `INTERNAL_ERROR` and non-`ApiError` (network) → `root` message: “Something went wrong. Please try again.” (do not imply account was not created — EC-04).
- `register-form.tsx`: show root error (`Alert` or `FieldError`); **submit guard** with `useRef` — block second `mutate` until `onSettled`; keep password clear on error and preserve username/email.
- Optional: `suppressErrorToast` on `POST /api/users` so Sonner does not duplicate the form alert.
- **No** Api/Application changes for EC-03 in this task.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `web/src/features/auth/map-register-error.ts` | 500 / network / root |
| MODIFY | `web/src/features/auth/register-form.tsx` | Guard + alert |
| MODIFY | `web/src/features/auth/api.ts` | Toast suppress |
| MODIFY | `web/src/lib/api/types.ts` | Flag |
| MODIFY | `web/src/lib/api/interceptors.ts` | Respect flag |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | API down → retry copy, re-submit | `web/` |
| Manual | Double-click → one navigation | `web/` |

#### Acceptance criteria

- [x] **500** and network errors show generic retry copy; submit re-enables
- [x] Username/email preserved; passwords cleared on error
- [x] Second click while pending does not start another register request

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| RFC 7807 | Client maps existing API codes only |
| ADR needed? | No |

#### Risk

None.

---

### Task 2: Integration tests (retry, 500, Redis EC-10, wallet invariant)

| Attribute | Value |
|-----------|--------|
| Spec story | Story 4 |
| Depends on | None (parallel with Task 1) |
| Estimated complexity | M |
| Parent story issue | #8 |

#### Objective

Automated proof of EC-04, EC-09, EC-10, and “at most one wallet” under parallel submit — without constraint mapper.

#### Implementation notes

- **EC-04:** `RegisterUser_RetrySameCredentials_Returns422_NotSecondWallet` — **201** then identical **POST** → **422** `USERNAME_TAKEN` (exists-check); wallet count unchanged.
- **EC-09:** Fake `IUserRepository` throws on `AddAsync` → **500** `INTERNAL_ERROR`; no new users/wallets.
- **EC-10:** Fake `ISessionRedisCache` throws on `TryWriteAsync` → **201**, cookie, `GET /api/wallet` **200**.
- **EC-03 (simplified):** `RegisterUser_ParallelSameUsername_AtMostOneWallet` — `Task.WhenAll` two posts, same username; assert **exactly one** **201** and **at most one** new wallet row (second response may be **422** or **500** — both acceptable; no mapper required).
- Extend `IntegrationTestWebApplicationFactory` with optional `ConfigureServices` callback for fakes (.NET 10 — `ConfigureTestServices` removed from Mvc.Testing).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `tests/Api.IntegrationTests/Users/RegisterUserTransientFailureTests.cs` | Scenarios above |
| MODIFY | `tests/Testing.Common/Fixtures/IntegrationTestWebApplicationFactory.cs` | Test DI hook |
| REUSE | `RegisterUserDuplicateTests.cs` | Count patterns |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | All four scenarios | `RegisterUserTransientFailureTests.cs` |

#### Acceptance criteria

- [x] Retry-after-success never returns second **201**
- [x] Persistence-failure test: **500**, counts unchanged
- [x] Redis-failure test: **201** + wallet authorized
- [x] Parallel test: exactly one **201**, wallet count +1 (not +2)

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PostgreSQL authoritative | Failed register rolls back — no orphans on **500** test |
| Redis projection | EC-10 |

#### Risk

Parallel test timing — use same username and assert DB invariant, not only status codes.

---

### Task 3: Observability and API contract polish

| Attribute | Value |
|-----------|--------|
| Spec story | Story 4 |
| Depends on | Task 1 (optional, for manual verify) |
| Estimated complexity | S |
| Parent story issue | #8 |

#### Objective

OpenAPI documents **500** for register; logging remains safe (no passwords).

#### Implementation notes

- `UsersEndpoint`: `.ProducesProblem(500)`.
- `yarn --cwd web api:export` → commit `contracts/openapi/api.v1.yaml`.
- Optional: `Activity` on `RegisterUserCommand` in `LoggingBehavior` — **skipped** (no existing `ActivitySource` in repo; `LoggingBehavior` already logs `{RequestName}` only, no command/password).
- Confirm failures log at Error without command/password payload — **verified** (no change).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `src/Api/Endpoints/UsersEndpoint.cs` | Produces 500 |
| MODIFY | `contracts/openapi/api.v1.yaml` | Export |
| MODIFY | `src/Application/Behaviors/LoggingBehavior.cs` | Optional Activity |

#### Acceptance criteria

- [x] `api:verify` passes
- [x] OpenAPI lists **500** on `POST /api/users`

#### Risk

None.

---

### Task 4: Polish — regression and manual UI checklist

| Attribute | Value |
|-----------|--------|
| Spec story | Story 4 |
| Depends on | Tasks 1–3 |
| Estimated complexity | S |
| Parent story issue | #8 |

#### Objective

No regressions; manual sign-off; update memory docs.

#### Implementation notes

- Run all `RegisterUser*` integration tests.
- Update `docs/memory/current-status.md` — note EC-03 simplified (no mapper; client + DB).
- Optional: document accepted residual **500** on extreme concurrent race in `known-issues.md` if desired.

#### Manual UI checklist

Operator sign-off in browser (Aspire + `web` dev server). Automated coverage noted in parentheses.

- [ ] Single submit → trading view, $100,000 (integration: `RegisterUser_Returns201_AndWalletShowsInitialCash`)
- [ ] Double-click → one account / one navigation (Task 1 submit guard — manual only)
- [ ] **500** / network → retry message, can submit again (Task 1 — manual or stop API)
- [ ] Register then retry same username → taken message, no second wallet (integration: `RegisterUser_RetrySameCredentials_Returns422_NotSecondWallet`)
- [ ] Stories 2–3 validation/duplicate copy still correct (integration: validation + duplicate suites)

#### Acceptance criteria

- [x] Full `Users/` integration suite green — **18** Testcontainers tests passed (`RegisterUserSessionTests` excluded; requires local Postgres on :5432)
- [ ] Manual checklist done — pending operator sign-off (see above)

#### Risk

None.

---

## Reference files

| File | Why open it |
|------|-------------|
| `src/Application/Users/Commands/RegisterUserCommandHandler.cs` | Exists-check (EC-04) — no changes expected |
| `src/Application/Behaviors/PostCommitSessionCacheBehavior.cs` | EC-10 |
| `src/Api/Middleware/ExceptionHandlingMiddleware.cs` | **500** mapping |
| `tests/Api.IntegrationTests/Users/RegisterUserDuplicateTests.cs` | Patterns |
| `web/src/features/auth/register-form.tsx` | Task 1 |

## Implementation details (for /build)

**No** `IRegisterUserUniqueViolationMapper`, **no** `UnitOfWorkBehavior` changes for `23505`.

**EC-03:** `register-form.tsx` — `submittingRef` set true before `registerMutation.mutate`, cleared in `onSettled`. Button stays `disabled={registerMutation.isPending}`.

**EC-04:** Rely on existing `ExistsByUsernameAsync` / `ExistsByEmailAsync` in handler — integration test only.

**Test factory hook:** optional `Action<IServiceCollection>` on `IntegrationTestWebApplicationFactory` / `IntegrationTestFixture.CreateFactory` — applied via `ConfigureServices` on the web host builder (.NET 10 Mvc.Testing; no `ConfigureTestServices`).

**Parallel test assertion (simplified):**

- Count wallets before/after; delta ≤ 1.
- Count responses with status **201**; equals 1.

**Client copy:** “Something went wrong. Please try again.”

## Verification matrix (plan-level)

| Spec AC | Verified by |
|---------|-------------|
| Single submit → one account | Task 4 + Story 1 test |
| Double-click → ≤1 wallet | Task 1 manual + Task 2 parallel test |
| 500 / timeout → retry UX | Task 1 manual + Task 2 simulated 500 |
| Retry after success → TAKEN | Task 2 exists-check test |
| PG down → 500, no orphans | Task 2 |
| Redis fail → still registered | Task 2 |

## Rollback / recovery

- Revert branch; no DB migration.

## Deferred work (Plan B)

- Postgres unique-violation mapper + register-scoped `UnitOfWorkBehavior` catch (if product requires **422** on all concurrent races).
- `registrations_total` metric.
- Playwright double-submit E2E.

## GitHub Links

| Local ref | Issue # | Type | Title | URL |
|-----------|---------|------|-------|-----|
| spec.Story 4 | 8 | Story | US-01 / Story 4: Recover from transient failures | https://github.com/tranvuongduy2003/trading-simulator/issues/8 |
| spec (epic) | 4 | Epic | Spec: User registration (US-01) | https://github.com/tranvuongduy2003/trading-simulator/issues/4 |
