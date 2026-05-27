---
artifact_type: plan
artifact_version: 1
id: plan-20260527-210000-portfolio-reset-story-2
title: Portfolio Reset — Story 2 (Restore starting cash and empty holdings)
slug: portfolio-reset-story-2
filename_template: 20260527-210000-portfolio-reset-story-2.md
created_at: 2026-05-27T21:00:00+07:00
updated_at: 2026-05-27T21:00:00+07:00
status: draft
owner: engineering
tags: [plan, implementation, trading-simulator, portfolio-reset, wallet, holdings, us-04, story-2]
related_spec: docs/specs/20260525-251500-portfolio-reset.md
related_plans: [docs/plans/20260525-260000-portfolio-reset-story-1.md]
prd_refs: [PRD §5.1 US-04, PRD §6.1 FR-1.4, PRD §7.3]
tech_refs: [Tech §5.2.1 Wallet, Tech §5.2.2 Portfolio, Tech §6 ResetPortfolioCommand, Tech §16 Trading:InitialVirtualCash, Tech §16 Trading:PortfolioResetCooldownMinutes]
db_refs: [DB §4.2 wallets, DB §4.3 portfolios, DB §4.4 holdings, DB §4.10 portfolio_resets, DB §10.4 Portfolio Reset]
github:
  repo: tranvuongduy2003/trading-simulator
  epic_issue: 43
  story_issue_ids: [45]
  last_synced_at: 2026-05-27T21:00:00+07:00
search_index:
  keywords: [portfolio reset, story 2, wallet reset, holdings clear, 100000, portfolio_resets, atomic transaction, IPortfolioResetWriteRepository, ResetPortfolioCommandHandler, read-your-writes, BR-03, BR-06, BR-09, EC-02, EC-06]
  bounded_contexts: [Trading]
  task_count: 4
---

# Implementation Plan: Portfolio Reset — Story 2

