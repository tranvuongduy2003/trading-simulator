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

## ADR-006: Portfolio reset Story 2 persists wallet and holdings slice

- Date: 2026-05-27
- Status: Accepted
- Context: Story 1 intentionally shipped a reset contract stub to unlock UI flow and API shape early. Story 2 requires real PostgreSQL persistence for the wallet and holdings portion of FR-1.4 while Story 3+ still owns open-order cancellation, matching-engine synchronization, and history cutover.
- Decision: `ResetPortfolioCommandHandler` now executes a real write path through `IPortfolioResetWriteRepository` inside the existing command unit-of-work transaction. The repository resets wallet balances to `Trading:InitialVirtualCash`, clears holdings for the user portfolio, and appends a `portfolio_resets` audit row with `reason = user_initiated`. Task-level integration tests assert read-your-writes and rollback guarantees for failure paths.
- Consequences: Story 1 stub behavior is superseded for wallet/holdings/audit writes. Users can see immediate wallet and holdings reset in API reads, while known inconsistency with open orders remains intentionally deferred to Story 3. This keeps scope aligned to planned incremental delivery without introducing partial transaction behavior for completed Story 2 slice.
- Supersedes: ADR-005 (partially; only the "no PostgreSQL writes" portion for reset)

---

## ADR-007: Portfolio reset history clearing uses read cutoff

- Date: 2026-05-27
- Status: Accepted
- Context: Story 3 requires the user's order/trade history to appear cleared after reset while preserving immutable market records and foreign-key integrity in `trades`/`orders`.
- Decision: Keep `orders` and `trades` rows intact and apply a per-user read cutoff using the latest `portfolio_resets.reset_at` timestamp in `GetMyOrderHistory`, `GetMyTradeHistory`, and reset-sensitive open-order reads. Reset writes remain fully transactional (cancel opens, release reservations, reset wallet/holdings, insert reset audit row), and visibility semantics are enforced on read models.
- Consequences: No destructive data deletion is required for history clearing; market/audit integrity is preserved, and another user's history remains visible to them. Query handlers/repositories must consistently apply cutoff logic and keep pagination defaults aligned with PRD requirements.
- Supersedes: None

---

## ADR-008: Post-reset client cache strategy (US-04 Story 5)

- Date: 2026-05-28
- Status: Accepted
- Context: After `POST /api/portfolio/reset` succeeds, every portfolio-related surface (wallet chip, virtual cash card, holdings, open orders, order history, trade history) must show post-reset figures within ~2 s without logout. Server reset work and SignalR notifications already exist from Stories 2–3; the gap was TanStack Query cache coordination on the web client.
- Decision: On reset success, (1) persist cooldown from the 200 body, (2) **seed** `['wallet', userId]` from the authoritative `wallet` snapshot in the response via `seedWalletQueryData`, then (3) **invalidate** all panel query prefixes (`wallet`, `portfolio`, `orders/open`, `orders/history`, `trades`) through `invalidatePortfolioPanels`. Panel hooks use `staleTime: 0` and `refetchOnWindowFocus: true` (aligned with virtual cash Story 4) so multi-tab stale data recovers on focus. SignalR `BalanceUpdated` and `OrderCancellationNotified` invalidate the same prefixes as a secondary path; POST success always invalidates even if hub delivery fails. Never hardcode `$100,000` in JSX — display comes from API/cache; if `GET /api/wallet` refetch fails after seeding, show the existing wallet error state.
- Consequences: Read-your-writes for cash is immediate from the 200 body; other panels rely on refetch after invalidation. Only wallet is seeded today (reset response does not include portfolio/orders). Logout clears `orders` and `trades` cache prefixes alongside wallet/portfolio. Future: optional `setQueryData` for other panels if the reset response grows.
- Supersedes: None

---

## Template
- Date: YYYY-MM-DD
- Status: Proposed | Accepted | Superseded
- Context:
- Decision:
- Consequences:
- Supersedes:
