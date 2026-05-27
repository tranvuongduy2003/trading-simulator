# Changelog

All notable project and process changes are tracked here.

## 2026-05-28

- verify: account-management close plan Task 6 — final verification matrix green (Domain Users **22**, Api Users + ResetPortfolio **85**, `web api:verify`, `web build`), memory/changelog synchronized, and close plan promoted to `status: approved` (administrative close with manual operator sign-off still deferred)
- impl: account-management close plan Task 5 — mapped concurrent register unique-violation race to 422 (`USERNAME_TAKEN` / `EMAIL_TAKEN`) via UoW persistence exception mapping; tightened parallel register integration test (`RegisterUserTransientFailureTests`) to require 422 and preserve wallet-count invariant
- docs: account-management close plan Task 4 — aligned `.cursor/rules/frontend.mdc` wallet query example with ADR-008 and removed stale RegisterUserSession local-Postgres caveat from memory docs
- test: account-management close plan Task 3 — split `tests/Api.IntegrationTests/Portfolios/ResetPortfolioTests.cs` into focused suites + shared helpers; `FullyQualifiedName~ResetPortfolio` passed (**23**)
- docs: account-management epic marked closed in archive README; archived specs promoted `draft` → `approved` (4 specs) as part of close-plan Task 2
- verify: account-management close plan Task 2 (partial) — confirmed `main` already includes story merges (#39–#55); regression filters green (Domain Users 22, Api Users+ResetPortfolio 85); final close updates blocked pending operator sign-off
- docs: Account Management operator runbook (`docs/epics/account-management/OPERATOR-SIGNOFF.md`) — consolidated 95 manual UI steps + E2E smoke; epic close plan Task 1 complete on `feature/account-management-epic-close`
- ci: enable `Api.IntegrationTests` in GitHub Actions (`.github/workflows/ci.yml`) by removing the integration-test exclusion filter from `.NET` test step
- plan: Account Management epic close and hygiene (`docs/plans/20260528-194500-account-management-epic-close.md`) — operator sign-off, merge hygiene, test split, doc sync, register 422 race
- epic-archived: Account Management (`docs/epics/account-management/`; 4 specs + 18 plans merged, sources deleted)
- review: Account Management (`docs/reviews/20260528-180000-account-management.md`) — 🟡 close with follow-ups; Domain 22 + Api 85 tests green
- impl: portfolio reset story 5 **Tasks 1–6** (automation) — TanStack Query panel invalidation + wallet seed from reset 200, user-scoped portfolio/orders/trades hooks, `PortfolioActivityTabs`, SignalR `trades` invalidation, logout cache purge, ADR-008; `yarn lint`/`build` green; manual UI checklist pending; branch `feature/portfolio-reset-story-5` (closes #48)
- plan: portfolio reset story 5 — consistent data everywhere after reset (`docs/plans/20260528-003204-portfolio-reset-story-5.md`, GitHub #48)
- impl: portfolio reset story 4 **Tasks 1–5** (automation) — server cooldown via `portfolio_resets`, `GET /api/portfolio/reset/eligibility`, `RESET_COOLDOWN_ACTIVE` + `nextEligibleAt`, TanStack Query menu disable, cooldown dialog copy, **22** `ResetPortfolioTests` + `api:verify` + `yarn build`; manual UI checklist pending; branch `feature/portfolio-reset-story-4` (closes #47)

## 2026-05-27

- plan: portfolio reset story 4 - respect the 24-hour cooldown (`docs/plans/20260527-231500-portfolio-reset-story-4.md`, GitHub #47)
- impl: portfolio reset story 3 **Tasks 1–5** (automation) — added `/api/orders/open`, `/api/orders/history`, `/api/trades` with reset-cutoff filtering; reset transaction now cancels open orders + reservation-safe reset; realtime cancel/book/balance notifications on reset; deterministic Story 3 edge/regression integration suite (20 tests in reset/order/trade slices) green; ADR-007 recorded; manual Aspire checklist handed off to operator; branch `feature/portfolio-reset-story-3` (story #46)
- plan: portfolio reset story 3 - cancel open orders and clear activity history (`docs/plans/20260527-214600-portfolio-reset-story-3.md`, GitHub #46)
- plan: portfolio reset story 2 — restore starting cash and empty holdings (`docs/plans/20260527-210000-portfolio-reset-story-2.md`, GitHub #45)

## 2026-05-25

- impl: portfolio reset story 1 **Tasks 1–5** (automation) — `POST /api/portfolio/reset` stub (ADR-005), confirmation dialog, in-flight **409**, client cooldown + toast, **13** `GetMyWallet`/`ResetPortfolio` integration tests, `api:verify`, `yarn build`; `GET /api/wallet` `.RequireAuthorization()`; manual UI checklist pending; branch `feature/portfolio-reset-story-1` (closes #44)
- plan: portfolio reset story 1 — confirm before reset (`docs/plans/20260525-260000-portfolio-reset-story-1.md`, GitHub #44)
- spec: portfolio reset US-04 (`docs/specs/20260525-251500-portfolio-reset.md`)
- impl: virtual cash story 4 **Tasks 1–5** (automation) — login/refetch integration tests (`GetMyWalletTests` 10), `prefetchWalletQuery` + register cache seed, wallet `staleTime: 0` + `refetchOnWindowFocus`, refresh-path comment; **10** `GetMyWalletTests` + `yarn build`; manual UI checklist pending; PR `feature/virtual-cash-story-4` → `main` (closes #37)
- plan: virtual cash balance story 4 — trust after login and refresh (`docs/plans/20260525-240000-virtual-cash-story-4.md`, #37)
- impl: virtual cash story 3 **Tasks 1–5** (automation) — wallet privacy integration tests (8), `clearUserScopedQueries`, user-scoped `useWalletQuery`, `canDisplayWallet` UI guards, `WalletEndpoint` handler 401 `UNAUTHORIZED`; **59** Users Testcontainers + `yarn build`; manual UI checklist pending; PR `feature/virtual-cash-story-3` → `main` (closes #36)
- plan: virtual cash balance story 3 — session-private wallet (`docs/plans/20260525-230000-virtual-cash-story-3.md`, #36)
- impl: virtual cash story 2 **Tasks 1–4** (automation) — reserved wallet integration tests (`GetMyWalletTests` 5/5), `VirtualCashCard` breakdown + helper copy, `lint:wallet-integrity` guard, **56** Users Testcontainers tests + `yarn build`; manual UI checklist pending; PR `feature/virtual-cash-story-2` → `main` (closes #35)
- plan: virtual cash balance story 2 — total vs reserved breakdown (`docs/plans/20260525-220000-virtual-cash-story-2.md`, #35)
- impl: virtual cash story 1 **Tasks 1–6** (automation) — `GetMyWalletTests` (3), decoupled wallet/portfolio UI, `useWalletQuery`, `VirtualCashCard`, `WalletTopBarChip`, OpenAPI wallet **500**, **54** Users Testcontainers tests + `api:verify` + `yarn build`; manual UI checklist pending; PR `feature/virtual-cash-story-1` → `main` (closes #34)
- plan: virtual cash story 1 updated — spec Q1 Yes, top-bar cash chip + Task 4 (`docs/plans/20260525-203000-virtual-cash-story-1.md`, ADR-004)
- plan: virtual cash balance story 1 — see available cash (`docs/plans/20260525-203000-virtual-cash-story-1.md`)
- spec: virtual cash balance US-03 (`docs/specs/20260525-201500-virtual-cash-balance.md`)
- impl: user login story 5 — `LoginUserCommandValidator`, validation/transient integration tests (8), login form 422/500 UX, OpenAPI login **422**, `api:verify`, **51** Users Testcontainers tests; manual checklist pending
- plan: user login story 5 — validate login input and transient failures (`docs/plans/20260525-190000-user-login-story-5.md`)
- impl: user login story 4 — `POST /api/auth/logout`, session revoke (PG + Redis), user menu + `useLogout`, EC-07 guards, **5** `LogoutUserTests`, OpenAPI `LogoutUser`, `api:verify`; **43** Users Testcontainers tests; manual checklist pending
- plan: user login story 4 — log out when done (`docs/plans/20260525-180000-user-login-story-4.md`)
- impl: user login story 3 — `SessionPersistenceTests` (5), `SessionStore` PG validation on resolve, session-expired UX, cookies-disabled guard; **38** Users Testcontainers tests + `api:verify`; manual UI checklist pending
- plan: user login story 3 — session persists until logout or expiry (`docs/plans/20260525-170000-user-login-story-3.md`)
- impl: user login story 2 — `LoginUserTestHelpers`, five HTTP enumeration/session tests (15 `LoginUser*`); UI error mapping verified; `api:verify`; **33** Users Testcontainers tests green (excl. `RegisterUserSessionTests`)
- plan: user login story 2 — reject invalid credentials safely (`docs/plans/20260525-160000-user-login-story-2.md`)
- impl: user login story 1 Task 6 — OpenAPI `POST /api/auth/login`, `api:verify`, Users regression (28 Testcontainers tests); manual UI checklist pending
- impl: user login story 1 Task 5 — login form, `map-login-error`, session prefetch, `from` redirect
- impl: user login story 1 Task 4 — `AuthEndpoint` session cookie + HTTP read-your-writes tests (wallet/portfolio)
- impl: user login story 1 Task 3 — `LoginUserCommandHandler` (credentials, session + Redis enqueue), MediatR integration tests
- impl: user login story 1 Task 2 — `GetByEmailAsync`, `IPasswordHasher.Verify`, `User`/`Wallet` `FromPersistence`, integration tests for lookup + verify
- impl: user login story 1 Task 1 — `POST /api/auth/login` skeleton (contracts, stub **401** `INVALID_CREDENTIALS`, `AuthEndpoint`, `LoginUserTests` smoke)
- plan: user login story 1 — log in and access portfolio (`docs/plans/20260525-150000-user-login-story-1.md`)
- spec: user login US-02 (`docs/specs/20260525-103709-user-login.md`)
- impl: user registration story 4 Task 4 — regression (18 Testcontainers `Users` tests), plan complete; manual UI checklist pending operator sign-off
- impl: user registration story 4 Task 3 — `POST /api/users` documents **500** in OpenAPI (`ProducesProblem` + `api:export`)
- impl: user registration story 4 Task 2 — `RegisterUserTransientFailureTests` (EC-04 retry, EC-09 500, EC-10 Redis, parallel wallet invariant); test factory `CreateFactory` hook
- impl: user registration story 4 Task 1 — register form retry alert on 500/network, submit guard, `suppressErrorToast` on `POST /api/users`
- plan: user registration story 4 — recover from transient failures (`docs/plans/20260525-095103-user-registration-story-4.md`)
- impl: user registration story 3 — granular FluentValidation (BR-03–BR-05), `INVALID_REQUEST` binding, `RegisterUserValidationTests`, client onBlur + aligned zod copy, OpenAPI `POST /api/users` 400/422 problem responses, `ResultFactory` fix
- plan: user registration story 3 — validate registration input (`docs/plans/20260525-120000-user-registration-story-3.md`)

## 2026-05-24

- impl: user registration story 2 — `USERNAME_TAKEN` / `EMAIL_TAKEN` on exists-check, `RegisterUserDuplicateTests`, OpenAPI `POST /api/users` documents 422 only
- plan: user registration story 2 — reject duplicate identity (`docs/plans/20260524-120000-user-registration-story-2.md`)

## 2026-05-23

- impl: Task 7 user registration story 1 — Testcontainers fixture, `RegisterUser_Returns201_AndWalletShowsInitialCash`, ADR session/password, `UserRegistered` log
- impl: Task 6 user registration story 1 — register form, wallet session probe, trading cash/holdings summary, EC-05 redirect
- impl: Task 5 user registration story 1 — Contracts DTOs, wallet/portfolio read queries, OpenAPI `api.v1.yaml` updated
- impl: Task 4 user registration story 1 — session store (PG + Redis), cookie auth middleware, `RegisterUser` sets real session, `GET /api/wallet` requires auth
- impl: Task 3 user registration story 1 — `RegisterUserCommand`, `UserRepository`, `IdentityPasswordHasher`, `TradingOptions`, post-commit domain events
- impl: Task 2 user registration story 1 — `User.Register`, domain value objects, `UserRegisteredEvent`, domain unit tests
- impl: Task 1 user registration story 1 — `InitialTradingSchema` EF migration, stub `POST /api/users` / `GET /api/wallet`, dev auto-migrate on Api startup
- plan: user registration story 1 (`docs/plans/20260523-201500-user-registration-story-1.md`)
- spec: user registration (`docs/specs/20260523-175509-user-registration.md`)