| Field | Value |
|-------|--------|
| Spec | `docs/specs/20260525-251500-portfolio-reset.md` (§2 Story 2) |
| Epic | [#43 — Portfolio reset (US-04)](https://github.com/tranvuongduy2003/trading-simulator/issues/43) |
| Story issue | [#45 — Restore starting cash and empty holdings](https://github.com/tranvuongduy2003/trading-simulator/issues/45) |
| Status | DRAFT |
| Tasks | 4 |
| Branch | `feature/portfolio-reset-story-2` |
| Aspire impact | No topology change |
| Schema impact | No — tables exist |
| Test levels | Api.IntegrationTests (primary); optional Domain.UnitTests for wallet reset helper |
| ADRs required | Update ADR-005 (stub superseded for wallet/holdings slice) |
| GitHub | Synced 2026-05-27 — see §GitHub Links |

Execution note (deviation): Implementation for this `/build` session runs on `feature/portfolio-reset-story-2-local` (branched from current workspace state) because local uncommitted documentation changes prevented checkout of `feature/portfolio-reset-story-2`.

## Executive summary

Story 2 (US-04) replaces the Story 1 **contract stub** with a real PostgreSQL write path: within one transaction, set the user’s wallet to **`Trading:InitialVirtualCash`** (100000.0000 USD) with **zero** reserved balance, **delete all holdings** for their portfolio, and **append** a `portfolio_resets` audit row. The handler returns the spec-shaped **200** body with post-reset wallet figures. **Order cancellation, reservation release on open orders, Redis book updates, and history cutoff remain Story 3+** (issue out of scope). Integration tests prove read-your-writes on `GET /api/wallet` and `GET /api/portfolio`, rollback on failure, and **401** with no audit row.

## Goals and non-goals

**Goals**

- G1: After successful `POST /api/portfolio/reset`, `GET /api/wallet` shows total/reserved/available **100000 / 0 / 100000** within **2 s** (local MVP).
- G2: After reset, `GET /api/portfolio` returns **no holdings**; portfolio value equals available cash at reset instant.
- G3: Atomic commit/rollback — failure yields pre-reset wallet and holdings; **401** performs **no** `portfolio_resets` insert.
- G4: Use `Trading:InitialVirtualCash` (BR-09: per-user reallocation, not system minting).

**Non-goals** (this plan will not do)

- NG1: Cancel open orders, matching-engine book removal, Redis tape/book projection (Story 3 — [#46](https://github.com/tranvuongduy2003/trading-simulator/issues/46)).
- NG2: Server-side **24h cooldown** enforcement (`RESET_COOLDOWN_ACTIVE`) — Story 4 ([#47](https://github.com/tranvuongduy2003/trading-simulator/issues/47)).
- NG3: Trade/order history read cutoff (Story 3 / spec open question #1).
- NG4: `PortfolioResetEvent` domain event + SignalR fan-out (Story 5 / BR-11 — optional note in Task 4 only).
- NG5: Frontend changes (Story 1 UI already calls POST; refetch behavior is Story 5).

## Traceability matrix

| Spec story | Plan task(s) | Test evidence |
|------------|--------------|---------------|
| Story 2 — happy path wallet | Task 2, 4 | `ResetPortfolio_AfterDepletedWallet_Returns100k_OnGetWallet` |
| Story 2 — empty holdings | Task 2, 4 | `ResetPortfolio_ClearsHoldings_OnGetPortfolio` |
| Story 2 — failure no partial state | Task 3 | `ResetPortfolio_WhenWriteFails_PreResetStateUnchanged` |
| Story 2 — 401 no reset row | Task 3 | `ResetPortfolio_WithoutSession_NoPortfolioResetsRow` |
| EC-02 zero holdings, depleted cash | Task 4 | `ResetPortfolio_WithZeroHoldings_StillRestoresCash` |
| EC-06 concurrency retry | Task 3 | Rely on existing `UnitOfWorkBehavior` + optional `RowVersion` bump test |
| BR-03 / BR-06 (partial) | Task 2 | Integration tests + transaction boundary |
| BR-09 | Task 2 | Uses `IOptions<TradingOptions>.InitialVirtualCash` |
| Story 1 contract shape | Task 1, 4 | Existing `ResetPortfolio_WithSession_Returns200_WithContractShape` (updated assertions) |
| Story 1 in-flight 409 | — | **Reuse** existing test (unchanged guard) |
| Cooldown enforcement | — | **Deferred** Story 4 |
| Open orders after reset | — | **Known gap** until Story 3 (see Risks) |

## Architecture impact

```text
┌──────── web/ (no change) ────────────────────────────────────────┐
│  POST /api/portfolio/reset  (Story 1 dialog already wired)        │
└────────────────────────────┬─────────────────────────────────────┘
                             │ session cookie
┌────────────────────────────▼─────────────────────────────────────┐
│  Api: PortfolioEndpoint → MediatR                                 │
│  ResetPortfolioCommandHandler                                     │
│    1. Auth (ICurrentUserAccessor)                                 │
│    2. IResetInFlightGuard (409)                                   │
│    3. IPortfolioResetWriteRepository.ResetForUserAsync  ◄── NEW   │
│    4. Map PortfolioResetWriteModel → PortfolioResetResponse       │
└────────────────────────────┬─────────────────────────────────────┘
                             │ IUnitOfWorkRequest → UnitOfWorkBehavior
┌────────────────────────────▼─────────────────────────────────────┐
│  Infrastructure: PortfolioResetWriteRepository                    │
│    BEGIN (implicit via behavior)                                  │
│    UPDATE wallets SET total=InitialVirtualCash, reserved=0        │
│    DELETE holdings WHERE portfolio_id = …                         │
│    INSERT portfolio_resets (user_id, reset_at, reason)            │
│    COMMIT                                                         │
└────────────────────────────┬─────────────────────────────────────┘
                             │
                    PostgreSQL (authoritative)
```

| Layer | Change summary |
|-------|----------------|
| Domain | Optional: `Wallet.ResetToInitial(Money)` value-object helper + unit test (keeps rules out of raw SQL) |
| Application | Replace stub handler body; depend on `IPortfolioResetWriteRepository` + `IOptions<TradingOptions>`; stop returning pre-reset wallet from read repo |
| Infrastructure | Finish `PortfolioResetWriteRepository.ResetForUserAsync`; register `IPortfolioResetWriteRepository` in DI |
| Api | No route change |
| MatchingEngine | **No change** (Story 3) |
| web/ | **No change** |
| AppHost | **No change** |

## Data & migration plan

| Artifact | Action | DB reference |
|----------|--------|--------------|
| EF migration | **None** | `portfolio_resets`, `wallets`, `holdings` exist |
| Redis keys | **None** | Wallet not cached (spec §5) |
| Book recovery | **N/A** | Story 3 |
| Transaction steps (this story) | **Subset of DB §10.4:** (3) set wallet initial, (4) remove holdings, (5) insert `portfolio_resets` | DB §10.4 steps 3–5 |
| Skipped until Story 3 | (1) cancel open orders, (2) release order-linked reservations | DB §10.4 steps 1–2 |

`portfolio_resets.reason`: set `"user_initiated"` (nullable column per DB §4.10).

## Open questions

| # | Question | Source | Answer | Status |
|---|----------|--------|--------|--------|
| 1 | Block reset when user has open orders until Story 3? | BR-06 vs story split | **No block in Story 2** — document risk; Story 3 restores full atomic flow | ✅ Answered |
| 2 | Domain methods vs repository-only SQL? | Clean Architecture | Prefer **`Wallet.ResetToInitial`** helper in Domain + EF maps amounts; holdings delete stays in write repository | ✅ Answered |
| 3 | Raise `PortfolioResetEvent` now? | BR-11 | **Defer** to Story 5 unless trivial `Raise` with no dispatcher | ⏳ Deferred |
| 4 | History soft-cutoff vs delete? | Spec §13 Q1 | **Out of scope** Story 2 | ⏳ Deferred |

## Risks and mitigations

| Risk | Likelihood | Impact | Mitigation | Owner task |
|------|------------|--------|------------|------------|
| User with open orders: wallet forced to 100k while orders still reserve cash/qty | M | H until Story 3 | Document in ADR-005 addendum; integration tests seed **no open orders**; Story 3 merges full DB §10.4 | Task 4 |
| Story 1 tests assumed stub (no DB writes) | M | M | Update tests to seed depleted state / assert `portfolio_resets` count | Task 4 |
| `ExecuteUpdate`/`ExecuteDelete` bypass domain events | L | L | Acceptable for cross-aggregate reset port; optional domain helper for amounts only | Task 2 |
| Optimistic concurrency on `wallets.row_version` | L | M | `UnitOfWorkBehavior` retries; integration test optional | Task 3 |
| Untracked `IPortfolioResetWriteRepository` files not in DI | H | H | Task 1 registers scoped implementation | Task 1 |

## Prerequisites

- [ ] Story 1 merged or cherry-picked onto `feature/portfolio-reset-story-2` (POST route, dialog, in-flight guard, OpenAPI)
- [ ] Aspire local stack runs; Docker available for Testcontainers
- [ ] Initial migration applied (`portfolio_resets` table present)
- [ ] Familiarity with `GetMyWalletTests.SeedWalletBalancesAsync` pattern for wallet seeding

## File structure (planned)

```text
src/
  Application/
    Abstractions/Persistence/
      IPortfolioResetWriteRepository.cs     MODIFY (ensure ResetForUserAsync contract)
    Portfolios/Commands/
      ResetPortfolioCommandHandler.cs       MODIFY (replace stub)
  Domain/
    Users/Wallet.cs                         MODIFY (optional ResetToInitial)
  Infrastructure/
    Persistence/Repositories/
      PortfolioResetWriteRepository.cs      MODIFY (implement ResetForUserAsync)
    DependencyInjection.cs                  MODIFY (register write repo)
tests/
  Api.IntegrationTests/
    Portfolios/
      ResetPortfolioTests.cs                MODIFY + new scenarios
      PortfolioResetTestHelpers.cs          CREATE (seed wallet + holdings)
      Fakes/
        ThrowOnPortfolioResetWriteRepository.cs  CREATE (rollback test)
docs/memory/decisions.md                    MODIFY (ADR-005 addendum)
```

## Authorization, session, and domain notes

- **Session model:** Cookie session (ADR-001). Handler returns **401** before any write when `ICurrentUserAccessor.UserId` is null.
- **Route protection:** `POST /api/portfolio/reset` already requires authorization (Story 1).
- **Domain rules (Story 2 slice):**
  - **BR-03:** Wallet total = `InitialVirtualCash`, reserved = 0; all holdings removed.
  - **BR-06:** Single UoW transaction for wallet + holdings + audit row; rollback on any failure.
  - **BR-09:** Only this user’s wallet row changes; constant from config, not computed from market.
- **Initial cash:** `IOptions<TradingOptions>.InitialVirtualCash` — same as registration (FR-1.3).

## Progress tracker

### Task 1: Replace reset stub with write-capable skeleton

| Attribute | Value |
|-----------|--------|
| Spec story | Story 2 |
| Depends on | None (branches from Story 1 baseline) |
| Estimated complexity | M |
| Parent story issue | #45 |

#### Objective

`ResetPortfolioCommandHandler` calls `IPortfolioResetWriteRepository` inside the existing MediatR UoW pipeline; DI registers the repository. `ResetForUserAsync` may initially return the target snapshot without persisting (skeleton), but the handler path no longer uses `IWalletReadRepository` for the success payload.

#### Implementation notes

- Register `services.AddScoped<IPortfolioResetWriteRepository, PortfolioResetWriteRepository>()`.
- Handler flow: auth → in-flight guard → `ResetForUserAsync(userId, options.InitialVirtualCash, clock.UtcNow)` → map to `PortfolioResetResponse` with `nextEligibleAt = resetAt + PortfolioResetCooldownMinutes` (timestamps only; **no cooldown check** yet).
- Remove stub comment; keep `IWalletReadRepository` only if needed for `WalletNotFound` when repo returns null.
- Ensure `ResetPortfolioCommand` remains `IUnitOfWorkRequest` so `UnitOfWorkBehavior` wraps the handler.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `src/Infrastructure/DependencyInjection.cs` | Register write repository |
| MODIFY | `src/Application/Portfolios/Commands/ResetPortfolioCommandHandler.cs` | Call write port |
| MODIFY | `src/Infrastructure/Persistence/Repositories/PortfolioResetWriteRepository.cs` | Skeleton `ResetForUserAsync` |
| REUSE | `src/Application/Abstractions/Persistence/IPortfolioResetWriteRepository.cs` | Port + DTOs |
| REUSE | `src/Application/Behaviors/UnitOfWorkBehavior.cs` | Transaction boundary |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | Existing suite still compiles; 401 unchanged | `ResetPortfolioTests.cs` |

#### Acceptance criteria

- [x] Handler depends on `IPortfolioResetWriteRepository`, not wallet read stub
- [x] DI resolves write repository in Api host
- [x] `dotnet build` succeeds

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | FR-1.4 partial; Tech §6; DB §10.4 (subset) |
| PostgreSQL authoritative | Writes go through EF context in UoW |
| RFC 7807 errors | Unchanged 401/409 |
| ADR needed? | No |

#### Risk

None — wiring only.

---

### Task 2: Implement atomic wallet reset and holdings clear

| Attribute | Value |
|-----------|--------|
| Spec story | Story 2 |
| Depends on | Task 1 |
| Estimated complexity | L |
| Parent story issue | #45 |

#### Objective

`ResetForUserAsync` performs, in one transaction: update wallet to initial cash and zero reserved; delete all holdings for the user’s portfolio; insert `portfolio_resets` row; return `PortfolioResetWriteModel` with **100000 / 0 / 100000**.

#### Implementation notes

- Resolve `portfolio_id` via `portfolios.user_id` (unique).
- Wallet update: `AsTracking()` load or `ExecuteUpdateAsync` on `wallets` filtered by `user_id`; set `TotalBalance = initialVirtualCash`, `ReservedBalance = 0`, bump `UpdatedAt`.
- Holdings: `ExecuteDeleteAsync` on `holdings` where `portfolio_id` matches (idempotent if zero rows).
- Audit: `PortfolioResets.Add(new PortfolioResetRecord { UserId, ResetAt, Reason = "user_initiated" })`.
- Return null if wallet/portfolio missing → handler maps to `WALLET_NOT_FOUND` or `PORTFOLIO_NOT_FOUND`.
- Optional domain: `Wallet.ResetToInitial(Money)` used to validate non-negative amounts before EF update.
- **Do not** cancel orders or touch `orders` table in this task.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `src/Infrastructure/Persistence/Repositories/PortfolioResetWriteRepository.cs` | Full reset implementation |
| MODIFY | `src/Domain/Users/Wallet.cs` | Optional `ResetToInitial` |
| CREATE | `tests/Domain.UnitTests/Users/WalletResetTests.cs` | Optional domain test |
| CREATE | `tests/Api.IntegrationTests/Portfolios/PortfolioResetTestHelpers.cs` | Seed depleted wallet + AAPL holding |
| MODIFY | `tests/Api.IntegrationTests/Portfolios/ResetPortfolioTests.cs` | Add Story 2 integration scenarios |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `ResetPortfolio_AfterDepletedWallet_Returns100k_OnGetWallet` | `ResetPortfolioTests.cs` |
| Integration | `ResetPortfolio_ClearsHoldings_OnGetPortfolio` | `ResetPortfolioTests.cs` |
| Integration | `ResetPortfolio_InsertsPortfolioResetsRow` | `ResetPortfolioTests.cs` |
| Domain | `ResetToInitial_SetsBalances` (if domain helper added) | `WalletResetTests.cs` |

#### Acceptance criteria

- [x] GIVEN seeded total 42000, reserved 5000, 50 AAPL shares → WHEN POST reset → THEN response wallet **100000 / 0 / 100000**
- [x] GIVEN same → WHEN GET wallet/portfolio within 2s → THEN matching balances and **empty holdings**
- [x] Exactly **one** new `portfolio_resets` row for user
- [x] Uses `InitialVirtualCash` from options (not hardcoded literal in repository)

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD | FR-1.3, FR-1.4 (cash slice) |
| PostgreSQL authoritative | Single transaction via UoW |
| Redis projection | N/A |
| Async matching | N/A this story |

#### Risk

Open orders + reset inconsistency until Story 3 — see Risks table.

---

### Task 3: Add rollback and unauthorized no-write guarantees

| Attribute | Value |
|-----------|--------|
| Spec story | Story 2 (failure paths) |
| Depends on | Task 2 |
| Estimated complexity | M |
| Parent story issue | #45 |

#### Objective

Prove **500** (or handler failure) leaves pre-reset wallet/holdings/audit unchanged, and **401** creates no `portfolio_resets` row.

#### Implementation notes

- `ThrowOnPortfolioResetWriteRepository` decorates or replaces `IPortfolioResetWriteRepository` in test factory: throw after intentional no-op or immediately on `ResetForUserAsync`.
- Assert pre-seeded wallet/holdings counts unchanged via `ApplicationDatabaseContext`.
- Unauthorized POST: assert `portfolio_resets` count for user remains 0 (join via `users` from registration id).
- Ensure `UnitOfWorkBehavior` rolls back on exception (existing behavior).
- Deviation: unauthorized no-write assertion is implemented by registering a user in test setup and verifying that user's `portfolio_resets` row count is unchanged before/after the unauthorized POST (equivalent guarantee without user-id extraction from the unauthorized request context).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `tests/Api.IntegrationTests/Portfolios/Fakes/ThrowOnPortfolioResetWriteRepository.cs` | Force failure |
| MODIFY | `tests/Api.IntegrationTests/Portfolios/ResetPortfolioTests.cs` | Rollback + 401 audit tests |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `ResetPortfolio_WhenWriteFails_PreResetStateUnchanged` | `ResetPortfolioTests.cs` |
| Integration | `ResetPortfolio_WithoutSession_NoPortfolioResetsRow` | `ResetPortfolioTests.cs` |

#### Acceptance criteria

- [x] Simulated write failure → GET wallet/portfolio still show **pre-reset** seeded values
- [x] No additional `portfolio_resets` row on failure
- [x] 401 → zero `portfolio_resets` for that user

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| RFC 7807 | 500 `INTERNAL_ERROR` on unhandled throw |
| EC-06 | Concurrency handled by pipeline (no partial commit) |

#### Risk

None — test-only + relies on existing UoW.

---

### Task 4: Polish and verification closure

| Attribute | Value |
|-----------|--------|
| Spec story | Polish |
| Depends on | Task 3 |
| Estimated complexity | S |
| Parent story issue | #45 |

#### Objective

Update ADR-005, align integration tests with real writes, add EC-02 test, run full regression, manual smoke on Aspire.

#### Implementation notes

- ADR-005 addendum: Story 2 persists wallet/holdings/`portfolio_resets`; Story 1 stub superseded for those writes; order cancel still future.
- Update `ResetPortfolio_WithSession_Returns200_WithContractShape`: register → seed depleted wallet → reset → assert response **and** GET wallet.
- `ResetPortfolio_WithZeroHoldings_StillRestoresCash` (EC-02): 500 total, 0 holdings → 100k.
- Run `dotnet test` on `Api.IntegrationTests` Portfolios + Users wallet tests.
- Manual: Aspire → login → seed via trades or SQL → reset → verify wallet card and portfolio tab (no frontend code required if Story 1 merged).
- Deviation: Manual Aspire smoke checklist is left to operator execution and is not performed during this automation run.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `docs/memory/decisions.md` | ADR-005 addendum |
| MODIFY | `tests/Api.IntegrationTests/Portfolios/ResetPortfolioTests.cs` | EC-02 + shape test updates |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | Full `ResetPortfolioTests` green | `ResetPortfolioTests.cs` |
| Manual | Wallet + portfolio panels after reset | Aspire |

#### Acceptance criteria

- [x] All Story 2 acceptance criteria in verification matrix pass
- [x] `dotnet test` passes for touched projects
- [x] ADR-005 documents Story 2 scope vs Story 3
- [ ] Manual smoke: depleted cash → reset → **$100,000** and empty holdings

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| OpenAPI | Regenerate if response examples drift (`api:verify` if web touched — likely N/A) |
| No regression | Login, register, GET wallet auth |

#### Risk

None.

## Reference files

| File | Why open it |
|------|-------------|
| `src/Application/Portfolios/Commands/ResetPortfolioCommandHandler.cs` | Replace stub |
| `src/Infrastructure/Persistence/Repositories/PortfolioResetWriteRepository.cs` | Implement writes |
| `tests/Api.IntegrationTests/Users/GetMyWalletTests.cs` | `SeedWalletBalancesAsync` pattern |
| `docs/plans/20260525-260000-portfolio-reset-story-1.md` | Prior story patterns |
| `docs/DATABASE.md` §10.4 | Full reset transaction (superset) |
| `src/Application/Users/Commands/RegisterUserCommandHandler.cs` | `InitialVirtualCash` usage |

## Implementation details (for /build)

### Handler (`ResetPortfolioCommandHandler`)

1. `userId = currentUserAccessor.UserId` → else `UNAUTHORIZED`.
2. `resetInFlightGuard.TryBegin` → else `RESET_IN_PROGRESS`.
3. `try/finally` with `End(userId)`.
4. `var result = await portfolioResetWriteRepository.ResetForUserAsync(userId, tradingOptions.InitialVirtualCash, clock.UtcNow, ct)`.
5. If `result` is null → `WALLET_NOT_FOUND` (or portfolio not found).
6. Return `PortfolioResetResponse(resetAt, resetAt.AddMinutes(cooldownMinutes), new PortfolioResetWalletSnapshot(...))` from **write model**, not read repository.

### Repository (`ResetForUserAsync`)

Pseudo-sequence (same DbContext as UoW — do not open nested transactions):

1. Load portfolio id for user; if missing return null.
2. Update wallet row (tracking or bulk update).
3. Delete holdings for portfolio id.
4. Add `PortfolioResetRecord`.
5. `SaveChanges` happens in `UnitOfWorkBehavior` after handler returns success.

### Error types

| HTTP | code | When |
|------|------|------|
| 401 | `UNAUTHORIZED` | No session |
| 404 | `WALLET_NOT_FOUND` | No wallet row |
| 409 | `RESET_IN_PROGRESS` | In-flight guard |
| 500 | `INTERNAL_ERROR` | Unhandled exception (transaction rolled back) |

### Integration seed helper (sketch)

- `SeedWalletBalancesAsync(userId, total, reserved)` — reuse from `GetMyWalletTests` or extract to shared helper.
- `SeedHoldingAsync(userId, symbol: "AAPL", quantity: 50, averagePrice: 150m)` — insert `holdings` row via portfolio id lookup.

### Story 3 handoff

When implementing order cancel, **move** reset orchestration to full DB §10.4 order (cancel → release → wallet → holdings → audit) in the same repository or a dedicated `IPortfolioResetOrchestrator` — do not duplicate wallet/holdings SQL.

## Verification matrix (plan-level)

| Spec AC | Verified by |
|---------|-------------|
| Depleted wallet → 100k on GET wallet | Task 2 integration test |
| Holdings cleared on GET portfolio | Task 2 integration test |
| 500 → pre-reset state | Task 3 integration test |
| 401 → no `portfolio_resets` row | Task 3 integration test |
| EC-02 zero holdings, low cash | Task 4 integration test |
| Double-submit 409 | Existing Story 1 test (unchanged) |

## Rollback / recovery

- **Code:** Revert branch; handler can be re-stubbed per ADR-005 if needed.
- **DB:** No migration; user data restored by another reset after Story 4 cooldown or manual SQL in dev.
- **Redis:** N/A.

## Deferred work (Plan B)

- Story 3: Cancel open orders + engine enqueue + history cutoff ([#46](https://github.com/tranvuongduy2003/trading-simulator/issues/46)).
- Story 4: Cooldown enforcement ([#47](https://github.com/tranvuongduy2003/trading-simulator/issues/47)).
- Story 5: Query invalidation + SignalR ([#48](https://github.com/tranvuongduy2003/trading-simulator/issues/48)).
- `PortfolioResetEvent` + dispatcher.
- Block reset when open orders exist (optional product rule).

## GitHub Links

| Local ref | Issue # | Type | Title | URL |
|-----------|---------|------|-------|-----|
| spec Story 2 | 45 | Story | US-04 / Story 2: Restore starting cash and empty holdings | https://github.com/tranvuongduy2003/trading-simulator/issues/45 |
| epic | 43 | Epic | Spec: Portfolio reset (US-04) | https://github.com/tranvuongduy2003/trading-simulator/issues/43 |

> Plan tasks (Task 1–4) are tracked in this file only, not as separate GitHub issues.

## Manual UI checklist (operator)

1. `aspire run` — register or login as test user.
2. Via SQL or future orders UI: set wallet to ~$42k total, $5k reserved; add AAPL holding (50 shares).
3. Open account menu → **Reset portfolio** → confirm.
4. Virtual cash card / top bar → **$100,000.00** available within ~2 s.
5. Portfolio / holdings tab → empty.
6. (Known) Open orders tab may still show pre-reset orders until Story 3 — note if present.
7. Repeat reset immediately → should still succeed (cooldown not enforced until Story 4).
