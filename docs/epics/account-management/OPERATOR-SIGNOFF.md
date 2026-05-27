---
artifact_type: operator-runbook
title: Account Management — operator manual UI sign-off
epic: Account Management (PRD §5.1)
operator: duyvt
created_at: 2026-05-28T20:45:00+07:00
status: awaiting-signoff
related_plan: docs/plans/20260528-194500-account-management-epic-close.md
related_archive: docs/epics/account-management/plans.md
---

# Account Management — operator manual UI sign-off

| Field | Value |
|-------|--------|
| Epic | US-01–04 (register, login, wallet, portfolio reset) |
| Close plan | [`docs/plans/20260528-194500-account-management-epic-close.md`](../../plans/20260528-194500-account-management-epic-close.md) Task 1 |
| Detail source | [`plans.md`](plans.md) Part 2 — per-story §Manual UI checklist |
| Operator (default) | duyvt |
| Sign-off gate | Task 2 spec promotion — do not mark epic **Closed** until all rows **Pass** |

## Pre-flight

Run on a clean Aspire stack before walking the matrix.

| Step | Action | Expected |
|------|--------|----------|
| P1 | Docker running | Testcontainers / Postgres / Redis healthy |
| P2 | `aspire run` (AppHost) | Api, MatchingEngine, web, Postgres, Redis up |
| P3 | Open web app URL from Aspire dashboard | Vite dev server loads |
| P4 | (Optional) `yarn --cwd web api:verify` | OpenAPI contract in sync if branch touched API |
| P5 | Private window or clear site data | Clean session for auth flows |

**Sign-off columns:** mark **Pass** / **Fail**, record **Date** (YYYY-MM-DD), **Operator** (name or handle).

---

## End-to-end regression smoke

| ID | Step | Expected | Pass | Fail | Date | Operator |
|----|------|----------|:----:|:----:|------|----------|
| E2E-1 | Register new user (`trader_<suffix>` / `SecurePass1!`) | Lands on `/trading`; wallet **$100,000.00** available | | | | |
| E2E-2 | Log out → log in with same credentials | Trading view; same wallet; session cookie set | | | | |
| E2E-3 | (Optional) Deplete wallet via trades/SQL → reset portfolio → confirm | Within ~2s: **$100,000.00**; holdings empty; activity tabs empty | | | | |
| E2E-4 | Log out → `/trading` | Redirect to `/login` (401 / no balances leaked) | | | | |

---

## US-01 — Registration

### Story 1 — Register and access trading (GitHub #5)

| ID | Step | Expected | Pass | Fail | Date | Operator |
|----|------|----------|:----:|:----:|------|----------|
| REG-1-1 | Register new user → **Trading** | Dashboard shows **$100,000.00** virtual cash; **0** AAPL (or empty holdings) | | | | |
| REG-1-2 | While logged in, open `/register` | Redirect to `/trading` (no second registration form) | | | | |

*Archive:* [registration story 1](plans.md) — Source 1 of 18, Task 6–7.

### Story 2 — Reject duplicate username/email (GitHub #6)

| ID | Step | Expected | Pass | Fail | Date | Operator |
|----|------|----------|:----:|:----:|------|----------|
| REG-2-1 | Register `trader_jane` + unique email → success | 201; trading view | | | | |
| REG-2-2 | Register again with same username | Inline **"That username is already in use."**; passwords cleared; email preserved | | | | |
| REG-2-3 | Change username only → submit | New account succeeds | | | | |
| REG-2-4 | Repeat with duplicate email | Email-taken message; no second wallet | | | | |

