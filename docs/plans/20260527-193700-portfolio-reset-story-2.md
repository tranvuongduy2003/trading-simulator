---
artifact_type: plan
artifact_version: 1
id: plan-20260527-193700-portfolio-reset-story-2
title: Portfolio Reset - Story 2 (Restore starting cash and empty holdings)
slug: portfolio-reset-story-2
filename_template: 20260527-193700-portfolio-reset-story-2.md
created_at: 2026-05-27T19:37:00+07:00
updated_at: 2026-05-27T19:37:00+07:00
status: draft
owner: engineering
tags: [plan, implementation, trading-simulator, portfolio-reset, wallet, holdings, us-04, story-2]
related_spec: docs/specs/20260525-251500-portfolio-reset.md
related_plans: [docs/plans/20260525-260000-portfolio-reset-story-1.md]
prd_refs: [PRD §5.1 US-04, PRD §6.1 FR-1.4, PRD §7.3]
tech_refs: [Tech §5.2.1 Wallet, Tech §5.2.2 Portfolio, Tech §6 ResetPortfolioCommand, Tech §10.4]
db_refs: [DB §4.2 wallets, DB §4.4 holdings, DB §4.10 portfolio_resets, DB §10.4 Portfolio Reset]
github:
  repo: tranvuongduy2003/trading-simulator
  epic_issue: null
  story_issue_ids: [45]
  last_synced_at: null
search_index:
  keywords: [portfolio reset, story 2, wallet reset, holdings clear, atomic transaction, rollback, unauthorized, reset portfolio command]
  bounded_contexts: [Trading]
  task_count: 5
---

# Implementation Plan: Portfolio Reset - Story 2

| Field | Value |
|-------|--------|
| Spec | `docs/specs/20260525-251500-portfolio-reset.md` (§2 Story 2) |
| Status | DRAFT |
| Tasks | 5 |
| Branch | `feature/portfolio-reset-story-2` |
| Aspire impact | No |
| Schema impact | No |
| Test levels | Domain, API integration, Manual UI |
| ADRs required | None (reuse ADR-005 scope boundary) |
| GitHub | not synced (spec is draft; Story issue already exists) |

## Executive summary

This plan upgrades the existing Story 1 reset stub into a real Story 2 atomic reset path for wallet and holdings only. The reset command will set wallet balances to initial cash and remove all holdings in one PostgreSQL transaction so partial state cannot leak on failure. Integration coverage will prove the happy path values and rollback behavior, while preserving existing unauthorized behavior. Open-order cancellation, history clearing, and cooldown enforcement remain explicitly deferred to Stories 3 and 4.

## Goals and non-goals

**Goals**
- G1: Replace Story 1 stub logic in `ResetPortfolioCommandHandler` with real transactional mutation for wallet and holdings.
- G2: Ensure post-reset `GET /api/wallet` returns `100000.0000 / 0.0000 / 100000.0000`.
- G3: Ensure post-reset `GET /api/portfolio` returns no holdings.
- G4: Guarantee atomic rollback: on failure, both wallet and holdings remain pre-reset authoritative values.
- G5: Keep unauthenticated `POST /api/portfolio/reset` at **401** with no inserted reset row.

**Non-goals**
- NG1: Cancelling open orders and releasing reservations tied to open orders (Story 3).
- NG2: Clearing order history and trade history views (Story 3).
- NG3: Server-enforced cooldown (`RESET_COOLDOWN_ACTIVE`) and eligibility endpoint (Story 4).
- NG4: Matching-engine order-book cleanup and SignalR fanout for reset (Story 3/5).
- NG5: New schema or migration work (tables already exist).

## Traceability matrix

| Spec story | Plan task(s) | Test evidence |
|------------|--------------|---------------|
| Story 2 happy path: wallet restored to $100k | Task 2, 3 | `ResetPortfolio_AfterSuccess_GetWalletReturnsResetSnapshot` |
| Story 2 happy path: holdings emptied | Task 2, 3 | `ResetPortfolio_AfterSuccess_GetPortfolioReturnsEmptyHoldings` |
| Story 2 failure path: no partial reset state | Task 4 | `ResetPortfolio_WhenMutationFails_RollsBackWalletAndHoldings` |
| Story 2 unauthorized path: 401 and no reset row | Task 1, 5 | `ResetPortfolio_WithoutSession_Returns401_NoResetRow` |
| BR-03 initial wallet and zero holdings | Task 2 | domain/application mutation checks + integration |
| BR-06 atomic transaction | Task 2, 4 | rollback integration test |

## Architecture impact

```text
web (existing dialog + mutation)
  -> Api PortfolioEndpoint POST /api/portfolio/reset
    -> ResetPortfolioCommandHandler (Application)
      -> Infrastructure write repositories + UoW transaction
         1) mutate wallet to initial
         2) clear holdings
         3) insert portfolio_resets audit row
      -> return PortfolioResetResponse (resetAt/nextEligibleAt + reset wallet snapshot)
```

| Layer | Change summary |
|-------|----------------|
| Domain | Add/confirm explicit behaviors for wallet reset and portfolio holding clear (if missing). |
| Application | Replace read-only stub with transactional reset orchestration in command handler. |
| Infrastructure | Add/write repository methods and reset-row persistence usage needed by handler. |
| Api | Route shape unchanged; preserve RFC 7807 mappings and auth behavior. |
| MatchingEngine | None in Story 2 scope. |
| web/ | No new feature work required; existing Story 1 flow consumes same response shape. |
| AppHost | None. |

## Data & migration plan

| Artifact | Action | DB reference |
|----------|--------|--------------|
| EF migration | None | DB §4.2, §4.4, §4.10 already present |
| Redis keys | None (Story 2 backend does not refresh projections yet) | DB §12 |
| Book recovery | N/A in this story | Tech §7 |

## Open questions

| # | Question | Source | Answer | Status |
|---|----------|--------|--------|--------|
| 1 | Should Story 2 insert `portfolio_resets` now or defer to Story 4 cooldown work? | Spec §2 + DB §10.4 | Insert in Story 2 to keep reset audit and future cooldown source-of-truth consistent. | ✅ |
| 2 | How to force deterministic failure for rollback integration test? | Test design | Inject test fake repository that throws between wallet update and holdings clear. | ✅ |
| 3 | Should Story 2 include open-order reservation release if open orders exist? | Story boundaries | No, keep scoped to wallet+holdings baseline and assume Story 3 will own open-order cancellation semantics. | ✅ |

**Status:** all answered for Story 2 scope.

## Risks and mitigations

| Risk | Likelihood | Impact | Mitigation | Owner task |
|------|------------|--------|------------|------------|
| Resetting wallet while open buy orders still exist can create temporary accounting inconsistency | M | H | Constrain Story 2 tests to seeded state without open orders and document dependency on Story 3 for full BR-06 sequence. | Task 5 |
| Handler becomes tightly coupled to Infrastructure implementation | M | M | Add/extend application abstractions instead of injecting DbContext directly. | Task 2 |
| Rollback path not actually exercised | M | H | Add explicit forced-failure integration test and assert unchanged wallet + holdings. | Task 4 |

## Prerequisites

- [ ] Spec approved for Story 2 implementation
- [ ] Docker/Testcontainers available for integration tests
- [ ] Story 1 branch merged or rebased as baseline (`feature/portfolio-reset-story-1`)
- [ ] Existing `POST /api/portfolio/reset` contract tests are green

## File structure (planned)

