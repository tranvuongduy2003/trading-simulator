---
artifact_type: plan
artifact_version: 1
id: plan-20260525-220000-virtual-cash-story-2
title: Virtual Cash Balance — Story 2 (Total vs reserved)
slug: virtual-cash-story-2
filename_template: 20260525-220000-virtual-cash-story-2.md
created_at: 2026-05-25T22:00:00+07:00
updated_at: 2026-05-25T24:00:00+07:00
status: completed
owner: engineering
tags: [plan, implementation, trading-simulator, wallet, cash, us-03, story-2]
related_spec: docs/specs/20260525-201500-virtual-cash-balance.md
related_plans: [docs/plans/20260525-203000-virtual-cash-story-1.md]
prd_refs: [PRD §5.1 US-03, PRD §6.6 FR-6.2, PRD §7.3, PRD §7.4]
tech_refs: [Tech §5.2.1, Tech §6, Tech §8.1, Tech §17.3]
db_refs: [DB §4.2 wallets, DB §5 invariants, DB §6.2]
github:
  repo: tranvuongduy2003/trading-simulator
  epic_issue: 33
  story_issue_ids: [35]
  last_synced_at: 2026-05-25T22:00:00+07:00
search_index:
  keywords: [wallet, total balance, reserved balance, available balance, virtual cash, breakdown, BR-02, BR-04, EC-02, GetMyWallet, VirtualCashCard, display integrity]
  bounded_contexts: [Trading]
  task_count: 4
---

# Implementation Plan: Virtual Cash Balance — Story 2

