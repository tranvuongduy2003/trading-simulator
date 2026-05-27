---
artifact_type: review
artifact_version: 1
id: review-20260528-180000-account-management
title: Epic review — Account Management
created_at: 2026-05-28T18:00:00Z
epic: Account Management (PRD §5.1)
user_stories: [US-01, US-02, US-03, US-04]
epic_archive: docs/epics/account-management/
sources_deleted: true
verdict: follow-ups
tags: [epic-review, trading-simulator, account-management]
---

# Epic Review: Account Management

| Field | Value |
|-------|--------|
| User stories | US-01, US-02, US-03, US-04 |
| Review date | 2026-05-28 |
| Artifacts | 4 specs, 18 plans → archived under `docs/epics/account-management/` |
| Tests run | Yes — Domain Users: **22 passed**; Api Users + ResetPortfolio: **85 passed** |
| Flags | `--save`, `--run-tests`, `--strict`, `--consolidate` |
| Verdict | 🟡 **Close with follow-ups** |

## Executive summary

Account Management is **functionally complete in code and automated tests**: registration (`POST /api/users`), login/logout/session, wallet display, and full portfolio reset (cooldown, order cancel, history cutoff, client cache invalidation) are implemented with strong API integration coverage (**85** tests in the Users + ResetPortfolio filter).

The epic is **not ready to close administratively** until operator **manual UI checklists** (preserved in archive `plans.md` Part 2) are signed off, specs are promoted from `draft` to `approved`, and open feature branches are merged to `main`. Secondary hygiene: `ResetPortfolioTests.cs` is **1169** lines (exceeds 500-line guideline), reset persistence lives in `PortfolioResetWriteRepository` rather than domain aggregate methods, and `RegisterUserSessionTests` remains outside the Testcontainers CI path.

**Consolidation:** Completed this review — `specs.md` was missing and has been created; all 4 specs + 18 plans merged verbatim and **deleted** from `docs/specs/` and `docs/plans/`.

---

## Artifact inventory

| Path | Type | US | Status | Notes |
|------|------|-----|--------|-------|
| `docs/epics/account-management/specs.md` | Archive (4 specs) | US-01–04 | Merged 2026-05-28 | Part 1 record + Part 2 verbatim |
| `docs/epics/account-management/plans.md` | Archive (18 plans) | US-01–04 | Merged 2026-05-28 | Part 1 index + Part 2 verbatim |
| `docs/epics/account-management/README.md` | Epic index | — | Current | Start here |

**Deleted sources (2026-05-28):** `docs/specs/20260523-175509-user-registration.md`, `20260525-103709-user-login.md`, `20260525-201500-virtual-cash-balance.md`, `20260525-251500-portfolio-reset.md`; all 18 `docs/plans/202605*-*.md` account-management plans.

---

## Completeness matrix (PRD + spec → code)

| US | Priority | Spec/plan | Verdict | Missing / partial items |
|----|----------|-----------|---------|-------------------------|
| **US-01** Register | Must | Registration (archive) | ✅ Shipped | **Manual** register → trading & double-submit UI not operator-signed. Rare concurrent duplicate → **500** (`ISSUE-REG-CONCURRENT-500`). |
| **US-02** Login | Must | Login (archive) | ✅ Shipped | **Manual** session expiry, logout EC-07, cookies-disabled, 2s UX not operator-signed. |
| **US-03** Cash balance | Must | Virtual cash (archive) | ✅ Shipped | **Manual** top bar, reserved breakdown, cross-user cache, refresh paths pending. PRD §8.1 symbol/price in top bar **deferred** (Market Data). |
| **US-04** Portfolio reset | Should → **Must** (`--strict`) | Reset (archive) | ✅ Shipped (API) | **Manual** full reset walkthrough (dialog, cooldown UI, tabs, multi-tab) pending. EC-08 book convergence **500 ms** not load-tested. |

### Per-spec story summary

| Spec story | API / backend | Frontend | Tests |
|------------|---------------|----------|-------|
| Reg 1–4 | ✅ | ✅ | ✅ 18+ integration + 22 domain |
| Login 1–5 | ✅ | ✅ | ✅ Users suite |
| Cash 1–4 | ✅ `GET /api/wallet` | ✅ `useWalletQuery`, cards, chip | ✅ `GetMyWalletTests` (10) |
| Reset 1–5 | ✅ POST + eligibility + writes | ✅ dialog, `useResetPortfolio`, tabs | ✅ `ResetPortfolioTests` (22) |

---

## Synchronization drift

| Area | Expected | Actual | Severity | Fix tag |
|------|----------|--------|----------|---------|
| Spec `status` | `approved` when shipped | All four still **`draft`** in archive Part 2 | Medium | `[hygiene]` |
| `current-status.md` | Epic closed / merged | Many **open PRs** + operator checklists | Medium | `[hygiene]` |
| OpenAPI | Register, auth, wallet, reset | ✅ `contracts/openapi/api.v1.yaml` | — | — |
| Frontend wallet `staleTime` | Story 4: `0` + refetch on focus | ✅ `web/src/features/trading/hooks/use-wallet-query.ts` | — | — |
| `frontend.mdc` example | `staleTime: 30_000` | Code uses **`0`** (ADR-008) | Low | `[sync]` |
| Domain reset | Tech §5 wallet/portfolio on aggregates | Reset in **`PortfolioResetWriteRepository`** | Low | `[deferred]` |
| PRD §8.1 top bar | Symbol, price, change | Cash chip only (ADR-004) | — | `[future-us]` |
| `RegisterUserSessionTests` | CI parity | Local Postgres :5432, not Testcontainers | Medium | `[hygiene]` |
| Epic archive | specs + plans merged | Was missing `specs.md`; **fixed** this review | — | — |