```text
src/
  TradingSimulator.Application/
    Abstractions/Persistence/
      (reset write abstractions - create or extend)
    Portfolios/Commands/
      ResetPortfolioCommandHandler.cs                MODIFY
      PortfolioResetErrors.cs                        MODIFY (only if new explicit error code needed)
  TradingSimulator.Domain/
    Users/Wallet.cs                                  MODIFY (if reset behavior missing)
    Portfolios/Portfolio.cs                          MODIFY (if clear-holdings behavior missing)
  TradingSimulator.Infrastructure/
    Persistence/Repositories/
      (wallet/portfolio/reset write methods)         MODIFY
    Persistence/
      ApplicationDatabaseContext.cs                  MODIFY (if DbSet access needed)
tests/
  TradingSimulator.Api.IntegrationTests/Portfolios/
    ResetPortfolioTests.cs                           MODIFY
    Fakes/                                           CREATE/MODIFY
```

## Authorization, session, and domain notes

- **Session model:** Keep cookie-session behavior unchanged from Story 1 (`401` when unauthenticated).
- **Route protection:** Endpoint remains `.RequireAuthorization()` and handler still guards `ICurrentUserAccessor`.
- **Domain rules to preserve:** PRD FR-1.4 initial-state semantics, PRD §7.3 non-negative balances, DB transactional atomicity in §10.4.

## Progress tracker

### Task 1: Baseline Story-2 skeleton and guardrails

| Attribute | Value |
|-----------|--------|
| Spec story | Story 2 |
| Depends on | None |
| Estimated complexity | S |
| Parent story issue | #45 |

#### Objective

Establish a Story 2 branch baseline with existing Story 1 tests green and explicit Story-2-scoped pending tests added (or placeholders) so implementation proceeds against concrete acceptance checks.

#### Implementation notes

- Keep endpoint contract unchanged to avoid UI churn.
- Add Story-2-specific integration test cases first (red tests), preserving existing Story-1 auth/in-flight tests.
- Keep command validator unchanged (no new request fields).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `tests/TradingSimulator.Api.IntegrationTests/Portfolios/ResetPortfolioTests.cs` | Add Story 2 happy/failure assertions scaffolding |
| REUSE | `src/TradingSimulator.Api/Endpoints/PortfolioEndpoint.cs` | Route remains stable |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `ResetPortfolio_WithoutSession_Returns401_NoResetRow` | `tests/.../ResetPortfolioTests.cs` |
| Integration | `ResetPortfolio_WhenStateIsDepleted_WalletReturnsInitialBalances` (initially red) | same |
| Integration | `ResetPortfolio_WhenHoldingsExist_PortfolioReturnsNoHoldings` (initially red) | same |

#### Acceptance criteria

- [x] Existing Story 1 reset tests still pass.
- [x] Story 2 test scaffolding exists and fails for expected reasons before implementation.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | PRD FR-1.4, Tech §6, DB §10.4 |
| Async matching | Not touched in Story 2 |
| PostgreSQL authoritative | Assertions read from API over persisted state |
| Redis projection | N/A |
| RFC 7807 errors | Keep 401 behavior intact |
| SignalR | N/A |
| Aspire | None |
| ADR needed? | No |

#### Risk

Low - test-first skeleton only.

#### Execution notes

- 2026-05-27: Task 1 implemented on branch `feature/portfolio-reset-story-2`.
- Deviation from initial expectation: `ResetPortfolio_WhenStateIsDepleted_WalletReturnsInitialBalances` is already green on the current baseline; `ResetPortfolio_WhenHoldingsExist_PortfolioReturnsNoHoldings` remains red and is the active Story 2 gap entering Task 2.

---

### Task 2: Implement atomic wallet and holdings reset transaction

| Attribute | Value |
|-----------|--------|
| Spec story | Story 2 |
| Depends on | Task 1 |
| Estimated complexity | L |
| Parent story issue | #45 |

#### Objective

Replace the Story 1 read-only stub with a real write transaction that resets wallet balances and clears holdings atomically, then returns the reset response snapshot.

#### Implementation notes

