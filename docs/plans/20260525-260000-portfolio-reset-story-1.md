---
artifact_type: plan
artifact_version: 1
id: plan-20260525-260000-portfolio-reset-story-1
title: Portfolio Reset — Story 1 (Confirm before resetting)
slug: portfolio-reset-story-1
filename_template: 20260525-260000-portfolio-reset-story-1.md
created_at: 2026-05-25T26:00:00+07:00
updated_at: 2026-05-25T26:00:00+07:00
status: completed
owner: engineering
tags: [plan, implementation, trading-simulator, portfolio-reset, confirmation, us-04, story-1]
related_spec: docs/specs/20260525-251500-portfolio-reset.md
related_plans: []
prd_refs: [PRD §5.1 US-04, PRD §6.1 FR-1.4, PRD §7.4, PRD §8.1]
tech_refs: [Tech §6 ResetPortfolioCommand, Tech §8.1 Portfolio endpoints, Tech §15.1, Tech §16 Trading:PortfolioResetCooldownMinutes]
db_refs: [DB §4.10 portfolio_resets]
github:
  repo: tranvuongduy2003/trading-simulator
  epic_issue: 43
  story_issue_ids: [44]
  last_synced_at: 2026-05-25T26:00:00+07:00
search_index:
  keywords: [portfolio reset, confirmation dialog, POST api portfolio reset, ResetPortfolioCommand, alert-dialog, UserMenu, double-submit, RESET_IN_PROGRESS, nextEligibleAt, contract stub, US-04 story-1]
  bounded_contexts: [Trading]
  task_count: 5
---

# Implementation Plan: Portfolio Reset — Story 1

