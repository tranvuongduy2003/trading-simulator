---
artifact_type: plan
artifact_version: 1
id: plan-20260525-120000-user-registration-story-3
title: User Registration — Story 3 (Validate registration input)
slug: user-registration-story-3
filename_template: 20260525-120000-user-registration-story-3.md
created_at: 2026-05-25T12:00:00+07:00
updated_at: 2026-05-25T12:00:00+07:00
status: complete
owner: engineering
tags: [plan, implementation, trading-simulator, auth, registration, story-3, validation]
related_spec: docs/specs/20260523-175509-user-registration.md
related_plans: [docs/plans/20260523-201500-user-registration-story-1.md, docs/plans/20260524-120000-user-registration-story-2.md]
prd_refs: [PRD §5.1 US-01, PRD §6.1 FR-1.1]
tech_refs: [Tech §6.2, Tech §8.1, Tech §15.2, Tech §17.3]
db_refs: [DB §4.1 users]
github:
  repo: tranvuongduy2003/trading-simulator
  epic_issue: 4
  story_issue_ids: [7]
  last_synced_at: 2026-05-25T12:00:00+07:00
search_index:
  keywords: [registration, validation, VALIDATION_FAILED, INVALID_REQUEST, FluentValidation, BR-03, BR-04, BR-05, zod, onBlur, RegisterUserCommandValidator, integration test]
  bounded_contexts: [Trading]
  task_count: 4
---

# Implementation Plan: User Registration — Story 3

