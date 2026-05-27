---
artifact_type: plan
artifact_version: 1
id: plan-20260527-231500-portfolio-reset-story-4
title: Portfolio Reset - Story 4 (Respect the 24-hour cooldown)
slug: portfolio-reset-story-4
filename_template: 20260527-231500-portfolio-reset-story-4.md
created_at: 2026-05-27T23:15:00+07:00
updated_at: 2026-05-28T12:00:00+07:00
status: complete
owner: engineering
tags: [plan, implementation, trading-simulator, portfolio-reset, cooldown, us-04, story-4]
related_spec: docs/specs/20260525-251500-portfolio-reset.md
related_plans: [docs/plans/20260525-260000-portfolio-reset-story-1.md, docs/plans/20260527-210000-portfolio-reset-story-2.md, docs/plans/20260527-214600-portfolio-reset-story-3.md]
prd_refs: [PRD SS6.1 FR-1.4, PRD SS7.4, PRD SS8.1]
tech_refs: [Tech SS6 CQRS Design, Tech SS8.1 Portfolio endpoints, Tech SS13.2 Trading:PortfolioResetCooldownMinutes, Tech SS16 Error handling, Tech SS17.3 API integration tests]
db_refs: [DB SS4.10 portfolio_resets, DB SS6.9 ix_portfolio_resets_user_time, DB SS10.4 Portfolio Reset]
github:
  repo: tranvuongduy2003/trading-simulator
  epic_issue: null
  story_issue_ids: []
  last_synced_at: null
search_index:
  keywords: [portfolio reset, cooldown, reset cooldown active, nextEligibleAt, 24 hours, eligibility endpoint, reset status, us-04 story-4]
  bounded_contexts: [Trading]
  task_count: 5
---

# Implementation Plan: Portfolio Reset - Story 4

| Field | Value |
|-------|--------|
| Spec | `docs/specs/20260525-251500-portfolio-reset.md` |
| Status | COMPLETE |
| Tasks | 5 |
| Branch | `feature/portfolio-reset-story-4` |
| Aspire impact | No topology change |
| Schema impact | No migration |
| Test levels | Api.IntegrationTests + manual UI |
| ADRs required | None |
| GitHub | Not synced by `/plan` (spec is draft; existing story issue is referenced in GitHub Links) |

## Executive summary

Story 4 adds server-authoritative cooldown enforcement for portfolio reset using `portfolio_resets` as the source of truth and `Trading:PortfolioResetCooldownMinutes` as the policy value. The API will reject early reset attempts with HTTP 422 and stable `RESET_COOLDOWN_ACTIVE`, including `nextEligibleAt` so the UI can render a disabled reset affordance with clear relative-time messaging. To satisfy "disabled without calling reset API", this plan introduces a lightweight eligibility read endpoint and client hook, while preserving Story 1-3 reset behavior and error contracts. Completion means cooldown is enforced end-to-end, tested through integration scenarios, and reflected in user-menu UX without regressions to wallet/order/history behavior already delivered.

## Goals and non-goals

**Goals**
- G1: Enforce rolling 24-hour cooldown on `POST /api/portfolio/reset` from latest successful reset.
- G2: Return 422 `RESET_COOLDOWN_ACTIVE` with `nextEligibleAt` and leave wallet/holdings/orders unchanged.
- G3: Add eligibility read path for UI (`nextEligibleAt`, `isEligible`) so menu can be disabled before reset click.
- G4: Map cooldown API errors in frontend reset flow and update disabled hint reliably across refresh/login.

**Non-goals**
- NG1: Changing Story 2-3 transactional reset semantics or cancellation/history logic.
- NG2: Adding new schema objects or changing `portfolio_resets` structure.
- NG3: Modifying global market tape/candlestick behaviors.
- NG4: Introducing cross-service infrastructure changes (AppHost, broker, outbox).

## Traceability matrix

| Spec story | Plan task(s) | Test evidence |
|------------|--------------|---------------|
| No prior reset -> success and new cooldown window | Task 1, 2 | `ResetPortfolio_FirstReset_SucceedsAndReturnsNextEligibleAt` |
| Last reset 25h ago -> success | Task 2, 4 | `ResetPortfolio_AfterTwentyFiveHours_SucceedsAndAppendsResetRow` |
| Last reset 2h ago -> 422 + nextEligibleAt unchanged state | Task 2, 4 | `ResetPortfolio_WhenCooldownActive_Returns422_WithNextEligibleAt`, `ResetPortfolio_WhenCooldownActive_DoesNotMutateState` |
| Cooldown active -> UI disabled without reset POST | Task 3, 5 | menu eligibility query/manual UI checklist |