- Handler orchestration order for Story 2: authenticate -> acquire in-flight guard -> load mutable wallet/portfolio -> apply domain reset behaviors -> persist -> insert `portfolio_resets` row -> return response.
- Ensure work executes within command transaction boundary (UnitOfWork behavior).
- Keep `nextEligibleAt` computation from existing cooldown config for response continuity.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `src/TradingSimulator.Application/Portfolios/Commands/ResetPortfolioCommandHandler.cs` | Transactional reset orchestration |
| MODIFY | `src/TradingSimulator.Domain/Users/Wallet.cs` | Reset behavior if missing |
| MODIFY | `src/TradingSimulator.Domain/Portfolios/Portfolio.cs` | Clear holdings behavior if missing |
| MODIFY | `src/TradingSimulator.Infrastructure/Persistence/Repositories/*` | Persist wallet/portfolio changes + reset row insert |
| REUSE | `src/Infrastructure/Persistence/Entities/PortfolioResetRecord.cs` | Audit row entity |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Domain | `Reset_FromDepletedState_SetsInitialBalances` | `tests/TradingSimulator.Domain.UnitTests/...` |
| Domain | `Reset_WithExistingHoldings_RemovesAllHoldings` | `tests/TradingSimulator.Domain.UnitTests/...` |
| Integration | `ResetPortfolio_WhenStateIsDepleted_WalletReturnsInitialBalances` | `tests/.../ResetPortfolioTests.cs` |
| Integration | `ResetPortfolio_WhenHoldingsExist_PortfolioReturnsNoHoldings` | `tests/.../ResetPortfolioTests.cs` |

#### Acceptance criteria

- [x] After reset, wallet API returns 100000 total, 0 reserved, 100000 available.
- [x] After reset, portfolio API returns empty holdings.
- [x] Reset response `wallet` snapshot matches persisted post-reset wallet.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | FR-1.4, Tech §5.2.1-§5.2.2, DB §10.4 |
| Async matching | Not enqueued in Story 2 |
| PostgreSQL authoritative | All mutations persisted in Postgres |
| Redis projection | Deferred |
| RFC 7807 errors | No new error type required for story scope |
| SignalR | Deferred |
| Aspire | None |
| ADR needed? | No |

#### Risk

Medium - open-order interactions remain intentionally deferred.

#### Execution notes

- 2026-05-27: implemented `IPortfolioRepository.ResetForUserAsync` with atomic wallet reset, holdings clear, and `portfolio_resets` insert inside the command UnitOfWork transaction.
- Deviation: Story 2 domain-unit tests were not added in this task because current domain aggregates do not yet expose mutation APIs for reset flows; Task 5 will explicitly track this test-gap for follow-up refactor.

---

### Task 3: Persisted read-your-writes verification and contract stability

| Attribute | Value |
|-----------|--------|
| Spec story | Story 2 |
| Depends on | Task 2 |
| Estimated complexity | M |
| Parent story issue | #45 |

#### Objective

Verify that immediately after successful reset, wallet and portfolio read APIs reflect authoritative post-reset values without requiring endpoint contract changes.

#### Implementation notes

- Keep response DTO shape stable from Story 1 to avoid frontend churn.
- Explicitly validate `GET /api/wallet` and `GET /api/portfolio` after POST in same integration flow.
- Confirm no stale read artifacts from previous seeded state.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `tests/TradingSimulator.Api.IntegrationTests/Portfolios/ResetPortfolioTests.cs` | Read-your-writes assertions |
| REUSE | `tests/Testing.Common/Fixtures/IntegrationTestFixture.cs` | Seed pre-reset states |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `ResetPortfolio_AfterSuccess_GetWalletReturnsResetSnapshot` | `tests/.../ResetPortfolioTests.cs` |
| Integration | `ResetPortfolio_AfterSuccess_GetPortfolioReturnsEmptyHoldings` | `tests/.../ResetPortfolioTests.cs` |
| Manual | Run reset from UI and verify wallet/portfolio cards in trading app | `web/` |

#### Acceptance criteria

- [x] Post-reset GET endpoints are consistent with persisted values within local 2s budget.
- [x] Existing frontend dialog flow still works without payload changes.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | Story 2 happy path AC |
| Async matching | N/A |
| PostgreSQL authoritative | Verified via GET APIs |
| Redis projection | N/A |
| RFC 7807 errors | unchanged |
| SignalR | N/A |
| Aspire | None |
| ADR needed? | No |

#### Risk

Low - test-focused stabilization.

#### Execution notes

- 2026-05-27: promoted Story 2 read-your-writes coverage to explicit Task 3 test names:
  - `ResetPortfolio_AfterSuccess_GetWalletReturnsResetSnapshot`
  - `ResetPortfolio_AfterSuccess_GetPortfolioReturnsEmptyHoldings`
- Verified reset response wallet snapshot equals immediate `GET /api/wallet` values and that `GET /api/portfolio` returns zero holdings for the reset user without contract changes.

---

### Task 4: Failure injection and rollback proof

| Attribute | Value |
|-----------|--------|
| Spec story | Story 2 |
| Depends on | Task 2 |
| Estimated complexity | M |
| Parent story issue | #45 |

#### Objective

Prove that partial reset state cannot persist when reset fails mid-operation by adding a deterministic failing path and asserting original wallet/holdings remain unchanged.

#### Implementation notes

- Introduce a test fake/override to throw inside reset mutation workflow after one write step starts.
- Assert API returns 500 `INTERNAL_ERROR` (or mapped equivalent), then query wallet/portfolio and compare to pre-reset baseline.
- Keep this failure mechanism test-only; no production toggles.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE/MODIFY | `tests/TradingSimulator.Api.IntegrationTests/Portfolios/Fakes/*` | Deterministic reset failure fake |
| MODIFY | `tests/TradingSimulator.Api.IntegrationTests/Portfolios/ResetPortfolioTests.cs` | Rollback scenario assertions |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `ResetPortfolio_WhenMutationFails_RollsBackWalletAndHoldings` | `tests/.../ResetPortfolioTests.cs` |

#### Acceptance criteria

- [x] Failure path returns 500-level error.
- [x] Wallet and holdings equal pre-reset values after failure.
- [x] No partial $100k wallet with old holdings persists.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | Story 2 failure path + DB §10.4 atomicity |
| Async matching | N/A |
| PostgreSQL authoritative | rollback validated in DB-backed integration |
| Redis projection | N/A |
| RFC 7807 errors | error response remains sanitized |
| SignalR | N/A |
| Aspire | None |
| ADR needed? | No |

#### Risk

Medium - flaky failure injection if not isolated; use deterministic fake.

#### Execution notes

- 2026-05-27: added deterministic test fake `FailingPortfolioRepository` that throws after mutating wallet inside reset workflow, before completion.
- Added integration test `ResetPortfolio_WhenMutationFails_RollsBackWalletAndHoldings` and verified:
  - reset returns `500 INTERNAL_ERROR`
  - post-failure `GET /api/wallet` and `GET /api/portfolio` remain equal to pre-reset baseline.

---

### Task 5: Polish, scope guardrails, and verification matrix completion

| Attribute | Value |
|-----------|--------|
| Spec story | Polish |
| Depends on | Task 3, Task 4 |
| Estimated complexity | S |
| Parent story issue | #45 |

#### Objective

Finalize Story 2 without scope bleed into Stories 3/4, document deferred dependencies clearly, and finish regression/manual verification checklist.

#### Implementation notes

- Keep plan and implementation notes explicit that open-order cancellation/history/cooldown enforcement are deferred.
- Run focused integration suite plus touched domain tests.
- Confirm no OpenAPI contract changes required; run `api:verify` if handler response shape unchanged.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `docs/memory/current-status.md` | reflect Story 2 execution readiness/completion during build |
| REUSE | `contracts/openapi/api.v1.yaml` | verify no drift |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | Full `ResetPortfolioTests` green | `tests/.../Portfolios/ResetPortfolioTests.cs` |
| Domain | Added reset behavior tests green | `tests/TradingSimulator.Domain.UnitTests/...` |
| Manual | Reset from UI then verify wallet and holdings panels update after refetch | `web/` |

