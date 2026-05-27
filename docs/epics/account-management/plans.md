---
artifact_type: epic-archive-plans
artifact_version: 2
id: epic-account-management-plans
title: Account Management — archived implementation plans
epic: Account Management (PRD §5.1)
user_stories: [US-01, US-02, US-03, US-04]
archived_at: 2026-05-28T16:00:00Z
consolidation_mode: epic-record-plus-verbatim
source_count: 18
sources_deleted: true
source_files:
  - docs/plans/20260523-201500-user-registration-story-1.md
  - docs/plans/20260524-120000-user-registration-story-2.md
  - docs/plans/20260525-120000-user-registration-story-3.md
  - docs/plans/20260525-095103-user-registration-story-4.md
  - docs/plans/20260525-150000-user-login-story-1.md
  - docs/plans/20260525-160000-user-login-story-2.md
  - docs/plans/20260525-170000-user-login-story-3.md
  - docs/plans/20260525-180000-user-login-story-4.md
  - docs/plans/20260525-190000-user-login-story-5.md
  - docs/plans/20260525-203000-virtual-cash-story-1.md
  - docs/plans/20260525-220000-virtual-cash-story-2.md
  - docs/plans/20260525-230000-virtual-cash-story-3.md
  - docs/plans/20260525-240000-virtual-cash-story-4.md
  - docs/plans/20260525-260000-portfolio-reset-story-1.md
  - docs/plans/20260527-210000-portfolio-reset-story-2.md
  - docs/plans/20260527-214600-portfolio-reset-story-3.md
  - docs/plans/20260527-231500-portfolio-reset-story-4.md
  - docs/plans/20260528-003204-portfolio-reset-story-5.md
related_review: docs/reviews/20260528-180000-account-management.md
tags: [epic-archive, trading-simulator, account-management]
---

# Account Management — implementation plans (archive)

> **Authoritative epic engineering archive.** Individual files under `docs/plans/` were merged here on 2026-05-28 and **deleted**. Part 1 indexes what was implemented per plan; Part 2 is the **full verbatim** plan text (tasks, `[x]`/`[ ]`, files, tests, manual checklists).

## Part 1 — Epic implementation record

### Plan index (18 plans)

| # | Plan | US | Story | Automation | Manual UI |
|---|------|-----|-------|------------|-----------|
| 1 | `20260523-201500-user-registration-story-1.md` | US-01 | 1 | ✅ Tasks `[x]` | ⏳ checklist |
| 2 | `20260524-120000-user-registration-story-2.md` | US-01 | 2 | ✅ | — |
| 3 | `20260525-120000-user-registration-story-3.md` | US-01 | 3 | ✅ | — |
| 4 | `20260525-095103-user-registration-story-4.md` | US-01 | 4 | ✅ | ⏳ operator |
| 5 | `20260525-150000-user-login-story-1.md` | US-02 | 1 | ✅ | ⏳ |
| 6 | `20260525-160000-user-login-story-2.md` | US-02 | 2 | ✅ | ⏳ Aspire |
| 7 | `20260525-170000-user-login-story-3.md` | US-02 | 3 | ✅ | ⏳ |
| 8 | `20260525-180000-user-login-story-4.md` | US-02 | 4 | ✅ | ⏳ |
| 9 | `20260525-190000-user-login-story-5.md` | US-02 | 5 | ✅ | ⏳ |
| 10 | `20260525-203000-virtual-cash-story-1.md` | US-03 | 1 | ✅ | ⏳ |
| 11 | `20260525-220000-virtual-cash-story-2.md` | US-03 | 2 | ✅ | ⏳ |
| 12 | `20260525-230000-virtual-cash-story-3.md` | US-03 | 3 | ✅ | ⏳ |
| 13 | `20260525-240000-virtual-cash-story-4.md` | US-03 | 4 | ✅ | ⏳ |
| 14 | `20260525-260000-portfolio-reset-story-1.md` | US-04 | 1 | ✅ | ⏳ |
| 15 | `20260527-210000-portfolio-reset-story-2.md` | US-04 | 2 | ✅ | ⏳ smoke |
| 16 | `20260527-214600-portfolio-reset-story-3.md` | US-04 | 3 | ✅ | ⏳ operator |
| 17 | `20260527-231500-portfolio-reset-story-4.md` | US-04 | 4 | ✅ | ⏳ |
| 18 | `20260528-003204-portfolio-reset-story-5.md` | US-04 | 5 | ✅ | ⏳ |

### Key code surfaces (where to look in repo)

| Area | Paths |
|------|--------|
| Auth commands | `src/Application/Users/` — `RegisterUser`, `LoginUser`, `LogoutUser` |
| Wallet query | `GetMyWalletQuery`, `src/Api` wallet endpoint |
| Reset | `ResetPortfolioCommand`, `PortfolioResetWriteRepository`, eligibility query |
| Frontend auth | `web/src/features/auth/` |
| Frontend wallet | `web/src/features/trading/` — `use-wallet-query`, cash card, chip |
| Frontend reset | `use-reset-portfolio`, `portfolio-activity-tabs`, panel query keys |
| Tests | `tests/Api.IntegrationTests/Users/*`, `tests/Api.IntegrationTests/Portfolios/ResetPortfolioTests.cs` |
| Domain tests | `tests/Domain.UnitTests/Users/` |

### Implementation timeline (by US)

**US-01 (4 plans):** E2E register skeleton → duplicate detection 422 → password/validation polish → transient failure + double-submit UX.

**US-02 (5 plans):** Login API + cookie → session store Redis/Postgres → protected routes + wallet probe → logout EC-07 → login FluentValidation 422.

**US-03 (4 plans):** GET wallet + dashboard shell → reserved breakdown display → session-private cache / user-scoped keys → post-login refetch `staleTime: 0`.

**US-04 (5 plans):** Reset POST stub + dialog → wallet/holdings persistence → cancel orders + history cutoff + SignalR → 24h cooldown eligibility → TanStack invalidation + activity tabs.

### Tests delivered (integration highlights)

- `RegisterUserTests`, `RegisterUserValidationTests`, `RegisterUserDuplicateTests`, `RegisterUserTransientFailureTests`
- `LoginUserTests`, `LoginUserValidationTests`, `LogoutUserTests`, `SessionPersistenceTests`
- `GetMyWalletTests` (10 scenarios)
- `ResetPortfolioTests` (22 scenarios — cooldown, mutation, history, notifications)

### Operator follow-ups (from epic review)

Manual UI checklists remain in **Part 2** inside each plan’s §Manual UI checklist. Epic review: [`docs/reviews/20260528-180000-account-management.md`](../../reviews/20260528-180000-account-management.md).

---

## Part 2 — Verbatim archived plans

### Table of contents

1. [registration-story-1](#source-20260523-201500-user-registration-story-1md)
2. [registration-story-2](#source-20260524-120000-user-registration-story-2md)
3. [registration-story-3](#source-20260525-120000-user-registration-story-3md)
4. [registration-story-4](#source-20260525-095103-user-registration-story-4md)
5. [login-story-1](#source-20260525-150000-user-login-story-1md)
6. [login-story-2](#source-20260525-160000-user-login-story-2md)
7. [login-story-3](#source-20260525-170000-user-login-story-3md)
8. [login-story-4](#source-20260525-180000-user-login-story-4md)
9. [login-story-5](#source-20260525-190000-user-login-story-5md)
10. [virtual-cash-story-1](#source-20260525-203000-virtual-cash-story-1md)
11. [virtual-cash-story-2](#source-20260525-220000-virtual-cash-story-2md)
12. [virtual-cash-story-3](#source-20260525-230000-virtual-cash-story-3md)
13. [virtual-cash-story-4](#source-20260525-240000-virtual-cash-story-4md)
14. [portfolio-reset-story-1](#source-20260525-260000-portfolio-reset-story-1md)
15. [portfolio-reset-story-2](#source-20260527-210000-portfolio-reset-story-2md)
16. [portfolio-reset-story-3](#source-20260527-214600-portfolio-reset-story-3md)
17. [portfolio-reset-story-4](#source-20260527-231500-portfolio-reset-story-4md)
18. [portfolio-reset-story-5](#source-20260528-003204-portfolio-reset-story-5md)


---

## Source 1 of 18: `docs/plans/20260523-201500-user-registration-story-1.md`

---
artifact_type: plan
artifact_version: 1
id: plan-20260523-201500-user-registration-story-1
title: User Registration — Story 1 (Register and enter simulator)
slug: user-registration-story-1
filename_template: 20260523-201500-user-registration-story-1.md
created_at: 2026-05-23T20:15:00+07:00
updated_at: 2026-05-23T23:30:00+07:00
status: completed
owner: engineering
tags: [plan, implementation, trading-simulator, auth, registration, story-1]
related_spec: docs/specs/20260523-175509-user-registration.md
related_plans: []
prd_refs: [PRD §5.1 US-01, PRD §6.1 FR-1.1, PRD §6.1 FR-1.3, PRD §7.4, PRD §8.1]
tech_refs: [Tech §5.2.1, Tech §6.2, Tech §8.1, Tech §15.1, Tech §15.2, Tech §15.3, Tech §16.2, Tech §17.3]
db_refs: [DB §4.1, DB §4.2, DB §4.3, DB §4.9, DB §12.1, DB §6.1]
github:
  repo: tranvuongduy2003/trading-simulator
  epic_issue: 4
  story_issue_ids: [5]
  task_issue_ids: [10, 11, 12, 13, 14, 15, 16]
  last_synced_at: 2026-05-23T20:20:00+07:00
search_index:
  keywords: [registration, signup, POST users, session cookie, wallet, portfolio, User aggregate, RegisterUserCommand, Testcontainers, react-hook-form, InitialVirtualCash]
  bounded_contexts: [Trading]
  task_count: 7
---

# Implementation Plan: User Registration — Story 1

| Field | Value |
|-------|--------|
| Spec | `docs/specs/20260523-175509-user-registration.md` |
| GitHub story | [#5 — Register and enter the simulator](https://github.com/tranvuongduy2003/trading-simulator/issues/5) |
| Status | COMPLETE (Tasks 1–7 done) |
| Tasks | 7 |
| Branch | `feature/user-registration-story-1` |
| Aspire impact | Yes — uses existing Postgres + Redis; dev migration apply on Api startup |
| Schema impact | Yes — initial migration `InitialTradingSchema` (users, wallets, portfolios, holdings, symbols, user_sessions) |
| Test levels | Domain unit · API integration (Testcontainers) · Manual UI |
| ADRs required | Session cookie + password hashing (Task 7) |
| GitHub | See §GitHub Links (synced after task issues created) |

## Executive summary

Story 1 delivers the **happy path** for US-01: a logged-out visitor registers with username, email, and password; the system atomically provisions user + USD 100,000 wallet + empty portfolio, issues a cookie session, and the client lands on the trading view showing starting cash and zero AAPL shares. The codebase is currently a **greenfield skeleton** (health endpoint only; placeholder register page; no domain aggregates or EF migrations). This plan builds in seven vertical slices from schema stub through domain, CQRS, session auth, API contract, UI, and an integration test proving read-your-writes on `GET /wallet`. Duplicate-identity UX (Story 2), exhaustive validation matrices (Story 3), and retry/idempotency polish (Story 4) are explicitly deferred.

## Goals and non-goals

**Goals**

- G1: Satisfy Story 1 acceptance criteria (register → session → trading view with $100k / 0 AAPL).
- G2: Enforce BR-01, BR-02, BR-06, BR-07, BR-08 on the register path; EC-05 via existing `PublicRoute`.
- G3: Establish patterns for `RegisterUserCommand`, session middleware, and OpenAPI sync reused by US-02+.

**Non-goals** (this plan will not do)

- NG1: Login, logout, session refresh (US-02).
- NG2: Dedicated `USERNAME_TAKEN` / `EMAIL_TAKEN` API codes and UI copy (Story 2) — DB unique constraints may surface generic errors until Story 2.
- NG3: Full validation matrix and double-submit hardening (Stories 3–4).
- NG4: Order placement, matching engine, SignalR market events on register.
- NG5: Email verification, CAPTCHA, rate limiting.

## Traceability matrix

| Spec story | Plan task(s) | Test evidence |
|------------|--------------|---------------|
| Story 1 — happy path | Tasks 1–6, 7 | `RegisterUser_Returns201_AndWalletShowsInitialCash` (Integration); domain wallet balance tests |
| Story 1 — EC-05 already authenticated | Task 6 (reuse `PublicRoute`) | Manual: open `/register` while logged in → `/trading` |
| Story 2–4 (epic) | Deferred — Plan B | — |

## Architecture impact

```text
┌─────────────┐   POST /api/users    ┌──────────────┐   RegisterUserCommand   ┌─────────────────┐
│  web/       │ ───────────────────► │  Api         │ ─────────────────────► │  Application    │
│  Register   │   cookie + 201       │  UsersEndpoint│                        │  Handler + UoW  │
│  Trading    │ ◄── GET /wallet ────── │  Auth middleware                       └────────┬────────┘
└─────────────┘                      └──────────────┘                                  │
                                                                                       ▼
                                                                              ┌─────────────────┐
                                                                              │  Domain         │
                                                                              │  User+Wallet+   │
                                                                              │  Portfolio      │
                                                                              └────────┬────────┘
                                                                                       │
                                                                                       ▼
                                                                              ┌─────────────────┐
                                                                              │  Infrastructure │
                                                                              │  EF + Redis     │
                                                                              │  session cache  │
                                                                              └─────────────────┘
```

| Layer | Change summary |
|-------|----------------|
| Domain | `User` aggregate, `Wallet` entity, `Portfolio` aggregate shell; VOs `Username`, `EmailAddress`, `PasswordHash`, `Money`, typed IDs; `UserRegisteredEvent` |
| Application | `RegisterUserCommand` + validator; `GetMyWalletQuery`, `GetMyPortfolioQuery`; ports `IUserRepository`, `ISessionStore`, `IPasswordHasher`, `ICurrentUserAccessor` |
| Infrastructure | EF configurations + `InitialTradingSchema` migration; repositories; Redis `session:{id}`; ASP.NET cookie auth; password hashing |
| Api | `UsersEndpoint`, `WalletEndpoint`, `PortfolioEndpoint`; cookie issuance; auth middleware; OpenAPI metadata |
| MatchingEngine | None |
| web/ | Registration form (RHF + zod); register mutation; trading page wallet/holdings summary; align `authApi` types with contract |
| AppHost | No topology change; ensure `Trading:InitialVirtualCash` flows to Api |

## Data & migration plan

| Artifact | Action | DB reference |
|----------|--------|--------------|
| EF migration `InitialTradingSchema` | **Add** — `users`, `wallets`, `portfolios`, `holdings`, `symbols` (seed `AAPL`), `user_sessions` | DB §4.1–4.3, §4.4, §4.5, §4.9 |
| Redis `session:{session_id}` | **Add** on register — TTL = session expiry | DB §12.1 |
| Book recovery | N/A | — |

Apply migration on Api startup in Development (per `migration.mdc`). Seed `symbols` (`AAPL`) inside migration, not at runtime.

## Open questions

| # | Question | Source | Answer | Status |
|---|----------|--------|--------|--------|
| 1 | 409 vs 422 for duplicate username/email? | Spec §13 Q4 | **422** always. Story 1: generic `VALIDATION_FAILED` or `CONFLICT` on unique-index failure. Story 2: stable `USERNAME_TAKEN` / `EMAIL_TAKEN` codes | ✅ Answered |
| 2 | Password hasher implementation? | Tech §15.2 | **`Microsoft.AspNetCore.Identity.PasswordHasher<TUser>`** behind Application `IPasswordHasher`; wire in Infrastructure (Task 3) | ✅ Answered |
| 3 | Session cookie name and TTL? | Tech §15.1 | No prior code convention — adopt **`TradingSimulator.Session`** cookie (config `Session:CookieName`), TTL **`Session:ExpirationHours`** default **24**; Redis cache key **`session:{session_id}`** per DB §12.1 | ✅ Answered |
| 4 | `GET /wallet` response shape for session probe? | `web` `useSession` | Extend Contracts DTO: `userId`, `username`, `availableBalance`, `currency` — replaces ad-hoc `userIdentifier` / `displayName` | ✅ Answered |

**Status legend:** ❓ Unanswered · ✅ Answered · ⏳ Deferred

## Risks and mitigations

| Risk | Likelihood | Impact | Mitigation | Owner task |
|------|------------|--------|------------|------------|
| Partial registration (user without wallet) | M | H | Single UoW transaction in `RegisterUserCommand`; integration test asserts wallet row | Task 3 |
| Redis session write fails after PG commit (EC-10) | L | M | Session valid via PG; cache on next read; log warning, do not fail 201 | Task 4 |
| OpenAPI / frontend type drift | M | M | Run `yarn --cwd web api:export` after Task 5; CI `api:verify` | Task 5 |
| Testcontainers not available locally | L | M | Document Docker prerequisite; keep `FoundationSmokeTests` for CI without containers until fixture lands | Task 7 |
| `web` register request shape mismatch (`displayName` vs `username`) | H | M | Fix `RegisterRequest` to `{ username, email, password }` in Task 6 | Task 6 |

## Prerequisites

- [ ] Spec approved for implementation (currently `draft` — user may proceed for Story 1 planning)
- [ ] Docker Desktop running; `aspire run` from AppHost
- [ ] `gh` authenticated (for issue sync)
- [ ] Greenfield: no prior migrations to conflict

## File structure (planned)

```text
src/
  TradingSimulator.Domain/
    Users/              User.cs, Wallet.cs, UserId.cs, Username.cs, EmailAddress.cs, PasswordHash.cs
    Portfolios/         Portfolio.cs, PortfolioId.cs
    Common/             Money.cs
    Events/             UserRegisteredEvent.cs
  TradingSimulator.Application/
    Users/              RegisterUserCommand.cs, RegisterUserCommandHandler.cs, RegisterUserCommandValidator.cs
    Users/Queries/      GetMyWalletQuery.cs, GetMyPortfolioQuery.cs (+ handlers)
    Abstractions/       IUserRepository.cs, ISessionStore.cs, IPasswordHasher.cs, ICurrentUserAccessor.cs
  TradingSimulator.Infrastructure/
    Persistence/        Configurations/*, Repositories/UserRepository.cs
    Auth/               PasswordHasher.cs, SessionStore.cs, CurrentUserAccessor.cs
    Migrations/         <timestamp>_InitialTradingSchema.cs
  TradingSimulator.Contracts/
    Users/              RegisterUserRequest.cs, UserRegistrationResponse.cs, WalletSummaryDto.cs
    Portfolio/          PortfolioResponse.cs, HoldingDto.cs
  TradingSimulator.Api/
    Endpoints/          UsersEndpoint.cs, WalletEndpoint.cs, PortfolioEndpoint.cs
    Auth/               SessionAuthenticationHandler.cs (or cookie middleware)
tests/
  TradingSimulator.Domain.UnitTests/Users/
  TradingSimulator.Api.IntegrationTests/Users/
web/src/
  features/auth/        register-form.tsx, api.ts (fix types)
  features/trading/     wallet-summary.tsx (or inline in trading-page)
contracts/openapi/      api.v1.yaml (generated)
```

## Authorization, session, and domain notes

- **Session model:** Server-side `user_sessions` row (PG authoritative) + Redis `session:{session_id}` cache with TTL. Cookie name **`TradingSimulator.Session`** (override via `Session:CookieName`); HttpOnly, Secure, SameSite=Lax on `POST /api/users` success (BR-06). US-02 login/logout must reuse the same cookie name and session store.
- **Route protection:** `GET /api/wallet`, `GET /api/portfolio` require authenticated session; `POST /api/users` is anonymous.
- **Domain rules (do not violate):**
  - BR-01: `User.Register` factory creates wallet + portfolio in same persistence transaction.
  - BR-02: Initial cash from `IOptions<TradingOptions>.InitialVirtualCash` (100000.0000), `reserved_balance = 0`.
  - BR-07: No `holdings` rows on register (portfolio empty → UI shows 0 AAPL).
  - BR-08: Display copy only — virtual simulation currency.

## Progress tracker

### Task 1: Skeleton — schema stub and register/wallet HTTP placeholders

| Attribute | Value |
|-----------|--------|
| Spec story | Story 1 |
| Depends on | None |
| Estimated complexity | M |
| GitHub issue | #10 |

#### Objective

An implementer can run Aspire, apply the initial EF migration, call `POST /api/users` and receive **201** with a placeholder body and `Set-Cookie`, and call `GET /api/wallet` to observe **401** without cookie or a stub **200** with cookie — proving the HTTP pipeline before domain logic lands.

#### Implementation notes

- Add `InitialTradingSchema` migration matching `DATABASE.md` for tables needed by registration (not full orders/trades yet if splitting migrations — prefer **one** initial migration with all MVP tables from DATABASE.md to avoid follow-up churn, or split: registration tables only + add orders in later epic; **recommend full MVP schema in one migration** per `migration.mdc` checklist).
- Register `IDevelopmentMigrationRunner` or apply `Database.Migrate()` on Api startup in Development only.
- `UsersEndpoint`: `POST /api/users` returns hard-coded 201 + dummy cookie (middleware not required yet).
- `WalletEndpoint`: `GET /api/wallet` returns 401 or static JSON when `X-Stub-Session` present (temporary until Task 4).
- Add `.WithName`, `.WithTags`, `.Produces` for OpenAPI.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `src/Infrastructure/Persistence/Configurations/UserConfiguration.cs` | EF model |
| CREATE | `src/Infrastructure/Persistence/Configurations/WalletConfiguration.cs` | EF model |
| CREATE | `src/Infrastructure/Persistence/Configurations/PortfolioConfiguration.cs` | EF model |
| CREATE | `src/Infrastructure/Persistence/Configurations/UserSessionConfiguration.cs` | EF model |
| CREATE | `src/Infrastructure/Persistence/Configurations/SymbolConfiguration.cs` | Seed AAPL |
| CREATE | `src/Infrastructure/Persistence/Migrations/*_InitialTradingSchema.cs` | Schema |
| MODIFY | `src/Infrastructure/Persistence/ApplicationDatabaseContext.cs` | DbSets |
| CREATE | `src/Api/Endpoints/UsersEndpoint.cs` | POST stub |
| CREATE | `src/Api/Endpoints/WalletEndpoint.cs` | GET stub |
| MODIFY | `src/Infrastructure/DependencyInjection.cs` | Dev migrate helper |
| REUSE | `src/Api/Endpoints/ApiHealthEndpoint.cs` | Endpoint pattern |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `PostUsers_Stub_ReturnsCreated` | `tests/Api.IntegrationTests/...` (optional with WebApplicationFactory only) |
| Manual | Scalar → POST /api/users | — |

#### Acceptance criteria

- [x] Migration applies cleanly on fresh Postgres (Aspire).
- [x] `POST /api/users` returns 201 from running Api.
- [x] `GET /api/health` still returns 200.

#### Task 1 completion notes (2026-05-23)

**Deviations from file list:**

- EF persistence models live under `Infrastructure/Persistence/Entities/*Record.cs` (infrastructure-only; domain aggregates land in Task 2).
- Migration output: `Infrastructure/Persistence/Migrations/` (not `Infrastructure/Migrations/`).
- Single migration includes **full MVP schema** (orders, trades, candlesticks, portfolio_resets) per plan recommendation — not registration tables only.
- Added `ApplicationDatabaseContextDesignTimeFactory` for `dotnet ef` tooling.
- `DevelopmentDatabaseMigrationHostedService` skips migrate when no connection string (keeps `WebApplicationFactory` tests working without Docker).
- Integration tests: `RegisterUserStubTests` (POST 201 + cookie, GET wallet 401/200 with `X-Stub-Session`).

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | FR-1.3 schema; DB §4.1–4.3, §4.9 |
| Async matching | N/A |
| PostgreSQL authoritative | Yes |
| Redis projection | N/A |
| RFC 7807 errors | Stub only |
| SignalR | N/A |
| Aspire | No AppHost change |
| ADR needed? | No |

#### Risk

None — isolated scaffolding.

---

### Task 2: Domain — User registration aggregate and value objects

| Attribute | Value |
|-----------|--------|
| Spec story | Story 1 |
| Depends on | Task 1 |
| Estimated complexity | M |
| GitHub issue | #11 |

#### Objective

`User.Register(...)` encapsulates BR-01/BR-02/BR-07: creates wallet with configured initial cash, empty portfolio, raises `UserRegisteredEvent`; invalid username/email/password rejected at construction with `BusinessRuleValidationException`.

#### Implementation notes

- `Money` enforces non-negative, 4 decimal places.
- `Username` / `EmailAddress` enforce BR-03/BR-04 (email normalized to lowercase in factory).
- `PasswordHash` wraps hashed value only (no plaintext in domain).
- `Portfolio` created with no holdings collection entries.
- Map `Trading:InitialVirtualCash` in Application when calling domain factory (domain receives `Money` amount, not `IConfiguration`).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `src/Domain/Users/User.cs` | Aggregate root |
| CREATE | `src/Domain/Users/Wallet.cs` | Owned entity |
| CREATE | `src/Domain/Portfolios/Portfolio.cs` | Aggregate root |
| CREATE | `src/Domain/Common/Money.cs` | Value object |
| CREATE | `src/Domain/Users/*.cs` | Username, Email, PasswordHash, UserId |
| CREATE | `src/Domain/Portfolios/PortfolioId.cs` | Typed id |
| CREATE | `src/Domain/Events/UserRegisteredEvent.cs` | Domain event |
| CREATE | `tests/Domain.UnitTests/Users/UserRegisterTests.cs` | Invariant tests |
| REUSE | `src/Domain/Abstractions/AggregateRoot.cs` | Base type |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Domain | `Register_WithValidInput_CreatesWalletWithInitialCash` | `UserRegisterTests.cs` |
| Domain | `Register_WithInvalidUsername_Throws` | `UserRegisterTests.cs` |
| Domain | `Register_RaisesUserRegisteredEvent` | `UserRegisterTests.cs` |

#### Acceptance criteria

- [x] Domain project has zero outward references.
- [x] All domain tests pass.

#### Task 2 completion notes (2026-05-23)

**Deviations:**

- `User.Register` returns `UserRegistrationResult` (`User` + `Portfolio`) — both aggregates created atomically at domain level; Application persists in one UoW (Task 3).
- Added `Password` value object for BR-05 plaintext rules at registration boundary (hashing stays in Application/Infrastructure).
- `EmailAddress` stores normalized lowercase `Value` plus `DisplayValue` for optional display casing.
- Typed IDs use `readonly record struct` (`UserId`, `PortfolioId`); other rules use `ValueObject` base.
- Minimal `Holding` shell type for portfolio structure (no holdings on register per BR-07).

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | Tech §5.2.1; BR-01–02, BR-07 |
| ADR needed? | No |

#### Risk

None — pure domain.

---

### Task 3: Application — RegisterUser command and persistence

| Attribute | Value |
|-----------|--------|
| Spec story | Story 1 |
| Depends on | Task 2 |
| Estimated complexity | L |
| GitHub issue | #12 |

#### Objective

`RegisterUserCommand` persists user, wallet, and portfolio atomically via `IUnitOfWork`; password hashed in Infrastructure; handler returns `RegisterUserResult` with ids and wallet summary for API mapping.

#### Implementation notes

- `RegisterUserCommand : ICommand<Result<RegisterUserResult>>, IUnitOfWorkRequest`.
- Validator: BR-03–BR-05 (FluentValidation).
- `IUserRepository`: `AddAsync(User)`, `ExistsByUsernameAsync`, `ExistsByEmailAsync` (exists checks used minimally in Story 1; full duplicate errors in Story 2).
- On `DbUpdateException` for unique index → map to **422** with generic `VALIDATION_FAILED` or `CONFLICT` (Story 1). Story 2 replaces with `USERNAME_TAKEN` / `EMAIL_TAKEN` (still 422).
- `IPasswordHasher` implementation wraps **`PasswordHasher<User>`** from `Microsoft.AspNetCore.Identity` (package reference on Infrastructure only).
- Dispatch `UserRegisteredEvent` after save via existing `IDomainEventDispatcher`.
- Add `TradingOptions` with `InitialVirtualCash` bound from config.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `src/Application/Users/RegisterUserCommand.cs` | Command + result |
| CREATE | `src/Application/Users/RegisterUserCommandHandler.cs` | Handler |
| CREATE | `src/Application/Users/RegisterUserCommandValidator.cs` | FluentValidation |
| MODIFY | `src/Application/Abstractions/Persistence/IUserRepository.cs` | Methods |
| CREATE | `src/Infrastructure/Persistence/Repositories/UserRepository.cs` | EF implementation |
| CREATE | `src/Application/Options/TradingOptions.cs` | Initial cash |
| CREATE | `src/Infrastructure/Auth/PasswordHasher.cs` | `IPasswordHasher` |
| MODIFY | `src/Infrastructure/DependencyInjection.cs` | Register services |
| MODIFY | `src/Infrastructure/Persistence/ApplicationDatabaseContext.cs` | DbSets mapping |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Domain | (covered Task 2) | — |

#### Acceptance criteria

- [x] Handler compiles; unit of work rolls back if wallet insert fails (manual or integration in Task 7).
- [x] No MediatR handler references `DbContext` directly.

#### Task 3 completion notes (2026-05-23)

**Deviations / additions:**

- `DomainEventDispatchBehavior` + `IPendingDomainEventsCollector` dispatch `UserRegisteredEvent` after UoW commit (not listed in plan file table).
- `IUnitOfWork.IsUniqueConstraintViolation` + `UnitOfWorkBehavior` maps Postgres `23505` → `CONFLICT` (422) for race duplicates.
- `IdentityPasswordHasher` (`Microsoft.Extensions.Identity.Core`) in `Infrastructure/Auth/`; mapper in `Persistence/Mapping/`.
- `ApplicationDatabaseContext` unchanged (DbSets already present from Task 1).

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| CQRS | `cqrs-handler-pattern` — validation before handler, UoW on command |
| PostgreSQL authoritative | Single transaction |
| ADR needed? | No |

#### Risk

Repository mapping errors could break atomicity — verify one `SaveChanges` per request.

---

### Task 4: Session — cookie auth, PG sessions, Redis cache

| Attribute | Value |
|-----------|--------|
| Spec story | Story 1 |
| Depends on | Task 3 |
| Estimated complexity | L |
| GitHub issue | #13 |

#### Objective

Successful registration creates `user_sessions` row, caches `session:{id}` in Redis, sets HttpOnly session cookie; `ICurrentUserAccessor` resolves `UserId` for downstream queries; unauthenticated requests to protected routes return **401**.

#### Implementation notes

- `ISessionStore.CreateSessionAsync(userId)` → session id + expiry.
- Redis write failure: log warning, do not fail registration (EC-10).
- Cookie authentication scheme reads **`TradingSimulator.Session`** (from `IOptions<TradingSessionOptions>.CookieName`), validates session id against Redis then PG fallback.
- `UseAuthentication` / `UseAuthorization` in `UseApiPipeline` before endpoints.
- Remove stub cookie logic from Task 1.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `src/Application/Abstractions/Auth/ISessionStore.cs` | Port |
| CREATE | `src/Application/Abstractions/Auth/ICurrentUserAccessor.cs` | Port |
| CREATE | `src/Infrastructure/Auth/SessionStore.cs` | PG + Redis |
| CREATE | `src/Api/Auth/CurrentUserAccessor.cs` | Scoped (Api host; avoids ASP.NET ref on Infrastructure) |
| CREATE | `src/Application/Behaviors/PostCommitSessionCacheBehavior.cs` | Redis cache after UoW commit (EC-10) |
| CREATE | `src/Application/Options/TradingSessionOptions.cs` | `Session` config section (renamed from `SessionOptions` — ASP.NET name clash) |
| CREATE | `src/Api/Auth/SessionAuthenticationHandler.cs` | Cookie validation |
| MODIFY | `src/Api/DependencyInjection.cs` | Auth registration |
| MODIFY | `src/Api/Program.cs` | `UseAuthentication` order |
| MODIFY | `src/Application/Users/RegisterUserCommandHandler.cs` | Create session after user persist |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | (deferred to Task 7 with Testcontainers) | — |

#### Acceptance criteria

- [x] Cookie set on successful register (via `RegisterUserCommand` + `SessionCookieWriter`; full e2e with Postgres deferred to Task 7).
- [x] `GET /api/wallet` without cookie → 401 (`RegisterUserSessionTests`).

#### Deviations (2026-05-23)

- `TradingSessionOptions` instead of `SessionOptions` (conflicts with `Microsoft.AspNetCore.Builder.SessionOptions`).
- `CurrentUserAccessor` lives in **Api**, not Infrastructure.
- `PostCommitSessionCacheBehavior` (outermost pipeline) flushes Redis **after** `UnitOfWorkBehavior` commit — not in `DomainEventDispatchBehavior`.
- `UsersEndpoint` wired to MediatR + real session cookie (planned for Task 5; done early). `GET /api/wallet` still returns stub body for authenticated users until Task 5 queries land.
- Removed `X-Stub-Session` header and `SessionCookieOptions` (Api); config via `TradingSessionOptions` + `appsettings.json` `Session` section.
- `builder.AddRedisClient("cache")` in Api `Program.cs`.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| Tech §15.1–15.3 | Cookie session |
| Redis | `session:{id}` TTL |
| ADR needed? | Document cookie + `PasswordHasher<User>` choice in Task 7 |

#### Risk

Auth middleware ordering — must run before endpoint execution.

---

### Task 5: API — POST /users and read queries with OpenAPI

| Attribute | Value |
|-----------|--------|
| Spec story | Story 1 |
| Depends on | Task 4 |
| Estimated complexity | M |
| GitHub issue | #14 |

#### Objective

`POST /api/users` returns **201** body matching spec §4b (`userId`, `username`, `email`, `createdAt`, `wallet`); `GET /api/wallet` and `GET /api/portfolio` return real data for authenticated user; OpenAPI YAML updated.

#### Implementation notes

- DTOs in `TradingSimulator.Contracts`.
- ~~Wire `RegisterUserCommand` via MediatR~~ — done in Task 4; verify OpenAPI/contract alignment only.
- `GetMyWalletQuery` / `GetMyPortfolioQuery` — portfolio returns empty holdings array (0 AAPL).
- Export OpenAPI: `yarn --cwd web api:export`.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `src/Contracts/Users/RegisterUserRequest.cs` | Request DTO |
| CREATE | `src/Contracts/Users/UserRegistrationResponse.cs` | 201 body |
| CREATE | `src/Contracts/Users/WalletSummaryDto.cs` | Nested wallet |
| CREATE | `src/Contracts/Portfolio/PortfolioResponse.cs` | Holdings list |
| MODIFY | `src/Api/Endpoints/UsersEndpoint.cs` | Real handler |
| MODIFY | `src/Api/Endpoints/WalletEndpoint.cs` | Query |
| CREATE | `src/Api/Endpoints/PortfolioEndpoint.cs` | Query |
| CREATE | `src/Application/Users/Queries/GetMyWalletQuery.cs` | + handler |
| CREATE | `src/Application/Portfolios/Queries/GetMyPortfolioQuery.cs` | + handler |
| MODIFY | `contracts/openapi/api.v1.yaml` | Generated export |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Scalar POST /users + GET /wallet with cookie | — |

#### Acceptance criteria

- [x] 201 response matches spec field names (camelCase JSON) via `UserRegistrationResponse` / `WalletSummaryDto` in Contracts.
- [x] `yarn --cwd web api:verify` passes.

#### Deviations (2026-05-23)

- Read models use `IWalletReadRepository` / `IPortfolioReadRepository` (not named in original file table).
- `GET /api/portfolio` added as `PortfolioEndpoint` with empty `holdings` after registration.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| RFC 7807 | `ResultHttpExtensions` for failures |
| Read-your-writes | GET wallet immediately after POST |

#### Risk

DTO naming drift vs frontend — coordinate Task 6.

---

### Task 6: Frontend — registration form and trading summary

| Attribute | Value |
|-----------|--------|
| Spec story | Story 1 |
| Depends on | Task 5 |
| Estimated complexity | M |
| GitHub issue | #15 |

#### Objective

Registration screen submits `{ username, email, password }` with confirm-password client check; on success navigates to `/trading` within 2s; trading view shows **USD 100,000.00** available and **0** AAPL shares; authenticated users hitting `/register` redirect to trading (EC-05).

#### Implementation notes

- zod schema mirrors BR-03–BR-05; helper text for password rules.
- TanStack Query `useMutation` for register; `credentials: 'include'`.
- Fix `authApi.register` return type to `UserRegistrationResponse`; update `useSession` to use `GET /api/wallet` shape (`userId`, `username`, balances).
- Loading: disable submit + spinner within 100ms (`isPending`).
- Clear password fields on error; preserve username/email.
- Optional value prop copy per spec §4a.
- Trading page: wallet card + holdings row for AAPL (0 qty) — minimal layout OK before full PRD §8.1 grid.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `web/src/features/auth/api.ts` | Request/response types |
| CREATE | `web/src/features/auth/register-form.tsx` | Form UI |
| MODIFY | `web/src/features/auth/pages/register-page.tsx` | Compose form |
| MODIFY | `web/src/hooks/use-session.ts` | Wallet session shape |
| MODIFY | `web/src/features/trading/pages/trading-page.tsx` | Cash + holdings display |
| CREATE | `web/src/types/auth.ts` | zod schemas |
| REUSE | `web/src/app/routes/public-route.tsx` | EC-05 redirect |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Register → trading shows $100k and 0 AAPL | `web/` |

#### Acceptance criteria

- [x] Story 1 happy path UI wired (register → `/trading` with wallet + AAPL row); verify manually via Aspire.
- [x] EC-05: `PublicRoute` probes session and redirects authenticated users away from `/register`.

#### Deviations (2026-05-23)

- `PublicRoute` runs `useSession()` (not only `authStatus`) so EC-05 works on cold load to `/register`.
- Auth store uses `userId` / `username` instead of `userIdentifier` / `displayName`.
- `web/src/lib/format.ts` added for USD display helpers.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD §8.1 | Minimal trading shell |
| frontend.mdc | TanStack Query, no Zustand for wallet |

#### Risk

CORS/credentials — rely on AppHost `Cors__AllowedOrigins__0` for Vite origin.

---

### Task 7: Integration test, observability, and polish

| Attribute | Value |
|-----------|--------|
| Spec story | Story 1 · Polish |
| Depends on | Task 6 |
| Estimated complexity | M |
| GitHub issue | #16 |

#### Objective

Automated proof that register → cookie → `GET /wallet` returns `100000.0000` available; structured `UserRegistered` log; ADR for session + password hashing; manual checklist complete.

#### Implementation notes

- Add Testcontainers fixture (PostgreSQL + Redis) per `testcontainers-integration-tests` skill; apply migrations before tests.
- `RegisterUser_Returns201_AndWalletShowsInitialCash`: POST register, forward cookie, GET wallet assert balance.
- Remove any remaining stubs from Task 1.
- Log `UserRegistered` with `userId`, `username` (no email/password).
- Append ADR to `docs/memory/decisions.md`.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `tests/Testing.Common/Fixtures/IntegrationTestWebApplicationFactory.cs` | Testcontainers |
| CREATE | `tests/Testing.Common/Fixtures/IntegrationTestFixture.cs` | Container lifecycle + migrate |
| CREATE | `tests/Api.IntegrationTests/Integration/IntegrationTestCollection.cs` | xUnit collection (same assembly as tests) |
| CREATE | `tests/Api.IntegrationTests/Users/RegisterUserTests.cs` | Story 1 proof |
| MODIFY | `docs/memory/decisions.md` | Session + password ADR |
| CREATE | `src/Application/Users/UserRegisteredEventHandler.cs` | Structured log (domain event) |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `RegisterUser_Returns201_AndWalletShowsInitialCash` | `RegisterUserTests.cs` |
| Domain | (existing Task 2) | — |

#### Acceptance criteria

- [x] Integration test passes in CI with Docker.
- [x] Definition of done on [#5](https://github.com/tranvuongduy2003/trading-simulator/issues/5) satisfied (automated path; manual UI checklist still recommended).

#### Deviations (2026-05-23)

- `UserRegistered` logging via `UserRegisteredEventHandler` (`IDomainEventHandler<UserRegisteredEvent>`) instead of `RegisterUserCommandHandler`.
- xUnit `[CollectionDefinition]` with `ICollectionFixture<IntegrationTestFixture>` lives in `Api.IntegrationTests` (must be same assembly as tests); `Testing.Common` holds fixture + factory only.
- `IntegrationTestWebApplicationFactory` uses `UseSetting` for connection strings so `AddInfrastructure` sees Postgres/Redis during host build.
- Migrations applied in fixture via standalone `ApplicationDatabaseContext` before host start (avoids race with `DevelopmentDatabaseMigrationHostedService`).
- No Task 1 HTTP stubs remained to remove.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| Tech §17.3 | Register integration test |
| BR-01 | No partial registration |

#### Risk

Flaky tests if DB not isolated — use collection fixture + respawn or fresh DB per class.

---

## Reference files

| File | Why open it |
|------|-------------|
| `docs/specs/20260523-175509-user-registration.md` | Story 1 AC, BR-xx, API contract |
| `docs/DATABASE.md` §4.1–4.3, §4.9, §12.1 | Column types, indexes, session cache |
| `docs/TECHNICAL.md` §5.2.1, §6.2, §15 | Domain + CQRS + security |
| `src/Api/Endpoints/ApiHealthEndpoint.cs` | Minimal endpoint pattern |
| `src/Application/Behaviors/UnitOfWorkBehavior.cs` | Transaction boundary |
| `src/Api/Mapping/ResultHttpExtensions.cs` | RFC 7807 mapping |
| `web/src/app/routes/public-route.tsx` | EC-05 already implemented |
| `.cursor/skills/openapi-contract-sync/SKILL.md` | Export/codegen workflow |

## Implementation details (for /build)

**Register flow (Task 3–5):**

1. Api binds `RegisterUserRequest` → `RegisterUserCommand`.
2. Validator runs (username/email/password rules).
3. Handler: check duplicates (optional minimal), `passwordHasher.Hash`, `User.Register(...)`, `userRepository.AddAsync`, `sessionStore.CreateSessionAsync`, return result.
4. UoW commits users + wallets + portfolios + user_sessions.
5. Api maps to `UserRegistrationResponse`, appends `Set-Cookie`, returns 201.

**Error codes:**

| Code | HTTP | When | Story |
|------|------|------|-------|
| `VALIDATION_FAILED` | 422 | FluentValidation / domain rule | 1 |
| `CONFLICT` (optional) | 422 | Unique index on register (generic message) | 1 |
| `USERNAME_TAKEN` | 422 | Duplicate username | 2 |
| `EMAIL_TAKEN` | 422 | Duplicate email | 2 |
| `INVALID_REQUEST` | 400 | JSON binding failure | 1 |
| `INTERNAL_ERROR` | 500 | Unhandled | 1 |

**Config keys:**

- `Trading:InitialVirtualCash` (existing `appsettings.json`)
- `Session:CookieName` (default `TradingSimulator.Session`)
- `Session:ExpirationHours` (default 24)

**Password hashing (Task 3):**

- Infrastructure: `Microsoft.AspNetCore.Identity` → `PasswordHasher<User>` (or thin `UserCredential` type if domain `User` should not reference Identity).
- Application port: `IPasswordHasher.Hash(plainText)` / `Verify(plainText, hash)` — no Identity types in Application or Domain.

**Frontend register mutation:**

```text
POST /api/users → 201 + Set-Cookie → invalidate ['auth','session'] → navigate(paths.trading)
```

## Verification matrix (plan-level)

| Spec AC | Verified by |
|---------|-------------|
| Register creates account + $100k wallet + empty portfolio + session + navigate <2s | Task 6 manual + Task 7 integration |
| Trading screen shows $100k and 0 AAPL | Task 6 manual |
| Already authenticated → redirect trading | `PublicRoute` + Task 6 manual |
| Read-your-writes after 201 | Task 7 integration GET wallet |
| BR-01 atomic provisioning | Task 7 integration + UoW |
| EC-10 Redis failure | Task 4 implementation note (optional manual test) |

## Rollback / recovery

- **Code:** Revert branch `feature/user-registration-story-1`.
- **DB:** `dotnet ef database update 0` locally only; or drop Aspire Postgres volume.
- **Redis:** Flush cache; sessions rebuild from PG on miss.

## Deferred work (Plan B)

- **Story 2** ([#6](https://github.com/tranvuongduy2003/trading-simulator/issues/6)): `USERNAME_TAKEN` / `EMAIL_TAKEN`, UI error mapping, exists-check before insert.
- **Story 3** ([#7](https://github.com/tranvuongduy2003/trading-simulator/issues/7)): Full validation matrix, inline field errors, 400 malformed JSON.
- **Story 4** ([#8](https://github.com/tranvuongduy2003/trading-simulator/issues/8)): Retry UX, double-submit protection.
- **US-02**: Login/logout endpoints and pages.
- Full PRD §8.1 trading layout (order book, chart, order form).

## GitHub Links

> Populated by `/plan` Step 8 sync.

| Local ref | Issue # | Type | Title | URL |
|-----------|---------|------|-------|-----|
| spec Story 1 | 5 | Story | US-01 / Story 1: Register and enter the simulator | https://github.com/tranvuongduy2003/trading-simulator/issues/5 |
| plan Task 1 | 10 | Task | Task 1: Skeleton — schema stub and register/wallet HTTP placeholders | https://github.com/tranvuongduy2003/trading-simulator/issues/10 |
| plan Task 2 | 11 | Task | Task 2: Domain — User registration aggregate and value objects | https://github.com/tranvuongduy2003/trading-simulator/issues/11 |
| plan Task 3 | 12 | Task | Task 3: Application — RegisterUser command and persistence | https://github.com/tranvuongduy2003/trading-simulator/issues/12 |
| plan Task 4 | 13 | Task | Task 4: Session — cookie auth, PG sessions, Redis cache | https://github.com/tranvuongduy2003/trading-simulator/issues/13 |
| plan Task 5 | 14 | Task | Task 5: API — POST /users and read queries with OpenAPI | https://github.com/tranvuongduy2003/trading-simulator/issues/14 |
| plan Task 6 | 15 | Task | Task 6: Frontend — registration form and trading summary | https://github.com/tranvuongduy2003/trading-simulator/issues/15 |
| plan Task 7 | 16 | Task | Task 7: Integration test, observability, and polish | https://github.com/tranvuongduy2003/trading-simulator/issues/16 |


---

<a id="source-20260524-120000-user-registration-story-2md"></a>

## Source 2 of 18: `docs/plans/20260524-120000-user-registration-story-2.md`

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


---

<a id="source-20260525-120000-user-registration-story-3md"></a>

## Source 3 of 18: `docs/plans/20260525-120000-user-registration-story-3.md`

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


---

<a id="source-20260525-095103-user-registration-story-4md"></a>

## Source 4 of 18: `docs/plans/20260525-095103-user-registration-story-4.md`

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


---

<a id="source-20260525-150000-user-login-story-1md"></a>

## Source 5 of 18: `docs/plans/20260525-150000-user-login-story-1.md`

---
artifact_type: plan
artifact_version: 1
id: plan-20260525-150000-user-login-story-1
title: User Login — Story 1 (Log in and access portfolio)
slug: user-login-story-1
filename_template: 20260525-150000-user-login-story-1.md
created_at: 2026-05-25T15:00:00+07:00
updated_at: 2026-05-25T18:30:00+07:00
status: draft
owner: engineering
tags: [plan, implementation, trading-simulator, auth, login, session, story-1]
related_spec: docs/specs/20260525-103709-user-login.md
related_plans: [docs/plans/20260523-201500-user-registration-story-1.md]
prd_refs: [PRD §5.1 US-02, PRD §6.1 FR-1.2, PRD §7.4, PRD §10.1]
tech_refs: [Tech §6.2, Tech §8.1, Tech §15.1, Tech §15.2, Tech §15.3, Tech §16.2, Tech §17.3]
db_refs: [DB §4.1 users, DB §4.9 user_sessions, DB §6.1 ux_users_email, DB §6.8 user_sessions, DB §12.1 session cache, DB §12.2]
github:
  repo: tranvuongduy2003/trading-simulator
  epic_issue: 21
  story_issue_ids: [22]
  last_synced_at: 2026-05-25T15:00:00+07:00
search_index:
  keywords: [login, POST auth/login, session cookie, LoginUserCommand, email password, GetByEmail, password verify, wallet probe, portfolio, PublicRoute, protected redirect, Testcontainers]
  bounded_contexts: [Trading]
  task_count: 6
---

# Implementation Plan: User Login — Story 1

| Field | Value |
|-------|--------|
| Spec | `docs/specs/20260525-103709-user-login.md` |
| GitHub story | [#22 — Log in and access my portfolio](https://github.com/tranvuongduy2003/trading-simulator/issues/22) |
| Epic | [#21 — User login (US-02)](https://github.com/tranvuongduy2003/trading-simulator/issues/21) |
| Status | COMPLETE (manual UI sign-off pending) |
| Tasks | 6 |
| Branch | `feature/user-login-story-1` |
| Aspire impact | No — reuses Postgres + Redis from registration |
| Schema impact | No — `user_sessions` and `users` already exist |
| Test levels | API integration (Testcontainers) · Manual UI |
| ADRs required | None (reuse ADR-001 session cookie, ADR-002 password hashing) |
| GitHub | Synced 2026-05-25 — see §GitHub Links |

## Executive summary

Story 1 delivers the **happy path** for US-02: a returning user signs in with email and password, receives a new server session (PostgreSQL row + Redis cache + HttpOnly cookie), and lands on the trading view with **their** wallet and portfolio data. Registration (US-01) already provides session infrastructure, `GET /api/wallet` as a session probe, `PublicRoute` / `ProtectedRoute` guards, and a placeholder login page. This plan adds `POST /api/auth/login` end-to-end (CQRS command, email lookup, password verification, cookie issuance), one integration test proving read-your-writes after login, and a real login form with post-login redirect to a saved `from` path. Invalid-credentials UX, logout, session-expiry messaging, and exhaustive validation are **deferred** to Stories 2–5 in epic #21.

## Goals and non-goals

**Goals**

- G1: Satisfy Story 1 acceptance criteria (login → session → trading view with correct wallet/portfolio).
- G2: Enforce BR-01 (no new accounts), BR-08 (no wallet/holdings mutation on login), BR-09 (reads reflect PostgreSQL).
- G3: Reuse registration session patterns (`SessionStore`, `SessionCookieWriter`, `PendingSessionCacheCollector`, cookie name `TradingSimulator.Session`).
- G4: Restore protected-route deep links via `location.state.from` after successful login.

**Non-goals** (this plan will not do)

- NG1: Story 2 — uniform `INVALID_CREDENTIALS` messaging, enumeration-safety tests, wrong-password UI polish (handler may return **401** for failed lookup; dedicated tests/copy in Story 2).
- NG2: Story 3 — session persistence across reload / expiry UX (existing cookie + `useSession` largely cover reload; expiry copy deferred).
- NG3: Story 4 — `POST /api/auth/logout` and user-menu logout.
- NG4: Story 5 — **422** validation matrix, double-submit hardening, **500** retry UX on login form.
- NG5: Password reset, username login, rate limiting, SignalR changes on login.

## Traceability matrix

| Spec story | Plan task(s) | Test evidence |
|------------|--------------|---------------|
| Story 1 — login happy path | Tasks 1–4, 5 | `LoginUser_Returns200_AndWalletShowsRegisteredUser` (Integration) |
| Story 1 — portfolio after login | Task 4 | `LoginUser_AfterRegister_PortfolioReturnsHoldings` (Integration) |
| Story 1 — redirect to saved route | Task 5 | Manual: open `/trading` logged out → login → lands on `/trading` |
| Story 1 — EC-05 already authenticated | Task 6 (reuse `PublicRoute`) | Manual: open `/login` while logged in → `/trading` |
| Stories 2–5 (epic) | Deferred — Plan B | — |

## Architecture impact

```text
┌─────────────┐  POST /api/auth/login   ┌──────────────┐   LoginUserCommand    ┌─────────────────┐
│  web/       │ ─────────────────────► │ AuthEndpoint │ ───────────────────► │  Application    │
│  login-form │   Set-Cookie + 200     │  (new)       │                      │  LoginHandler   │
│  Trading    │ ◄── GET /wallet ─────── │              │                      └────────┬────────┘
└─────────────┘                        └──────────────┘                               │
                                                                                      ▼
                                                                             ┌─────────────────┐
                                                                             │ IUserRepository │
                                                                             │ GetByEmailAsync │
                                                                             │ IPasswordHasher │
                                                                             │ .Verify         │
                                                                             │ ISessionStore   │
                                                                             └────────┬────────┘
                                                                                      │
                                                                                      ▼
                                                                             ┌─────────────────┐
                                                                             │  PG user_sessions│
                                                                             │  Redis session:* │
                                                                             └─────────────────┘
```

| Layer | Change summary |
|-------|----------------|
| Domain | **None** — login is orchestration; no new aggregates or wallet/portfolio mutations |
| Application | `LoginUserCommand`, handler (no FluentValidation in Story 1 — Story 5); `LoginErrors` or shared auth errors; extend `IUserRepository.GetByEmailAsync`; extend `IPasswordHasher.Verify` |
| Infrastructure | `UserRepository.GetByEmailAsync` (load user + hash); `IdentityPasswordHasher.Verify`; **REUSE** `SessionStore`, `SessionRedisCache` |
| Api | **CREATE** `AuthEndpoint` (`POST /api/auth/login`); **REUSE** `SessionCookieWriter`, `Result.ToHttpResult()` |
| MatchingEngine | None |
| web/ | `login-form.tsx`, `loginFormSchema`, login mutation, `map-login-error` stub; update `LoginPage`; fix `authApi.login` response type |
| AppHost | None |

## Data & migration plan

| Artifact | Action | DB reference |
|----------|--------|--------------|
| EF migration | **None** | — |
| `user_sessions` | **Insert** on login (same as register) | DB §4.9 |
| Redis `session:{session_id}` | **Set** on login via `IPendingSessionCacheCollector` | DB §12.1 |
| `users` | **Read** by normalized email only | DB §4.1, §6.1 `ux_users_email` |
| Book recovery | N/A | — |

## Open questions

| # | Question | Source | Answer | Status |
|---|----------|--------|--------|--------|
| 1 | Login response includes wallet vs separate GETs? | Spec §13 Q1 | **Separate GETs** — mirror registration post-auth flow (`GET /api/wallet`, `GET /api/portfolio`) | ✅ Answered |
| 2 | HTTP 200 + body vs 204? | Spec §13 Q3 | **200** + `LoginUserResponse` (`userId`, `username`, `email`) for client auth store | ✅ Answered |
| 3 | Minimal validator in Story 1 vs wait for Story 5? | Issue #22 out of scope | **Wait for Story 5** — Story 1 has no `LoginUserCommandValidator` and no login **422** tests; binding only via `RequireCompleteJsonBody` (**400**). Story 5 (#26) adds FluentValidation, zod format rules, field-level **422**, and integration tests | ✅ Answered |
| 4 | Return **401** on bad credentials in Story 1? | Story 2 scope | **Yes** in handler (`INVALID_CREDENTIALS`) so happy-path tests can assert wrong password fails; Story 2 adds enumeration-safety integration tests and UI copy | ✅ Answered |

## Risks and mitigations

| Risk | Likelihood | Impact | Mitigation | Owner task |
|------|------------|--------|------------|------------|
| Login diverges from register session cookie/TTL | M | H | Reuse `SessionCookieWriter`, `TradingSessionOptions`, `CreateSessionAsync` — no parallel auth path | Task 3 |
| `IPasswordHasher` only hashes today | H | H | Add `Verify(Password, PasswordHash)` using same Identity hasher | Task 2 |
| `IUserRepository` cannot load user by email | H | H | Add `GetByEmailAsync` returning domain `User` (or auth read model with hash) | Task 2 |
| OpenAPI / `authApi.login` typed as `void` | M | M | `LoginUserResponse` contract + `api:export` in Task 6 | Task 6 |
| Frontend `from` redirect lost | L | M | Read `location.state.from` in login success handler; default `paths.trading` | Task 5 |
| Redis cache fail after PG session (EC-10) | L | M | **REUSE** post-commit session cache behavior from registration | Task 3 |

## Prerequisites

- [ ] US-01 registration Stories 1–4 merged or available on branch (session + `POST /api/users` working)
- [ ] Docker + `aspire run` (or Testcontainers for integration tests)
- [ ] Spec `status: draft` — user may proceed; implementation follows this plan

## File structure (planned)

```text
src/
  TradingSimulator.Application/
    Users/Commands/           LoginUserCommand.cs, LoginUserCommandHandler.cs
    Users/                    LoginErrors.cs (or AuthErrors.cs)
    Abstractions/Persistence/ IUserRepository.cs (+ GetByEmailAsync)
    Abstractions/Auth/        IPasswordHasher.cs (+ Verify)
  TradingSimulator.Infrastructure/
    Persistence/Repositories/ UserRepository.cs
    Auth/                     PasswordHasher.cs
  TradingSimulator.Contracts/
    Users/                    LoginUserRequest.cs, LoginUserResponse.cs
  TradingSimulator.Api/
    Endpoints/                AuthEndpoint.cs
tests/
  TradingSimulator.Api.IntegrationTests/Users/
                            LoginUserTests.cs
web/src/
  features/auth/            login-form.tsx, map-login-error.ts, pages/login-page.tsx
  types/auth.ts             loginFormSchema
contracts/openapi/          api.v1.yaml (generated)
```

## Authorization, session, and domain notes

- **Session model:** Identical to registration (ADR-001): new `user_sessions` row per successful login, cookie `TradingSimulator.Session`, Redis `session:{id}` with TTL to `expires_at`. Multiple concurrent sessions allowed (spec BR-04 ✅).
- **Route protection:** `POST /api/auth/login` is **anonymous**; `GET /api/wallet` and `GET /api/portfolio` remain protected. `PublicRoute` redirects authenticated users away from `/login` (EC-05).
- **Domain rules (do not violate):**
  - BR-01: Handler must **not** call `User.Register` or create wallet/portfolio rows.
  - BR-08: No `SaveChanges` on wallets, portfolios, holdings, or orders in login handler.
  - BR-09: Post-login reads use existing query handlers (authoritative PostgreSQL).

## Progress tracker

### Task 1: Skeleton — login HTTP pipeline and contracts

| Attribute | Value |
|-----------|--------|
| Spec story | Story 1 |
| Depends on | None |
| Estimated complexity | S |
| Parent story issue | #22 |

#### Objective

`POST /api/auth/login` is routable and returns a predictable placeholder (**401** or **501**) until the handler is implemented; Contracts and MediatR wire-up exist so later tasks only fill behavior.

#### Implementation notes

- Add `LoginUserRequest` / `LoginUserResponse` in Contracts (email, password / userId, username, email).
- Add `LoginUserCommand` + handler stub returning `Error.Unauthorized` or `Result` failure.
- Add `AuthEndpoint` with `.AllowAnonymous()`, `.RequireCompleteJsonBody<LoginUserRequest>()`, OpenAPI metadata (200, 400, 401, 500). Do **not** add `LoginUserCommandValidator` — Story 5.
- Register handler via existing MediatR assembly scan (`Application.AssemblyReference`).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `src/Contracts/Users/LoginUserRequest.cs` | JSON body |
| CREATE | `src/Contracts/Users/LoginUserResponse.cs` | Success body |
| CREATE | `src/Application/Users/Commands/LoginUserCommand.cs` | Command + result type |
| CREATE | `src/Application/Users/Commands/LoginUserCommandHandler.cs` | Stub handler |
| CREATE | `src/Application/Users/LoginErrors.cs` | `INVALID_CREDENTIALS` (moved up from Task 3) |
| CREATE | `src/Api/Endpoints/AuthEndpoint.cs` | Maps `POST /api/auth/login`; binding via `RequireCompleteJsonBody` only |
| CREATE | `tests/Api.IntegrationTests/Users/LoginUserTests.cs` | Smoke: 401 stub + 400 malformed JSON |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `LoginUser_EndpointExists_ReturnsNon404` (optional smoke) | `tests/.../LoginUserTests.cs` |
| Manual | Aspire dashboard — route listed | — |

#### Acceptance criteria

- [x] `POST /api/auth/login` with JSON body returns non-404 (stub status acceptable).
- [x] Endpoint discovered via `MapEndpoints` without manual registration beyond new class.
- [x] Malformed JSON returns **400** via existing `InvalidRequestMiddleware` / `RequireCompleteJsonBody`.

#### Notes (Task 1 complete)

- Stub handler returns **401** `INVALID_CREDENTIALS` (not 501).
- `LoginErrors.cs` added in Task 1 (plan had Task 3) so stub uses shared error constant.
- Integration: `LoginUser_EndpointExists_Returns401_InvalidCredentials`, `LoginUser_MalformedJson_Returns400_INVALID_REQUEST`.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | PRD §5.1 US-02; Tech §8.1, §15.3 |
| Async matching | N/A |
| PostgreSQL authoritative | N/A (stub) |
| Redis projection | N/A |
| RFC 7807 errors | Reuse `Result.ToHttpResult()` |
| SignalR | N/A |
| Aspire | None |
| ADR needed? | No |

#### Risk

None — isolated stub.

---

### Task 2: Email lookup and password verification ports

| Attribute | Value |
|-----------|--------|
| Spec story | Story 1 (infrastructure for credentials) |
| Depends on | Task 1 |
| Estimated complexity | M |
| Parent story issue | #22 |

#### Objective

Application layer can load a user by normalized email and verify a submitted password against the stored hash using the same Identity hasher as registration.

#### Implementation notes

- `IUserRepository.GetByEmailAsync(string normalizedEmail)` → `User?` (include `PasswordHash` via mapper — follow `UserPersistenceMapper` / `UserRecord`).
- `IPasswordHasher.Verify(Password password, PasswordHash storedHash)` → `bool` using `PasswordVerificationResult.Success` / `SuccessRehashNeeded`.
- Normalize email in handler with `EmailAddress.Create(command.Email)` (BR-02 / BR-04 alignment).
- Do not expose whether email exists outside handler (Story 2 tests enumeration; handler returns same error type).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `src/Application/Abstractions/Persistence/IUserRepository.cs` | `GetByEmailAsync` |
| MODIFY | `src/Application/Abstractions/Auth/IPasswordHasher.cs` | `Verify` |
| MODIFY | `src/Infrastructure/Persistence/Repositories/UserRepository.cs` | Query + map to domain `User` |
| MODIFY | `src/Infrastructure/Auth/PasswordHasher.cs` | `Verify` implementation |
| MODIFY | `src/Infrastructure/Persistence/Mapping/UserPersistenceMapper.cs` | `ToUser` record → aggregate |
| MODIFY | `src/Domain/Users/User.cs` | `FromPersistence` factory |
| MODIFY | `src/Domain/Users/Wallet.cs` | `FromPersistence` factory |
| MODIFY | `tests/Api.IntegrationTests/Users/LoginUserTests.cs` | Repository + hasher integration tests |
| MODIFY | `tests/Api.IntegrationTests/Users/Fakes/ThrowOnAddUserRepository.cs` | Stub `GetByEmailAsync` |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Domain | None (no domain change) | — |
| Integration | Optional: unit-test hasher round-trip in existing test project | — |

#### Acceptance criteria

- [x] `Verify` returns true for password used at register time.
- [x] `GetByEmailAsync` returns null for unknown email; returns user with correct `PasswordHash` for registered email.

#### Notes (Task 2 complete)

- `GetByEmailAsync` uses `Include(Wallet)` so `User.FromPersistence` receives a valid wallet (login handler does not use wallet).
- Integration tests: `PasswordHasher_Verify_ReturnsTrue_ForRegisteredPassword`, `UserRepository_GetByEmailAsync_ReturnsNull_WhenUnknown`, `UserRepository_GetByEmailAsync_ReturnsUser_WithPasswordHash_WhenRegistered`.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | Tech §15.2; DB §4.1 |
| Async matching | N/A |
| PostgreSQL authoritative | User row read from PG |
| Redis projection | N/A |
| RFC 7807 errors | N/A |
| SignalR | N/A |
| Aspire | None |
| ADR needed? | No — extends ADR-002 |

#### Risk

Mapping `User` without wallet eager-load is fine — login handler only needs id, username, email, hash.

---

### Task 3: Login command handler — session creation (happy path)

| Attribute | Value |
|-----------|--------|
| Spec story | Story 1 |
| Depends on | Task 2 |
| Estimated complexity | M |
| Parent story issue | #22 |

#### Objective

`LoginUserCommand` authenticates valid email/password, creates a new session, enqueues Redis cache entry, and returns identity — without mutating wallet or portfolio.

#### Implementation notes

- Handler flow: normalize email → `GetByEmailAsync` → if null or `!Verify` → `Error.Unauthorized("INVALID_CREDENTIALS", generic message)` (BR-05 minimal; Story 2 adds tests).
- If `EmailAddress.Create` fails (malformed email), return **`INVALID_CREDENTIALS`** in Story 1 — not **422** (Story 5 owns format validation).
- On success: `sessionStore.CreateSessionAsync(user.Id)` → `pendingSessionCacheCollector.Enqueue(...)` — **mirror** `RegisterUserCommandHandler` session block.
- Return `LoginUserResult` (userId, username, email display).
- No domain events required for MVP login.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `src/Application/Users/Commands/LoginUserCommandHandler.cs` | Full implementation |
| MODIFY | `src/Application/Users/Commands/LoginUserCommand.cs` | `LoginUserResult` includes `SessionId`, `SessionExpiresAt` (for Task 4 cookie) |
| REUSE | `src/Application/Users/LoginErrors.cs` | Already added in Task 1 |
| REUSE | `src/Application/Users/Commands/RegisterUserCommandHandler.cs` | Session + cache pattern |
| MODIFY | `tests/Api.IntegrationTests/Users/LoginUserTests.cs` | MediatR handler tests (moved up from Task 4 deferral) |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `LoginUserCommand_Succeeds_ForRegisteredUser` | `LoginUserTests.cs` |
| Integration | `LoginUserCommand_WrongPassword_ReturnsInvalidCredentials` | `LoginUserTests.cs` |
| Integration | `LoginUserCommand_DoesNotModify_WalletPortfolioOrHoldings` | `LoginUserTests.cs` |

#### Acceptance criteria

- [x] MediatR `Send(LoginUserCommand)` succeeds for user created via register.
- [x] Wrong password returns failure with `INVALID_CREDENTIALS` code (handler level).
- [x] No writes to `wallets`, `portfolios`, `holdings` tables in login transaction.

#### Notes (Task 3 complete)

- Malformed email/password → `INVALID_CREDENTIALS` (not **422** until Story 5).
- `LoginUserResult` carries session fields; `AuthEndpoint` cookie wiring remains Task 4.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | BR-01, BR-08; DB §4.9, §12.1 |
| Async matching | N/A |
| PostgreSQL authoritative | Session row inserted in PG |
| Redis projection | Best-effort cache after commit |
| RFC 7807 errors | `INVALID_CREDENTIALS` → 401 |
| SignalR | N/A |
| Aspire | None |
| ADR needed? | No |

#### Risk

None — mirrors proven register session path.

---

### Task 4: Auth endpoint + integration tests (read-your-writes)

| Attribute | Value |
|-----------|--------|
| Spec story | Story 1 |
| Depends on | Task 3 |
| Estimated complexity | M |
| Parent story issue | #22 |

#### Objective

HTTP login returns **200**, sets session cookie, and subsequent `GET /api/wallet` and `GET /api/portfolio` return the authenticated user's data.

#### Implementation notes

- `AuthEndpoint` on success: `SessionCookieWriter.Append` + `Results.Ok(LoginUserResponse)`.
- Integration test: register user → clear cookie or use second client → login → `GET /api/wallet` asserts `userId` / `username` / balances match registration.
- Second test: after login, `GET /api/portfolio` returns `userId` and holdings array (may be empty).
- Use `WebApplicationFactoryClientOptions { HandleCookies = true }` and Testcontainers fixture (same as `RegisterUserTests`).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `src/Api/Endpoints/AuthEndpoint.cs` | Cookie + 200 body |
| MODIFY | `tests/Api.IntegrationTests/Users/LoginUserTests.cs` | HTTP happy-path + read-your-writes |
| REUSE | `tests/.../RegisterUserTests.cs` | Client/fixture pattern |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `LoginUser_Returns200_AndWalletShowsRegisteredUser` | `LoginUserTests.cs` |
| Integration | `LoginUser_AfterRegister_PortfolioReturnsHoldings` | `LoginUserTests.cs` |

#### Acceptance criteria

- [x] Login with registered credentials returns **200** and `Set-Cookie`.
- [x] `GET /api/wallet` without cookie → **401**; with cookie after login → **200** with correct `userId`.
- [x] `GET /api/portfolio` after login returns same `userId`.
- [x] Both integration tests pass in CI (Docker).

#### Notes (Task 4 complete)

- Login test uses a fresh `HttpClient` (separate cookie jar) after register so the session cookie comes from login, not registration.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | Spec §6 read-your-writes; Tech §17.3 |
| Async matching | N/A |
| PostgreSQL authoritative | Wallet/portfolio from PG |
| Redis projection | N/A for reads |
| RFC 7807 errors | 401 on bad credentials |
| SignalR | N/A |
| Aspire | None |
| ADR needed? | No |

#### Risk

Test must register before login — use unique email per run (Guid suffix).

---

### Task 5: Login UI — form, session store, redirect to `from`

| Attribute | Value |
|-----------|--------|
| Spec story | Story 1 |
| Depends on | Task 4 |
| Estimated complexity | M |
| Parent story issue | #22 |

#### Objective

Login screen collects email and password, calls `POST /api/auth/login`, updates auth store and TanStack Query cache, and navigates to trading or the saved protected-route path.

#### Implementation notes

- `loginFormSchema` in `web/src/types/auth.ts` — **required fields only** (non-empty email/password); no email-format or password-complexity zod rules until Story 5.
- `login-form.tsx` — mirror `register-form.tsx`: RHF + minimal zod, `useMutation`, `submittingRef`, link to Register.
- On success: `setSession` from response; `queryClient.setQueryData(['auth', 'session'], wallet-shaped cache or invalidate `['auth','session']` + wallet/portfolio); `navigate(from ?? paths.trading, { replace: true })` where `from` is `useLocation().state?.from`.
- Update `authApi.login` return type to `LoginUserResponse`.
- Replace placeholder copy on `login-page.tsx` with `LoginForm`.
- **REUSE** `PublicRoute` for EC-05 (no code change expected).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `web/src/features/auth/login-form.tsx` | Form + mutation |
| CREATE | `web/src/features/auth/map-login-error.ts` | Minimal error mapping (generic fallback) |
| MODIFY | `web/src/features/auth/pages/login-page.tsx` | Host form |
| MODIFY | `web/src/features/auth/api.ts` | Typed login response |
| MODIFY | `web/src/types/auth.ts` | `loginFormSchema` |
| REUSE | `web/src/features/auth/register-form.tsx` | UX patterns |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Register → logout N/A — use incognito: login → trading shows username/cash | `web/` |
| Manual | Logged out visit `/trading` → login → returns to `/trading` | `web/` |
| Manual | Logged in visit `/login` → redirect `/trading` | `web/` |

#### Acceptance criteria

- [x] Login form submits to API with `credentials: 'include'`.
- [x] Successful login navigates to trading within 2 s on local stack.
- [x] Trading view shows wallet data for logged-in user (session probe succeeds).
- [x] Protected-route `from` redirect works.

#### Notes (Task 5 complete)

- After login: `fetchQuery(['auth','session'])` via `getWallet`, then invalidate `wallet` / `portfolio` keys.
- `INVALID_CREDENTIALS` / **401** → root alert; **500** → generic retry message (Story 5 adds field-level **422**).
- Manual UI checklist deferred to Task 6.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | PRD §8; `frontend.mdc` |
| Async matching | N/A |
| PostgreSQL authoritative | Via GET after login |
| Redis projection | N/A |
| RFC 7807 errors | Basic mapping only |
| SignalR | N/A |
| Aspire | `VITE_*` unchanged |
| ADR needed? | No |

#### Risk

Without logout (Story 4), manual tests use fresh browser profile or clear cookies via devtools.

---

### Task 6: Polish — OpenAPI, regression, manual checklist

| Attribute | Value |
|-----------|--------|
| Spec story | Story 1 · Infrastructure |
| Depends on | Task 5 |
| Estimated complexity | S |
| Parent story issue | #22 |

#### Objective

Contract is exported and verified; registration tests still pass; Story 1 manual checklist documented and runnable.

#### Implementation notes

- Run `yarn --cwd web api:export` and `api:verify`.
- Confirm `POST /api/auth/login` appears in `contracts/openapi/api.v1.yaml` with 200/401/400 (422 documented in Story 5).
- Run full `Api.IntegrationTests` Users suite + existing register tests.
- Export `LoginForm` from `features/auth/index.ts` if needed.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `contracts/openapi/api.v1.yaml` | Generated |
| MODIFY | `web/src/features/auth/index.ts` | Exports |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | All existing `RegisterUser*` tests green | `tests/.../Users/` |
| Manual | Story 1 checklist below | — |

#### Acceptance criteria

- [x] `yarn --cwd web api:verify` passes.
- [x] No regression in registration integration tests.
- [ ] Manual checklist completed (or documented blockers in `known-issues.md`).

#### Notes (Task 6 complete — automation)

- `contracts/openapi/api.v1.yaml`: `POST /api/auth/login` documents **200**, **400**, **401**, **500** + `LoginUserRequest` / `LoginUserResponse`.
- Regression: **28** Users integration tests green (Testcontainers); **10** login + **18** register (excl. `RegisterUserSessionTests`, which needs local Postgres on :5432 — pre-existing).
- `LoginForm` export already present in `features/auth/index.ts` (Task 5).
- Manual UI checklist (§Manual UI checklist) — **pending operator** on Aspire stack.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | `openapi-contract-sync` skill |
| Async matching | N/A |
| PostgreSQL authoritative | N/A |
| Redis projection | N/A |
| RFC 7807 errors | OpenAPI documents problem types |
| SignalR | N/A |
| Aspire | None |
| ADR needed? | No |

#### Risk

None — verification only.

---

## Reference files

| File | Why open it |
|------|-------------|
| `src/Api/Endpoints/UsersEndpoint.cs` | Cookie issuance + MediatR send pattern |
| `src/Application/Users/Commands/RegisterUserCommandHandler.cs` | Session create + Redis enqueue |
| `src/Api/Auth/SessionCookieWriter.cs` | Cookie options |
| `web/src/features/auth/register-form.tsx` | Form/mutation/submit-guard pattern |
| `web/src/app/routes/protected-route.tsx` | `state.from` for post-login redirect |
| `web/src/app/routes/public-route.tsx` | EC-05 authenticated redirect |
| `tests/Api.IntegrationTests/Users/RegisterUserTests.cs` | Testcontainers + cookie client |
| `docs/plans/20260523-201500-user-registration-story-1.md` | Story 1 registration plan parity |

## Implementation details (for /build)

### Command and handler

- `LoginUserCommand(string Email, string Password)` → `ICommand<LoginUserResult>`.
- `LoginUserResult(Guid UserId, string Username, string Email)`.
- Handler dependencies: `IUserRepository`, `IPasswordHasher`, `ISessionStore`, `IPendingSessionCacheCollector` (no `IUserRepository.AddAsync`, no wallet/portfolio repos).

### Error codes (Story 1 minimum)

| Code | HTTP | When |
|------|------|------|
| `INVALID_CREDENTIALS` | 401 | Unknown email or failed verify |
| `INVALID_REQUEST` | 400 | Incomplete JSON body (`RequireCompleteJsonBody`) |
| `VALIDATION_FAILED` | 422 | **Story 5 only** — FluentValidation + login form format rules |

### API endpoint sketch

- Route: `POST /api/auth/login`
- Tags: `Auth`
- `.AllowAnonymous()`
- Success: **200** + `LoginUserResponse` + `SessionCookieWriter.Append(sessionId, expiresAt, options)`

### Frontend auth flow after login

1. `loginMutation` → `authApi.login`.
2. `setSession({ userId, username })`.
3. `queryClient.invalidateQueries({ queryKey: ['auth', 'session'] })` or prefetch `getWallet`.
4. `navigate(typeof from === 'string' ? from : paths.trading, { replace: true })`.

### Channel / matching

No channel enqueue. No MatchingEngine changes.

## Verification matrix (plan-level)

| Spec AC | Verified by |
|---------|-------------|
| Submit valid credentials → session + navigate trading | Task 5 manual + Task 4 integration |
| Wallet shows my balances | `LoginUser_Returns200_AndWalletShowsRegisteredUser` |
| Portfolio shows my holdings | `LoginUser_AfterRegister_PortfolioReturnsHoldings` |
| Redirect to saved protected route | Task 5 manual |
| Already authenticated on `/login` → trading | Task 6 manual (`PublicRoute`) |

## Rollback / recovery

- **Code:** Revert `feature/user-login-story-1` commits.
- **DB:** No migration — login only inserts session rows (safe to leave or delete test sessions).
- **Redis:** Session keys expire; flush optional for local dev.

## Deferred work (Plan B — epic #21)

- Story 2 (#23): `INVALID_CREDENTIALS` integration tests (unknown email vs wrong password same response), login form error copy, timing-safe path review.
- Story 3 (#24): Session-expired message on 401, reload/tab persistence tests, cookies-disabled messaging.
- Story 4 (#25): `POST /api/auth/logout`, user menu Log out, revoke session + clear cookie.
- Story 5 (#26): Login **422** matrix, double-submit guard, **500**/network retry UX (mirror register Story 4).

## GitHub Links

| Local ref | Issue # | Type | Title | URL |
|-----------|---------|------|-------|-----|
| spec Story 1 | 22 | Story | US-02 / Story 1: Log in and access my portfolio | https://github.com/tranvuongduy2003/trading-simulator/issues/22 |
| epic | 21 | Epic | Spec: User login (US-02) | https://github.com/tranvuongduy2003/trading-simulator/issues/21 |

## Manual UI checklist (Task 6)

1. Start Aspire; open web app logged out.
2. Register `trader_<suffix>@example.com` / `SecurePass1!` (or use existing user).
3. Clear cookies or use private window; open `/login`.
4. Submit same email/password → lands on `/trading`; header shows username; cash matches account.
5. Open holdings/portfolio area → 0 AAPL (or prior trades if applicable).
6. Logged out: navigate to `/trading` → redirected to `/login`; after login → back to `/trading`.
7. While logged in, open `/login` → redirected to `/trading` without second login form.


---

<a id="source-20260525-160000-user-login-story-2md"></a>

## Source 6 of 18: `docs/plans/20260525-160000-user-login-story-2.md`

---
artifact_type: plan
artifact_version: 1
id: plan-20260525-160000-user-login-story-2
title: User Login — Story 2 (Reject invalid credentials safely)
slug: user-login-story-2
filename_template: 20260525-160000-user-login-story-2.md
created_at: 2026-05-25T16:00:00+07:00
updated_at: 2026-05-25T21:00:00+07:00
status: complete
owner: engineering
tags: [plan, implementation, trading-simulator, auth, login, story-2, INVALID_CREDENTIALS, enumeration]
related_spec: docs/specs/20260525-103709-user-login.md
related_plans: [docs/plans/20260525-150000-user-login-story-1.md]
prd_refs: [PRD §5.1 US-02, PRD §6.1 FR-1.2, PRD §7.4]
tech_refs: [Tech §6.2, Tech §8.1, Tech §15.1, Tech §15.2, Tech §15.3, Tech §17.3]
db_refs: [DB §4.1 users, DB §4.9 user_sessions, DB §6.1 ux_users_email, DB §12.1 session cache]
github:
  repo: tranvuongduy2003/trading-simulator
  epic_issue: 21
  story_issue_ids: [23]
  last_synced_at: 2026-05-25T16:00:00+07:00
search_index:
  keywords: [login, INVALID_CREDENTIALS, enumeration, uniform 401, email normalization, wrong password, unknown email, no session cookie, LoginUserCommandHandler, map-login-error, integration test, BR-05]
  bounded_contexts: [Trading]
  task_count: 4
---

# Implementation Plan: User Login — Story 2

| Field | Value |
|-------|--------|
| Spec | `docs/specs/20260525-103709-user-login.md` (§2 Story 2) |
| GitHub story | [#23 — Reject invalid credentials safely](https://github.com/tranvuongduy2003/trading-simulator/issues/23) |
| Epic | [#21 — User login (US-02)](https://github.com/tranvuongduy2003/trading-simulator/issues/21) |
| Depends on | Story 1 — `docs/plans/20260525-150000-user-login-story-1.md` (handler, endpoint, login form) |
| Status | COMPLETE (automation); manual UI checklist pending operator |
| Tasks | 4 |
| Branch | `feature/user-login-story-2` (from `main` after Story 1 merges, or from `feature/user-login-story-1` if still open) |
| Aspire impact | No |
| Schema impact | No |
| Test levels | API integration (Testcontainers) · Manual UI |
| ADRs required | None |
| GitHub | Synced 2026-05-25 — see §GitHub Links |

## Executive summary

Story 2 closes the **security and UX contract** for failed login (US-02): unknown email, wrong password, and malformed identifier must all return the same **401** with `INVALID_CREDENTIALS` and a generic message (BR-05), with **no** session cookie or `user_sessions` row. Email **case normalization** must allow login when the stored address matches after trim/lowercase (BR-02, EC-03). Story 1 already implemented `LoginUserCommandHandler`, `LoginErrors`, `AuthEndpoint`, and client `map-login-error` + password clear on error — this plan **verifies and proves** those behaviors with HTTP-level integration tests and a short manual UI checklist, not a greenfield login feature.

## Goals and non-goals

**Goals**

- G1: HTTP tests assert **401** + `code` `INVALID_CREDENTIALS` + generic `detail` for unknown email and wrong password, with **identical** problem bodies (EC-01, EC-02).
- G2: Failed login emits **no** `Set-Cookie` and does **not** insert into `user_sessions` (spec data impact).
- G3: Login with mixed-case email succeeds when normalized value matches registration (EC-03 / BR-02).
- G4: UI shows "Email or password is incorrect.", preserves email, clears password on failure (spec §4a).
- G5: No regression to Story 1 happy-path tests.

**Non-goals**

- NG1: Story 3 — session expiry / reload messaging.
- NG2: Story 4 — logout.
- NG3: Story 5 — **422** validation matrix, double-submit hardening, **500** retry UX (malformed email still maps to `INVALID_CREDENTIALS` until Story 5).
- NG4: Rate limiting, CAPTCHA, constant-time password compare beyond existing hasher (local MVP).
- NG5: Timing-attack elimination via artificial delays (document as accepted MVP risk).

## Traceability matrix

| Spec story | Plan task(s) | Test evidence |
|------------|--------------|---------------|
| Story 2 — unknown email | Task 2 | `LoginUser_UnknownEmail_Returns401_InvalidCredentials_NoSession` |
| Story 2 — wrong password | Task 2 | `LoginUser_WrongPassword_Returns401_InvalidCredentials_NoSession` |
| Story 2 — uniform response | Task 2 | `LoginUser_UnknownEmail_AndWrongPassword_ReturnSameProblemBody` |
| Story 2 — email casing | Task 2 | `LoginUser_MixedCaseEmail_Returns200_WhenNormalizedMatches` |
| Story 2 — valid login (regression) | Task 4 | Re-run `LoginUser_Returns200_AndWalletShowsRegisteredUser` |
| Story 2 — UI copy / field behavior | Task 3 | Manual checklist (§Manual UI checklist) |
| Stories 3–5 | Deferred — Plan B | — |

## Architecture impact

```text
┌─────────────┐  POST /api/auth/login   ┌──────────────┐   LoginUserCommandHandler
│  login-form │ ─────────────────────► │ AuthEndpoint │ ──► EmailAddress.Create (normalize)
│  map-login- │ ◄── 401 problem+json   │              │     GetByEmailAsync
│  error.ts   │   (no Set-Cookie)      └──────────────┘     Verify OR → INVALID_CREDENTIALS
└─────────────┘                                              (no SessionStore call)
```

| Layer | Change summary |
|-------|----------------|
| Domain | **REUSE** `EmailAddress.Create` (trim + lowercase lookup value) |
| Application | **VERIFY** `LoginUserCommandHandler` — single `LoginErrors.InvalidCredentials` exit for null user, failed verify, invalid email shape |
| Infrastructure | **REUSE** `GetByEmailAsync(normalized)`, `IPasswordHasher.Verify` — no changes unless audit finds gap |
| Api | **VERIFY** `AuthEndpoint` — cookie only on success; **optional** OpenAPI 401 example |
| MatchingEngine | None |
| web/ | **VERIFY** `map-login-error.ts`, `login-form.tsx` `onError` clears password — no structural change expected |
| AppHost | None |

## Data & migration plan

| Artifact | Action | DB reference |
|----------|--------|--------------|
| EF migration | **None** | — |
| `user_sessions` | **No insert** on failed login | DB §4.9 |
| Redis `session:{id}` | **No write** on failed login | DB §12.1 |
| `users` | **Read-only** on login | DB §4.1, §6.1 |

## Open questions

| # | Question | Source | Answer | Status |
|---|----------|--------|--------|--------|
| 1 | Should malformed email return **422** instead of **401**? | Story 5 vs Story 2 | **401** `INVALID_CREDENTIALS` until Story 5 (same as Story 1 plan Q4) | ✅ Answered |
| 2 | Add constant-time delay on failed login? | Security hardening | **No** for local MVP; note in risks | ✅ Answered |
| 3 | Branch from Story 1 PR or `main`? | Git workflow | Prefer `feature/user-login-story-2` from merged `main`; if Story 1 PR open, stack on `feature/user-login-story-1` | ⏳ Operator choice |

## Risks and mitigations

| Risk | Likelihood | Impact | Mitigation | Owner task |
|------|------------|--------|------------|------------|
| Story 1 not merged — duplicate work on handler | M | L | Reuse Story 1 branch; only add tests/helpers in Story 2 | Prereq |
| Timing difference unknown vs wrong password enables enumeration | L | M | Same code path after email parse; parity test compares full JSON body; defer artificial delay | Task 2 |
| Test asserts cookie absence but client still has stale cookie | L | L | Use fresh `HttpClient` per scenario (`CreateClient()` pattern) | Task 2 |
| `LoginUser_EndpointExists_Returns401` only checks status — false confidence | M | M | Replace/strengthen in Task 2 with problem `code` assertion | Task 2 |

## Prerequisites

- [x] Story 1 implementation available (`POST /api/auth/login`, handler, `LoginUserTests` happy path)
- [x] `dotnet test tests/Api.IntegrationTests` passes for existing login tests (Docker / Testcontainers)
- [ ] Aspire local stack optional for manual UI only (Task 3 manual checklist)

## File structure (planned)

```text
tests/Api.IntegrationTests/Users/
  LoginUserTests.cs              MODIFY — Story 2 HTTP tests
  LoginUserTestHelpers.cs        CREATE — AssertInvalidCredentialsAsync (mirror RegisterUserTestHelpers)
  RegisterUserTestHelpers.cs     REUSE — JsonOptions, ApiProblemDetails patterns

src/Application/Users/Commands/
  LoginUserCommandHandler.cs     VERIFY — no change unless audit fails

web/src/features/auth/
  map-login-error.ts             VERIFY
  login-form.tsx                 VERIFY

contracts/openapi/api.v1.yaml    MODIFY (optional) — 401 example documentation
```

## Authorization, session, and domain notes

- **BR-02:** `EmailAddress.Create` trims and lowercases for lookup; `DisplayValue` preserves trimmed input for response.
- **BR-03:** Verification uses `IPasswordHasher.Verify` only — never compare plaintext to stored string.
- **BR-05:** One `Error.Unauthorized("INVALID_CREDENTIALS", "Email or password is incorrect.")` for all credential failures — **do not** add `USER_NOT_FOUND` or distinct messages.
- **Session model:** `SessionStore.CreateSessionAsync` runs only after successful verify; failed path must not call it or `PendingSessionCacheCollector.Enqueue`.
- **Logs:** If adding `LoginFailed` logs (spec §8), omit email in message or use hashed identifier — optional in Task 4 polish.

## Progress tracker

### Task 1: Audit handler and add test helpers

| Attribute | Value |
|-----------|--------|
| Spec story | Story 2 |
| Depends on | Story 1 complete |
| Estimated complexity | S |
| Parent story issue | #23 |

#### Objective

Confirm `LoginUserCommandHandler` uses one failure outcome for unknown user, bad password, and invalid email shape; add reusable integration helpers to assert RFC 7807 `INVALID_CREDENTIALS` at HTTP layer.

#### Implementation notes

- Read handler: failure paths must all return `LoginErrors.InvalidCredentials` before any `sessionStore` call.
- **CREATE** `LoginUserTestHelpers.AssertInvalidCredentialsAsync` — expect **401**, `application/problem+json`, `code` == `INVALID_CREDENTIALS`, `detail` contains generic copy (match `LoginErrors` message).
- **REUSE** `RegisterUserTestHelpers.JsonOptions` or share `JsonSerializerOptions` pattern.
- If audit finds `SessionStore` invoked on failure, fix in handler (should not happen in current code).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `tests/Api.IntegrationTests/Users/LoginUserTestHelpers.cs` | HTTP problem assertions |
| VERIFY | `src/Application/Users/Commands/LoginUserCommandHandler.cs` | BR-05 single exit |
| VERIFY | `src/Application/Users/LoginErrors.cs` | Stable code + message |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | Existing `LoginUserCommand_WrongPassword_ReturnsInvalidCredentials` still passes | `LoginUserTests.cs` |

#### Acceptance criteria

- [x] Handler audit documented in PR description (no separate code change if already correct)
- [x] `AssertInvalidCredentialsAsync` compiles and is used in Task 2

#### Notes (Task 1 completion)

- **Handler audit (2026-05-25):** `LoginUserCommandHandler` returns `LoginErrors.InvalidCredentials` for invalid email/password shape (catch), null user, and failed verify; `sessionStore` / `pendingSessionCacheCollector` only after successful verify — no code change.
- **Tests:** `LoginUserTestHelpers.AssertInvalidCredentialsAsync` added; `LoginUser_EndpointExists_Returns401_InvalidCredentials` now uses helper (isolated `HttpClient`).

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | PRD §5.1 US-02; Tech §15; DB §4.9 read-only on failure |
| RFC 7807 errors | `code` on `ApiProblemDetails` |
| ADR needed? | No |

#### Risk

None — read-only audit unless bug found.

---

### Task 2: Story 2 HTTP integration tests (enumeration safety)

| Attribute | Value |
|-----------|--------|
| Spec story | Story 2 |
| Depends on | Task 1 |
| Estimated complexity | M |
| Parent story issue | #23 |

#### Objective

Prove acceptance criteria at HTTP boundary: uniform 401 problem body, no session cookie, no new `user_sessions` row, mixed-case email success.

#### Implementation notes

- **Unknown email:** `POST /api/auth/login` with `unknown_{guid}@example.com` + any password → assert via helper; `response.Headers` must not contain `Set-Cookie`; `GET /api/wallet` on same client → **401**.
- **Wrong password:** Register user, login with wrong password → same assertions; capture problem JSON.
- **Parity:** Compare `detail`, `code`, `status`, `title` (and `type` if stable) between unknown-email and wrong-password responses — must match.
- **No session row:** Before/after failed login, `UserSessions.CountAsync()` for that `user_id` unchanged (wrong-password case); for unknown email, total session count unchanged vs snapshot before attempt.
- **Email casing:** Register with `jane_{suffix}@example.com`, login with `Jane@Example.COM` + correct password → **200**, `Set-Cookie` present.
- **Strengthen** `LoginUser_EndpointExists_Returns401_InvalidCredentials` to use helper (assert `code`, not only status).
- Use isolated `HttpClient` per test (`CreateClient()`).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `tests/Api.IntegrationTests/Users/LoginUserTests.cs` | Story 2 tests |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `LoginUser_UnknownEmail_Returns401_InvalidCredentials_NoSession` | `LoginUserTests.cs` |
| Integration | `LoginUser_WrongPassword_Returns401_InvalidCredentials_NoSession` | `LoginUserTests.cs` |
| Integration | `LoginUser_UnknownEmail_AndWrongPassword_ReturnSameProblemBody` | `LoginUserTests.cs` |
| Integration | `LoginUser_MixedCaseEmail_Returns200_WhenNormalizedMatches` | `LoginUserTests.cs` |
| Integration | `LoginUser_FailedLogin_DoesNotInsert_UserSession` | `LoginUserTests.cs` |

#### Acceptance criteria

- [x] All five tests pass under Testcontainers
- [x] Problem `code` is `INVALID_CREDENTIALS` for both failure scenarios
- [x] No `Set-Cookie` on failure responses
- [x] Mixed-case email login returns **200**

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PostgreSQL authoritative | Session rows only on success |
| Redis projection | No enqueue on failure |
| RFC 7807 errors | Full body parity |

#### Risk

Flaky tests if sharing one `HttpClient` with cookies — use per-test clients.

#### Notes (Task 2 completion)

- Five tests added to `LoginUserTests.cs`; mixed-case login uses `Jane_{suffix}@Example.COM` against registered `jane_{suffix}@example.com`.
- `LoginUser_FailedLogin_DoesNotInsert_UserSession` covers global session count (unknown email) and per-user count (wrong password).
- **15** `LoginUser*` integration tests green (Testcontainers).

---

### Task 3: Verify login UI error behavior

| Attribute | Value |
|-----------|--------|
| Spec story | Story 2 |
| Depends on | Task 2 (API contract proven) |
| Estimated complexity | S |
| Parent story issue | #23 |

#### Objective

Confirm the login form matches spec §4a for `INVALID_CREDENTIALS`: generic alert, email preserved, password cleared, submit re-enabled after error.

#### Implementation notes

- **VERIFY** `applyLoginApiError` maps `INVALID_CREDENTIALS` and generic **401** to `loginInvalidCredentialsMessage`.
- **VERIFY** `login-form.tsx` `onError` calls `form.setValue('password', '')` before `applyLoginApiError`.
- **No change required** if verification passes — document in PR.
- If gap: align copy exactly with API `detail` string (optional consistency).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| VERIFY | `web/src/features/auth/map-login-error.ts` | Error mapping |
| VERIFY | `web/src/features/auth/login-form.tsx` | Password clear + root alert |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Story 2 UI checklist (below) | `web/` |

#### Acceptance criteria

- [ ] Manual checklist completed on Aspire (`web` + `api`) — **operator** before merge (steps below)
- [x] Wrong password shows root destructive alert with spec copy (code audit 2026-05-25)
- [x] Email field still populated after failure (code audit 2026-05-25)

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| design-system | Destructive `Alert` for auth errors |

#### Risk

None — verification-only unless copy mismatch.

#### Notes (Task 3 completion)

- **No web code changes** — Story 1 UI already matches spec §4a.
- `map-login-error.ts`: `applyLoginApiError` maps `problem.code === 'INVALID_CREDENTIALS'` **or** `status === 401` → `loginInvalidCredentialsMessage` (`Email or password is incorrect.`); matches API `LoginErrors` detail.
- `login-form.tsx`: `onError` calls `form.setValue('password', '')` **before** `applyLoginApiError`; email field untouched; root error uses `<Alert variant="destructive">`; submit re-enabled via `onSettled` + `disabled={loginMutation.isPending}` only while pending.
- `yarn --cwd web lint` and `yarn --cwd web build` green (2026-05-25).

#### Manual UI checklist

1. Register `story2_{suffix}@example.com` / `SecurePass1!`.
2. Log out or use private window → open `/login`.
3. Submit wrong password → alert "Email or password is incorrect."; email unchanged; password field empty.
4. Submit `unknown@example.com` / any password → same alert text (indistinguishable).
5. Submit correct mixed-case email `Story2_{suffix}@Example.COM` → navigates to trading view.

---

### Task 4: OpenAPI polish and regression

| Attribute | Value |
|-----------|--------|
| Spec story | Story 2 · Polish |
| Depends on | Tasks 2–3 |
| Estimated complexity | S |
| Parent story issue | #23 |

#### Objective

Keep contract and CI green; re-run full Users integration suite; close Story 2 with changelog/memory updates.

#### Implementation notes

- Run `yarn --cwd web api:export` if `AuthEndpoint` metadata changed (likely unchanged).
- Optional: add OpenAPI `example` on **401** for login documenting `INVALID_CREDENTIALS` (if export supports — otherwise rely on `ProducesProblem` only).
- Run `dotnet test tests/Api.IntegrationTests --filter "FullyQualifiedName~LoginUser"` then full `Users` collection.
- Update `docs/memory/current-status.md` and close checklist on issue #23 when merging.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| VERIFY/MODIFY | `contracts/openapi/api.v1.yaml` | Contract drift |
| MODIFY | `docs/CHANGELOG.md` | Plan + impl entries |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | All `LoginUser*` + Story 1 register regression | `tests/Api.IntegrationTests/Users/` |
| CI | `yarn --cwd web api:verify` | — |

#### Acceptance criteria

- [x] `api:verify` passes
- [x] Users Testcontainers suite green (**33** tests, excl. `RegisterUserSessionTests`)
- [x] Story 1 happy-path login tests still pass (`LoginUser_Returns200_*`, register flows)

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| openapi-contract-sync | Export + verify |

#### Risk

None.

#### Notes (Task 4 completion)

- `AuthEndpoint` unchanged in Story 2 — `yarn --cwd web api:verify` in sync; no YAML commit.
- Optional OpenAPI **401** `INVALID_CREDENTIALS` example skipped (export does not emit `code` examples; `ProducesProblem(401)` already documented).
- Regression filter: `FullyQualifiedName~TradingSimulator.Api.IntegrationTests.Users&FullyQualifiedName!~RegisterUserSessionTests`.

---

## Reference files

| File | Why open it |
|------|-------------|
| `src/Application/Users/Commands/LoginUserCommandHandler.cs` | BR-05 failure paths |
| `src/Application/Users/LoginErrors.cs` | Canonical code + message |
| `src/Api/Mapping/ResultHttpExtensions.cs` | Problem mapping |
| `tests/Api.IntegrationTests/Users/RegisterUserTestHelpers.cs` | Assertion patterns |
| `tests/Api.IntegrationTests/Users/LoginUserTests.cs` | Extend tests |
| `web/src/features/auth/login-form.tsx` | Password clear on error |
| `docs/plans/20260525-150000-user-login-story-1.md` | Story 1 baseline |

## Implementation details (for /build)

- **Failure contract:** `LoginErrors.InvalidCredentials` → `Error.Unauthorized` → `ToHttpResult` → **401** `ApiProblemDetails` with `Code = "INVALID_CREDENTIALS"`, `Detail` = "Email or password is incorrect."
- **Do not** return **404** for unknown email.
- **Email lookup:** Always `userRepository.GetByEmailAsync(EmailAddress.Create(command.Email).Value)`.
- **Session side effects:** Only after `passwordHasher.Verify` succeeds.
- **Test helper sketch:**

```csharp
public static async Task<ApiProblemDetails> AssertInvalidCredentialsAsync(HttpResponseMessage response)
{
    // 401, deserialize ApiProblemDetails, code == INVALID_CREDENTIALS
    // assert no Set-Cookie header
}
```

- **Parity test:** Serialize both problem bodies (or compare `code`, `status`, `detail`, `title`) — ignore `instance`/`traceId` if present.

## Verification matrix (plan-level)

| Spec AC | Verified by |
|---------|-------------|
| Unknown email → 401 INVALID_CREDENTIALS, no cookie | Task 2 `LoginUser_UnknownEmail_*` |
| Wrong password → same as unknown | Task 2 parity + wrong-password test |
| Mixed-case email succeeds | Task 2 `LoginUser_MixedCaseEmail_*` |
| No session row on failure | Task 2 session count test |
| UI generic message, preserve email, clear password | Task 3 manual checklist |
| Valid login still 200 + cookie | Task 4 regression |

## Rollback / recovery

- **Code:** Revert branch commits (tests only if handler unchanged).
- **DB:** N/A — no migration.
- **Redis:** N/A.

## Deferred work (Plan B)

- Story 3 (#24): session expiry UX and reload messaging.
- Story 4 (#25): `POST /api/auth/logout`.
- Story 5 (#26): **422** validation, double-submit, **500** retry on login form.
- Constant-time login failure delay and rate limiting (post-MVP).

## GitHub Links

| Local ref | Issue # | Type | Title | URL |
|-----------|---------|------|-------|-----|
| spec.Story 2 | 23 | Story | US-02 / Story 2: Reject invalid credentials safely | https://github.com/tranvuongduy2003/trading-simulator/issues/23 |
| epic | 21 | Epic | Spec: User login (US-02) | https://github.com/tranvuongduy2003/trading-simulator/issues/21 |

> **Plan tasks** (Task 1–4) are tracked in this file only. Comment on #23 when `/build` starts or completes tasks.


---

<a id="source-20260525-170000-user-login-story-3md"></a>

## Source 7 of 18: `docs/plans/20260525-170000-user-login-story-3.md`

---
artifact_type: plan
artifact_version: 1
id: plan-20260525-170000-user-login-story-3
title: User Login — Story 3 (Session persists until logout or expiry)
slug: user-login-story-3
filename_template: 20260525-170000-user-login-story-3.md
created_at: 2026-05-25T17:00:00+07:00
updated_at: 2026-05-25T17:00:00+07:00
status: complete
owner: engineering
tags: [plan, implementation, trading-simulator, auth, login, story-3, session, persistence, expiry, cookies]
related_spec: docs/specs/20260525-103709-user-login.md
related_plans: [docs/plans/20260525-150000-user-login-story-1.md, docs/plans/20260525-160000-user-login-story-2.md]
prd_refs: [PRD §5.1 US-02, PRD §6.1 FR-1.2, PRD §7.4, PRD §10.1]
tech_refs: [Tech §8.1, Tech §15.1, Tech §15.3, Tech §16.2, Tech §17.3]
db_refs: [DB §4.9 user_sessions, DB §6.8 user_sessions, DB §12.1 session cache, DB §12.2]
github:
  repo: tranvuongduy2003/trading-simulator
  epic_issue: 21
  story_issue_ids: [24]
  last_synced_at: 2026-05-25T17:00:00+07:00
search_index:
  keywords: [session persistence, reload, cookie, GET wallet, ProtectedRoute, PublicRoute, useSession, expires_at, revoked_at, UNAUTHORIZED, session expired, cookies disabled, SessionStore, Redis session cache, TestClock, BR-04, BR-07, BR-10, EC-04, EC-06]
  bounded_contexts: [Trading]
  task_count: 5
---

# Implementation Plan: User Login — Story 3

| Field | Value |
|-------|--------|
| Spec | `docs/specs/20260525-103709-user-login.md` (§2 Story 3) |
| GitHub story | [#24 — Session persists until logout or expiry](https://github.com/tranvuongduy2003/trading-simulator/issues/24) |
| Epic | [#21 — User login (US-02)](https://github.com/tranvuongduy2003/trading-simulator/issues/21) |
| Depends on | Stories 1–2 — `docs/plans/20260525-150000-user-login-story-1.md`, `docs/plans/20260525-160000-user-login-story-2.md` (login endpoint, cookie, `useSession`, route guards) |
| Status | COMPLETE (automation); manual UI checklist pending operator |
| Tasks | 5 |
| Branch | `feature/user-login-story-3` |
| Aspire impact | No |
| Schema impact | No |
| Test levels | API integration (Testcontainers) · Manual UI |
| ADRs required | None (extends ADR-001) |
| GitHub | Synced 2026-05-25 — see §GitHub Links |

## Executive summary

Story 3 completes **session continuity** for US-02: after login, the HttpOnly session cookie must keep the user authenticated across reloads and short absences within the configured **24 h** lifetime (BR-04), while **expired** or **revoked** sessions must return **401** on protected APIs with a clear client-side session-expired flow (EC-04). Stories 1–2 already delivered `POST /api/auth/login`, `SessionAuthenticationHandler`, `SessionStore`, Redis `session:{id}` cache, `GET /api/wallet` as probe, and `ProtectedRoute` / `PublicRoute` with `useSession`. This plan **proves** persistence with integration tests, **hardens** session resolution so PostgreSQL authority is not bypassed on Redis hits (BR-10), and adds **frontend** messaging for session expiry and cookies-disabled login (spec §4a).

## Goals and non-goals

**Goals**

- G1: After login, a second `GET /api/wallet` on the same cookie jar returns **200** (reload / same-origin tab simulation — EC-06).
- G2: Session still valid within `Session:ExpirationHours` after a short simulated absence (TestClock unchanged — still **200**).
- G3: Expired (`expires_at` in the past) or revoked (`revoked_at` set) sessions return **401** `UNAUTHORIZED` on `GET /api/wallet`; no false authentication.
- G4: Client clears auth on **401** when previously authenticated, shows **"Your session has expired. Please log in again."** on login, and redirects guests from protected routes (EC-04).
- G5: Login form detects `navigator.cookieEnabled === false`, shows cookies-required copy, and does not leave the user in a false `authenticated` state.
- G6: No regression to Story 1–2 login tests.

**Non-goals**

- NG1: Story 4 — `POST /api/auth/logout`, user-menu logout, cookie clear on server revoke (revoked sessions tested via direct DB update only).
- NG2: Story 5 — login **422** matrix, double-submit, **500** retry UX.
- NG3: "Remember me" beyond default cookie lifetime, session list, revoke-all-sessions.
- NG4: SignalR reconnect policy changes on session loss.
- NG5: Background job to delete expired rows (`ix_user_sessions_expires` cleanup deferred).

## Traceability matrix

| Spec story | Plan task(s) | Test evidence |
|------------|--------------|---------------|
| Story 3 — reload stays authenticated | Task 1 | `SessionPersistence_AfterLogin_SecondWalletRequest_Returns200` |
| Story 3 — short absence within 24 h | Task 1 | `SessionPersistence_WithinLifetime_Returns200` (TestClock) |
| Story 3 — expired session | Task 2 | `SessionPersistence_ExpiredSession_Returns401` |
| Story 3 — revoked session | Task 2 | `SessionPersistence_RevokedSession_Returns401` |
| Story 3 — Redis cache miss (BR-10) | Task 2 | `SessionPersistence_RedisUnavailable_StillAuthenticates` (reuse pattern) |
| Story 3 — session-expired UI | Task 3 | Manual: 401 on trading → login with expired message |
| Story 3 — cookies disabled | Task 4 | Manual: disable cookies → login shows requirement message |
| Story 3 — protected route guard | Task 3 | Manual + reuse `ProtectedRoute` / `useSession` |
| Stories 1–2 regression | Task 5 | Full `LoginUserTests` + `SessionPersistenceTests` green |
| Story 4–5 | Deferred — Plan B | — |

## Architecture impact

```text
┌──────────────┐  GET /api/wallet (credentials)   ┌─────────────────────────┐
│ useSession   │ ─────────────────────────────► │ SessionAuthentication   │
│ ProtectedRoute│ ◄── 200 / 401 ─────────────── │ Handler → SessionStore  │
└──────────────┘                                 │  PG: expires_at/revoked │
       │ 401 + authenticated                     │  Redis: session:{id}    │
       ▼                                         └─────────────────────────┘
 clearSession + login redirect w/ reason
```

| Layer | Change summary |
|-------|----------------|
| Domain | **None** |
| Application | **None** — reuse `TradingSessionOptions.ExpirationHours` |
| Infrastructure | **MODIFY** `SessionStore.ResolveUserIdAsync` — PostgreSQL must validate active session even when Redis returns `userId` (BR-10 / revoked edge case) |
| Api | **VERIFY** `WalletEndpoint` + `RequireAuthorization()`; **401** via auth middleware when `AuthenticateResult.Fail` |
| MatchingEngine | None |
| web/ | **MODIFY** login page / form (session-expired + cookies messages); **MODIFY** `ProtectedRoute` or `UnauthorizedListener` for redirect reason; **VERIFY** `useSession`, `PublicRoute` |
| AppHost | None |

## Data & migration plan

| Artifact | Action | DB reference |
|----------|--------|--------------|
| EF migration | **None** | — |
| `user_sessions.expires_at` | **Read** on every resolve; tests advance via `TestClock` or direct update | DB §4.9 |
| `user_sessions.revoked_at` | **Read** on every resolve; tests set via EF (logout API deferred) | DB §4.9 |
| Redis `session:{session_id}` | **TTL** aligned to `expires_at`; optional **delete** on failed PG validation | DB §12.1, §12.2 |
| Book recovery | N/A | — |

## Open questions

| # | Question | Source | Answer | Status |
|---|----------|--------|--------|--------|
| 1 | On Redis hit, skip PG or always validate? | Code review (BR-10) | **Always validate** session row in PostgreSQL (`revoked_at`, `expires_at`) when resolving; Redis is optimization only — delete stale Redis key on mismatch | ✅ Answered |
| 2 | Use `TestClock` vs raw SQL for expiry tests? | Test design | **`TestClock`** singleton replace in factory `configureTestServices`; advance `UtcNow` past `expires_at` | ✅ Answered |
| 3 | Navigate to login on `api:unauthorized` globally? | UX | **Yes** when `status === 'authenticated'` — `navigate(paths.login, { state: { reason: 'session-expired' } })` after `clearSession` | ✅ Answered |
| 4 | Branch base? | Git | `feature/user-login-story-3` from `main` after Story 2 merges (or stack on `feature/user-login-story-2` if still open) | ⏳ Operator choice |

## Risks and mitigations

| Risk | Likelihood | Impact | Mitigation | Owner task |
|------|------------|--------|------------|------------|
| Redis hit bypasses revoked session until TTL | H | H | Harden `SessionStore` PG validation on every resolve | Task 2 |
| Login **200** without cookie leaves client "authenticated" | M | M | `useSession` wallet probe + cookies-disabled guard before `setSession` optimism | Task 4 |
| `UnauthorizedListener` clears cache during bootstrap **401** | L | M | Keep `status !== 'authenticated'` guard (existing) | Task 3 |
| TestClock not registered as same instance for `SessionStore` | M | M | Register `TestClock` as singleton `IClock` + expose instance to test | Task 2 |
| Revoked test flaky if Redis not invalidated | M | H | PG validation + delete Redis key when session invalid | Task 2 |

## Prerequisites

- [ ] Stories 1–2 merged or available on branch (`POST /api/auth/login`, login tests green)
- [ ] Docker for Testcontainers (`Api.IntegrationTests`)
- [ ] `aspire run` or equivalent for manual UI checklist

## File structure (planned)

```text
src/
  Infrastructure/Auth/SessionStore.cs          MODIFY
tests/
  Api.IntegrationTests/Users/
    SessionPersistenceTests.cs                 CREATE
    SessionPersistenceTestHelpers.cs           CREATE (optional shared login + clock)
  Testing.Common/Fixtures/TestClock.cs         REUSE
web/src/
  app/providers.tsx                            MODIFY (navigate on unauthorized)
  app/routes/protected-route.tsx               MODIFY (optional redirect reason)
  features/auth/pages/login-page.tsx           MODIFY (expired alert)
  features/auth/login-form.tsx                 MODIFY (cookies-disabled)
  store/auth-store.ts                          MODIFY (optional sessionExpired flag)
  types/auth.ts                                MODIFY (location state type)
```

## Authorization, session, and domain notes

- **Session model:** Cookie `TradingSimulator.Session` → `SessionAuthenticationHandler` → `ISessionStore.ResolveUserIdAsync` → `ICurrentUserAccessor` (ADR-001).
- **Route protection:** `RequireAuthorization()` on wallet/portfolio/orders; `ProtectedRoute` blocks UI until `useSession` sets `authenticated`.
- **Domain rules (must not violate):**
  - **BR-04:** `expires_at = UtcNow + ExpirationHours` on create (unchanged).
  - **BR-07:** Protected APIs reject missing/invalid session (**401**).
  - **BR-10:** PostgreSQL authoritative; Redis rebuildable — **invalid Redis entries must not grant access**.
- **Logout (BR-06):** Deferred to Story 4 — revoked tests use direct `revoked_at` update.

## Progress tracker

### Task 1: Prove session persistence (reload simulation)

| Attribute | Value |
|-----------|--------|
| Spec story | Story 3 |
| Depends on | None (Stories 1–2 code on branch) |
| Estimated complexity | S |
| Parent story issue | #24 |

#### Objective

Add integration tests that demonstrate a logged-in session survives a **second** authenticated request (same `HttpClient` cookie jar = browser reload / same-origin tab). Assert wallet probe returns the same user without re-login.

#### Implementation notes

- New `SessionPersistenceTests` in `tests/Api.IntegrationTests/Users/`.
- Reuse `IntegrationTestFixture`, `HandleCookies = true`, register → login → `GET /api/wallet` twice (or login only if register already sets cookie).
- Optional: second `HttpClient` with copied `Cookie` header to simulate parallel tab — not required if same-jar test is green.
- **Short absence:** factory with `TestClock` at T0; login; assert wallet **200** at T0 + 1 hour (within 24 h default).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `tests/Api.IntegrationTests/Users/SessionPersistenceTests.cs` | Persistence + within-lifetime tests |
| REUSE | `tests/Api.IntegrationTests/Users/LoginUserTests.cs` | Login/register patterns |
| REUSE | `tests/Testing.Common/Fixtures/IntegrationTestFixture.cs` | Testcontainers |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `SessionPersistence_AfterLogin_SecondWalletRequest_Returns200` | `SessionPersistenceTests.cs` |
| Integration | `SessionPersistence_WithinLifetime_Returns200` | `SessionPersistenceTests.cs` |

#### Acceptance criteria

- [x] Second `GET /api/wallet` after login returns **200** with same `userId`.
- [x] Wallet still **200** when `TestClock` advanced by 1 h (not past expiry).
- [x] Tests pass with `dotnet test tests/Api.IntegrationTests`.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | PRD §5.1 US-02; Tech §15.1; DB §4.9 |
| PostgreSQL authoritative | Session row exists |
| Redis projection | Populated after login |
| Aspire | None |
| ADR needed? | No |

#### Risk

None — test-only slice.

---

### Task 2: Harden session resolution and test expiry / revoke

| Attribute | Value |
|-----------|--------|
| Spec story | Story 3 |
| Depends on | Task 1 |
| Estimated complexity | M |
| Parent story issue | #24 |

#### Objective

Ensure **expired** and **revoked** sessions cannot authenticate, including when Redis still holds `session:{id}`. Cover EC-04 at the HTTP layer.

#### Implementation notes

- **SessionStore change:** After Redis returns a `userId`, load `user_sessions` by `sessionId` (or always query PG first — team choice; prefer **single PG check** with same predicates as today: `RevokedAt == null`, `ExpiresAt > clock.UtcNow`). If invalid, attempt Redis key delete and return `null`.
- Register `TestClock` in test factory: `services.RemoveAll<IClock>(); var clock = new TestClock(); services.AddSingleton<IClock>(clock); services.AddSingleton(clock);`
- **Expired test:** login at T0; set `clock.UtcNow = expiresAt + TimeSpan.FromMinutes(1)`; `GET /api/wallet` → **401**.
- **Revoked test:** login; set `revoked_at = UtcNow` on session row via `ApplicationDatabaseContext`; `GET /api/wallet` → **401** even if Redis key exists.
- **Redis unavailable:** reuse `ThrowingSessionRedisCache` or `ConfigureThrowingSessionRedisCache` from `RegisterUserTransientFailureTests` — wallet still **200** when PG valid (EC-10 / BR-10).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `src/Infrastructure/Auth/SessionStore.cs` | PG validation on resolve; optional Redis invalidation |
| CREATE | `tests/Api.IntegrationTests/Users/SessionPersistenceTests.cs` | Add expiry/revoke/redis tests |
| REUSE | `tests/Testing.Common/Fixtures/TestClock.cs` | Controllable time |
| REUSE | `tests/Api.IntegrationTests/Users/Fakes/ThrowingSessionRedisCache.cs` | Redis write failure |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `SessionPersistence_ExpiredSession_Returns401` | `SessionPersistenceTests.cs` |
| Integration | `SessionPersistence_RevokedSession_Returns401` | `SessionPersistenceTests.cs` |
| Integration | `SessionPersistence_RedisCacheWriteFails_StillAuthenticatesViaPostgres` | `SessionPersistenceTests.cs` |

#### Acceptance criteria

- [x] Expired session returns **401** on `/api/wallet`.
- [x] Revoked session returns **401** even when Redis key present (proves PG authority).
- [x] Redis cache write failure does not block authentication while session valid in PG.
- [x] Story 1–2 login tests still pass.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | BR-04, BR-07, BR-10; DB §12.2 |
| Redis projection | Delete stale key on invalid PG row |
| RFC 7807 errors | **401** on unauthorized wallet |
| ADR needed? | No — clarifies ADR-001 |

#### Risk

SessionStore change touches all protected routes — keep diff minimal and test-heavy.

---

### Task 3: Session-expired UX and protected-route redirect

| Attribute | Value |
|-----------|--------|
| Spec story | Story 3 |
| Depends on | Task 2 |
| Estimated complexity | M |
| Parent story issue | #24 |

#### Objective

When an authenticated user receives **401** (expired/revoked session), the client clears auth, navigates to login with a **session-expired** message, and protected routes do not show trading UI as guest-with-stale-state.

#### Implementation notes

- Extend login location state: `{ from?: string; reason?: 'session-expired' }`.
- `LoginPage` or `LoginForm`: if `reason === 'session-expired'`, show `Alert` — *"Your session has expired. Please log in again."* (spec §4a).
- `UnauthorizedListener`: when clearing authenticated session, `router.navigate(paths.login, { replace: true, state: { reason: 'session-expired' } })` — import router from `router.tsx` or use `window.location` only if necessary; prefer React Router `useNavigate` in a small inner component.
- `ProtectedRoute`: when redirecting unauthenticated user, preserve `from` pathname; do not set `authenticated` on failed probe (`useSession` already clears on **401**).
- Ensure `api:unauthorized` does not fire spurious redirects during initial bootstrap (keep existing guard).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `web/src/app/providers.tsx` | Navigate to login with reason on 401 |
| MODIFY | `web/src/features/auth/pages/login-page.tsx` | Session-expired alert |
| MODIFY | `web/src/features/auth/login-form.tsx` | Read location state (if alert lives in form) |
| REUSE | `web/src/hooks/use-session.ts` | Probe + clearSession on 401 |
| REUSE | `web/src/app/routes/protected-route.tsx` | Guest redirect |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Expire session (DevTools → delete cookie or wait) / force 401 → login message | `web/` |
| Manual | Open `/trading` with valid cookie → loads; without cookie → login | `web/` |

#### Acceptance criteria

- [x] **401** while authenticated shows session-expired copy on login screen.
- [x] User lands on login, not trading, after session loss.
- [x] `yarn --cwd web build` succeeds.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech | PRD §7.4; Tech §15.3 |
| SignalR | Hub may disconnect on 401 — no change required MVP |
| Aspire | None |

#### Risk

Double navigation if both `ProtectedRoute` and listener redirect — use `replace: true` and single listener path.

---

### Task 4: Cookies-disabled login guard

| Attribute | Value |
|-----------|--------|
| Spec story | Story 3 |
| Depends on | Task 1 |
| Estimated complexity | S |
| Parent story issue | #24 |

#### Objective

If the browser blocks cookies, login shows a clear **cookies required** message and never presents a false authenticated state.

#### Implementation notes

- At start of `onSubmit` (or before `loginMutation.mutate`), if `typeof navigator !== 'undefined' && navigator.cookieEnabled === false`, set root error: *"Cookies are required to stay signed in. Enable cookies for this site and try again."*
- Do not call `setSession` until wallet probe succeeds (already true in `onSuccess` flow); on login **200** without working cookie, wallet probe fails → `useSession` stays unauthenticated.
- Optional: after login **200**, if `getWallet` fails with **401**, show cookies hint instead of generic error.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `web/src/features/auth/login-form.tsx` | `navigator.cookieEnabled` guard |
| REUSE | `web/src/hooks/use-session.ts` | Probe failure handling |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Disable cookies in browser → submit login → message, no trading access | `web/` |

#### Acceptance criteria

- [x] Cookies disabled → root error message; no navigation to trading.
- [x] No `authenticated` status without successful wallet probe.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD | US-02 edge path |
| ADR needed? | No |

#### Risk

None — isolated UI guard.

---

### Task 5: Polish, regression, and manual sign-off

| Attribute | Value |
|-----------|--------|
| Spec story | Infrastructure / Polish |
| Depends on | Tasks 1–4 |
| Estimated complexity | S |
| Parent story issue | #24 |

#### Objective

Run full auth integration suite, document manual checklist results, and confirm no regression to Stories 1–2.

#### Implementation notes

- `dotnet test tests/Api.IntegrationTests --filter "FullyQualifiedName~Users"` (or whole project).
- `yarn --cwd web lint` and `build`.
- Update `docs/memory/current-status.md` after `/build` completes (not in `/plan`).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| REUSE | `tests/Api.IntegrationTests/Users/LoginUserTests.cs` | Regression |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | All Users tests green | CI-local |
| Manual | Checklist below | — |

#### Acceptance criteria

- [x] All integration tests in Users suite pass (38 Testcontainers tests; excludes `RegisterUserSessionTests` — local Postgres :5432).
- [ ] Manual UI checklist signed off (operator) — see §Manual UI checklist.
- [x] No OpenAPI drift — `yarn --cwd web api:verify` green (2026-05-25).

#### Verification notes (Task 5 /build)

```text
dotnet test ... --filter "FullyQualifiedName~Users&FullyQualifiedName!~RegisterUserSessionTests" → 38 passed
yarn --cwd web lint → 0 errors
yarn --cwd web build → success
yarn --cwd web api:verify → in sync
```

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| Epic #21 | Story 3 AC complete; Story 4 still open |

#### Risk

None.

---

## Manual UI checklist

| # | Step | Expected |
|---|------|----------|
| 1 | Log in on Aspire → open `/trading` | Trading view loads |
| 2 | Hard refresh (F5) | Still on trading; wallet data loads |
| 3 | Open new tab, same origin → `/trading` | Authenticated without login form |
| 4 | DevTools → Application → delete session cookie → refresh | Redirect to login; session-expired message |
| 5 | Settings → disable cookies → attempt login | Cookies-required message; stay on login |
| 6 | Log in again (cookies on) | Trading view restored |

## Reference files

| File | Why open it |
|------|-------------|
| `src/Infrastructure/Auth/SessionStore.cs` | Resolve path + Task 2 hardening |
| `src/Api/Auth/SessionAuthenticationHandler.cs` | Cookie → user id |
| `src/Api/Endpoints/WalletEndpoint.cs` | Session probe contract |
| `web/src/hooks/use-session.ts` | Bootstrap auth state |
| `web/src/app/providers.tsx` | `api:unauthorized` listener |
| `tests/Api.IntegrationTests/Users/RegisterUserTransientFailureTests.cs` | Redis failure + wallet pattern |
| `docs/plans/20260525-150000-user-login-story-1.md` | Deferred session UX note (now implemented) |

## Implementation details (for /build)

- **401 body:** Unauthenticated wallet uses ASP.NET authorization failure → problem+json with `UNAUTHORIZED` (see `ExceptionHandlingMiddleware` / challenge flow). Tests assert `HttpStatusCode.Unauthorized` (and optional `code` if challenge returns problem body).
- **TestClock wiring:** `IntegrationTestFixture.CreateFactory(services => { ... TestClock ... })` per test class or shared helper `CreateFactoryWithTestClock(out TestClock clock)`.
- **Revoke without logout API:** `await db.UserSessions.Where(s => s.Id == sessionId).ExecuteUpdateAsync(s => s.SetProperty(x => x.RevokedAt, clock.UtcNow))` (EF7+) or load entity and save.
- **Router navigate from provider:** Extract `UnauthorizedNavigationHandler` child under `RouterProvider` with `useNavigate`, or pass `router.navigate` from `createBrowserRouter` instance if already exported.
- **Login success without cookies:** Sequence is POST login **200** → `setSession` in mutation → `fetchQuery` wallet **401** → `useSession` effect may run `clearSession` — ensure login form does not `navigate` to trading before wallet probe completes (Story 1 already awaits `fetchQuery` in `onSuccess`).

## Verification matrix (plan-level)

| Spec AC | Verified by |
|---------|-------------|
| Reload / new tab authenticated | Task 1 integration + Manual #2–3 |
| Short absence within 24 h | Task 1 TestClock + Manual #1 |
| Expired/revoked → 401 | Task 2 integration + Manual #4 |
| Client clear + session-expired message | Task 3 manual |
| Cookies disabled message | Task 4 manual |
| BR-07 protected routes | Task 1–3 + existing `RequireAuthorization` |
| BR-10 Redis + PG | Task 2 integration |

## Rollback / recovery

- **Code:** Revert `feature/user-login-story-3` commits.
- **DB:** N/A — no migration.
- **Redis:** Flush; sessions reload from PostgreSQL on next request.

## Deferred work (Plan B)

- Story 4: `POST /api/auth/logout`, Redis key delete on revoke, user menu Log out.
- Story 5: Login validation **422**, double-submit, network retry UX.
- Background cleanup job for expired `user_sessions` rows.
- Optional: `last_seen_at` updates on each request (not in spec).

## GitHub Links

| Local ref | Issue # | Type | Title | URL |
|-----------|---------|------|-------|-----|
| spec.Story 3 | 24 | Story | US-02 / Story 3: Session persists until logout or expiry | https://github.com/tranvuongduy2003/trading-simulator/issues/24 |
| epic | 21 | Epic | Spec: User login (US-02) | https://github.com/tranvuongduy2003/trading-simulator/issues/21 |


---

<a id="source-20260525-180000-user-login-story-4md"></a>

## Source 8 of 18: `docs/plans/20260525-180000-user-login-story-4.md`

---
artifact_type: plan
artifact_version: 1
id: plan-20260525-180000-user-login-story-4
title: User Login — Story 4 (Log out when done)
slug: user-login-story-4
filename_template: 20260525-180000-user-login-story-4.md
created_at: 2026-05-25T18:00:00+07:00
updated_at: 2026-05-25T20:00:00+07:00
status: complete
owner: engineering
tags: [plan, implementation, trading-simulator, auth, login, story-4, logout, session, revoke, cookie]
related_spec: docs/specs/20260525-103709-user-login.md
related_plans: [docs/plans/20260525-150000-user-login-story-1.md, docs/plans/20260525-160000-user-login-story-2.md, docs/plans/20260525-170000-user-login-story-3.md]
prd_refs: [PRD §5.1 US-02, PRD §6.1 FR-1.2, PRD §7.4, PRD §10.1]
tech_refs: [Tech §8.1, Tech §15.1, Tech §15.3, Tech §16.2, Tech §17.3]
db_refs: [DB §4.9 user_sessions, DB §6.8 user_sessions, DB §12.1 session cache]
github:
  repo: tranvuongduy2003/trading-simulator
  epic_issue: 21
  story_issue_ids: [25]
  last_synced_at: 2026-05-25T18:00:00+07:00
search_index:
  keywords: [logout, POST auth logout, revoked_at, session cookie clear, Redis session delete, user menu, clearSession, ProtectedRoute, BR-06, EC-07, ISessionStore, LogoutUserCommand, 204, UNAUTHORIZED]
  bounded_contexts: [Trading]
  task_count: 5
---

# Implementation Plan: User Login — Story 4

| Field | Value |
|-------|--------|
| Spec | `docs/specs/20260525-103709-user-login.md` (§2 Story 4) |
| GitHub story | [#25 — Log out when I am done](https://github.com/tranvuongduy2003/trading-simulator/issues/25) |
| Epic | [#21 — User login (US-02)](https://github.com/tranvuongduy2003/trading-simulator/issues/21) |
| Depends on | Stories 1–3 — login endpoint, session cookie, `SessionStore`, `ProtectedRoute`, `useSession`, persistence tests |
| Status | COMPLETE (automation) |
| Tasks | 5 |
| Branch | `feature/user-login-story-4` |
| Aspire impact | No |
| Schema impact | No |
| Test levels | API integration (Testcontainers) · Manual UI |
| ADRs required | None (extends ADR-001 session model) |
| GitHub | Synced 2026-05-25 — see §GitHub Links |

## Executive summary

Story 4 delivers **explicit logout** for US-02: an authenticated user can end their session so others on the same device cannot trade as them. The server must set `user_sessions.revoked_at`, delete the Redis `session:{id}` key (BR-06), clear the HttpOnly session cookie, and the client must clear local auth state and return to login within **2 s**. Stories 1–3 already provide login, cookie auth, `GET /api/wallet` as probe, and revoked-session **401** behavior (tested via direct DB update). This plan adds **`POST /api/auth/logout`**, extends **`ISessionStore`** with revoke, wires the **user menu** in `AppLayout`, and proves **EC-07** (stale tab / back button cannot use wallet after logout).

## Goals and non-goals

**Goals**

- G1: `POST /api/auth/logout` with valid session returns **204**, clears session cookie, sets `revoked_at`, deletes Redis session key.
- G2: After logout, `GET /api/wallet` returns **401**; UI `ProtectedRoute` redirects to login (EC-07).
- G3: Unauthenticated `POST /api/auth/logout` returns **401** `UNAUTHORIZED` without leaking session state (spec failure path).
- G4: User menu **Log out** in app shell calls API, clears Zustand + TanStack Query auth-related keys, navigates to `/login`.
- G5: No regression to Stories 1–3 (`LoginUserTests`, `SessionPersistenceTests`).

**Non-goals**

- NG1: Story 5 — login **422** matrix, double-submit login, **500** retry UX.
- NG2: Revoke-all-sessions, session list UI, password reset.
- NG3: SignalR disconnect policy on logout (client may drop connection naturally; no hub changes).
- NG4: Domain aggregate changes — session revoke is infrastructure/application concern only.

## Traceability matrix

| Spec story | Plan task(s) | Test evidence |
|------------|--------------|---------------|
| Story 4 — happy path logout | Task 1, 3 | `LogoutUser_Returns204_AndClearsCookie` |
| Story 4 — wallet 401 after logout | Task 1 | `LogoutUser_AfterLogout_WalletReturns401` |
| Story 4 — UI redirect to login | Task 3, 4 | Manual: Log out → login screen; `/trading` → login |
| Story 4 — already logged out API | Task 2 | `LogoutUser_WithoutSession_Returns401` |
| Story 4 — BR-06 Redis + PG | Task 1, 2 | Integration asserts `revoked_at` + Redis key absent |
| Story 4 — EC-07 back button | Task 4 | Manual: logout → browser back → no wallet data / redirect |
| Stories 1–3 regression | Task 5 | Full Users integration suite + `api:verify` |

## Architecture impact

```text
┌─────────────────┐  POST /api/auth/logout   ┌──────────────────────────┐
│ UserMenu        │ ───────────────────────► │ AuthEndpoint             │
│ useLogout()     │ ◄── 204 + Set-Cookie     │  → LogoutUserCommand     │
│ clearSession    │     (delete cookie)      │  → SessionStore.Revoke   │
└─────────────────┘                          │  → PG revoked_at         │
       │ navigate /login                     │  → Redis DEL session:{id}│
       ▼                                     └──────────────────────────┘
 ProtectedRoute + useSession → 401 on stale tab
```

| Layer | Change summary |
|-------|----------------|
| Domain | **None** |
| Application | **CREATE** `LogoutUserCommand` + handler; **MODIFY** `ISessionStore` — `RevokeSessionAsync` |
| Infrastructure | **MODIFY** `SessionStore` — update `revoked_at`, `DeleteAsync` Redis key |
| Api | **MODIFY** `AuthEndpoint` — map `POST /api/auth/logout`; **MODIFY** `SessionCookieWriter` — `Delete`; **RequireAuthorization** |
| MatchingEngine | None |
| web/ | **MODIFY** `app-layout.tsx` — user menu + Log out; **CREATE** `use-logout` mutation hook; **REUSE** `authApi.logout` |
| AppHost | None |

## Data & migration plan

| Artifact | Action | DB reference |
|----------|--------|--------------|
| EF migration | **None** | — |
| `user_sessions.revoked_at` | **UPDATE** on logout (`WHERE id = @sessionId`) | DB §4.9 |
| Redis `session:{session_id}` | **DELETE** on logout | DB §12.1 |
| Book recovery | N/A | — |

## Open questions

| # | Question | Source | Answer | Status |
|---|----------|--------|--------|--------|
| 1 | Logout without session: **204** or **401**? | Spec §2 Story 4 | **401** via `RequireAuthorization()` — matches spec API “authenticated”; client clears local state on any logout attempt | ✅ Answered |
| 2 | Pass `sessionId` via claim vs read cookie in endpoint? | Clean Architecture | Read cookie in **Api** endpoint, pass `LogoutUserCommand(sessionId, userId)` — Application stays HTTP-free | ✅ Answered |
| 3 | Revoke when session already revoked? | Idempotency | `ExecuteUpdate` only when `revoked_at IS NULL`; still **204** + clear cookie | ✅ Answered |
| 4 | Structured log event for logout? | Spec §8 | Optional `UserLoggedOut` info log with `userId` + `sessionId` (no password) | ⏳ Deferred to Task 5 if time |

## Risks and mitigations

| Risk | Likelihood | Impact | Mitigation | Owner task |
|------|------------|--------|------------|------------|
| Cookie cleared but PG revoke fails → session still valid | L | H | UoW on command; integration test asserts `revoked_at` before cookie-only check | Task 1 |
| Client navigates away before API completes | M | M | `useMutation` await logout; disable menu item while pending | Task 3 |
| Stale tab shows cached wallet in React Query | M | M | `queryClient.clear()` or remove `wallet`/`portfolio`/`auth` keys on logout | Task 3 |
| OpenAPI drift | L | M | Task 5 `api:export` + `api:verify` | Task 5 |

## Prerequisites

- [x] Stories 1–3 implemented (login, session persistence, route guards)
- [x] Branch `feature/user-login-story-4` from latest `main` (or story-3 branch if not merged)
- [ ] Aspire local stack runs; Docker for integration tests
- [ ] Spec Story 4 AC reviewed (issue #25)

## File structure (planned)

```text
src/
  Application/
    Abstractions/Auth/ISessionStore.cs          MODIFY
    Users/Commands/LogoutUserCommand.cs         CREATE
    Users/Commands/LogoutUserCommandHandler.cs  CREATE
  Infrastructure/Auth/SessionStore.cs         MODIFY
  Api/
    Endpoints/AuthEndpoint.cs                   MODIFY
    Auth/SessionCookieWriter.cs                 MODIFY
tests/
  Api.IntegrationTests/Users/LogoutUserTests.cs CREATE
web/src/
  layouts/app-layout.tsx                        MODIFY
  features/auth/use-logout.ts                   CREATE
  features/auth/index.ts                        MODIFY (export)
contracts/openapi/api.v1.yaml                   MODIFY (via export)
```

## Authorization, session, and domain notes

- **Session model:** Logout revokes **only the current session** (BR-04 MVP: multiple sessions allowed). Cookie carries `session_id`; handler must not revoke other users’ sessions.
- **Route protection:** `POST /api/auth/logout` uses same `Session` scheme as wallet; **401** when cookie missing/invalid/expired/revoked.
- **BR-06:** Order of operations in handler: (1) PostgreSQL `revoked_at`, (2) Redis delete, (3) commit via UoW; Api clears cookie after successful command.
- **BR-07 / BR-08:** Logout does not touch wallet, portfolio, or orders.
- **Anti-forgery:** Existing `RequestVerificationToken` header on POST from `apiClient` — no change.

## Progress tracker

### Task 1: Logout API skeleton — command, revoke store, 204 + cookie clear

| Attribute | Value |
|-----------|--------|
| Spec story | Story 4 |
| Depends on | None (Stories 1–3 merged or available) |
| Estimated complexity | M |
| Parent story issue | #25 |

#### Objective

Authenticated client can call `POST /api/auth/logout` and receive **204** with session cookie removed; PostgreSQL row has `revoked_at` set; subsequent `GET /api/wallet` on same cookie jar returns **401**.

#### Implementation notes

- Add `Task RevokeSessionAsync(Guid sessionId, CancellationToken)` to `ISessionStore`.
- `SessionStore.RevokeSessionAsync`: `ExecuteUpdateAsync` on `UserSessions` where `Id == sessionId && RevokedAt == null`; then `cacheService.DeleteAsync(session:{id})` (ignore missing key).
- `LogoutUserCommand(Guid SessionId, Guid UserId)` — handler calls `RevokeSessionAsync`; no domain aggregate load.
- `SessionCookieWriter.Delete(HttpContext, TradingSessionOptions)` — append expired cookie (`MaxAge = 0` or `Expires` in past), same `Path`/`SameSite`/`HttpOnly` as append.
- `AuthEndpoint`: `MapPost("/api/auth/logout", LogoutUser).RequireAuthorization().WithName("LogoutUser")...Produces(204).ProducesProblem(401)`.
- Handler reads `sessionId` from cookie in endpoint (parse `Guid`), `userId` from `ICurrentUserAccessor`; reject mismatch if needed (optional defense — session row’s `user_id` should match).
- Register handler via existing MediatR assembly scan.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `src/Application/Abstractions/Auth/ISessionStore.cs` | `RevokeSessionAsync` |
| CREATE | `src/Application/Users/Commands/LogoutUserCommand.cs` | Command record |
| CREATE | `src/Application/Users/Commands/LogoutUserCommandHandler.cs` | Revoke orchestration |
| MODIFY | `src/Infrastructure/Auth/SessionStore.cs` | PG + Redis revoke |
| MODIFY | `src/Api/Auth/SessionCookieWriter.cs` | `Delete` |
| MODIFY | `src/Api/Endpoints/AuthEndpoint.cs` | Logout route |
| CREATE | `tests/Api.IntegrationTests/Users/LogoutUserTests.cs` | Happy path |
| REUSE | `tests/.../LoginUserTests.cs` | Client + register/login pattern |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `LogoutUser_Returns204_AndClearsCookie` | `LogoutUserTests.cs` |
| Integration | `LogoutUser_AfterLogout_WalletReturns401` | `LogoutUserTests.cs` |

#### Acceptance criteria

- [x] Login → logout → **204**; `Set-Cookie` deletes session cookie (empty or expired).
- [x] `user_sessions.revoked_at` not null for session id used at login.
- [x] Same client: `GET /api/wallet` after logout → **401**.
- [x] Both integration tests pass with Testcontainers.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | BR-06, DB §4.9, §12.1; Tech §15.3 |
| Async matching | N/A |
| PostgreSQL authoritative | Revoke in PG first |
| Redis projection | Delete `session:{id}` |
| RFC 7807 errors | 401 unauthenticated |
| SignalR | N/A |
| Aspire | None |
| ADR needed? | No |

#### Risk

Revoke must commit before cookie clear; keep revoke inside UoW command pipeline.

---

### Task 2: Logout edge cases — unauthenticated, idempotent revoke, Redis assertion

| Attribute | Value |
|-----------|--------|
| Spec story | Story 4 |
| Depends on | Task 1 |
| Estimated complexity | S |
| Parent story issue | #25 |

#### Objective

Unauthenticated logout returns **401** without error leakage; double revoke / second logout is safe; Redis session key is gone after logout.

#### Implementation notes

- `LogoutUser_WithoutSession_Returns401`: fresh `HttpClient`, `POST /api/auth/logout` with no cookie → **401**.
- `LogoutUser_WhenAlreadyRevoked_Returns204`: login → logout → logout again: second call may be **401** (no valid auth) — assert no **500** and no sensitive body; document chosen behavior in test name.
- Optional: `LogoutUser_RemovesRedisSessionKey` — resolve `ICacheService` or Redis from factory, assert key missing after logout (reuse pattern from `SessionPersistenceTests` if present).
- Idempotent PG: second revoke on same id from handler path should not throw.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `tests/Api.IntegrationTests/Users/LogoutUserTests.cs` | Edge cases |
| REUSE | `tests/.../SessionPersistenceTests.cs` | Revoke/Redis helpers |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `LogoutUser_WithoutSession_Returns401` | `LogoutUserTests.cs` |
| Integration | `LogoutUser_SecondCallAfterLogout_Returns401` | `LogoutUserTests.cs` |
| Integration | `LogoutUser_RemovesRedisSessionKey` (if feasible) | `LogoutUserTests.cs` |

#### Acceptance criteria

- [x] No cookie → logout **401**.
- [x] After successful logout, repeat logout from same jar → **401** (cookie cleared).
- [x] No **500** on edge paths.
- [x] Redis key absent after logout (if test added).

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | Spec failure path; BR-06 |
| Async matching | N/A |
| PostgreSQL authoritative | Yes |
| Redis projection | Delete verified |
| RFC 7807 errors | Generic 401 body |
| SignalR | N/A |
| Aspire | None |
| ADR needed? | No |

#### Risk

None — isolated tests.

---

### Task 3: User menu — Log out control and client mutation

| Attribute | Value |
|-----------|--------|
| Spec story | Story 4 |
| Depends on | Task 1 |
| Estimated complexity | M |
| Parent story issue | #25 |

#### Objective

Authenticated shell shows username (or avatar fallback) with **Log out**; clicking runs `authApi.logout`, clears auth store, invalidates session queries, navigates to login within **2 s**.

#### Implementation notes

- `useLogout` — `useMutation` calling `authApi.logout` with `credentials: 'include'` (default in client).
- `onSuccess` / `onSettled`: `clearSession()`, `queryClient.removeQueries({ queryKey: ['wallet'] })`, `['portfolio']`, `['auth', 'session']` (or `resetQueries`).
- `navigate(paths.login, { replace: true })` — no `from` state on voluntary logout.
- `AppLayout`: right side of header — `DropdownMenu` with `useAuthStore` username + `DropdownMenuItem` “Log out” (destructive variant optional); show only when `status === 'authenticated'` (layout is under `ProtectedRoute`, so safe).
- Disable menu item while `isPending`; show subtle loading if needed.
- Export hook from `features/auth/index.ts`.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `web/src/features/auth/use-logout.ts` | Mutation + cleanup |
| MODIFY | `web/src/layouts/app-layout.tsx` | User menu UI |
| MODIFY | `web/src/features/auth/index.ts` | Export |
| REUSE | `web/src/components/ui/dropdown-menu.tsx` | shadcn menu |
| REUSE | `web/src/store/auth-store.ts` | `clearSession` |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Login → trading → user menu → Log out → login page | `web/` |
| Manual | After logout, nav to `/trading` → redirect login | `web/` |

#### Acceptance criteria

- [x] User menu visible on trading (and other app routes using `AppLayout`).
- [x] Log out completes API call and lands on login.
- [x] `authStatus` is `unauthenticated` after logout.
- [ ] Logout within 2 s on local Aspire. *(manual on Aspire)*

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | PRD §8 top bar user menu |
| Async matching | N/A |
| PostgreSQL authoritative | Via API |
| Redis projection | N/A |
| RFC 7807 errors | Toast optional on failure; still clear local session? **No** — only clear on success; on **401** still navigate login (already logged out) |
| SignalR | N/A |
| Aspire | None |
| VITE_* | Unchanged |
| ADR needed? | No |

#### Risk

On network failure, user may think they are logged out — show brief error toast, keep session until success (prefer explicit failure message).

---

### Task 4: EC-07 and protected-route hardening after logout

| Attribute | Value |
|-----------|--------|
| Spec story | Story 4 |
| Depends on | Task 3 |
| Estimated complexity | S |
| Parent story issue | #25 |

#### Objective

After logout, browser back button or stale tab cannot present authenticated trading UI with prior user’s wallet data (EC-07).

#### Implementation notes

- Ensure `useSession` does not repopulate `authenticated` from stale React Query cache after `clearSession` — Task 3 query removal should suffice; verify `useSession` `enabled` tied to `authStatus`.
- **Done (Task 4):** `useSession` uses `enabled: authStatus !== 'unauthenticated'`; effect skips `setSession` when status is `unauthenticated` or wallet data is absent.
- **Done (Task 4):** `ProtectedRoute` redirects `unauthenticated` users to login **before** session probe loading (no flash of protected shell); `replace` without `from` after voluntary logout / back (EC-07).
- Optional: `useEffect` on `AppLayout` mount when `unauthenticated` inside protected tree should not occur — already gated by `ProtectedRoute`.
- Manual EC-07: login → trading → logout → browser **Back** → expect redirect to login or error state without USD balances from prior user.
- Manual: second tab still open on trading → after logout in tab A, tab B refresh → **401** / login redirect.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `web/src/hooks/use-session.ts` | Verify enabled/guard if needed |
| MODIFY | `web/src/app/routes/protected-route.tsx` | Verify redirect when `unauthenticated` |
| REUSE | `web/src/app/providers.tsx` | Global 401 handler consistency |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | EC-07 back button after logout | `web/` |
| Manual | Two-tab logout isolation | `web/` |

#### Acceptance criteria

- [x] Back button after logout does not show prior user wallet as authenticated. *(code: session probe disabled + immediate redirect; confirm on Aspire)*
- [x] `GET /api/wallet` from devtools after logout returns **401**. *(API Task 1–2; manual devtools on Aspire)*
- [x] Protected routes redirect to login when `authStatus !== 'authenticated'`.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | EC-07 |
| Async matching | N/A |
| PostgreSQL authoritative | Revoked session |
| Redis projection | N/A |
| RFC 7807 errors | 401 on probe |
| SignalR | N/A |
| Aspire | Manual |
| ADR needed? | No |

#### Risk

None — verification task.

---

### Task 5: Polish — OpenAPI, regression, manual sign-off

| Attribute | Value |
|-----------|--------|
| Spec story | Story 4 · Infrastructure |
| Depends on | Tasks 1–4 |
| Estimated complexity | S |
| Parent story issue | #25 |

#### Objective

OpenAPI documents `POST /api/auth/logout`; full Users integration suite green; Story 4 manual checklist complete.

#### Implementation notes

- `yarn --cwd web api:export` and `api:verify`.
- Run `dotnet test tests/TradingSimulator.Api.IntegrationTests` (Users tests).
- Optional structured log: `UserLoggedOut` with `userId`, `sessionId`.
- Update issue #25 checkboxes when manual steps done.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `contracts/openapi/api.v1.yaml` | Via export |
| MODIFY | `tests/Api.IntegrationTests/Users/LogoutUserTests.cs` | Final cleanup |
| REUSE | `.github/workflows/ci.yml` | Already runs api:verify |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | Full Users suite | CI |
| Contract | `yarn --cwd web api:verify` | — |
| Manual | Story 4 checklist below | — |

#### Acceptance criteria

- [x] OpenAPI includes `/api/auth/logout` POST **204** / **401**.
- [x] All integration tests pass. *(43/44 Users Testcontainers; `RegisterUserSessionTests` needs local Postgres — known)*
- [ ] Manual checklist signed off. *(operator on Aspire — plan §Manual UI checklist)*

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | Spec DoD |
| Async matching | N/A |
| PostgreSQL authoritative | Yes |
| Redis projection | Yes |
| RFC 7807 errors | Documented |
| SignalR | N/A |
| Aspire | Smoke logout |
| ADR needed? | No |

#### Risk

None.

---

## Manual UI checklist (operator)

1. Register or login as user A → open `/trading` → confirm wallet shows user A.
2. User menu → **Log out** → land on `/login` within 2 s.
3. Navigate to `/trading` → redirected to login.
4. Login as user B → wallet shows B (not A).
5. Log out → browser **Back** → must not show A’s authenticated trading balances (EC-07).
6. DevTools: `POST /api/auth/logout` without cookie → **401**.

## Reference files

| File | Why open it |
|------|-------------|
| `src/Api/Endpoints/AuthEndpoint.cs` | Login pattern to mirror for logout |
| `src/Infrastructure/Auth/SessionStore.cs` | Resolve + revoke + Redis keys |
| `src/Api/Auth/SessionCookieWriter.cs` | Cookie append/delete symmetry |
| `tests/Api.IntegrationTests/Users/LoginUserTests.cs` | HTTP client cookie jar |
| `tests/Api.IntegrationTests/Users/SessionPersistenceTests.cs` | Revoke helper, Redis failure pattern |
| `web/src/layouts/app-layout.tsx` | Header shell for user menu |
| `web/src/features/auth/api.ts` | `logout()` already defined |
| `web/src/hooks/use-session.ts` | Session probe behavior |
| `docs/plans/20260525-170000-user-login-story-3.md` | Prior story patterns |

## Implementation details (for /build)

- **Command:** `LogoutUserCommand(Guid SessionId, Guid UserId)` → `ICommand` (no response body); handler returns `Result` success.
- **Revoke SQL (EF):** `ExecuteUpdateAsync` set `RevokedAt = clock.UtcNow` where `Id == sessionId && RevokedAt == null`.
- **Redis:** `DeleteAsync($"session:{sessionId:D}")` — same key format as `SessionStore.CreateSessionAsync` / `TryWriteCacheAsync`.
- **Cookie delete:** Match `CookieName`, `Path=/`, `HttpOnly`, `SameSite=Lax`, `Secure` when HTTPS; `MaxAge = TimeSpan.Zero`.
- **Endpoint auth:** `.RequireAuthorization()` on logout — unauthenticated → **401** challenge (spec-allowed).
- **OpenAPI:** `.WithName("LogoutUser").WithTags("Auth").Produces(StatusCodes.Status204NoContent).ProducesProblem(401)`.
- **Frontend:** `useLogout` → on success: `clearSession()`, strip query keys, `navigate(paths.login, { replace: true })`.
- **Do not** add `LogoutUser` to domain aggregates or modify wallet/portfolio tables.

## Verification matrix (plan-level)

| Spec AC | Verified by |
|---------|-------------|
| Authenticated → Log out → revoke + cookie + login nav | Task 1 + Task 3 manual |
| After logout `GET /api/wallet` → 401 | Task 1 integration |
| After logout `/trading` → login | Task 3–4 manual |
| Already logged out → POST logout → 401 safe | Task 2 integration |
| EC-07 back button | Task 4 manual |
| BR-06 | Task 1–2 PG + Redis |
| Stories 1–3 regression | Task 5 |

## Rollback / recovery

- **Code:** Revert `feature/user-login-story-4` commits.
- **DB:** N/A — no migration; revoked rows remain revoked (acceptable).
- **Redis:** Flush; invalid sessions already rejected by PG resolve.

## Deferred work (Plan B)

- Story 5 (#26): login validation **422**, double-submit, network retry UX.
- Revoke-all-sessions for user.
- `UserLoggedOut` metrics (`logouts_total`).
- Background job: purge expired `user_sessions` via `ix_user_sessions_expires`.

## GitHub Links

| Local ref | Issue # | Type | Title | URL |
|-----------|---------|------|-------|-----|
| spec.Story 4 | 25 | Story | US-02 / Story 4: Log out when I am done | https://github.com/tranvuongduy2003/trading-simulator/issues/25 |
| epic | 21 | Epic | Spec: User login (US-02) | https://github.com/tranvuongduy2003/trading-simulator/issues/21 |


---

<a id="source-20260525-190000-user-login-story-5md"></a>

## Source 9 of 18: `docs/plans/20260525-190000-user-login-story-5.md`

---
artifact_type: plan
artifact_version: 1
id: plan-20260525-190000-user-login-story-5
title: User Login — Story 5 (Validate input and transient failures)
slug: user-login-story-5
filename_template: 20260525-190000-user-login-story-5.md
created_at: 2026-05-25T19:00:00+07:00
updated_at: 2026-05-25T22:30:00+07:00
status: complete
owner: engineering
tags: [plan, implementation, trading-simulator, auth, login, story-5, validation, transient, double-submit]
related_spec: docs/specs/20260525-103709-user-login.md
related_plans: [docs/plans/20260525-150000-user-login-story-1.md, docs/plans/20260525-160000-user-login-story-2.md, docs/plans/20260525-170000-user-login-story-3.md, docs/plans/20260525-180000-user-login-story-4.md]
prd_refs: [PRD §5.1 US-02, PRD §6.1 FR-1.2, PRD §7.4]
tech_refs: [Tech §6.2, Tech §8.1, Tech §15.2, Tech §15.3, Tech §17.3]
db_refs: [DB §4.9 user_sessions, DB §12.1 session cache]
github:
  repo: tranvuongduy2003/trading-simulator
  epic_issue: 21
  story_issue_ids: [26]
  last_synced_at: 2026-05-25T19:00:00+07:00
search_index:
  keywords: [login, VALIDATION_FAILED, INVALID_REQUEST, INTERNAL_ERROR, FluentValidation, LoginUserCommandValidator, double-submit, EC-08, EC-09, EC-11, zod, map-login-error, transient retry, BR-02]
  bounded_contexts: [Trading]
  task_count: 5
---

# Implementation Plan: User Login — Story 5

| Field | Value |
|-------|--------|
| Spec | `docs/specs/20260525-103709-user-login.md` (§2 Story 5) |
| GitHub story | [#26 — Validate login input and transient failures](https://github.com/tranvuongduy2003/trading-simulator/issues/26) |
| Epic | [#21 — User login (US-02)](https://github.com/tranvuongduy2003/trading-simulator/issues/21) |
| Depends on | Stories 1–4 — login endpoint, session cookie, invalid-credentials UX, logout, `login-form` submit guard shell |
| Status | COMPLETE |
| Tasks | 5 |
| Branch | `feature/user-login-story-5` |
| Aspire impact | No |
| Schema impact | No |
| Test levels | API integration (Testcontainers) · Manual UI |
| ADRs required | None |
| GitHub | Synced 2026-05-25 — see §GitHub Links |

## Executive summary

Story 5 closes the **input validation and ambiguous-failure** gap for US-02 login. Stories 1–4 already ship `POST /api/auth/login`, session cookies, invalid-credentials handling, persistence, logout, and a client `submittingRef` guard — but invalid email/password currently fall through `EmailAddress.Create` / `Password.Create` in the handler and surface as **401** `INVALID_CREDENTIALS` instead of **422** `VALIDATION_FAILED`. Login OpenAPI omits **422**, integration tests lack the validation/transient matrix, and `map-login-error.ts` does not map field errors. This plan adds **FluentValidation** for login (BR-02 email format, password required), adjusts the handler to verify credentials without registration-grade password rules, mirrors **registration Story 3/4** tests and UX (422/400/500, double-submit, retry message), and documents **422** in OpenAPI.

## Goals and non-goals

**Goals**

- G1: `not-an-email` → **422** `VALIDATION_FAILED` with `errors.email`; no `user_sessions` row (BR-02).
- G2: Empty password → **422** with `errors.password`; no session.
- G3: Malformed JSON / incomplete body → **400** `INVALID_REQUEST` (already partially covered — keep and extend tests).
- G4: Simulated persistence failure on login → **500** `INTERNAL_ERROR`; no session cookie; session row count unchanged (EC-11).
- G5: **500** / network errors on the client → generic retry message; submit re-enables; passwords cleared on error; `suppressErrorToast` on login POST (already set).
- G6: Double-submit / parallel login → client guard prevents duplicate in-flight requests; integration test proves wallet probe succeeds and no auth stuck state (EC-08); single intentional submit creates exactly one new session row (happy path).
- G7: EC-09 — after login **200** + cookie, a follow-up wallet probe succeeds even if the client previously showed a transient error path (existing `onSuccess` wallet fetch — verify with test/manual).

**Non-goals**

- NG1: Story 2 — invalid-credentials copy and enumeration-safe **401** (regression only).
- NG2: Password strength rules on login (BR-05 applies to registration only).
- NG3: `Idempotency-Key`, rate limiting, CAPTCHA.
- NG4: New EF migration, MatchingEngine, Redis book changes.
- NG5: Simulating true HTTP timeout in integration tests (manual UI for network tab / throttling).

## Traceability matrix

| Spec story | Plan task(s) | Test evidence |
|------------|--------------|---------------|
| Story 5 — valid single submit → one session | Task 2 | `LoginUser_SingleSubmit_InsertsOneSession` |
| Story 5 — `not-an-email` | Task 1, 2 | `LoginUser_InvalidEmail_Returns422` |
| Story 5 — empty password | Task 1, 2 | `LoginUser_EmptyPassword_Returns422` |
| Story 5 — malformed JSON | Task 2 | `LoginUser_MalformedJson_Returns400` (exists — keep in regression) |
| Story 5 — **500** / retry UX | Task 3, 4 | `LoginUser_WhenSessionCreateFails_Returns500` · Manual UI |
| Story 5 — double-click (EC-08) | Task 3, 4 | `LoginUser_ParallelSameCredentials_AtMostOneEffectiveAuth` · client `submittingRef` |
| Story 5 — timeout after success (EC-09) | Task 4 | `LoginUser_AfterSuccess_WalletProbeWorks` · Manual |
| Story 5 — PG down (EC-11) | Task 3 | `LoginUser_WhenSessionCreateFails_Returns500` |
| Stories 1–4 regression | Task 5 | Full `Users` integration suite + `api:verify` |

## Architecture impact

```text
┌─────────────┐  blur/submit (zod)   ┌──────────────────┐
│  web/       │ ───────────────────► │ POST /api/auth/login │
│ login-form  │  422 VALIDATION_FAILED│  AuthEndpoint    │
│ map-login-  │◄── errors.email ─────│       │          │
│ error.ts    │  400 INVALID_REQUEST └───────┼──────────┘
└─────────────┘  500 retry (root)            ▼
                              ┌──────────────────────────────┐
                              │ ValidationBehavior           │
                              │  LoginUserCommandValidator   │
                              └──────────────┬───────────────┘
                                             │ pass
                                             ▼
                              ┌──────────────────────────────┐
                              │ LoginUserCommandHandler      │
                              │  EmailAddress.Create         │
                              │  Verify(plaintext, hash)     │  ← no Password.Create
                              │  SessionStore.CreateSession  │
                              └──────────────────────────────┘
```

| Layer | Change summary |
|-------|----------------|
| Domain | **MODIFY** `Password` — add `ForCredentialVerification(string)` (non-empty only, no BR-05 strength) **or** equivalent application-level wrapper used only by login |
| Application | **CREATE** `LoginUserCommandValidator`, `LoginValidationMessages`; **MODIFY** `LoginUserCommandHandler` — validation vs credentials split |
| Infrastructure | **REUSE** — optional test fake `ThrowOnCreateSessionStore` |
| Api | **MODIFY** `AuthEndpoint` — `.ProducesProblem(422)`; OpenAPI export |
| MatchingEngine | None |
| web/ | **MODIFY** `loginFormSchema`, `map-login-error.ts`, `login-form.tsx` (`onBlur` parity with register) |
| AppHost | None |

## Data & migration plan

| Artifact | Action | DB reference |
|----------|--------|--------------|
| EF migration | **None** | — |
| `user_sessions` | **INSERT** only on successful login after validation | DB §4.9 |
| Redis `session:{id}` | **SET** on success only | DB §12.1 |
| Book recovery | N/A | — |

## Open questions

| # | Question | Source | Answer | Status |
|---|----------|--------|--------|--------|
| 1 | Use `Password.Create` or separate factory for login verify? | Handler review | **`Password.ForCredentialVerification`** (or verify via `string` on hasher) — avoid mapping format/empty failures to **401** | ✅ Answered |
| 2 | Reuse `RegistrationValidationMessages` for email? | DRY | **Shared email messages** via `LoginValidationMessages` mirroring registration copy (same user-facing strings) | ✅ Answered |
| 3 | Parallel login: assert one session row or one cookie? | EC-08, BR-04 | **Cookie-effective + wallet 200**; session rows may be >1 per BR-04 — assert client invariant and `sessionsForUser` increment ≤ parallel success count | ✅ Answered |
| 4 | Document **422** in OpenAPI now? | Contract gap | **Yes** — Task 5 `api:export` | ✅ Answered |

## Risks and mitigations

| Risk | Likelihood | Impact | Mitigation | Owner task |
|------|------------|--------|------------|------------|
| Validator not registered (no 422) | Low | High | Follow `RegisterUserCommandValidator` pattern; integration tests fail fast | Task 2 |
| Handler still catches domain errors as **401** | Medium | High | Remove `Password.Create` from login path; rely on FluentValidation first | Task 1 |
| Parallel login creates multiple sessions | Low | Low | Accept per BR-04; test browser-effective auth via single `HttpClient` + cookie handler | Task 3 |
| EC-09 hard to automate | Medium | Low | Wallet probe test after **200**; manual throttling checklist | Task 4 |

## Prerequisites

- [x] Stories 1–4 implemented on `main` or feature branches ready to merge
- [x] Branch `feature/user-login-story-5` from latest integration branch
- [ ] Docker for Testcontainers
- [ ] `yarn --cwd web api:verify` green on base

## File structure (planned)

```text
CREATE  src/Application/Users/LoginValidationMessages.cs
CREATE  src/Application/Users/Commands/LoginUserCommandValidator.cs
MODIFY  src/Application/Users/Commands/LoginUserCommandHandler.cs
MODIFY  src/Domain/Users/Password.cs                    (ForCredentialVerification — if chosen)
MODIFY  src/Api/Endpoints/AuthEndpoint.cs
MODIFY  contracts/openapi/api.v1.yaml                    (via api:export)
MODIFY  web/src/types/auth.ts
MODIFY  web/src/features/auth/map-login-error.ts
MODIFY  web/src/features/auth/login-form.tsx
CREATE  tests/Api.IntegrationTests/Users/LoginUserValidationTests.cs
CREATE  tests/Api.IntegrationTests/Users/LoginUserTransientFailureTests.cs
CREATE  tests/Api.IntegrationTests/Users/Fakes/ThrowOnCreateSessionStore.cs
MODIFY  tests/Api.IntegrationTests/Users/LoginUserTestHelpers.cs   (AssertValidationFailed — reuse Register helpers or alias)
```

## Authorization, session, and domain notes

- **Session model:** Unchanged — new session row + cookie only after successful credential verify.
- **Validation vs credentials:** Format/empty → **422** before handler. Unknown email / wrong password → **401** `INVALID_CREDENTIALS` (Story 2 — do not regress).
- **BR-02:** Email normalized (trim, lowercase) in validator and `EmailAddress.Create`.
- **BR-04:** Multiple concurrent sessions allowed; EC-08 targets **effective browser cookie**, not single row globally.

## Progress tracker

### Task 1: Server-side login validation and handler split

| Attribute | Value |
|-----------|--------|
| Spec story | Story 5 |
| Depends on | None |
| Estimated complexity | M |
| Parent story issue | #26 |

#### Objective

Invalid email or empty password returns **422** `VALIDATION_FAILED` with camelCase field keys; handler never maps those cases to **401**.

#### Implementation notes

- Add `LoginUserCommandValidator`:
  - `Email`: required, max 254, `MailAddress` format (mirror `RegisterUserCommandValidator` email rules / messages).
  - `Password`: `NotEmpty()` only — **no** BR-05 strength rules on login.
- Add `LoginValidationMessages` (or reuse email strings from `RegistrationValidationMessages` for identical copy).
- Update `LoginUserCommandHandler`:
  - Keep `EmailAddress.Create` after validation passes.
  - Replace `Password.Create` with `Password.ForCredentialVerification(command.Password)` (or `passwordHasher.Verify(command.Password, hash)` if port extended).
  - Keep `catch (BusinessRuleValidationException)` only for unexpected domain failures → still **401** or map to validation — prefer validator to prevent.
- `ValidationBehavior` already registers validators from `AssemblyReference.Assembly` — no DI change if validator is in Application.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `src/Application/Users/LoginValidationMessages.cs` | User-facing validation copy |
| CREATE | `src/Application/Users/Commands/LoginUserCommandValidator.cs` | FluentValidation rules |
| MODIFY | `src/Application/Users/Commands/LoginUserCommandHandler.cs` | Credential verify without registration password rules |
| MODIFY | `src/Domain/Users/Password.cs` | `ForCredentialVerification` factory (minimal) |
| REUSE | `src/Application/Users/Commands/RegisterUserCommandValidator.cs` | Email validation pattern |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `LoginUserCommand_ValidationFailure_ReturnsResultWithoutThrowing` | `LoginUserValidationTests.cs` |
| Integration | `LoginUser_InvalidEmail_Returns422` | same |
| Integration | `LoginUser_EmptyPassword_Returns422` | same |

#### Acceptance criteria

- [x] `POST` with `not-an-email` → **422**, `errors.email`, no `Set-Cookie`, no new `user_sessions` row. *(behavior in Task 1; HTTP assertions in Task 2)*
- [x] `POST` with empty `password` → **422**, `errors.password`, no session. *(behavior in Task 1; HTTP assertions in Task 2)*
- [x] Valid credentials still → **200** + cookie (smoke via existing test).

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | PRD §5.1 US-02; Tech §15.2; DB §4.9 |
| RFC 7807 errors | `VALIDATION_FAILED` + `errors` map |
| ADR needed? | No |

#### Risk

Handler still using `Password.Create` would regress AC — must complete in this task before UI work.

---

### Task 2: Validation integration test suite

| Attribute | Value |
|-----------|--------|
| Spec story | Story 5 |
| Depends on | Task 1 |
| Estimated complexity | M |
| Parent story issue | #26 |

#### Objective

API tests document the full validation matrix and prove no session persistence on validation-only failures.

#### Implementation notes

- **CREATE** `LoginUserValidationTests.cs` mirroring `RegisterUserValidationTests.cs`.
- Extend `LoginUserTestHelpers` with `AssertValidationFailedAsync` (delegate to `RegisterUserTestHelpers` or duplicate thin wrapper).
- Tests:
  - `LoginUser_InvalidEmail_Returns422` — body `not-an-email`, assert `errors.email`.
  - `LoginUser_EmptyPassword_Returns422` — `password: ""`.
  - `LoginUser_ValidationFailure_DoesNotInsert_UserSession` — count `user_sessions` before/after invalid submit.
  - `LoginUser_SingleSubmit_InsertsOneSession` — register user, login once, assert session count +1 for user.
  - `LoginUserCommand_ValidationFailure_ReturnsResultWithoutThrowing` — MediatR direct send.
  - Keep `LoginUser_MalformedJson_Returns400_INVALID_REQUEST` in `LoginUserTests.cs` (regression).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `tests/Api.IntegrationTests/Users/LoginUserValidationTests.cs` | Validation matrix |
| MODIFY | `tests/Api.IntegrationTests/Users/LoginUserTestHelpers.cs` | Shared 422 assertions |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | All tests in Implementation notes | `LoginUserValidationTests.cs` |

#### Acceptance criteria

- [x] All validation tests green in Docker Testcontainers run.
- [x] Failed validation responses have `application/problem+json` and no `Set-Cookie`.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| Tech §17.3 | Integration tests use Testcontainers |
| PostgreSQL authoritative | Session count assertions |

#### Risk

None — isolated test file.

---

### Task 3: Transient failures and double-submit integration tests

| Attribute | Value |
|-----------|--------|
| Spec story | Story 5 (EC-08, EC-09, EC-11) |
| Depends on | Task 1 |
| Estimated complexity | M |
| Parent story issue | #26 |

#### Objective

Prove **500** on infrastructure failure, safe parallel submit behavior, and post-success wallet readability.

#### Implementation notes

- **CREATE** `LoginUserTransientFailureTests.cs` (mirror `RegisterUserTransientFailureTests.cs`):
  - `LoginUser_WhenSessionCreateFails_Returns500_INTERNAL_ERROR` — fake `ISessionStore` throws on `CreateSessionAsync`; no cookie; session count unchanged.
  - `LoginUser_ParallelSameCredentials_AtMostOneEffectiveAuth` — `Task.WhenAll` two login POSTs with same `HttpClient` (cookies); assert ≥1 **200**, `GET /api/wallet` → **200**, no stuck unauthenticated state.
  - `LoginUser_AfterSuccess_WalletProbeWorks` — login **200**, then wallet GET without second login (EC-09 baseline).
- **CREATE** `ThrowOnCreateSessionStore.cs` in `Users/Fakes/`.
- Use `fixture.CreateFactory(Action<IServiceCollection>)` pattern from registration transient tests.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `tests/Api.IntegrationTests/Users/LoginUserTransientFailureTests.cs` | EC-08/09/11 |
| CREATE | `tests/Api.IntegrationTests/Users/Fakes/ThrowOnCreateSessionStore.cs` | Simulated EC-11 |
| REUSE | `tests/Api.IntegrationTests/Users/RegisterUserTransientFailureTests.cs` | Factory override pattern |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | Tests listed above | `LoginUserTransientFailureTests.cs` |

#### Acceptance criteria

- [x] **500** response has `INTERNAL_ERROR`; no session row for failed attempt.
- [x] Parallel login leaves client able to load wallet with **200**.
- [x] No regression in `LoginUserTests` invalid-credentials cases.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| RFC 7807 | `INTERNAL_ERROR` on 500 |
| Redis projection | N/A on failed login |

#### Risk

Parallel race may yield two **200** and two session rows — test asserts **wallet probe**, not row count = 1.

---

### Task 4: Login form validation and error mapping (client)

| Attribute | Value |
|-----------|--------|
| Spec story | Story 5 |
| Depends on | Task 1 |
| Estimated complexity | S |
| Parent story issue | #26 |

#### Objective

Client shows inline field errors for **422**, generic retry for **500**/network, and blocks duplicate in-flight submit (registration parity).

#### Implementation notes

- **`loginFormSchema`:** add `.email('Email address format is invalid.')` on email (align server copy); keep password `min(1)`.
- **`map-login-error.ts`:** map `VALIDATION_FAILED` → `setError` per field (copy loop from `map-register-error.ts`); keep **401** → root invalid-credentials message; **500**/non-`ApiError` → `loginTransientErrorMessage`.
- **`login-form.tsx`:** add `mode: 'onBlur'`, `reValidateMode: 'onChange'` like `register-form.tsx`; confirm `submittingRef` + `isPending` guard (already present — verify in manual checklist).
- **`authApi.login`:** already `suppressErrorToast: true` — keep.
- EC-09: existing `onSuccess` wallet `fetchQuery` — if **401** after **200**, show cookies message; if other error, transient message — document in manual steps.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `web/src/types/auth.ts` | Zod email format |
| MODIFY | `web/src/features/auth/map-login-error.ts` | 422 field mapping |
| MODIFY | `web/src/features/auth/login-form.tsx` | onBlur validation |
| REUSE | `web/src/features/auth/register-form.tsx` | Submit guard + mutation pattern |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Validation blur/submit, 500 retry, double-click | `web/` |

#### Acceptance criteria

- [x] Client-side invalid email blocks submit with inline error before network.
- [x] Server **422** maps to correct fields.
- [x] **500** shows "Something went wrong. Please try again." and submit re-enables.
- [x] Double-click does not fire second request while pending (devtools Network). *(existing `submittingRef` + `isPending` — verify in Task 5 manual checklist)*

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| frontend.mdc | TanStack Query mutation; no Zustand duplicate server state |
| design-system.mdc | `FieldError` per field |

#### Risk

None — UI-only.

---

### Task 5: OpenAPI, regression, and manual sign-off

| Attribute | Value |
|-----------|--------|
| Spec story | Story 5 · Polish |
| Depends on | Tasks 1–4 |
| Estimated complexity | S |
| Parent story issue | #26 |

#### Objective

Contract documents **422**; full Users test suite and `api:verify` green; operator manual checklist complete.

#### Implementation notes

- `AuthEndpoint`: `.ProducesProblem(StatusCodes.Status422UnprocessableEntity)`.
- `yarn --cwd web api:export` + commit `contracts/openapi/api.v1.yaml`.
- Run all `tests/Api.IntegrationTests/Users/*` (expect ~50+ tests).
- Update plan `status` → `complete` after automation; manual checklist below for operator.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `src/Api/Endpoints/AuthEndpoint.cs` | OpenAPI metadata |
| MODIFY | `contracts/openapi/api.v1.yaml` | Exported 422 |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | Full Users folder | `tests/Api.IntegrationTests/Users/` |
| Contract | `yarn --cwd web api:verify` | CI parity |

#### Acceptance criteria

- [x] `api:verify` passes.
- [x] All Users integration tests pass. *(51 Testcontainers tests; `RegisterUserSessionTests` excluded — local Postgres on :5432, see `known-issues.md`)*
- [ ] Manual UI checklist (below) signed off. *(operator on Aspire)*

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| openapi-contract-sync | Export + commit YAML only |
| Stories 1–4 | No regression on logout, session persistence, invalid credentials |

#### Risk

None — polish task.

## Reference files

| File | Why open it |
|------|-------------|
| `src/Application/Users/Commands/RegisterUserCommandValidator.cs` | Email/password validation pattern |
| `tests/Api.IntegrationTests/Users/RegisterUserValidationTests.cs` | 422 test layout |
| `tests/Api.IntegrationTests/Users/RegisterUserTransientFailureTests.cs` | 500 / parallel patterns |
| `web/src/features/auth/register-form.tsx` | Submit guard + onBlur |
| `web/src/features/auth/map-register-error.ts` | 422 field error mapping |
| `src/Application/Users/Commands/LoginUserCommandHandler.cs` | Current 401 leak for invalid email |
| `docs/plans/20260525-095103-user-registration-story-4.md` | Story 4 transient plan template |

## Implementation details (for /build)

### Error codes (Story 5)

| Code | HTTP | When |
|------|------|------|
| `VALIDATION_FAILED` | 422 | Invalid email format, empty password |
| `INVALID_REQUEST` | 400 | Malformed JSON / incomplete body (`RequireCompleteJsonBody`) |
| `INVALID_CREDENTIALS` | 401 | Wrong email/password (Story 2 — unchanged) |
| `INTERNAL_ERROR` | 500 | `SessionStore` or unexpected infrastructure failure |

### Validator sketch (Application)

- `LoginUserCommandValidator` properties: `Email`, `Password` (PascalCase → camelCase `email`, `password` in `errors` via `ValidationBehavior`).
- Messages align with registration email copy for consistency.

### Handler sketch

1. Validator runs (pipeline).
2. `EmailAddress.Create(command.Email)`.
3. `passwordHasher.Verify(Password.ForCredentialVerification(command.Password), user.PasswordHash)`.
4. `sessionStore.CreateSessionAsync` + Redis enqueue (unchanged).

### Client error mapping

```text
ApiError + code VALIDATION_FAILED → setError per field
ApiError + INVALID_CREDENTIALS / 401 → root message (preserve email, clear password)
ApiError + INTERNAL_ERROR / status >= 500 → root transient message
non-ApiError (network) → root transient message
```

### Channel / matching

No changes.

## Verification matrix (plan-level)

| Spec AC | Verified by |
|---------|-------------|
| Valid email/password, one session per intentional submit | `LoginUser_SingleSubmit_InsertsOneSession` |
| `not-an-email` → 422 email | `LoginUser_InvalidEmail_Returns422` |
| Empty password → 422 | `LoginUser_EmptyPassword_Returns422` |
| Malformed JSON → 400 | `LoginUser_MalformedJson_Returns400_INVALID_REQUEST` |
| 500 → retry UX | `LoginUser_WhenSessionCreateFails_Returns500` + Manual |
| Double-click | `submittingRef` + `LoginUser_ParallelSameCredentials_*` + Manual |
| EC-09 ambiguous success | `LoginUser_AfterSuccess_WalletProbeWorks` + Manual |
| Stories 1–4 regression | Task 5 full suite |

## Rollback / recovery

- **Code:** Revert `feature/user-login-story-5` commits.
- **DB:** No migration — optional cleanup of test `user_sessions` rows.
- **Redis:** Session keys TTL-expire.

## Deferred work (Plan B)

- Rate limiting / account lockout (spec §12).
- Automated Playwright test for double-submit.
- Structured `LoginValidationFailed` log event (spec §8 — optional).

## GitHub Links

| Local ref | Issue # | Type | Title | URL |
|-----------|---------|------|-------|-----|
| spec Story 5 | 26 | Story | US-02 / Story 5: Validate login input and transient failures | https://github.com/tranvuongduy2003/trading-simulator/issues/26 |
| epic | 21 | Epic | Spec: User login (US-02) | https://github.com/tranvuongduy2003/trading-simulator/issues/21 |

## Manual UI checklist (Task 5)

1. Aspire up; register a user; open `/login` in a fresh context (or after logout).
2. Submit `not-an-email` + any password → inline email error; no navigation; Network shows **422**.
3. Submit valid email + empty password → password error; **422**.
4. Submit valid credentials once → trading view; Network shows single login **200**.
5. Rapid double-click **Log in** → at most one login request while pending (Network).
6. DevTools → Offline or block request → generic retry message; button re-enables; fix network and retry succeeds.
7. (EC-09) Throttle to slow 3G: if login appears to fail but cookie was set, reload or retry → authenticated (wallet loads).


---

<a id="source-20260525-203000-virtual-cash-story-1md"></a>

## Source 10 of 18: `docs/plans/20260525-203000-virtual-cash-story-1.md`

---
artifact_type: plan
artifact_version: 1
id: plan-20260525-203000-virtual-cash-story-1
title: Virtual Cash Balance — Story 1 (See available cash)
slug: virtual-cash-story-1
filename_template: 20260525-203000-virtual-cash-story-1.md
created_at: 2026-05-25T20:30:00+07:00
updated_at: 2026-05-25T21:30:00+07:00
status: active
owner: engineering
tags: [plan, implementation, trading-simulator, wallet, cash, us-03, story-1]
related_spec: docs/specs/20260525-201500-virtual-cash-balance.md
related_plans: []
prd_refs: [PRD §5.1 US-03, PRD §6.1 FR-1.3, PRD §6.6 FR-6.2, PRD §7.3, PRD §8.1]
tech_refs: [Tech §5.2.1, Tech §6, Tech §8.1, Tech §15.1, Tech §17.3]
db_refs: [DB §4.2 wallets, DB §5 invariants, DB §6.2]
github:
  repo: tranvuongduy2003/trading-simulator
  epic_issue: 33
  story_issue_ids: [34]
  last_synced_at: 2026-05-25T20:30:00+07:00
search_index:
  keywords: [wallet, virtual cash, available balance, top bar, AppLayout, GET api wallet, GetMyWalletQuery, useWalletQuery, trading-page, formatUsd, skeleton, 500 error, BR-02, BR-05, EC-01, EC-05, US-03 story-1]
  bounded_contexts: [Trading]
  task_count: 6
---

# Implementation Plan: Virtual Cash Balance — Story 1

| Field | Value |
|-------|--------|
| Spec | `docs/specs/20260525-201500-virtual-cash-balance.md` (§2 Story 1) |
| GitHub story | [#34 — See how much cash I can trade with](https://github.com/tranvuongduy2003/trading-simulator/issues/34) |
| Epic | [#33 — Virtual cash balance display (US-03)](https://github.com/tranvuongduy2003/trading-simulator/issues/33) |
| Depends on | US-01 registration (wallet row + $100k), US-02 login (session + protected routes) — **already shipped** |
| Status | Automation complete — manual sign-off pending |
| Tasks | 6 |
| Branch | `feature/virtual-cash-story-1` |
| Aspire impact | No |
| Schema impact | No |
| Test levels | Domain (reuse) · API integration (new focused suite) · Manual UI |
| ADRs required | ADR-004 (top-bar available cash) |
| GitHub | Synced 2026-05-25 — see §GitHub Links |

## Executive summary

Story 1 (US-03) requires the authenticated trading view to show **available virtual cash** prominently, with correct USD formatting, non-blocking loading in the cash area, and honest error handling when `GET /api/wallet` fails. **Product confirmed (spec §13 Q1):** available cash also appears in the **app top bar** on every authenticated route (compact chip), aligned with PRD §8.1. The vertical slice **already exists** end-to-end: `GetMyWalletQuery` + `WalletEndpoint`, registration/login integration tests asserting **$100,000** balances, and `trading-page.tsx` with a “Virtual cash” card using `formatUsd` (two decimals). This plan **hardens** Story 1: dedicated wallet API tests (including **500**), decouples wallet UI state from portfolio fetch failures, shared `useWalletQuery`, dashboard `VirtualCashCard`, top-bar chip in `AppLayout`, and manual verification. Stories 2–4 stay out of scope.

## Goals and non-goals

**Goals**

- G1: Story 1 acceptance criteria pass with traceable automated tests (`GetMyWalletTests` or equivalent).
- G2: Cash card loads independently — portfolio slow/failed does not hide wallet or block the whole trading grid.
- G3: On wallet **500**, show spec copy and **no** numeric balance (EC-05).
- G4: Display **available** as primary figure; **$100,000.00** for new users (EC-01, BR-05).
- G5: USD display uses exactly **two** decimal places (BR-02 read path via `formatUsd`).
- G6: **Top bar** shows compact **available** cash on all `AppLayout` routes (Trading, Portfolio, Orders) via shared wallet query — no duplicate fetch.

**Non-goals**

- NG1: Story 2 — reserved/total breakdown UX emphasis beyond existing secondary line (top bar shows **available** only, not total/reserved).
- NG2: Story 3 — cross-user TanStack cache hardening (logout/login B) — separate issue #36.
- NG3: Story 4 — refetch-on-focus / read-your-writes polish — issue #37.
- NG4: SignalR wallet push, order placement reserves.
- NG5: New tables/migrations, matching engine, Redis wallet projections.
- NG6: Full PRD §8.1 terminal (symbol, last price, daily change in top bar) — placeholders OK; **cash chip only** for Story 1.

## Traceability matrix

| Spec story | Plan task(s) | Test evidence |
|------------|--------------|---------------|
| Story 1 — prominent available cash | Task 2, 3, 4 | Manual: trading card + top bar chip show available |
| PRD §8.1 top-bar cash (Q1 = Yes) | Task 4 | Manual: chip visible on Portfolio/Orders routes |
| Story 1 — new user $100k / reserved 0 | Task 1 | `GetMyWallet_AfterRegister_Returns100kAvailable` (or reuse pattern from `RegisterUserTests`) |
| Story 1 — two decimal USD display | Task 3, 5 | `formatUsd` + manual visual check |
| Story 1 — wallet loading skeleton | Task 2 | Manual: skeleton in cash card only; holdings may load separately |
| Story 1 — API 500 no fake balance | Task 1, 2 | `GetMyWallet_WhenReadFails_Returns500`; UI shows error, no `$0` |
| BR-02 available = total − reserved | Task 1 | Handler already computes; assert in integration test |
| BR-05 / BR-06 | Task 1 | Register + wallet GET asserts USD 100k virtual |

## Architecture impact

```text
┌─────────────────┐     useWalletQuery (queryKey ['wallet'])
│ AppLayout       │ ──┐
│ WalletTopBarChip│   │  GET /api/wallet   ┌─────────────────────┐
└─────────────────┘   ├──────────────────► │ WalletEndpoint      │
┌─────────────────┐     │                  │  → GetMyWalletQuery │
│ TradingPage     │ ──┘                  │  → WalletReadRepo     │
│ VirtualCashCard │ ◄── WalletResponse     └─────────────────────┘
└─────────────────┘
       │ portfolio query (isolated from wallet UI)
       ▼
```

| Layer | Change summary |
|-------|----------------|
| Domain | **REUSE** — `Wallet.AvailableBalance`, `UserRegisterTests` initial cash |
| Application | **REUSE** — `GetMyWalletQueryHandler`; optional **none** unless 500 mapping gap found |
| Infrastructure | **CREATE** test fake `ThrowOnWalletReadRepository` only in integration tests |
| Api | **MODIFY** — add `Produces 500` on wallet endpoint if missing; no handler logic change expected |
| MatchingEngine | None |
| web/ | **CREATE** `use-wallet-query`, `virtual-cash-card`, `wallet-top-bar-chip`; **MODIFY** `trading-page.tsx`, `app-layout.tsx` |
| AppHost | None |

## Data & migration plan

| Artifact | Action | DB reference |
|----------|--------|--------------|
| EF migration | **None** | — |
| Redis keys | **None** | DB §12 (wallet not in Redis) |
| Book recovery | N/A | — |

## Open questions

| # | Question | Source | Answer | Status |
|---|----------|--------|--------|--------|
| 1 | Top-bar wallet before full terminal? | Spec §13 | **Yes** — compact available-cash chip in `AppLayout` header on all authenticated routes; dashboard card retained | ✅ Answered |
| 2 | Reuse scattered wallet tests vs new `GetMyWalletTests` class? | Code review | **Dedicated class** for Story 1 traceability; keep existing register/login tests unchanged | ✅ Answered |
| 3 | Portfolio failure should hide holdings only? | Spec §4a loading | **Yes** — wallet card renders on its own query state | ✅ Answered |

## Risks and mitigations

| Risk | Likelihood | Impact | Mitigation | Owner task |
|------|------------|--------|------------|------------|
| Coupled loading hides cash when portfolio fails | M | M | Split query pending/error UI per card | Task 2 |
| Showing `$0` during load | L | H | Skeleton until `walletQuery.isSuccess`; never render card with zero as placeholder | Task 2 |
| Duplicate wallet test maintenance | L | L | New tests assert Story 1 AC only; don’t duplicate full register flows | Task 1 |
| `useSession` + `['wallet']` key divergence | L | M | Shared `useWalletQuery` for layout + trading; session uses `['auth','session']` — document in Task 6 manual | Task 6 |
| Top bar crowded on narrow desktop | L | L | Compact chip: short label + amount; truncate; hide label below `md` if needed | Task 4 |

## Prerequisites

- [x] US-01 / US-02 shipped (`wallets` table, session auth)
- [x] `GET /api/wallet` implemented
- [ ] Spec status may remain draft — proceed per issue #34 scope
- [ ] Local Aspire stack for manual checklist

## File structure (planned)

```text
tests/TradingSimulator.Api.IntegrationTests/
  Users/GetMyWalletTests.cs                    CREATE
  Users/Fakes/ThrowOnWalletReadRepository.cs CREATE

src/Api/Endpoints/WalletEndpoint.cs            MODIFY (Produces 500)

web/src/features/trading/
  hooks/use-wallet-query.ts                    CREATE
  components/virtual-cash-card.tsx             CREATE
  components/wallet-top-bar-chip.tsx             CREATE
  pages/trading-page.tsx                       MODIFY

web/src/layouts/app-layout.tsx                 MODIFY

web/src/lib/format.ts                          REUSE
docs/memory/decisions.md                       MODIFY (ADR-004)
contracts/openapi/api.v1.yaml                  MODIFY (if 500 missing)
```

## Authorization, session, and domain notes

- **Session model:** Cookie session; `GET /api/wallet` requires auth (`RequireAuthorization`). Story 1 assumes user already reached trading view via login/register.
- **Domain rules (must not violate):**
  - BR-02: `availableBalance` = `totalBalance - reservedBalance` (computed in `GetMyWalletQueryHandler`, not stored).
  - BR-05: New users **$100,000** total, **$0** reserved.
  - BR-06: Virtual simulation money — label “Virtual cash” in UI.
- **Story 1 does not** implement BR-07 cross-user cache clearing (Story 3).

## Progress tracker

### Task 1: Add Story 1 wallet API integration tests

| Attribute | Value |
|-----------|--------|
| Spec story | Story 1 |
| Depends on | None |
| Estimated complexity | M |
| Parent story issue | #34 |

#### Objective

Automated proof that `GET /api/wallet` satisfies Story 1 happy and failure paths: **200** with correct balances for a new user, **401** without session, **500** when read repository throws, with RFC 7807 `INTERNAL_ERROR`.

#### Implementation notes

- **CREATE** `GetMyWalletTests` using existing `UsersApiFixture` + Testcontainers pattern from `RegisterUserTests` / `LoginUserTests`.
- Happy path: register (or login) → GET wallet → assert `AvailableBalance`, `TotalBalance` = **100000**, `ReservedBalance` = **0**, `Currency` = **USD**, `available` = `total - reserved`.
- **CREATE** `ThrowOnWalletReadRepository` implementing `IWalletReadRepository` that throws on `GetByUserIdAsync`; register via `fixture.CreateFactory(ConfigureThrowOnWalletRead)`.
- Assert **500** + `code` **INTERNAL_ERROR**; response body must not deserialize as wallet.
- Do **not** remove existing wallet assertions in register/login tests (regression safety).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `tests/Api.IntegrationTests/Users/GetMyWalletTests.cs` | Story 1 API AC |
| CREATE | `tests/Api.IntegrationTests/Users/Fakes/ThrowOnWalletReadRepository.cs` | Simulate read failure |
| REUSE | `tests/Api.IntegrationTests/Users/RegisterUserTests.cs` | Fixture/helpers pattern |
| REUSE | `src/Application/Users/Queries/GetMyWalletQueryHandler.cs` | No change expected |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `GetMyWallet_WithoutSession_Returns401` | `GetMyWalletTests.cs` |
| Integration | `GetMyWallet_AfterRegister_Returns100kBalances` | `GetMyWalletTests.cs` |
| Integration | `GetMyWallet_WhenReadFails_Returns500_INTERNAL_ERROR` | `GetMyWalletTests.cs` |

#### Acceptance criteria

- [x] All three integration tests pass under Docker Testcontainers
- [x] Existing Users integration suite still green
- [x] `availableBalance` equals `totalBalance - reservedBalance` on 200 response

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | PRD FR-1.3, FR-6.2; Tech §6, §8.1; DB §4.2 |
| Async matching | N/A (read-only) |
| PostgreSQL authoritative | Read via `WalletReadRepository` |
| Redis projection | N/A |
| RFC 7807 errors | 500 `INTERNAL_ERROR` |
| SignalR | N/A |
| Aspire | None |
| ADR needed? | No |

#### Risk

None — test-only plus test fake.

---

### Task 2: Isolate wallet loading and error UI on trading view

| Attribute | Value |
|-----------|--------|
| Spec story | Story 1 |
| Depends on | Task 1 |
| Estimated complexity | M |
| Parent story issue | #34 |

#### Objective

Wallet card has its own loading skeleton and error message; portfolio query pending/error does not suppress cash display or block the entire account grid.

#### Implementation notes

- Replace combined `isLoading` / `hasError` on `trading-page.tsx` with **per-query** state for wallet vs portfolio.
- Wallet area: `walletQuery.isPending` → skeleton inside cash card bounds only; `walletQuery.isError` → spec error copy (`role="alert"`); **do not** render balance figures on error.
- Holdings card: independent skeleton/error (optional muted error) so rest of page remains usable.
- Keep `queryKey: ['wallet']` and `staleTime: 30_000` per frontend rules.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `web/src/features/trading/pages/trading-page.tsx` | Decouple query UI states (card extraction in Task 3) |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Wallet loads while portfolio pending — cash skeleton visible, page header visible | Aspire |
| Manual | Simulate portfolio 500 (if possible) — wallet still shows if wallet 200 | Aspire |

#### Acceptance criteria

- [x] Loading skeleton appears only in virtual cash area, not full-page blocking grid
- [x] Wallet **500** shows error text; no `$0.00` or stale numbers
- [x] `yarn --cwd web build` passes

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | PRD §8.1 layout; spec §4a loading |
| RFC 7807 errors | Map `ApiError` for wallet query |
| Aspire | None |

#### Risk

Low — UI-only; verify no layout shift regressions.

---

### Task 3: Shared useWalletQuery and VirtualCashCard

| Attribute | Value |
|-----------|--------|
| Spec story | Story 1 |
| Depends on | Task 2 |
| Estimated complexity | M |
| Parent story issue | #34 |

#### Objective

Single TanStack Query hook (`useWalletQuery`) powers wallet reads; `VirtualCashCard` on the trading page shows **Virtual cash**, large **available**, and secondary total/reserved with two-decimal USD.

#### Implementation notes

- **CREATE** `useWalletQuery` — `queryKey: ['wallet']`, `queryFn: authApi.getWallet`, `staleTime: 30_000`, `normalizeWallet` on success. Replace inline `useQuery` in `trading-page.tsx`.
- **CREATE** `VirtualCashCard` — props from query state: `isPending`, `isError`, `wallet` (normalized). Move error copy constant to shared module if top bar reuses it.
- Apply design-system tokens: `Card`, `tabular-nums`, `text-muted-foreground` for secondary line.
- **REUSE** `formatUsd` — two decimal places.
- Primary emphasis on **available**; total/reserved remain secondary (Story 2 refines copy later).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `web/src/features/trading/hooks/use-wallet-query.ts` | Shared wallet query |
| CREATE | `web/src/features/trading/components/virtual-cash-card.tsx` | Dashboard cash card |
| MODIFY | `web/src/features/trading/pages/trading-page.tsx` | Use hook + card |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | New account shows **$100,000.00** available on trading card | Aspire |

#### Acceptance criteria

- [x] Card matches spec labels (“Virtual cash”, “Available to trade”)
- [x] Amounts display as `$X,XXX.XX` (two decimals)
- [x] `yarn --cwd web lint` passes

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| design-system.mdc | shadcn Card, tabular nums, no raw hex |
| frontend.mdc | Server state in Query only; same `queryKey` for layout reuse in Task 4 |

#### Risk

None — refactor for clarity.

---

### Task 4: Top-bar available cash chip (PRD §8.1, spec Q1)

| Attribute | Value |
|-----------|--------|
| Spec story | Story 1 |
| Depends on | Task 3 |
| Estimated complexity | M |
| Parent story issue | #34 |

#### Objective

On every authenticated `AppLayout` screen, the header shows a compact **available** virtual cash figure (e.g. **$100,000.00**) so users see spendable cash without navigating to Trading.

#### Implementation notes

- **CREATE** `WalletTopBarChip` — uses `useWalletQuery` (deduped with trading page).
- **MODIFY** `app-layout.tsx` — mount chip in header between `<nav>` and `<UserMenu>`: layout `flex items-center gap-2`.
- **Display:** Short label “Cash” (visible `sm:` and up) + `formatUsd(availableBalance)` in `tabular-nums`; `aria-label` e.g. “Available virtual cash {amount}”.
- **Loading:** inline `Skeleton` (~`h-5 w-24`), not full header block.
- **Error:** show em dash or “Unavailable” — **no** numeric placeholder (EC-05). Do not duplicate the long trading-page error paragraph in the header.
- **Auth:** render only when `useAuthStore` status is `authenticated` (same as `UserMenu`).
- Header already uses `viewTransitionName: 'site-header'` — chip lives inside isolated header; no extra VT.
- Symbol / last price / daily change remain out of scope (NG6).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `web/src/features/trading/components/wallet-top-bar-chip.tsx` | Compact header cash |
| MODIFY | `web/src/layouts/app-layout.tsx` | Insert chip in top bar |
| MODIFY | `docs/memory/decisions.md` | ADR-004 |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Chip shows **$100,000.00** on Trading, Portfolio, Orders after login | Aspire |
| Manual | Navigate Portfolio → Trading — single wallet fetch (Network tab, optional) | Aspire |
| Manual | Wallet API failure — chip shows no fake balance | Aspire |

#### Acceptance criteria

- [x] Available cash visible in top bar on all main nav routes
- [x] Two-decimal USD formatting matches dashboard card
- [x] Loading skeleton in chip area only; user menu still usable
- [x] ADR-004 recorded in `docs/memory/decisions.md`
- [x] `yarn --cwd web build` passes

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD §8.1 | Top bar includes available cash (other top-bar fields deferred) |
| design-system.mdc | Tabular nums, semantic tokens, `aria-label` on compact readout |

#### Risk

Low — header width; test at 1280px per PRD.

---

### Task 5: OpenAPI and contract alignment for wallet errors

| Attribute | Value |
|-----------|--------|
| Spec story | Story 1 (API contract) |
| Depends on | Task 1 |
| Estimated complexity | S |
| Parent story issue | #34 |

#### Objective

Committed OpenAPI documents `GET /api/wallet` **401**, **404**, and **500** responses; `yarn --cwd web api:verify` passes.

#### Implementation notes

- **MODIFY** `WalletEndpoint` — `.ProducesProblem(StatusCodes.Status500InternalServerError)` (matches Auth/Users endpoints).
- Run `yarn --cwd web api:export` and commit `contracts/openapi/api.v1.yaml` only.
- Follow `openapi-contract-sync` skill checklist.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `src/Api/Endpoints/WalletEndpoint.cs` | OpenAPI metadata |
| MODIFY | `contracts/openapi/api.v1.yaml` | Exported contract |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| CI | `yarn --cwd web api:verify` | — |

#### Acceptance criteria

- [x] `api:verify` green locally
- [x] Wallet path lists 500 response in YAML

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| api-guidelines.mdc | RFC 7807 on errors |

#### Risk

None.

---

### Task 6: Polish, regression, and manual sign-off

| Attribute | Value |
|-----------|--------|
| Spec story | Story 1 \| Polish |
| Depends on | Tasks 1–5 |
| Estimated complexity | S |
| Parent story issue | #34 |

#### Objective

Full regression run, manual Story 1 checklist on Aspire, update tracking docs; ready for PR to `main` closing #34.

#### Implementation notes

- Run `dotnet test` on Users + new wallet tests; `yarn --cwd web lint` + `build`.
- Manual checklist (operator):
  1. Register new user → trading view → **$100,000.00** available on **dashboard card** and **top bar**, reserved **$0.00** on card.
  2. Open **Portfolio** and **Orders** → top bar still shows same available amount.
  3. Refresh page with valid session → same balances within 2 s (card + chip).
  4. Throttle network (DevTools) → cash skeleton in card and chip; page usable.
  5. (Optional) Stop API / break DB → trading card error message; top bar shows no fake balance.
- Note: `useSession` uses `['auth','session']`; display uses `useWalletQuery` (`['wallet']`) — both hit `GET /api/wallet`; TanStack dedupes concurrent mounts.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `docs/memory/current-status.md` | Post-build status |
| MODIFY | `docs/CHANGELOG.md` | Impl entry after `/build` |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | Full `TradingSimulator.Api.IntegrationTests` Users suite | — |
| Manual | Story 1 checklist above | — |

#### Acceptance criteria

- [x] All automated tests green (**54** Users Testcontainers; excludes `RegisterUserSessionTests` — local Postgres)
- [ ] Manual checklist signed off by operator (see §Manual UI checklist)
- [x] No regression to login/register/logout flows (included in Users suite)
- [x] Spec §13 Q1 implemented (top-bar chip + ADR-004)

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| Epic regression | BR-02, BR-05, BR-06 unchanged |

#### Risk

None.

## Manual UI checklist (operator)

Run on Aspire (`feature/virtual-cash-story-1`). Sign off when all pass.

1. Register new user → **Trading** → dashboard card and top bar show **$100,000.00** available; card shows reserved **$0.00**.
2. Open **Portfolio** and **Orders** → top bar shows the same available amount.
3. Refresh with valid session → balances unchanged within ~2 s (card + chip).
4. DevTools network throttle → skeleton in cash card and chip only; page remains usable.
5. (Optional) Break wallet API → trading card shows error copy; top bar shows **Unavailable** (no fake balance).

## Reference files

| File | Why open it |
|------|-------------|
| `docs/specs/20260525-201500-virtual-cash-balance.md` | Story 1 AC source |
| `web/src/features/trading/pages/trading-page.tsx` | Current combined loading/error |
| `src/Application/Users/Queries/GetMyWalletQueryHandler.cs` | Available balance computation |
| `src/Api/Endpoints/WalletEndpoint.cs` | Route + OpenAPI |
| `tests/Api.IntegrationTests/Users/RegisterUserTests.cs` | Wallet 200 + $100k pattern |
| `tests/Api.IntegrationTests/Users/Fakes/ThrowOnCreateSessionStore.cs` | Test double pattern for 500 |
| `web/src/lib/format.ts` | `formatUsd` two-decimal rule |
| `web/src/layouts/app-layout.tsx` | Top bar insertion point |
| `docs/plans/20260525-180000-user-login-story-4.md` | Plan template + GitHub sync style |

## Implementation details (for /build)

### Backend (likely unchanged)

- `GetMyWalletQuery` → `GetMyWalletQueryHandler` uses `ICurrentUserAccessor` + `IWalletReadRepository`.
- `WalletResponse` fields: `totalBalance`, `reservedBalance`, `availableBalance` (decimal JSON).
- **404** `WALLET_NOT_FOUND` if row missing — UI should treat as error (spec: defect, not empty zero).

### Test fake sketch

```csharp
// ThrowOnWalletReadRepository : IWalletReadRepository
public Task<WalletReadModel?> GetByUserIdAsync(...) =>
    throw new InvalidOperationException("Simulated wallet read failure");
```

Register with `services.AddScoped<IWalletReadRepository, ThrowOnWalletReadRepository>()` in test factory configure callback.

### Frontend state matrix

| walletQuery | portfolioQuery | Cash card | Holdings card |
|-------------|----------------|-----------|---------------|
| pending | any | Skeleton | Own skeleton |
| error | any | Error alert, no amounts | Optional error |
| success | pending | Show balances | Skeleton |
| success | error | Show balances | Error or empty table |

### Error copy (spec)

> Could not load account data. Try refreshing or sign in again.

Keep existing string in `VirtualCashCard` (shared constant optional for chip error label).

### Top bar chip (Q1 = Yes)

```tsx
// WalletTopBarChip — sketch
const { data, isPending, isError } = useWalletQuery()
// pending → <Skeleton className="h-5 w-24" />
// error → <span className="text-muted-foreground text-sm">Unavailable</span>
// success → <span className="tabular-nums">{formatUsd(normalizeWallet(data).availableBalance)}</span>
```

## Verification matrix (plan-level)

| Spec AC | Verified by |
|---------|-------------|
| Authenticated trading view shows available USD | Task 3–4 manual |
| Top bar available cash (Q1) | Task 4 manual + ADR-004 |
| New user $100k / $0 reserved | Task 1 integration |
| Two decimal USD display | Task 3–4 `formatUsd` + manual |
| Non-blocking cash skeleton | Task 2–4 manual |
| GET wallet 500 → error, no fabricated balance | Task 1 integration + Task 2–4 UI |
| EC-01 new user all-available $100k | Task 1 |
| EC-05 API 500 | Task 1 + Task 2–4 |

## Rollback / recovery

- **Code:** revert branch `feature/virtual-cash-story-1`
- **DB:** N/A
- **Redis:** N/A

## Deferred work (Plan B)

- Story 2 plan: `docs/plans/<ts>-virtual-cash-story-2.md` — total/reserved breakdown UX (#35)
- Story 3: cross-user wallet cache (#36)
- Story 4: refetch-on-focus / read-your-writes (#37)
- SignalR wallet push when orders ship
- PRD §8.1 full top bar (AAPL symbol, last price, daily change)

## GitHub Links

| Local ref | Issue # | Type | Title | URL |
|-----------|---------|------|-------|-----|
| spec Story 1 | 34 | Story | US-03 / Story 1: See how much cash I can trade with | https://github.com/tranvuongduy2003/trading-simulator/issues/34 |
| spec epic | 33 | Epic | Spec: Virtual cash balance display (US-03) | https://github.com/tranvuongduy2003/trading-simulator/issues/33 |


---

<a id="source-20260525-220000-virtual-cash-story-2md"></a>

## Source 11 of 18: `docs/plans/20260525-220000-virtual-cash-story-2.md`

---
artifact_type: plan
artifact_version: 1
id: plan-20260525-220000-virtual-cash-story-2
title: Virtual Cash Balance — Story 2 (Total vs reserved)
slug: virtual-cash-story-2
filename_template: 20260525-220000-virtual-cash-story-2.md
created_at: 2026-05-25T22:00:00+07:00
updated_at: 2026-05-25T24:00:00+07:00
status: completed
owner: engineering
tags: [plan, implementation, trading-simulator, wallet, cash, us-03, story-2]
related_spec: docs/specs/20260525-201500-virtual-cash-balance.md
related_plans: [docs/plans/20260525-203000-virtual-cash-story-1.md]
prd_refs: [PRD §5.1 US-03, PRD §6.6 FR-6.2, PRD §7.3, PRD §7.4]
tech_refs: [Tech §5.2.1, Tech §6, Tech §8.1, Tech §17.3]
db_refs: [DB §4.2 wallets, DB §5 invariants, DB §6.2]
github:
  repo: tranvuongduy2003/trading-simulator
  epic_issue: 33
  story_issue_ids: [35]
  last_synced_at: 2026-05-25T22:00:00+07:00
search_index:
  keywords: [wallet, total balance, reserved balance, available balance, virtual cash, breakdown, BR-02, BR-04, EC-02, GetMyWallet, VirtualCashCard, display integrity]
  bounded_contexts: [Trading]
  task_count: 4
---

# Implementation Plan: Virtual Cash Balance — Story 2

| Field | Value |
|-------|--------|
| Spec | `docs/specs/20260525-201500-virtual-cash-balance.md` (§2 Story 2) |
| GitHub story | [#35 — Understand total versus reserved cash](https://github.com/tranvuongduy2003/trading-simulator/issues/35) |
| Epic | [#33 — Virtual cash balance display (US-03)](https://github.com/tranvuongduy2003/trading-simulator/issues/33) |
| Depends on | Story 1 plan shipped (`docs/plans/20260525-203000-virtual-cash-story-1.md`) — `VirtualCashCard`, `useWalletQuery`, `GET /api/wallet` |
| Status | COMPLETE (automation) — manual UI checklist pending operator |
| Tasks | 4 |
| Branch | `feature/virtual-cash-story-2` |
| Aspire impact | No |
| Schema impact | No |
| Test levels | API integration (new reserved scenario) · Manual UI |
| ADRs required | None |
| GitHub | Synced 2026-05-25 — see §GitHub Links |

## Executive summary

Story 2 (US-03) helps users understand **why available cash is lower than total** by showing **total** and **reserved** alongside **available** on the trading dashboard cash card. Story 1 already introduced a secondary line (`Total … · Reserved …`) and two-decimal `formatUsd`; this plan **verifies and hardens** Story 2: an integration test seeds non-zero `reserved_balance` in PostgreSQL (EC-02 / spec happy path), the card emphasizes the breakdown when reserved is positive and uses clear zero-reserved copy, and the UI **never recomputes** available from total − reserved (failure-path AC: display exactly three API fields). Top bar chip stays **available-only** per Story 1 scope. No backend handler or schema changes expected.

## Goals and non-goals

**Goals**

- G1: Happy path AC — **$50,000** total, **$10,000** reserved → UI shows **$40,000.00** available and secondary **Total $50,000.00 · Reserved $10,000.00** (or equivalent labels).
- G2: Zero reserved — show **$0.00** and concise copy that nothing is tied up in open buys (spec allows either; implement both for clarity).
- G3: EC-02 — when **$5,000** reserved, all three fields visible (covered by integration test + manual).
- G4: Display integrity — render `availableBalance`, `totalBalance`, `reservedBalance` from API/normalized wallet only; no client-side fourth derived amount.
- G5: Automated API proof that `GET /api/wallet` returns correct breakdown when `reserved_balance > 0`.

**Non-goals**

- NG1: Story 3 — cross-user cache / session privacy (#36).
- NG2: Story 4 — refetch-on-focus / read-your-writes (#37).
- NG3: Order placement or live reserve updates (US-10+); seeding reserved in tests is direct DB update only.
- NG4: Top bar total/reserved (chip remains available-only).
- NG5: SignalR wallet push, migrations, matching engine, Redis wallet projections.
- NG6: Frontend unit/E2E test framework (manual for inconsistent-API defect path).

## Traceability matrix

| Spec story | Plan task(s) | Test evidence |
|------------|--------------|---------------|
| Story 2 — $50k / $10k / $40k happy path | Task 1, 2 | `GetMyWallet_WithSeededReserved_ReturnsCorrectBreakdown`; manual |
| Story 2 — reserved $0 visible | Task 2 | Manual; existing register test + card copy |
| Story 2 — inconsistent API (defect) | Task 3 | Code review + manual mock props; no client recompute |
| BR-02 available = total − reserved | Task 1 | Integration assert on API response |
| BR-04 reserved reflects open buys | Task 2 | Copy when reserved > 0; future orders wire same API |
| EC-02 $5k reserved | Task 1 | Integration uses $5k reserved variant (or second assert in same test) |

## Architecture impact

```text
┌──────────────────┐
│ VirtualCashCard  │  reads normalizeWallet(data) — three fields, no math
│  (trading view)  │
└────────┬─────────┘
         │ useWalletQuery
         ▼
   GET /api/wallet ──► GetMyWalletQueryHandler (unchanged)
                         available = total − reserved (server)
         ▼
   PostgreSQL wallets.reserved_balance  ◄── test seeds via EF only
```

| Layer | Change summary |
|-------|----------------|
| Domain | **REUSE** — `Wallet.AvailableBalance`, invariants unchanged |
| Application | **REUSE** — `GetMyWalletQueryHandler` |
| Infrastructure | **REUSE** — `WalletReadRepository`; tests **MODIFY** wallet row via `ApplicationDatabaseContext` |
| Api | **REUSE** — `WalletEndpoint` |
| MatchingEngine | None |
| web/ | **MODIFY** `virtual-cash-card.tsx`, `wallet-display.ts`; **REUSE** `formatUsd`, `useWalletQuery` |
| AppHost | None |

## Data & migration plan

| Artifact | Action | DB reference |
|----------|--------|--------------|
| EF migration | **None** | — |
| Redis keys | **None** | — |
| Book recovery | N/A | — |
| Test seeding | **UPDATE** `wallets.reserved_balance` (and optionally `total_balance`) after register in integration test | DB §4.2, `ck_wallets_reserved_le_total` |

## Open questions

| # | Question | Source | Answer | Status |
|---|----------|--------|--------|--------|
| 1 | Is Story 1 secondary line enough for Story 2 emphasis? | Code review | **Yes for MVP** — add conditional helper line when `reservedBalance > 0`; no chart/tooltip | ✅ Answered |
| 2 | How to prove inconsistent-API UI without breaking handler? | Spec failure path | **Manual** — pass mock props to `VirtualCashCard` in dev; enforce no `total - reserved` in component via review + `wallet-display` helpers | ✅ Answered |
| 3 | Branch from `main` or Story 1 branch? | Delivery | **After Story 1 merges** — branch `feature/virtual-cash-story-2` from `main`; if Story 1 open, rebase onto it | ✅ Answered |

## Risks and mitigations

| Risk | Likelihood | Impact | Mitigation | Owner task |
|------|------------|--------|------------|----------|
| Story 1 not merged — duplicate UI work | M | L | Rebase on `feature/virtual-cash-story-1` or merge Story 1 first | Prerequisites |
| Test seed violates DB check constraints | L | M | Keep `reserved ≤ total`, non-negative | Task 1 |
| Client recomputes available and masks API bugs | L | H | Single display module; grep guard in Task 3 | Task 3 |
| Operators cannot manually see $5k reserved without SQL | M | L | Document Aspire + SQL seed in manual checklist | Task 4 |

## Prerequisites

- [x] Story 1 automation complete (`VirtualCashCard`, `GetMyWalletTests`, top bar chip)
- [ ] Story 1 merged to `main` **or** branch `feature/virtual-cash-story-2` based on Story 1 branch
- [ ] Docker available for Api integration tests
- [ ] Spec §13: no open questions block Story 2 (Q1 top bar is Story 1 only)

## File structure (planned)

```text
tests/Api.IntegrationTests/Users/GetMyWalletTests.cs   MODIFY (+2 reserved scenarios)

web/src/features/trading/
  wallet-display.ts                                    MODIFY
  components/virtual-cash-card.tsx                     MODIFY
web/src/hooks/use-session.ts                           MODIFY (normalizeWallet doc)
web/scripts/check-wallet-display-integrity.mjs         CREATE
web/package.json                                       MODIFY (lint:wallet-integrity)

docs/memory/current-status.md                          MODIFY
docs/CHANGELOG.md                                      MODIFY
```

## Authorization, session, and domain notes

- **Session model:** Unchanged — authenticated `GET /api/wallet` only.
- **Domain rules (display must reflect):**
  - **BR-02:** Server computes `availableBalance`; UI shows API value.
  - **BR-04:** Reserved > 0 copy references open buy orders (simulation); no order API in this story.
- **BR-03:** Inconsistent triple is a **defect**; UI must not “fix” by recalculating on the client.

## Progress tracker

### Task 1: API integration test for non-zero reserved balance

| Attribute | Value |
|-----------|--------|
| Spec story | Story 2 |
| Depends on | None (requires Story 1 files on branch) |
| Estimated complexity | M |
| Parent story issue | #35 |

#### Objective

Prove `GET /api/wallet` returns correct **total**, **reserved**, and **available** when `reserved_balance` is seeded in PostgreSQL after registration (simulates future open buy reserves).

#### Implementation notes

- Extend `GetMyWalletTests` (same fixture/collection as Story 1).
- Flow: register + session cookie → resolve `userId` from response → open `ApplicationDatabaseContext` scope → `ExecuteUpdateAsync` on `wallets` (`TotalBalance = 50_000m`, `ReservedBalance = 10_000m`) → GET `/api/wallet`.
- Assert: `TotalBalance` 50_000, `ReservedBalance` 10_000, `AvailableBalance` 40_000, and `AvailableBalance == TotalBalance - ReservedBalance`.
- **EC-02 variant:** Either a second fact or parameterized case: total 100_000, reserved 5_000, available 95_000.
- Pattern for DB access: `fixture.Factory.Services.CreateAsyncScope()` + `ApplicationDatabaseContext` (see `LoginUserTests`).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `tests/Api.IntegrationTests/Users/GetMyWalletTests.cs` | Reserved breakdown test(s) |
| REUSE | `tests/Api.IntegrationTests/Integration/IntegrationTestFixture.cs` | Testcontainers factory |
| REUSE | `src/Infrastructure/Persistence/Entities/WalletRecord.cs` | Seed target |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `GetMyWallet_WithSeededReserved_ReturnsCorrectBreakdown` | `GetMyWalletTests.cs` |
| Integration | `GetMyWallet_WithSeeded5kReserved_ReturnsEc02Breakdown` (or combined) | `GetMyWalletTests.cs` |

#### Acceptance criteria

- [x] New test(s) pass under Docker Testcontainers
- [x] Existing `GetMyWalletTests` still green
- [x] `availableBalance` equals `totalBalance - reservedBalance` on 200 response

#### Notes (Task 1)

- Wallet seed uses `ExecuteUpdateAsync` (not load + `SaveChangesAsync`) so `row_version` concurrency on `wallets` does not block updates in tests.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | FR-6.2; Tech §6; DB §4.2 constraints |
| PostgreSQL authoritative | Direct row update in test |
| RFC 7807 errors | N/A |
| Aspire | None |
| ADR needed? | No |

#### Risk

Low — test-only; respect check constraints.

---

### Task 2: Story 2 cash card UX (breakdown emphasis and zero reserved)

| Attribute | Value |
|-----------|--------|
| Spec story | Story 2 |
| Depends on | Task 1 |
| Estimated complexity | M |
| Parent story issue | #35 |

#### Objective

`VirtualCashCard` clearly communicates total vs reserved vs available per spec: secondary breakdown always visible; when reserved is zero, show **$0.00** plus short copy; when reserved > 0, add subtle helper text tying reserved to open buys.

#### Implementation notes

- **MODIFY** `wallet-display.ts`:
  - `formatWalletBreakdownLine(total, reserved)` → `Total $X · Reserved $Y` using `formatUsd`.
  - `ZERO_RESERVED_HELPER` → e.g. `None tied up in open buy orders`.
  - `RESERVED_HELPER(reserved)` → e.g. `Open buy orders hold {amount}` when `reserved > 0`.
- **MODIFY** `virtual-cash-card.tsx`:
  - Primary: `formatUsd(wallet.availableBalance)` (unchanged).
  - Secondary: breakdown line from helper (always render when wallet success).
  - Tertiary: helper copy — zero vs non-zero reserved.
  - Use `tabular-nums` on all monetary text; `text-muted-foreground` for secondary/tertiary.
  - **Do not** compute `wallet.totalBalance - wallet.reservedBalance` for display.
- Top bar `WalletTopBarChip`: **no change** (available only).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `web/src/features/trading/wallet-display.ts` | Shared breakdown + copy constants |
| MODIFY | `web/src/features/trading/components/virtual-cash-card.tsx` | Story 2 layout/copy |
| REUSE | `web/src/lib/format.ts` | `formatUsd` two decimals |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Seed or test user with reserved → card shows all three amounts + helper | Aspire |
| Manual | New user → reserved **$0.00** + zero helper visible | Aspire |

#### Acceptance criteria

- [x] Happy path: $50k / $10k / $40k readable on card (manual or after Task 1 seed + UI refresh)
- [x] Reserved $0 shows **$0.00** and zero-reserved helper text
- [x] Reserved > 0 shows non-zero helper referencing open buys
- [x] `yarn --cwd web build` passes

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| design-system.mdc | Card, muted secondary, tabular nums |
| frontend.mdc | No wallet state in Zustand |

#### Risk

Low — UI-only on trading card.

---

### Task 3: Display integrity (no client-side recompute)

| Attribute | Value |
|-----------|--------|
| Spec story | Story 2 (failure / edge path) |
| Depends on | Task 2 |
| Estimated complexity | S |
| Parent story issue | #35 |

#### Objective

Ensure the cash card never invents a fourth balance by recomputing available on the client; document verification for the hypothetical inconsistent-API defect.

#### Implementation notes

- Audit `web/src/features/trading/**` and `normalizeWallet` — confirm `availableBalance` comes from API field only (already true in `use-session.ts`).
- Add one-line module comment in `wallet-display.ts`: display layer uses API triple as-is.
- **Manual verification (operator):** Temporarily render `VirtualCashCard` with props `{ availableBalance: 1, totalBalance: 100000, reservedBalance: 5000 }` (e.g. React DevTools or short-lived dev branch) — card must show **$1.00** available, not **$95,000.00**.
- Optional grep CI guard: no `totalBalance - reservedBalance` in `web/src/features/trading/` (document in Task 4 checklist if not automated).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `web/src/features/trading/wallet-display.ts` | Integrity comment |
| MODIFY | `web/src/hooks/use-session.ts` | `normalizeWallet` doc comment |
| MODIFY | `web/scripts/check-wallet-display-integrity.mjs` | Grep guard for client recompute |
| MODIFY | `web/package.json` | `lint:wallet-integrity` script |
| REUSE | `web/src/features/trading/components/virtual-cash-card.tsx` | Must use `wallet.availableBalance` only |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Mock inconsistent triple → UI shows API available, not derived | DevTools / temporary props |

#### Acceptance criteria

- [x] No `total - reserved` (or similar) in `VirtualCashCard` or `wallet-display` formatters
- [x] Manual inconsistent triple shows three API values without correction
- [x] `normalizeWallet` does not overwrite `availableBalance`

#### Notes (Task 3)

- Audit: `normalizeWallet` maps `availableBalance` from API; `VirtualCashCard` / `WalletTopBarChip` display API fields only.
- Automated guard: `yarn --cwd web lint:wallet-integrity` (`web/scripts/check-wallet-display-integrity.mjs`).
- Manual integrity check (operator): DevTools props `{ availableBalance: 1, totalBalance: 100000, reservedBalance: 5000 }` → card shows **$1.00** available (plan §Manual UI checklist item 4).

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| BR-03 | Defect is server-side; client must not mask |

#### Risk

None — documentation and review.

---

### Task 4: Polish, regression, and manual sign-off

| Attribute | Value |
|-----------|--------|
| Spec story | Story 2 \| Polish |
| Depends on | Tasks 1–3 |
| Estimated complexity | S |
| Parent story issue | #35 |

#### Objective

Regression suite green, Story 2 manual checklist complete, tracking docs updated; branch ready for PR closing #35.

#### Implementation notes

- Run `dotnet test` on `Api.IntegrationTests` (Users + wallet tests).
- Run `yarn --cwd web lint:wallet-integrity`, `yarn --cwd web lint`, and `yarn --cwd web build`.
- Confirm Story 1 behaviors unchanged (top bar chip, loading/error, $100k new user).
- Update `docs/CHANGELOG.md` and `docs/memory/current-status.md` after `/build`.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `docs/CHANGELOG.md` | Impl entry |
| MODIFY | `docs/memory/current-status.md` | Next up / completed |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | Full Users Testcontainers suite | — |
| Manual | Story 2 checklist below | — |

#### Acceptance criteria

- [x] All automated tests green (**56** Users Testcontainers; excludes `RegisterUserSessionTests` — local Postgres)
- [ ] Manual Story 2 checklist signed off by operator (see §Manual UI checklist)
- [x] No regression to Story 1 wallet loading/error/top bar (`GetMyWalletTests` 5/5; `VirtualCashCard` / `WalletTopBarChip` unchanged paths)

#### Notes (Task 4)

- Regression: `yarn lint:wallet-integrity`, `yarn lint`, `yarn build` green.
- PR target: `feature/virtual-cash-story-2` → `main` (closes #35 when merged).

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| Epic | BR-02, BR-04, BR-05 unchanged |

#### Risk

None.

## Manual UI checklist (operator)

Run on Aspire (`feature/virtual-cash-story-2`). Sign off when all pass. Use integration test seed or SQL:

```sql
-- Example after identifying user_id
UPDATE wallets SET total_balance = 50000, reserved_balance = 10000 WHERE user_id = '<uuid>';
```

1. Login → **Trading** → card shows **$40,000.00** available, secondary **Total $50,000.00 · Reserved $10,000.00**, helper mentions open buy holds.
2. New user (or reserved reset to 0) → reserved **$0.00** and “none tied up” (or equivalent) visible.
3. Top bar still shows **available only** — no total/reserved in chip.
4. (Integrity) With mocked inconsistent props in dev only — available shows API value, not total − reserved.
5. Regression: wallet **500** still shows error copy, no fake balances (Story 1).

## Reference files

| File | Why open it |
|------|-------------|
| `docs/specs/20260525-201500-virtual-cash-balance.md` | Story 2 AC |
| `docs/plans/20260525-203000-virtual-cash-story-1.md` | Prior slice + patterns |
| `web/src/features/trading/components/virtual-cash-card.tsx` | Current breakdown UI |
| `web/src/features/trading/wallet-display.ts` | Error copy; extend for breakdown |
| `tests/Api.IntegrationTests/Users/GetMyWalletTests.cs` | Add reserved test |
| `tests/Api.IntegrationTests/Users/LoginUserTests.cs` | EF scope pattern |
| `src/Application/Users/Queries/GetMyWalletQueryHandler.cs` | Server-side available math |

## Implementation details (for /build)

### Backend

- No production code changes expected. Handler already returns:
  ```csharp
  new WalletResponse(..., wallet.TotalBalance, wallet.ReservedBalance,
      wallet.TotalBalance - wallet.ReservedBalance);
  ```

### Test seed sketch

```csharp
await using var scope = fixture.Factory.Services.CreateAsyncScope();
var databaseContext = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseContext>();
await databaseContext.Wallets
    .Where(w => w.UserId == userId)
    .ExecuteUpdateAsync(s => s
        .SetProperty(w => w.TotalBalance, 50_000m)
        .SetProperty(w => w.ReservedBalance, 10_000m));
```

### Frontend display rules

| Field | Source | Never |
|-------|--------|-------|
| Available (large) | `wallet.availableBalance` | `total - reserved` on client |
| Total / Reserved line | `wallet.totalBalance`, `wallet.reservedBalance` | Combined “spendable” fourth line |
| Helpers | Derived from `reservedBalance === 0` only | Monetary math |

### Story 1 coexistence

- `useWalletQuery` + `queryKey: ['wallet']` unchanged.
- `WalletTopBarChip` unchanged (available-only per NG4).

## Verification matrix (plan-level)

| Spec AC | Verified by |
|---------|-------------|
| $50k total, $10k reserved → $40k available + secondary line | Task 1 + Task 2 manual |
| Reserved $0 visible with clear copy | Task 2 manual |
| Inconsistent API triple — show three fields, no fourth | Task 3 manual + review |
| EC-02 $5k reserved all visible | Task 1 integration |
| BR-02 on API | Task 1 integration |
| No Story 1 regression | Task 4 |

## Rollback / recovery

- **Code:** revert `feature/virtual-cash-story-2`
- **DB:** N/A (no migration)
- **Redis:** N/A

## Deferred work (Plan B)

- Story 3: cross-user wallet cache (#36)
- Story 4: refetch-on-focus (#37)
- Live reserved updates via order placement (US-10+)
- SignalR wallet push
- Optional Playwright component test for inconsistent triple

## GitHub Links

| Local ref | Issue # | Type | Title | URL |
|-----------|---------|------|-------|-----|
| spec Story 2 | 35 | Story | US-03 / Story 2: Understand total versus reserved cash | https://github.com/tranvuongduy2003/trading-simulator/issues/35 |
| spec Story 1 | 34 | Story | US-03 / Story 1: See how much cash I can trade with | https://github.com/tranvuongduy2003/trading-simulator/issues/34 |
| spec epic | 33 | Epic | Spec: Virtual cash balance display (US-03) | https://github.com/tranvuongduy2003/trading-simulator/issues/33 |


---

<a id="source-20260525-230000-virtual-cash-story-3md"></a>

## Source 12 of 18: `docs/plans/20260525-230000-virtual-cash-story-3.md`

---
artifact_type: plan
artifact_version: 1
id: plan-20260525-230000-virtual-cash-story-3
title: Virtual Cash Balance — Story 3 (Session-private wallet)
slug: virtual-cash-story-3
filename_template: 20260525-230000-virtual-cash-story-3.md
created_at: 2026-05-25T23:00:00+07:00
updated_at: 2026-05-25T23:45:00+07:00
status: completed
owner: engineering
tags: [plan, implementation, trading-simulator, wallet, session, privacy, us-03, story-3]
related_spec: docs/specs/20260525-201500-virtual-cash-balance.md
related_plans: [docs/plans/20260525-203000-virtual-cash-story-1.md, docs/plans/20260525-220000-virtual-cash-story-2.md]
prd_refs: [PRD §5.1 US-03, PRD §7.3, PRD §8.1]
tech_refs: [Tech §5.2.1, Tech §6, Tech §8.1, Tech §15.1, Tech §17.3]
db_refs: [DB §4.2 wallets, DB §5 invariants]
github:
  repo: tranvuongduy2003/trading-simulator
  epic_issue: 33
  story_issue_ids: [36]
  last_synced_at: 2026-05-25T23:00:00+07:00
search_index:
  keywords: [wallet privacy, session scope, BR-07, UNAUTHORIZED, 401, TanStack Query cache, EC-03, EC-04, cross-user, GetMyWallet, ICurrentUserAccessor, ProtectedRoute, useWalletQuery, removeQueries]
  bounded_contexts: [Trading]
  task_count: 5
---

# Implementation Plan: Virtual Cash Balance — Story 3

| Field | Value |
|-------|--------|
| Spec | `docs/specs/20260525-201500-virtual-cash-balance.md` (§2 Story 3) |
| GitHub story | [#36 — Only see my own balance when signed in](https://github.com/tranvuongduy2003/trading-simulator/issues/36) |
| Epic | [#33 — Virtual cash balance display (US-03)](https://github.com/tranvuongduy2003/trading-simulator/issues/33) |
| Depends on | Story 1–2 shipped (`useWalletQuery`, `VirtualCashCard`, `GetMyWalletQuery`, US-02 session) |
| Status | COMPLETED (automation); manual UI checklist pending operator |
| Tasks | 5 |
| Branch | `feature/virtual-cash-story-3` |
| Aspire impact | No |
| Schema impact | No |
| Test levels | API integration (new privacy scenarios) · Manual UI |
| ADRs required | None |
| GitHub | Synced 2026-05-25 — see §GitHub Links |

## Executive summary

Story 3 (US-03) ensures **wallet reads are private to the signed-in session**: `GET /api/wallet` returns only the caller’s row (BR-07, Tech §15.1), unauthenticated calls get **401** `UNAUTHORIZED`, and the React client must **not flash another user’s cached balances** when user B logs in after user A on the same browser (EC-04). The backend path is largely correct (`ICurrentUserAccessor`, `RequireAuthorization`, no `userId` query param). This plan **proves** ownership with new integration tests and **hardens** TanStack Query cache behavior on login/logout/session expiry so trading UI and the top-bar cash chip never render stale wallet numbers.

## Goals and non-goals

**Goals**

- G1: Authenticated `GET /api/wallet` returns `userId` matching the session user (happy path AC).
- G2: User B login after user A on the same cookie jar never surfaces A’s balances in UI (EC-04).
- G3: Unauthenticated `GET /api/wallet` → **401** with stable `code` `UNAUTHORIZED`; protected routes redirect to login.
- G4: Session expired mid-view → **401**, auth cleared, **no numeric balances** on trading view or top bar (EC-03).
- G5: Logout and login transitions **purge** wallet (and portfolio) query cache — not only `invalidateQueries`.

**Non-goals**

- NG1: Story 4 — refetch-on-focus / read-your-writes polish (#37).
- NG2: Story 1–2 display formatting, reserved breakdown emphasis.
- NG3: New `userId` query parameter, admin wallet APIs, or SignalR wallet push.
- NG4: Re-implementing US-02 login/logout/session store (reuse; only wallet-specific cache rules).
- NG5: Frontend unit/E2E framework; schema/matching/Redis changes.

## Traceability matrix

| Spec story | Plan task(s) | Test evidence |
|------------|--------------|---------------|
| Story 3 — userId matches session | Task 1 | `GetMyWallet_AfterLogin_UserIdMatchesSession`; existing register test |
| Story 3 — user B after user A (no stale cache) | Task 2, 3, 4 | `GetMyWallet_AfterSecondUserLogin_ReturnsSecondUserWalletOnly`; manual EC-04 |
| Story 3 — unauthenticated 401 | Task 1 | `GetMyWallet_WithoutSession_Returns401_UNAUTHORIZED` (extend existing test) |
| Story 3 — session expired, no balances | Task 4, 5 | `SessionPersistence_ExpiredSession_Returns401` (reuse); manual EC-03 |
| BR-07 session-scoped wallet | Task 1 | Handler uses `ICurrentUserAccessor` only — no code change expected |
| EC-03 session expired mid-view | Task 4, 5 | Manual + `UnauthorizedListener` / wallet `isError` |
| EC-04 same browser user switch | Task 2, 3 | Integration + manual |

## Architecture impact

```text
┌─────────────────────────────────────────────────────────────┐
│ Browser (single cookie jar)                                  │
│  Login/Logout ──► removeQueries(['wallet']) / clear on 401   │
│  useWalletQuery(['wallet', userId]) ──► GET /api/wallet        │
│  VirtualCashCard / WalletTopBarChip ──► hide $ if !match/401  │
└────────────────────────────┬────────────────────────────────┘
                             │ session cookie
                             ▼
┌─────────────────────────────────────────────────────────────┐
│ Api: WalletEndpoint.RequireAuthorization()                   │
│  → GetMyWalletQueryHandler(ICurrentUserAccessor → userId)    │
│  → WalletReadRepository.GetByUserIdAsync(sessionUserId)      │
└────────────────────────────┬────────────────────────────────┘
                             ▼
                    PostgreSQL wallets (pk user_id)
```

| Layer | Change summary |
|-------|----------------|
| Domain | **REUSE** — no change |
| Application | **REUSE** — `GetMyWalletQueryHandler` + `ICurrentUserAccessor` |
| Infrastructure | **REUSE** — `WalletReadRepository` filters by `userId` |
| Api | **MODIFY** — `WalletEndpoint` drops `RequireAuthorization()` so handler returns RFC 7807 `UNAUTHORIZED` (ASP.NET auth returned empty 401) |
| MatchingEngine | None |
| web/ | **MODIFY** auth transitions, `use-wallet-query`, trading/top-bar wallet display guards |
| AppHost | None |

## Data & migration plan

| Artifact | Action | DB reference |
|----------|--------|--------------|
| EF migration | **None** | — |
| Redis keys | **None** | — |
| Book recovery | N/A | — |

## Open questions

| # | Question | Source | Answer | Status |
|---|----------|--------|--------|--------|
| 1 | Is `invalidateQueries` on login enough for EC-04? | Code review | **No** — `invalidateQueries` keeps prior `data` during refetch; use `removeQueries` before session switch and user-scoped `queryKey` | ✅ Answered |
| 2 | Should wallet query key include `userId`? | TanStack best practice | **Yes** — `['wallet', userId]` prevents cache collision; `removeQueries({ queryKey: ['wallet'] })` still clears all wallet queries | ✅ Answered |
| 3 | Defense-in-depth UI guard if cache bug regresses? | Spec risk | **Yes** — do not render amounts unless `wallet.userId === authStore.userId` | ✅ Answered |

## Risks and mitigations

| Risk | Likelihood | Impact | Mitigation | Owner task |
|------|------------|--------|------------|------------|
| Stale wallet flash on login (EC-04) | M | H | `removeQueries` + user-scoped key + UI userId guard | Task 2, 3, 4 |
| `useWalletQuery` runs while logged out, repopulates cache | L | M | `enabled: status === 'authenticated'` | Task 3 |
| Integration test order-dependent shared DB | L | L | Unique emails per run; single client cookie sequence | Task 1 |
| SignalR interceptors still use `['wallet']` prefix | L | L | Partial key match still works; document in Task 2 | Task 2 |

## Prerequisites

- [x] US-01 / US-02 shipped (session, `GET /api/wallet`)
- [x] Virtual cash Story 1–2 automation on branches (or merged to `main`)
- [ ] Local Aspire for manual EC-03 / EC-04 checklist
- [x] Branch `feature/virtual-cash-story-3` from latest `main` (or Story 2 branch if not merged)

## File structure (planned)

```text
tests/TradingSimulator.Api.IntegrationTests/
  Users/GetMyWalletTests.cs                         MODIFY (privacy tests)

web/src/features/auth/
  clear-user-queries.ts                             CREATE (shared purge helper)
  login-form.tsx                                    MODIFY
  register-form.tsx                                 MODIFY
  use-logout.ts                                     REUSE (already removeQueries)

web/src/features/trading/
  hooks/use-wallet-query.ts                         MODIFY
  pages/trading-page.tsx                            MODIFY
  components/virtual-cash-card.tsx                  MODIFY (optional guard props)
  components/wallet-top-bar-chip.tsx                MODIFY

web/src/lib/signalr/interceptors.ts                 MODIFY (query key comment/prefix only if needed)

src/Application/Users/Queries/GetMyWalletQueryHandler.cs   REUSE
src/Api/Endpoints/WalletEndpoint.cs                          REUSE
```

## Authorization, session, and domain notes

- **Session model:** HttpOnly cookie; `SessionAuthenticationHandler` sets `ICurrentUserAccessor.UserId`.
- **Route protection:** `ProtectedRoute` + `useSession` bootstrap; unauthenticated → `/login`.
- **API:** `GET /api/wallet` has **no** `userId` route/query parameter — ownership from session only (spec §7).
- **BR-07:** Handler must never accept a client-supplied user id; repository lookup uses accessor only.
- **401 global path:** `unauthorizedResponseInterceptor` dispatches `api:unauthorized`; `UnauthorizedListener` clears `queryClient` and navigates to login with `session-expired` when previously authenticated.
- **Logout path:** `useLogout` already `removeQueries` for `wallet`, `portfolio`, `auth/session` — keep aligned with login/register.

## Progress tracker

### Task 1: Prove API wallet ownership and 401 contract

| Attribute | Value |
|-----------|--------|
| Spec story | Story 3 |
| Depends on | None |
| Estimated complexity | S |
| Parent story issue | #36 |

#### Objective

Automated tests document that wallet reads are session-bound: correct `userId` on success, **401** `UNAUTHORIZED` without session, and after user B logs in on the same client, wallet data belongs to B only (not A’s seeded balances).

#### Implementation notes

- Extend `GetMyWalletTests` — do not duplicate full register flows unnecessarily.
- Add helper to assert wallet problem `code` for 401 (mirror `LoginUserTestHelpers` pattern).
- **Two-user test:** register/login user A → seed distinct `total_balance` (e.g. 50_000) → logout → register/login user B → `GET /api/wallet` → assert `userId` is B and balances are B’s default 100k (not A’s 50k).
- **Login-without-logout variant (optional second fact):** login A → wallet → login B with same client → wallet `userId` is B (session replacement).
- Reuse `SeedWalletBalancesAsync` from existing `GetMyWalletTests`.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `tests/.../Users/GetMyWalletTests.cs` | New facts + 401 code assert |
| REUSE | `tests/.../Users/GetMyWalletTests.cs` | `SeedWalletBalancesAsync` |
| REUSE | `src/Application/Users/Queries/GetMyWalletQueryHandler.cs` | Ownership via accessor |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `GetMyWallet_WithoutSession_Returns401_UNAUTHORIZED` | `GetMyWalletTests.cs` |
| Integration | `GetMyWallet_AfterSecondUserLogin_ReturnsSecondUserWalletOnly` | `GetMyWalletTests.cs` |
| Integration | `GetMyWallet_AfterLogin_UserIdMatchesSession` (or fold into above) | `GetMyWalletTests.cs` |

#### Acceptance criteria

- [x] No session → **401** and problem `code` `UNAUTHORIZED`
- [x] User A seeded wallet ≠ user B wallet after B’s session established on same `HttpClient`
- [x] `userId` in 200 body matches registered/logged-in user
- [x] All new tests pass in Users Testcontainers collection

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | Tech §15.1, DB §4.2 |
| Async matching | N/A |
| PostgreSQL authoritative | Read path only |
| Redis projection | N/A |
| RFC 7807 errors | `UNAUTHORIZED` stable code |
| SignalR | N/A |
| Aspire | None |
| ADR needed? | No |

#### Risk

None — tests only; may expose existing backend bug if two-user test fails.

---

### Task 2: Purge wallet cache on auth user change

| Attribute | Value |
|-----------|--------|
| Spec story | Story 3 |
| Depends on | Task 1 (tests define expected behavior) |
| Estimated complexity | S |
| Parent story issue | #36 |

#### Objective

Login and register flows **remove** prior user’s wallet/portfolio cache before establishing the new session, matching logout behavior and spec §6 stale-UI handling.

#### Implementation notes

- **CREATE** `clearUserScopedQueries(queryClient)` — `removeQueries` for `['wallet']`, `['portfolio']`, and optionally `['auth', 'session']` when replacing user.
- **login-form.tsx:** call purge **before** `setSession` / wallet fetch; then `fetchQuery` session wallet; then navigate.
- **register-form.tsx:** purge before `setQueryData` / navigation (register always new user).
- Keep `useLogout.finalizeLogout` using same helper for one code path.
- Do **not** rely on `invalidateQueries` alone for user switch (EC-04).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `web/src/features/auth/clear-user-queries.ts` | Shared cache purge |
| MODIFY | `web/src/features/auth/login-form.tsx` | Purge on successful login |
| MODIFY | `web/src/features/auth/register-form.tsx` | Purge on successful register |
| MODIFY | `web/src/features/auth/use-logout.ts` | Call shared helper |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Login B after A — no flash of A’s amount | Aspire |
| Integration | Task 1 two-user test | `GetMyWalletTests.cs` |

#### Acceptance criteria

- [x] Successful login calls `removeQueries` for wallet (not only invalidate)
- [x] Successful register clears prior wallet cache before navigation
- [x] Logout still clears wallet cache (regression)

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | Spec §6 EC-04 |
| RFC 7807 | N/A |
| Aspire | None |
| ADR needed? | No |

#### Risk

Low — auth-only files; verify public routes still bootstrap session without infinite loader (`providers.tsx` guard).

---

### Task 3: User-scoped wallet query hook

| Attribute | Value |
|-----------|--------|
| Spec story | Story 3 |
| Depends on | Task 2 |
| Estimated complexity | S |
| Parent story issue | #36 |

#### Objective

`useWalletQuery` only runs for the authenticated user and keys cache by `userId`, so TanStack Query cannot serve user A’s wallet entry to user B’s session.

#### Implementation notes

- Read `userId` from `useAuthStore`.
- `queryKey: ['wallet', userId]` with `enabled: status === 'authenticated' && !!userId`.
- `staleTime` unchanged (30s).
- Update any `invalidateQueries`/`removeQueries` call sites to use prefix `['wallet']` (already partial-match friendly).
- **Do not** store wallet in Zustand (frontend rule).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `web/src/features/trading/hooks/use-wallet-query.ts` | Scoped key + enabled |
| MODIFY | `web/src/lib/signalr/interceptors.ts` | Confirm invalidation prefix still valid |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | React Query devtools / network: new key after login B | Aspire |

#### Acceptance criteria

- [x] Wallet query disabled when `authStatus !== 'authenticated'`
- [x] Query key includes current `userId`
- [x] No duplicate fetch storms on navigation

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | Tech §11 TanStack Query |
| ADR needed? | No |

#### Risk

None — isolated hook change.

---

### Task 4: Hide balances on 401 and userId mismatch

| Attribute | Value |
|-----------|--------|
| Spec story | Story 3 |
| Depends on | Task 3 |
| Estimated complexity | M |
| Parent story issue | #36 |

#### Objective

Trading view and top-bar chip **never show numeric balances** when wallet fetch fails with **401**, session is invalid, or API `userId` does not match the auth store (defense in depth).

#### Implementation notes

- Add small helper `canDisplayWallet(wallet, sessionUserId)` in `wallet-display.ts` or next to hook.
- **trading-page.tsx:** pass `wallet` to card only when `walletQuery.isSuccess && canDisplayWallet(...)`.
- **wallet-top-bar-chip.tsx:** same guard; keep skeleton on pending; “Unavailable” on error (no amounts).
- On **401**, `ApiError` + global listener should clear session; local UI must not render last successful `data` if status flipped to error — with Task 3 key purge this is unlikely; guard anyway.
- **401 on wallet while authenticated:** treat as error state (no tabular nums), rely on redirect from `UnauthorizedListener`.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `web/src/features/trading/wallet-display.ts` | `canDisplayWallet` helper |
| MODIFY | `web/src/features/trading/pages/trading-page.tsx` | Guarded render |
| MODIFY | `web/src/features/trading/components/wallet-top-bar-chip.tsx` | Guarded render |
| MODIFY | `web/src/features/trading/components/virtual-cash-card.tsx` | Optional: accept `showBalances` flag |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Expire session (revoke in DB or wait) — trading shows no $ | Aspire |
| Manual | Force 401 on wallet — redirect + no balances | Aspire |
| Integration | Reuse `SessionPersistence_ExpiredSession_Returns401` | existing |

#### Acceptance criteria

- [x] EC-03: expired session → no numeric balances on trading page or top bar (code guard; manual Aspire pending)
- [x] Wallet `userId` ≠ `authStore.userId` → card/chip show error/empty, not amounts
- [x] Protected route still redirects unauthenticated users to login

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | Spec §4a 401 flow |
| RFC 7807 | 401 handling |
| ADR needed? | No |

#### Risk

Low — UI-only; avoid blocking loading skeleton indefinitely.

---

### Task 5: Polish — regression smoke and manual privacy checklist

| Attribute | Value |
|-----------|--------|
| Spec story | Story 3 · Polish |
| Depends on | Tasks 1–4 |
| Estimated complexity | S |
| Parent story issue | #36 |

#### Objective

Confirm no regressions to Stories 1–2 and complete manual verification of EC-03 and EC-04 on Aspire.

#### Implementation notes

- Run Users Testcontainers suite (wallet + session tests).
- Run `yarn --cwd web build`.
- Run `yarn --cwd web api:verify` if OpenAPI touched.
- Execute manual checklist below; comment results on GitHub #36 when done.

#### Manual UI checklist

1. **EC-04:** Register/login user A → note available cash → logout → register/login user B → trading view shows B’s balance (typically $100,000.00), never A’s amount.
2. **EC-04 variant:** Login A → login B without logout (if supported) → same as above.
3. **EC-03:** While on trading view, revoke session (logout other tab or DB revoke) → refresh or trigger wallet fetch → redirect login, session-expired message, **no** dollar amounts visible during transition.
4. **Unauthenticated:** Open `/trading` logged out → redirect to login; direct `GET /api/wallet` returns 401 (browser devtools).
5. **Regression:** Story 1 skeleton/error paths; Story 2 reserved breakdown still correct.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `docs/memory/current-status.md` | After `/build` completes |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | Full Users collection green | CI/local Docker |
| Manual | Checklist above | Aspire |

#### Acceptance criteria

- [x] All Task 1–4 acceptance criteria still pass
- [ ] Manual checklist steps 1–5 signed off (operator on Aspire)
- [x] No regression in `GetMyWalletTests` (including Story 2 reserved cases)

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| Aspire | `aspire run` for manual |
| ADR needed? | No |

#### Risk

None — verification only.

## Reference files

| File | Why open it |
|------|-------------|
| `src/Application/Users/Queries/GetMyWalletQueryHandler.cs` | BR-07 ownership implementation |
| `src/Api/Endpoints/WalletEndpoint.cs` | `RequireAuthorization`, route shape |
| `tests/.../Users/GetMyWalletTests.cs` | Extend privacy tests |
| `tests/.../Users/LogoutUserTests.cs` | Logout → wallet 401 pattern |
| `tests/.../Users/SessionPersistenceTests.cs` | Expired session 401 |
| `web/src/features/auth/login-form.tsx` | Cache purge insertion point |
| `web/src/features/auth/use-logout.ts` | Existing `removeQueries` |
| `web/src/app/providers.tsx` | `UnauthorizedListener` |
| `web/src/hooks/use-session.ts` | Session bootstrap + 401 clear |
| `docs/plans/20260525-203000-virtual-cash-story-1.md` | Prior wallet UI patterns |

## Implementation details (for /build)

### Backend

- `GetMyWalletQueryHandler` returns `Error.Unauthorized("UNAUTHORIZED", ...)` when `currentUserAccessor.UserId` is null.
- `WalletEndpoint` maps via `ToHttpResult()` → 401 problem+json.
- **Deviation:** removed `.RequireAuthorization()` on `GET /api/wallet` so unauthenticated calls reach the handler (middleware-only 401 had no problem body / `UNAUTHORIZED` code). Session still required via `ICurrentUserAccessor`.
- If Task 1 fails on two-user test, inspect `LoginUserCommandHandler` session replacement and `WalletReadRepository` filter — not Story 3 UI.

### Frontend cache contract

| Event | Query action |
|-------|----------------|
| Logout success | `removeQueries(['wallet'])`, `['portfolio']`, `['auth','session']` |
| Login success | **removeQueries** before new session, then fetch |
| Register success | **removeQueries** before seeding session cache |
| 401 while authenticated | `queryClient.clear()` (existing) |
| Wallet query key | `['wallet', userId]` |

### UI display rule

```text
show numeric balances IFF
  walletQuery.isSuccess
  AND wallet.userId === authStore.userId
  AND NOT (walletQuery.error is 401)
```

### Error codes

- Wallet unauthenticated: **401**, `code`: `UNAUTHORIZED` (spec §4b).

## Verification matrix (plan-level)

| Spec AC | Verified by |
|---------|-------------|
| Authenticated wallet `userId` matches account | Task 1 integration + register test |
| User B after A — no A cached balances | Task 1 API + Task 2–4 UI + manual EC-04 |
| Unauthenticated → 401 + login redirect | Task 1 + `ProtectedRoute` + manual |
| Session expired → 401, clear auth, no balances | Task 4 + `SessionPersistenceTests` + manual EC-03 |
| BR-07 | Task 1 + handler review |
| No `userId` query param | Code review `WalletEndpoint` |

## Rollback / recovery

- **Code:** revert branch commits.
- **DB:** N/A.
- **Redis:** N/A.

## Deferred work (Plan B)

- Story 4: refetch-on-focus and read-your-writes (#37).
- E2E Playwright for EC-04 flash detection.
- Structured audit log when wallet `userId` mismatch detected client-side (debug-only).

## GitHub Links

| Local ref | Issue # | Type | Title | URL |
|-----------|---------|------|-------|-----|
| spec.Story 3 | 36 | Story | US-03 / Story 3: Only see my own balance when signed in | https://github.com/tranvuongduy2003/trading-simulator/issues/36 |
| epic | 33 | Epic | Virtual cash balance display (US-03) | https://github.com/tranvuongduy2003/trading-simulator/issues/33 |


---

<a id="source-20260525-240000-virtual-cash-story-4md"></a>

## Source 13 of 18: `docs/plans/20260525-240000-virtual-cash-story-4.md`

---
artifact_type: plan
artifact_version: 1
id: plan-20260525-240000-virtual-cash-story-4
title: Virtual Cash Balance — Story 4 (Trust after login and refresh)
slug: virtual-cash-story-4
filename_template: 20260525-240000-virtual-cash-story-4.md
created_at: 2026-05-25T24:00:00+07:00
updated_at: 2026-05-25T25:30:00+07:00
status: completed
owner: engineering
tags: [plan, implementation, trading-simulator, wallet, refetch, read-your-writes, us-03, story-4]
related_spec: docs/specs/20260525-201500-virtual-cash-balance.md
related_plans: [docs/plans/20260525-203000-virtual-cash-story-1.md, docs/plans/20260525-220000-virtual-cash-story-2.md, docs/plans/20260525-230000-virtual-cash-story-3.md]
prd_refs: [PRD §5.1 US-03, PRD §6.1 FR-1.2, PRD §6.6 FR-6.2]
tech_refs: [Tech §5.2.1, Tech §6, Tech §8.1, Tech §15.1, Tech §17.3]
db_refs: [DB §4.2 wallets, DB §5 invariants, DB §8 BR-08]
github:
  repo: tranvuongduy2003/trading-simulator
  epic_issue: 33
  story_issue_ids: [37]
  last_synced_at: 2026-05-25T24:00:00+07:00
search_index:
  keywords: [wallet refetch, read-your-writes, refetchOnWindowFocus, staleTime, login prefetch, browser refresh, PostgreSQL authoritative, BR-08, BR-04, EC-07, GetMyWallet, useWalletQuery, prefetchWallet]
  bounded_contexts: [Trading]
  task_count: 5
---

# Implementation Plan: Virtual Cash Balance — Story 4

| Field | Value |
|-------|--------|
| Spec | `docs/specs/20260525-201500-virtual-cash-balance.md` (§2 Story 4) |
| GitHub story | [#37 — Trust balances after login and refresh](https://github.com/tranvuongduy2003/trading-simulator/issues/37) |
| Epic | [#33 — Virtual cash balance display (US-03)](https://github.com/tranvuongduy2003/trading-simulator/issues/33) |
| Depends on | Stories 1–3 shipped (`useWalletQuery`, session privacy, `GetMyWalletTests`, auth flows) |
| Status | COMPLETED (automation) — manual checklist pending operator |
| Tasks | 5 |
| Branch | `feature/virtual-cash-story-4` |
| Aspire impact | No |
| Schema impact | No |
| Test levels | API integration (new read-your-writes / refetch scenarios) · Manual UI |
| ADRs required | None |
| GitHub | Synced 2026-05-25 — see §GitHub Links |

## Executive summary

Story 4 (US-03) closes the **trust loop**: after login, browser refresh, or tab focus, displayed cash must match the latest **PostgreSQL** wallet row from `GET /api/wallet` (BR-08), including reserved amounts when seeded (BR-04). Stories 1–3 delivered display, breakdown, and session privacy; the main gap is **client refetch policy** — `QueryClient` sets `refetchOnWindowFocus: false` globally and `useWalletQuery` uses `staleTime: 30_000`, so a focused tab can keep stale balances for up to 30s (violates EC-07 / stale-tab AC). Login seeds `['auth', 'session']` but not `['wallet', userId]`, adding an extra round-trip before the trading view shows cash. This plan adds **integration proof** of login→wallet and second-fetch-after-DB-update, a **shared post-auth wallet prefetch**, **per-query refetch-on-focus with `staleTime: 0`**, and a manual checklist for refresh and focus on Aspire.

## Goals and non-goals

**Goals**

- G1: After successful login, trading view shows wallet within **2 s** matching PostgreSQL (read-your-writes).
- G2: Browser refresh with valid session re-loads the same balances (session probe + wallet fetch).
- G3: Window focus and remount refetch wallet so stale-tab balances update (EC-07).
- G4: Integration test: login → `GET /api/wallet` **200** with correct balances; second GET after DB seed returns updated figures.
- G5: **500** path unchanged — no partial wallet row (reuse Story 1 test + UI).

**Non-goals**

- NG1: SignalR wallet push (spec §6, issue deferred).
- NG2: Order-place/cancel mutation invalidation (no order epic yet; document hook point in `interceptors.ts`).
- NG3: Portfolio reset (US-04 / EC-08) — only note refetch compatibility.
- NG4: Backend handler or schema changes (read path already authoritative).
- NG5: Frontend unit/E2E framework; lowering global `refetchOnWindowFocus` for non-wallet queries.

## Traceability matrix

| Spec story | Plan task(s) | Test evidence |
|------------|--------------|---------------|
| Story 4 — login → wallet within 2s, PG match | Task 2, 3 | `GetMyWallet_AfterLogin_ReturnsSeededBalances`; manual step 1 |
| Story 4 — browser refresh same balances | Task 3, 5 | Manual step 2; `useSession` + `useWalletQuery` on remount |
| Story 4 — focus/refetch latest reserved | Task 3 | `GetMyWallet_SecondFetchAfterDbUpdate_ReturnsLatestBalances`; manual step 3 |
| Story 4 — stale tab updates on focus | Task 3 | Manual step 3; `staleTime: 0` + `refetchOnWindowFocus: true` on wallet |
| Story 4 — PG unavailable **500**, no partial UI | Task 1, 5 | Reuse `GetMyWallet_WhenReadFails_Returns500_INTERNAL_ERROR`; manual regression |
| BR-08 PostgreSQL authoritative | Task 1 | Second-fetch-after-seed test |
| BR-04 reserved refetch after orders | Task 3 | Per-query refetch; future `invalidateQueries` on order mutations |
| EC-07 slightly stale until refetch | Task 3 | Documented MVP behavior |
| EC-08 portfolio reset | — | Deferred; refetch-on-focus compatible |

## Architecture impact

```text
┌──────────────────────────────────────────────────────────────────┐
│ Auth success (login / register)                                   │
│  clearUserScopedQueries → prefetchWallet(['wallet', userId])      │
│  setSession → navigate /trading                                   │
└────────────────────────────┬─────────────────────────────────────┘
                             │
┌────────────────────────────▼─────────────────────────────────────┐
│ ProtectedRoute: useSession() → GET /api/wallet (session bootstrap)  │
│ AppLayout / TradingPage: useWalletQuery()                           │
│   staleTime: 0 · refetchOnWindowFocus: true · enabled if authed    │
└────────────────────────────┬─────────────────────────────────────┘
                             │ session cookie
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│ Api GET /api/wallet → GetMyWalletQuery → WalletReadRepository     │
│                      → PostgreSQL wallets (authoritative)         │
└──────────────────────────────────────────────────────────────────┘
```

| Layer | Change summary |
|-------|----------------|
| Domain | **REUSE** — no change |
| Application | **REUSE** — `GetMyWalletQueryHandler` |
| Infrastructure | **REUSE** — `WalletReadRepository` |
| Api | **REUSE** — `WalletEndpoint`; optional comment only |
| MatchingEngine | None |
| web/ | **MODIFY** — `use-wallet-query`, auth prefetch helper, `login-form`, `register-form` |
| AppHost | None |

## Data & migration plan

| Artifact | Action | DB reference |
|----------|--------|--------------|
| EF migration | **None** | — |
| Redis keys | **None** | — |
| Book recovery | N/A | — |

## Open questions

| # | Question | Source | Answer | Status |
|---|----------|--------|--------|--------|
| 1 | Keep global `refetchOnWindowFocus: false` and override only wallet? | code review | **Yes** — avoids refetch storms on market/portfolio placeholders | ✅ |
| 2 | Is `staleTime: 0` on wallet too aggressive for MVP? | spec §6 EC-07 | Acceptable; wallet reads are cheap; focus refetch is the product requirement | ✅ |
| 3 | Should login block navigation until wallet prefetch completes? | spec 2s SLA | **Yes** — extend existing login `fetchQuery` pattern to wallet key before `navigate` | ✅ |

## Risks and mitigations

| Risk | Likelihood | Impact | Mitigation | Owner task |
|------|------------|--------|------------|------------|
| Extra wallet GET on every tab focus | M | L | Single lightweight read; no SignalR yet | Task 3 |
| Login prefetch races with `clearUserScopedQueries` | L | M | Clear first, then `fetchQuery` with new `userId` | Task 2 |
| Regression: Story 3 cross-user flash | L | H | Keep `removeQueries` on auth change; prefetch only after `setSession` userId known | Task 2, 5 |
| Session query (`staleTime: 60s`) hides expired session on focus | M | M | Out of scope for Story 4; wallet 401 still triggers `UnauthorizedListener` | — |

## Prerequisites

- [x] Spec Story 4 defined (§2)
- [x] Stories 1–3 automation on feature branches (wallet UI, reserved breakdown, cache privacy)
- [ ] Aspire local stack for manual checklist
- [x] `GetMyWalletTests` baseline green (Docker Testcontainers)

## File structure (planned)

```text
tests/Api.IntegrationTests/Users/
  GetMyWalletTests.cs                    MODIFY — Story 4 refetch / login balance tests

web/src/features/trading/hooks/
  use-wallet-query.ts                    MODIFY — refetch policy

web/src/features/auth/
  prefetch-wallet.ts                     CREATE — shared prefetch + cache seed
  login-form.tsx                         MODIFY — prefetch wallet query key
  register-form.tsx                      MODIFY — seed ['wallet', userId] from registration

docs/memory/current-status.md            MODIFY — after /build
```

## Authorization, session, and domain notes

- **Session model:** Cookie session; `ProtectedRoute` bootstraps via `useSession` → `GET /api/wallet`.
- **Read-your-writes:** Registration creates wallet in same transaction as user; login must see committed row — integration tests use real PostgreSQL.
- **BR-08:** UI never recomputes `available` from total − reserved; always display API fields (`normalizeWallet`).
- **BR-04:** Until order placement ships, prove refetch fidelity via seeded `reserved_balance` + second GET (and focus refetch manual).

## Progress tracker

### Task 1: Integration tests — login wallet balances and refetch after DB update

| Attribute | Value |
|-----------|--------|
| Spec story | Story 4 |
| Depends on | None |
| Estimated complexity | M |
| Parent story issue | #37 |

#### Objective

Automate API-level proof that login returns wallet **200** with PostgreSQL-aligned balances, and a subsequent `GET /api/wallet` after `ExecuteUpdate` on `wallets` returns the updated reserved/available breakdown.

#### Implementation notes

- Add `GetMyWallet_AfterLogin_ReturnsSeededBalances`: register → logout → login → seed 50k/10k reserved → GET wallet → assert 40k available.
- Add `GetMyWallet_SecondFetchAfterDbUpdate_ReturnsLatestBalances`: authenticated GET → seed different balances → second GET → assert new values (simulates refetch after stale tab).
- Reuse `SeedWalletBalancesAsync` from existing tests.
- Keep `GetMyWallet_WhenReadFails_Returns500_INTERNAL_ERROR` as regression anchor.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `tests/Api.IntegrationTests/Users/GetMyWalletTests.cs` | New Story 4 scenarios |
| REUSE | `tests/.../IntegrationTestFixture.cs` | Testcontainers client |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `GetMyWallet_AfterLogin_ReturnsSeededBalances` | `GetMyWalletTests.cs` |
| Integration | `GetMyWallet_SecondFetchAfterDbUpdate_ReturnsLatestBalances` | `GetMyWalletTests.cs` |
| Integration | Existing 500 / login userId tests still pass | `GetMyWalletTests.cs` |

#### Acceptance criteria

- [x] New tests pass in Docker Testcontainers suite
- [x] Login → wallet **200** asserts monetary fields, not only `userId`
- [x] Second fetch reflects DB update (BR-08)

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | US-03, Tech §6, DB §4.2 |
| PostgreSQL authoritative | Seed via EF `ExecuteUpdate`, read via API |
| RFC 7807 | N/A for 200 paths |
| Aspire | None |
| ADR needed? | No |

#### Risk

None — test-only slice.

---

### Task 2: Post-auth wallet prefetch and cache seeding

| Attribute | Value |
|-----------|--------|
| Spec story | Story 4 |
| Depends on | Task 1 (optional parallel) |
| Estimated complexity | M |
| Parent story issue | #37 |

#### Objective

After login or register, warm `['wallet', userId]` in TanStack Query before navigating to trading so cash appears immediately and matches the server response.

#### Implementation notes

- Create `prefetchWalletQuery(queryClient, userId)` calling `authApi.getWallet` + `normalizeWallet`, `queryKey: ['wallet', userId]`.
- **Login:** after `clearUserScopedQueries`, `await prefetchWalletQuery` (and keep session `fetchQuery` or consolidate to one wallet fetch that sets both session + wallet keys).
- **Register:** after `setSession`, `setQueryData(['wallet', userId], normalizeWallet(registrationWallet))` in addition to existing session cache seed.
- Do not prefetch before `userId` is known (Story 3 EC-04).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `web/src/features/auth/prefetch-wallet.ts` | Shared prefetch/seed |
| MODIFY | `web/src/features/auth/login-form.tsx` | Prefetch wallet before navigate |
| MODIFY | `web/src/features/auth/register-form.tsx` | Seed wallet query cache |
| REUSE | `web/src/features/auth/clear-user-queries.ts` | Clear before prefetch |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | Task 1 tests | API |
| Manual | Login → trading shows balance without long skeleton | Aspire |

#### Acceptance criteria

- [x] Login success path populates `['wallet', userId]` before `navigate(paths.trading)`
- [x] Register seeds wallet cache from `response.wallet` (no `$0` flash)
- [x] Story 3 `clearUserScopedQueries` still runs first on auth transitions

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| Async matching | N/A |
| Redis projection | N/A |
| ADR needed? | No |

#### Risk

Low — ensure prefetch errors surface like today (login root error).

---

### Task 3: Wallet query refetch policy (focus and mount)

| Attribute | Value |
|-----------|--------|
| Spec story | Story 4 |
| Depends on | Task 2 |
| Estimated complexity | S |
| Parent story issue | #37 |

#### Objective

Enable refetch-on-focus and immediate staleness for wallet only, so EC-07 stale reads correct on tab focus without enabling global refetch storms.

#### Implementation notes

- In `useWalletQuery`, set:
  - `staleTime: 0`
  - `refetchOnWindowFocus: true` (overrides `providers.tsx` default `false`)
  - Keep `enabled: authStatus === 'authenticated' && userId !== null`
- Leave global `QueryClient` defaults unchanged for portfolio/market placeholders.
- Document in file comment: order mutations should `invalidateQueries({ queryKey: ['wallet'] })` when order epic ships (already in `interceptors.ts`).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `web/src/features/trading/hooks/use-wallet-query.ts` | Refetch policy |
| REUSE | `web/src/app/providers.tsx` | Global defaults stay |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Focus tab after seeding reserved in DB → UI updates | Aspire + devtools |

#### Acceptance criteria

- [x] Focusing browser tab triggers network `GET /api/wallet` when authenticated
- [x] Wallet refetches on trading page remount after full page refresh
- [x] No change to global `refetchOnWindowFocus: false` for other queries

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| SignalR | Deferred; invalidation hook exists |
| ADR needed? | No |

#### Risk

Low — slightly more API traffic on focus; acceptable for MVP.

---

### Task 4: Read-your-writes verification on session refresh path

| Attribute | Value |
|-----------|--------|
| Spec story | Story 4 |
| Depends on | Task 3 |
| Estimated complexity | S |
| Parent story issue | #37 |

#### Objective

Confirm and document that a full browser refresh on `/trading` re-runs session bootstrap and wallet fetch without showing stale in-memory Zustand balances (auth store is not persisted).

#### Implementation notes

- **No code required** if manual verification passes: `auth-store` resets to `unknown` on reload → `ProtectedRoute` skeleton → `useSession` + `useWalletQuery` both fetch `/api/wallet`.
- If gap found: ensure `useWalletQuery` is not disabled while `authStatus === 'unknown'` during bootstrap — may need `enabled` tied to session success only (evaluate during `/build`; avoid showing balances during `unknown`).
- Optional: add one-line comment on `ProtectedRoute` referencing Story 4 refresh contract.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `web/src/app/routes/protected-route.tsx` | Comment only if helpful |
| MODIFY | `web/src/hooks/use-session.ts` | Only if refresh gap found |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Hard refresh on trading → same balances | Aspire |

#### Acceptance criteria

- [x] Browser refresh with valid session shows same balances after load completes
- [x] No persisted Zustand wallet amounts (only server state)

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| FR-1.2 session persistence | Cookie survives refresh |
| ADR needed? | No |

#### Risk

None — verification-first task.

---

### Task 5: Polish — regression and manual trust checklist

| Attribute | Value |
|-----------|--------|
| Spec story | Story 4 · Polish |
| Depends on | Tasks 1–4 |
| Estimated complexity | S |
| Parent story issue | #37 |

#### Objective

Run automated regression and manual steps proving trust AC; update memory artifacts.

#### Manual UI checklist

1. **Login read-your-writes:** Login as user with known balance (new user $100k) → trading view shows correct available within ~2s without second login.
2. **Browser refresh:** On `/trading`, hard refresh (F5) → same balances after load.
3. **Focus refetch:** With devtools open, seed reserved via integration test DB or second browser tab not available — use Postgres MCP: update `reserved_balance` for your user → focus tab → wallet UI updates to match `GET /api/wallet`.
4. **Stale tab:** Open trading, change wallet in DB (reserved 5k), switch away 1 min, focus tab → balances update (not stuck at $100k).
5. **500 regression:** Force wallet read failure (or use test env) → error copy, no dollar amounts (Story 1).
6. **Story 3 regression:** Logout/login different users → no cross-user balances.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `docs/memory/current-status.md` | Post-build status |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | Full `GetMyWalletTests` + Users collection | Docker |
| Manual | Checklist 1–6 | Aspire |

#### Acceptance criteria

- [x] All Task 1–4 acceptance criteria pass
- [ ] Manual checklist 1–6 signed off (operator on Aspire)
- [x] `yarn --cwd web build` green

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| Aspire | `aspire run` for manual |
| ADR needed? | No |

#### Risk

None.

## Reference files

| File | Why open it |
|------|-------------|
| `web/src/features/trading/hooks/use-wallet-query.ts` | Refetch policy change |
| `web/src/app/providers.tsx` | Global `refetchOnWindowFocus: false` |
| `web/src/features/auth/login-form.tsx` | Post-login prefetch |
| `web/src/features/auth/register-form.tsx` | Registration wallet seed |
| `tests/.../Users/GetMyWalletTests.cs` | Extend Story 4 tests |
| `docs/plans/20260525-230000-virtual-cash-story-3.md` | Cache contract table |
| `docs/specs/20260525-201500-virtual-cash-balance.md` | Story 4 AC |

## Implementation details (for /build)

### TanStack Query contract (target state)

| Query key | staleTime | refetchOnWindowFocus | When enabled |
|-----------|-----------|----------------------|--------------|
| `['auth', 'session']` | 60_000 (unchanged) | false (global) | `authStatus !== 'unauthenticated'` |
| `['wallet', userId]` | **0** | **true** | authenticated + `userId` set |

### Post-auth sequence (login)

```text
clearUserScopedQueries()
→ fetchQuery(['auth','session'], getWallet)  // existing session probe
→ fetchQuery(['wallet', userId], getWallet)   // NEW — warm trading UI
→ setSession({ userId, username })
→ navigate(/trading)
```

### Post-auth sequence (register)

```text
clearUserScopedQueries()
→ setSession(...)
→ setQueryData(['auth','session'], walletShape)
→ setQueryData(['wallet', userId], normalizeWallet(response.wallet))  // NEW
→ navigate(/trading)
```

### Future order epic

When place/cancel ships: `queryClient.invalidateQueries({ queryKey: ['wallet'] })` in mutation `onSuccess` (in addition to focus refetch). SignalR `BalanceUpdated` already invalidates in `interceptors.ts`.

## Verification matrix (plan-level)

| Spec AC | Verified by |
|---------|-------------|
| Login → wallet within 2s, PG match | Task 2 + manual 1 + `GetMyWallet_AfterLogin_ReturnsSeededBalances` |
| Browser refresh same balances | Task 4 manual 2 |
| Focus/refetch latest reserved | Task 3 + manual 3–4 + `GetMyWallet_SecondFetchAfterDbUpdate_*` |
| Stale tab updates | Task 3 manual 4 |
| PG down → 500, no partial UI | Task 1 reuse + manual 5 |
| BR-08 / BR-04 | Task 1 integration |

## Rollback / recovery

- **Code:** revert branch commits.
- **DB:** N/A.
- **Redis:** N/A.

## Deferred work (Plan B)

- SignalR wallet push for sub-second updates (spec §13 Q2).
- `invalidateQueries` on order mutations when order epic lands.
- Playwright timing test for 2s login→wallet SLA.
- Lower `useSession` staleTime if session expiry on focus becomes a product requirement.

## GitHub Links

| Local ref | Issue # | Type | Title | URL |
|-----------|---------|------|-------|-----|
| spec.Story 4 | 37 | Story | US-03 / Story 4: Trust balances after login and refresh | https://github.com/tranvuongduy2003/trading-simulator/issues/37 |
| epic | 33 | Epic | Virtual cash balance display (US-03) | https://github.com/tranvuongduy2003/trading-simulator/issues/33 |


---

<a id="source-20260525-260000-portfolio-reset-story-1md"></a>

## Source 14 of 18: `docs/plans/20260525-260000-portfolio-reset-story-1.md`

---
artifact_type: plan
artifact_version: 1
id: plan-20260525-260000-portfolio-reset-story-1
title: Portfolio Reset — Story 1 (Confirm before resetting)
slug: portfolio-reset-story-1
filename_template: 20260525-260000-portfolio-reset-story-1.md
created_at: 2026-05-25T26:00:00+07:00
updated_at: 2026-05-25T26:00:00+07:00
status: completed
owner: engineering
tags: [plan, implementation, trading-simulator, portfolio-reset, confirmation, us-04, story-1]
related_spec: docs/specs/20260525-251500-portfolio-reset.md
related_plans: []
prd_refs: [PRD §5.1 US-04, PRD §6.1 FR-1.4, PRD §7.4, PRD §8.1]
tech_refs: [Tech §6 ResetPortfolioCommand, Tech §8.1 Portfolio endpoints, Tech §15.1, Tech §16 Trading:PortfolioResetCooldownMinutes]
db_refs: [DB §4.10 portfolio_resets]
github:
  repo: tranvuongduy2003/trading-simulator
  epic_issue: 43
  story_issue_ids: [44]
  last_synced_at: 2026-05-25T26:00:00+07:00
search_index:
  keywords: [portfolio reset, confirmation dialog, POST api portfolio reset, ResetPortfolioCommand, alert-dialog, UserMenu, double-submit, RESET_IN_PROGRESS, nextEligibleAt, contract stub, US-04 story-1]
  bounded_contexts: [Trading]
  task_count: 5
---

# Implementation Plan: Portfolio Reset — Story 1

| Field | Value |
|-------|--------|
| Spec | `docs/specs/20260525-251500-portfolio-reset.md` (§2 Story 1) |
| GitHub story | [#44 — Confirm before resetting](https://github.com/tranvuongduy2003/trading-simulator/issues/44) |
| Epic | [#43 — Portfolio reset (US-04)](https://github.com/tranvuongduy2003/trading-simulator/issues/43) |
| Depends on | US-01 registration, US-02 login/session, US-03 wallet display (`useWalletQuery`, `AppLayout` user menu) — **already shipped** |
| Status | COMPLETE (automation) — manual UI checklist pending operator |
| Tasks | 5 |
| Branch | `feature/portfolio-reset-story-1` |
| Aspire impact | No |
| Schema impact | No |
| Test levels | API integration (auth + response contract) · Manual UI |
| ADRs required | ADR-005 (Story 1 contract stub — see below) |
| GitHub | Synced 2026-05-25 — see §GitHub Links |

## Executive summary

Story 1 (US-04) delivers the **destructive-action guardrail**: an authenticated user opens **Reset portfolio** from the account menu, reads a confirmation dialog listing all consequences, and only on confirm calls `POST /api/portfolio/reset` with loading on the confirm button. Cancel/dismiss must not hit the API. Success closes the dialog, shows success copy, and disables the menu entry until `nextEligibleAt` from the response (full server cooldown enforcement is Story 4; wallet/order mutations are Stories 2–3). The backend for this story is intentionally a **contract stub**—it returns the spec-shaped **200** body from the current wallet read and computed timestamps **without** writing `portfolio_resets` or changing balances, so Story 1 can be tested and merged before the atomic reset transaction lands in Story 2.

## Goals and non-goals

**Goals**

- G1: **Reset portfolio** entry in `AppLayout` user menu on Trading / Portfolio / Orders routes (≤2 clicks from trading view).
- G2: Confirmation dialog lists $100k cash, zero AAPL holdings, open orders cancelled, personal history cleared, 24h cooldown (copy only in Story 1).
- G3: Confirm triggers `POST /api/portfolio/reset` with session cookie, loading/disabled confirm until response (≤2 s local).
- G4: Cancel, Escape, or overlay dismiss → no POST; balances unchanged on refetch.
- G5: Double-submit → client disables confirm; optional server **409** `RESET_IN_PROGRESS` via in-flight guard.
- G6: Integration tests: **401** without session, **200** with session and response shape (`resetAt`, `nextEligibleAt`, `wallet`).

**Non-goals**

- NG1: PostgreSQL reset transaction, `portfolio_resets` insert, order cancel, holdings clear (Stories 2–3).
- NG2: Server cooldown **422** `RESET_COOLDOWN_ACTIVE` (Story 4).
- NG3: TanStack invalidation of wallet/portfolio/orders/trades (Story 5).
- NG4: Matching engine / Redis book updates (Stories 2–3).
- NG5: SignalR notifications on reset.
- NG6: Dedicated `GET /api/portfolio/reset/eligibility` (Story 4 may add; Story 1 uses `nextEligibleAt` from POST success).
- NG7: Domain `Wallet.Reset` / `Portfolio.ClearHoldings` unit tests (Story 2).

## Traceability matrix

| Spec story | Plan task(s) | Test evidence |
|------------|--------------|---------------|
| Story 1 — menu → confirmation dialog | Task 1, 2 | Manual: consequences list visible |
| Story 1 — confirm → POST within 2s + loading | Task 1, 3 | Manual; `ResetPortfolio_WithSession_Returns200_WithContractShape` |
| Story 1 — success → toast + disable reset until eligible | Task 4 | Manual; client uses `nextEligibleAt` |
| Story 1 — cancel/dismiss → no API | Task 2 | Manual; no POST in network tab |
| Story 1 — double-click → single in-flight | Task 3 | Manual; optional `ResetPortfolio_Concurrent_Returns409` |
| BR-01 user-initiated only | Task 1 | Authenticated POST only |
| BR-05 session valid after reset | Task 1 | No logout on success (stub does not touch session) |
| EC-07 session expires on confirm → 401 | Task 3 | `ResetPortfolio_WithoutSession_Returns401`; manual |
| `portfolio_resets` row on success | — | **Deferred to Story 2** (stub writes nothing) |

## Architecture impact

```text
┌─────────────────────────────────────────────────────────────────┐
│ AppLayout / UserMenu                                             │
│  "Reset portfolio" → AlertDialog (consequences)                  │
│  Confirm → useResetPortfolioMutation → POST /api/portfolio/reset │
│  Success → toast + disable menu until nextEligibleAt (client)  │
└────────────────────────────┬────────────────────────────────────┘
                             │ session cookie
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│ Api PortfolioEndpoint POST                                       │
│  → ResetPortfolioCommand → ResetPortfolioCommandHandler (STUB)   │
│     reads wallet via IWalletReadRepository                       │
│     returns 200 { resetAt, nextEligibleAt, wallet } — no UoW     │
└─────────────────────────────────────────────────────────────────┘
```

| Layer | Change summary |
|-------|----------------|
| Domain | **None** in Story 1 (reset methods added in Story 2) |
| Application | **CREATE** `ResetPortfolioCommand`, stub `ResetPortfolioCommandHandler`, validator (empty body), `PortfolioResetErrors` |
| Infrastructure | **REUSE** `IWalletReadRepository`; optional `IResetInFlightGuard` (in-memory singleton for 409) |
| Contracts | **CREATE** `PortfolioResetResponse` (+ nested wallet snapshot if not reusing `WalletResponse`) |
| Api | **MODIFY** `PortfolioEndpoint` — add POST; **MODIFY** `ApiErrorCodes` / problem mapping for new codes |
| MatchingEngine | None |
| web/ | **CREATE** `features/portfolio-reset/` (dialog, mutation, api); **MODIFY** `app-layout.tsx` UserMenu; add shadcn `alert-dialog` |
| AppHost | None |

## Data & migration plan

| Artifact | Action | DB reference |
|----------|--------|--------------|
| EF migration | **None** | `portfolio_resets` table already exists (DB §4.10) |
| Redis keys | **None** | — |
| Book recovery | N/A | — |
| Story 1 writes | **None** | Spec: insert `portfolio_resets` on success starts Story 2+ |

## Open questions

| # | Question | Source | Answer | Status |
|---|----------|--------|--------|--------|
| 1 | Trade history cleared without deleting `trades` rows? | Spec §13 Q1 | Soft cutoff on reads — implement in Story 3; document ADR then | ⏳ Deferred (Story 3 plan) |
| 2 | Empty order history after reset? | Spec §13 Q2 | Yes — Story 3 read model | ⏳ Deferred |
| 3 | **200 + body** vs **204**? | Spec §13 Q3 | **200** with `resetAt`, `nextEligibleAt`, `wallet` (matches §4b assumption) | ✅ Answered (this plan) |
| 4 | Eligibility GET vs infer from POST? | Spec §13 Q4 | Story 1: cache `nextEligibleAt` from last success; Story 4 adds server cooldown + disabled UI without POST | ✅ Answered (this plan) |
| 5 | Story 1 stub acceptable for `/build`? | Code review | Yes — ADR-005; manual checklist notes wallet unchanged until Story 2 | ✅ Answered (this plan) |

## Risks and mitigations

| Risk | Likelihood | Impact | Mitigation | Owner task |
|------|------------|--------|------------|------------|
| Users think reset worked but wallet unchanged (stub) | M | M | Success toast + manual checklist step; merge Story 2 before epic close | Task 5 |
| Missing `RequireAuthorization` on POST | L | H | Mirror `GET /api/portfolio` + handler `ICurrentUserAccessor` check | Task 1 |
| No `alert-dialog` in repo | L | L | `yarn --cwd web dlx shadcn@latest add alert-dialog` | Task 1 |
| Order aggregate not in domain yet | M | L | Stub handler does not touch orders; Story 3 depends on order epic or parallel work | — |
| 409 guard leaks across test hosts | L | M | Register in-memory guard as singleton; reset guard in test factory if needed | Task 3 |

## Prerequisites

- [x] Spec approved for planning (epic #43 / story #44 open)
- [ ] Aspire local stack runs (`aspire run` or env-doctor)
- [x] Schema includes `portfolio_resets` (initial migration applied)
- [x] US-02 session + US-03 wallet query patterns available

## File structure (planned)

```text
src/
  TradingSimulator.Contracts/Portfolio/
    PortfolioResetResponse.cs          CREATE
  TradingSimulator.Application/Portfolios/Commands/
    ResetPortfolioCommand.cs           CREATE
    ResetPortfolioCommandHandler.cs    CREATE (stub)
    ResetPortfolioCommandValidator.cs  CREATE
    PortfolioResetErrors.cs            CREATE
  TradingSimulator.Application/Abstractions/Portfolio/
    IResetInFlightGuard.cs             CREATE (optional)
  TradingSimulator.Infrastructure/Portfolio/
    InMemoryResetInFlightGuard.cs      CREATE (optional)
  TradingSimulator.Api/Endpoints/
    PortfolioEndpoint.cs               MODIFY (POST)
  TradingSimulator.Api/Common/
    ApiErrorCodes.cs                   MODIFY (RESET_* codes if centralized)
web/src/
  components/ui/alert-dialog.tsx         CREATE (shadcn)
  features/portfolio-reset/
    api.ts                               CREATE
    reset-portfolio-dialog.tsx           CREATE
    use-reset-portfolio.ts               CREATE
    map-reset-error.ts                   CREATE
    reset-eligibility.ts                 CREATE (nextEligibleAt client cache)
  layouts/app-layout.tsx                 MODIFY (menu item + dialog)
contracts/openapi/api.v1.yaml            MODIFY (via api:export)
tests/TradingSimulator.Api.IntegrationTests/Portfolios/
  ResetPortfolioTests.cs                 CREATE
docs/memory/decisions.md                 MODIFY (ADR-005)
```

## Authorization, session, and domain notes

- **Session model:** Cookie session (ADR-001). `POST /api/portfolio/reset` requires authenticated user; **401** `UNAUTHORIZED` when `ICurrentUserAccessor.UserId` is null (EC-07).
- **Route protection:** `.RequireAuthorization()` on POST endpoint (align with `GET /api/portfolio`). Handler still returns structured **401** for consistency with `GetMyWallet`.
- **Domain rules (Story 1):** BR-01 (user-initiated entry point only), BR-05 (no session invalidation on success). Do **not** implement BR-02–BR-08 persistence in this story.
- **Stub response contract** (spec §4b):

```json
{
  "resetAt": "<ISO-8601>",
  "nextEligibleAt": "<ISO-8601>",
  "wallet": {
    "totalBalance": 100000.0000,
    "reservedBalance": 0.0000,
    "availableBalance": 100000.0000,
    "currency": "USD"
  }
}
```

  Stub sets `resetAt` = `IClock.UtcNow`, `nextEligibleAt` = `resetAt + Trading:PortfolioResetCooldownMinutes`, and `wallet` from **current** `IWalletReadRepository` read (not forced $100k) so integrators see honest pre-reset figures until Story 2.

## Progress tracker

### Task 1: Wire reset POST skeleton and menu entry

| Attribute | Value |
|-----------|--------|
| Spec story | Story 1 |
| Depends on | None |
| Estimated complexity | M |
| Parent story issue | #44 |

#### Objective

An authenticated user sees **Reset portfolio** in the account menu; opening it shows a dialog shell; confirming calls `POST /api/portfolio/reset` and receives **200** with the contract body. Unauthenticated POST returns **401**. No PostgreSQL mutations yet.

#### Implementation notes

- Add `ResetPortfolioCommand` (no properties) and stub handler: resolve `userId`, read wallet, compute `nextEligibleAt` from `IOptions<TradingOptions>.PortfolioResetCooldownMinutes`, return `PortfolioResetResponse`.
- Extend `PortfolioEndpoint` with `MapPost("/api/portfolio/reset", ...)` + OpenAPI metadata (Produces 200, 401, 409, 500).
- Register handler via existing MediatR `AssemblyReference` scan.
- Add shadcn `alert-dialog`; create `ResetPortfolioDialog` with placeholder consequence bullets and Confirm/Cancel.
- `useResetPortfolio` mutation: `apiClient.post` with `credentials: 'include'`.
- Run `yarn --cwd web api:export` and commit `contracts/openapi/api.v1.yaml`.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `src/TradingSimulator.Contracts/Portfolio/PortfolioResetResponse.cs` | Response DTO |
| CREATE | `src/TradingSimulator.Application/Portfolios/Commands/ResetPortfolioCommand.cs` | Command |
| CREATE | `src/TradingSimulator.Application/Portfolios/Commands/ResetPortfolioCommandHandler.cs` | Stub handler |
| CREATE | `src/TradingSimulator.Application/Portfolios/Commands/ResetPortfolioCommandValidator.cs` | Empty command validator |
| MODIFY | `src/TradingSimulator.Api/Endpoints/PortfolioEndpoint.cs` | POST route |
| CREATE | `web/src/components/ui/alert-dialog.tsx` | shadcn primitive |
| CREATE | `web/src/features/portfolio-reset/api.ts` | POST client |
| CREATE | `web/src/features/portfolio-reset/reset-portfolio-dialog.tsx` | Dialog shell |
| CREATE | `web/src/features/portfolio-reset/use-reset-portfolio.ts` | Mutation hook |
| MODIFY | `web/src/layouts/app-layout.tsx` | Menu item + dialog trigger |
| MODIFY | `contracts/openapi/api.v1.yaml` | Exported contract |
| REUSE | `src/TradingSimulator.Application/Users/Queries/GetMyWalletQueryHandler.cs` | Auth + wallet read pattern |
| REUSE | `web/src/features/auth/use-logout.ts` | Mutation + toast pattern |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `ResetPortfolio_WithoutSession_Returns401_UNAUTHORIZED` | `tests/.../Portfolios/ResetPortfolioTests.cs` |
| Integration | `ResetPortfolio_WithSession_Returns200_WithContractShape` | same |
| Manual | Menu item visible when logged in; confirm returns 200 in network tab | `web/` |

#### Acceptance criteria

- [x] `POST /api/portfolio/reset` exists and is named in OpenAPI `ResetPortfolio`
- [x] Authenticated integration test passes with `resetAt`, `nextEligibleAt`, `wallet` fields
- [x] Unauthenticated request returns **401** (empty body from `RequireAuthorization` middleware; handler would return `UNAUTHORIZED` problem if reached)
- [x] User menu shows **Reset portfolio** on authenticated layout
- [x] Confirm in dialog triggers POST (observable in DevTools)

#### Notes (Task 1 implementation)

- shadcn CLI wrote to `web/@/components/ui/`; component copied to `web/src/components/ui/alert-dialog.tsx`.
- Stub handler: no PostgreSQL writes (ADR-005 deferred to Task 5).
- Dialog copy expanded to five bullets in Task 2 (`reset-consequences.ts`).

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | PRD §5.1 US-04; Tech §6, §8.1; DB §4.10 (no write) |
| Async matching | N/A — stub does not enqueue |
| PostgreSQL authoritative | No writes in Story 1 |
| Redis projection | N/A |
| RFC 7807 errors | **401** on missing session |
| SignalR | N/A |
| Aspire | None |
| ADR needed? | **Yes — ADR-005** (stub) |

#### Risk

Stub may confuse QA if Story 2 not merged soon — document in manual checklist.

---

### Task 2: Confirmation copy and dismiss without API

| Attribute | Value |
|-----------|--------|
| Spec story | Story 1 |
| Depends on | Task 1 |
| Estimated complexity | S |
| Parent story issue | #44 |

#### Objective

Dialog shows the full consequence list from the spec (cash, holdings, orders, history, 24h cooldown). Cancel button, dialog close control, and Escape dismiss the dialog without any `POST`.

#### Implementation notes

- Use `AlertDialog` controlled `open` / `onOpenChange` from `UserMenu` parent or internal state; set `open = false` on cancel without calling mutation.
- Destructive styling on Confirm (not on menu item — menu item is neutral; confirm button is destructive).
- Keep dialog open until success or error from POST (do not close on confirm click before response — Task 3).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `web/src/features/portfolio-reset/reset-portfolio-dialog.tsx` | Full copy + dismiss handlers |
| MODIFY | `web/src/layouts/app-layout.tsx` | Wire open state |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Open dialog → Cancel → no `portfolio/reset` in network | `web/` |
| Manual | Open → click outside / Escape → no POST | `web/` |

#### Acceptance criteria

- [x] All five consequence bullets visible (100k, zero AAPL, cancel opens, history cleared, 24h cooldown)
- [x] Cancel/dismiss closes dialog; wallet unchanged on subsequent `GET /api/wallet`

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | PRD §7.4 reachability |
| RFC 7807 | N/A |
| ADR needed? | No |

#### Risk

None — UI-only.

---

### Task 3: Loading, double-submit, and error mapping

| Attribute | Value |
|-----------|--------|
| Spec story | Story 1 |
| Depends on | Task 2 |
| Estimated complexity | M |
| Parent story issue | #44 |

#### Objective

Confirm button shows loading and stays disabled while POST is in flight. A second click does not fire a second request. API errors map to user-visible messages: `UNAUTHORIZED`, `INTERNAL_ERROR`, `RESET_IN_PROGRESS` (optional **409**).

#### Implementation notes

- `useMutation` `isPending` disables Confirm and menu trigger.
- Optional `IResetInFlightGuard`: per-`UserId` flag set at handler start, cleared in `finally`; second concurrent call returns `Error.Conflict("RESET_IN_PROGRESS", ...)`.
- `map-reset-error.ts` mirrors `map-register-error` / wallet patterns; **401** → clear session or redirect via existing auth interceptor.
- Do not close dialog on error; allow retry after message.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `web/src/features/portfolio-reset/map-reset-error.ts` | Error copy |
| MODIFY | `web/src/features/portfolio-reset/use-reset-portfolio.ts` | Pending + onError |
| MODIFY | `web/src/features/portfolio-reset/reset-portfolio-dialog.tsx` | Loading UI |
| CREATE | `src/TradingSimulator.Application/Abstractions/Portfolios/IResetInFlightGuard.cs` | Optional (namespace `Portfolios` avoids clash with domain `Portfolio`) |
| CREATE | `src/TradingSimulator.Infrastructure/Portfolios/InMemoryResetInFlightGuard.cs` | Optional |
| MODIFY | `src/TradingSimulator.Application/Portfolios/Commands/ResetPortfolioCommandHandler.cs` | Guard usage |
| MODIFY | `src/TradingSimulator.Infrastructure/DependencyInjection.cs` | Register guard |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `ResetPortfolio_WhenInFlight_SecondRequestReturns409` (if guard implemented) | `ResetPortfolioTests.cs` |
| Manual | Double-click confirm → one POST | `web/` |
| Manual | Expired session on confirm → 401 message | `web/` |

#### Acceptance criteria

- [x] Confirm disabled during `isPending`
- [x] `UNAUTHORIZED` shows safe message; no reset row (N/A for stub)
- [x] `RESET_IN_PROGRESS` shows spec copy when **409** implemented
- [x] `INTERNAL_ERROR` shows retry copy; dialog stays open

#### Notes (Task 3 implementation)

- Guard interface/folder use `Abstractions/Portfolios` and `Infrastructure/Portfolios` (not `Portfolio/`) to avoid C# namespace clash with `TradingSimulator.Domain.Portfolios.Portfolio`.
- `UserMenu` owns `useResetPortfolio`; dialog receives `isPending`, `errorMessage`, `onConfirm`.
- Dialog blocks dismiss (Cancel, Escape, overlay) while `isPending`; closes only on success.
- Integration test uses `DelayingWalletReadRepository` (300 ms) to overlap concurrent POSTs for **409**.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| RFC 7807 | **409** Conflict, **401**, **500** |
| Async matching | N/A |
| ADR needed? | No |

#### Risk

In-memory guard is per-process only — acceptable for local MVP; Story 2 may replace with DB-level idempotency.

---

### Task 4: Success UX and disable reset until nextEligibleAt

| Attribute | Value |
|-----------|--------|
| Spec story | Story 1 (disable preview for Story 4) |
| Depends on | Task 3 |
| Estimated complexity | S |
| Parent story issue | #44 |

#### Objective

On **200**, dialog closes, success toast shows (“Portfolio reset. You're starting fresh with $100,000.” per spec — clarify in toast that positions/orders update in a follow-up if stub), and **Reset portfolio** menu item is disabled until `nextEligibleAt` from the response (persist in `sessionStorage` key `portfolio-reset:nextEligibleAt` scoped by `userId`).

#### Implementation notes

- `onSuccess`: `setNextEligibleAt`, close dialog, `toast.success`.
- `reset-eligibility.ts`: `isResetAllowed()`, `saveNextEligibleAt()`, read on menu render.
- Do **not** invalidate `['wallet']` yet (Story 5); optional comment in hook for Story 5.
- Menu item: `disabled={!isResetAllowed() || isPending}` with `title` tooltip showing relative time until eligible.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `web/src/features/portfolio-reset/reset-eligibility.ts` | Client cooldown cache |
| MODIFY | `web/src/features/portfolio-reset/use-reset-portfolio.ts` | onSuccess handler |
| MODIFY | `web/src/layouts/app-layout.tsx` | Disabled menu state |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | After success, menu item disabled; re-enabled after `nextEligibleAt` (adjust clock or wait) | `web/` |

#### Acceptance criteria

- [x] Success toast shown once per successful POST
- [x] Dialog closes only on success
- [x] Reset menu entry disabled until `nextEligibleAt` (client-side)
- [x] Document in manual steps: wallet figures may not change until Story 2

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD | FR-1.4 cooldown messaging preview |
| ADR needed? | No |

#### Risk

Client-only disable does not stop API replay — Story 4 adds server **422**.

---

### Task 5: Polish, ADR-005, and manual verification

| Attribute | Value |
|-----------|--------|
| Spec story | Polish |
| Depends on | Task 4 |
| Estimated complexity | S |
| Parent story issue | #44 |

#### Objective

Record ADR-005, align auth on wallet GET if inconsistent, run `api:verify`, full manual checklist, and smoke regression on login/wallet.

#### Implementation notes

- Append ADR-005 to `docs/memory/decisions.md`: Story 1 stub, Story 2 replaces handler body with DB §10.4 transaction.
- Consider adding `.RequireAuthorization()` to `GET /api/wallet` if missing (consistency only — out of scope unless trivial).
- Run `dotnet test` on `ResetPortfolioTests` + existing `GetMyWalletTests`.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `docs/memory/decisions.md` | ADR-005 |
| MODIFY | `src/TradingSimulator.Api/Endpoints/WalletEndpoint.cs` | Optional RequireAuthorization |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | Full `ResetPortfolioTests` suite green | `tests/.../Portfolios/` |
| Manual | Checklist below | Aspire |

#### Acceptance criteria

- [x] ADR-005 accepted in `decisions.md` (recorded during Task 1–4 work)
- [x] `yarn --cwd web api:verify` passes
- [x] `dotnet test` passes for integration projects touched (13 `ResetPortfolio` + `GetMyWallet` tests)
- [ ] Manual UI checklist signed off (operator — see §Manual UI checklist)

#### Notes (Task 5 implementation)

- `GET /api/wallet` now uses `.RequireAuthorization()` (aligned with portfolio routes).
- `GetMyWalletTests.AssertUnauthorizedAsync` accepts empty **401** body from auth middleware (same as reset POST).

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| Aspire | `aspire run` — web + api |
| ADR needed? | ADR-005 completion |

#### Risk

None — isolated change.

## Reference files

| File | Why open it |
|------|-------------|
| `web/src/layouts/app-layout.tsx` | User menu insertion point |
| `web/src/features/auth/use-logout.ts` | Mutation + session pattern |
| `src/Api/Endpoints/PortfolioEndpoint.cs` | Extend with POST |
| `src/Application/Users/Queries/GetMyWalletQueryHandler.cs` | Auth + wallet read |
| `tests/Api.IntegrationTests/Users/GetMyWalletTests.cs` | Session cookie test patterns |
| `docs/plans/20260525-240000-virtual-cash-story-4.md` | Per-story plan structure reference |
| `docs/specs/20260525-251500-portfolio-reset.md` | Story 1 AC verbatim |
| `.cursor/skills/openapi-contract-sync/SKILL.md` | Export/codegen workflow |

## Implementation details (for /build)

### Application

- `ResetPortfolioCommand`: sealed record, no parameters.
- `ResetPortfolioCommandHandler`: inject `ICurrentUserAccessor`, `IWalletReadRepository`, `IClock`, `IOptions<TradingOptions>`, optional `IResetInFlightGuard`.
- Return `Result<PortfolioResetResponse>`; map wallet fields to contract (same decimals as `WalletResponse`).
- Validator: empty rules (command has no fields) — satisfies pipeline convention.

### Api

- POST handler: `sender.Send(new ResetPortfolioCommand())` → `ToHttpResult()`.
- Produces: `PortfolioResetResponse` 200; Problem 401, 409, 500.
- Tags: `Portfolio`.

### Error codes (Application → Api)

| Code | ErrorType | HTTP |
|------|-----------|------|
| `UNAUTHORIZED` | Unauthorized | 401 |
| `RESET_IN_PROGRESS` | Conflict | 409 |
| `INTERNAL_ERROR` | Unexpected | 500 |

(`RESET_COOLDOWN_ACTIVE` added in Story 4.)

### web

- API path: `/api/portfolio/reset` via generated `paths` after codegen.
- Dialog: `@/components/ui/alert-dialog` — `AlertDialog`, `AlertDialogContent`, `AlertDialogHeader`, `AlertDialogFooter`, `AlertDialogCancel`, `AlertDialogAction` (use `onClick` + prevent default if Action auto-closes).
- Feature folder exports from `features/portfolio-reset/index.ts` for clean imports in layout.

### Configuration

- `Trading:PortfolioResetCooldownMinutes` already in `appsettings.json` (1440) — use for `nextEligibleAt` calculation in stub and later real handler.

## Verification matrix (plan-level)

| Spec AC | Verified by |
|---------|-------------|
| Menu → dialog with consequences | Task 2 manual + Task 1 menu |
| Confirm → POST ≤2s with loading | Task 3 manual + integration 200 |
| Success → toast + disabled action | Task 4 manual |
| Cancel/dismiss → no API | Task 2 manual |
| Double-click → single request | Task 3 manual + optional 409 test |
| EC-07 401 on expired session | Task 3 manual + integration 401 |
| BR-05 session remains | Task 1 — no logout on success |

## Rollback / recovery

- **Code:** Revert branch `feature/portfolio-reset-story-1`.
- **DB:** N/A — no migrations.
- **Redis:** N/A.

## Manual UI checklist (operator)

1. Log in → open user menu on **Trading** → **Reset portfolio** visible.
2. Open dialog → read five consequences → **Cancel** → verify no `POST /api/portfolio/reset` in network.
3. Open again → **Confirm** → loading on button → **200** → success toast → dialog closes.
4. **Reset portfolio** menu item disabled; tooltip/hint shows next eligible time.
5. `GET /api/wallet` still shows **pre-reset** balances (stub) — expected until Story 2.
6. Log out → log in as another user → reset menu enabled (eligibility not leaked).
7. Regression: top-bar wallet chip and trading cash card still load.

## Deferred work (Plan B — other stories)

- **Story 2 plan:** Real `ResetPortfolioCommand` — wallet $100k, clear holdings, `portfolio_resets` insert (issue #45).
- **Story 3 plan:** Cancel open orders, history read cutoff, matching channel (issue #46).
- **Story 4 plan:** Server cooldown **422**, disabled UI from server state (issue #47).
- **Story 5 plan:** Query invalidation + SignalR alignment (issue #48).
- **ADR for history cutoff** (spec Q1) in Story 3 plan.

## GitHub Links

> Plan tasks are tracked in this file only. Story issue #44 updated with task checklist.

| Local ref | Issue # | Type | Title | URL |
|-----------|---------|------|-------|-----|
| spec Story 1 | 44 | Story | US-04 / Story 1: Confirm before resetting | https://github.com/tranvuongduy2003/trading-simulator/issues/44 |
| spec epic | 43 | Epic | Spec: Portfolio reset (US-04) | https://github.com/tranvuongduy2003/trading-simulator/issues/43 |


---

<a id="source-20260527-210000-portfolio-reset-story-2md"></a>

## Source 15 of 18: `docs/plans/20260527-210000-portfolio-reset-story-2.md`

---
artifact_type: plan
artifact_version: 1
id: plan-20260527-210000-portfolio-reset-story-2
title: Portfolio Reset — Story 2 (Restore starting cash and empty holdings)
slug: portfolio-reset-story-2
filename_template: 20260527-210000-portfolio-reset-story-2.md
created_at: 2026-05-27T21:00:00+07:00
updated_at: 2026-05-27T21:00:00+07:00
status: draft
owner: engineering
tags: [plan, implementation, trading-simulator, portfolio-reset, wallet, holdings, us-04, story-2]
related_spec: docs/specs/20260525-251500-portfolio-reset.md
related_plans: [docs/plans/20260525-260000-portfolio-reset-story-1.md]
prd_refs: [PRD §5.1 US-04, PRD §6.1 FR-1.4, PRD §7.3]
tech_refs: [Tech §5.2.1 Wallet, Tech §5.2.2 Portfolio, Tech §6 ResetPortfolioCommand, Tech §16 Trading:InitialVirtualCash, Tech §16 Trading:PortfolioResetCooldownMinutes]
db_refs: [DB §4.2 wallets, DB §4.3 portfolios, DB §4.4 holdings, DB §4.10 portfolio_resets, DB §10.4 Portfolio Reset]
github:
  repo: tranvuongduy2003/trading-simulator
  epic_issue: 43
  story_issue_ids: [45]
  last_synced_at: 2026-05-27T21:00:00+07:00
search_index:
  keywords: [portfolio reset, story 2, wallet reset, holdings clear, 100000, portfolio_resets, atomic transaction, IPortfolioResetWriteRepository, ResetPortfolioCommandHandler, read-your-writes, BR-03, BR-06, BR-09, EC-02, EC-06]
  bounded_contexts: [Trading]
  task_count: 4
---

# Implementation Plan: Portfolio Reset — Story 2

| Field | Value |
|-------|--------|
| Spec | `docs/specs/20260525-251500-portfolio-reset.md` (§2 Story 2) |
| Epic | [#43 — Portfolio reset (US-04)](https://github.com/tranvuongduy2003/trading-simulator/issues/43) |
| Story issue | [#45 — Restore starting cash and empty holdings](https://github.com/tranvuongduy2003/trading-simulator/issues/45) |
| Status | DRAFT |
| Tasks | 4 |
| Branch | `feature/portfolio-reset-story-2` |
| Aspire impact | No topology change |
| Schema impact | No — tables exist |
| Test levels | Api.IntegrationTests (primary); optional Domain.UnitTests for wallet reset helper |
| ADRs required | Update ADR-005 (stub superseded for wallet/holdings slice) |
| GitHub | Synced 2026-05-27 — see §GitHub Links |

Execution note (deviation): Implementation for this `/build` session runs on `feature/portfolio-reset-story-2-local` (branched from current workspace state) because local uncommitted documentation changes prevented checkout of `feature/portfolio-reset-story-2`.

## Executive summary

Story 2 (US-04) replaces the Story 1 **contract stub** with a real PostgreSQL write path: within one transaction, set the user’s wallet to **`Trading:InitialVirtualCash`** (100000.0000 USD) with **zero** reserved balance, **delete all holdings** for their portfolio, and **append** a `portfolio_resets` audit row. The handler returns the spec-shaped **200** body with post-reset wallet figures. **Order cancellation, reservation release on open orders, Redis book updates, and history cutoff remain Story 3+** (issue out of scope). Integration tests prove read-your-writes on `GET /api/wallet` and `GET /api/portfolio`, rollback on failure, and **401** with no audit row.

## Goals and non-goals

**Goals**

- G1: After successful `POST /api/portfolio/reset`, `GET /api/wallet` shows total/reserved/available **100000 / 0 / 100000** within **2 s** (local MVP).
- G2: After reset, `GET /api/portfolio` returns **no holdings**; portfolio value equals available cash at reset instant.
- G3: Atomic commit/rollback — failure yields pre-reset wallet and holdings; **401** performs **no** `portfolio_resets` insert.
- G4: Use `Trading:InitialVirtualCash` (BR-09: per-user reallocation, not system minting).

**Non-goals** (this plan will not do)

- NG1: Cancel open orders, matching-engine book removal, Redis tape/book projection (Story 3 — [#46](https://github.com/tranvuongduy2003/trading-simulator/issues/46)).
- NG2: Server-side **24h cooldown** enforcement (`RESET_COOLDOWN_ACTIVE`) — Story 4 ([#47](https://github.com/tranvuongduy2003/trading-simulator/issues/47)).
- NG3: Trade/order history read cutoff (Story 3 / spec open question #1).
- NG4: `PortfolioResetEvent` domain event + SignalR fan-out (Story 5 / BR-11 — optional note in Task 4 only).
- NG5: Frontend changes (Story 1 UI already calls POST; refetch behavior is Story 5).

## Traceability matrix

| Spec story | Plan task(s) | Test evidence |
|------------|--------------|---------------|
| Story 2 — happy path wallet | Task 2, 4 | `ResetPortfolio_AfterDepletedWallet_Returns100k_OnGetWallet` |
| Story 2 — empty holdings | Task 2, 4 | `ResetPortfolio_ClearsHoldings_OnGetPortfolio` |
| Story 2 — failure no partial state | Task 3 | `ResetPortfolio_WhenWriteFails_PreResetStateUnchanged` |
| Story 2 — 401 no reset row | Task 3 | `ResetPortfolio_WithoutSession_NoPortfolioResetsRow` |
| EC-02 zero holdings, depleted cash | Task 4 | `ResetPortfolio_WithZeroHoldings_StillRestoresCash` |
| EC-06 concurrency retry | Task 3 | Rely on existing `UnitOfWorkBehavior` + optional `RowVersion` bump test |
| BR-03 / BR-06 (partial) | Task 2 | Integration tests + transaction boundary |
| BR-09 | Task 2 | Uses `IOptions<TradingOptions>.InitialVirtualCash` |
| Story 1 contract shape | Task 1, 4 | Existing `ResetPortfolio_WithSession_Returns200_WithContractShape` (updated assertions) |
| Story 1 in-flight 409 | — | **Reuse** existing test (unchanged guard) |
| Cooldown enforcement | — | **Deferred** Story 4 |
| Open orders after reset | — | **Known gap** until Story 3 (see Risks) |

## Architecture impact

```text
┌──────── web/ (no change) ────────────────────────────────────────┐
│  POST /api/portfolio/reset  (Story 1 dialog already wired)        │
└────────────────────────────┬─────────────────────────────────────┘
                             │ session cookie
┌────────────────────────────▼─────────────────────────────────────┐
│  Api: PortfolioEndpoint → MediatR                                 │
│  ResetPortfolioCommandHandler                                     │
│    1. Auth (ICurrentUserAccessor)                                 │
│    2. IResetInFlightGuard (409)                                   │
│    3. IPortfolioResetWriteRepository.ResetForUserAsync  ◄── NEW   │
│    4. Map PortfolioResetWriteModel → PortfolioResetResponse       │
└────────────────────────────┬─────────────────────────────────────┘
                             │ IUnitOfWorkRequest → UnitOfWorkBehavior
┌────────────────────────────▼─────────────────────────────────────┐
│  Infrastructure: PortfolioResetWriteRepository                    │
│    BEGIN (implicit via behavior)                                  │
│    UPDATE wallets SET total=InitialVirtualCash, reserved=0        │
│    DELETE holdings WHERE portfolio_id = …                         │
│    INSERT portfolio_resets (user_id, reset_at, reason)            │
│    COMMIT                                                         │
└────────────────────────────┬─────────────────────────────────────┘
                             │
                    PostgreSQL (authoritative)
```

| Layer | Change summary |
|-------|----------------|
| Domain | Optional: `Wallet.ResetToInitial(Money)` value-object helper + unit test (keeps rules out of raw SQL) |
| Application | Replace stub handler body; depend on `IPortfolioResetWriteRepository` + `IOptions<TradingOptions>`; stop returning pre-reset wallet from read repo |
| Infrastructure | Finish `PortfolioResetWriteRepository.ResetForUserAsync`; register `IPortfolioResetWriteRepository` in DI |
| Api | No route change |
| MatchingEngine | **No change** (Story 3) |
| web/ | **No change** |
| AppHost | **No change** |

## Data & migration plan

| Artifact | Action | DB reference |
|----------|--------|--------------|
| EF migration | **None** | `portfolio_resets`, `wallets`, `holdings` exist |
| Redis keys | **None** | Wallet not cached (spec §5) |
| Book recovery | **N/A** | Story 3 |
| Transaction steps (this story) | **Subset of DB §10.4:** (3) set wallet initial, (4) remove holdings, (5) insert `portfolio_resets` | DB §10.4 steps 3–5 |
| Skipped until Story 3 | (1) cancel open orders, (2) release order-linked reservations | DB §10.4 steps 1–2 |

`portfolio_resets.reason`: set `"user_initiated"` (nullable column per DB §4.10).

## Open questions

| # | Question | Source | Answer | Status |
|---|----------|--------|--------|--------|
| 1 | Block reset when user has open orders until Story 3? | BR-06 vs story split | **No block in Story 2** — document risk; Story 3 restores full atomic flow | ✅ Answered |
| 2 | Domain methods vs repository-only SQL? | Clean Architecture | Prefer **`Wallet.ResetToInitial`** helper in Domain + EF maps amounts; holdings delete stays in write repository | ✅ Answered |
| 3 | Raise `PortfolioResetEvent` now? | BR-11 | **Defer** to Story 5 unless trivial `Raise` with no dispatcher | ⏳ Deferred |
| 4 | History soft-cutoff vs delete? | Spec §13 Q1 | **Out of scope** Story 2 | ⏳ Deferred |

## Risks and mitigations

| Risk | Likelihood | Impact | Mitigation | Owner task |
|------|------------|--------|------------|------------|
| User with open orders: wallet forced to 100k while orders still reserve cash/qty | M | H until Story 3 | Document in ADR-005 addendum; integration tests seed **no open orders**; Story 3 merges full DB §10.4 | Task 4 |
| Story 1 tests assumed stub (no DB writes) | M | M | Update tests to seed depleted state / assert `portfolio_resets` count | Task 4 |
| `ExecuteUpdate`/`ExecuteDelete` bypass domain events | L | L | Acceptable for cross-aggregate reset port; optional domain helper for amounts only | Task 2 |
| Optimistic concurrency on `wallets.row_version` | L | M | `UnitOfWorkBehavior` retries; integration test optional | Task 3 |
| Untracked `IPortfolioResetWriteRepository` files not in DI | H | H | Task 1 registers scoped implementation | Task 1 |

## Prerequisites

- [ ] Story 1 merged or cherry-picked onto `feature/portfolio-reset-story-2` (POST route, dialog, in-flight guard, OpenAPI)
- [ ] Aspire local stack runs; Docker available for Testcontainers
- [ ] Initial migration applied (`portfolio_resets` table present)
- [ ] Familiarity with `GetMyWalletTests.SeedWalletBalancesAsync` pattern for wallet seeding

## File structure (planned)

```text
src/
  Application/
    Abstractions/Persistence/
      IPortfolioResetWriteRepository.cs     MODIFY (ensure ResetForUserAsync contract)
    Portfolios/Commands/
      ResetPortfolioCommandHandler.cs       MODIFY (replace stub)
  Domain/
    Users/Wallet.cs                         MODIFY (optional ResetToInitial)
  Infrastructure/
    Persistence/Repositories/
      PortfolioResetWriteRepository.cs      MODIFY (implement ResetForUserAsync)
    DependencyInjection.cs                  MODIFY (register write repo)
tests/
  Api.IntegrationTests/
    Portfolios/
      ResetPortfolioTests.cs                MODIFY + new scenarios
      PortfolioResetTestHelpers.cs          CREATE (seed wallet + holdings)
      Fakes/
        ThrowOnPortfolioResetWriteRepository.cs  CREATE (rollback test)
docs/memory/decisions.md                    MODIFY (ADR-005 addendum)
```

## Authorization, session, and domain notes

- **Session model:** Cookie session (ADR-001). Handler returns **401** before any write when `ICurrentUserAccessor.UserId` is null.
- **Route protection:** `POST /api/portfolio/reset` already requires authorization (Story 1).
- **Domain rules (Story 2 slice):**
  - **BR-03:** Wallet total = `InitialVirtualCash`, reserved = 0; all holdings removed.
  - **BR-06:** Single UoW transaction for wallet + holdings + audit row; rollback on any failure.
  - **BR-09:** Only this user’s wallet row changes; constant from config, not computed from market.
- **Initial cash:** `IOptions<TradingOptions>.InitialVirtualCash` — same as registration (FR-1.3).

## Progress tracker

### Task 1: Replace reset stub with write-capable skeleton

| Attribute | Value |
|-----------|--------|
| Spec story | Story 2 |
| Depends on | None (branches from Story 1 baseline) |
| Estimated complexity | M |
| Parent story issue | #45 |

#### Objective

`ResetPortfolioCommandHandler` calls `IPortfolioResetWriteRepository` inside the existing MediatR UoW pipeline; DI registers the repository. `ResetForUserAsync` may initially return the target snapshot without persisting (skeleton), but the handler path no longer uses `IWalletReadRepository` for the success payload.

#### Implementation notes

- Register `services.AddScoped<IPortfolioResetWriteRepository, PortfolioResetWriteRepository>()`.
- Handler flow: auth → in-flight guard → `ResetForUserAsync(userId, options.InitialVirtualCash, clock.UtcNow)` → map to `PortfolioResetResponse` with `nextEligibleAt = resetAt + PortfolioResetCooldownMinutes` (timestamps only; **no cooldown check** yet).
- Remove stub comment; keep `IWalletReadRepository` only if needed for `WalletNotFound` when repo returns null.
- Ensure `ResetPortfolioCommand` remains `IUnitOfWorkRequest` so `UnitOfWorkBehavior` wraps the handler.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `src/Infrastructure/DependencyInjection.cs` | Register write repository |
| MODIFY | `src/Application/Portfolios/Commands/ResetPortfolioCommandHandler.cs` | Call write port |
| MODIFY | `src/Infrastructure/Persistence/Repositories/PortfolioResetWriteRepository.cs` | Skeleton `ResetForUserAsync` |
| REUSE | `src/Application/Abstractions/Persistence/IPortfolioResetWriteRepository.cs` | Port + DTOs |
| REUSE | `src/Application/Behaviors/UnitOfWorkBehavior.cs` | Transaction boundary |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | Existing suite still compiles; 401 unchanged | `ResetPortfolioTests.cs` |

#### Acceptance criteria

- [x] Handler depends on `IPortfolioResetWriteRepository`, not wallet read stub
- [x] DI resolves write repository in Api host
- [x] `dotnet build` succeeds

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | FR-1.4 partial; Tech §6; DB §10.4 (subset) |
| PostgreSQL authoritative | Writes go through EF context in UoW |
| RFC 7807 errors | Unchanged 401/409 |
| ADR needed? | No |

#### Risk

None — wiring only.

---

### Task 2: Implement atomic wallet reset and holdings clear

| Attribute | Value |
|-----------|--------|
| Spec story | Story 2 |
| Depends on | Task 1 |
| Estimated complexity | L |
| Parent story issue | #45 |

#### Objective

`ResetForUserAsync` performs, in one transaction: update wallet to initial cash and zero reserved; delete all holdings for the user’s portfolio; insert `portfolio_resets` row; return `PortfolioResetWriteModel` with **100000 / 0 / 100000**.

#### Implementation notes

- Resolve `portfolio_id` via `portfolios.user_id` (unique).
- Wallet update: `AsTracking()` load or `ExecuteUpdateAsync` on `wallets` filtered by `user_id`; set `TotalBalance = initialVirtualCash`, `ReservedBalance = 0`, bump `UpdatedAt`.
- Holdings: `ExecuteDeleteAsync` on `holdings` where `portfolio_id` matches (idempotent if zero rows).
- Audit: `PortfolioResets.Add(new PortfolioResetRecord { UserId, ResetAt, Reason = "user_initiated" })`.
- Return null if wallet/portfolio missing → handler maps to `WALLET_NOT_FOUND` or `PORTFOLIO_NOT_FOUND`.
- Optional domain: `Wallet.ResetToInitial(Money)` used to validate non-negative amounts before EF update.
- **Do not** cancel orders or touch `orders` table in this task.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `src/Infrastructure/Persistence/Repositories/PortfolioResetWriteRepository.cs` | Full reset implementation |
| MODIFY | `src/Domain/Users/Wallet.cs` | Optional `ResetToInitial` |
| CREATE | `tests/Domain.UnitTests/Users/WalletResetTests.cs` | Optional domain test |
| CREATE | `tests/Api.IntegrationTests/Portfolios/PortfolioResetTestHelpers.cs` | Seed depleted wallet + AAPL holding |
| MODIFY | `tests/Api.IntegrationTests/Portfolios/ResetPortfolioTests.cs` | Add Story 2 integration scenarios |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `ResetPortfolio_AfterDepletedWallet_Returns100k_OnGetWallet` | `ResetPortfolioTests.cs` |
| Integration | `ResetPortfolio_ClearsHoldings_OnGetPortfolio` | `ResetPortfolioTests.cs` |
| Integration | `ResetPortfolio_InsertsPortfolioResetsRow` | `ResetPortfolioTests.cs` |
| Domain | `ResetToInitial_SetsBalances` (if domain helper added) | `WalletResetTests.cs` |

#### Acceptance criteria

- [x] GIVEN seeded total 42000, reserved 5000, 50 AAPL shares → WHEN POST reset → THEN response wallet **100000 / 0 / 100000**
- [x] GIVEN same → WHEN GET wallet/portfolio within 2s → THEN matching balances and **empty holdings**
- [x] Exactly **one** new `portfolio_resets` row for user
- [x] Uses `InitialVirtualCash` from options (not hardcoded literal in repository)

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD | FR-1.3, FR-1.4 (cash slice) |
| PostgreSQL authoritative | Single transaction via UoW |
| Redis projection | N/A |
| Async matching | N/A this story |

#### Risk

Open orders + reset inconsistency until Story 3 — see Risks table.

---

### Task 3: Add rollback and unauthorized no-write guarantees

| Attribute | Value |
|-----------|--------|
| Spec story | Story 2 (failure paths) |
| Depends on | Task 2 |
| Estimated complexity | M |
| Parent story issue | #45 |

#### Objective

Prove **500** (or handler failure) leaves pre-reset wallet/holdings/audit unchanged, and **401** creates no `portfolio_resets` row.

#### Implementation notes

- `ThrowOnPortfolioResetWriteRepository` decorates or replaces `IPortfolioResetWriteRepository` in test factory: throw after intentional no-op or immediately on `ResetForUserAsync`.
- Assert pre-seeded wallet/holdings counts unchanged via `ApplicationDatabaseContext`.
- Unauthorized POST: assert `portfolio_resets` count for user remains 0 (join via `users` from registration id).
- Ensure `UnitOfWorkBehavior` rolls back on exception (existing behavior).
- Deviation: unauthorized no-write assertion is implemented by registering a user in test setup and verifying that user's `portfolio_resets` row count is unchanged before/after the unauthorized POST (equivalent guarantee without user-id extraction from the unauthorized request context).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `tests/Api.IntegrationTests/Portfolios/Fakes/ThrowOnPortfolioResetWriteRepository.cs` | Force failure |
| MODIFY | `tests/Api.IntegrationTests/Portfolios/ResetPortfolioTests.cs` | Rollback + 401 audit tests |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `ResetPortfolio_WhenWriteFails_PreResetStateUnchanged` | `ResetPortfolioTests.cs` |
| Integration | `ResetPortfolio_WithoutSession_NoPortfolioResetsRow` | `ResetPortfolioTests.cs` |

#### Acceptance criteria

- [x] Simulated write failure → GET wallet/portfolio still show **pre-reset** seeded values
- [x] No additional `portfolio_resets` row on failure
- [x] 401 → zero `portfolio_resets` for that user

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| RFC 7807 | 500 `INTERNAL_ERROR` on unhandled throw |
| EC-06 | Concurrency handled by pipeline (no partial commit) |

#### Risk

None — test-only + relies on existing UoW.

---

### Task 4: Polish and verification closure

| Attribute | Value |
|-----------|--------|
| Spec story | Polish |
| Depends on | Task 3 |
| Estimated complexity | S |
| Parent story issue | #45 |

#### Objective

Update ADR-005, align integration tests with real writes, add EC-02 test, run full regression, manual smoke on Aspire.

#### Implementation notes

- ADR-005 addendum: Story 2 persists wallet/holdings/`portfolio_resets`; Story 1 stub superseded for those writes; order cancel still future.
- Update `ResetPortfolio_WithSession_Returns200_WithContractShape`: register → seed depleted wallet → reset → assert response **and** GET wallet.
- `ResetPortfolio_WithZeroHoldings_StillRestoresCash` (EC-02): 500 total, 0 holdings → 100k.
- Run `dotnet test` on `Api.IntegrationTests` Portfolios + Users wallet tests.
- Manual: Aspire → login → seed via trades or SQL → reset → verify wallet card and portfolio tab (no frontend code required if Story 1 merged).
- Deviation: Manual Aspire smoke checklist is left to operator execution and is not performed during this automation run.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `docs/memory/decisions.md` | ADR-005 addendum |
| MODIFY | `tests/Api.IntegrationTests/Portfolios/ResetPortfolioTests.cs` | EC-02 + shape test updates |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | Full `ResetPortfolioTests` green | `ResetPortfolioTests.cs` |
| Manual | Wallet + portfolio panels after reset | Aspire |

#### Acceptance criteria

- [x] All Story 2 acceptance criteria in verification matrix pass
- [x] `dotnet test` passes for touched projects
- [x] ADR-005 documents Story 2 scope vs Story 3
- [ ] Manual smoke: depleted cash → reset → **$100,000** and empty holdings

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| OpenAPI | Regenerate if response examples drift (`api:verify` if web touched — likely N/A) |
| No regression | Login, register, GET wallet auth |

#### Risk

None.

## Reference files

| File | Why open it |
|------|-------------|
| `src/Application/Portfolios/Commands/ResetPortfolioCommandHandler.cs` | Replace stub |
| `src/Infrastructure/Persistence/Repositories/PortfolioResetWriteRepository.cs` | Implement writes |
| `tests/Api.IntegrationTests/Users/GetMyWalletTests.cs` | `SeedWalletBalancesAsync` pattern |
| `docs/plans/20260525-260000-portfolio-reset-story-1.md` | Prior story patterns |
| `docs/DATABASE.md` §10.4 | Full reset transaction (superset) |
| `src/Application/Users/Commands/RegisterUserCommandHandler.cs` | `InitialVirtualCash` usage |

## Implementation details (for /build)

### Handler (`ResetPortfolioCommandHandler`)

1. `userId = currentUserAccessor.UserId` → else `UNAUTHORIZED`.
2. `resetInFlightGuard.TryBegin` → else `RESET_IN_PROGRESS`.
3. `try/finally` with `End(userId)`.
4. `var result = await portfolioResetWriteRepository.ResetForUserAsync(userId, tradingOptions.InitialVirtualCash, clock.UtcNow, ct)`.
5. If `result` is null → `WALLET_NOT_FOUND` (or portfolio not found).
6. Return `PortfolioResetResponse(resetAt, resetAt.AddMinutes(cooldownMinutes), new PortfolioResetWalletSnapshot(...))` from **write model**, not read repository.

### Repository (`ResetForUserAsync`)

Pseudo-sequence (same DbContext as UoW — do not open nested transactions):

1. Load portfolio id for user; if missing return null.
2. Update wallet row (tracking or bulk update).
3. Delete holdings for portfolio id.
4. Add `PortfolioResetRecord`.
5. `SaveChanges` happens in `UnitOfWorkBehavior` after handler returns success.

### Error types

| HTTP | code | When |
|------|------|------|
| 401 | `UNAUTHORIZED` | No session |
| 404 | `WALLET_NOT_FOUND` | No wallet row |
| 409 | `RESET_IN_PROGRESS` | In-flight guard |
| 500 | `INTERNAL_ERROR` | Unhandled exception (transaction rolled back) |

### Integration seed helper (sketch)

- `SeedWalletBalancesAsync(userId, total, reserved)` — reuse from `GetMyWalletTests` or extract to shared helper.
- `SeedHoldingAsync(userId, symbol: "AAPL", quantity: 50, averagePrice: 150m)` — insert `holdings` row via portfolio id lookup.

### Story 3 handoff

When implementing order cancel, **move** reset orchestration to full DB §10.4 order (cancel → release → wallet → holdings → audit) in the same repository or a dedicated `IPortfolioResetOrchestrator` — do not duplicate wallet/holdings SQL.

## Verification matrix (plan-level)

| Spec AC | Verified by |
|---------|-------------|
| Depleted wallet → 100k on GET wallet | Task 2 integration test |
| Holdings cleared on GET portfolio | Task 2 integration test |
| 500 → pre-reset state | Task 3 integration test |
| 401 → no `portfolio_resets` row | Task 3 integration test |
| EC-02 zero holdings, low cash | Task 4 integration test |
| Double-submit 409 | Existing Story 1 test (unchanged) |

## Rollback / recovery

- **Code:** Revert branch; handler can be re-stubbed per ADR-005 if needed.
- **DB:** No migration; user data restored by another reset after Story 4 cooldown or manual SQL in dev.
- **Redis:** N/A.

## Deferred work (Plan B)

- Story 3: Cancel open orders + engine enqueue + history cutoff ([#46](https://github.com/tranvuongduy2003/trading-simulator/issues/46)).
- Story 4: Cooldown enforcement ([#47](https://github.com/tranvuongduy2003/trading-simulator/issues/47)).
- Story 5: Query invalidation + SignalR ([#48](https://github.com/tranvuongduy2003/trading-simulator/issues/48)).
- `PortfolioResetEvent` + dispatcher.
- Block reset when open orders exist (optional product rule).

## GitHub Links

| Local ref | Issue # | Type | Title | URL |
|-----------|---------|------|-------|-----|
| spec Story 2 | 45 | Story | US-04 / Story 2: Restore starting cash and empty holdings | https://github.com/tranvuongduy2003/trading-simulator/issues/45 |
| epic | 43 | Epic | Spec: Portfolio reset (US-04) | https://github.com/tranvuongduy2003/trading-simulator/issues/43 |

> Plan tasks (Task 1–4) are tracked in this file only, not as separate GitHub issues.

## Manual UI checklist (operator)

1. `aspire run` — register or login as test user.
2. Via SQL or future orders UI: set wallet to ~$42k total, $5k reserved; add AAPL holding (50 shares).
3. Open account menu → **Reset portfolio** → confirm.
4. Virtual cash card / top bar → **$100,000.00** available within ~2 s.
5. Portfolio / holdings tab → empty.
6. (Known) Open orders tab may still show pre-reset orders until Story 3 — note if present.
7. Repeat reset immediately → should still succeed (cooldown not enforced until Story 4).


---

<a id="source-20260527-214600-portfolio-reset-story-3md"></a>

## Source 16 of 18: `docs/plans/20260527-214600-portfolio-reset-story-3.md`

---
artifact_type: plan
artifact_version: 1
id: plan-20260527-214600-portfolio-reset-story-3
title: Portfolio Reset - Story 3 (Cancel open orders and clear activity history)
slug: portfolio-reset-story-3
filename_template: 20260527-214600-portfolio-reset-story-3.md
created_at: 2026-05-27T21:46:00+07:00
updated_at: 2026-05-27T22:23:00+07:00
status: draft
owner: engineering
tags: [plan, implementation, trading-simulator, portfolio-reset, orders, history, us-04, story-3]
related_spec: docs/specs/20260525-251500-portfolio-reset.md
related_plans: [docs/plans/20260527-210000-portfolio-reset-story-2.md, docs/plans/20260525-260000-portfolio-reset-story-1.md]
prd_refs: [PRD SS6.1 FR-1.4, PRD SS6.5 FR-5.1, PRD SS6.5 FR-5.2, PRD SS6.6 FR-6.4, PRD SS7.1, PRD SS7.3]
tech_refs: [Tech SS5.2.3 Order, Tech SS6 CQRS Design, Tech SS7 Producer-Consumer Pipeline, Tech SS8.1 API Service, Tech SS9.2 SignalR groups, Tech SS10.5 Read projections, Tech SS16 Error handling]
db_refs: [DB SS4.5 orders, DB SS4.6 trades, DB SS4.10 portfolio_resets, DB SS6.5 ix_orders_user_status, DB SS6.6 trade history indexes, DB SS10.4 Portfolio Reset]
github:
  repo: tranvuongduy2003/trading-simulator
  epic_issue: 43
  story_issue_ids: [46]
  last_synced_at: 2026-05-27T21:46:00+07:00
search_index:
  keywords: [portfolio reset, story 3, cancel open orders, clear order history, clear trade history, reset cutoff, order book convergence, reset in progress, user-scoped history, reset orchestration]
  bounded_contexts: [Trading]
  task_count: 5
---

# Implementation Plan: Portfolio Reset - Story 3

| Field | Value |
|-------|--------|
| Spec | `docs/specs/20260525-251500-portfolio-reset.md` |
| Status | DRAFT |
| Tasks | 5 |
| Branch | `feature/portfolio-reset-story-3` |
| Aspire impact | No topology change |
| Schema impact | No migration |
| Test levels | Api.IntegrationTests + manual UI |
| ADRs required | ADR-007: reset history visibility strategy |
| GitHub | Synced 2026-05-27T21:46:00+07:00 - see GitHub Links |

## Executive summary

Story 3 completes the reset write path started in Stories 1-2 by adding open-order cancellation and user-scoped activity-history clearing behavior. The implementation will atomically cancel the current user's open orders (Pending/PartiallyFilled), release reserved balances/quantities through the reset transaction, and persist a reset cutoff instant used by order/trade history reads. Because the current codebase does not yet expose `/api/orders/open`, `/api/orders/history`, or `/api/trades`, this plan includes a minimal read-side slice for those endpoints with reset-aware filtering to satisfy acceptance criteria. Matching-engine and real-time updates are delivered via Application realtime ports and existing SignalR groups, with explicit non-goal of global tape mutation.

## Goals and non-goals

**Goals**
- G1: Reset cancels all current-user open orders and sets `status = Cancelled` + `terminated_at`.
- G2: Reserved wallet/holding amounts tied to open orders are released before wallet reset to keep invariants valid.
- G3: `GET /api/orders/open`, `GET /api/orders/history`, `GET /api/trades` return empty first page immediately after reset for the same user.
- G4: Order-book/user notifications converge quickly; no canceled user order remains matchable after reset commit.

**Non-goals**
- NG1: Changing global market tape retention or deleting `trades` rows.
- NG2: Implementing Story 4 cooldown enforcement (`RESET_COOLDOWN_ACTIVE`).
- NG3: Multi-symbol behavior (still AAPL-only MVP).
- NG4: New infrastructure or broker/outbox architecture.

## Traceability matrix

| Spec story | Plan task(s) | Test evidence |
|------------|--------------|---------------|
| Story 3 open orders become zero | Task 2, 4 | `ResetPortfolio_WhenUserHasOpenOrders_OpenOrdersEndpointReturnsEmpty` |
| Story 3 no ghost liquidity | Task 3, 5 | Integration test with realtime/order-book assertion + manual checklist |
| Story 3 order + trade history first page empty | Task 1, 4 | `GetMyOrderHistory_AfterReset_ReturnsEmpty`, `GetMyTradeHistory_AfterReset_ReturnsEmpty` |
| Story 3 no-op when no open orders/history | Task 2, 4 | `ResetPortfolio_WithoutOpenOrdersAndHistory_Succeeds` |
| Story 3 counterparty order remains | Task 2, 4 | `ResetPortfolio_CancelsOnlyCurrentUserOpenOrders` |

## Architecture impact

```text
web (reset button + tabs)
  -> POST /api/portfolio/reset
  -> GET /api/orders/open | /api/orders/history | /api/trades
         |
Api Endpoints -> MediatR handlers (Application)
         |
PortfolioResetWriteRepository (Infrastructure, PostgreSQL)
  - cancel user open orders
  - release reservations
  - reset wallet + holdings
  - append portfolio_resets row
         |
Realtime publisher (user:{userId}, market:{symbol}) after commit
```

| Layer | Change summary |
|-------|----------------|
| Domain | Optional order-status constants helper only; no new aggregate behavior required for this slice |
| Application | Add order/trade query handlers + reset orchestration abstractions |
| Infrastructure | Extend reset repository with order cancellation/release logic and read repositories for orders/trades |
| Api | Add three read endpoints + response contracts |
| MatchingEngine | No host-level change; reset publishes cancellation/removal notifications |
| web/ | No new feature work required; uses existing query invalidation in Story 5 plan |
| AppHost | None |

## Data and migration plan

| Artifact | Action | DB reference |
|----------|--------|--------------|
| EF migration | None | Schema already has `orders`, `trades`, `portfolio_resets` |
| PostgreSQL write path | Extend reset transaction order | DB SS10.4 |
| Redis keys | Update/refresh order-book projection through existing publisher flow | DB SS12 |
| History strategy | Read-side cutoff by latest `portfolio_resets.reset_at` per user | DB SS4.10 |

## Open questions

| # | Question | Source | Answer | Status |
|---|----------|--------|--------|--------|
| 1 | Clear history by deleting rows or by read cutoff? | Spec SS13 Q1 | Use read cutoff; keep immutable market records | ✅ |
| 2 | Should reset create synthetic cancellation history entries while history is hidden? | Story 3 wording | No for MVP; cutoff hides all prior rows | ✅ |
| 3 | How to prove 500 ms convergence locally without flaky tests? | PRD SS7.1 | Keep automated bounded polling + manual Aspire timing check | ⏳ |

## Risks and mitigations

| Risk | Likelihood | Impact | Mitigation | Owner task |
|------|------------|--------|------------|------------|
| Missing order/trade read stack causes oversized scope | H | H | Add minimal read contracts/endpoints only for required paths | Task 1 |
| Reset writes bypass domain cancellation semantics | M | M | Centralize SQL update rules in one repository + explicit tests for release math | Task 2 |
| Realtime convergence assertions flake | M | M | Use polling helper with timeout; keep strict checks in manual checklist | Task 5 |
| Counterparty order accidentally canceled | L | H | Filter updates by `user_id` and add dedicated test | Task 4 |

## Prerequisites

- [ ] Story 2 branch baseline available (wallet/holdings/audit row reset already implemented)
- [ ] Docker/Testcontainers ready for integration tests
- [ ] Existing reset endpoint contract preserved (`POST /api/portfolio/reset` 200 payload)
- [ ] No pending schema changes needed for orders/trades tables

## File structure (planned)

```text
src/
  Contracts/
    Orders/
      OpenOrderDto.cs
      OrderHistoryItemDto.cs
      OrderHistoryResponse.cs
    Trades/
      TradeHistoryItemDto.cs
      TradeHistoryResponse.cs
  Application/
    Abstractions/Persistence/
      IOrderReadRepository.cs
      ITradeReadRepository.cs
      IPortfolioResetWriteRepository.cs              MODIFY
    Orders/Queries/
      GetMyOpenOrdersQuery.cs
      GetMyOpenOrdersQueryHandler.cs
      GetMyOrderHistoryQuery.cs
      GetMyOrderHistoryQueryHandler.cs
    Trades/Queries/
      GetMyTradeHistoryQuery.cs
      GetMyTradeHistoryQueryHandler.cs
    Portfolios/Commands/
      ResetPortfolioCommandHandler.cs                MODIFY
  Infrastructure/
    Persistence/Repositories/
      OrderReadRepository.cs
      TradeReadRepository.cs
      PortfolioResetWriteRepository.cs               MODIFY
    DependencyInjection.cs                           MODIFY
  Api/
    Endpoints/
      OrdersEndpoint.cs
      TradesEndpoint.cs
      PortfolioEndpoint.cs                           MODIFY (response codes if needed)
tests/
  Api.IntegrationTests/
    Orders/
      GetMyOpenOrdersTests.cs
      GetMyOrderHistoryTests.cs
    Trades/
      GetMyTradeHistoryTests.cs
    Portfolios/
      ResetPortfolioTests.cs                         MODIFY
      PortfolioResetTestHelpers.cs                   MODIFY
docs/memory/decisions.md                             MODIFY (ADR-007)
```

## Authorization, session, and domain notes

- **Session model:** Cookie session; all new read endpoints require authorization.
- **Route protection:** owner-scoped reads only (`currentUserAccessor.UserId`).
- **Domain rules to preserve:** BR-04 (user-scoped history empty, market tape unchanged), BR-07 (cancel releases reservations), BR-08 (engine/book updated).

## Progress tracker

### Task 1: Add order/trade read slice with reset-aware filters

| Attribute | Value |
|-----------|--------|
| Spec story | Story 3 |
| Depends on | None |
| Estimated complexity | L |
| Parent story issue | #46 |

#### Objective

Introduce minimal `/api/orders/open`, `/api/orders/history`, and `/api/trades` query pipeline so Story 3 acceptance can be enforced and verified through API-level behavior.

#### Implementation notes

- Create Contracts DTOs for open orders and history items.
- Add read repositories with NoTracking projections and bounded first-page defaults.
- Apply reset cutoff filter: ignore rows with `created_at`/`executed_at` earlier than latest `portfolio_resets.reset_at` for that user.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `src/Api/Endpoints/OrdersEndpoint.cs` | Open/history routes |
| CREATE | `src/Api/Endpoints/TradesEndpoint.cs` | Trade history route |
| CREATE | `src/Application/Orders/Queries/*` | Query contracts + handlers |
| CREATE | `src/Application/Trades/Queries/*` | Query contracts + handlers |
| CREATE | `src/Infrastructure/Persistence/Repositories/OrderReadRepository.cs` | Order reads |
| CREATE | `src/Infrastructure/Persistence/Repositories/TradeReadRepository.cs` | Trade reads |
| MODIFY | `src/Infrastructure/DependencyInjection.cs` | Register new read repositories |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `GetMyOpenOrders_WithSession_ReturnsOnlyCurrentUser` | `tests/Api.IntegrationTests/Orders/GetMyOpenOrdersTests.cs` |
| Integration | `GetMyOrderHistory_AfterReset_ReturnsEmpty` | `tests/Api.IntegrationTests/Orders/GetMyOrderHistoryTests.cs` |
| Integration | `GetMyTradeHistory_AfterReset_ReturnsEmpty` | `tests/Api.IntegrationTests/Trades/GetMyTradeHistoryTests.cs` |

#### Acceptance criteria

- [x] Three endpoints return 200 for authenticated user and 401 when unauthenticated.
- [x] First page after reset is empty for order/trade history.
- [x] Endpoints are owner-scoped and do not leak other users' rows.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | PRD SS6.5/6.6, Tech SS6, DB SS6.5/6.6 |
| PostgreSQL authoritative | Direct EF projections from DB |
| RFC 7807 errors | 401/404 patterns align with existing API |
| ADR needed? | Covered by ADR-007 |

#### Risk

Moderate: new read surface in a codebase that currently has only wallet/portfolio reads.

### Task 2: Extend reset transaction to cancel open orders and release reservations

| Attribute | Value |
|-----------|--------|
| Spec story | Story 3 |
| Depends on | Task 1 |
| Estimated complexity | L |
| Parent story issue | #46 |

#### Objective

Complete DB SS10.4 steps 1-2 before existing wallet/holdings reset so reset never leaves open orders or stale reservations for the current user.

#### Implementation notes

- In `ResetForUserAsync`, load user open orders (`status in (0,1)`), mark canceled (`status=3`, `terminated_at=resetAt`, `updated_at=resetAt`).
- Release reservations:
  - Buy orders release wallet reserved by remaining quantity * price (for limit orders).
  - Sell orders release holding reserved quantity for remaining shares.
- Apply this before wallet force-reset and holdings delete to preserve consistent transition semantics.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `src/Application/Abstractions/Persistence/IPortfolioResetWriteRepository.cs` | Expose cancellation result metadata if needed |
| MODIFY | `src/Infrastructure/Persistence/Repositories/PortfolioResetWriteRepository.cs` | Add cancellation + release logic |
| MODIFY | `tests/Api.IntegrationTests/Portfolios/PortfolioResetTestHelpers.cs` | Seed open orders + trades |
| MODIFY | `tests/Api.IntegrationTests/Portfolios/ResetPortfolioTests.cs` | Reset cancel behavior tests |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `ResetPortfolio_WhenUserHasOpenOrders_CancelsPendingAndPartiallyFilled` | `ResetPortfolioTests.cs` |
| Integration | `ResetPortfolio_CancelsOnlyCurrentUserOpenOrders` | `ResetPortfolioTests.cs` |
| Integration | `ResetPortfolio_ReleasesReservationsBeforeWalletReset` | `ResetPortfolioTests.cs` |

#### Acceptance criteria

- [x] Current-user open orders become canceled.
- [x] Counterparty orders remain unchanged.
- [x] Wallet and holdings remain invariant-safe throughout transaction.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| Async matching | API write path persists cancellation intent; no synchronous matching |
| PostgreSQL authoritative | One transaction in UoW |
| Redis projection | Handed off by realtime/publisher task |
| ADR needed? | No |

#### Risk

High: reservation release math can drift if not aligned with order remainder semantics.

### Task 3: Publish reset cancellation notifications and projection refresh signals

| Attribute | Value |
|-----------|--------|
| Spec story | Story 3 |
| Depends on | Task 2 |
| Estimated complexity | M |
| Parent story issue | #46 |

#### Objective

Ensure cancellation effects are observable in user and market streams after reset commit, avoiding ghost liquidity.

#### Implementation notes

- Reuse `IRealtimeNotificationPublisher` and existing cancellation message contracts.
- Publish one user cancellation notification per canceled order, plus market order-book update trigger for AAPL.
- Keep publication post-commit (existing command flow) to avoid phantom events on rollback.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `src/Application/Portfolios/Commands/ResetPortfolioCommandHandler.cs` | Trigger notification publisher with cancellation results |
| REUSE | `src/Api/Realtime/SignalRRealtimeNotificationPublisher.cs` | Existing adapter |
| MODIFY | `tests/Api.IntegrationTests/Portfolios/ResetPortfolioTests.cs` | Verify notification side effects where feasible |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `ResetPortfolio_PublishesOrderCancellationNotifications` | `ResetPortfolioTests.cs` |
| Manual | Reset with active open orders updates open-order panel quickly | Aspire checklist |

#### Acceptance criteria

- [x] User receives reset-driven cancellation events equivalent to manual cancel.
- [x] Market/order-book view no longer includes user canceled orders after convergence window.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| SignalR | `user:{userId}` and `market:{symbol}` groups |
| RFC 7807 errors | Unchanged |
| Aspire | None |
| ADR needed? | No |

#### Risk

Medium: without full matching engine order-book pipeline in place, some convergence checks remain manual.

### Task 4: History visibility validation and edge-case hardening

| Attribute | Value |
|-----------|--------|
| Spec story | Story 3 |
| Depends on | Task 2 |
| Estimated complexity | M |
| Parent story issue | #46 |

#### Objective

Cover edge/failure paths from Story 3 with repeatable integration tests and finalize reset-aware history behavior.

#### Implementation notes

- Add tests for no-open-order/no-history reset success.
- Add partially-filled order scenario where remainder is canceled and hidden history still returns empty.
- Confirm global-market semantics by proving another user's history remains visible to that user.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `tests/Api.IntegrationTests/Portfolios/ResetPortfolioTests.cs` | Story 3 edge tests |
| MODIFY | `tests/Api.IntegrationTests/Orders/GetMyOrderHistoryTests.cs` | Cutoff behavior |
| MODIFY | `tests/Api.IntegrationTests/Trades/GetMyTradeHistoryTests.cs` | Cutoff behavior |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `ResetPortfolio_WithoutOpenOrdersAndHistory_Succeeds` | `ResetPortfolioTests.cs` |
| Integration | `ResetPortfolio_WithPartiallyFilledOrder_HistoryHiddenForCurrentUser` | `ResetPortfolioTests.cs` |
| Integration | `ResetPortfolio_DoesNotHideCounterpartyOwnHistory` | `GetMyTradeHistoryTests.cs` |

#### Acceptance criteria

- [x] All Story 3 failure/edge paths have deterministic integration coverage.
- [x] No test depends on wall-clock sleeps; polling helper only.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | BR-04, BR-07, BR-08 |
| PostgreSQL authoritative | Verified by direct reads |
| Async matching | No direct match execution in tests |
| ADR needed? | No |

#### Risk

Low once repository and read filters are complete.

### Task 5: Polish, ADR, and manual verification closure

| Attribute | Value |
|-----------|--------|
| Spec story | Polish |
| Depends on | Task 1,2,3,4 |
| Estimated complexity | S |
| Parent story issue | #46 |

#### Objective

Finalize documentation, run regression subset, and record manual checklist outcomes for Story 3.

#### Implementation notes

- Add ADR-007 documenting read-cutoff strategy for "history cleared" without deleting trade records.
- Run focused integration suites for portfolio reset, orders, and trades endpoints.
- Execute manual Aspire checklist on tabs (Open Orders, Order History, Trade History, Holdings).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `docs/memory/decisions.md` | ADR-007 |
| MODIFY | `docs/memory/current-status.md` | completion/next-up update |
| MODIFY | `docs/CHANGELOG.md` | plan entry |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `dotnet test` reset/order/trade subsets | `tests/Api.IntegrationTests/*` |
| Manual | Tabs show empty state after reset | Aspire |

#### Acceptance criteria

- [x] ADR-007 captured and linked.
- [x] Integration subset green.
- [x] Manual checklist completed or explicitly handed off to operator.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | Traceability matrix complete |
| SignalR | Convergence notes documented |
| Aspire | Manual operator validation |
| ADR needed? | Yes - ADR-007 |

#### Risk

None - isolated to verification/documentation.

## Reference files

| File | Why open it |
|------|-------------|
| `src/Application/Portfolios/Commands/ResetPortfolioCommandHandler.cs` | Current reset orchestrator |
| `src/Infrastructure/Persistence/Repositories/PortfolioResetWriteRepository.cs` | Current Story 2 write path |
| `tests/Api.IntegrationTests/Portfolios/ResetPortfolioTests.cs` | Existing reset test baseline |
| `src/Infrastructure/Persistence/Configurations/OrderConfiguration.cs` | Order status and indexes |
| `src/Infrastructure/Persistence/Configurations/TradeConfiguration.cs` | Trade index model |
| `docs/specs/20260525-251500-portfolio-reset.md` | Story 3 acceptance and edge cases |

## Implementation details (for /build)

- Keep `ResetPortfolioCommand` under UoW behavior.
- Introduce repository methods to compute latest reset cutoff once and apply to both order-history and trade-history queries.
- Prefer pagination contract with default page size 25 for history endpoints (align PRD FR-5.2/FR-6.4).
- For open orders, filter `status IN (0,1)` and `created_at >= latestResetAt` if cutoff exists.
- For trade history, filter `(buyer_user_id = user OR seller_user_id = user)` and `executed_at >= latestResetAt`.
- Do not delete from `trades` table; preserve market integrity.

## Verification matrix (plan-level)

| Spec AC | Verified by |
|---------|-------------|
| Open orders zero after reset | Task 2 + open orders integration test |
| No ghost liquidity | Task 3 integration/manual convergence check |
| Order/trade history empty first page | Task 1/4 integration tests |
| No-op reset with no data | Task 4 integration test |
| Counterparty order remains | Task 2/4 integration test |

## Rollback / recovery

- **Code:** revert Story 3 commits.
- **DB:** no schema rollback needed; data effects can be re-tested with fresh seeded users.
- **Redis:** clear projections and rebuild from PostgreSQL if stale.

## Deferred work (Plan B)

- Story 4 cooldown enforcement and user-facing eligibility endpoint.
- Story 5 frontend query invalidation polish and richer realtime payloads.
- Matching-engine dedicated cancellation channel implementation once order pipeline is introduced.

## GitHub Links

| Local ref | Issue # | Type | Title | URL |
|-----------|---------|------|-------|-----|
| `spec.Story 3` | `#46` | Story | US-04 / Story 3: Cancel open orders and clear activity history | [#46](https://github.com/tranvuongduy2003/trading-simulator/issues/46) |
| `epic` | `#43` | Epic | Spec: Portfolio reset (US-04) | [#43](https://github.com/tranvuongduy2003/trading-simulator/issues/43) |


---

<a id="source-20260527-231500-portfolio-reset-story-4md"></a>

## Source 17 of 18: `docs/plans/20260527-231500-portfolio-reset-story-4.md`

---
artifact_type: plan
artifact_version: 1
id: plan-20260527-231500-portfolio-reset-story-4
title: Portfolio Reset - Story 4 (Respect the 24-hour cooldown)
slug: portfolio-reset-story-4
filename_template: 20260527-231500-portfolio-reset-story-4.md
created_at: 2026-05-27T23:15:00+07:00
updated_at: 2026-05-28T12:00:00+07:00
status: complete
owner: engineering
tags: [plan, implementation, trading-simulator, portfolio-reset, cooldown, us-04, story-4]
related_spec: docs/specs/20260525-251500-portfolio-reset.md
related_plans: [docs/plans/20260525-260000-portfolio-reset-story-1.md, docs/plans/20260527-210000-portfolio-reset-story-2.md, docs/plans/20260527-214600-portfolio-reset-story-3.md]
prd_refs: [PRD SS6.1 FR-1.4, PRD SS7.4, PRD SS8.1]
tech_refs: [Tech SS6 CQRS Design, Tech SS8.1 Portfolio endpoints, Tech SS13.2 Trading:PortfolioResetCooldownMinutes, Tech SS16 Error handling, Tech SS17.3 API integration tests]
db_refs: [DB SS4.10 portfolio_resets, DB SS6.9 ix_portfolio_resets_user_time, DB SS10.4 Portfolio Reset]
github:
  repo: tranvuongduy2003/trading-simulator
  epic_issue: null
  story_issue_ids: []
  last_synced_at: null
search_index:
  keywords: [portfolio reset, cooldown, reset cooldown active, nextEligibleAt, 24 hours, eligibility endpoint, reset status, us-04 story-4]
  bounded_contexts: [Trading]
  task_count: 5
---

# Implementation Plan: Portfolio Reset - Story 4

| Field | Value |
|-------|--------|
| Spec | `docs/specs/20260525-251500-portfolio-reset.md` |
| Status | COMPLETE |
| Tasks | 5 |
| Branch | `feature/portfolio-reset-story-4` |
| Aspire impact | No topology change |
| Schema impact | No migration |
| Test levels | Api.IntegrationTests + manual UI |
| ADRs required | None |
| GitHub | Not synced by `/plan` (spec is draft; existing story issue is referenced in GitHub Links) |

## Executive summary

Story 4 adds server-authoritative cooldown enforcement for portfolio reset using `portfolio_resets` as the source of truth and `Trading:PortfolioResetCooldownMinutes` as the policy value. The API will reject early reset attempts with HTTP 422 and stable `RESET_COOLDOWN_ACTIVE`, including `nextEligibleAt` so the UI can render a disabled reset affordance with clear relative-time messaging. To satisfy "disabled without calling reset API", this plan introduces a lightweight eligibility read endpoint and client hook, while preserving Story 1-3 reset behavior and error contracts. Completion means cooldown is enforced end-to-end, tested through integration scenarios, and reflected in user-menu UX without regressions to wallet/order/history behavior already delivered.

## Goals and non-goals

**Goals**
- G1: Enforce rolling 24-hour cooldown on `POST /api/portfolio/reset` from latest successful reset.
- G2: Return 422 `RESET_COOLDOWN_ACTIVE` with `nextEligibleAt` and leave wallet/holdings/orders unchanged.
- G3: Add eligibility read path for UI (`nextEligibleAt`, `isEligible`) so menu can be disabled before reset click.
- G4: Map cooldown API errors in frontend reset flow and update disabled hint reliably across refresh/login.

**Non-goals**
- NG1: Changing Story 2-3 transactional reset semantics or cancellation/history logic.
- NG2: Adding new schema objects or changing `portfolio_resets` structure.
- NG3: Modifying global market tape/candlestick behaviors.
- NG4: Introducing cross-service infrastructure changes (AppHost, broker, outbox).

## Traceability matrix

| Spec story | Plan task(s) | Test evidence |
|------------|--------------|---------------|
| No prior reset -> success and new cooldown window | Task 1, 2 | `ResetPortfolio_FirstReset_SucceedsAndReturnsNextEligibleAt` |
| Last reset 25h ago -> success | Task 2, 4 | `ResetPortfolio_AfterTwentyFiveHours_SucceedsAndAppendsResetRow` |
| Last reset 2h ago -> 422 + nextEligibleAt unchanged state | Task 2, 4 | `ResetPortfolio_WhenCooldownActive_Returns422_WithNextEligibleAt`, `ResetPortfolio_WhenCooldownActive_DoesNotMutateState` |
| Cooldown active -> UI disabled without reset POST | Task 3, 5 | menu eligibility query/manual UI checklist |

## Architecture impact

```text
web UserMenu
  -> GET /api/portfolio/reset/eligibility
  -> POST /api/portfolio/reset
          |
Api Endpoints (PortfolioEndpoint)
          |
Application
  ResetPortfolioCommandHandler + eligibility query
          |
Infrastructure
  portfolio_resets latest-reset lookup (PostgreSQL index ix_portfolio_resets_user_time)
```

| Layer | Change summary |
|-------|----------------|
| Domain | No aggregate rule changes; cooldown remains application policy |
| Application | Add cooldown checker abstraction + eligibility query; map cooldown error to validation result |
| Infrastructure | Add/read method for latest reset timestamp lookup by user |
| Api | Add eligibility endpoint and 422 response contract metadata |
| MatchingEngine | No change |
| web/ | Add eligibility query hook and reuse existing reset-disable UX path |
| AppHost | None |

## Data and migration plan

| Artifact | Action | DB reference |
|----------|--------|--------------|
| EF migration | None | DB SS4.10 already present |
| PostgreSQL query | Read latest `portfolio_resets.reset_at` by user (DESC limit 1) | DB SS6.9 |
| Redis keys | None | N/A |
| Book recovery | N/A | Tech SS7.7 |

## Open questions

| # | Question | Source | Answer | Status |
|---|----------|--------|--------|--------|
| 1 | Should cooldown block be 422 or 429? | Spec Story 4 | Use 422 to match current RFC7807 validation mapping | ✅ |
| 2 | Add eligibility endpoint or rely on cached `nextEligibleAt` only? | Spec Story 4 UI | Add endpoint to satisfy disabled-on-open without reset POST | ✅ |
| 3 | Should eligibility endpoint include server current time? | UI timing precision | Deferred; initial response only needs `isEligible` + `nextEligibleAt` | ⏳ |

## Risks and mitigations

| Risk | Likelihood | Impact | Mitigation | Owner task |
|------|------------|--------|------------|------------|
| Cooldown race around boundary timestamp | M | M | Compare against clock consistently in handler; integration tests for boundary windows | Task 2, 4 |
| UI displays stale eligibility from sessionStorage | M | M | Prefer server eligibility query; treat storage as fallback only | Task 3 |
| Missing 422 mapping in frontend causes generic error copy | M | L | Extend reset error mapper with `RESET_COOLDOWN_ACTIVE` and computed hint | Task 3 |
| Regression in existing Story 1-3 reset success path | L | H | Keep focused regression tests for successful reset + wallet contract | Task 4 |

## Prerequisites

- [ ] Story 3 branch baseline merged or available for cherry-pick
- [ ] Existing reset endpoint contract and tests green
- [ ] Testcontainers integration test environment available
- [ ] No pending schema migrations required

## File structure (planned)

```text
src/
  TradingSimulator.Application/
    Abstractions/Persistence/
      IPortfolioResetReadRepository.cs           CREATE
    Portfolios/Queries/
      GetPortfolioResetEligibilityQuery.cs       CREATE
      GetPortfolioResetEligibilityQueryHandler.cs CREATE
    Portfolios/Commands/
      ResetPortfolioCommandHandler.cs            MODIFY
      PortfolioResetErrors.cs                    MODIFY
  TradingSimulator.Infrastructure/
    Persistence/Repositories/
      PortfolioResetReadRepository.cs            CREATE
    DependencyInjection.cs                       MODIFY
  TradingSimulator.Contracts/
    Portfolio/
      PortfolioResetEligibilityResponse.cs       CREATE
  TradingSimulator.Api/
    Endpoints/
      PortfolioEndpoint.cs                       MODIFY
tests/
  Api.IntegrationTests/
    Portfolios/
      ResetPortfolioTests.cs                     MODIFY
web/
  src/features/portfolio-reset/
    api.ts                                       MODIFY
    reset-eligibility.ts                         MODIFY
    map-reset-error.ts                           MODIFY
    use-reset-portfolio.ts                       MODIFY
  src/layouts/app-layout.tsx                     MODIFY
```

## Authorization, session, and domain notes

- **Session model:** authenticated cookie session required for reset and eligibility checks.
- **Route protection:** both reset POST and eligibility GET require authorization (401 on missing session).
- **Domain rules preserved:** BR-02 rolling cooldown, BR-03 reset semantics, BR-06 atomic reset transaction.

## Progress tracker

### Task 1: Add cooldown read abstraction and eligibility query skeleton

| Attribute | Value |
|-----------|--------|
| Spec story | Story 4 |
| Depends on | None |
| Estimated complexity | M |
| Parent story issue | #47 (existing) |

#### Objective

Introduce a small read-side slice that returns cooldown eligibility for the current user from authoritative reset history, enabling both API handler reuse and UI preflight state.

#### Implementation notes

- Add `IPortfolioResetReadRepository` with latest-reset lookup.
- Add `GetPortfolioResetEligibilityQuery`/handler returning `isEligible`, `nextEligibleAt`.
- Use `IClock` and `TradingOptions.PortfolioResetCooldownMinutes` for deterministic calculations.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `src/Application/Abstractions/Persistence/IPortfolioResetReadRepository.cs` | Read port |
| CREATE | `src/Application/Portfolios/Queries/GetPortfolioResetEligibilityQuery.cs` | Query request |
| CREATE | `src/Application/Portfolios/Queries/GetPortfolioResetEligibilityQueryHandler.cs` | Eligibility logic |
| CREATE | `src/Infrastructure/Persistence/Repositories/PortfolioResetReadRepository.cs` | PG latest reset read |
| MODIFY | `src/Infrastructure/DependencyInjection.cs` | Register read repository |
| CREATE | `src/Contracts/Portfolio/PortfolioResetEligibilityResponse.cs` | Response DTO |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `GetResetEligibility_WithoutPriorReset_IsEligible` | `tests/Api.IntegrationTests/Portfolios/ResetPortfolioTests.cs` |
| Integration | `GetResetEligibility_WithRecentReset_ReturnsNextEligibleAt` | `tests/Api.IntegrationTests/Portfolios/ResetPortfolioTests.cs` |

#### Acceptance criteria

- [x] Eligibility query returns deterministic values for no-reset and recent-reset users.
- [x] Cooldown calculation uses configured minutes (default 1440).
- [x] No layering violation (Application only depends on abstraction).

#### Deviations (Task 1)

- Added `GET /api/portfolio/reset/eligibility` in Task 1 (planned for Task 3) so cookie-authenticated integration tests can exercise the query without MediatR-only setup.
- OpenAPI path/schema added in Task 1 (`contracts/openapi/api.v1.yaml`); run `yarn --cwd web api:codegen` before frontend work in Task 3.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | PRD FR-1.4, Tech SS13.2, DB SS6.9 |
| PostgreSQL authoritative | Yes |
| RFC 7807 errors | N/A for query success path |
| Aspire | None |
| ADR needed? | No |

#### Risk

Low; isolated read-side addition.

### Task 2: Enforce cooldown in reset command handler with 422 error

| Attribute | Value |
|-----------|--------|
| Spec story | Story 4 |
| Depends on | Task 1 |
| Estimated complexity | M |
| Parent story issue | #47 (existing) |

#### Objective

Block reset requests inside active cooldown window while preserving existing successful-reset transaction behavior and returning a stable cooldown error payload.

#### Implementation notes

- Inject cooldown read abstraction into `ResetPortfolioCommandHandler`.
- On active cooldown: return validation error `RESET_COOLDOWN_ACTIVE` with `nextEligibleAt` in message detail.
- Keep in-flight guard semantics and existing success payload unchanged.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `src/Application/Portfolios/Commands/ResetPortfolioCommandHandler.cs` | Cooldown guard before reset write |
| MODIFY | `src/Application/Portfolios/Commands/PortfolioResetErrors.cs` | Add cooldown error builder |
| MODIFY | `src/Api/Endpoints/PortfolioEndpoint.cs` | Document 422 for reset (Task 2) |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `ResetPortfolio_WhenCooldownActive_Returns422_WithNextEligibleAt` | `tests/Api.IntegrationTests/Portfolios/ResetPortfolioTests.cs` |
| Integration | `ResetPortfolio_AfterCooldownWindow_Succeeds` | `tests/Api.IntegrationTests/Portfolios/ResetPortfolioTests.cs` |

#### Acceptance criteria

- [x] Reset within cooldown returns 422 with code `RESET_COOLDOWN_ACTIVE`.
- [x] Error detail includes parseable `nextEligibleAt` timestamp.
- [x] No wallet/order/holding mutation occurs on cooldown rejection.

#### Deviations (Task 2)

- Added optional `Error.Extensions` on application errors and mapped them to RFC 7807 `extensions` (includes `nextEligibleAt` ISO-8601 for `RESET_COOLDOWN_ACTIVE`).

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | PRD Story 4 failure path, Tech SS16, DB SS4.10 |
| Async matching | unchanged |
| PostgreSQL authoritative | yes |
| RFC 7807 errors | 422 via `ErrorType.Validation` |
| ADR needed? | No |

#### Risk

Medium: details extension shape may require small Result/Problem mapping changes if currently unavailable.

### Task 3: Add eligibility endpoint and frontend integration

| Attribute | Value |
|-----------|--------|
| Spec story | Story 4 |
| Depends on | Task 1 |
| Estimated complexity | M |
| Parent story issue | #47 (existing) |

#### Objective

Make reset menu state server-driven so users can see disabled/reset-ready state without triggering a reset POST.

#### Implementation notes

- Add `GET /api/portfolio/reset/eligibility` endpoint (authorized).
- Update frontend API/hook to fetch eligibility in user menu.
- Keep `sessionStorage` helper as fallback cache, but prioritize server values.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `src/Api/Endpoints/PortfolioEndpoint.cs` | Add eligibility route |
| MODIFY | `web/src/features/portfolio-reset/api.ts` | Add eligibility API call |
| MODIFY | `web/src/features/portfolio-reset/reset-eligibility.ts` | Integrate query-backed eligibility |
| MODIFY | `web/src/layouts/app-layout.tsx` | Drive menu disabled state from eligibility |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `GetResetEligibility_RequiresAuthentication` | `tests/Api.IntegrationTests/Portfolios/ResetPortfolioTests.cs` |
| Manual | User menu shows disabled hint while cooldown active before reset click | `web/` checklist |

#### Acceptance criteria

- [x] Eligibility endpoint returns `isEligible` + `nextEligibleAt`.
- [x] User menu disables reset action and shows next-available hint when cooldown active.
- [x] UI does not call reset POST when disabled.

#### Deviations (Task 3)

- Eligibility route added in Task 1; Task 3 scoped to frontend query + invalidation only.
- `app-layout.tsx` unchanged — existing `useResetEligibility` hook now server-backed (menu `disabled` / `title` unchanged).

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | PRD Story 4 UI path, Tech SS8.1 |
| SignalR | N/A |
| Aspire | None |
| ADR needed? | No |

#### Risk

Medium: balancing server query + local cached hints without flicker.

### Task 4: Integration regression and edge-path hardening

| Attribute | Value |
|-----------|--------|
| Spec story | Story 4 |
| Depends on | Task 2, 3 |
| Estimated complexity | M |
| Parent story issue | #47 (existing) |

#### Objective

Prove Story 4 acceptance and protect existing Story 1-3 reset guarantees through focused integration coverage.

#### Implementation notes

- Add first-reset success and cooldown-blocked cases.
- Verify cooldown uses latest reset row and allows reset after window.
- Assert no side effects when blocked by cooldown.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `tests/Api.IntegrationTests/Portfolios/ResetPortfolioTests.cs` | Story 4 scenarios |
| REUSE | `tests/Api.IntegrationTests/Portfolios/PortfolioResetTestHelpers.cs` | Seed reset timestamps |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `ResetPortfolio_FirstReset_SucceedsAndReturnsNextEligibleAt` | `ResetPortfolioTests.cs` |
| Integration | `ResetPortfolio_WhenCooldownActive_DoesNotMutateState` | `ResetPortfolioTests.cs` |
| Integration | `ResetPortfolio_AfterTwentyFiveHours_SucceedsAndAppendsResetRow` | `ResetPortfolioTests.cs` |

#### Acceptance criteria

- [x] Story 4 happy/failure paths pass in integration tests.
- [x] Existing reset success tests remain green.
- [x] OpenAPI-documented response codes match runtime behavior.

#### Deviations (Task 4)

- Renamed `ResetPortfolio_AfterCooldownWindow_Succeeds` → `ResetPortfolio_AfterTwentyFiveHours_SucceedsAndAppendsResetRow` (plan naming).
- Added `ResetPortfolio_WhenCooldownActive_UsesLatestResetRow` to prove cooldown reads the latest `portfolio_resets` row (older expired row does not bypass guard).

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| Tech SS17 | Testcontainers integration coverage |
| DB SS6.9 | Uses latest reset index access path |
| RFC 7807 | Code and status stable |
| ADR needed? | No |

#### Risk

Low if existing reset test helpers are reused.

### Task 5: Polish and manual UX verification

| Attribute | Value |
|-----------|--------|
| Spec story | Polish |
| Depends on | Task 3, 4 |
| Estimated complexity | S |
| Parent story issue | #47 (existing) |

#### Objective

Finalize user-facing cooldown messaging and complete manual validation checklist across refresh/tab scenarios.

#### Implementation notes

- Extend reset error mapping for cooldown response details.
- Confirm disabled copy remains understandable (`nextEligibleAt` relative time).
- Validate behavior on page refresh and second tab focus.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `web/src/features/portfolio-reset/map-reset-error.ts` | Cooldown-specific error mapping |
| MODIFY | `web/src/features/portfolio-reset/use-reset-portfolio.ts` | Align toast/error behavior |
| MODIFY | `docs/memory/current-status.md` | Mark plan completion and next step |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Cooldown active message + disabled menu in same tab | `web/` |
| Manual | Refresh/another tab reflects eligibility on next fetch | `web/` |

#### Acceptance criteria

- [x] Error copy for cooldown is explicit and actionable.
- [x] Manual checklist confirms disabled behavior without reset POST (operator handoff below).
- [x] Story 4 automation complete on `feature/portfolio-reset-story-4`.

#### Manual UI checklist (operator)

1. Sign in, perform one successful reset → menu **Reset portfolio** disabled with relative-time hint; no second POST while disabled.
2. Open confirm dialog only when menu item is enabled; cancel leaves state unchanged.
3. If eligible, force early POST (e.g. devtools) while cooldown active → dialog shows **You can reset again in …**; wallet unchanged on refetch.
4. Refresh page while cooldown active → menu stays disabled after eligibility GET (no reset POST).
5. Second tab: focus window → eligibility refetch; disabled state matches server.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD SS7.4 | Clear actionable message |
| frontend.mdc | Query-driven server state |
| design-system.mdc | Accessible disabled state and labels |
| ADR needed? | No |

#### Risk

None - polish only.

## Reference files

| File | Why open it |
|------|-------------|
| `src/Application/Portfolios/Commands/ResetPortfolioCommandHandler.cs` | Current reset orchestration point |
| `src/Infrastructure/Persistence/Repositories/PortfolioResetWriteRepository.cs` | Current reset transaction behavior |
| `web/src/features/portfolio-reset/reset-eligibility.ts` | Existing client-side cooldown cache logic |
| `web/src/layouts/app-layout.tsx` | User-menu disable wiring |
| `tests/Api.IntegrationTests/Portfolios/ResetPortfolioTests.cs` | Existing Story 1-3 reset tests |
| `docs/specs/20260525-251500-portfolio-reset.md` | Story 4 acceptance criteria |

## Implementation details (for /build)

- Cooldown formula: `nextEligibleAt = latestResetAt + PortfolioResetCooldownMinutes`.
- `isEligible = latestResetAt is null || clock.UtcNow >= nextEligibleAt`.
- For blocked reset, return `Error.Validation("RESET_COOLDOWN_ACTIVE", "...nextEligibleAt...")`; if structured extension support is needed, add a minimal convention in result/problem mapping.
- Eligibility endpoint returns a small DTO: `{ isEligible, nextEligibleAt }` where `nextEligibleAt` can be null for users with no prior reset.
- Keep existing in-flight guard behavior (`RESET_IN_PROGRESS`) independent from cooldown logic.

## Verification matrix (plan-level)

| Spec AC | Verified by |
|---------|-------------|
| First reset records row and sets 24h eligibility | Task 2 + Task 4 tests |
| Reset after 25h succeeds | Task 2 + Task 4 tests |
| Reset at 2h blocked with 422 and `nextEligibleAt` | Task 2 + Task 4 tests |
| UI shows disabled/reset-available timing without reset POST | Task 3 + Task 5 manual checks |

## Rollback / recovery

- **Code:** revert Story 4 commits.
- **DB:** no migration rollback required.
- **Redis:** not affected by cooldown feature directly.

## Deferred work (Plan B)

- Add explicit server `now` in eligibility response for tighter client relative-time precision.
- Add telemetry counter for `RESET_COOLDOWN_ACTIVE` blocks.

## GitHub Links

| Local ref | Issue # | Type | Title | URL |
|-----------|---------|------|-------|-----|
| `spec.Story 4` | `#47` | Story | US-04 / Story 4: Respect the 24-hour cooldown | [#47](https://github.com/tranvuongduy2003/trading-simulator/issues/47) |


---

<a id="source-20260528-003204-portfolio-reset-story-5md"></a>

## Source 18 of 18: `docs/plans/20260528-003204-portfolio-reset-story-5.md`

---
artifact_type: plan
artifact_version: 1
id: plan-20260528-003204-portfolio-reset-story-5
title: Portfolio Reset - Story 5 (Consistent data everywhere after reset)
slug: portfolio-reset-story-5
filename_template: 20260528-003204-portfolio-reset-story-5.md
created_at: 2026-05-28T00:32:04+07:00
updated_at: 2026-05-28T18:00:00+07:00
status: complete
owner: engineering
tags: [plan, implementation, trading-simulator, portfolio-reset, tanstack-query, signalr, us-04, story-5]
related_spec: docs/specs/20260525-251500-portfolio-reset.md
related_plans:
  - docs/plans/20260525-260000-portfolio-reset-story-1.md
  - docs/plans/20260527-210000-portfolio-reset-story-2.md
  - docs/plans/20260527-214600-portfolio-reset-story-3.md
  - docs/plans/20260527-231500-portfolio-reset-story-4.md
prd_refs: [PRD SS5.1 US-04, PRD SS6.1 FR-1.4, PRD SS6.6 FR-6.2, PRD SS7.1, PRD SS7.4, PRD SS8.1]
tech_refs: [Tech SS5.4 PortfolioResetEvent, Tech SS8.1 Portfolio endpoints, Tech SS9.2 SignalR user group, Tech SS11 Frontend, Tech SS17.3 API integration tests]
db_refs: [DB SS4.2 wallets, DB SS4.3 portfolios, DB SS4.5 orders, DB SS4.6 trades, DB SS10.4 Portfolio Reset]
github:
  repo: tranvuongduy2003/trading-simulator
  epic_issue: 43
  story_issue_ids: [48]
  last_synced_at: 2026-05-28T00:32:04+07:00
search_index:
  keywords:
    [
      portfolio reset,
      story 5,
      query invalidation,
      tanstack query,
      wallet refetch,
      read-your-writes,
      signalr balance updated,
      order cancellation notified,
      multi-tab stale,
      virtual cash card,
      wallet top bar chip,
      EC-10,
      EC-11,
    ]
  bounded_contexts: [Trading]
  task_count: 6
---

# Implementation Plan: Portfolio Reset - Story 5

| Field | Value |
|-------|--------|
| Spec | `docs/specs/20260525-251500-portfolio-reset.md` (Story 5) |
| GitHub story | [#48 — Consistent data everywhere after reset](https://github.com/tranvuongduy2003/trading-simulator/issues/48) |
| Epic | [#43 — Portfolio reset (US-04)](https://github.com/tranvuongduy2003/trading-simulator/issues/43) |
| Depends on | Stories 1–4 shipped (reset POST, wallet/holdings write, order cancel + history cutoff, cooldown + eligibility) |
| Status | COMPLETE (automation); manual UI checklist pending operator |
| Tasks | 6 |
| Branch | `feature/portfolio-reset-story-5` |
| Aspire impact | No |
| Schema impact | No |
| Test levels | Manual UI (primary) · optional Api.IntegrationTests regression only |
| ADRs required | ADR-008 (post-reset client cache strategy) |
| GitHub | Synced 2026-05-28 — see GitHub Links |

## Executive summary

Story 5 closes the portfolio-reset UX loop on the **web client**: after a successful `POST /api/portfolio/reset`, every portfolio-related surface must show post-reset figures within **2 s** without logout. Backend work is already in place from Stories 2–3 (`ResetPortfolioCommandHandler` publishes `BalanceUpdated` and per-order `OrderCancellationNotified`; integration tests assert the publisher). The remaining gap is **TanStack Query cache coordination** — `use-reset-portfolio.ts` still has a Story 5 TODO, the success toast still defers holdings/orders to a “later release”, `['portfolio']` is not user-scoped and uses a 30s `staleTime` (breaks EC-11 multi-tab), and there are **no frontend hooks** yet for `GET /api/orders/open`, `/api/orders/history`, or `/api/trades` despite OpenAPI/codegen existing. This plan adds a shared invalidation helper, seeds wallet cache from the **200 response body** (authoritative, not client-fabricated), aligns portfolio/orders/trades queries with virtual-cash Story 4 refetch policy, extends the SignalR query bridge for trades, and mounts a minimal tabbed activity panel on the trading page so acceptance criteria for empty post-reset tabs are observable.

## Goals and non-goals

**Goals**

- G1: After reset success, `VirtualCashCard` and `WalletTopBarChip` show **$100,000.00** available within **2 s** (US-03, ADR-004).
- G2: Invalidate/refetch `wallet`, `portfolio`, `orders/open`, `orders/history`, and `trades` query prefixes on reset success; no pre-reset figures after refetch completes.
- G3: Rely on existing SignalR user-group messages; client bridge invalidates matching query keys (no new hub methods).
- G4: On wallet refetch **500** after successful reset, show US-03 error state — do not hardcode $100k in JSX.
- G5: Multi-tab stale holdings/cash corrected on window focus via per-query `refetchOnWindowFocus` (EC-11, compatible with virtual cash Story 4).

**Non-goals**

- NG1: New SignalR event types or backend reset transaction changes (Stories 2–3 own BR-08/BR-11).
- NG2: Full PRD §8.1 trading terminal layout (order book, chart, order form) — only the **bottom activity tabs** slice needed for Story 5 AC.
- NG3: Automated frontend tests (MVP policy).
- NG4: Order placement UI (EC-10 documented for manual verification when place-order ships).
- NG5: Changing global `refetchOnWindowFocus` in `providers.tsx`.

## Traceability matrix

| Spec story | Plan task(s) | Test evidence |
|------------|--------------|---------------|
| Cash card + top bar $100k within 2s | Task 2, 6 | Manual steps 1–2; reset response seeds `['wallet', userId]` |
| Invalidate wallet, portfolio, orders, trades | Task 1, 2, 3, 4 | Manual step 3; devtools Query cache inspection |
| SignalR cancel/balance on `user:{userId}` | Task 6 | REUSE `ResetPortfolio_PublishesOrderCancellationNotifications`; manual hub debug |
| Wallet refetch 500 → error, no fabricated $100k | Task 2, 6 | Manual step 4; no `100000` literals in components |
| Multi-tab focus → post-reset data | Task 3, 6 | Manual step 5; portfolio `staleTime: 0` + `refetchOnWindowFocus` |
| EC-10 place order after reset | — | Manual note when order form exists |
| EC-11 multi-tab stale | Task 3, 6 | Manual step 5 |

## Architecture impact

```text
User confirms reset (AppLayout / ResetPortfolioDialog)
  -> POST /api/portfolio/reset (200 + wallet snapshot)     [existing Api]
  -> onSuccess:
       seedWalletQueryData from response.wallet
       invalidatePortfolioPanels(queryClient)             [NEW web helper]
  -> parallel refetch:
       GET /api/wallet | /portfolio | /orders/open | /orders/history | /trades
  -> VirtualCashCard + WalletTopBarChip + Trading tabs re-render

Parallel path (already live):
  ResetPortfolioCommandHandler
    -> NotifyOrderCancellationAsync (per cancelled order)
    -> NotifyBalanceUpdatedAsync
    -> PublishOrderBookUpdatedAsync
  -> SignalR user:{userId}
  -> createSimulationHubQueryBridge invalidates overlapping keys
```

| Layer | Change summary |
|-------|----------------|
| Domain | **REUSE** — no change |
| Application | **REUSE** — `ResetPortfolioCommandHandler` realtime publishes |
| Infrastructure | **REUSE** — reset write + read repositories |
| Api | **REUSE** — endpoints from Stories 2–4 |
| MatchingEngine | **REUSE** — book updates via existing publisher |
| web/ | **MODIFY/CREATE** — query keys, invalidation, hooks, trading tabs, SignalR bridge, reset hook |
| AppHost | None |

## Data and migration plan

| Artifact | Action | DB reference |
|----------|--------|--------------|
| EF migration | None | — |
| Redis keys | None (client refetch only) | DB SS12 |
| Book recovery | N/A | Tech SS7 |

## Open questions

| # | Question | Source | Answer | Status |
|---|----------|--------|--------|--------|
| 1 | Is `POST` 200 `wallet` snapshot sufficient for immediate $100k display if `GET /api/wallet` refetch fails? | Story 5 failure AC | Yes — 200 body is server-authoritative; show US-03 error only when **no** successful wallet payload in cache (do not hardcode 100k in JSX) | ✅ |
| 2 | Mount bottom tabs on `TradingPage` vs separate `OrdersPage`? | PRD SS8.1 vs current routes | Minimal tabbed section on **TradingPage** (Holdings tab reuses existing card content) | ✅ |
| 3 | User-scope `['portfolio']` → `['portfolio', userId]`? | Story 3 privacy pattern | Yes — match `['wallet', userId]` | ✅ |

## Risks and mitigations

| Risk | Likelihood | Impact | Mitigation | Owner task |
|------|------------|--------|------------|------------|
| Invalidation prefix too narrow (misses keyed queries) | M | H | Central `portfolioPanelQueryKeys` + prefix invalidation | Task 1 |
| `setQueryData` masks failed refetch while showing wrong user | L | H | Scope keys with `userId`; `canDisplayWallet` guard unchanged | Task 2 |
| SignalR bridge omits `trades` | M | M | Add `['trades']` invalidation on cancel/balance | Task 6 |
| Bottom panel scope creep | M | M | Tab UI read-only tables + empty states only | Task 5 |
| Portfolio 30s staleTime blocks EC-11 | H | M | Align with wallet: `staleTime: 0`, `refetchOnWindowFocus: true` | Task 3 |

## Prerequisites

- [ ] Stories 1–4 merged or available on branch baseline (`feature/portfolio-reset-story-4` or `main`)
- [ ] `aspire run` / env-doctor green
- [ ] `yarn --cwd web build` passes on baseline
- [ ] Operator can seed user with open orders + history for manual reset walkthrough

## File structure (planned)

```text
web/src/
  lib/
    query-keys.ts                              CREATE  portfolio panel key factories
  features/
    portfolio-reset/
      invalidate-portfolio-panels.ts           CREATE
      use-reset-portfolio.ts                   MODIFY
    trading/
      hooks/
        use-portfolio-query.ts                 CREATE
      components/
        portfolio-activity-tabs.tsx            CREATE  open/history/trades/holdings tabs
      pages/
        trading-page.tsx                       MODIFY
    orders/
      api.ts                                   CREATE
      hooks/
        use-open-orders-query.ts               CREATE
        use-order-history-query.ts             CREATE
    trades/
      api.ts                                   CREATE
      hooks/
        use-trade-history-query.ts             CREATE
  lib/signalr/
    interceptors.ts                            MODIFY  trades invalidation
  features/auth/
    clear-user-queries.ts                      MODIFY  orders/trades prefixes
docs/memory/
  decisions.md                                 MODIFY  ADR-008
```

## Authorization, session, and domain notes

- **Session model:** Cookie session unchanged; all reads require authentication.
- **Route protection:** Reset and panel reads only when `authStatus === 'authenticated'`.
- **Domain rules:** BR-11 notifications already emitted server-side; client must not assume SignalR delivery — always invalidate on POST success.
- **Read-your-writes:** Prefer reset **200 wallet snapshot** for immediate UI, then confirm via `GET /api/wallet` refetch (virtual cash Story 4 policy).

## Progress tracker

### Task 1: Skeleton — shared query keys and post-reset invalidation

| Attribute | Value |
|-----------|--------|
| Spec story | Story 5 |
| Depends on | None |
| Estimated complexity | S |
| Parent story issue | #48 |

#### Objective

Introduce a single module defining portfolio-panel TanStack Query keys and an `invalidatePortfolioPanels` helper; wire `useResetPortfolio` `onSuccess` to call it so reset success immediately marks all panel queries stale (observable in React Query Devtools) even before UI surfaces mount.

#### Implementation notes

- Keys (prefix-friendly for `invalidateQueries`):
  - `['wallet', userId]`
  - `['portfolio', userId]`
  - `['orders', 'open', userId]`
  - `['orders', 'history', userId]`
  - `['trades', userId]`
- `invalidatePortfolioPanels(queryClient, userId)` calls `invalidateQueries` with each prefix.
- Keep existing eligibility invalidation.
- Remove Story 5 TODO comment once wired.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `web/src/lib/query-keys.ts` | Key factories |
| CREATE | `web/src/features/portfolio-reset/invalidate-portfolio-panels.ts` | Invalidation helper |
| MODIFY | `web/src/features/portfolio-reset/use-reset-portfolio.ts` | Call helper on success |
| REUSE | `web/src/features/trading/hooks/use-wallet-query.ts` | Wallet key shape |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Reset success → devtools shows invalidation for wallet/portfolio/orders/trades keys | `web/` |

#### Acceptance criteria

- [x] Successful reset triggers invalidation for all five panel prefixes
- [x] No change to error/cooldown paths
- [x] `yarn --cwd web lint` passes

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech | US-04 Story 5; Tech SS11 |
| PostgreSQL authoritative | Invalidation triggers refetch, not local fabrication |
| SignalR | N/A this task |
| ADR needed? | No |

#### Risk

None — isolated wiring.

---

### Task 2: Wallet read-your-writes from reset response

| Attribute | Value |
|-----------|--------|
| Spec story | Story 5 (cash surfaces) |
| Depends on | Task 1 |
| Estimated complexity | S |
| Parent story issue | #48 |

#### Objective

Within **2 s** of reset success, `VirtualCashCard` and `WalletTopBarChip` show **$100,000.00** available using the **200 response wallet snapshot**, then converge on `GET /api/wallet` after invalidation refetch.

#### Implementation notes

- Reuse `seedWalletQueryData` from `prefetch-wallet.ts` with `response.wallet` mapped to `WalletResponse` shape (`userId` from auth store).
- Call `invalidatePortfolioPanels` after seeding so refetch still runs.
- Update success toast copy — remove “later release” deferral; state holdings/orders/history refresh now.
- **Failure path:** never assign literal `100000` in JSX; if refetch errors and cache empty, `WALLET_LOAD_ERROR_MESSAGE` displays (US-03).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `web/src/features/portfolio-reset/use-reset-portfolio.ts` | Seed + invalidate |
| REUSE | `web/src/features/auth/prefetch-wallet.ts` | `seedWalletQueryData` |
| REUSE | `web/src/features/trading/components/virtual-cash-card.tsx` | Display |
| REUSE | `web/src/features/trading/components/wallet-top-bar-chip.tsx` | Display |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Reset depleted wallet → chip + card show $100,000.00 without reload | `web/` |

#### Acceptance criteria

- [x] Top bar and virtual cash card update immediately after successful reset
- [x] Refetch still issued; no hardcoded $100k in components
- [x] Success toast reflects full panel refresh

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD | US-03 + US-04 Story 5 |
| ADR-004 | Top-bar chip reuses `useWalletQuery` |
| Virtual cash Story 4 | Compatible `staleTime: 0` + focus refetch |

#### Risk

Low — uses existing normalize/ display guards.

---

### Task 3: User-scoped portfolio query and multi-tab refetch

| Attribute | Value |
|-----------|--------|
| Spec story | Story 5 (holdings); EC-11 |
| Depends on | Task 1 |
| Estimated complexity | S |
| Parent story issue | #48 |

#### Objective

Holdings on the trading view show **zero AAPL** after reset and stale tabs recover on focus without a full logout.

#### Implementation notes

- Extract `usePortfolioQuery` mirroring `useWalletQuery`: `queryKey: ['portfolio', userId]`, `staleTime: 0`, `refetchOnWindowFocus: true`, `authApi.getPortfolio`.
- Replace inline `useQuery` in `trading-page.tsx`.
- Update `clear-user-queries.ts` if needed (prefix `['portfolio']` already removes all).
- Post-reset: invalidation refetch shows empty holdings / zero available quantity.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `web/src/features/trading/hooks/use-portfolio-query.ts` | Portfolio hook |
| MODIFY | `web/src/features/trading/pages/trading-page.tsx` | Use hook |
| MODIFY | `web/src/features/auth/clear-user-queries.ts` | Confirm orders/trades prefixes (Task 6 may finish) |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Two tabs: reset in tab A → focus tab B → holdings show 0 | `web/` |

#### Acceptance criteria

- [x] Holdings table shows 0 available AAPL after reset refetch
- [x] Focused stale tab refetches portfolio without 30s stale window

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| EC-11 | Multi-tab focus refetch |
| Redis projection | N/A — PG read |

#### Risk

Key migration from `['portfolio']` — ensure no stale callers remain.

---

### Task 4: Orders and trades query hooks

| Attribute | Value |
|-----------|--------|
| Spec story | Story 5 (activity queries) |
| Depends on | Task 1 |
| Estimated complexity | M |
| Parent story issue | #48 |

#### Objective

Add typed API clients and TanStack Query hooks for owner-scoped open orders, order history, and trade history using generated OpenAPI types where available.

#### Implementation notes

- Endpoints: `GET /api/orders/open`, `GET /api/orders/history`, `GET /api/trades` (already in `api-schema.ts`).
- Hooks: `useOpenOrdersQuery`, `useOrderHistoryQuery`, `useTradeHistoryQuery` with user-scoped keys matching Task 1.
- Same refetch policy as wallet: `staleTime: 0`, `refetchOnWindowFocus: true`, `enabled` when authenticated.
- Do not duplicate server state in Zustand.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `web/src/features/orders/api.ts` | HTTP wrappers |
| CREATE | `web/src/features/orders/hooks/use-open-orders-query.ts` | Open orders |
| CREATE | `web/src/features/orders/hooks/use-order-history-query.ts` | Order history |
| CREATE | `web/src/features/trades/api.ts` | HTTP wrappers |
| CREATE | `web/src/features/trades/hooks/use-trade-history-query.ts` | Trade history |
| REUSE | `web/src/generated/api-schema.ts` | Types/paths |
| REUSE | `src/Contracts/Orders/*.cs`, `src/Contracts/Trades/*.cs` | Field shapes |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Hooks return data before reset, empty arrays after reset | `web/` |

#### Acceptance criteria

- [x] Hooks fetch successfully on trading view mount
- [x] Keys align with `invalidatePortfolioPanels` prefixes
- [x] `yarn --cwd web build` passes

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| api-guidelines | Owner-scoped reads |
| ADR-007 | History cutoff — empty after reset |

#### Risk

None — read-only client layer.

---

### Task 5: Trading activity tabs (minimal panel)

| Attribute | Value |
|-----------|--------|
| Spec story | Story 5 (bottom panel empty states) |
| Depends on | Task 3, 4 |
| Estimated complexity | M |
| Parent story issue | #48 |

#### Objective

On the trading view, render tabbed **Open Orders**, **Order History**, **Trade History**, and **Holdings** so post-reset empty states are visible without navigating away.

#### Implementation notes

- `PortfolioActivityTabs` using shadcn `Tabs`.
- Holdings tab: reuse existing holdings table content (extract from `TradingPage` if needed).
- Other tabs: compact tables or list rows; `isPending` skeletons; empty copy (“No open orders”, “No orders yet”, “No trades yet”).
- No order placement/cancel UI in this task.
- Page composes wallet card + activity tabs grid per PRD direction (full book/chart still out of scope).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `web/src/features/trading/components/portfolio-activity-tabs.tsx` | Tabbed panel |
| MODIFY | `web/src/features/trading/pages/trading-page.tsx` | Compose panel |
| REUSE | `web/src/components/ui/tabs.tsx` | UI primitive |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | After reset, all four tabs show empty/zero within 2s | `web/` |

#### Acceptance criteria

- [x] Open orders count 0 after reset
- [x] Order history and trade history first page empty after reset
- [x] Holdings tab shows zero AAPL available
- [x] No stale pre-reset rows remain after refetch completes

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD SS8.1 | Bottom panel slice only |
| design-system | Tabs + table density |

#### Risk

Scope creep into full terminal layout — keep tabs read-only.

---

### Task 6: Polish — SignalR bridge, session cleanup, ADR-008, manual checklist

| Attribute | Value |
|-----------|--------|
| Spec story | Polish |
| Depends on | Task 2, 5 |
| Estimated complexity | S |
| Parent story issue | #48 |

#### Objective

Align realtime invalidation with panel keys, document cache strategy, extend logout cache purge, and complete manual verification for SignalR + multi-tab + wallet error path.

#### Implementation notes

- `interceptors.ts`: on `onBalanceUpdated` / `onOrderCancellationNotified`, also `invalidateQueries({ queryKey: ['trades'] })`.
- `clear-user-queries.ts`: `removeQueries` for `['orders']` and `['trades']` prefixes.
- ADR-008: post-reset client strategy — seed from 200 + invalidate/refetch; SignalR as secondary.
- Confirm backend tests still green: `ResetPortfolio_PublishesOrderCancellationNotifications` (no new server tests required).
- Run `yarn --cwd web lint && yarn --cwd web build`.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `web/src/lib/signalr/interceptors.ts` | Trades invalidation |
| MODIFY | `web/src/features/auth/clear-user-queries.ts` | Logout purge |
| MODIFY | `docs/memory/decisions.md` | ADR-008 |
| REUSE | `tests/Api.IntegrationTests/Portfolios/ResetPortfolioTests.cs` | SignalR publisher regression |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | REUSE existing reset realtime tests | `ResetPortfolioTests.cs` |
| Manual | Checklist below | `web/` |

#### Acceptance criteria

- [x] SignalR cancel/balance messages invalidate same keys as POST success
- [x] Logout clears orders/trades cache
- [x] ADR-008 recorded
- [ ] Manual checklist completed on Aspire (operator — plan §Manual UI checklist)

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| BR-11 | PortfolioResetEvent → notifications (server done) |
| Virtual cash Story 4 | Focus refetch compatible |
| ADR needed? | ADR-008 |

#### Risk

None — polish only.

---

## Reference files

| File | Why open it |
|------|-------------|
| `web/src/features/portfolio-reset/use-reset-portfolio.ts` | Story 5 TODO at invalidation site |
| `web/src/features/trading/hooks/use-wallet-query.ts` | Refetch policy template |
| `web/src/lib/signalr/interceptors.ts` | Hub → Query bridge |
| `src/Application/Portfolios/Commands/ResetPortfolioCommandHandler.cs` | Balance/cancel publishes |
| `tests/Api.IntegrationTests/Portfolios/ResetPortfolioTests.cs` | Realtime + reset regression |
| `docs/plans/20260525-240000-virtual-cash-story-4.md` | Refetch-on-focus precedent |
| `docs/plans/20260527-214600-portfolio-reset-story-3.md` | History cutoff + API reads |

## Implementation details (for /build)

### Post-reset client flow

1. `resetPortfolio()` returns `PortfolioResetResponse` with `wallet`, `resetAt`, `nextEligibleAt`.
2. `saveNextEligibleAt` (existing).
3. `seedWalletQueryData(queryClient, userId, mapResetWalletToWalletResponse(response.wallet, userId))`.
4. `await invalidatePortfolioPanels(queryClient, userId)` — use `void` + `queryClient.refetchQueries` if eager refresh needed for 2s AC; prefer `invalidateQueries` then rely on mounted observers to refetch.
5. Toast: “Portfolio reset. You're starting fresh with $100,000.”

### Query key contract (single source)

```ts
// web/src/lib/query-keys.ts — illustrative
export const queryKeys = {
  wallet: (userId: string) => ['wallet', userId] as const,
  portfolio: (userId: string) => ['portfolio', userId] as const,
  ordersOpen: (userId: string) => ['orders', 'open', userId] as const,
  ordersHistory: (userId: string) => ['orders', 'history', userId] as const,
  trades: (userId: string) => ['trades', userId] as const,
}
```

### SignalR (no server change)

| Hub method | Client interceptor | Invalidated keys |
|------------|-------------------|------------------|
| `BalanceUpdated` | `onBalanceUpdated` | `wallet`, `portfolio`, `trades` (add) |
| `OrderCancellationNotified` | `onOrderCancellationNotified` | `orders/open`, `orders/history`, `trades` (add) |

### Wallet 500 failure path

- Do not catch reset success and force display $100k without `response.wallet`.
- If `seedWalletQueryData` ran but subsequent `getWallet` refetch returns 500, `useWalletQuery` `isError` shows `WALLET_LOAD_ERROR_MESSAGE` — acceptable because user should retry; optional follow-up: keep seeded data until refetch succeeds (document in ADR-008).

### EC-10 (manual, when order form exists)

After reset refetch completes, place buy → available balance reflects $100k minus reservation.

## Verification matrix (plan-level)

| Spec AC | Verified by |
|---------|-------------|
| Cash card + chip $100k < 2s | Task 2 manual + seed from 200 |
| All panel queries invalidated | Task 1 devtools + Task 5 manual |
| SignalR user notifications | Task 6 manual + existing integration test |
| Wallet refetch 500 → error | Task 6 manual (simulate Api failure) |
| Multi-tab focus | Task 3 + Task 6 manual |
| EC-10 | Manual when order UI ships |
| No regression Stories 1–4 | Run `ResetPortfolioTests` subset |

## Manual UI checklist (Aspire)

1. Login as user with depleted cash, AAPL holdings, open orders, and history rows.
2. Reset portfolio → within 2s top-bar chip and virtual cash card show **$100,000.00** available.
3. Open Orders / Order History / Trade History tabs → empty; Holdings → 0 AAPL.
4. (Optional) Break `GET /api/wallet` temporarily → confirm error state, no JSX hardcode $100k.
5. Two browser tabs: reset in tab A → focus tab B → cash/holdings/orders match post-reset after refetch.
6. DevTools: confirm SignalR `BalanceUpdated` / `OrderCancellationNotified` while connected.

## Rollback / recovery

- **Code:** revert `feature/portfolio-reset-story-5` branch.
- **DB:** N/A — no migrations.
- **Redis:** N/A — client-only changes.

## Deferred work (Plan B)

- Full trading terminal layout (order book, chart, order form) and EC-10 automation.
- `setQueryData` for portfolio/orders from reset response (only wallet returned today).
- E2E Playwright suite for reset panel consistency.

## GitHub Links

| Local ref | Issue # | Type | Title | URL |
|-----------|---------|------|-------|-----|
| spec Story 5 | 48 | Story | US-04 / Story 5: Consistent data everywhere after reset | https://github.com/tranvuongduy2003/trading-simulator/issues/48 |
| epic | 43 | Epic | Spec: Portfolio reset (US-04) | https://github.com/tranvuongduy2003/trading-simulator/issues/43 |