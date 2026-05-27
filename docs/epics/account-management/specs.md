---
artifact_type: epic-archive-specs
artifact_version: 2
id: epic-account-management-specs
title: Account Management — archived product specs
epic: Account Management (PRD §5.1)
user_stories: [US-01, US-02, US-03, US-04]
archived_at: 2026-05-28T18:00:00Z
consolidation_mode: epic-record-plus-verbatim
source_count: 4
sources_deleted: true
source_files:
  - docs/specs/20260523-175509-user-registration.md
  - docs/specs/20260525-103709-user-login.md
  - docs/specs/20260525-201500-virtual-cash-balance.md
  - docs/specs/20260525-251500-portfolio-reset.md
related_review: docs/reviews/20260528-180000-account-management.md
related_plan: docs/plans/20260528-194500-account-management-epic-close.md
tags: [epic-archive, trading-simulator, account-management]
---

# Account Management — product specs (archive)

> **Authoritative epic product archive.** Individual files under `docs/specs/` were merged here on 2026-05-28 and **deleted**. Part 1 is the readable epic record; Part 2 is **full verbatim** spec text.

## Part 1 — Epic product record

### PRD §5.1 user stories

| US | Story | Priority | FR |
|----|-------|----------|-----|
| US-01 | Register an account to start trading | Must | FR-1.1 |
| US-02 | Log in securely to access portfolio | Must | FR-1.2 |
| US-03 | See virtual cash balance | Must | FR-1.3, FR-6.2 |
| US-04 | Reset portfolio after poor performance | Should (strict: Must) | FR-1.4 |

### What shipped (summary)

| US | Spec | Stories | API / UX highlights |
|----|------|---------|---------------------|
| US-01 | User Registration | 4 | `POST /api/users` — user + $100k wallet + portfolio + session; 422 duplicates/validation; transient UX |
| US-02 | User Login | 5 | `POST /api/auth/login`, `POST /api/auth/logout`; cookie session + Redis; 401 invalid creds; 422 validation |
| US-03 | Virtual Cash Balance | 4 | `GET /api/wallet` — total/reserved/available; dashboard card + top-bar chip (ADR-004); user-scoped query (ADR-008) |
| US-04 | Portfolio Reset | 5 | `POST /api/portfolio/reset`, `GET /api/portfolio/reset/eligibility`; 24h cooldown; cancel opens; history cutoff (ADR-007) |

### Cross-epic dependencies

- US-04 depends on US-01–03 (authenticated user, wallet, session).
- Order placement epics depend on wallet reserve semantics from US-03.

### Traceability

See [`docs/TRACEABILITY.md`](../../TRACEABILITY.md) rows US-01–US-04.

### Open questions status (epic)

| Spec | Notable resolutions |
|------|---------------------|
| Registration | Q4: duplicate → **422** (not 409) |
| Virtual cash | Q1: top-bar cash chip **Yes** (symbol/price deferred to Market Data) |
| Portfolio reset | Read cutoff via `portfolio_resets.reset_at` (ADR-007) |

### Operator / doc hygiene

All four specs are promoted to `status: approved` in Part 2 as part of epic-close administration. Epic review: [`docs/reviews/20260528-180000-account-management.md`](../../reviews/20260528-180000-account-management.md).

---

## Part 2 — Verbatim archived specs


## Source 1 of 4: `docs/specs/20260523-175509-user-registration.md`

---
artifact_type: spec
artifact_version: 1
id: spec-20260523-175509-user-registration
title: User Registration
slug: user-registration
filename_template: 20260523-175509-user-registration.md
created_at: 2026-05-23T17:55:09+07:00
updated_at: 2026-05-23T20:35:00+07:00
status: approved
owner: product
tags: [spec, feature, trading-simulator, auth, registration, account]
related_plan: docs/plans/20260525-095103-user-registration-story-4.md
related_plans: [docs/plans/20260523-201500-user-registration-story-1.md, docs/plans/20260524-120000-user-registration-story-2.md, docs/plans/20260525-120000-user-registration-story-3.md, docs/plans/20260525-095103-user-registration-story-4.md]
related_specs: [docs/specs/20260525-103709-user-login.md]
github_epic_issue: 4
github_story_issues: [5, 6, 7, 8]
prd_refs: [PRD Â§4, PRD Â§5.1 US-01, PRD Â§6.1 FR-1.1, PRD Â§6.1 FR-1.3, PRD Â§7.4]
tech_refs: [Tech Â§5.2.1, Tech Â§6.2, Tech Â§8.1, Tech Â§15.1, Tech Â§15.2, Tech Â§15.3, Tech Â§16.2]
db_refs: [DB Â§4.1 users, DB Â§4.2 wallets, DB Â§4.3 portfolios, DB Â§4.9 user_sessions, DB Â§12.1 session cache]
search_index:
  keywords: [registration, signup, account, username, email, password, session, wallet, initial capital, virtual cash, cookie, auth, aspiring trader]
  bounded_contexts: [Trading]
  user_personas: [Aspiring Trader, Curious Learner]
---