#### Acceptance criteria

- [x] Story 2 tests pass and Story 1 tests do not regress.
- [x] Deferred Story 3/4 items are documented in plan and issue updates.
- [x] `/build` can execute tasks without re-research.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | all cited references remain aligned |
| Async matching | deferred documented |
| PostgreSQL authoritative | maintained |
| Redis projection | unchanged |
| RFC 7807 errors | unchanged |
| SignalR | deferred |
| Aspire | no topology changes |
| ADR needed? | No |

#### Risk

Low - closure and verification only.

#### Execution notes

- 2026-05-27 verification run completed:
  - `dotnet test tests/Api.IntegrationTests --filter FullyQualifiedName~ResetPortfolioTests -c Release --nologo` (6/6 green)
  - `dotnet test tests/Domain.UnitTests -c Release --nologo` (27/27 green regression)
  - `yarn --cwd web api:verify` (OpenAPI in sync)
- Story 3/4 deferrals remain explicit in **Non-goals** and **Deferred work (Plan B)**; no Story 3/4 behavior was implemented in Story 2 scope.
- Follow-up note retained: Story 2 currently validates reset behavior via API integration tests; dedicated domain reset mutation methods/tests are deferred pending a domain-model refactor.

## Reference files

| File | Why open it |
|------|-------------|
| `src/Application/Portfolios/Commands/ResetPortfolioCommandHandler.cs` | Replace Story 1 stub with transactional logic |
| `src/Infrastructure/Persistence/Entities/PortfolioResetRecord.cs` | Persist reset audit row |
| `tests/Api.IntegrationTests/Portfolios/ResetPortfolioTests.cs` | Existing reset test harness |
| `docs/plans/20260525-260000-portfolio-reset-story-1.md` | Baseline from previous story |
| `docs/specs/20260525-251500-portfolio-reset.md` | Story 2 acceptance criteria source |

## Implementation details (for /build)

- Keep `POST /api/portfolio/reset` request/response contract stable.
- Reuse existing in-flight guard and unauthorized mapping from Story 1.
- Move from read repository flow to write-capable orchestration through application abstractions.
- Prefer domain methods for reset semantics, not direct primitive mutation in handlers.
- Integration tests must seed non-default wallet and holdings before reset call to prove behavior.
- Rollback test must simulate an exception inside transaction scope and assert pre-state remains.

## Verification matrix (plan-level)

| Spec AC | Verified by |
|---------|-------------|
| Wallet becomes 100000/0/100000 after reset | Task 2 + Task 3 integration tests |
| Portfolio holdings are empty after reset | Task 2 + Task 3 integration tests |
| Failed reset does not partially mutate state | Task 4 rollback integration test |
| Unauthorized reset returns 401 and no reset row | Task 1/5 integration regression |

## Rollback / recovery

- **Code:** revert Story 2 branch commits.
- **DB:** no migration rollback required; data-level rollback covered by transaction.
- **Redis:** no reset-specific key changes in Story 2.

## Deferred work (Plan B)

- Story 3: cancel open orders, release related reservations, clear order/trade history reads, and matching-engine cleanup.
- Story 4: server cooldown enforcement (`RESET_COOLDOWN_ACTIVE`) and eligibility UX source-of-truth.
- Story 5: query invalidation and real-time consistency fanout.

## GitHub Links

| Local ref | Issue # | Type | Title | URL |
|-----------|---------|------|-------|-----|
| spec.Story 2 | 45 | Story | US-04 / Story 2: Restore starting cash and empty holdings | https://github.com/tranvuongduy2003/trading-simulator/issues/45 |