| Field | Value |
|-------|--------|
| Spec | `docs/specs/20260525-251500-portfolio-reset.md` (§2 Story 1) |
| GitHub story | [#44 — Confirm before resetting](https://github.com/tranvuongduy2003/trading-simulator/issues/44) |
| Epic | [#43 — Portfolio reset (US-04)](https://github.com/tranvuongduy2003/trading-simulator/issues/43) |
| Depends on | US-01 registration, US-02 login/session, US-03 wallet display (`useWalletQuery`, `AppLayout` user menu) — **already shipped** |
| Status | COMPLETE (automation) — manual UI checklist pending operator |
| Tasks | 5 |
| Branch | `feature/portfolio-reset-story-1` |
| Aspire impact | No |
| Schema impact | No |
| Test levels | API integration (auth + response contract) · Manual UI |
| ADRs required | ADR-005 (Story 1 contract stub — see below) |
| GitHub | Synced 2026-05-25 — see §GitHub Links |

## Executive summary

Story 1 (US-04) delivers the **destructive-action guardrail**: an authenticated user opens **Reset portfolio** from the account menu, reads a confirmation dialog listing all consequences, and only on confirm calls `POST /api/portfolio/reset` with loading on the confirm button. Cancel/dismiss must not hit the API. Success closes the dialog, shows success copy, and disables the menu entry until `nextEligibleAt` from the response (full server cooldown enforcement is Story 4; wallet/order mutations are Stories 2–3). The backend for this story is intentionally a **contract stub**—it returns the spec-shaped **200** body from the current wallet read and computed timestamps **without** writing `portfolio_resets` or changing balances, so Story 1 can be tested and merged before the atomic reset transaction lands in Story 2.

## Goals and non-goals

**Goals**

- G1: **Reset portfolio** entry in `AppLayout` user menu on Trading / Portfolio / Orders routes (≤2 clicks from trading view).
- G2: Confirmation dialog lists $100k cash, zero AAPL holdings, open orders cancelled, personal history cleared, 24h cooldown (copy only in Story 1).
- G3: Confirm triggers `POST /api/portfolio/reset` with session cookie, loading/disabled confirm until response (≤2 s local).
- G4: Cancel, Escape, or overlay dismiss → no POST; balances unchanged on refetch.
- G5: Double-submit → client disables confirm; optional server **409** `RESET_IN_PROGRESS` via in-flight guard.
- G6: Integration tests: **401** without session, **200** with session and response shape (`resetAt`, `nextEligibleAt`, `wallet`).

**Non-goals**

- NG1: PostgreSQL reset transaction, `portfolio_resets` insert, order cancel, holdings clear (Stories 2–3).
- NG2: Server cooldown **422** `RESET_COOLDOWN_ACTIVE` (Story 4).
- NG3: TanStack invalidation of wallet/portfolio/orders/trades (Story 5).
- NG4: Matching engine / Redis book updates (Stories 2–3).
- NG5: SignalR notifications on reset.
- NG6: Dedicated `GET /api/portfolio/reset/eligibility` (Story 4 may add; Story 1 uses `nextEligibleAt` from POST success).
- NG7: Domain `Wallet.Reset` / `Portfolio.ClearHoldings` unit tests (Story 2).

## Traceability matrix

| Spec story | Plan task(s) | Test evidence |
|------------|--------------|---------------|
| Story 1 — menu → confirmation dialog | Task 1, 2 | Manual: consequences list visible |
| Story 1 — confirm → POST within 2s + loading | Task 1, 3 | Manual; `ResetPortfolio_WithSession_Returns200_WithContractShape` |
| Story 1 — success → toast + disable reset until eligible | Task 4 | Manual; client uses `nextEligibleAt` |
| Story 1 — cancel/dismiss → no API | Task 2 | Manual; no POST in network tab |
| Story 1 — double-click → single in-flight | Task 3 | Manual; optional `ResetPortfolio_Concurrent_Returns409` |
| BR-01 user-initiated only | Task 1 | Authenticated POST only |
| BR-05 session valid after reset | Task 1 | No logout on success (stub does not touch session) |
| EC-07 session expires on confirm → 401 | Task 3 | `ResetPortfolio_WithoutSession_Returns401`; manual |
| `portfolio_resets` row on success | — | **Deferred to Story 2** (stub writes nothing) |

## Architecture impact

```text
┌─────────────────────────────────────────────────────────────────┐
│ AppLayout / UserMenu                                             │
│  "Reset portfolio" → AlertDialog (consequences)                  │
│  Confirm → useResetPortfolioMutation → POST /api/portfolio/reset │
│  Success → toast + disable menu until nextEligibleAt (client)  │
└────────────────────────────┬────────────────────────────────────┘
                             │ session cookie
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│ Api PortfolioEndpoint POST                                       │
│  → ResetPortfolioCommand → ResetPortfolioCommandHandler (STUB)   │
│     reads wallet via IWalletReadRepository                       │
│     returns 200 { resetAt, nextEligibleAt, wallet } — no UoW     │
└─────────────────────────────────────────────────────────────────┘
```

| Layer | Change summary |
|-------|----------------|
| Domain | **None** in Story 1 (reset methods added in Story 2) |
| Application | **CREATE** `ResetPortfolioCommand`, stub `ResetPortfolioCommandHandler`, validator (empty body), `PortfolioResetErrors` |
| Infrastructure | **REUSE** `IWalletReadRepository`; optional `IResetInFlightGuard` (in-memory singleton for 409) |
| Contracts | **CREATE** `PortfolioResetResponse` (+ nested wallet snapshot if not reusing `WalletResponse`) |
| Api | **MODIFY** `PortfolioEndpoint` — add POST; **MODIFY** `ApiErrorCodes` / problem mapping for new codes |
| MatchingEngine | None |
| web/ | **CREATE** `features/portfolio-reset/` (dialog, mutation, api); **MODIFY** `app-layout.tsx` UserMenu; add shadcn `alert-dialog` |
| AppHost | None |

## Data & migration plan

| Artifact | Action | DB reference |
|----------|--------|--------------|
| EF migration | **None** | `portfolio_resets` table already exists (DB §4.10) |
| Redis keys | **None** | — |
| Book recovery | N/A | — |
| Story 1 writes | **None** | Spec: insert `portfolio_resets` on success starts Story 2+ |

## Open questions

| # | Question | Source | Answer | Status |
|---|----------|--------|--------|--------|
| 1 | Trade history cleared without deleting `trades` rows? | Spec §13 Q1 | Soft cutoff on reads — implement in Story 3; document ADR then | ⏳ Deferred (Story 3 plan) |
| 2 | Empty order history after reset? | Spec §13 Q2 | Yes — Story 3 read model | ⏳ Deferred |
| 3 | **200 + body** vs **204**? | Spec §13 Q3 | **200** with `resetAt`, `nextEligibleAt`, `wallet` (matches §4b assumption) | ✅ Answered (this plan) |
| 4 | Eligibility GET vs infer from POST? | Spec §13 Q4 | Story 1: cache `nextEligibleAt` from last success; Story 4 adds server cooldown + disabled UI without POST | ✅ Answered (this plan) |
| 5 | Story 1 stub acceptable for `/build`? | Code review | Yes — ADR-005; manual checklist notes wallet unchanged until Story 2 | ✅ Answered (this plan) |

## Risks and mitigations

| Risk | Likelihood | Impact | Mitigation | Owner task |
|------|------------|--------|------------|------------|
| Users think reset worked but wallet unchanged (stub) | M | M | Success toast + manual checklist step; merge Story 2 before epic close | Task 5 |
| Missing `RequireAuthorization` on POST | L | H | Mirror `GET /api/portfolio` + handler `ICurrentUserAccessor` check | Task 1 |
| No `alert-dialog` in repo | L | L | `yarn --cwd web dlx shadcn@latest add alert-dialog` | Task 1 |
| Order aggregate not in domain yet | M | L | Stub handler does not touch orders; Story 3 depends on order epic or parallel work | — |
| 409 guard leaks across test hosts | L | M | Register in-memory guard as singleton; reset guard in test factory if needed | Task 3 |

## Prerequisites

- [x] Spec approved for planning (epic #43 / story #44 open)
- [ ] Aspire local stack runs (`aspire run` or env-doctor)
- [x] Schema includes `portfolio_resets` (initial migration applied)
- [x] US-02 session + US-03 wallet query patterns available

## File structure (planned)

```text
src/
  TradingSimulator.Contracts/Portfolio/
    PortfolioResetResponse.cs          CREATE
  TradingSimulator.Application/Portfolios/Commands/
    ResetPortfolioCommand.cs           CREATE
    ResetPortfolioCommandHandler.cs    CREATE (stub)
    ResetPortfolioCommandValidator.cs  CREATE
    PortfolioResetErrors.cs            CREATE
  TradingSimulator.Application/Abstractions/Portfolio/
    IResetInFlightGuard.cs             CREATE (optional)
  TradingSimulator.Infrastructure/Portfolio/
    InMemoryResetInFlightGuard.cs      CREATE (optional)
  TradingSimulator.Api/Endpoints/
    PortfolioEndpoint.cs               MODIFY (POST)
  TradingSimulator.Api/Common/
    ApiErrorCodes.cs                   MODIFY (RESET_* codes if centralized)
web/src/
  components/ui/alert-dialog.tsx         CREATE (shadcn)
  features/portfolio-reset/
    api.ts                               CREATE
    reset-portfolio-dialog.tsx           CREATE
    use-reset-portfolio.ts               CREATE
    map-reset-error.ts                   CREATE
    reset-eligibility.ts                 CREATE (nextEligibleAt client cache)
  layouts/app-layout.tsx                 MODIFY (menu item + dialog)
contracts/openapi/api.v1.yaml            MODIFY (via api:export)
tests/TradingSimulator.Api.IntegrationTests/Portfolios/
  ResetPortfolioTests.cs                 CREATE
docs/memory/decisions.md                 MODIFY (ADR-005)
```

## Authorization, session, and domain notes

- **Session model:** Cookie session (ADR-001). `POST /api/portfolio/reset` requires authenticated user; **401** `UNAUTHORIZED` when `ICurrentUserAccessor.UserId` is null (EC-07).
- **Route protection:** `.RequireAuthorization()` on POST endpoint (align with `GET /api/portfolio`). Handler still returns structured **401** for consistency with `GetMyWallet`.
- **Domain rules (Story 1):** BR-01 (user-initiated entry point only), BR-05 (no session invalidation on success). Do **not** implement BR-02–BR-08 persistence in this story.
- **Stub response contract** (spec §4b):

```json
{
  "resetAt": "<ISO-8601>",
  "nextEligibleAt": "<ISO-8601>",
  "wallet": {
    "totalBalance": 100000.0000,
    "reservedBalance": 0.0000,
    "availableBalance": 100000.0000,
    "currency": "USD"
  }
}
```

  Stub sets `resetAt` = `IClock.UtcNow`, `nextEligibleAt` = `resetAt + Trading:PortfolioResetCooldownMinutes`, and `wallet` from **current** `IWalletReadRepository` read (not forced $100k) so integrators see honest pre-reset figures until Story 2.

## Progress tracker

### Task 1: Wire reset POST skeleton and menu entry

| Attribute | Value |
|-----------|--------|
| Spec story | Story 1 |
| Depends on | None |
| Estimated complexity | M |
| Parent story issue | #44 |

#### Objective

An authenticated user sees **Reset portfolio** in the account menu; opening it shows a dialog shell; confirming calls `POST /api/portfolio/reset` and receives **200** with the contract body. Unauthenticated POST returns **401**. No PostgreSQL mutations yet.

#### Implementation notes

- Add `ResetPortfolioCommand` (no properties) and stub handler: resolve `userId`, read wallet, compute `nextEligibleAt` from `IOptions<TradingOptions>.PortfolioResetCooldownMinutes`, return `PortfolioResetResponse`.
- Extend `PortfolioEndpoint` with `MapPost("/api/portfolio/reset", ...)` + OpenAPI metadata (Produces 200, 401, 409, 500).
- Register handler via existing MediatR `AssemblyReference` scan.
- Add shadcn `alert-dialog`; create `ResetPortfolioDialog` with placeholder consequence bullets and Confirm/Cancel.
- `useResetPortfolio` mutation: `apiClient.post` with `credentials: 'include'`.
- Run `yarn --cwd web api:export` and commit `contracts/openapi/api.v1.yaml`.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `src/TradingSimulator.Contracts/Portfolio/PortfolioResetResponse.cs` | Response DTO |
| CREATE | `src/TradingSimulator.Application/Portfolios/Commands/ResetPortfolioCommand.cs` | Command |
| CREATE | `src/TradingSimulator.Application/Portfolios/Commands/ResetPortfolioCommandHandler.cs` | Stub handler |
| CREATE | `src/TradingSimulator.Application/Portfolios/Commands/ResetPortfolioCommandValidator.cs` | Empty command validator |
| MODIFY | `src/TradingSimulator.Api/Endpoints/PortfolioEndpoint.cs` | POST route |
| CREATE | `web/src/components/ui/alert-dialog.tsx` | shadcn primitive |
| CREATE | `web/src/features/portfolio-reset/api.ts` | POST client |
| CREATE | `web/src/features/portfolio-reset/reset-portfolio-dialog.tsx` | Dialog shell |
| CREATE | `web/src/features/portfolio-reset/use-reset-portfolio.ts` | Mutation hook |
| MODIFY | `web/src/layouts/app-layout.tsx` | Menu item + dialog trigger |
| MODIFY | `contracts/openapi/api.v1.yaml` | Exported contract |
| REUSE | `src/TradingSimulator.Application/Users/Queries/GetMyWalletQueryHandler.cs` | Auth + wallet read pattern |
| REUSE | `web/src/features/auth/use-logout.ts` | Mutation + toast pattern |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `ResetPortfolio_WithoutSession_Returns401_UNAUTHORIZED` | `tests/.../Portfolios/ResetPortfolioTests.cs` |
| Integration | `ResetPortfolio_WithSession_Returns200_WithContractShape` | same |
| Manual | Menu item visible when logged in; confirm returns 200 in network tab | `web/` |

#### Acceptance criteria

- [x] `POST /api/portfolio/reset` exists and is named in OpenAPI `ResetPortfolio`
- [x] Authenticated integration test passes with `resetAt`, `nextEligibleAt`, `wallet` fields
- [x] Unauthenticated request returns **401** (empty body from `RequireAuthorization` middleware; handler would return `UNAUTHORIZED` problem if reached)
- [x] User menu shows **Reset portfolio** on authenticated layout
- [x] Confirm in dialog triggers POST (observable in DevTools)

#### Notes (Task 1 implementation)

- shadcn CLI wrote to `web/@/components/ui/`; component copied to `web/src/components/ui/alert-dialog.tsx`.
- Stub handler: no PostgreSQL writes (ADR-005 deferred to Task 5).
- Dialog copy expanded to five bullets in Task 2 (`reset-consequences.ts`).

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | PRD §5.1 US-04; Tech §6, §8.1; DB §4.10 (no write) |
| Async matching | N/A — stub does not enqueue |
| PostgreSQL authoritative | No writes in Story 1 |
| Redis projection | N/A |
| RFC 7807 errors | **401** on missing session |
| SignalR | N/A |
| Aspire | None |
| ADR needed? | **Yes — ADR-005** (stub) |

#### Risk

Stub may confuse QA if Story 2 not merged soon — document in manual checklist.

---

### Task 2: Confirmation copy and dismiss without API

| Attribute | Value |
|-----------|--------|
| Spec story | Story 1 |
| Depends on | Task 1 |
| Estimated complexity | S |
| Parent story issue | #44 |

#### Objective

Dialog shows the full consequence list from the spec (cash, holdings, orders, history, 24h cooldown). Cancel button, dialog close control, and Escape dismiss the dialog without any `POST`.

#### Implementation notes

- Use `AlertDialog` controlled `open` / `onOpenChange` from `UserMenu` parent or internal state; set `open = false` on cancel without calling mutation.
- Destructive styling on Confirm (not on menu item — menu item is neutral; confirm button is destructive).
- Keep dialog open until success or error from POST (do not close on confirm click before response — Task 3).

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `web/src/features/portfolio-reset/reset-portfolio-dialog.tsx` | Full copy + dismiss handlers |
| MODIFY | `web/src/layouts/app-layout.tsx` | Wire open state |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | Open dialog → Cancel → no `portfolio/reset` in network | `web/` |
| Manual | Open → click outside / Escape → no POST | `web/` |

#### Acceptance criteria

- [x] All five consequence bullets visible (100k, zero AAPL, cancel opens, history cleared, 24h cooldown)
- [x] Cancel/dismiss closes dialog; wallet unchanged on subsequent `GET /api/wallet`

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD / Tech / DB | PRD §7.4 reachability |
| RFC 7807 | N/A |
| ADR needed? | No |

#### Risk

None — UI-only.

---

### Task 3: Loading, double-submit, and error mapping

| Attribute | Value |
|-----------|--------|
| Spec story | Story 1 |
| Depends on | Task 2 |
| Estimated complexity | M |
| Parent story issue | #44 |

#### Objective

Confirm button shows loading and stays disabled while POST is in flight. A second click does not fire a second request. API errors map to user-visible messages: `UNAUTHORIZED`, `INTERNAL_ERROR`, `RESET_IN_PROGRESS` (optional **409**).

#### Implementation notes

- `useMutation` `isPending` disables Confirm and menu trigger.
- Optional `IResetInFlightGuard`: per-`UserId` flag set at handler start, cleared in `finally`; second concurrent call returns `Error.Conflict("RESET_IN_PROGRESS", ...)`.
- `map-reset-error.ts` mirrors `map-register-error` / wallet patterns; **401** → clear session or redirect via existing auth interceptor.
- Do not close dialog on error; allow retry after message.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `web/src/features/portfolio-reset/map-reset-error.ts` | Error copy |
| MODIFY | `web/src/features/portfolio-reset/use-reset-portfolio.ts` | Pending + onError |
| MODIFY | `web/src/features/portfolio-reset/reset-portfolio-dialog.tsx` | Loading UI |
| CREATE | `src/TradingSimulator.Application/Abstractions/Portfolios/IResetInFlightGuard.cs` | Optional (namespace `Portfolios` avoids clash with domain `Portfolio`) |
| CREATE | `src/TradingSimulator.Infrastructure/Portfolios/InMemoryResetInFlightGuard.cs` | Optional |
| MODIFY | `src/TradingSimulator.Application/Portfolios/Commands/ResetPortfolioCommandHandler.cs` | Guard usage |
| MODIFY | `src/TradingSimulator.Infrastructure/DependencyInjection.cs` | Register guard |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | `ResetPortfolio_WhenInFlight_SecondRequestReturns409` (if guard implemented) | `ResetPortfolioTests.cs` |
| Manual | Double-click confirm → one POST | `web/` |
| Manual | Expired session on confirm → 401 message | `web/` |

#### Acceptance criteria

- [x] Confirm disabled during `isPending`
- [x] `UNAUTHORIZED` shows safe message; no reset row (N/A for stub)
- [x] `RESET_IN_PROGRESS` shows spec copy when **409** implemented
- [x] `INTERNAL_ERROR` shows retry copy; dialog stays open

#### Notes (Task 3 implementation)

- Guard interface/folder use `Abstractions/Portfolios` and `Infrastructure/Portfolios` (not `Portfolio/`) to avoid C# namespace clash with `TradingSimulator.Domain.Portfolios.Portfolio`.
- `UserMenu` owns `useResetPortfolio`; dialog receives `isPending`, `errorMessage`, `onConfirm`.
- Dialog blocks dismiss (Cancel, Escape, overlay) while `isPending`; closes only on success.
- Integration test uses `DelayingWalletReadRepository` (300 ms) to overlap concurrent POSTs for **409**.

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| RFC 7807 | **409** Conflict, **401**, **500** |
| Async matching | N/A |
| ADR needed? | No |

#### Risk

In-memory guard is per-process only — acceptable for local MVP; Story 2 may replace with DB-level idempotency.

---

### Task 4: Success UX and disable reset until nextEligibleAt

| Attribute | Value |
|-----------|--------|
| Spec story | Story 1 (disable preview for Story 4) |
| Depends on | Task 3 |
| Estimated complexity | S |
| Parent story issue | #44 |

#### Objective

On **200**, dialog closes, success toast shows (“Portfolio reset. You're starting fresh with $100,000.” per spec — clarify in toast that positions/orders update in a follow-up if stub), and **Reset portfolio** menu item is disabled until `nextEligibleAt` from the response (persist in `sessionStorage` key `portfolio-reset:nextEligibleAt` scoped by `userId`).

#### Implementation notes

- `onSuccess`: `setNextEligibleAt`, close dialog, `toast.success`.
- `reset-eligibility.ts`: `isResetAllowed()`, `saveNextEligibleAt()`, read on menu render.
- Do **not** invalidate `['wallet']` yet (Story 5); optional comment in hook for Story 5.
- Menu item: `disabled={!isResetAllowed() || isPending}` with `title` tooltip showing relative time until eligible.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| CREATE | `web/src/features/portfolio-reset/reset-eligibility.ts` | Client cooldown cache |
| MODIFY | `web/src/features/portfolio-reset/use-reset-portfolio.ts` | onSuccess handler |
| MODIFY | `web/src/layouts/app-layout.tsx` | Disabled menu state |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Manual | After success, menu item disabled; re-enabled after `nextEligibleAt` (adjust clock or wait) | `web/` |

#### Acceptance criteria

- [x] Success toast shown once per successful POST
- [x] Dialog closes only on success
- [x] Reset menu entry disabled until `nextEligibleAt` (client-side)
- [x] Document in manual steps: wallet figures may not change until Story 2

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| PRD | FR-1.4 cooldown messaging preview |
| ADR needed? | No |

#### Risk

Client-only disable does not stop API replay — Story 4 adds server **422**.

---

### Task 5: Polish, ADR-005, and manual verification

| Attribute | Value |
|-----------|--------|
| Spec story | Polish |
| Depends on | Task 4 |
| Estimated complexity | S |
| Parent story issue | #44 |

#### Objective

Record ADR-005, align auth on wallet GET if inconsistent, run `api:verify`, full manual checklist, and smoke regression on login/wallet.

#### Implementation notes

- Append ADR-005 to `docs/memory/decisions.md`: Story 1 stub, Story 2 replaces handler body with DB §10.4 transaction.
- Consider adding `.RequireAuthorization()` to `GET /api/wallet` if missing (consistency only — out of scope unless trivial).
- Run `dotnet test` on `ResetPortfolioTests` + existing `GetMyWalletTests`.

#### Files

| Action | Path | Purpose |
|--------|------|---------|
| MODIFY | `docs/memory/decisions.md` | ADR-005 |
| MODIFY | `src/TradingSimulator.Api/Endpoints/WalletEndpoint.cs` | Optional RequireAuthorization |

#### Tests required

| Level | Test name / scenario | File |
|-------|----------------------|------|
| Integration | Full `ResetPortfolioTests` suite green | `tests/.../Portfolios/` |
| Manual | Checklist below | Aspire |

#### Acceptance criteria

- [x] ADR-005 accepted in `decisions.md` (recorded during Task 1–4 work)
- [x] `yarn --cwd web api:verify` passes
- [x] `dotnet test` passes for integration projects touched (13 `ResetPortfolio` + `GetMyWallet` tests)
- [ ] Manual UI checklist signed off (operator — see §Manual UI checklist)

#### Notes (Task 5 implementation)

- `GET /api/wallet` now uses `.RequireAuthorization()` (aligned with portfolio routes).
- `GetMyWalletTests.AssertUnauthorizedAsync` accepts empty **401** body from auth middleware (same as reset POST).

#### Cross-cutting checks

| Check | Detail |
|-------|--------|
| Aspire | `aspire run` — web + api |
| ADR needed? | ADR-005 completion |

#### Risk

None — isolated change.

## Reference files

| File | Why open it |
|------|-------------|
| `web/src/layouts/app-layout.tsx` | User menu insertion point |
| `web/src/features/auth/use-logout.ts` | Mutation + session pattern |
| `src/Api/Endpoints/PortfolioEndpoint.cs` | Extend with POST |
| `src/Application/Users/Queries/GetMyWalletQueryHandler.cs` | Auth + wallet read |
| `tests/Api.IntegrationTests/Users/GetMyWalletTests.cs` | Session cookie test patterns |
| `docs/plans/20260525-240000-virtual-cash-story-4.md` | Per-story plan structure reference |
| `docs/specs/20260525-251500-portfolio-reset.md` | Story 1 AC verbatim |
| `.cursor/skills/openapi-contract-sync/SKILL.md` | Export/codegen workflow |

## Implementation details (for /build)

### Application

- `ResetPortfolioCommand`: sealed record, no parameters.
- `ResetPortfolioCommandHandler`: inject `ICurrentUserAccessor`, `IWalletReadRepository`, `IClock`, `IOptions<TradingOptions>`, optional `IResetInFlightGuard`.
- Return `Result<PortfolioResetResponse>`; map wallet fields to contract (same decimals as `WalletResponse`).
- Validator: empty rules (command has no fields) — satisfies pipeline convention.

### Api

- POST handler: `sender.Send(new ResetPortfolioCommand())` → `ToHttpResult()`.
- Produces: `PortfolioResetResponse` 200; Problem 401, 409, 500.
- Tags: `Portfolio`.

### Error codes (Application → Api)

| Code | ErrorType | HTTP |
|------|-----------|------|
| `UNAUTHORIZED` | Unauthorized | 401 |
| `RESET_IN_PROGRESS` | Conflict | 409 |
| `INTERNAL_ERROR` | Unexpected | 500 |

(`RESET_COOLDOWN_ACTIVE` added in Story 4.)

### web

- API path: `/api/portfolio/reset` via generated `paths` after codegen.
- Dialog: `@/components/ui/alert-dialog` — `AlertDialog`, `AlertDialogContent`, `AlertDialogHeader`, `AlertDialogFooter`, `AlertDialogCancel`, `AlertDialogAction` (use `onClick` + prevent default if Action auto-closes).
- Feature folder exports from `features/portfolio-reset/index.ts` for clean imports in layout.

### Configuration

- `Trading:PortfolioResetCooldownMinutes` already in `appsettings.json` (1440) — use for `nextEligibleAt` calculation in stub and later real handler.

## Verification matrix (plan-level)

| Spec AC | Verified by |
|---------|-------------|
| Menu → dialog with consequences | Task 2 manual + Task 1 menu |
| Confirm → POST ≤2s with loading | Task 3 manual + integration 200 |
| Success → toast + disabled action | Task 4 manual |
| Cancel/dismiss → no API | Task 2 manual |
| Double-click → single request | Task 3 manual + optional 409 test |
| EC-07 401 on expired session | Task 3 manual + integration 401 |
| BR-05 session remains | Task 1 — no logout on success |

## Rollback / recovery

- **Code:** Revert branch `feature/portfolio-reset-story-1`.
- **DB:** N/A — no migrations.
- **Redis:** N/A.

## Manual UI checklist (operator)

1. Log in → open user menu on **Trading** → **Reset portfolio** visible.
2. Open dialog → read five consequences → **Cancel** → verify no `POST /api/portfolio/reset` in network.
3. Open again → **Confirm** → loading on button → **200** → success toast → dialog closes.
4. **Reset portfolio** menu item disabled; tooltip/hint shows next eligible time.
5. `GET /api/wallet` still shows **pre-reset** balances (stub) — expected until Story 2.
6. Log out → log in as another user → reset menu enabled (eligibility not leaked).
7. Regression: top-bar wallet chip and trading cash card still load.

## Deferred work (Plan B — other stories)

- **Story 2 plan:** Real `ResetPortfolioCommand` — wallet $100k, clear holdings, `portfolio_resets` insert (issue #45).
- **Story 3 plan:** Cancel open orders, history read cutoff, matching channel (issue #46).
- **Story 4 plan:** Server cooldown **422**, disabled UI from server state (issue #47).
- **Story 5 plan:** Query invalidation + SignalR alignment (issue #48).
- **ADR for history cutoff** (spec Q1) in Story 3 plan.

## GitHub Links

> Plan tasks are tracked in this file only. Story issue #44 updated with task checklist.

| Local ref | Issue # | Type | Title | URL |
|-----------|---------|------|-------|-----|
| spec Story 1 | 44 | Story | US-04 / Story 1: Confirm before resetting | https://github.com/tranvuongduy2003/trading-simulator/issues/44 |
| spec epic | 43 | Epic | Spec: Portfolio reset (US-04) | https://github.com/tranvuongduy2003/trading-simulator/issues/43 |
