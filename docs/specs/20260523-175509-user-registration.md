---
artifact_type: spec
artifact_version: 1
id: spec-20260523-175509-user-registration
title: User Registration
slug: user-registration
filename_template: 20260523-175509-user-registration.md
created_at: 2026-05-23T17:55:09+07:00
updated_at: 2026-05-23T18:10:00+07:00
status: draft
owner: product
tags: [spec, feature, trading-simulator, auth, registration, account]
related_plan: null
related_specs: []
github_epic_issue: 4
github_story_issues: [5, 6, 7, 8]
prd_refs: [PRD §4, PRD §5.1 US-01, PRD §6.1 FR-1.1, PRD §6.1 FR-1.3, PRD §7.4]
tech_refs: [Tech §5.2.1, Tech §6.2, Tech §8.1, Tech §15.1, Tech §15.2, Tech §15.3, Tech §16.2]
db_refs: [DB §4.1 users, DB §4.2 wallets, DB §4.3 portfolios, DB §4.9 user_sessions, DB §12.1 session cache]
search_index:
  keywords: [registration, signup, account, username, email, password, session, wallet, initial capital, virtual cash, cookie, auth, aspiring trader]
  bounded_contexts: [Trading]
  user_personas: [Aspiring Trader, Curious Learner]
---

