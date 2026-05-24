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

## Template
- Date: YYYY-MM-DD
- Status: Proposed | Accepted | Superseded
- Context:
- Decision:
- Consequences:
- Supersedes:
