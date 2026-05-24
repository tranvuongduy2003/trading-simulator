# Changelog

All notable project and process changes are tracked here.

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