> GitHub: [#4 Spec: User registration (US-01)](https://github.com/tranvuongduy2003/trading-simulator/issues/4)

# Feature: User Registration
> Status: DRAFT  |  Date: 2026-05-23
> PRD: PRD Â§4, Â§5.1 US-01, Â§6.1 FR-1.1, FR-1.3, Â§7.4
> Tech: Tech Â§5.2.1, Â§6.2, Â§8.1, Â§15
> DB: DB Â§4.1â€“4.3, Â§4.9, Â§12.1
> Owner: Product

## 1. Problem & Solution

**Problem:** A visitor cannot trade, view a personal wallet, or place orders without an account. Without registration, the simulator has no identity boundary and no per-user virtual capital.

**Solution:** Allow a new visitor to create an account with a unique username, email, and password. On success, the system provisions a user record, a wallet funded with the MVP starting balance (USD 100,000 virtual cash), an empty portfolio, and an authenticated session so the user can reach the trading experience immediately.

**Persona:** Aspiring Trader or Curious Learner visiting the local web app for the first time (PRD Â§4).

**Smallest valuable version:** Registration form + successful account creation + starting wallet + authenticated session + redirect to the main trading view. Login-only flows (US-02), password reset, and email verification are out of this phase.

## 2. User Stories & Acceptance Criteria

### Story 1: Register and enter the simulator
> As a **new user**, I want to **register an account**, so that **I can start trading**.

**Happy path:**
- GIVEN I am logged out on the registration screen â†’ WHEN I submit a valid username, email, and password â†’ THEN the system creates my account, funds my wallet with **USD 100,000.0000** virtual cash (available = total, reserved = 0), creates an empty portfolio (no AAPL holdings), establishes an authenticated session, and navigates me to the main trading view within **2 s** (local MVP).
- GIVEN registration succeeded â†’ WHEN I view account or wallet summary on the trading screen â†’ THEN I see **USD 100,000.00** (or equivalent formatted display) as my starting cash and **0** shares of **AAPL**.

**Failure / edge path:**
- GIVEN I am already authenticated â†’ WHEN I open the registration screen â†’ THEN I am redirected to the trading view (or shown a message that I am already signed in) without creating a second account.

---

### Story 2: Reject duplicate identity
> As a **new user**, I want **clear feedback when my username or email is already taken**, so that **I can choose different credentials**.

**Happy path:**
- GIVEN username `trader_jane` does not exist â†’ WHEN I register with username `trader_jane` and a unique email â†’ THEN registration succeeds (Story 1).

**Failure / edge path:**
- GIVEN username `trader_jane` already exists â†’ WHEN I submit registration with username `trader_jane` (any email) â†’ THEN registration fails with HTTP **422**, a stable error `code` (e.g. `USERNAME_TAKEN`), and a human-readable message; no user, wallet, or session is created.
- GIVEN email `jane@example.com` already exists â†’ WHEN I submit registration with that email (any unused username) â†’ THEN registration fails with `EMAIL_TAKEN` (or equivalent) and the same non-creation guarantee.
- GIVEN I fix only the conflicting field â†’ WHEN I resubmit with unique username and email â†’ THEN registration succeeds.

---

### Story 3: Validate registration input before persistence
> As a **new user**, I want **immediate validation on my username, email, and password**, so that **I can correct mistakes before submitting**.

**Happy path:**
- GIVEN I am on the registration form â†’ WHEN I blur or submit fields â†’ THEN inline validation reflects the rules in Â§3 (BR-03â€“BR-05) without a round trip where client-side rules already apply.

**Failure / edge path:**
- GIVEN username `ab` (too short) â†’ WHEN I submit â†’ THEN the request is rejected with HTTP **422**, field-level errors for `username`, and no account is created.
- GIVEN email `not-an-email` â†’ WHEN I submit â†’ THEN **422** with `email` validation errors.
- GIVEN password `short1` (fails complexity â€” too short or missing special character) â†’ WHEN I submit â†’ THEN **422** with `password` validation errors listing all rules (length, letter, digit, special character).
- GIVEN malformed JSON or missing required fields â†’ WHEN I submit â†’ THEN **400** with a problem-details body; no partial user row is persisted.

---

### Story 4: Recover from transient failures
> As a **new user**, I want **reliable feedback if registration fails due to a server or network error**, so that **I know whether to retry**.

**Happy path:**
- GIVEN the API is healthy â†’ WHEN I submit valid data once â†’ THEN exactly one account is created (no duplicate users on a single intentional submit).

**Failure / edge path:**
- GIVEN the API returns **500** or the network times out â†’ WHEN I submit â†’ THEN I see a generic retry message, the submit control re-enables, and I can try again; if the first attempt actually succeeded, a retry with the same username/email surfaces **USERNAME_TAKEN** / **EMAIL_TAKEN** instead of a second funded account.
- GIVEN I double-click Submit rapidly â†’ WHEN both requests reach the server â†’ THEN at most one account is created; the other receives a duplicate-identity or idempotent-safe error, never a second wallet for the same person.

## 3. Domain & Business Rules

```
BR-01: Registration is atomic. A successful registration always creates together: user identity, wallet with initial virtual cash, and portfolio shell. Partial creation (user without wallet) must not occur.

BR-02: Initial virtual cash is USD 100,000.0000 (NUMERIC(18,4)), sourced from product configuration (PRD FR-1.3, Tech configuration). Currency is USD only for MVP. Wallet starts with reserved_balance = 0; available cash equals total_balance.

BR-03: Username is required, unique system-wide, 3â€“32 characters, allowed characters: letters (Aâ€“Z, aâ€“z), digits (0â€“9), and underscore (_). No spaces. Case-sensitive uniqueness (e.g. "Trader" and "trader" are distinct usernames).

BR-04: Email is required, unique system-wide, valid email format (RFC 5322â€“practical subset), max 254 characters. Stored normalized to lowercase for uniqueness checks (display may preserve submitted casing if desired).

BR-05: Password is required, minimum 8 characters, and must include at least one letter (Aâ€“Z or aâ€“z), one digit (0â€“9), and one special character from `! @ # $ % ^ & * ( ) _ + - = [ ] { } | ; : ' " , . < > ? / \` ~`. Password is never stored or logged in plaintext; only a salted password hash is persisted (PRD FR-1.1, Tech Â§15.2). Example valid password: `SecurePass1!`.

BR-06: On successful registration, the user receives an authenticated session (cookie-based) so they can call protected APIs immediately without a separate login step (supports PRD Â§7.4: register and place first order within 3 minutes). Full login/logout UX is covered by US-02; registration must not leave the user unauthenticated if the goal is to start trading.

BR-07: Registration does not create orders, trades, or holdings. AAPL holdings quantity remains 0 until the user trades.

BR-08: Virtual cash is non-redeemable, non-transferable, and for simulation only (PRD Â§3.2, FR-1.3).

BR-09: Duplicate username or duplicate email is a registration rejection, not an update of an existing account.
```

## 4. UI Behavior **or** API Contract

### 4a. UI Behavior

```
Screen: Registration (unauthenticated)
- Arrival: Guest sees a focused form with fields: Username, Email, Password, Confirm password (optional but recommended for UX), primary action "Create account", and link "Already have an account? Log in" (login behavior is US-02; link may route to login page stub).
- Action: User completes fields and submits â†’ inline/field errors for validation failures; on success, navigate to main trading view (PRD Â§8.1 layout).
- Loading: Disable submit and show inline spinner or button loading state within 100 ms of click; preserve entered username/email on error (clear password fields on failure).
- Empty: N/A (form starts empty). Optional short value prop: "Start with $100,000 virtual cash â€” no real money." Password field shows helper text for BR-05 (8+ chars, letter, digit, special character).
- Error: Map API RFC 7807 `code` to human copy â€” e.g. USERNAME_TAKEN â†’ "That username is already in use."; EMAIL_TAKEN â†’ "An account with this email already exists."; validation â†’ field messages under inputs.
- Real-time: None required for registration itself. After redirect, trading view may subscribe to market/user channels per separate features (not part of this spec).

Screen: Main trading view (post-registration)
- Arrival: Authenticated user sees AAPL trading layout; wallet/cash area shows USD 100,000 available (US-03 may deepen display; minimum: user can see they have full starting balance).
- Error: If session is missing after redirect, show session-expired pattern and link back to login/register (defensive; should not occur on happy path).
```

### 4b. API Contract

- **Endpoint(s):** `POST /users` (public; align with api-guidelines auth surface). Prefix `/api` if the codebase standardizes on it â€” registration and login remain public.

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
| 422 | `VALIDATION_FAILED` | Field rules (BR-03â€“BR-05); `errors` map per field |
| 422 | `USERNAME_TAKEN` | Duplicate username |
| 422 | `EMAIL_TAKEN` | Duplicate email |
| 500 | `INTERNAL_ERROR` | Unexpected failure; generic detail to client |

- **Auth:** No session required to call `POST /users`. Successful call creates session. All other trading endpoints remain protected (Tech Â§15.3).

- **Idempotency:** Not required. Retries are safe only because unique constraints prevent duplicate accounts; clients must handle `USERNAME_TAKEN` / `EMAIL_TAKEN` on retry after an ambiguous timeout.

- **Pagination / filtering:** N/A.

## 5. Data & Storage Impact

| Concern | Answer |
|---|---|
| PostgreSQL tables / columns | **Insert** `users` (id, username, email, password_hash, created_at, updated_at, row_version). **Insert** `wallets` (user_id PK/FK, total_balance = 100000.0000, reserved_balance = 0, currency = USD). **Insert** `portfolios` (id, user_id unique). **Insert** `user_sessions` row when session is issued. No `holdings` row until first trade. |
| Redis keys / projections | **Set** `session:{session_id}` cache entry with TTL aligned to session expiration (DB Â§12.1). No order-book or market projection changes. |
| Matching / channel behavior | **None** â€” registration does not enqueue orders. |
| Migration needed | **No** if MVP schema already includes users, wallets, portfolios, user_sessions. **Yes** only if those tables/constraints are not yet deployed. |
| Rebuild strategy if Redis cleared | Session cache misses fall back to PostgreSQL `user_sessions`; user re-authenticates if session row expired. Wallet/portfolio authoritative in PostgreSQL. |

Cross-check: unique indexes `ux_users_username`, `ux_users_email` (DB Â§6.1); one wallet and one portfolio per user (DB Â§5 invariants).

## 6. Real-Time & Consistency

- **SignalR events:** None emitted specifically for registration in MVP. Post-redirect, the client may connect to the simulation hub as an authenticated user (user group subscription uses `userId` from session).

- **Read-your-writes:** Immediately after **201**, the same session cookie must authorize `GET /wallet` (or equivalent) returning **100000.0000** available balance without delay or stale zero balance.

- **Stale UI handling:** If registration succeeds but navigation fails, refreshing the trading page should still show the authenticated session and starting balance. If cookie is blocked, user sees a clear "enable cookies" / session error â€” no silent anonymous trading.

## 7. Security & Privacy (MVP)

- **Authn / Authz:** Registration is public. Response must not leak whether a password was wrong for an existing email (N/A for register). Duplicate username/email messages are acceptable for register (user is creating, not attacking). No cross-user data in response.

- **Sensitive fields:** Password only in transit (HTTPS); hash at rest. Never log password, session token, or password hash.

- **Threat surface:** Mass account creation (local MVP: minimal rate limit optional); credential stuffing on register (same as login â€” out of scope for advanced throttling); session fixation â€” issue new session id on register; CSRF â€” anti-forgery on cookie-based state-changing browser requests (Tech Â§15.5).

## 8. Observability (local MVP)

| Signal | What to emit |
|---|---|
| Structured logs | `UserRegistered` with `userId`, `username` (no email in info logs if policy prefers privacy), duration; validation failures at warning without password; duplicate username/email at info |
| Traces | Span around register command (ServiceDefaults) |
| Metrics | `minimal for MVP` â€” optional counter `registrations_total` |
| Audit | N/A |

## 9. Edge Cases

```
EC-01: Duplicate username â†’ 422 USERNAME_TAKEN; no row inserted.
EC-02: Duplicate email (different username) â†’ 422 EMAIL_TAKEN.
EC-03: Simultaneous duplicate submissions â†’ database unique constraint; one succeeds, one fails; never two wallets for one identity.
EC-04: Request timeout after server committed â†’ client retry shows EMAIL_TAKEN or USERNAME_TAKEN; user directed to log in (US-02).
EC-05: Already authenticated visitor opens /register â†’ redirect to trading; no second account.
EC-06: Password confirmation mismatch (UI-only field) â†’ block submit client-side; server does not receive request.
EC-07: Email with leading/trailing spaces â†’ trim before validation and persistence.
EC-08: Username with invalid characters (e.g. space, @) â†’ 422 validation before DB.
EC-09: PostgreSQL unavailable â†’ 500; no orphaned user without wallet if transaction boundaries enforced.
EC-10: Redis session cache write fails after PG commit â†’ user still has valid PG session; cache repopulates on next read (DB Â§12.2).
EC-11: Non-AAPL symbol â†’ N/A for registration.
EC-12: Session expired â†’ N/A on register path; handled at login/protected routes.
```

## 10. Dependencies, Risks, Decision Triggers

- **Depends on:** Base API host, PostgreSQL, Redis, Aspire local stack; empty or seed `symbols` table (AAPL) for later trading but not for register itself.

- **Impacts:** US-02 Login (session model must match); US-03 Cash balance display; all authenticated trading stories.

- **External services:** PostgreSQL, Redis (session cache), Aspire â€” no third-party identity provider.

- **Key risk:** Split transaction (user created without wallet) breaks money invariants â€” mitigated by atomic unit-of-work (BR-01).

- **Decision triggers:** If product requires email verification before trading â†’ new spec + defer auto-trading until verified.

## 11. Assumptions

- **Confirmed (product):** Registration **auto sign-in** â€” session established on success (BR-06); no redirect to login-only screen.
- **Confirmed (product):** Password requires **â‰¥ 8 characters**, at least **one letter**, **one digit**, and **one special character** (BR-05).
- **Confirmed (product):** `201` response **includes `email`** alongside `userId`, `username`, and `wallet` (Â§4b).
- Username rules in BR-03 align with `VARCHAR(32)` (DB Â§4.1).
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
| 1 | Should registration auto-sign-in, or redirect to a login screen? | BR-06, PRD Â§7.4 | **Yes â€” auto sign-in** (session on success) | âœ… |
| 2 | Exact password rules beyond "length + variety" â€” special characters required? | PRD FR-1.1 | **Yes â€”** min 8, letter, digit, and special character (BR-05) | âœ… |
| 3 | Should `201` response include email in body, or only username + userId? | API sketch | **Yes â€” include `email`** in response body | âœ… |
| 4 | Return **409** vs **422** for duplicate username/email? | API guidelines | **422** â€” Story 1 generic conflict/validation; Story 2 `USERNAME_TAKEN` / `EMAIL_TAKEN` | âœ… |


## Source 2 of 4: `docs/specs/20260525-103709-user-login.md`

---
artifact_type: spec
artifact_version: 1
id: spec-20260525-103709-user-login
title: User Login
slug: user-login
filename_template: 20260525-103709-user-login.md
created_at: 2026-05-25T10:37:09+07:00
updated_at: 2026-05-25T10:37:09+07:00
status: approved
owner: product
tags: [spec, feature, trading-simulator, auth, login, session, portfolio]
related_plan: docs/plans/20260525-190000-user-login-story-5.md
related_plans: [docs/plans/20260525-150000-user-login-story-1.md, docs/plans/20260525-160000-user-login-story-2.md, docs/plans/20260525-170000-user-login-story-3.md, docs/plans/20260525-180000-user-login-story-4.md, docs/plans/20260525-190000-user-login-story-5.md]
related_specs: [docs/specs/20260523-175509-user-registration.md]
github_epic_issue: 21
github_story_issues: [22, 23, 24, 25, 26]
prd_refs: [PRD Â§5.1 US-02, PRD Â§6.1 FR-1.2, PRD Â§7.4, PRD Â§10.1]
tech_refs: [Tech Â§6.2, Tech Â§8.1, Tech Â§15.1, Tech Â§15.2, Tech Â§15.3, Tech Â§16.2]
db_refs: [DB Â§4.1 users, DB Â§4.9 user_sessions, DB Â§6.8 user_sessions, DB Â§12.1 session cache]
search_index:
  keywords: [login, sign-in, authentication, session, cookie, email, password, credentials, wallet, portfolio, logout, returning user, INVALID_CREDENTIALS]
  bounded_contexts: [Trading]
  user_personas: [Aspiring Trader, Returning User]
---

> GitHub epic: [#21 Spec: User login (US-02)](https://github.com/tranvuongduy2003/trading-simulator/issues/21)

# Feature: User Login
> Status: DRAFT  |  Date: 2026-05-25
> PRD: PRD Â§5.1 US-02, Â§6.1 FR-1.2, Â§7.4, Â§10.1
> Tech: Tech Â§6.2, Â§8.1, Â§15
> DB: DB Â§4.1, Â§4.9, Â§6.8, Â§12.1
> Owner: Product

## 1. Problem & Solution

**Problem:** A returning user who already registered cannot access their wallet, portfolio, or trading tools without signing in again. Without login, protected routes redirect guests away and the simulator cannot associate activity with the correct account.

**Solution:** Allow a returning user to authenticate with **email and password**, receive a server-issued session (cookie-based), and reach the main trading view with their existing virtual cash and **AAPL** holdings. Sessions survive page reload until the user logs out or the session expires. Users can log out explicitly at any time.

**Persona:** Returning user (Aspiring Trader) who registered earlier (US-01) and returns to the local web app on the same browser or after closing the tab.

**Smallest valuable version:** Login form + successful authentication + session cookie + redirect to trading view showing **their** wallet/portfolio (not a new account). Logout clears the session. Invalid credentials return a single safe error. Password reset, email verification, and username-based login are out of this phase.

## 2. User Stories & Acceptance Criteria

### Story 1: Log in and access my portfolio
> As a **returning user**, I want to **log in with my email and password**, so that **I can access my portfolio and trade**.

**Happy path:**
- GIVEN I am logged out on the login screen and an account exists for `jane@example.com` with password `SecurePass1!` â†’ WHEN I submit that email and password â†’ THEN the system validates credentials, creates an authenticated session (new session row + session cookie), and navigates me to the main trading view within **2 s** (local MVP).
- GIVEN login succeeded â†’ WHEN the trading view loads â†’ THEN `GET /api/wallet` (session probe) returns **my** `userId`, `username`, and balances (not USD 100,000 unless that is my actual balance); `GET /api/portfolio` returns **my** holdings (e.g. **0** or prior **AAPL** quantity from past trades).
- GIVEN I attempted to open a protected route while logged out (e.g. `/trading`) â†’ WHEN login succeeds â†’ THEN I am returned to that route (or trading view if none was saved) with my session active.

**Failure / edge path:**
- GIVEN I am already authenticated â†’ WHEN I open `/login` â†’ THEN I am redirected to the trading view without issuing a second session for the same browser tab flow (no duplicate login UX).

---

### Story 2: Reject invalid credentials safely
> As a **returning user**, I want **clear feedback when login fails**, so that **I know to fix my credentials without exposing whether an email is registered**.

**Happy path:**
- GIVEN valid credentials â†’ WHEN I log in â†’ THEN HTTP **200** (or **204** if no body) and session cookie set (Story 1).

**Failure / edge path:**
- GIVEN no account exists for `unknown@example.com` â†’ WHEN I submit any password â†’ THEN HTTP **401**, stable `code` `INVALID_CREDENTIALS`, and a generic human message (e.g. "Email or password is incorrect."); **no** session cookie; response does not state that the email is unregistered.
- GIVEN account `jane@example.com` exists but password is wrong â†’ WHEN I submit â†’ THEN the same **401** / `INVALID_CREDENTIALS` message as for unknown email (no distinction in status, code, or timing that would enable account enumeration).
- GIVEN my account exists but I use wrong casing in email (`Jane@Example.COM`) â†’ WHEN I submit â†’ THEN login succeeds if normalized email matches stored value (BR-04 alignment with registration).

---

### Story 3: Keep me signed in across reloads until logout or expiry
> As a **returning user**, I want **my session to persist when I refresh the page**, so that **I do not have to log in on every visit during a session**.

**Happy path:**
- GIVEN I logged in successfully â†’ WHEN I reload the browser or open a new tab to the same origin â†’ THEN I remain authenticated and can load protected routes without re-entering credentials (session cookie sent; `GET /api/wallet` returns **200**).
- GIVEN my session is valid and within the configured lifetime (default **24 hours** from issuance, Tech session configuration) â†’ WHEN I return after a short absence â†’ THEN I am still authenticated.

**Failure / edge path:**
- GIVEN my session expired (past `expires_at`) or was revoked â†’ WHEN I call a protected endpoint or reload the app â†’ THEN I receive HTTP **401**, the client clears local auth state, and I am directed to login with a session-expired message (no silent anonymous access to wallet/orders).
- GIVEN cookies are disabled or blocked â†’ WHEN I submit login â†’ THEN I see a clear message that cookies are required; protected features do not appear authenticated.

---

### Story 4: Log out when I am done
> As a **returning user**, I want to **log out**, so that **others using my device cannot access my account**.

**Happy path:**
- GIVEN I am authenticated â†’ WHEN I choose Log out (user menu or equivalent) â†’ THEN the server invalidates the current session (`revoked_at` set), the session cookie is cleared, and I am navigated to the login screen within **2 s**.
- GIVEN I logged out â†’ WHEN I try `GET /api/wallet` or open `/trading` â†’ THEN I am unauthenticated (**401** on API; redirect to login on UI).

**Failure / edge path:**
- GIVEN I am already logged out â†’ WHEN I call `POST /api/auth/logout` â†’ THEN the API responds safely (**204** or **401** without error leakage); no crash; cookie remains absent.

---

### Story 5: Validate login input and recover from transient failures
> As a **returning user**, I want **validation and retry guidance on the login form**, so that **I can fix mistakes and recover from network glitches**.

**Happy path:**
- GIVEN I am on the login form â†’ WHEN I submit valid email and password once â†’ THEN exactly one new active session is created for that login action (no duplicate session rows from a single intentional submit).

**Failure / edge path:**
- GIVEN email `not-an-email` â†’ WHEN I submit â†’ THEN HTTP **422** with field-level errors for `email`; no session created.
- GIVEN password empty â†’ WHEN I submit â†’ THEN **422** with `password` validation errors.
- GIVEN malformed JSON or missing fields â†’ WHEN I submit â†’ THEN **400** with problem-details body.
- GIVEN the API returns **500** or the network times out â†’ WHEN I submit â†’ THEN I see a generic retry message, the submit control re-enables, and I can try again; if the first attempt actually succeeded, a retry may redirect me as already authenticated rather than creating confusing duplicate sessions.
- GIVEN I double-click Log in rapidly â†’ WHEN both requests reach the server â†’ THEN at most one active session cookie is effective for the browser; no inconsistent auth state on the client.

## 3. Domain & Business Rules

```
BR-01: Login authenticates an existing user only. It must not create users, wallets, or portfolios. Registration (US-01) remains the sole path for new accounts.

BR-02: Login identifier for MVP is email + password. Email is normalized (trim, lowercase) before lookup, consistent with registration (BR-04 in registration spec).

BR-03: Password verification compares the submitted password to the stored salted hash only; plaintext passwords are never logged or persisted on login.

BR-04: On successful login, the system issues a new server-side session (UUID session id) with `expires_at` = now + configured session lifetime (default 24 hours). A new session cookie is set (HttpOnly, Secure on HTTPS, SameSite appropriate for local MVP). Prior sessions for the same user may remain valid until expiry or logout unless product chooses single-session â€” MVP default: multiple concurrent sessions allowed; logout revokes only the current session.

BR-05: Failed login (unknown email or wrong password) returns the same client-visible outcome: HTTP 401, code INVALID_CREDENTIALS, generic message. No hint whether email exists.

BR-06: Logout sets `revoked_at` on the current session row, removes Redis `session:{id}` cache entry, and clears the session cookie.

BR-07: Protected APIs require a valid, non-expired, non-revoked session. Registration and login endpoints remain public (Tech Â§15.3).

BR-08: Login does not modify wallet balances, holdings, or orders. It only establishes identity for subsequent commands/queries.

BR-09: Virtual cash and AAPL holdings shown after login reflect PostgreSQL authoritative state (may differ from USD 100,000 if the user has traded or reset portfolio).

BR-10: Session lookup uses Redis cache `session:{session_id}` with TTL aligned to expiration; PostgreSQL `user_sessions` is authoritative on cache miss (DB Â§12.2).
```

## 4. UI Behavior **or** API Contract

### 4a. UI Behavior

```
Screen: Login (unauthenticated, public route)
- Arrival: Guest sees Email, Password, primary action "Log in", link "No account? Register" â†’ `/register`. If already authenticated, redirect to trading (EC-05).
- Action: User submits â†’ inline/field errors for validation; on success navigate to trading (or saved `from` path). Clear password field on INVALID_CREDENTIALS; preserve email.
- Loading: Disable submit and show button loading state within 100 ms of click.
- Empty: Form starts empty (optional: remember email in sessionStorage â€” out of scope unless added in plan).
- Error: Map API codes â€” INVALID_CREDENTIALS â†’ "Email or password is incorrect."; VALIDATION_FAILED â†’ per-field; session expired on protected route â†’ "Your session has expired. Please log in again." with link to login.
- Real-time: None on login screen. After redirect, trading view uses existing session probe (`GET /api/wallet`) and may connect SignalR as authenticated user (separate features).

Screen: Main trading view (post-login)
- Arrival: Authenticated user sees AAPL layout; top bar user menu includes Log out (Story 4).
- Error: 401 on session probe â†’ clear auth store, redirect to login.

Screen: Protected routes (e.g. /trading)
- Arrival when logged out: Redirect to login with return state; after login, restore intended path when practical.
```

### 4b. API Contract

- **Endpoint(s):**
  - `POST /api/auth/login` (public)
  - `POST /api/auth/logout` (authenticated â€” requires valid session cookie)
  - Session probe (existing): `GET /api/wallet` (protected)

- **Login request (JSON):**
```json
{
  "email": "jane@example.com",
  "password": "SecurePass1!"
}
```

- **Login success:** HTTP **200 OK** (or **204 No Content** if no body)
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "username": "trader_jane",
  "email": "jane@example.com"
}
```
  Response sets **HttpOnly, Secure (HTTPS), SameSite** session cookie. No password or hash in body. Wallet/portfolio are fetched via separate GETs (read-your-writes below).

- **Logout success:** HTTP **204 No Content**; clears session cookie; revokes server session.

- **Errors (RFC 7807):**

| HTTP | `code` (example) | When |
|------|------------------|------|
| 400 | `INVALID_REQUEST` | Malformed body |
| 401 | `INVALID_CREDENTIALS` | Wrong email/password (uniform) |
| 401 | `UNAUTHORIZED` | Missing/invalid/expired session (protected routes & logout without session) |
| 422 | `VALIDATION_FAILED` | Email format empty password, etc. |
| 500 | `INTERNAL_ERROR` | Unexpected failure |

- **Auth:** Login and register are public. Logout and all trading reads/writes require session. Cookie-based; client sends `credentials: 'include'`.

- **Idempotency:** Login is not strictly idempotent (each success may create a new session row). Clients should disable double-submit. Logout is idempotent for the same session.

- **Pagination / filtering:** N/A.

## 5. Data & Storage Impact

| Concern | Answer |
|---|---|
| PostgreSQL tables / columns | **Insert** `user_sessions` on login (id, user_id, created_at, expires_at, last_seen_at, revoked_at null). **Update** `user_sessions.revoked_at` on logout. **Read** `users` by normalized email for credential check. No change to `wallets` / `portfolios` on login. |
| Redis keys / projections | **Set** `session:{session_id}` on login with TTL = time until `expires_at`. **Delete** on logout. No order-book/tape/candle changes. |
| Matching / channel behavior | **None** â€” login does not enqueue orders. |
| Migration needed | **No** if `user_sessions` and `users` already exist from registration work. |
| Rebuild strategy if Redis cleared | Session cache miss loads from PostgreSQL `user_sessions`; expired/revoked sessions reject with 401. User re-logs in if cookie invalid. |

Cross-check: `ux_users_email` for login lookup (DB Â§6.1); `ix_user_sessions_expires` for cleanup (DB Â§6.8).

## 6. Real-Time & Consistency

- **SignalR events:** None emitted specifically for login/logout in MVP. After login, client may connect to simulation hub with authenticated context (`user:{userId}` group per existing hub design).

- **Read-your-writes:** Immediately after login success, the same session cookie must authorize `GET /api/wallet` and `GET /api/portfolio` returning **that user's** current balances and holdings without stale empty data.

- **Stale UI handling:** On 401, client clears auth store and routes to login. Reload with valid cookie re-probes wallet. Logout must invalidate server session so a stale tab cannot place orders after logout (next API call â†’ 401).

## 7. Security & Privacy (MVP)

- **Authn / Authz:** Login is public; uniform invalid-credentials response (BR-05). Logout requires current session. No cross-user data in login response.

- **Sensitive fields:** Password only in transit (HTTPS); never log password, session id in client logs at info level, or password hash.

- **Threat surface:** Credential stuffing â€” generic errors, optional local rate limit deferred; session fixation â€” issue new session id on login; CSRF â€” anti-forgery on cookie-based state-changing browser requests where applicable (Tech Â§15.5); overspend/cancel others' orders â€” prevented by session-bound user id on commands (not login-specific but enabled by login).

## 8. Observability (local MVP)

| Signal | What to emit |
|---|---|
| Structured logs | `UserLoggedIn` with `userId` (no password); `LoginFailed` at info without email in message if privacy-sensitive; `UserLoggedOut` with `userId`, `sessionId`; validation failures at warning |
| Traces | Span around login/logout commands (ServiceDefaults) |
| Metrics | `minimal for MVP` â€” optional `logins_total`, `login_failures_total` |
| Audit | N/A |

## 9. Edge Cases

```
EC-01: Wrong password for valid email â†’ 401 INVALID_CREDENTIALS; same as unknown email.
EC-02: Unknown email â†’ 401 INVALID_CREDENTIALS; no user enumeration.
EC-03: Email normalization (case/trim) â†’ successful login when credentials match stored user.
EC-04: Session expired â†’ 401 on protected API; UI session-expired flow.
EC-05: Already authenticated on /login â†’ redirect to trading.
EC-06: Login then immediate reload â†’ still authenticated.
EC-07: Logout then back button â†’ protected API returns 401; no trading as prior user.
EC-08: Double-submit login â†’ at most one effective session cookie; client guards duplicate clicks.
EC-09: Timeout after server created session â†’ retry may succeed as authenticated or show already logged in; user not stuck permanently.
EC-10: Redis cache write fails after PG session insert â†’ session still valid via PG on read path (DB Â§12.2).
EC-11: PostgreSQL unavailable â†’ 500 on login; no partial session without user resolution.
EC-12: Cancel filled order / insufficient funds / non-AAPL symbol â†’ N/A on login path (handled in trading features).
EC-13: Engine down / channel backlog â†’ N/A for login.
```

## 10. Dependencies, Risks, Decision Triggers

- **Depends on:** US-01 User Registration (users, password hashes, session infrastructure, `GET /api/wallet` probe, auth middleware, public/protected route guards).

- **Impacts:** All protected features (US-03 wallet display, order placement, portfolio views) require working login/logout; registration "Already have account? Log in" link becomes functional.

- **External services:** PostgreSQL, Redis, Aspire â€” same as registration.

- **Key risk:** Frontend currently stubs login UI while API may not implement `POST /api/auth/login` yet â€” contract and session cookie behavior must match registration's session model to avoid divergent auth paths.

- **Decision triggers:** If product requires username login or single-session-only policy, document in `docs/memory/decisions.md` before `/plan`.

## 11. Assumptions

- Login uses **email + password** (matches existing client `LoginRequest` shape), not username, unless product revises.
- Successful login returns a minimal user identity payload; wallet/portfolio loaded via existing GET endpoints (same as post-registration flow).
- Default session lifetime **24 hours** (Tech configuration `Session:ExpirationHours`).
- Multiple active sessions per user are allowed in MVP; logout revokes only the current session.
- No CAPTCHA or rate limiting in MVP (local simulator); may add in future scope.

## 12. Out of Scope

- Password reset / forgot password
- Email verification and magic-link login
- Username-based login (email only for MVP)
- "Remember me" beyond default session cookie lifetime
- Multi-factor authentication
- OAuth / social login
- Admin impersonation
- Login audit dashboard
- Advanced throttling and account lockout
- Global MVP exclusions: message broker, multi-symbol, production CD, fractional shares

**Future scope (Phase 2+ one-liners):** Password reset flow; optional username login; rate limiting; revoke-all-sessions; device/session list in account settings.

## 13. Open Questions

| # | Question | Source | Answer | Status |
|---|---|---|---|---|
| 1 | Should login return wallet summary in the response body (like registration 201) or only user id + separate GETs? | API ergonomics | Assume separate GETs (wallet probe already exists) unless plan unifies | â³ Deferred |
| 2 | Single active session per user vs multiple concurrent sessions? | BR-04 | Multiple allowed; logout revokes current only | âœ… Answered |
| 3 | HTTP 200 with body vs 204 on login success? | API contract | Prefer 200 + minimal identity JSON for client auth store | â³ Deferred |


## Source 3 of 4: `docs/specs/20260525-201500-virtual-cash-balance.md`

---
artifact_type: spec
artifact_version: 1
id: spec-20260525-201500-virtual-cash-balance
title: Virtual Cash Balance Display
slug: virtual-cash-balance
filename_template: 20260525-201500-virtual-cash-balance.md
created_at: 2026-05-25T20:15:00+07:00
updated_at: 2026-05-25T20:45:00+07:00
status: approved
owner: product
tags: [spec, feature, trading-simulator, wallet, cash, balance, portfolio, us-03]
related_plan: docs/plans/20260525-240000-virtual-cash-story-4.md
related_plans: [docs/plans/20260525-203000-virtual-cash-story-1.md, docs/plans/20260525-220000-virtual-cash-story-2.md, docs/plans/20260525-230000-virtual-cash-story-3.md, docs/plans/20260525-240000-virtual-cash-story-4.md]
related_specs: [docs/specs/20260523-175509-user-registration.md, docs/specs/20260525-103709-user-login.md]
github_epic_issue: 33
github_story_issues: [34, 35, 36, 37]
prd_refs: [PRD Â§5.1 US-03, PRD Â§6.1 FR-1.3, PRD Â§6.6 FR-6.2, PRD Â§7.3, PRD Â§7.4, PRD Â§8.1, PRD Â§10.1]
tech_refs: [Tech Â§5.2.1 User/Wallet, Tech Â§6 GetMyWalletQuery, Tech Â§8.1 Wallet endpoint, Tech Â§9.2 user group, Tech Â§15.1]
db_refs: [DB Â§4.2 wallets, DB Â§5 invariants, DB Â§6.2 wallets indexes]
search_index:
  keywords: [wallet, cash, balance, available, reserved, total, virtual cash, USD, trading dashboard, GetMyWallet, session, read-your-writes, FR-6.2, US-03]
  bounded_contexts: [Trading]
  user_personas: [Aspiring Trader, Authenticated User]
---

> GitHub epic: [#33 Spec: Virtual cash balance display (US-03)](https://github.com/tranvuongduy2003/trading-simulator/issues/33)

# Feature: Virtual Cash Balance Display
> Status: DRAFT  |  Date: 2026-05-25
> PRD: PRD Â§5.1 US-03, Â§6.1 FR-1.3, Â§6.6 FR-6.2, Â§7.3â€“7.4, Â§8.1, Â§10.1
> Tech: Tech Â§5.2.1, Â§6, Â§8.1, Â§9.2, Â§15.1
> DB: DB Â§4.2, Â§5, Â§6.2
> Owner: Product

## 1. Problem & Solution

**Problem:** After registering or logging in, a user cannot confidently know how much virtual USD they can spend on the next order. Without a clear, trustworthy cash display, limit and market buy flows (US-10â€“US-15) feel opaque and error-prone.

**Solution:** On the authenticated main trading view, show the userâ€™s virtual cash with **available** balance as the primary figure they trade against, plus **total** and **reserved** breakdown per FR-6.2. Balances come from the authoritative wallet in PostgreSQL; only the signed-in user may read their own wallet.

**Persona:** Authenticated trader (Aspiring Trader) using the local **AAPL** simulator after US-01 registration and/or US-02 login.

**Smallest valuable version:** Read-only cash display on the trading dashboard + protected `GET /api/wallet` + loading/error/session handling + balances that match server state after login and on manual refresh. Real-time SignalR wallet pushes, top-bar compact wallet chip, and portfolio reset (US-04) are Phase 2 / separate stories.

## 2. User Stories & Acceptance Criteria

### Story 1: See how much cash I can trade with
> As a **user**, I want to **see my available virtual cash balance prominently**, so that **I know how much I can use for the next buy order**.

**Happy path:**
- GIVEN I am authenticated and on the main trading view â†’ WHEN the wallet loads within **2 s** (local MVP) â†’ THEN I see a labeled **Virtual cash** (or equivalent) area showing **available** balance formatted as USD (e.g. **$100,000.00** for a new account).
- GIVEN I registered today and have no open buy orders â†’ WHEN the wallet loads â†’ THEN **available** equals **$100,000.00**, **total** equals **$100,000.00**, and **reserved** equals **$0.00** (FR-1.3 initial capital).
- GIVEN amounts are stored as `NUMERIC(18,4)` â†’ WHEN displayed in the UI â†’ THEN money shows **two** decimal places for USD; quantities elsewhere remain whole shares.

**Failure / edge path:**
- GIVEN wallet data is still loading â†’ WHEN I view the trading screen â†’ THEN I see a non-blocking loading state (skeleton or inline placeholder) in the cash area without layout jump that hides the rest of the page.
- GIVEN `GET /api/wallet` fails with a non-auth error (e.g. **500**) â†’ WHEN the trading view renders â†’ THEN I see a human-readable error (e.g. â€œCould not load account data. Try refreshing or sign in again.â€) and no fabricated balance figures.

---

### Story 2: Understand total versus reserved cash
> As a **user**, I want to **see total and reserved cash alongside available**, so that **I understand why my spendable amount is lower when I have open buy orders**.

**Happy path:**
- GIVEN my wallet has **total** **$50,000.0000**, **reserved** **$10,000.0000** â†’ WHEN I view the cash area â†’ THEN **available** displays **$40,000.00** and secondary copy shows **Total $50,000.00 Â· Reserved $10,000.00** (or equivalent clear labels).
- GIVEN **reserved** is **$0.00** â†’ WHEN I view the cash area â†’ THEN reserved is still visible as **$0.00** or concise copy that none is tied up in open buys (no hidden reserved state).

**Failure / edge path:**
- GIVEN the API returns inconsistent numbers where `availableBalance` â‰  `totalBalance - reservedBalance` (data bug) â†’ WHEN the UI renders â†’ THEN the UI still shows the three fields returned by the API and does not invent a fourth figure; operators treat this as a defect against BR-03.

---

### Story 3: Only see my own balance when signed in
> As a **user**, I want **my cash balance to be private to my session**, so that **no one else can read my wallet through the app**.

**Happy path:**
- GIVEN I am authenticated as user A â†’ WHEN `GET /api/wallet` runs â†’ THEN the response `userId` matches my account and balances reflect **my** wallet only (never another userâ€™s **$100,000** default unless that is actually my balance).
- GIVEN I just logged in as user B who previously traded â†’ WHEN the trading view loads â†’ THEN I do **not** briefly see user Aâ€™s cached balance from a prior session (client must not show stale wallet data across users).

**Failure / edge path:**
- GIVEN I am not authenticated (no valid session cookie) â†’ WHEN the client calls `GET /api/wallet` â†’ THEN HTTP **401** with stable `code` `UNAUTHORIZED`; the trading route redirects to login (or shows session-expired messaging per US-02).
- GIVEN my session expired â†’ WHEN I open the trading view â†’ THEN wallet fetch returns **401**, local auth state clears, and I am prompted to sign in again without displaying numeric balances.

---

### Story 4: Trust balances after login and refresh
> As a **user**, I want **the displayed cash to match the server after I sign in or refresh**, so that **I can place orders with confidence**.

**Happy path:**
- GIVEN I log in successfully â†’ WHEN the trading view loads â†’ THEN wallet data loads without requiring a second login and shows my current PostgreSQL balances (read-your-writes within **2 s**).
- GIVEN I am on the trading view and press browser refresh â†’ WHEN the page reloads with a valid session â†’ THEN the same balances reappear (session persistence per FR-1.2).
- GIVEN I return to the trading tab after placing or cancelling a buy order in a later release â†’ WHEN the wallet query refetches (focus, interval, or post-mutation invalidation) â†’ THEN **available**, **total**, and **reserved** reflect the latest reserved amounts for open buys (BR-04). *Until order placement ships, manual refresh must show the same values as `GET /api/wallet`.*

**Failure / edge path:**
- GIVEN I had the trading view open before a trade filled elsewhere (second tab) â†’ WHEN I focus the tab or refresh â†’ THEN balances update to the server state on the next successful wallet fetch (no permanent stale **$100,000** if reserved cash changed).
- GIVEN PostgreSQL is temporarily unavailable â†’ WHEN `GET /api/wallet` runs â†’ THEN HTTP **500** and the UI error path from Story 1; no partial wallet row is shown.

## 3. Domain & Business Rules

```
BR-01: One wallet per user. Wallet rows are keyed by user_id; a user never has two wallet records (DB Â§5, Tech Â§5.2.1).

BR-02: Available cash = total_balance âˆ’ reserved_balance. Available is computed at read time, not stored (DB Â§4.2). It is the amount the user can allocate to new buy orders (PRD FR-3.3, FR-6.2).

BR-03: Wallet invariants: total_balance â‰¥ 0, reserved_balance â‰¥ 0, reserved_balance â‰¤ total_balance, and available â‰¥ 0 always (Tech Â§5.2.1, PRD Â§7.3). Violations are system defects, not user-facing states.

BR-04: Reserved balance increases when cash is reserved for open buy orders and decreases on cancel or fill settlement (Tech Â§5.2.1). US-03 display must reflect reservations once order placement exists; until then reserved stays 0 for new users.

BR-05: Initial virtual cash for new registrations is USD 100,000.0000 with reserved 0 (PRD FR-1.3, registration spec BR-02). Currency is USD only for MVP (`wallets.currency` = USD).

BR-06: Virtual cash is non-redeemable simulation money (PRD Â§3.2). UI copy may remind users it is not real money where appropriate; no withdrawal or transfer flows.

BR-07: Wallet reads are scoped to the authenticated session user. Cross-user wallet access is forbidden at the application layer (Tech Â§15.1).

BR-08: PostgreSQL is authoritative for wallet balances; Redis does not store wallet projections for MVP (Tech Â§3, DB Â§4.2). Clearing Redis does not change displayed cash after refetch.
```

## 4. UI Behavior **or** API Contract

### 4a. UI Behavior

```
Screen: Main trading view (authenticated)
- Arrival: User lands after login/register redirect or navigates to /trading (or equivalent). Cash area visible without extra navigation (PRD Â§7.4 â€” portfolio/cash reachable within 2 clicks; on-dashboard display satisfies US-03 MVP).
- Primary display: "Virtual cash" card on trading view showing available balance large (tabular numerals), subtitle "Available to trade".
- Top bar (all authenticated routes): compact chip with **available** cash (PRD Â§8.1, spec Â§13 Q1 âœ…) â€” label "Cash" + formatted amount; loading skeleton / no fake balance on error.
- Secondary display: Total and reserved on one line or tooltip-friendly secondary text (FR-6.2).
- Loading: Skeleton or inline placeholder in the cash card while wallet (and optionally portfolio) queries are pending; do not show $0.00 as if real.
- Empty: N/A for wallet (every authenticated user has a wallet row). If API returns 404 for wallet (defect), show error state, not empty zero balance.
- Error: Non-401 failures â†’ destructive text + suggestion to refresh or sign in again. 401 â†’ session flow (redirect login / session expired banner per login spec).
- Real-time (Phase 1): No SignalR requirement for US-03; refetch on mount, window focus, and after auth-changing mutations is sufficient. Future: user-group balance push when order stories ship.
```

```
Screen: Registration success / Login success (transition)
- After 201 register or 200 login, trading view must load wallet and show starting **$100,000.00** available for a new account without an extra manual step.
```

### 4b. API Contract

- **Endpoint:** `GET /api/wallet`
- **Auth:** Session required (cookie). Public routes: register/login only (Tech Â§15.1).
- **Response 200 (JSON sketch):**
  ```json
  {
    "userId": "<uuid>",
    "username": "trader_jane",
    "currency": "USD",
    "totalBalance": "100000.0000",
    "reservedBalance": "0.0000",
    "availableBalance": "100000.0000"
  }
  ```
  Monetary fields may serialize as string or number; client normalizes to decimal for display.
- **Errors:**
  - **401** `UNAUTHORIZED` â€” missing, expired, or revoked session.
  - **500** `INTERNAL_ERROR` â€” infrastructure failure; no wallet body.
- **Idempotency:** Read-only; safe to repeat. Used as session probe in auth flows (login spec).
- **Pagination:** N/A (single wallet row per user).

## 5. Data & Storage Impact

| Concern | Answer |
|---|---|
| PostgreSQL tables / columns | **Read** `wallets` (`user_id`, `total_balance`, `reserved_balance`, `currency`, `updated_at`, `row_version`). No new columns for US-03. |
| Redis keys / projections | **None** for wallet in MVP. |
| Matching / channel behavior | **None** for read path. Writes to `wallets` occur on order place/cancel/match (future order stories); US-03 consumes results via GET. |
| Migration needed | **No** if `wallets` table already deployed (registration). |
| Rebuild strategy if Redis cleared | **N/A** â€” wallet not in Redis. |

Cross-check: `pk_wallets` on `user_id` (DB Â§6.2); check constraints on non-negative balances (DB Â§7).

## 6. Real-Time & Consistency

- **SignalR events:** Not required for US-03 Phase 1. Tech Â§9.2 user group may later emit balance updates on fill/cancel; defer to order/portfolio stories. MVP uses HTTP refetch.
- **Read-your-writes:** After login/register, first `GET /api/wallet` on the trading view must return current PostgreSQL balances within **2 s** â€” not zero, not another userâ€™s data.
- **Stale UI handling:** On session user change (logout/login as different user), clear wallet query cache. On tab focus or manual refresh, refetch wallet. When order placement ships, invalidate wallet query after successful place/cancel mutations.

## 7. Security & Privacy (MVP)

- **Authn / Authz:** Session identifies user; wallet query returns only the callerâ€™s row (BR-07).
- **Sensitive fields:** Balances are not secrets but are personal financial simulation data â€” never log full wallet responses at info level in shared logs.
- **Threat surface:** Guessing another userâ€™s `userId` must not change wallet results; no `userId` query parameter on GET. Session fixation mitigated by login spec; US-03 relies on existing session model.

## 8. Observability (local MVP)

| Signal | What to emit |
|---|---|
| Structured logs | Optional debug on wallet read failures; log `userId` only at debug. No balance values required in MVP logs. |
| Traces | Span on `GetMyWallet` handler if ServiceDefaults enabled â€” minimal for MVP. |
| Metrics | `minimal for MVP` |
| Audit | N/A (read-only) |

## 9. Edge Cases

```
EC-01: New user, no trades, no open orders â†’ available = total = $100,000.00, reserved = $0.00.

EC-02: Open limit buy reserved $5,000 â†’ total unchanged, reserved $5,000, available reduced by $5,000 (display all three).

EC-03: Session expired mid-view â†’ wallet GET 401 â†’ clear auth, redirect login, hide balances.

EC-04: User B logs in after User A on same browser â†’ no display of Aâ€™s balances after Bâ€™s session is established.

EC-05: Wallet API 500 â†’ error message, no fake zeros.

EC-06: Very large balance within NUMERIC(18,4) â†’ UI formats without scientific notation; no overflow in display layer.

EC-07: Concurrent wallet read during matching settlement â†’ READ COMMITTED may show slightly stale reserved until refetch; acceptable for MVP if refetch on focus (document in plan).

EC-08: Portfolio reset (US-04, future) â†’ wallet returns to $100,000 total, $0 reserved; US-03 display must update after refetch when that feature exists.
```

## 10. Dependencies, Risks, Decision Triggers

- **Depends on:** US-01 Registration (wallet row + initial capital), US-02 Login (session + protected routes). `wallets` schema and `GET /api/wallet` contract.
- **Impacts:** US-10â€“US-15 order placement (users compare available cash to order cost); US-15 validation messaging; US-23 portfolio valuation (cash component).
- **External services:** PostgreSQL (authoritative); session store (auth gate only).
- **Key risk:** Client shows cached wallet from a previous user or stale TanStack Query data after logout â€” mitigated by cache clear on auth transitions (Story 3).
- **Decision triggers:** If cash moves to top bar only (PRD Â§8.1), update layout spec and ensure still within 2-click rule â€” add to `docs/memory/decisions.md`.

## 11. Assumptions

- **Confirmed (product):** Main trading view is the primary surface for US-03 MVP (not only the future Holdings tab).
- **Confirmed (product):** `GET /api/wallet` already exists in the API contract; US-03 is end-to-end correctness and UX, not inventing a new resource.
- **Assumed:** Phase 1 does not require SignalR wallet events; HTTP refetch is acceptable (PRD real-time targets apply to market data and fills, not strictly to cash read on first delivery).
- **Assumed:** Registration/login flows already navigate to trading view; US-03 tightens display and cross-user cache rules.

## 12. Out of Scope

- Placing, cancelling, or matching orders (US-10â€“US-19) â€” only display impact when reserves exist.
- Holdings detail, unrealized P&L, total portfolio value (US-20, US-21, US-23) â€” may appear adjacent on trading view but not part of US-03 acceptance.
- Portfolio reset UI and behavior (US-04).
- Real-time SignalR push for wallet-only updates (Phase 2 enhancement).
- Full PRD Â§8.1 top bar (AAPL symbol, last price, daily change) â€” Story 1 adds **available cash chip only** per Q1 âœ….
- Multi-currency, fractional shares, multi-symbol.
- Global MVP exclusions: message broker, outbox, production CD, horizontal scaling.

## 13. Open Questions

| # | Question | Source | Answer | Status |
|---|---|---|---|---|
| 1 | Should available cash also appear in the top bar before the full terminal layout ships? | PRD Â§8.1 | **Yes** â€” compact available-cash chip in `AppLayout` on all authenticated routes (Story 1); full terminal top bar (symbol, price) still deferred | âœ… |
| 2 | After order placement ships, is refetch-on-mutation enough or is SignalR wallet push required for NFR latency? | Tech Â§9.2 | â€” | â³ Deferred to order epic |


## Source 4 of 4: `docs/specs/20260525-251500-portfolio-reset.md`

---
artifact_type: spec
artifact_version: 1
id: spec-20260525-251500-portfolio-reset
title: Portfolio Reset
slug: portfolio-reset
filename_template: 20260525-251500-portfolio-reset.md
created_at: 2026-05-25T25:15:00+07:00
updated_at: 2026-05-25T25:15:00+07:00
status: approved
owner: product
tags: [spec, feature, trading-simulator, portfolio, reset, wallet, orders, cooldown, us-04]
related_plan: docs/plans/20260528-003204-portfolio-reset-story-5.md
related_specs: [docs/specs/20260523-175509-user-registration.md, docs/specs/20260525-103709-user-login.md, docs/specs/20260525-201500-virtual-cash-balance.md]
github_epic_issue: 43
github_story_issues: [44, 45, 46, 47, 48]
prd_refs: [PRD Â§5.1 US-04, PRD Â§6.1 FR-1.4, PRD Â§6.5 FR-5.1, PRD Â§6.6 FR-6.1â€“FR-6.4, PRD Â§7.3â€“7.4, PRD Â§8.1, PRD Â§10.1]
tech_refs: [Tech Â§5.2.1 Wallet, Tech Â§5.2.2 Portfolio, Tech Â§5.2.3 Order, Tech Â§5.4 PortfolioResetEvent, Tech Â§6 ResetPortfolioCommand, Tech Â§8.1 Portfolio endpoints, Tech Â§9.2 user group, Tech Â§15.1, Tech Â§16 Trading:PortfolioResetCooldownMinutes]
db_refs: [DB Â§4.2 wallets, DB Â§4.3 portfolios, DB Â§4.4 holdings, DB Â§4.5 orders, DB Â§4.6 trades, DB Â§4.10 portfolio_resets, DB Â§6.9 portfolio_resets indexes, DB Â§10.4 Portfolio Reset]
search_index:
  keywords: [portfolio reset, reset portfolio, cooldown, 24 hours, initial capital, 100000, cancel open orders, trade history, holdings, wallet, FR-1.4, US-04, POST portfolio reset, PortfolioResetEvent]
  bounded_contexts: [Trading]
  user_personas: [Aspiring Trader, Authenticated User]
---

> GitHub epic: [#43 Spec: Portfolio reset (US-04)](https://github.com/tranvuongduy2003/trading-simulator/issues/43)

# Feature: Portfolio Reset
> Status: DRAFT  |  Date: 2026-05-25
> PRD: PRD Â§5.1 US-04, Â§6.1 FR-1.4, Â§6.5â€“6.6, Â§7.3â€“7.4, Â§8.1
> Tech: Tech Â§5.2, Â§6, Â§8.1, Â§9.2, Â§15.1, Â§16
> DB: DB Â§4.2â€“4.6, Â§4.10, Â§6.9, Â§10.4
> Owner: Product

## 1. Problem & Solution

**Problem:** After losing virtual money or building a messy position, a user has no way to return to a known starting point. They stay stuck with depleted cash, leftover **AAPL** holdings, open orders, and a long personal trade historyâ€”discouraging experimentation and repeat play.

**Solution:** Let an authenticated user **reset their portfolio** from the app: confirm the destructive action, then atomically restore **USD 100,000** available cash (no reservations), **zero** holdings, **cancel all their open orders**, and **clear their personal trade and order history views**. Enforce **at most one reset per 24 hours** per user with a clear cooldown message when blocked.

**Persona:** Authenticated trader (Aspiring Trader) on the local **AAPL** simulator who has registered (US-01), can log in (US-02), and may already see wallet cash (US-03).

**Smallest valuable version:** Protected `POST /api/portfolio/reset` + confirmation UI + cooldown enforcement + post-reset read-your-writes across wallet, holdings, open orders, and personal trade/order history tabs. Market-wide tape, candlesticks, and other usersâ€™ data are unchanged. Leaderboards, email notifications, and admin-initiated resets are out of scope.

## 2. User Stories & Acceptance Criteria

### Story 1: Confirm before resetting
> As a **user**, I want to **confirm portfolio reset before it runs**, so that **I do not wipe my progress by accident**.

**Happy path:**
- GIVEN I am authenticated on the main trading view or portfolio area â†’ WHEN I choose **Reset portfolio** (or equivalent in the user/account menu) â†’ THEN I see a modal or dialog listing consequences: returns to **$100,000** virtual cash, **zero** **AAPL** holdings, **all open orders cancelled**, **personal trade and order history cleared**, and **24-hour cooldown** before the next reset.
- GIVEN the confirmation dialog is open â†’ WHEN I confirm â†’ THEN the client calls `POST /api/portfolio/reset` with session cookie within **2 s** (local MVP) and shows a non-blocking loading state on the confirm action until the response returns.
- GIVEN reset succeeds (HTTP **200** or **204**) â†’ WHEN the dialog closes â†’ THEN I see success copy (e.g. â€œPortfolio reset. Youâ€™re starting fresh with $100,000.â€) and the destructive action is disabled until the next eligible window (Story 4).

**Failure / edge path:**
- GIVEN I open the confirmation dialog â†’ WHEN I cancel or dismiss â†’ THEN no API call is made and my wallet, holdings, and orders are unchanged.
- GIVEN I double-click confirm â†’ WHEN the first request is in flight â†’ THEN only one reset is applied (client disables submit; server rejects duplicate in-flight attempts with **409** `RESET_IN_PROGRESS` or processes onceâ€”observable outcome: single reset row in `portfolio_resets`).

---

### Story 2: Restore starting cash and empty holdings
> As a **user**, I want **my virtual cash and holdings returned to the starting state**, so that **I can place new buy and sell orders as if I just registered**.

**Happy path:**
- GIVEN I had traded (e.g. **total** **$42,000.0000**, **reserved** **$5,000.0000**, **available** **$37,000.00**, **AAPL** holding **50** shares) â†’ WHEN reset completes successfully â†’ THEN `GET /api/wallet` within **2 s** shows **totalBalance** **100000.0000**, **reservedBalance** **0.0000**, **availableBalance** **100000.0000** (FR-1.3, FR-1.4).
- GIVEN I held **AAPL** â†’ WHEN reset completes â†’ THEN `GET /api/portfolio` shows **no holdings** (empty list or zero rows); total portfolio value equals available cash (**$100,000.00** at reset time before new market moves).

**Failure / edge path:**
- GIVEN reset fails mid-operation (HTTP **500** `INTERNAL_ERROR`) â†’ WHEN I call `GET /api/wallet` and `GET /api/portfolio` â†’ THEN balances and holdings reflect the **pre-reset** authoritative PostgreSQL state (no partial $100,000 wallet with old holdings still present).
- GIVEN I am not authenticated â†’ WHEN I call `POST /api/portfolio/reset` â†’ THEN HTTP **401** `UNAUTHORIZED` and no reset row is created.

---

### Story 3: Cancel open orders and clear my activity history
> As a **user**, I want **my open orders cancelled and my past trades and orders hidden**, so that **my workspace matches a fresh start**.

**Happy path:**
- GIVEN I have **3** open **AAPL** orders (Pending or Partially Filled) â†’ WHEN reset completes â†’ THEN `GET /api/orders/open` returns **0** orders within **2 s**.
- GIVEN those open orders were on the public book â†’ WHEN reset completes â†’ THEN the order book and market tape update within **500 ms** (PRD Â§7.1) to reflect removals/cancellations for my orders (no ghost liquidity from my cancelled orders).
- GIVEN I had **â‰¥ 1** row in my trade history and order history â†’ WHEN reset completes â†’ THEN `GET /api/trades` (my history) returns **0** items on the first page and `GET /api/orders/history` returns **0** items on the first page (FR-1.4 trade history cleared; order history cleared for fresh-start UX).
- GIVEN reset completes â†’ WHEN I view **Open Orders**, **Order History**, **Trade History**, and **Holdings** tabs on the trading view â†’ THEN all show empty states appropriate to zero data (not stale pre-reset rows).

**Failure / edge path:**
- GIVEN I have no open orders and no history â†’ WHEN I reset â†’ THEN reset still succeeds and tabs remain empty; wallet still returns to **$100,000**.
- GIVEN another userâ€™s open order would match mine â†’ WHEN my reset cancels my resting order â†’ THEN only **my** order is cancelled; the counterpartyâ€™s order remains valid on the book.

---

### Story 4: Respect the 24-hour cooldown
> As a **user**, I want to **know when I can reset again**, so that **I understand the once-per-day limit**.

**Happy path:**
- GIVEN I have never reset (no `portfolio_resets` row) â†’ WHEN I reset successfully â†’ THEN a row is recorded with `reset_at` â‰ˆ now and I can reset again only after **24 hours** (configurable default **1440** minutes per Tech Â§16 / FR-1.4).
- GIVEN my last successful reset was **25 hours** ago â†’ WHEN I reset again â†’ THEN reset succeeds and a new `portfolio_resets` row is appended.

**Failure / edge path:**
- GIVEN my last successful reset was **2 hours** ago â†’ WHEN I call `POST /api/portfolio/reset` â†’ THEN HTTP **422** (or **429**) with stable `code` `RESET_COOLDOWN_ACTIVE`, `detail` includes **nextEligibleAt** (ISO-8601 timestamp), and wallet/holdings/orders are **unchanged**.
- GIVEN cooldown is active â†’ WHEN I open the reset entry point in the UI â†’ THEN the control is disabled or shows â€œNext reset available &lt;relative time&gt;â€ using **nextEligibleAt** without calling the reset API.

---

### Story 5: See consistent data everywhere after reset
> As a **user**, I want **all portfolio-related panels to update immediately after reset**, so that **I do not see old cash, holdings, or orders**.

**Happy path:**
- GIVEN reset returns success â†’ WHEN the trading view re-renders â†’ THEN the virtual cash card and top-bar cash chip (US-03) show **$100,000.00** available within **2 s** without requiring a full logout.
- GIVEN reset returns success â†’ WHEN I stay on the trading view â†’ THEN TanStack Query (or equivalent) invalidates/refetches `wallet`, `portfolio`, `orders/open`, `orders/history`, and `trades` so no panel shows pre-reset figures after refetch completes.
- GIVEN I am connected to SignalR â†’ WHEN reset cancels my orders â†’ THEN I receive user-scoped notifications consistent with manual cancel (order cancelled) and optional balance/holding refresh payloads on the `user:{userId}` group (Tech Â§9.2).

**Failure / edge path:**
- GIVEN reset succeeded but a refetch fails (wallet **500**) â†’ WHEN the UI renders â†’ THEN cash areas show the error state from US-03 (no fabricated **$100,000**) until refetch succeeds.
- GIVEN I reset in one browser tab â†’ WHEN I focus another tab that still shows old holdings â†’ THEN focusing or refreshing that tab shows post-reset data on the next successful fetch (no permanent stale state).

## 3. Domain & Business Rules

```
BR-01: Portfolio reset is user-initiated only for MVP. No operator or admin reset in this phase (PRD FR-1.4).

BR-02: Cooldown â€” at most one successful reset per user per rolling **24 hours**, measured from the timestamp of the most recent row in `portfolio_resets` for that user (PRD FR-1.4, DB Â§4.10, DB Â§6.9). Default configuration: **1440** minutes (`Trading:PortfolioResetCooldownMinutes`, Tech Â§16).

BR-03: Initial state after reset â€” wallet **total_balance** = **100000.0000** USD, **reserved_balance** = **0.0000**, all holdings removed, all of the userâ€™s orders in Pending or Partially Filled transition to Cancelled with reservations released (PRD FR-1.4, DB Â§10.4, Tech Â§5.2.1â€“5.2.3).

BR-04: User-scoped history â€” after reset, queries for **my** trade history and **my** order history return no rows for activity that occurred before the reset instant. Market-wide recent trades (`GET /api/market/trades` or equivalent) and global candlesticks are unaffected (single-symbol **AAPL** public market data).

BR-05: Reset does not delete the user account, session, username, or email. Login session remains valid after reset (FR-1.2).

BR-06: Reset runs as one atomic unit of work in PostgreSQL: cancel open orders â†’ release reservations â†’ reset wallet â†’ remove holdings â†’ append `portfolio_resets` â†’ commit (DB Â§10.4). Failure rolls back entirely.

BR-07: Open orders cancelled by reset follow the same business rules as user-initiated cancel: reserved cash or **AAPL** quantity returns to available balances (PRD FR-5.3, Tech Â§5.2.1â€“5.2.2).

BR-08: Reset must enqueue or synchronously notify the matching engine so in-memory order book state for the userâ€™s cancelled orders is removed; no cancelled order may remain matchable (Tech Â§8.2, async matching invariant).

BR-09: System-wide cash conservation (PRD Â§7.3) â€” reset only reallocates **this userâ€™s** wallet to **$100,000**; it does not mint or destroy counterparty balances. Simulated liquidity orders are untouched.

BR-10: Whole shares only; symbol remains **AAPL** for MVP. Reset does not change symbol reference data.

BR-11: A `PortfolioResetEvent` is raised after successful persistence for downstream notifications (Tech Â§5.4).
```

## 4. UI Behavior **or** API Contract

### 4a. UI Behavior

```
Screen: Main trading view â€” user / account menu (authenticated)
- Arrival: Menu includes **Reset portfolio** (or under **Account**). If cooldown active (Story 4), control disabled with next-eligible hint.
- Action: User opens reset â†’ confirmation dialog (Story 1) â†’ on confirm, POST reset â†’ invalidate wallet/portfolio/orders/trades queries â†’ close dialog with success toast.
- Loading: Confirm button shows loading/disabled during POST; do not close dialog until success or error.
- Empty: N/A for reset entry; post-reset tabs show standard empty copy (â€œNo open ordersâ€, â€œNo trades yetâ€, etc.).
- Error: Map API codes â€” RESET_COOLDOWN_ACTIVE â†’ â€œYou can reset again after &lt;time&gt;.â€; RESET_IN_PROGRESS â†’ â€œReset already in progress. Please wait.â€; UNAUTHORIZED â†’ redirect to login; INTERNAL_ERROR â†’ â€œCould not reset portfolio. Try again.â€ with wallet unchanged on refetch.
- Real-time: After reset, user group may receive order-cancelled and balance-update style messages; client refetches even if SignalR missed.

Screen: Virtual cash card / top-bar chip (US-03)
- Arrival after reset: Shows **$100,000.00** available when refetch completes.
- Error: Same as US-03 if wallet refetch fails after successful reset.

Screen: Bottom panel tabs â€” Open Orders, Order History, Trade History, Holdings
- After reset: All reflect empty/zero state within **2 s** of successful POST + refetch.
```

### 4b. API Contract

- **Endpoint(s):**
  - `POST /api/portfolio/reset` (authenticated)
  - Reads for verification: `GET /api/wallet`, `GET /api/portfolio`, `GET /api/orders/open`, `GET /api/orders/history`, `GET /api/trades` (protected, owner-scoped)

- **Reset request:** Empty body or `{}` (no parameters required for MVP).

- **Reset success:** HTTP **200 OK** with body:
```json
{
  "resetAt": "2026-05-25T18:30:00Z",
  "nextEligibleAt": "2026-05-26T18:30:00Z",
  "wallet": {
    "totalBalance": 100000.0000,
    "reservedBalance": 0.0000,
    "availableBalance": 100000.0000,
    "currency": "USD"
  }
}
```
  Alternatively HTTP **204 No Content** if the client must refetch wallet separately â€” `/plan` picks one shape and updates OpenAPI.

- **Errors (RFC 7807):**

| HTTP | `code` | When |
|------|--------|------|
| 401 | `UNAUTHORIZED` | Missing, invalid, or expired session |
| 409 | `RESET_IN_PROGRESS` | Concurrent reset attempt for same user (optional MVP guard) |
| 422 | `RESET_COOLDOWN_ACTIVE` | Last reset within cooldown window; include `nextEligibleAt` in extensions |
| 500 | `INTERNAL_ERROR` | Unexpected failure; no partial reset committed |

- **Auth:** Session cookie required; only the authenticated userâ€™s portfolio may be reset (Tech Â§15.1).

- **Idempotency:** Not idempotent across cooldown window â€” second call within 24h fails with `RESET_COOLDOWN_ACTIVE`. Repeating after cooldown creates a new `portfolio_resets` row. Clients must disable double-submit on confirm.

- **Pagination / filtering:** History endpoints after reset return empty first page; no special query params required for MVP.

## 5. Data & Storage Impact

| Concern | Answer |
|---|---|
| PostgreSQL tables / columns | **Update** `wallets` (total/reserved to initial). **Delete** all `holdings` for userâ€™s portfolio. **Update** `orders` (open â†’ Cancelled, `terminated_at` set). **Insert** `portfolio_resets` (`user_id`, `reset_at`, optional `reason` = `user_initiated`). **User-scoped history:** implement per `/plan` â€” options include soft cutoff by `reset_at` on reads or purging user-visible history rows; must satisfy BR-04 without breaking immutable `trades` FK rules for market integrity (DB Â§5.1 RESTRICT). |
| Redis keys / projections | **Update** order book projection and tape after cancellations; rebuild from PostgreSQL if Redis cleared (standard recovery). No wallet cache in Redis for MVP. |
| Matching / channel behavior | Cancelled orders must be removed from engine book (BR-08). No new matches from userâ€™s pre-reset resting orders after reset completes. |
| Migration needed | **No** â€” `portfolio_resets` and related schema exist (initial migration). |
| Rebuild strategy if Redis cleared | Order book rebuilt from active orders in PostgreSQL; user wallet/portfolio read from PostgreSQL authoritative rows. |

Cross-check: `ix_portfolio_resets_user_time` for cooldown lookup (DB Â§6.9); `ix_orders_user_status` for finding open orders (DB Â§6.5).

## 6. Real-Time & Consistency

- **SignalR events:** On successful reset, emit user-scoped messages for each cancelled open order (same family as user cancel) and a portfolio/wallet refresh signal so clients update cash and holdings. Market group receives order-book delta when user orders leave the book.
- **Read-your-writes:** Successful `POST /api/portfolio/reset` response (or immediate `GET /api/wallet`) reflects **$100,000** / **$0** reserved within **2 s** on local MVP. Open-order and history queries must not return pre-reset user rows after reset commit.
- **Stale UI handling:** Client invalidates user-scoped queries on success; reconnect refetches wallet and open orders. Cooldown state may be read from last reset response `nextEligibleAt` or a lightweight `GET /api/portfolio/reset/status` â€” `/plan` may add status endpoint if needed; otherwise derive from error payload after first blocked attempt.

## 7. Security & Privacy (MVP)

- **Authn / Authz:** Only the session owner may reset their portfolio. No cross-user reset or read of another userâ€™s cooldown.
- **Sensitive fields:** No passwords in reset payloads or logs. Log `userId`, `resetAt`, order cancel count â€” not full order book snapshots.
- **Threat surface:** Rapid reset spam â†’ cooldown (BR-02). CSRF â†’ session cookie + SameSite as login. Replay of reset POST â†’ cooldown and idempotent client guard. Cannot cancel or reset another userâ€™s orders.

## 8. Observability (local MVP)

| Signal | What to emit |
|---|---|
| Structured logs | `PortfolioReset` started/completed with `userId`, duration ms, `openOrdersCancelled`, `holdingsCleared`; failures with reason |
| Traces | Span on `ResetPortfolio` command handler |
| Metrics | Optional counter `portfolio_reset_total` / `portfolio_reset_cooldown_blocked` or `minimal for MVP` |
| Audit | `portfolio_resets` table is the in-app audit trail (DB Â§4.10) |

## 9. Edge Cases

```
EC-01: User on cooldown tries reset â†’ 422 RESET_COOLDOWN_ACTIVE with nextEligibleAt; UI shows wait message (Story 4).

EC-02: User with zero open orders and zero holdings but depleted cash (e.g. $500 total) â†’ reset still restores $100,000 and records cooldown row.

EC-03: User with only filled/cancelled historical orders (no open) â†’ reset clears history views and restores cash; no open-order cancel step needed.

EC-04: Partially filled open order â†’ entire order cancelled; filled portion remains in pre-reset trade history until reset clears user trade view (Story 3).

EC-05: Concurrent place-order and reset â†’ one transaction wins; loser retries or returns 409/422; observable: never negative available cash or holdings (PRD Â§7.3).

EC-06: Optimistic concurrency on wallet/portfolio row during reset â†’ bounded retry then 409 CONFLICT or 500 with no partial state (Tech Â§concurrency).

EC-07: Session expires between opening dialog and confirm â†’ 401; no reset row.

EC-08: Matching engine temporarily behind â†’ user may briefly see cancelled order on book until projection catches up; must converge within 500 ms nominal (PRD Â§7.1).

EC-09: Market order in flight at reset instant â†’ reset transaction cancels open remainder; no fill after reset commit for that order.

EC-10: User resets then immediately places buy â†’ available $100,000 minus new reservation; read-your-writes on wallet after reset refetch.

EC-11: Second tab shows old $37,000 until focus/refetch (Story 5).

EC-12: Global market tape still shows trades the user participated in before reset (other participantsâ€™ view); only **my** history endpoints are empty.
```

## 10. Dependencies, Risks, Decision Triggers

- **Depends on:** US-01 registration (initial $100,000 semantics), US-02 login/session, US-03 wallet display (post-reset refresh). Meaningful E2E also needs order placement/cancel (US-10â€“US-18) and portfolio/history reads (US-20â€“US-22); reset API can be tested with seeded orders before those UIs ship.
- **Impacts:** Wallet display (US-03), open orders UI, trade/order history tabs, order book projections, matching engine cancel path. Virtual-cash story 4 refetch behavior must remain compatible (known plan note EC-08).
- **External services:** PostgreSQL (authoritative), Redis (projections), matching engine channel â€” standard Aspire stack.
- **Key risk:** Aligning PRD â€œtrade history clearedâ€ with immutable `trades` rows and FK constraints â€” requires a deliberate read-model strategy (BR-04, Â§5) so user views are empty without breaking market audit trail.
- **Decision triggers:** If history is implemented via soft cutoff vs hard delete â†’ document in `docs/memory/decisions.md`. If cooldown uses calendar day vs rolling 24h â†’ default rolling 24h per FR-1.4 unless product changes.

## 11. Assumptions

- **Rolling 24 hours** from last `reset_at`, not calendar midnight UTC (matches FR-1.4 wording and DB index purpose).
- **Order history** is cleared from the userâ€™s perspective (empty paginated results), not only trade history, because â€œstart freshâ€ implies no visible past orders in the bottom panel (PRD FR-1.4 implies trade history; UX extends to order history unless corrected).
- **Reset entry point** lives in the authenticated user/account menu on the trading shell (reachable within **2 clicks** from trading view per PRD Â§7.4).
- **Success response** includes wallet snapshot and `nextEligibleAt` (Â§4b) unless `/plan` chooses 204 + refetch-only.
- **Initial capital** after reset matches registration: **100000.0000** USD, not configurable per user in MVP.
- **Simulated liquidity** orders are never cancelled by user reset.

## 12. Out of Scope

- Portfolio reset for other symbols (multi-symbol).
- Admin/operator reset, reset another user, or reset without cooldown override.
- Partial reset (cash only, holdings only, keep history).
- Email or push notification on reset.
- Leaderboards or performance rankings tied to reset.
- Password re-entry before reset (optional enhancement).
- Fractional shares, real money, production deployment.
- Message broker, transactional outbox, horizontal scaling.
- Changing global market trade tape or deleting other usersâ€™ trade records.

**Future scope (Phase 2+ one-liners):** Optional â€œtype RESET to confirmâ€ safeguard; reset reason survey; export history before reset; configurable starting capital for practice modes; admin support reset with audit.

## 13. Open Questions

| # | Question | Source | Answer | Status |
|---|---|---|---|---|
| 1 | How is â€œtrade history clearedâ€ implemented without violating immutable `trades` FKsâ€”soft cutoff on reads vs delete user-linked rows? | PRD FR-1.4 vs DB Â§10.4 | â€” | â“ |
| 2 | Should **order history** be empty after reset, or only trade history? | Assumption Â§11 | Empty both for fresh-start UX | â³ Deferred to user confirm |
| 3 | Should reset return **200 + body** or **204** only? | Â§4b | 200 with wallet + `nextEligibleAt` assumed | â³ Deferred to `/plan` |
| 4 | Dedicated `GET /api/portfolio/reset/eligibility` for disabled UI, or infer only from last success / 422? | Story 4 UI | Infer from 422 + cache last `nextEligibleAt` | â³ Deferred to `/plan` |