| Field | Value |
|-------|--------|
| Spec | `docs/specs/20260525-201500-virtual-cash-balance.md` (§2 Story 2) |
| GitHub story | [#35 — Understand total versus reserved cash](https://github.com/tranvuongduy2003/trading-simulator/issues/35) |
| Epic | [#33 — Virtual cash balance display (US-03)](https://github.com/tranvuongduy2003/trading-simulator/issues/33) |
| Depends on | Story 1 plan shipped (`docs/plans/20260525-203000-virtual-cash-story-1.md`) — `VirtualCashCard`, `useWalletQuery`, `GET /api/wallet` |
| Status | COMPLETE (automation) — manual UI checklist pending operator |
| Tasks | 4 |
| Branch | `feature/virtual-cash-story-2` |
| Aspire impact | No |
| Schema impact | No |
| Test levels | API integration (new reserved scenario) · Manual UI |
| ADRs required | None |
| GitHub | Synced 2026-05-25 — see §GitHub Links |

## Executive summary

Story 2 (US-03) helps users understand **why available cash is lower than total** by showing **total** and **reserved** alongside **available** on the trading dashboard cash card. Story 1 already introduced a secondary line (`Total … · Reserved …`) and two-decimal `formatUsd`; this plan **verifies and hardens** Story 2: an integration test seeds non-zero `reserved_balance` in PostgreSQL (EC-02 / spec happy path), the card emphasizes the breakdown when reserved is positive and uses clear zero-reserved copy, and the UI **never recomputes** available from total − reserved (failure-path AC: display exactly three API fields). Top bar chip stays **available-only** per Story 1 scope. No backend handler or schema changes expected.

## Goals and non-goals

**Goals**

- G1: Happy path AC — **$50,000** total, **$10,000** reserved → UI shows **$40,000.00** available and secondary **Total $50,000.00 · Reserved $10,000.00** (or equivalent labels).
- G2: Zero reserved — show **$0.00** and concise copy that nothing is tied up in open buys (spec allows either; implement both for clarity).
- G3: EC-02 — when **$5,000** reserved, all three fields visible (covered by integration test + manual).
- G4: Display integrity — render `availableBalance`, `totalBalance`, `reservedBalance` from API/normalized wallet only; no client-side fourth derived amount.
- G5: Automated API proof that `GET /api/wallet` returns correct breakdown when `reserved_balance > 0`.

**Non-goals**

- NG1: Story 3 — cross-user cache / session privacy (#36).
- NG2: Story 4 — refetch-on-focus / read-your-writes (#37).
- NG3: Order placement or live reserve updates (US-10+); seeding reserved in tests is direct DB update only.
- NG4: Top bar total/reserved (chip remains available-only).
- NG5: SignalR wallet push, migrations, matching engine, Redis wallet projections.
- NG6: Frontend unit/E2E test framework (manual for inconsistent-API defect path).

## Traceability matrix

| Spec story | Plan task(s) | Test evidence |
|------------|--------------|---------------|
| Story 2 — $50k / $10k / $40k happy path | Task 1, 2 | `GetMyWallet_WithSeededReserved_ReturnsCorrectBreakdown`; manual |
| Story 2 — reserved $0 visible | Task 2 | Manual; existing register test + card copy |
| Story 2 — inconsistent API (defect) | Task 3 | Code review + manual mock props; no client recompute |
| BR-02 available = total − reserved | Task 1 | Integration assert on API response |
| BR-04 reserved reflects open buys | Task 2 | Copy when reserved > 0; future orders wire same API |
| EC-02 $5k reserved | Task 1 | Integration uses $5k reserved variant (or second assert in same test) |

## Architecture impact

```text
┌──────────────────┐
│ VirtualCashCard  │  reads normalizeWallet(data) — three fields, no math
│  (trading view)  │
└────────┬─────────┘
         │ useWalletQuery
         ▼
   GET /api/wallet ──► GetMyWalletQueryHandler (unchanged)
                         available = total − reserved (server)
         ▼
   PostgreSQL wallets.reserved_balance  ◄── test seeds via EF only
```

| Layer | Change summary |
|-------|----------------|
| Domain | **REUSE** — `Wallet.AvailableBalance`, invariants unchanged |
| Application | **REUSE** — `GetMyWalletQueryHandler` |
| Infrastructure | **REUSE** — `WalletReadRepository`; tests **MODIFY** wallet row via `ApplicationDatabaseContext` |
| Api | **REUSE** — `WalletEndpoint` |
| MatchingEngine | None |
| web/ | **MODIFY** `virtual-cash-card.tsx`, `wallet-display.ts`; **REUSE** `formatUsd`, `useWalletQuery` |
| AppHost | None |

## Data & migration plan

| Artifact | Action | DB reference |
|----------|--------|--------------|
| EF migration | **None** | — |
| Redis keys | **None** | — |
| Book recovery | N/A | — |
| Test seeding | **UPDATE** `wallets.reserved_balance` (and optionally `total_balance`) after register in integration test | DB §4.2, `ck_wallets_reserved_le_total` |

## Open questions

| # | Question | Source | Answer | Status |
|---|----------|--------|--------|--------|
| 1 | Is Story 1 secondary line enough for Story 2 emphasis? | Code review | **Yes for MVP** — add conditional helper line when `reservedBalance > 0`; no chart/tooltip | ✅ Answered |
| 2 | How to prove inconsistent-API UI without breaking handler? | Spec failure path | **Manual** — pass mock props to `VirtualCashCard` in dev; enforce no `total - reserved` in component via review + `wallet-display` helpers | ✅ Answered |
| 3 | Branch from `main` or Story 1 branch? | Delivery | **After Story 1 merges** — branch `feature/virtual-cash-story-2` from `main`; if Story 1 open, rebase onto it | ✅ Answered |

## Risks and mitigations

| Risk | Likelihood | Impact | Mitigation | Owner task |
|------|------------|--------|------------|----------|
| Story 1 not merged — duplicate UI work | M | L | Rebase on `feature/virtual-cash-story-1` or merge Story 1 first | Prerequisites |
| Test seed violates DB check constraints | L | M | Keep `reserved ≤ total`, non-negative | Task 1 |
| Client recomputes available and masks API bugs | L | H | Single display module; grep guard in Task 3 | Task 3 |
| Operators cannot manually see $5k reserved without SQL | M | L | Document Aspire + SQL seed in manual checklist | Task 4 |

## Prerequisites

- [x] Story 1 automation complete (`VirtualCashCard`, `GetMyWalletTests`, top bar chip)
- [ ] Story 1 merged to `main` **or** branch `feature/virtual-cash-story-2` based on Story 1 branch
- [ ] Docker available for Api integration tests
- [ ] Spec §13: no open questions block Story 2 (Q1 top bar is Story 1 only)

## File structure (planned)

```text
tests/Api.IntegrationTests/Users/GetMyWalletTests.cs   MODIFY (+2 reserved scenarios)

web/src/features/trading/
  wallet-display.ts                                    MODIFY
  components/virtual-cash-card.tsx                     MODIFY
web/src/hooks/use-session.ts                           MODIFY (normalizeWallet doc)
web/scripts/check-wallet-display-integrity.mjs         CREATE
web/package.json                                       MODIFY (lint:wallet-integrity)

docs/memory/current-status.md                          MODIFY
docs/CHANGELOG.md                                      MODIFY
```

## Authorization, session, and domain notes

- **Session model:** Unchanged — authenticated `GET /api/wallet` only.
- **Domain rules (display must reflect):**
  - **BR-02:** Server computes `availableBalance`; UI shows API value.
  - **BR-04:** Reserved > 0 copy references open buy orders (simulation); no order API in this story.
- **BR-03:** Inconsistent triple is a **defect**; UI must not “fix” by recalculating on the client.

## Progress tracker

### Task 1: API integration test for non-zero reserved balance

| Attribute | Value |
|-----------|--------|
| Spec story | Story 2 |
| Depends on | None (requires Story 1 files on branch) |
| Estimated complexity | M |
| Parent story issue | #35 |

#### Objective

Prove `GET /api/wallet` returns correct **total**, **reserved**, and **available** when `reserved_balance` is seeded in PostgreSQL after registration (simulates future open buy reserves).

#### Implementation notes

- Extend `GetMyWalletTests` (same fixture/collection as Story 1).
- Flow: register + session cookie → resolve `userId` from response → open `ApplicationDatabaseContext` scope → `ExecuteUpdateAsync` on `wallets` (`TotalBalance = 50_000m`, `ReservedBalance = 10_000m`) → GET `/api/wallet`.
- Assert: `TotalBalance` 50_000, `ReservedBalance` 10_000, `AvailableBalance` 40_000, and `AvailableBalance == TotalBalance - ReservedBalance`.
- **EC-02 variant:** Either a second fact or parameterized case: total 100_000, reserved 5_000, available 95_000.
- Pattern for DB access: `fixture.Factory.Services.CreateAsyncScope()` + `ApplicationDatabaseContext` (see `LoginUserTests`).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `tests/Api.IntegrationTests/Users/GetMyWalletTests.cs` | Reserved breakdown test(s) |
| REUSE | `tests/Api.IntegrationTests/Integration/IntegrationTestFixture.cs` | Testcontainers factory |
| REUSE | `src/Infrastructure/Persistence/Entities/WalletRecord.cs` | Seed target |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `GetMyWallet_WithSeededReserved_ReturnsCorrectBreakdown` | `GetMyWalletTests.cs` |
| Integration | `GetMyWallet_WithSeeded5kReserved_ReturnsEc02Breakdown` (or combined) | `GetMyWalletTests.cs` |

#### Acceptance criteria

- [x] New test(s) pass under Docker Testcontainers
- [x] Existing `GetMyWalletTests` still green
- [x] `availableBalance` equals `totalBalance - reservedBalance` on 200 response

#### Notes (Task 1)

- Wallet seed uses `ExecuteUpdateAsync` (not load + `SaveChangesAsync`) so `row_version` concurrency on `wallets` does not block updates in tests.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | FR-6.2; Tech §6; DB §4.2 constraints |
| PostgreSQL authoritative | Direct row update in test |
| RFC 7807 errors | N/A |
| Aspire | None |
| ADR needed? | No |

#### Risk

Low — test-only; respect check constraints.

---

### Task 2: Story 2 cash card UX (breakdown emphasis and zero reserved)

| Attribute | Value |
|-----------|--------|
| Spec story | Story 2 |
| Depends on | Task 1 |
| Estimated complexity | M |
| Parent story issue | #35 |

#### Objective

`VirtualCashCard` clearly communicates total vs reserved vs available per spec: secondary breakdown always visible; when reserved is zero, show **$0.00** plus short copy; when reserved > 0, add subtle helper text tying reserved to open buys.

#### Implementation notes

- **MODIFY** `wallet-display.ts`:
  - `formatWalletBreakdownLine(total, reserved)` → `Total $X · Reserved $Y` using `formatUsd`.
  - `ZERO_RESERVED_HELPER` → e.g. `None tied up in open buy orders`.
  - `RESERVED_HELPER(reserved)` → e.g. `Open buy orders hold {amount}` when `reserved > 0`.
- **MODIFY** `virtual-cash-card.tsx`:
  - Primary: `formatUsd(wallet.availableBalance)` (unchanged).
  - Secondary: breakdown line from helper (always render when wallet success).
  - Tertiary: helper copy — zero vs non-zero reserved.
  - Use `tabular-nums` on all monetary text; `text-muted-foreground` for secondary/tertiary.
  - **Do not** compute `wallet.totalBalance - wallet.reservedBalance` for display.
- Top bar `WalletTopBarChip`: **no change** (available only).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `web/src/features/trading/wallet-display.ts` | Shared breakdown + copy constants |
| MODIFY | `web/src/features/trading/components/virtual-cash-card.tsx` | Story 2 layout/copy |
| REUSE | `web/src/lib/format.ts` | `formatUsd` two decimals |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Seed or test user with reserved → card shows all three amounts + helper | Aspire |
| Manual | New user → reserved **$0.00** + zero helper visible | Aspire |

#### Acceptance criteria

- [x] Happy path: $50k / $10k / $40k readable on card (manual or after Task 1 seed + UI refresh)
- [x] Reserved $0 shows **$0.00** and zero-reserved helper text
- [x] Reserved > 0 shows non-zero helper referencing open buys
- [x] `yarn --cwd web build` passes

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| design-system.mdc | Card, muted secondary, tabular nums |
| frontend.mdc | No wallet state in Zustand |

#### Risk

Low — UI-only on trading card.

---

### Task 3: Display integrity (no client-side recompute)

| Attribute | Value |
|-----------|--------|
| Spec story | Story 2 (failure / edge path) |
| Depends on | Task 2 |
| Estimated complexity | S |
| Parent story issue | #35 |

#### Objective

Ensure the cash card never invents a fourth balance by recomputing available on the client; document verification for the hypothetical inconsistent-API defect.

#### Implementation notes

- Audit `web/src/features/trading/**` and `normalizeWallet` — confirm `availableBalance` comes from API field only (already true in `use-session.ts`).
- Add one-line module comment in `wallet-display.ts`: display layer uses API triple as-is.
- **Manual verification (operator):** Temporarily render `VirtualCashCard` with props `{ availableBalance: 1, totalBalance: 100000, reservedBalance: 5000 }` (e.g. React DevTools or short-lived dev branch) — card must show **$1.00** available, not **$95,000.00**.
- Optional grep CI guard: no `totalBalance - reservedBalance` in `web/src/features/trading/` (document in Task 4 checklist if not automated).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `web/src/features/trading/wallet-display.ts` | Integrity comment |
| MODIFY | `web/src/hooks/use-session.ts` | `normalizeWallet` doc comment |
| MODIFY | `web/scripts/check-wallet-display-integrity.mjs` | Grep guard for client recompute |
| MODIFY | `web/package.json` | `lint:wallet-integrity` script |
| REUSE | `web/src/features/trading/components/virtual-cash-card.tsx` | Must use `wallet.availableBalance` only |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Mock inconsistent triple → UI shows API available, not derived | DevTools / temporary props |

#### Acceptance criteria

- [x] No `total - reserved` (or similar) in `VirtualCashCard` or `wallet-display` formatters
- [x] Manual inconsistent triple shows three API values without correction
- [x] `normalizeWallet` does not overwrite `availableBalance`

#### Notes (Task 3)

- Audit: `normalizeWallet` maps `availableBalance` from API; `VirtualCashCard` / `WalletTopBarChip` display API fields only.
- Automated guard: `yarn --cwd web lint:wallet-integrity` (`web/scripts/check-wallet-display-integrity.mjs`).
- Manual integrity check (operator): DevTools props `{ availableBalance: 1, totalBalance: 100000, reservedBalance: 5000 }` → card shows **$1.00** available (plan §Manual UI checklist item 4).

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| BR-03 | Defect is server-side; client must not mask |

#### Risk

None — documentation and review.

---

### Task 4: Polish, regression, and manual sign-off

| Attribute | Value |
|-----------|--------|
| Spec story | Story 2 \| Polish |
| Depends on | Tasks 1–3 |
| Estimated complexity | S |
| Parent story issue | #35 |

#### Objective

Regression suite green, Story 2 manual checklist complete, tracking docs updated; branch ready for PR closing #35.

#### Implementation notes

- Run `dotnet test` on `Api.IntegrationTests` (Users + wallet tests).
- Run `yarn --cwd web lint:wallet-integrity`, `yarn --cwd web lint`, and `yarn --cwd web build`.
- Confirm Story 1 behaviors unchanged (top bar chip, loading/error, $100k new user).
- Update `docs/CHANGELOG.md` and `docs/memory/current-status.md` after `/build`.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `docs/CHANGELOG.md` | Impl entry |
| MODIFY | `docs/memory/current-status.md` | Next up / completed |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | Full Users Testcontainers suite | — |
| Manual | Story 2 checklist below | — |

#### Acceptance criteria

- [x] All automated tests green (**56** Users Testcontainers; excludes `RegisterUserSessionTests` — local Postgres)
- [ ] Manual Story 2 checklist signed off by operator (see §Manual UI checklist)
- [x] No regression to Story 1 wallet loading/error/top bar (`GetMyWalletTests` 5/5; `VirtualCashCard` / `WalletTopBarChip` unchanged paths)

#### Notes (Task 4)

- Regression: `yarn lint:wallet-integrity`, `yarn lint`, `yarn build` green.
- PR target: `feature/virtual-cash-story-2` → `main` (closes #35 when merged).

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| Epic | BR-02, BR-04, BR-05 unchanged |

#### Risk

None.

## Manual UI checklist (operator)

Run on Aspire (`feature/virtual-cash-story-2`). Sign off when all pass. Use integration test seed or SQL:

```sql
-- Example after identifying user_id
UPDATE wallets SET total_balance = 50000, reserved_balance = 10000 WHERE user_id = '<uuid>';
```

1. Login → **Trading** → card shows **$40,000.00** available, secondary **Total $50,000.00 · Reserved $10,000.00**, helper mentions open buy holds.
2. New user (or reserved reset to 0) → reserved **$0.00** and “none tied up” (or equivalent) visible.
3. Top bar still shows **available only** — no total/reserved in chip.
4. (Integrity) With mocked inconsistent props in dev only — available shows API value, not total − reserved.
5. Regression: wallet **500** still shows error copy, no fake balances (Story 1).

## Reference files

| File | Why open it |
|------|-------------|
| `docs/specs/20260525-201500-virtual-cash-balance.md` | Story 2 AC |
| `docs/plans/20260525-203000-virtual-cash-story-1.md` | Prior slice + patterns |
| `web/src/features/trading/components/virtual-cash-card.tsx` | Current breakdown UI |
| `web/src/features/trading/wallet-display.ts` | Error copy; extend for breakdown |
| `tests/Api.IntegrationTests/Users/GetMyWalletTests.cs` | Add reserved test |
| `tests/Api.IntegrationTests/Users/LoginUserTests.cs` | EF scope pattern |
| `src/Application/Users/Queries/GetMyWalletQueryHandler.cs` | Server-side available math |

## Implementation details (for /build)

### Backend

- No production code changes expected. Handler already returns:
  ```csharp
  new WalletResponse(..., wallet.TotalBalance, wallet.ReservedBalance,
      wallet.TotalBalance - wallet.ReservedBalance);
  ```

### Test seed sketch

```csharp
await using var scope = fixture.Factory.Services.CreateAsyncScope();
var databaseContext = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseContext>();
await databaseContext.Wallets
    .Where(w => w.UserId == userId)
    .ExecuteUpdateAsync(s => s
        .SetProperty(w => w.TotalBalance, 50_000m)
        .SetProperty(w => w.ReservedBalance, 10_000m));
```

### Frontend display rules

| Field | Source | Never |
|-------|--------|-------|
| Available (large) | `wallet.availableBalance` | `total - reserved` on client |
| Total / Reserved line | `wallet.totalBalance`, `wallet.reservedBalance` | Combined “spendable” fourth line |
| Helpers | Derived from `reservedBalance === 0` only | Monetary math |

### Story 1 coexistence

- `useWalletQuery` + `queryKey: ['wallet']` unchanged.
- `WalletTopBarChip` unchanged (available-only per NG4).

## Verification matrix (plan-level)

| Spec AC | Verified by |
|---------|-------------|
| $50k total, $10k reserved → $40k available + secondary line | Task 1 + Task 2 manual |
| Reserved $0 visible with clear copy | Task 2 manual |
| Inconsistent API triple — show three fields, no fourth | Task 3 manual + review |
| EC-02 $5k reserved all visible | Task 1 integration |
| BR-02 on API | Task 1 integration |
| No Story 1 regression | Task 4 |

## Rollback / recovery

- **Code:** revert `feature/virtual-cash-story-2`
- **DB:** N/A (no migration)
- **Redis:** N/A

## Deferred work (Plan B)

- Story 3: cross-user wallet cache (#36)
- Story 4: refetch-on-focus (#37)
- Live reserved updates via order placement (US-10+)
- SignalR wallet push
- Optional Playwright component test for inconsistent triple

## GitHub Links

| Local ref | Issue # | Type | Title | URL |
|-----------|---------|------|-------|-----|
| spec Story 2 | 35 | Story | US-03 / Story 2: Understand total versus reserved cash | https://github.com/tranvuongduy2003/trading-simulator/issues/35 |
| spec Story 1 | 34 | Story | US-03 / Story 1: See how much cash I can trade with | https://github.com/tranvuongduy2003/trading-simulator/issues/34 |
| spec epic | 33 | Epic | Spec: Virtual cash balance display (US-03) | https://github.com/tranvuongduy2003/trading-simulator/issues/33 |
