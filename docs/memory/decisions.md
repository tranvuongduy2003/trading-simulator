# Architecture Decisions

This file records key decisions that affect implementation and planning.
If a decision changes, add a new entry and mark the old one as superseded.

---

---

## ADR-001: Cookie-based session authentication (Story 1)

- Date: 2026-05-23
- Status: Accepted
- Context: US-01 requires registration to establish an authenticated session without JWT complexity for the local MVP.
- Decision: Issue an HTTP-only session cookie after successful registration. Persist sessions in PostgreSQL (`user_sessions`) and cache active session metadata in Redis (`session:{id}`) for fast authentication on subsequent requests. Use a custom `SessionAuthenticationHandler` in the Api host; session creation and Redis cache flush occur in the register command pipeline after the unit-of-work commit.
- Consequences: Browser clients must send cookies (`credentials: 'include'`). Integration tests use `WebApplicationFactory` with `HandleCookies = true`. Logout and session rotation are deferred to US-02.

---

## ADR-002: ASP.NET Identity password hashing (Story 1)

- Date: 2026-05-23
- Status: Accepted
- Context: Passwords must never be stored in plaintext; Story 1 needs a proven hashing algorithm without building custom crypto.
- Decision: Hash passwords with `Microsoft.AspNetCore.Identity.PasswordHasher` via `IdentityPasswordHasher` in Infrastructure. Domain stores `PasswordHash` value object; raw passwords exist only in the register command boundary.
- Consequences: Hash format is tied to Identity's versioning. Future password policy changes remain in domain `Password` rules and FluentValidation.

---

## ADR-003: Two-layer request validation (binding vs business rules)

- Date: 2026-05-25
- Status: Accepted
- Context: Story 3 needs distinct **400** (`INVALID_REQUEST`) for malformed or incomplete JSON bodies and **422** (`VALIDATION_FAILED`) for BR-03–BR-05. A user-specific `RegisterUserBindingEndpointFilter` in `Endpoints/` would not scale as more commands are added.
- Decision: Treat validation as two layers in the Api host:
  1. **Transport binding** — `InvalidRequestMiddleware` (JSON parse / bad HTTP) and generic `CompleteJsonBodyEndpointFilter<TBody>` via `RequireCompleteJsonBody<T>()` when any non-optional contract property is null after deserialization. Shared factory: `InvalidRequestProblems`.
  2. **Business rules** — FluentValidation in Application (`ValidationBehavior`) before handlers; unchanged.
  Optional body properties use nullable reference types (`string?`) or `[BindOptional]` on the contract. Endpoints only call `.RequireCompleteJsonBody<TContract>()`; no per-entity filter classes.
- Consequences: Contracts stay dumb DTOs; Api owns HTTP problem shape for binding failures. New POST/PUT bodies reuse the same extension. Domain/Application remain the authority for semantic validation.

---

## ADR-004: Top-bar available cash chip (US-03 Story 1)

- Date: 2026-05-25
- Status: Accepted
- Context: PRD §8.1 describes a top bar on the trading terminal. Spec §13 Q1 asked whether available virtual cash should appear there before the full layout ships. Product answered **Yes**.
- Decision: Add a compact **available** cash readout to `AppLayout` header on every authenticated route (Trading, Portfolio, Orders). Reuse the same TanStack Query key `['wallet']` and `GET /api/wallet` as the trading dashboard card via a shared `useWalletQuery` hook. Top bar shows **available only** (not total/reserved breakdown); dashboard card remains the primary detailed surface.
- Consequences: Wallet fetch may run from layout + page simultaneously; Query dedupes. Chip must not show fabricated zeros on load or API failure. Symbol, last price, and daily change in the top bar remain future work.

---

## ADR-005: Portfolio reset Story 1 contract stub (US-04)

- Date: 2026-05-25
- Status: Accepted
- Context: Story 1 requires confirmation UI and `POST /api/portfolio/reset` wiring before the atomic DB §10.4 reset transaction (Stories 2–3). Spec defers `portfolio_resets` insert until Story 2+.
- Decision: Story 1 ships a **stub** `ResetPortfolioCommandHandler` that authenticates the user, reads the current wallet, returns **200** with `resetAt`, `nextEligibleAt` (from `Trading:PortfolioResetCooldownMinutes`), and `wallet` — **no** PostgreSQL writes, order cancels, or matching enqueue. Story 2 replaces the handler body with the real reset transaction; Story 4 adds server cooldown enforcement.
- Consequences: Manual QA must note balances unchanged until Story 2. Client disable-after-success uses `nextEligibleAt` from the response (client cache). OpenAPI and integration tests validate the contract early.

---

## Template
- Date: YYYY-MM-DD
- Status: Proposed | Accepted | Superseded
- Context:
- Decision:
- Consequences:
- Supersedes:
