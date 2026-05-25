# Changelog

All notable project and process changes are tracked here.

## 2026-05-25

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