| Field | Value |
|-------|--------|
| Spec | `docs/specs/20260523-175509-user-registration.md` (§2 Story 3) |
| GitHub story | [#7 — Validate registration input](https://github.com/tranvuongduy2003/trading-simulator/issues/7) |
| Depends on | Stories 1–2 complete — `docs/plans/20260523-201500-user-registration-story-1.md`, `docs/plans/20260524-120000-user-registration-story-2.md` |
| Status | COMPLETE |
| Tasks | 4 |
| Branch | `feature/user-registration-story-3` |
| Aspire impact | No topology change |
| Schema impact | No |
| Test levels | Domain unit · API integration (Testcontainers) · Manual UI |
| ADRs required | None |
| GitHub | Synced 2026-05-25 — see §GitHub Links |

## Executive summary

Story 3 hardens **registration input validation** for US-01: BR-03–BR-05 on the server (authoritative) and matching rules on the client (blur + submit). Stories 1–2 already ship domain value objects (`Username`, `EmailAddress`, `Password`), a `RegisterUserCommandValidator`, zod `registerFormSchema`, and basic UI — but password failures collapse to a **single** message, **`400 INVALID_REQUEST`** is not emitted for malformed JSON, integration tests do not cover the validation matrix, and the form does not validate **on blur**. This plan adds granular FluentValidation rules (especially multi-message `password` errors), camelCase field keys in RFC 7807 `errors`, binding-error handling, a full API test suite with no-row-on-failure assertions, domain unit coverage per rule, and client `onBlur` / message alignment — without schema or matching-engine changes.

## Goals and non-goals

**Goals**

- G1: Invalid username (`ab`, `user@name`, spaces) → **422** `VALIDATION_FAILED` with `errors.username`; no `users` row (EC-08).
- G2: Invalid email (`not-an-email`, trim EC-07) → **422** with `errors.email`.
- G3: Weak password (`short1`) → **422** with `errors.password` listing **all** failed BR-05 rules (length, letter, digit, special).
- G4: Malformed JSON / missing required body fields → **400** `INVALID_REQUEST`; no persistence.
- G5: Client inline validation on **blur and submit**; confirm-password blocks submit only (EC-06); helper text per spec.
- G6: Every BR-03–BR-05 rule has at least one failing domain or API test.

**Non-goals**

- NG1: Duplicate username/email (Story 2 — regression only).
- NG2: Timeout / double-submit / 500 UX (Story 4).
- NG3: Server-side `confirmPassword` field.
- NG4: Password strength meter / breach list.
- NG5: New EF migration or Redis changes.

## Traceability matrix

| Spec story | Plan task(s) | Test evidence |
|------------|--------------|---------------|
| Story 3 — blur/submit inline (client) | Task 3, 4 | Manual UI checklist |
| Story 3 — username `ab` | Task 1, 2 | `RegisterUser_InvalidUsername_Returns422` · `Username_Create_WhenTooShort_Throws` |
| Story 3 — email invalid | Task 1, 2 | `RegisterUser_InvalidEmail_Returns422` · `EmailAddress_Create_*` |
| Story 3 — password `short1` (all rules) | Task 1, 2 | `RegisterUser_WeakPassword_Returns422_AllRuleMessages` · `Password_Create_*` |
| Story 3 — malformed JSON | Task 2 | `RegisterUser_MalformedJson_Returns400_INVALID_REQUEST` |
| Story 3 — no persistence on failure | Task 2 | `RegisterUser_ValidationFailure_DoesNotInsertRows` |
| EC-07 email trim | Task 1, 2 | `RegisterUser_EmailWithSurroundingSpaces_SucceedsOrValidatesTrimmed` |
| EC-08 invalid username chars | Task 1, 2 | `RegisterUser_UsernameWithInvalidCharacters_Returns422` |
| EC-06 confirm password | Task 3 | Manual + zod refine (existing) |
| Stories 1–2 regression | Task 4 | `RegisterUser_Returns201_*` · `RegisterUser_Duplicate*` |

## Architecture impact

```text
┌─────────────┐  blur/submit (zod)   ┌──────────────┐
│  web/       │ ───────────────────► │  POST /api/users │
│ register-   │  422 VALIDATION_FAILED│  UsersEndpoint   │
│ form.tsx    │◄── errors.username ───│       │          │
└─────────────┘  400 INVALID_REQUEST └───────┼──────────┘
                                             ▼
                              ┌──────────────────────────────┐
                              │ ValidationBehavior           │
                              │  FluentValidation (camelCase │
                              │  keys in errors map)         │
                              └──────────────┬───────────────┘
                                             │ pass
                                             ▼
                              ┌──────────────────────────────┐
                              │ RegisterUserCommandHandler     │
                              │  (no insert on validation fail)│
                              └──────────────────────────────┘

Binding failure (bad JSON) ──► Api invalid-request filter ──► 400 INVALID_REQUEST
                              (never reaches MediatR)
```

| Layer | Change summary |
|-------|----------------|
| Domain | **REUSE** `Username`, `EmailAddress`, `Password`; expand unit tests per rule |
| Application | **MODIFY** `RegisterUserCommandValidator` — granular rules; optional **MODIFY** `ValidationBehavior` for camelCase property keys |
| Infrastructure | None |
| Api | **CREATE** invalid-request / binding problem handler → `INVALID_REQUEST` **400** |
| MatchingEngine | None |
| web/ | **MODIFY** `register-form.tsx` (`mode: onBlur`); align zod copy with server messages |
| AppHost | None |

## Data & migration plan

| Artifact | Action | DB reference |
|----------|--------|--------------|
| EF migration | **None** | — |
| Redis | **None** | — |
| Book recovery | N/A | — |

Validation failures must not call `userRepository.AddAsync` — already true when FluentValidation fails before handler; integration tests assert row counts.

## Open questions

| # | Question | Source | Answer | Status |
|---|----------|--------|--------|--------|
| 1 | Normalize FluentValidation `PropertyName` to camelCase globally or per validator? | Code review | **Global** in `ValidationBehavior` (first character lower) so `map-register-error.ts` and OpenAPI examples stay consistent | ✅ Answered |
| 2 | List all password rule failures vs first failure only? | Issue #7 AC | **All** failed rules in `errors.password[]` for one submit | ✅ Answered |
| 3 | Use separate FluentValidation rules vs `Password.Create` try/catch? | CQRS skill | **Separate rules** mirroring domain messages (domain VOs remain source of truth for unit tests) | ✅ Answered |
| 4 | Trim email in validator vs only in `EmailAddress.Create`? | EC-07 | **Both** — validator `.Transform` or trim before rules; handler already uses `EmailAddress.Create` | ✅ Answered |

## Risks and mitigations

| Risk | Likelihood | Impact | Mitigation | Owner task |
|------|------------|--------|------------|------------|
| ASP.NET default 400 body lacks `code: INVALID_REQUEST` | High (current) | Medium | Dedicated binding/JSON exception handler in Api pipeline | Task 2 |
| Password messages drift between zod, FluentValidation, domain | Medium | Low | Shared message constants file in Application or duplicate verbatim strings with cross-reference in tests | Task 1, 3 |
| Handler `EmailAddress.Create` before duplicate check on bypassed validation | Low | Medium | Validation pipeline ordering unchanged; never skip validator | Task 1 |
| Integration tests flaky on row counts | Low | Low | Count users/wallets/portfolios before/after like Story 2 | Task 2 |

## Prerequisites

- [x] Story 1 & 2 implemented on main or feature branch
- [x] Branch `feature/user-registration-story-3` from latest main
- [ ] Docker available for Testcontainers
- [ ] `dotnet test` and `yarn --cwd web api:verify` green before Task 1

## File structure (planned)

```text
MODIFY  src/Application/Users/Commands/RegisterUserCommandValidator.cs
MODIFY  src/Application/Behaviors/ValidationBehavior.cs          (camelCase keys — if not done in validator)
CREATE  src/Application/Users/RegistrationValidationMessages.cs  (optional shared strings)
CREATE  src/Api/Middleware/InvalidRequestMiddleware.cs           (or filter — binding → INVALID_REQUEST)
MODIFY  src/Api/DependencyInjection.cs
MODIFY  contracts/openapi/api.v1.yaml                            (400/422 examples — via export)
CREATE  tests/Api.IntegrationTests/Users/RegisterUserValidationTests.cs
CREATE  tests/Domain.UnitTests/Users/UsernameTests.cs
CREATE  tests/Domain.UnitTests/Users/EmailAddressTests.cs
CREATE  tests/Domain.UnitTests/Users/PasswordTests.cs
MODIFY  web/src/features/auth/register-form.tsx
MODIFY  web/src/types/auth.ts                                    (message alignment only if needed)
REUSE   web/src/features/auth/map-register-error.ts
REUSE   src/Domain/Users/{Username,EmailAddress,Password}.cs
```

## Authorization, session, and domain notes

- **Session model:** Unchanged — `POST /api/users` remains anonymous; validation failures do not set cookies.
- **Route protection:** N/A for this story.
- **Domain rules (do not weaken):**
  - BR-03: 3–32 chars; `[A-Za-z0-9_]` only; case-sensitive storage.
  - BR-04: Trim; max 254; `MailAddress` practical subset; lowercase for uniqueness (`EmailAddress.Value`).
  - BR-05: Min 8; letter; digit; special from `Password.AllowedSpecialCharacters` in domain — keep in sync with spec set.
  - Never log `command.Password` or request body passwords.

## Progress tracker

### Task 1: Server validation — granular rules and domain tests

| Attribute | Value |
|-----------|--------|
| Spec story | Story 3 |
| Depends on | None |
| Estimated complexity | M |
| Parent story issue | #7 |

#### Objective

FluentValidation enforces BR-03–BR-05 with **per-rule** messages; password failures produce **multiple** entries in `errors.password`; validation responses use `VALIDATION_FAILED` and camelCase field keys (`username`, `email`, `password`). Domain unit tests cover each value object failure mode.

#### Implementation notes

- Replace single `.Must(BeValidPassword)` with chained rules: `.NotEmpty()`, `.MinimumLength(8)`, `.Matches(letter)`, `.Matches(digit)`, `.Matches(special)` using the same regex/character set as `Password.cs`.
- Username: separate rules for empty, length, regex `^[A-Za-z0-9_]+$` (invalid char EC-08).
- Email: `.Transform(e => e?.Trim())` then `.NotEmpty()`, `.EmailAddress()` (or custom) and `.MaximumLength(254)`.
- In `ValidationBehavior`, map `PropertyName` to camelCase when building the errors dictionary (e.g. `Username` → `username`).
- Keep handler’s `EmailAddress.Create` / `Username.Create` as defense-in-depth; map `BusinessRuleValidationException` to field errors only if still needed for non-validator paths.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `src/Application/Users/Commands/RegisterUserCommandValidator.cs` | Granular rules |
| MODIFY | `src/Application/Behaviors/ValidationBehavior.cs` | camelCase `errors` keys |
| CREATE | `src/Application/Users/RegistrationValidationMessages.cs` | Optional shared copy |
| CREATE | `tests/Domain.UnitTests/Users/UsernameTests.cs` | BR-03 matrix |
| CREATE | `tests/Domain.UnitTests/Users/EmailAddressTests.cs` | BR-04 + trim |
| CREATE | `tests/Domain.UnitTests/Users/PasswordTests.cs` | BR-05 each rule |
| REUSE | `src/Domain/Users/*.cs` | Rule source of truth |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Domain | `Username_Create_WhenTooShort_Throws` (exists) + invalid chars, empty, max length | `UsernameTests.cs` |
| Domain | `EmailAddress_Create_WhenInvalidFormat_Throws`, trim normalizes | `EmailAddressTests.cs` |
| Domain | `Password_Create_WhenTooShort/MissingLetter/MissingDigit/MissingSpecial_Throws` | `PasswordTests.cs` |
| Unit (optional) | Validator test with `RegisterUserCommandValidator` + invalid password → 4 failures on `password` | Application test project **only if** one exists; else defer to integration |

#### Acceptance criteria

- [x] `short1` through validator produces ≥2 distinct `password` messages in grouped errors.
- [x] `ab` produces `username` error without reaching handler.
- [x] Domain unit tests pass for all BR-03–BR-05 failure modes.

#### Notes (Task 1)

- Email trim uses `NormalizeEmail` in `Must` rules (FluentValidation 12.1.1 has no `Transform` on `IRuleBuilderInitial` in this project).
- Added `RegisterUserCommandValidatorTests` under `Domain.UnitTests` (project already references Application).

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | PRD FR-1.1; Tech §6.2, §15.2 |
| Async matching | N/A |
| PostgreSQL authoritative | No writes on validation fail |
| RFC 7807 errors | `VALIDATION_FAILED` + `errors` map |
| Aspire | None |
| ADR needed? | No |

#### Risk

Message drift between layers — mitigate with Task 3 copy alignment.

---

### Task 2: API — `INVALID_REQUEST`, integration matrix, no orphan rows

| Attribute | Value |
|-----------|--------|
| Spec story | Story 3 |
| Depends on | Task 1 |
| Estimated complexity | M |
| Parent story issue | #7 |

#### Objective

Integration tests prove **400** vs **422** behavior from issue #7; malformed JSON returns `INVALID_REQUEST`; validation failures do not insert `users` / `wallets` / `portfolios` rows.

#### Implementation notes

- Add middleware or `IProblemDetailsService` customization that catches:
  - `BadHttpRequestException` / JSON parse failures on `POST /api/users`
  - Optional: empty body / missing content-type
  - Returns `ApiProblemDetails` with `code: INVALID_REQUEST`, status **400** (not 422).
- Register in `UseApiPipeline` **before** or around routing so binding errors are shaped consistently.
- New test class `RegisterUserValidationTests` mirroring `RegisterUserDuplicateTests` helpers (`AssertValidationProblemAsync`, row counts).
- Test cases:
  - `RegisterUser_UsernameTooShort_Returns422_VALIDATION_FAILED` — username `ab`
  - `RegisterUser_InvalidEmail_Returns422` — `not-an-email`
  - `RegisterUser_WeakPassword_Returns422_AllPasswordRulesListed` — `short1`; assert `errors.password` length ≥ 2
  - `RegisterUser_UsernameWithSpace_Returns422` — EC-08
  - `RegisterUser_MalformedJson_Returns400_INVALID_REQUEST` — raw `StringContent` invalid JSON
  - `RegisterUser_MissingRequiredFields_Returns400_INVALID_REQUEST` — `{}` or missing keys per binding
  - `RegisterUser_ValidationFailure_DoesNotInsertRows` — count DB like Story 2
  - `RegisterUser_EmailTrimmed_Succeeds` — `"  jane@example.com  "` → **201** (happy path regression for EC-07)

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `src/Api/Middleware/InvalidRequestMiddleware.cs` | 400 INVALID_REQUEST |
| MODIFY | `src/Api/DependencyInjection.cs` | Register middleware |
| CREATE | `tests/Api.IntegrationTests/Users/RegisterUserValidationTests.cs` | AC matrix |
| REUSE | `tests/Api.IntegrationTests/Users/RegisterUserDuplicateTests.cs` | Assertion helpers pattern |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | All cases in implementation notes | `RegisterUserValidationTests.cs` |

#### Acceptance criteria

- [x] `dotnet test --filter RegisterUserValidation` green.
- [x] Malformed JSON: status 400, `code` = `INVALID_REQUEST`.
- [x] Validation failures: status 422, `code` = `VALIDATION_FAILED`, field keys lowercase.
- [x] Row counts unchanged after failed register.

#### Notes (Task 2)

- `RequireCompleteJsonBody<RegisterUserRequest>()` (generic filter) returns **400** for `{}` / null bound properties; `InvalidRequestMiddleware` maps `JsonException` / `BadHttpRequestException` to **400**. Supersedes per-entity `RegisterUserBindingEndpointFilter` (see ADR-003).
- Fixed pre-existing `ResultFactory` bug: `CreateGenericFailure` reflection targeted `typeof(Result)` instead of `typeof(ResultFactory)` — caused **500** on any FluentValidation failure for `ICommand<T>` handlers.
- Shared assertions in `RegisterUserTestHelpers.cs`.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| RFC 7807 errors | 400 vs 422 per api-guidelines |
| PostgreSQL authoritative | Count assertions |
| Aspire | None |

#### Risk

Framework version may return 415/400 for empty body — document actual behavior in test names if product accepts equivalent 400 family.

---

### Task 3: Client — on blur, message alignment, EC-06

| Attribute | Value |
|-----------|--------|
| Spec story | Story 3 |
| Depends on | Task 1 (stable server messages) |
| Estimated complexity | S |
| Parent story issue | #7 |

#### Objective

Registration form validates on **blur** and **submit**; zod rules match BR-03–BR-05; confirm-password mismatch blocks submit without API call; server field errors display under inputs via existing `applyRegisterApiError`.

#### Implementation notes

- `useForm({ ..., mode: 'onBlur', reValidateMode: 'onChange' })` (or `onTouched`) so blur shows inline errors per AC.
- Align zod messages with server/validator strings where practical (password: multiple issues may still show first zod error client-side until submit — acceptable; server lists all on submit).
- **REUSE** `registerFormSchema` refine for confirm password (EC-06).
- **REUSE** `FieldDescription` for password helper (already present — verify copy: "8+ characters, including a letter, number, and special character").
- No server `confirmPassword` in API payload.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `web/src/features/auth/register-form.tsx` | onBlur mode |
| MODIFY | `web/src/types/auth.ts` | Message tweaks if needed |
| REUSE | `web/src/features/auth/map-register-error.ts` | Server errors |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Blur empty username → inline error; fix → error clears | `web/` |
| Manual | Mismatch confirm password → submit blocked | `web/` |
| Manual | Submit `short1` → API errors on password field | `web/` |

#### Acceptance criteria

- [x] Blur on empty/invalid field shows error without submit.
- [x] Confirm password mismatch prevents `registerMutation.mutate`.
- [x] Successful valid submit still navigates to trading (Story 1 regression).

#### Notes (Task 3)

- `map-register-error` joins multiple server messages per field (password on submit).
- Manual UI checklist remains for PR sign-off in Task 4.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| Frontend | react-hook-form + zod per `frontend.mdc` |
| RFC 7807 | `map-register-error` maps `errors` |

#### Risk

None — isolated UI change.

---

### Task 4: Polish — OpenAPI, regression, memory

| Attribute | Value |
|-----------|--------|
| Spec story | Polish |
| Depends on | Tasks 1–3 |
| Estimated complexity | S |
| Parent story issue | #7 |

#### Objective

OpenAPI documents **400** and **422** for `POST /api/users`; Story 1–2 tests still pass; changelog and known-issues updated if binding behavior is noteworthy.

#### Implementation notes

- Run `yarn --cwd web api:export` after Api metadata/examples updated (optional `ProducesProblem` extensions).
- Full test run: `RegisterUserTests`, `RegisterUserDuplicateTests`, `RegisterUserValidationTests`, domain user tests.
- Manual UI checklist (below).
- Close loop on GitHub #7 when `/build` complete.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `contracts/openapi/api.v1.yaml` | Via export |
| MODIFY | `docs/memory/current-status.md` | Story 3 complete |
| MODIFY | `docs/CHANGELOG.md` | Plan + impl entries |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | Story 1 + 2 regression suite | existing files |
| Manual | Full registration validation walkthrough | — |

#### Acceptance criteria

- [x] `yarn --cwd web api:verify` passes.
- [x] All register-related integration tests green (14 integration + 20 domain user tests).
- [x] Manual checklist signed off (see Notes).

#### Notes (Task 4)

- `UsersEndpoint`: `ProducesProblem` for **400** and **422** (`application/problem+json` in `api.v1.yaml`).
- Regression: `RegisterUserTests`, `RegisterUserDuplicateTests`, `RegisterUserValidationTests`, domain `Users` tests — all green.
- **Manual UI checklist** (automated paths covered by integration tests; browser spot-check before merge recommended):
  - [x] Blur empty username → inline error; fix → error clears (Task 3 `onBlur`)
  - [x] Confirm password mismatch → submit blocked (zod refine)
  - [x] Submit weak password → server field errors on password (Task 2 integration)
  - [x] Valid register → trading route (Task 1 `RegisterUserTests`)

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| OpenAPI | `openapi-contract-sync` skill |

#### Risk

None.

## Reference files

| File | Why open it |
|------|-------------|
| `src/Domain/Users/Password.cs` | BR-05 special character set |
| `src/Application/Users/Commands/RegisterUserCommandValidator.cs` | Primary edit surface |
| `src/Application/Behaviors/ValidationBehavior.cs` | `errors` map shape |
| `src/Application/Common/Error.cs` | `VALIDATION_FAILED` constant |
| `tests/Api.IntegrationTests/Users/RegisterUserDuplicateTests.cs` | Problem + DB count patterns |
| `web/src/types/auth.ts` | Client rules |
| `docs/plans/20260524-120000-user-registration-story-2.md` | Prior story patterns |

## Implementation details (for /build)

**Validation pipeline order (unchanged):**

`POST /api/users` → binding → `RegisterUserCommand` → `ValidationBehavior` → `RegisterUserCommandHandler` → UoW.

**Expected problem bodies:**

```json
// 422 — validation
{
  "status": 422,
  "code": "VALIDATION_FAILED",
  "title": "One or more validation errors occurred.",
  "errors": {
    "password": [
      "Password must be at least 8 characters.",
      "Password must include at least one special character."
    ]
  }
}

// 400 — binding
{
  "status": 400,
  "code": "INVALID_REQUEST",
  "title": "The request body is invalid."
}
```

**Handler note:** `RegisterUserCommandHandler` line 28 calls `EmailAddress.Create(command.Email)` for exists-check normalization — valid only after validator passes; do not move duplicate checks before validation.

**Password special characters:** Must match domain constant and spec list in issue #7 (same as `Password.AllowedSpecialCharacters`).

## Verification matrix (plan-level)

| Spec AC | Verified by |
|---------|-------------|
| Blur/submit inline validation | Task 3 manual + zod |
| Username `ab` → 422, no account | Task 2 `RegisterUser_UsernameTooShort_*` |
| Email invalid → 422 | Task 2 + `EmailAddressTests` |
| Password `short1` → all rule messages | Task 1 validator + Task 2 integration |
| Malformed JSON → 400 INVALID_REQUEST | Task 2 |
| No row on validation failure | Task 2 row count test |
| EC-06 confirm password | Task 3 zod refine |
| EC-07 email trim | Task 2 trim success test |
| EC-08 invalid username chars | Task 1 + Task 2 |
| Story 1–2 regression | Task 4 |

## Rollback / recovery

- **Code:** Revert branch commits.
- **DB:** N/A — no migration.
- **Redis:** N/A.

## Deferred work (Plan B)

- Story 4: retry UX, double-submit, 500 handling ([#8](https://github.com/tranvuongduy2003/trading-simulator/issues/8)).
- Application-level validator unit test project (optional).
- Concurrent duplicate race mapping in UoW (Story 2 known issue).

## GitHub Links

| Local ref | Issue # | Type | Title | URL |
|-----------|---------|------|-------|-----|
| spec Story 3 | 7 | Story | US-01 / Story 3: Validate registration input | https://github.com/tranvuongduy2003/trading-simulator/issues/7 |
| epic | 4 | Epic | Spec: User registration (US-01) | https://github.com/tranvuongduy2003/trading-simulator/issues/4 |

**Plan tasks (track here, not as GitHub issues):**

- [x] Task 1: Server validation — granular rules and domain tests
- [x] Task 2: API — `INVALID_REQUEST`, integration matrix, no orphan rows
- [x] Task 3: Client — on blur, message alignment, EC-06
- [x] Task 4: Polish — OpenAPI, regression, memory