---

## Test coverage matrix

### By layer

| Level | Project | Epic coverage |
|-------|---------|---------------|
| Domain unit | `tests/Domain.UnitTests` | **22** — `Username`, `Email`, `Password`, `RegisterUserCommandValidator`, `User.Register` wallet $100k |
| Matching unit | — | N/A |
| API integration | `tests/Api.IntegrationTests` | **85** — Users + `ResetPortfolioTests` |
| Frontend | — | MVP manual only |

### Per US

| US | Domain | Integration | Manual | Notes |
|----|--------|-------------|--------|-------|
| US-01 | ✅ | `RegisterUserTests`, `RegisterUserValidationTests`, `RegisterUserDuplicateTests`, `RegisterUserTransientFailureTests` | ⏳ | `RegisterUserSessionTests` (1) separate host |
| US-02 | — | `LoginUserTests` (15), `LoginUserValidationTests`, `LogoutUserTests`, `SessionPersistenceTests` | ⏳ | |
| US-03 | ✅ initial cash | `GetMyWalletTests` (10) | ⏳ | |
| US-04 | — | `ResetPortfolioTests` (22) | ⏳ | **1169** lines — split candidate |

### Epic journey scenarios

| Journey | Covered | Evidence |
|---------|---------|----------|
| Register → session → wallet $100k | ✅ | `RegisterUserTests`, wallet after register |
| Register → login → wallet matches user | ✅ | Login + wallet tests |
| Invalid login → 401, no cookie | ✅ | `LoginUserTests`, `LoginUserValidationTests` |
| Login → reload session persists | ✅ | `SessionPersistenceTests` |
| Logout → wallet 401 | ✅ | `LogoutUserTests` |
| User A → User B wallet isolation | ✅ | `GetMyWallet_AfterSecondUserLogin_*` |
| Depleted wallet → reset → $100k + empty holdings | ✅ | Reset wallet/holdings tests |
| Reset → open/history/trades empty | ✅ | Reset + order/trade history tests |
| Reset within cooldown → 422 | ✅ | `ResetPortfolio_WhenCooldownActive_*` |
| Register → orders → reset → fresh panels | ⚠️ Partial | API covered; **full UI manual** pending Story 5 checklist |

---

## Recommendations

### Must fix before epic close (P0–P1)

1. **[P1][epic-gap]** Complete **operator manual UI checklists** in [`docs/epics/account-management/plans.md`](../epics/account-management/plans.md) Part 2 (search “Manual UI checklist”).
2. **[P1][hygiene]** Merge open feature branches to `main` (virtual cash 1–4, portfolio reset 1–5 per `current-status.md`).
3. **[P1][sync]** Promote the four Account Management specs from **`draft` → `approved`** in archive Part 2 after manual sign-off.

### Safe hygiene (P2)

1. **[P2][hygiene]** Split `tests/Api.IntegrationTests/Portfolios/ResetPortfolioTests.cs` (**1169** lines).
2. **[P2][hygiene]** Fold `RegisterUserSessionTests` into Testcontainers or document as local-only in CI.
3. **[P2][sync]** Update `frontend.mdc` wallet example to `staleTime: 0` / `queryKey: ['wallet', userId]`.
4. **[P2][deferred]** Postgres unique violation → **422** for concurrent register (`ISSUE-REG-CONCURRENT-500`).

### Explicitly deferred / future US (do not do now)

- PRD §8.1 full top bar (AAPL symbol, price, change) — Market Data epic
- Live wallet push on every fill — order epics
- Configurable starting capital — PRD v1.1
- Admin/operator reset — spec out of scope

---

## Refactor roadmap

| Priority | Item | Files | Effort | Risk |
|----------|------|-------|--------|------|
| P1 | Manual epic UI sign-off | `docs/epics/account-management/plans.md` | M | Low |
| P1 | Merge PRs / update `current-status` | git, memory | S | Low |
| P2 | Split `ResetPortfolioTests` | `tests/.../ResetPortfolioTests.cs` | M | Low |
| P2 | CI strategy for session tests | `RegisterUserSessionTests.cs`, `ci.yml` | S | Med |
| P3 | Domain reset methods | `Domain/Users`, `Domain/Portfolios` | L | Med |

---

## Epic archive

| File | Role |
|------|------|
| [`docs/epics/account-management/README.md`](../epics/account-management/README.md) | **Start here** |
| [`docs/epics/account-management/specs.md`](../epics/account-management/specs.md) | Part 1 product record + Part 2 verbatim specs (4) |
| [`docs/epics/account-management/plans.md`](../epics/account-management/plans.md) | Part 1 implementation record + Part 2 verbatim plans (18) |

Sources under `docs/specs/` and `docs/plans/` for this epic: **deleted** (2026-05-28).

---

## Suggested next commands

- Operator: manual checklists in [`docs/epics/account-management/plans.md`](../epics/account-management/plans.md) Part 2.
- `yarn --cwd web api:verify` before OpenAPI-touching merges.
- `dotnet test tests/Api.IntegrationTests --filter "FullyQualifiedName~Users|FullyQualifiedName~ResetPortfolio"`.
- Re-run with **`/epic-review Account Management --fix`** for approved doc/test hygiene only.