*Archive:* [registration story 2](plans.md#source-20260524-120000-user-registration-story-2md) — Task 4 manual checklist.

### Story 3 — Client validation polish (GitHub #7)

| ID | Step | Expected | Pass | Fail | Date | Operator |
|----|------|----------|:----:|:----:|------|----------|
| REG-3-1 | Blur empty username | Inline error; fix field → error clears | | | | |
| REG-3-2 | Confirm password mismatch | Submit blocked (client) | | | | |
| REG-3-3 | Submit weak password | Server field errors on password | | | | |
| REG-3-4 | Valid register | Navigates to trading route | | | | |

*Archive:* [registration story 3](plans.md#source-20260525-120000-user-registration-story-3md) — Task 4 Notes.

### Story 4 — Transient failures and double-submit (GitHub #8)

| ID | Step | Expected | Pass | Fail | Date | Operator |
|----|------|----------|:----:|:----:|------|----------|
| REG-4-1 | Single submit → trading | **$100,000.00** shown | | | | |
| REG-4-2 | Double-click **Register** quickly | At most one account / one navigation | | | | |
| REG-4-3 | Simulate **500** or network error (devtools offline / stop Api) | Retry message; can submit again after recovery | | | | |
| REG-4-4 | Register success, then retry same username | Taken message; no second wallet | | | | |
| REG-4-5 | Stories 2–3 validation/duplicate copy still correct | Spot-check one 422 path | | | | |

*Archive:* [registration story 4](plans.md#source-20260525-095103-user-registration-story-4md) — §Manual UI checklist.

---

## US-02 — Login and session

### Story 1 — Log in and access portfolio (GitHub #22)

| ID | Step | Expected | Pass | Fail | Date | Operator |
|----|------|----------|:----:|:----:|------|----------|
| LOG-1-1 | Logged out → open `/login` | Login form | | | | |
| LOG-1-2 | Submit valid credentials | `/trading`; header username; cash matches account | | | | |
| LOG-1-3 | Holdings area | 0 AAPL (or prior trades if applicable) | | | | |
| LOG-1-4 | Logged out → `/trading` | Redirect `/login`; after login → back to `/trading` | | | | |
| LOG-1-5 | Logged in → `/login` | Redirect `/trading` without second form | | | | |

*Archive:* [login story 1](plans.md#source-20260525-150000-user-login-story-1md) — Manual UI checklist (Task 6).

### Story 2 — Invalid credentials (GitHub #23)

| ID | Step | Expected | Pass | Fail | Date | Operator |
|----|------|----------|:----:|:----:|------|----------|
| LOG-2-1 | Wrong password | **"Email or password is incorrect."**; email unchanged; password empty | | | | |
| LOG-2-2 | Unknown email + any password | Same alert text (indistinguishable) | | | | |
| LOG-2-3 | Mixed-case email matching registration | Login succeeds → trading view | | | | |

*Archive:* [login story 2](plans.md#source-20260525-160000-user-login-story-2md) — §Manual UI checklist.

### Story 3 — Session persistence and expiry (GitHub #24)

| ID | Step | Expected | Pass | Fail | Date | Operator |
|----|------|----------|:----:|:----:|------|----------|
| LOG-3-1 | Login → `/trading` | Trading view loads | | | | |
| LOG-3-2 | Hard refresh (F5) | Still authenticated; wallet loads | | | | |
| LOG-3-3 | New tab, same origin → `/trading` | Authenticated without login form | | | | |
| LOG-3-4 | Delete session cookie → refresh | Redirect login; session-expired message | | | | |
| LOG-3-5 | Disable cookies → attempt login | Cookies-required message; stay on login | | | | |
| LOG-3-6 | Log in again (cookies on) | Trading view restored | | | | |

*Archive:* [login story 3](plans.md#source-20260525-170000-user-login-story-3md) — §Manual UI checklist.

### Story 4 — Log out (GitHub #25)

| ID | Step | Expected | Pass | Fail | Date | Operator |
|----|------|----------|:----:|:----:|------|----------|
| LOG-4-1 | Login as user A → `/trading` | Wallet shows user A | | | | |
| LOG-4-2 | User menu → **Log out** | `/login` within ~2s | | | | |
| LOG-4-3 | Navigate `/trading` | Redirect login | | | | |
| LOG-4-4 | Login as user B | Wallet shows B (not A) | | | | |
| LOG-4-5 | Log out → browser **Back** | Must not show A’s authenticated balances (EC-07) | | | | |
| LOG-4-6 | `POST /api/auth/logout` without cookie (devtools) | **401** | | | | |

*Archive:* [login story 4](plans.md#source-20260525-180000-user-login-story-4md) — §Manual UI checklist (operator).

### Story 5 — Login validation and transient failures (GitHub #26)

| ID | Step | Expected | Pass | Fail | Date | Operator |
|----|------|----------|:----:|:----:|------|----------|
| LOG-5-1 | `not-an-email` + password → submit | Inline email error; no navigation; Network **422** | | | | |
| LOG-5-2 | Valid email + empty password | Password error; **422** | | | | |
| LOG-5-3 | Valid credentials once | Trading view; single login **200** | | | | |
| LOG-5-4 | Rapid double-click **Log in** | At most one login request while pending | | | | |
| LOG-5-5 | Offline / blocked request | Generic retry message; button re-enables; retry succeeds | | | | |
| LOG-5-6 | (EC-09) Slow 3G: apparent fail but cookie set | Reload/retry → authenticated, wallet loads | | | | |

*Archive:* [login story 5](plans.md#source-20260525-190000-user-login-story-5md) — Manual UI checklist (Task 5).

---

## US-03 — Virtual cash balance

### Story 1 — See available cash (GitHub #34)

| ID | Step | Expected | Pass | Fail | Date | Operator |
|----|------|----------|:----:|:----:|------|----------|
| CASH-1-1 | New user → **Trading** | Card + top bar **$100,000.00** available; reserved **$0.00** | | | | |
| CASH-1-2 | Open **Portfolio** / **Orders** routes | Top bar same available amount | | | | |
| CASH-1-3 | Refresh with valid session | Balances unchanged within ~2s | | | | |
| CASH-1-4 | Network throttle | Skeleton in cash card/chip only; page usable | | | | |
| CASH-1-5 | (Optional) Break wallet API | Error copy; top bar **Unavailable** (no fake balance) | | | | |

*Archive:* [virtual cash story 1](plans.md#source-20260525-203000-virtual-cash-story-1md).

### Story 2 — Reserved breakdown (GitHub #35)

| ID | Step | Expected | Pass | Fail | Date | Operator |
|----|------|----------|:----:|:----:|------|----------|
| CASH-2-1 | User with $50k total, $10k reserved (SQL seed) | **$40,000.00** available; secondary total/reserved line | | | | |
| CASH-2-2 | New user / reserved 0 | Reserved **$0.00** + clear helper copy | | | | |
| CASH-2-3 | Top bar | Available only (no total/reserved in chip) | | | | |
| CASH-2-4 | Wallet **500** regression | Error copy; no dollar amounts (Story 1) | | | | |

*Archive:* [virtual cash story 2](plans.md#source-20260525-220000-virtual-cash-story-2md) — SQL seed example in plan.

### Story 3 — Session-private wallet (GitHub #36)

| ID | Step | Expected | Pass | Fail | Date | Operator |
|----|------|----------|:----:|:----:|------|----------|
| CASH-3-1 | Login A → note balance → logout → login B | B’s balance (typically **$100k**), never A’s | | | | |
| CASH-3-2 | Revoked session → refresh / wallet fetch | Redirect login; session-expired; **no** amounts during transition | | | | |
| CASH-3-3 | Logged out → `/trading` | Redirect login; `GET /api/wallet` → **401** | | | | |
| CASH-3-4 | Regression | Story 1 skeleton/error; Story 2 breakdown intact | | | | |

*Archive:* [virtual cash story 3](plans.md#source-20260525-230000-virtual-cash-story-3md).

### Story 4 — Trust after login and refresh (GitHub #37)

| ID | Step | Expected | Pass | Fail | Date | Operator |
|----|------|----------|:----:|:----:|------|----------|
| CASH-4-1 | Login → trading | Correct available within ~2s | | | | |
| CASH-4-2 | Hard refresh on `/trading` | Same balances after load | | | | |
| CASH-4-3 | Update `reserved_balance` in DB → focus tab | UI matches `GET /api/wallet` | | | | |
| CASH-4-4 | Stale tab: change wallet in DB, focus after ~1 min | Balances update (not stuck at $100k) | | | | |
| CASH-4-5 | **500** regression | Error copy; no amounts | | | | |
| CASH-4-6 | Logout/login different users | No cross-user balances | | | | |

*Archive:* [virtual cash story 4](plans.md#source-20260525-240000-virtual-cash-story-4md).

---

## US-04 — Portfolio reset

### Story 1 — Confirm before reset (GitHub #44)

| ID | Step | Expected | Pass | Fail | Date | Operator |
|----|------|----------|:----:|:----:|------|----------|
| RST-1-1 | User menu → **Reset portfolio** visible on **Trading** | Menu item present | | | | |
| RST-1-2 | Open dialog → read consequences → **Cancel** | No `POST /api/portfolio/reset` in network | | | | |
| RST-1-3 | Open → **Confirm** | Loading → **200** → success toast; dialog closes | | | | |
| RST-1-4 | After success | Menu disabled; cooldown hint | | | | |
| RST-1-5 | Another user | Reset menu enabled (no leaked eligibility) | | | | |
| RST-1-6 | Regression | Top-bar chip + virtual cash card still load | | | | |

*Archive:* [portfolio reset story 1](plans.md#source-20260525-260000-portfolio-reset-story-1md).

### Story 2 — Restore cash and holdings (GitHub #45)

| ID | Step | Expected | Pass | Fail | Date | Operator |
|----|------|----------|:----:|:----:|------|----------|
| RST-2-1 | Seed ~$42k wallet, $5k reserved, AAPL holding | Pre-reset state visible | | | | |
| RST-2-2 | Reset portfolio → confirm | **$100,000.00** available within ~2s | | | | |
| RST-2-3 | Holdings tab | Empty | | | | |
| RST-2-4 | Open orders tab | Empty (post Story 3); note if legacy data | | | | |

*Archive:* [portfolio reset story 2](plans.md#source-20260527-210000-portfolio-reset-story-2md).

### Story 3 — Cancel orders and clear history (GitHub #46)

| ID | Step | Expected | Pass | Fail | Date | Operator |
|----|------|----------|:----:|:----:|------|----------|
| RST-3-1 | User with open orders + history → reset | Open Orders panel clears; no ghost liquidity | | | | |
| RST-3-2 | **Open Orders** tab | Empty after reset | | | | |
| RST-3-3 | **Order History** tab | Empty first page | | | | |
| RST-3-4 | **Trade History** tab | Empty first page | | | | |
| RST-3-5 | **Holdings** tab | 0 AAPL | | | | |

*Archive:* [portfolio reset story 3](plans.md#source-20260527-214600-portfolio-reset-story-3md) — Task 5 Aspire tabs.

### Story 4 — 24-hour cooldown (GitHub #47)

| ID | Step | Expected | Pass | Fail | Date | Operator |
|----|------|----------|:----:|:----:|------|----------|
| RST-4-1 | After successful reset | Menu disabled + relative-time hint; no second POST | | | | |
| RST-4-2 | Dialog only when menu enabled | Cancel unchanged | | | | |
| RST-4-3 | Force POST during cooldown (devtools) | **You can reset again in …**; wallet unchanged on refetch | | | | |
| RST-4-4 | Refresh while cooldown | Menu stays disabled after eligibility GET | | | | |
| RST-4-5 | Second tab focus | Disabled state matches server | | | | |

*Archive:* [portfolio reset story 4](plans.md#source-20260527-231500-portfolio-reset-story-4md).

### Story 5 — Consistent data after reset (GitHub #48)

| ID | Step | Expected | Pass | Fail | Date | Operator |
|----|------|----------|:----:|:----:|------|----------|
| RST-5-1 | User with depleted cash, holdings, open orders, history → reset | Chip + card **$100,000.00** within ~2s | | | | |
| RST-5-2 | Open Orders / Order History / Trade History / Holdings | All empty | | | | |
| RST-5-3 | (Optional) Break `GET /api/wallet` | Error state; no hardcoded $100k in JSX | | | | |
| RST-5-4 | Two tabs: reset in A → focus B | Post-reset data after refetch | | | | |
| RST-5-5 | DevTools SignalR connected | `BalanceUpdated` / `OrderCancellationNotified` during reset | | | | |

*Archive:* [portfolio reset story 5](plans.md#source-20260528-003204-portfolio-reset-story-5md) — Manual UI checklist (Aspire).

---

## Sign-off summary

| Section | Rows | Pass | Fail | Signed date | Operator |
|---------|------|------|------|-------------|----------|
| Pre-flight (P1–P5) | 5 | | | | |
| E2E smoke (E2E-1–4) | 4 | | | | |
| US-01 Registration | 17 | | | | |
| US-02 Login | 26 | | | | |
| US-03 Virtual cash | 19 | | | | |
| US-04 Portfolio reset | 24 | | | | |
| **Total** | **95** | | | | |

**Epic manual sign-off complete:** [ ] Yes — all rows Pass — **Date:** ______ — **Operator:** ______

When complete, proceed to close plan **Task 2** (merge hygiene + promote archive specs to `approved`).