## Architecture impact

```text
web UserMenu
  -> GET /api/portfolio/reset/eligibility
  -> POST /api/portfolio/reset
          |
Api Endpoints (PortfolioEndpoint)
          |
Application
  ResetPortfolioCommandHandler + eligibility query
          |
Infrastructure
  portfolio_resets latest-reset lookup (PostgreSQL index ix_portfolio_resets_user_time)
```

| Layer | Change summary |
|-------|----------------|
| Domain | No aggregate rule changes; cooldown remains application policy |
| Application | Add cooldown checker abstraction + eligibility query; map cooldown error to validation result |
| Infrastructure | Add/read method for latest reset timestamp lookup by user |
| Api | Add eligibility endpoint and 422 response contract metadata |
| MatchingEngine | No change |
| web/ | Add eligibility query hook and reuse existing reset-disable UX path |
| AppHost | None |

## Data and migration plan

| Artifact | Action | DB reference |
|----------|--------|--------------|
| EF migration | None | DB SS4.10 already present |
| PostgreSQL query | Read latest `portfolio_resets.reset_at` by user (DESC limit 1) | DB SS6.9 |
| Redis keys | None | N/A |
| Book recovery | N/A | Tech SS7.7 |

## Open questions

| # | Question | Source | Answer | Status |
|---|----------|--------|--------|--------|
| 1 | Should cooldown block be 422 or 429? | Spec Story 4 | Use 422 to match current RFC7807 validation mapping | ✅ |
| 2 | Add eligibility endpoint or rely on cached `nextEligibleAt` only? | Spec Story 4 UI | Add endpoint to satisfy disabled-on-open without reset POST | ✅ |
| 3 | Should eligibility endpoint include server current time? | UI timing precision | Deferred; initial response only needs `isEligible` + `nextEligibleAt` | ⏳ |

## Risks and mitigations

| Risk | Likelihood | Impact | Mitigation | Owner task |
|------|------------|--------|------------|------------|
| Cooldown race around boundary timestamp | M | M | Compare against clock consistently in handler; integration tests for boundary windows | Task 2, 4 |
| UI displays stale eligibility from sessionStorage | M | M | Prefer server eligibility query; treat storage as fallback only | Task 3 |
| Missing 422 mapping in frontend causes generic error copy | M | L | Extend reset error mapper with `RESET_COOLDOWN_ACTIVE` and computed hint | Task 3 |
| Regression in existing Story 1-3 reset success path | L | H | Keep focused regression tests for successful reset + wallet contract | Task 4 |

## Prerequisites

- [ ] Story 3 branch baseline merged or available for cherry-pick
- [ ] Existing reset endpoint contract and tests green
- [ ] Testcontainers integration test environment available
- [ ] No pending schema migrations required

## File structure (planned)

```text
src/
  TradingSimulator.Application/
    Abstractions/Persistence/
      IPortfolioResetReadRepository.cs           CREATE
    Portfolios/Queries/
      GetPortfolioResetEligibilityQuery.cs       CREATE
      GetPortfolioResetEligibilityQueryHandler.cs CREATE
    Portfolios/Commands/
      ResetPortfolioCommandHandler.cs            MODIFY
      PortfolioResetErrors.cs                    MODIFY
  TradingSimulator.Infrastructure/
    Persistence/Repositories/
      PortfolioResetReadRepository.cs            CREATE
    DependencyInjection.cs                       MODIFY
  TradingSimulator.Contracts/
    Portfolio/
      PortfolioResetEligibilityResponse.cs       CREATE
  TradingSimulator.Api/
    Endpoints/
      PortfolioEndpoint.cs                       MODIFY
tests/
  Api.IntegrationTests/
    Portfolios/
      ResetPortfolioTests.cs                     MODIFY
web/
  src/features/portfolio-reset/
    api.ts                                       MODIFY
    reset-eligibility.ts                         MODIFY
    map-reset-error.ts                           MODIFY
    use-reset-portfolio.ts                       MODIFY
  src/layouts/app-layout.tsx                     MODIFY
```

## Authorization, session, and domain notes

- **Session model:** authenticated cookie session required for reset and eligibility checks.
- **Route protection:** both reset POST and eligibility GET require authorization (401 on missing session).
- **Domain rules preserved:** BR-02 rolling cooldown, BR-03 reset semantics, BR-06 atomic reset transaction.

## Progress tracker

### Task 1: Add cooldown read abstraction and eligibility query skeleton

| Attribute | Value |
|-----------|--------|
| Spec story | Story 4 |
| Depends on | None |
| Estimated complexity | M |
| Parent story issue | #47 (existing) |

#### Objective

Introduce a small read-side slice that returns cooldown eligibility for the current user from authoritative reset history, enabling both API handler reuse and UI preflight state.

#### Implementation notes

- Add `IPortfolioResetReadRepository` with latest-reset lookup.
- Add `GetPortfolioResetEligibilityQuery`/handler returning `isEligible`, `nextEligibleAt`.
- Use `IClock` and `TradingOptions.PortfolioResetCooldownMinutes` for deterministic calculations.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `src/Application/Abstractions/Persistence/IPortfolioResetReadRepository.cs` | Read port |
| CREATE | `src/Application/Portfolios/Queries/GetPortfolioResetEligibilityQuery.cs` | Query request |
| CREATE | `src/Application/Portfolios/Queries/GetPortfolioResetEligibilityQueryHandler.cs` | Eligibility logic |
| CREATE | `src/Infrastructure/Persistence/Repositories/PortfolioResetReadRepository.cs` | PG latest reset read |
| MODIFY | `src/Infrastructure/DependencyInjection.cs` | Register read repository |
| CREATE | `src/Contracts/Portfolio/PortfolioResetEligibilityResponse.cs` | Response DTO |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `GetResetEligibility_WithoutPriorReset_IsEligible` | `tests/Api.IntegrationTests/Portfolios/ResetPortfolioTests.cs` |
| Integration | `GetResetEligibility_WithRecentReset_ReturnsNextEligibleAt` | `tests/Api.IntegrationTests/Portfolios/ResetPortfolioTests.cs` |

#### Acceptance criteria

- [x] Eligibility query returns deterministic values for no-reset and recent-reset users.
- [x] Cooldown calculation uses configured minutes (default 1440).
- [x] No layering violation (Application only depends on abstraction).

#### Deviations (Task 1)

- Added `GET /api/portfolio/reset/eligibility` in Task 1 (planned for Task 3) so cookie-authenticated integration tests can exercise the query without MediatR-only setup.
- OpenAPI path/schema added in Task 1 (`contracts/openapi/api.v1.yaml`); run `yarn --cwd web api:codegen` before frontend work in Task 3.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | PRD FR-1.4, Tech SS13.2, DB SS6.9 |
| PostgreSQL authoritative | Yes |
| RFC 7807 errors | N/A for query success path |
| Aspire | None |
| ADR needed? | No |

#### Risk

Low; isolated read-side addition.

### Task 2: Enforce cooldown in reset command handler with 422 error

| Attribute | Value |
|-----------|--------|
| Spec story | Story 4 |
| Depends on | Task 1 |
| Estimated complexity | M |
| Parent story issue | #47 (existing) |

#### Objective

Block reset requests inside active cooldown window while preserving existing successful-reset transaction behavior and returning a stable cooldown error payload.

#### Implementation notes

- Inject cooldown read abstraction into `ResetPortfolioCommandHandler`.
- On active cooldown: return validation error `RESET_COOLDOWN_ACTIVE` with `nextEligibleAt` in message detail.
- Keep in-flight guard semantics and existing success payload unchanged.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `src/Application/Portfolios/Commands/ResetPortfolioCommandHandler.cs` | Cooldown guard before reset write |
| MODIFY | `src/Application/Portfolios/Commands/PortfolioResetErrors.cs` | Add cooldown error builder |
| MODIFY | `src/Api/Endpoints/PortfolioEndpoint.cs` | Document 422 for reset (Task 2) |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `ResetPortfolio_WhenCooldownActive_Returns422_WithNextEligibleAt` | `tests/Api.IntegrationTests/Portfolios/ResetPortfolioTests.cs` |
| Integration | `ResetPortfolio_AfterCooldownWindow_Succeeds` | `tests/Api.IntegrationTests/Portfolios/ResetPortfolioTests.cs` |

#### Acceptance criteria

- [x] Reset within cooldown returns 422 with code `RESET_COOLDOWN_ACTIVE`.
- [x] Error detail includes parseable `nextEligibleAt` timestamp.
- [x] No wallet/order/holding mutation occurs on cooldown rejection.

#### Deviations (Task 2)

