---
artifact_type: plan
artifact_version: 1
id: plan-20260523-201500-user-registration-story-1
title: User Registration — Story 1 (Register and enter simulator)
slug: user-registration-story-1
filename_template: 20260523-201500-user-registration-story-1.md
created_at: 2026-05-23T20:15:00+07:00
updated_at: 2026-05-23T21:00:00+07:00
status: in_progress
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
| Status | IN PROGRESS (Task 1 done) |
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

- [ ] Domain project has zero outward references.
- [ ] All domain tests pass.

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

- [ ] Handler compiles; unit of work rolls back if wallet insert fails (manual or integration in Task 7).
- [ ] No MediatR handler references `DbContext` directly.

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
- Cookie authentication scheme reads **`TradingSimulator.Session`** (from `IOptions<SessionOptions>.CookieName`), validates session id against Redis then PG fallback.
- `UseAuthentication` / `UseAuthorization` in `UseApiPipeline` before endpoints.
- Remove stub cookie logic from Task 1.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `src/Application/Abstractions/Auth/ISessionStore.cs` | Port |
| CREATE | `src/Application/Abstractions/Auth/ICurrentUserAccessor.cs` | Port |
| CREATE | `src/Infrastructure/Auth/SessionStore.cs` | PG + Redis |
| CREATE | `src/Infrastructure/Auth/CurrentUserAccessor.cs` | Scoped |
| CREATE | `src/Api/Auth/SessionAuthenticationHandler.cs` | Cookie validation |
| MODIFY | `src/Api/DependencyInjection.cs` | Auth registration |
| MODIFY | `src/Api/Program.cs` | `UseAuthentication` order |
| MODIFY | `src/Application/Users/RegisterUserCommandHandler.cs` | Create session after user persist |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | (deferred to Task 7 with Testcontainers) | — |

#### Acceptance criteria

- [ ] Cookie set on successful register (browser or integration `Handler` cookie container).
- [ ] `GET /api/wallet` without cookie → 401.

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
- Wire `RegisterUserCommand` via MediatR; map `Result` → `ToCreatedHttpResult`.
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

- [ ] 201 response matches spec field names (camelCase JSON).
- [ ] `yarn --cwd web api:verify` passes.

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

- [ ] Story 1 happy path works in browser via Aspire stack.
- [ ] EC-05: logged-in user cannot open register.

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
| CREATE | `tests/Api.IntegrationTests/Users/RegisterUserTests.cs` | Story 1 proof |
| MODIFY | `docs/memory/decisions.md` | Session + password ADR |
| MODIFY | `src/Application/Users/RegisterUserCommandHandler.cs` | Structured log |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `RegisterUser_Returns201_AndWalletShowsInitialCash` | `RegisterUserTests.cs` |
| Domain | (existing Task 2) | — |

#### Acceptance criteria

- [ ] Integration test passes in CI with Docker.
- [ ] Definition of done on [#5](https://github.com/tranvuongduy2003/trading-simulator/issues/5) satisfied.

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