> GitHub: [#4 Spec: User registration (US-01)](https://github.com/tranvuongduy2003/trading-simulator/issues/4)

# Feature: User Registration
> Status: DRAFT  |  Date: 2026-05-23
> PRD: PRD §4, §5.1 US-01, §6.1 FR-1.1, FR-1.3, §7.4
> Tech: Tech §5.2.1, §6.2, §8.1, §15
> DB: DB §4.1–4.3, §4.9, §12.1
> Owner: Product

## 1. Problem & Solution

**Problem:** A visitor cannot trade, view a personal wallet, or place orders without an account. Without registration, the simulator has no identity boundary and no per-user virtual capital.

**Solution:** Allow a new visitor to create an account with a unique username, email, and password. On success, the system provisions a user record, a wallet funded with the MVP starting balance (USD 100,000 virtual cash), an empty portfolio, and an authenticated session so the user can reach the trading experience immediately.

**Persona:** Aspiring Trader or Curious Learner visiting the local web app for the first time (PRD §4).

**Smallest valuable version:** Registration form + successful account creation + starting wallet + authenticated session + redirect to the main trading view. Login-only flows (US-02), password reset, and email verification are out of this phase.

## 2. User Stories & Acceptance Criteria

### Story 1: Register and enter the simulator
> As a **new user**, I want to **register an account**, so that **I can start trading**.

**Happy path:**
- GIVEN I am logged out on the registration screen → WHEN I submit a valid username, email, and password → THEN the system creates my account, funds my wallet with **USD 100,000.0000** virtual cash (available = total, reserved = 0), creates an empty portfolio (no AAPL holdings), establishes an authenticated session, and navigates me to the main trading view within **2 s** (local MVP).
- GIVEN registration succeeded → WHEN I view account or wallet summary on the trading screen → THEN I see **USD 100,000.00** (or equivalent formatted display) as my starting cash and **0** shares of **AAPL**.

**Failure / edge path:**
- GIVEN I am already authenticated → WHEN I open the registration screen → THEN I am redirected to the trading view (or shown a message that I am already signed in) without creating a second account.

---

### Story 2: Reject duplicate identity
> As a **new user**, I want **clear feedback when my username or email is already taken**, so that **I can choose different credentials**.

**Happy path:**
- GIVEN username `trader_jane` does not exist → WHEN I register with username `trader_jane` and a unique email → THEN registration succeeds (Story 1).

**Failure / edge path:**
- GIVEN username `trader_jane` already exists → WHEN I submit registration with username `trader_jane` (any email) → THEN registration fails with HTTP **422**, a stable error `code` (e.g. `USERNAME_TAKEN`), and a human-readable message; no user, wallet, or session is created.
- GIVEN email `jane@example.com` already exists → WHEN I submit registration with that email (any unused username) → THEN registration fails with `EMAIL_TAKEN` (or equivalent) and the same non-creation guarantee.
- GIVEN I fix only the conflicting field → WHEN I resubmit with unique username and email → THEN registration succeeds.

---

### Story 3: Validate registration input before persistence
> As a **new user**, I want **immediate validation on my username, email, and password**, so that **I can correct mistakes before submitting**.

**Happy path:**
- GIVEN I am on the registration form → WHEN I blur or submit fields → THEN inline validation reflects the rules in §3 (BR-03–BR-05) without a round trip where client-side rules already apply.

**Failure / edge path:**
- GIVEN username `ab` (too short) → WHEN I submit → THEN the request is rejected with HTTP **422**, field-level errors for `username`, and no account is created.
- GIVEN email `not-an-email` → WHEN I submit → THEN **422** with `email` validation errors.
- GIVEN password `short1` (fails complexity — too short or missing special character) → WHEN I submit → THEN **422** with `password` validation errors listing all rules (length, letter, digit, special character).
- GIVEN malformed JSON or missing required fields → WHEN I submit → THEN **400** with a problem-details body; no partial user row is persisted.

---

### Story 4: Recover from transient failures
> As a **new user**, I want **reliable feedback if registration fails due to a server or network error**, so that **I know whether to retry**.

**Happy path:**
- GIVEN the API is healthy → WHEN I submit valid data once → THEN exactly one account is created (no duplicate users on a single intentional submit).

**Failure / edge path:**
- GIVEN the API returns **500** or the network times out → WHEN I submit → THEN I see a generic retry message, the submit control re-enables, and I can try again; if the first attempt actually succeeded, a retry with the same username/email surfaces **USERNAME_TAKEN** / **EMAIL_TAKEN** instead of a second funded account.
- GIVEN I double-click Submit rapidly → WHEN both requests reach the server → THEN at most one account is created; the other receives a duplicate-identity or idempotent-safe error, never a second wallet for the same person.

## 3. Domain & Business Rules

```
BR-01: Registration is atomic. A successful registration always creates together: user identity, wallet with initial virtual cash, and portfolio shell. Partial creation (user without wallet) must not occur.

BR-02: Initial virtual cash is USD 100,000.0000 (NUMERIC(18,4)), sourced from product configuration (PRD FR-1.3, Tech configuration). Currency is USD only for MVP. Wallet starts with reserved_balance = 0; available cash equals total_balance.

BR-03: Username is required, unique system-wide, 3–32 characters, allowed characters: letters (A–Z, a–z), digits (0–9), and underscore (_). No spaces. Case-sensitive uniqueness (e.g. "Trader" and "trader" are distinct usernames).

BR-04: Email is required, unique system-wide, valid email format (RFC 5322–practical subset), max 254 characters. Stored normalized to lowercase for uniqueness checks (display may preserve submitted casing if desired).

BR-05: Password is required, minimum 8 characters, and must include at least one letter (A–Z or a–z), one digit (0–9), and one special character from `! @ # $ % ^ & * ( ) _ + - = [ ] { } | ; : ' " , . < > ? / \` ~`. Password is never stored or logged in plaintext; only a salted password hash is persisted (PRD FR-1.1, Tech §15.2). Example valid password: `SecurePass1!`.

BR-06: On successful registration, the user receives an authenticated session (cookie-based) so they can call protected APIs immediately without a separate login step (supports PRD §7.4: register and place first order within 3 minutes). Full login/logout UX is covered by US-02; registration must not leave the user unauthenticated if the goal is to start trading.

BR-07: Registration does not create orders, trades, or holdings. AAPL holdings quantity remains 0 until the user trades.

BR-08: Virtual cash is non-redeemable, non-transferable, and for simulation only (PRD §3.2, FR-1.3).

BR-09: Duplicate username or duplicate email is a registration rejection, not an update of an existing account.
```

## 4. UI Behavior **or** API Contract

### 4a. UI Behavior

```
Screen: Registration (unauthenticated)
- Arrival: Guest sees a focused form with fields: Username, Email, Password, Confirm password (optional but recommended for UX), primary action "Create account", and link "Already have an account? Log in" (login behavior is US-02; link may route to login page stub).
- Action: User completes fields and submits → inline/field errors for validation failures; on success, navigate to main trading view (PRD §8.1 layout).
- Loading: Disable submit and show inline spinner or button loading state within 100 ms of click; preserve entered username/email on error (clear password fields on failure).
- Empty: N/A (form starts empty). Optional short value prop: "Start with $100,000 virtual cash — no real money." Password field shows helper text for BR-05 (8+ chars, letter, digit, special character).
- Error: Map API RFC 7807 `code` to human copy — e.g. USERNAME_TAKEN → "That username is already in use."; EMAIL_TAKEN → "An account with this email already exists."; validation → field messages under inputs.
- Real-time: None required for registration itself. After redirect, trading view may subscribe to market/user channels per separate features (not part of this spec).

Screen: Main trading view (post-registration)
- Arrival: Authenticated user sees AAPL trading layout; wallet/cash area shows USD 100,000 available (US-03 may deepen display; minimum: user can see they have full starting balance).
- Error: If session is missing after redirect, show session-expired pattern and link back to login/register (defensive; should not occur on happy path).
```

### 4b. API Contract

- **Endpoint(s):** `POST /users` (public; align with api-guidelines auth surface). Prefix `/api` if the codebase standardizes on it — registration and login remain public.

- **Request (JSON):**
```json
{
  "username": "trader_jane",
  "email": "jane@example.com",
  "password": "SecurePass1!"
}
```

- **Success response:** HTTP **201 Created**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "username": "trader_jane",
  "email": "jane@example.com",
  "createdAt": "2026-05-23T10:55:09Z",
  "wallet": {
    "currency": "USD",
    "totalBalance": 100000.0000,
    "reservedBalance": 0.0000,
    "availableBalance": 100000.0000
  }
}
```
  Response sets an **HttpOnly, Secure (in HTTPS), SameSite** session cookie. No password or hash in the body.

- **Errors (RFC 7807):**

| HTTP | `code` (example) | When |
|------|------------------|------|
| 400 | `INVALID_REQUEST` | Malformed body |
| 422 | `VALIDATION_FAILED` | Field rules (BR-03–BR-05); `errors` map per field |
| 422 | `USERNAME_TAKEN` | Duplicate username |
| 422 | `EMAIL_TAKEN` | Duplicate email |
| 500 | `INTERNAL_ERROR` | Unexpected failure; generic detail to client |

- **Auth:** No session required to call `POST /users`. Successful call creates session. All other trading endpoints remain protected (Tech §15.3).

- **Idempotency:** Not required. Retries are safe only because unique constraints prevent duplicate accounts; clients must handle `USERNAME_TAKEN` / `EMAIL_TAKEN` on retry after an ambiguous timeout.

- **Pagination / filtering:** N/A.

## 5. Data & Storage Impact

| Concern | Answer |
|---|---|
| PostgreSQL tables / columns | **Insert** `users` (id, username, email, password_hash, created_at, updated_at, row_version). **Insert** `wallets` (user_id PK/FK, total_balance = 100000.0000, reserved_balance = 0, currency = USD). **Insert** `portfolios` (id, user_id unique). **Insert** `user_sessions` row when session is issued. No `holdings` row until first trade. |
| Redis keys / projections | **Set** `session:{session_id}` cache entry with TTL aligned to session expiration (DB §12.1). No order-book or market projection changes. |
| Matching / channel behavior | **None** — registration does not enqueue orders. |
| Migration needed | **No** if MVP schema already includes users, wallets, portfolios, user_sessions. **Yes** only if those tables/constraints are not yet deployed. |
| Rebuild strategy if Redis cleared | Session cache misses fall back to PostgreSQL `user_sessions`; user re-authenticates if session row expired. Wallet/portfolio authoritative in PostgreSQL. |

Cross-check: unique indexes `ux_users_username`, `ux_users_email` (DB §6.1); one wallet and one portfolio per user (DB §5 invariants).

## 6. Real-Time & Consistency

- **SignalR events:** None emitted specifically for registration in MVP. Post-redirect, the client may connect to the simulation hub as an authenticated user (user group subscription uses `userId` from session).

- **Read-your-writes:** Immediately after **201**, the same session cookie must authorize `GET /wallet` (or equivalent) returning **100000.0000** available balance without delay or stale zero balance.

- **Stale UI handling:** If registration succeeds but navigation fails, refreshing the trading page should still show the authenticated session and starting balance. If cookie is blocked, user sees a clear "enable cookies" / session error — no silent anonymous trading.

## 7. Security & Privacy (MVP)

- **Authn / Authz:** Registration is public. Response must not leak whether a password was wrong for an existing email (N/A for register). Duplicate username/email messages are acceptable for register (user is creating, not attacking). No cross-user data in response.

- **Sensitive fields:** Password only in transit (HTTPS); hash at rest. Never log password, session token, or password hash.

- **Threat surface:** Mass account creation (local MVP: minimal rate limit optional); credential stuffing on register (same as login — out of scope for advanced throttling); session fixation — issue new session id on register; CSRF — anti-forgery on cookie-based state-changing browser requests (Tech §15.5).

## 8. Observability (local MVP)

| Signal | What to emit |
|---|---|
| Structured logs | `UserRegistered` with `userId`, `username` (no email in info logs if policy prefers privacy), duration; validation failures at warning without password; duplicate username/email at info |
| Traces | Span around register command (ServiceDefaults) |
| Metrics | `minimal for MVP` — optional counter `registrations_total` |
| Audit | N/A |

## 9. Edge Cases

```
EC-01: Duplicate username → 422 USERNAME_TAKEN; no row inserted.
EC-02: Duplicate email (different username) → 422 EMAIL_TAKEN.
EC-03: Simultaneous duplicate submissions → database unique constraint; one succeeds, one fails; never two wallets for one identity.
EC-04: Request timeout after server committed → client retry shows EMAIL_TAKEN or USERNAME_TAKEN; user directed to log in (US-02).
EC-05: Already authenticated visitor opens /register → redirect to trading; no second account.
EC-06: Password confirmation mismatch (UI-only field) → block submit client-side; server does not receive request.
EC-07: Email with leading/trailing spaces → trim before validation and persistence.
EC-08: Username with invalid characters (e.g. space, @) → 422 validation before DB.
EC-09: PostgreSQL unavailable → 500; no orphaned user without wallet if transaction boundaries enforced.
EC-10: Redis session cache write fails after PG commit → user still has valid PG session; cache repopulates on next read (DB §12.2).
EC-11: Non-AAPL symbol → N/A for registration.
EC-12: Session expired → N/A on register path; handled at login/protected routes.
```

## 10. Dependencies, Risks, Decision Triggers

- **Depends on:** Base API host, PostgreSQL, Redis, Aspire local stack; empty or seed `symbols` table (AAPL) for later trading but not for register itself.

- **Impacts:** US-02 Login (session model must match); US-03 Cash balance display; all authenticated trading stories.

- **External services:** PostgreSQL, Redis (session cache), Aspire — no third-party identity provider.

- **Key risk:** Split transaction (user created without wallet) breaks money invariants — mitigated by atomic unit-of-work (BR-01).

- **Decision triggers:** If product requires email verification before trading → new spec + defer auto-trading until verified.

## 11. Assumptions

- **Confirmed (product):** Registration **auto sign-in** — session established on success (BR-06); no redirect to login-only screen.
- **Confirmed (product):** Password requires **≥ 8 characters**, at least **one letter**, **one digit**, and **one special character** (BR-05).
- **Confirmed (product):** `201` response **includes `email`** alongside `userId`, `username`, and `wallet` (§4b).
- Username rules in BR-03 align with `VARCHAR(32)` (DB §4.1).
- Email normalization to lowercase for uniqueness; user-visible email may match stored normalized form.
- `POST /users` response includes wallet summary so the client can show starting balance without an extra round trip (optional second call still valid).
- Login page link is present but full login flow is **US-02**, not this spec.
- No email verification, CAPTCHA, or terms-of-service gate for local MVP.
- Confirm-password is a **UI-only** check unless product later requires server-side `confirmPassword` field.

## 12. Out of Scope

- Login, logout, session refresh, and "remember me" (US-02 and related).
- Email verification, password reset, change password, MFA.
- OAuth / social sign-up.
- Admin-provisioned accounts or invite links.
- Configurable starting capital (PRD v1.1 candidate).
- Multi-symbol, fractional shares, real money, production CDN, horizontal scaling.
- CAPTCHA / bot prevention beyond basic validation.
- Portfolio reset (US-04).
- Placing orders or matching engine behavior.

### Future scope (Phase 2+)

- Email verification before first trade.
- Password strength meter and breach list check.
- Rate limiting and abuse monitoring.
- Welcome onboarding / first-trade walkthrough (PRD risk mitigation).

## 13. Open Questions

| # | Question | Source | Answer | Status |
|---|----------|--------|--------|--------|
| 1 | Should registration auto-sign-in, or redirect to a login screen? | BR-06, PRD §7.4 | **Yes — auto sign-in** (session on success) | ✅ |
| 2 | Exact password rules beyond "length + variety" — special characters required? | PRD FR-1.1 | **Yes —** min 8, letter, digit, and special character (BR-05) | ✅ |
| 3 | Should `201` response include email in body, or only username + userId? | API sketch | **Yes — include `email`** in response body | ✅ |
| 4 | Return **409** vs **422** for duplicate username/email? | API guidelines | Default **422** until decided | ❓ |