- Added optional `Error.Extensions` on application errors and mapped them to RFC 7807 `extensions` (includes `nextEligibleAt` ISO-8601 for `RESET_COOLDOWN_ACTIVE`).

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | PRD Story 4 failure path, Tech SS16, DB SS4.10 |
| Async matching | unchanged |
| PostgreSQL authoritative | yes |
| RFC 7807 errors | 422 via `ErrorType.Validation` |
| ADR needed? | No |

#### Risk

Medium: details extension shape may require small Result/Problem mapping changes if currently unavailable.

### Task 3: Add eligibility endpoint and frontend integration

| Attribute | Value |
|-----------|--------|
| Spec story | Story 4 |
| Depends on | Task 1 |
| Estimated complexity | M |
| Parent story issue | #47 (existing) |

#### Objective

Make reset menu state server-driven so users can see disabled/reset-ready state without triggering a reset POST.

#### Implementation notes

- Add `GET /api/portfolio/reset/eligibility` endpoint (authorized).
- Update frontend API/hook to fetch eligibility in user menu.
- Keep `sessionStorage` helper as fallback cache, but prioritize server values.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `src/Api/Endpoints/PortfolioEndpoint.cs` | Add eligibility route |
| MODIFY | `web/src/features/portfolio-reset/api.ts` | Add eligibility API call |
| MODIFY | `web/src/features/portfolio-reset/reset-eligibility.ts` | Integrate query-backed eligibility |
| MODIFY | `web/src/layouts/app-layout.tsx` | Drive menu disabled state from eligibility |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `GetResetEligibility_RequiresAuthentication` | `tests/Api.IntegrationTests/Portfolios/ResetPortfolioTests.cs` |
| Manual | User menu shows disabled hint while cooldown active before reset click | `web/` checklist |

#### Acceptance criteria

- [x] Eligibility endpoint returns `isEligible` + `nextEligibleAt`.
- [x] User menu disables reset action and shows next-available hint when cooldown active.
- [x] UI does not call reset POST when disabled.

#### Deviations (Task 3)

- Eligibility route added in Task 1; Task 3 scoped to frontend query + invalidation only.
- `app-layout.tsx` unchanged — existing `useResetEligibility` hook now server-backed (menu `disabled` / `title` unchanged).

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | PRD Story 4 UI path, Tech SS8.1 |
| SignalR | N/A |
| Aspire | None |
| ADR needed? | No |

#### Risk

Medium: balancing server query + local cached hints without flicker.

### Task 4: Integration regression and edge-path hardening

| Attribute | Value |
|-----------|--------|
| Spec story | Story 4 |
| Depends on | Task 2, 3 |
| Estimated complexity | M |
| Parent story issue | #47 (existing) |

#### Objective

Prove Story 4 acceptance and protect existing Story 1-3 reset guarantees through focused integration coverage.

#### Implementation notes

- Add first-reset success and cooldown-blocked cases.
- Verify cooldown uses latest reset row and allows reset after window.
- Assert no side effects when blocked by cooldown.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `tests/Api.IntegrationTests/Portfolios/ResetPortfolioTests.cs` | Story 4 scenarios |
| REUSE | `tests/Api.IntegrationTests/Portfolios/PortfolioResetTestHelpers.cs` | Seed reset timestamps |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `ResetPortfolio_FirstReset_SucceedsAndReturnsNextEligibleAt` | `ResetPortfolioTests.cs` |
| Integration | `ResetPortfolio_WhenCooldownActive_DoesNotMutateState` | `ResetPortfolioTests.cs` |
| Integration | `ResetPortfolio_AfterTwentyFiveHours_SucceedsAndAppendsResetRow` | `ResetPortfolioTests.cs` |

#### Acceptance criteria

- [x] Story 4 happy/failure paths pass in integration tests.
- [x] Existing reset success tests remain green.
- [x] OpenAPI-documented response codes match runtime behavior.

#### Deviations (Task 4)

- Renamed `ResetPortfolio_AfterCooldownWindow_Succeeds` → `ResetPortfolio_AfterTwentyFiveHours_SucceedsAndAppendsResetRow` (plan naming).
- Added `ResetPortfolio_WhenCooldownActive_UsesLatestResetRow` to prove cooldown reads the latest `portfolio_resets` row (older expired row does not bypass guard).

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| Tech SS17 | Testcontainers integration coverage |
| DB SS6.9 | Uses latest reset index access path |
| RFC 7807 | Code and status stable |
| ADR needed? | No |

#### Risk

Low if existing reset test helpers are reused.

### Task 5: Polish and manual UX verification

| Attribute | Value |
|-----------|--------|
| Spec story | Polish |
| Depends on | Task 3, 4 |
| Estimated complexity | S |
| Parent story issue | #47 (existing) |

#### Objective

Finalize user-facing cooldown messaging and complete manual validation checklist across refresh/tab scenarios.

#### Implementation notes

- Extend reset error mapping for cooldown response details.
- Confirm disabled copy remains understandable (`nextEligibleAt` relative time).
- Validate behavior on page refresh and second tab focus.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `web/src/features/portfolio-reset/map-reset-error.ts` | Cooldown-specific error mapping |
| MODIFY | `web/src/features/portfolio-reset/use-reset-portfolio.ts` | Align toast/error behavior |
| MODIFY | `docs/memory/current-status.md` | Mark plan completion and next step |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Cooldown active message + disabled menu in same tab | `web/` |
| Manual | Refresh/another tab reflects eligibility on next fetch | `web/` |

#### Acceptance criteria

- [x] Error copy for cooldown is explicit and actionable.
- [x] Manual checklist confirms disabled behavior without reset POST (operator handoff below).
- [x] Story 4 automation complete on `feature/portfolio-reset-story-4`.

#### Manual UI checklist (operator)

1. Sign in, perform one successful reset → menu **Reset portfolio** disabled with relative-time hint; no second POST while disabled.
2. Open confirm dialog only when menu item is enabled; cancel leaves state unchanged.
3. If eligible, force early POST (e.g. devtools) while cooldown active → dialog shows **You can reset again in …**; wallet unchanged on refetch.
4. Refresh page while cooldown active → menu stays disabled after eligibility GET (no reset POST).
5. Second tab: focus window → eligibility refetch; disabled state matches server.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD SS7.4 | Clear actionable message |
| frontend.mdc | Query-driven server state |
| design-system.mdc | Accessible disabled state and labels |
| ADR needed? | No |

#### Risk

None - polish only.

## Reference files

| File | Why open it |
|------|-------------|
| `src/Application/Portfolios/Commands/ResetPortfolioCommandHandler.cs` | Current reset orchestration point |
| `src/Infrastructure/Persistence/Repositories/PortfolioResetWriteRepository.cs` | Current reset transaction behavior |
| `web/src/features/portfolio-reset/reset-eligibility.ts` | Existing client-side cooldown cache logic |
| `web/src/layouts/app-layout.tsx` | User-menu disable wiring |
| `tests/Api.IntegrationTests/Portfolios/ResetPortfolioTests.cs` | Existing Story 1-3 reset tests |
| `docs/specs/20260525-251500-portfolio-reset.md` | Story 4 acceptance criteria |

## Implementation details (for /build)

- Cooldown formula: `nextEligibleAt = latestResetAt + PortfolioResetCooldownMinutes`.
- `isEligible = latestResetAt is null || clock.UtcNow >= nextEligibleAt`.
- For blocked reset, return `Error.Validation("RESET_COOLDOWN_ACTIVE", "...nextEligibleAt...")`; if structured extension support is needed, add a minimal convention in result/problem mapping.
- Eligibility endpoint returns a small DTO: `{ isEligible, nextEligibleAt }` where `nextEligibleAt` can be null for users with no prior reset.
- Keep existing in-flight guard behavior (`RESET_IN_PROGRESS`) independent from cooldown logic.

## Verification matrix (plan-level)

| Spec AC | Verified by |
|---------|-------------|
| First reset records row and sets 24h eligibility | Task 2 + Task 4 tests |
| Reset after 25h succeeds | Task 2 + Task 4 tests |
| Reset at 2h blocked with 422 and `nextEligibleAt` | Task 2 + Task 4 tests |
| UI shows disabled/reset-available timing without reset POST | Task 3 + Task 5 manual checks |

## Rollback / recovery

- **Code:** revert Story 4 commits.
- **DB:** no migration rollback required.
- **Redis:** not affected by cooldown feature directly.

## Deferred work (Plan B)

- Add explicit server `now` in eligibility response for tighter client relative-time precision.
- Add telemetry counter for `RESET_COOLDOWN_ACTIVE` blocks.

## GitHub Links

| Local ref | Issue # | Type | Title | URL |
|-----------|---------|------|-------|-----|
| `spec.Story 4` | `#47` | Story | US-04 / Story 4: Respect the 24-hour cooldown | [#47](https://github.com/tranvuongduy2003/trading-simulator/issues/47) |

