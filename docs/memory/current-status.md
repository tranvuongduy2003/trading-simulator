# Current Status

Last updated: 2026-05-28

Owner: @tranvuongduy2003

## Active Focus

- Portfolio reset **Story 5** automation **complete** on `feature/portfolio-reset-story-5` — operator manual UI checklist + PR → `main` (GitHub #48)
- Portfolio reset **Story 4** automation **complete** on `feature/portfolio-reset-story-4` — operator manual UI checklist + PR → `main` (closes #47)
- Portfolio reset **Story 3 Tasks 1–5** automation complete on `feature/portfolio-reset-story-3`; manual Aspire checklist handed off to operator (GitHub #46)
- Portfolio reset **Story 2** Tasks 1–4 automation complete on `feature/portfolio-reset-story-2-local`; manual Aspire checklist remains operator step (GitHub #45)
- Portfolio reset **Story 1** automation **complete** on `feature/portfolio-reset-story-1` — operator manual UI checklist + PR → `main` (closes #44)
- Virtual cash balance **Story 4** automation complete on `feature/virtual-cash-story-4` — open PR → `main` (closes #37); operator manual UI checklist pending (plan §Manual UI checklist)
- Virtual cash balance **Story 3** automation complete on `feature/virtual-cash-story-3` — open PR → `main` (closes #36); operator manual UI checklist pending (plan §Manual UI checklist)
- Virtual cash balance **Story 2** automation complete on `feature/virtual-cash-story-2` — open PR → `main` (closes #35); operator manual UI checklist pending (plan §Manual UI checklist)
- Virtual cash balance **Story 1** automation complete on `feature/virtual-cash-story-1` — open PR → `main` (closes #34); operator manual UI checklist pending (plan §Manual UI checklist)

## Latest Completed

- impl: portfolio reset story 5 **Tasks 1–6** (automation) — query invalidation, wallet seed from 200, portfolio/orders/trades hooks, activity tabs, SignalR + logout purge, ADR-008; `ResetPortfolio_PublishesOrderCancellationNotifications` green; `yarn lint`/`build` green; manual Aspire checklist pending (`feature/portfolio-reset-story-5`, #48)
- impl: portfolio reset story 5 **Task 5** — `PortfolioActivityTabs` on trading page (open orders, order history, trade history, holdings); `yarn lint`/`build` green (`feature/portfolio-reset-story-5`, #48)
- impl: portfolio reset story 5 **Task 4** — orders/trades API + `useOpenOrdersQuery`, `useOrderHistoryQuery`, `useTradeHistoryQuery` with `portfolioPanelQueryKeys`; `yarn lint`/`build` green (`feature/portfolio-reset-story-5`, #48)
- impl: portfolio reset story 5 **Task 3** — `usePortfolioQuery` with `['portfolio', userId]`, `staleTime: 0`, `refetchOnWindowFocus`; `trading-page` migrated; `yarn lint`/`build` green (`feature/portfolio-reset-story-5`, #48)
- impl: portfolio reset story 5 **Task 2** — seed wallet from reset 200 via `seedWalletQueryData`, then `invalidatePortfolioPanels`; updated success toast; `yarn lint`/`build` green (`feature/portfolio-reset-story-5`, #48)
- impl: portfolio reset story 5 **Task 1** — `portfolioPanelQueryKeys`, `invalidatePortfolioPanels`, wired in `useResetPortfolio` onSuccess; `yarn lint`/`build` green (`feature/portfolio-reset-story-5`, #48)
- plan: portfolio reset story 5 — consistent data everywhere after reset (`docs/plans/20260528-003204-portfolio-reset-story-5.md`, #48)
- impl: portfolio reset story 4 **Tasks 1–5** (automation) — eligibility API + cooldown enforcement + frontend query/messaging + 22 integration tests; `yarn lint`/`build` green; manual UI checklist in plan §Task 5 (`feature/portfolio-reset-story-4`, #47)
- impl: portfolio reset story 4 **Task 4** — first-reset, cooldown no-mutation, 25h success, latest-reset-row tests; all 22 `ResetPortfolioTests` green (`feature/portfolio-reset-story-4`, #47)
- impl: portfolio reset story 4 **Task 3** — `getResetEligibility` + query-backed `useResetEligibility`, `api:codegen`, `yarn lint`/`build` green, `GetResetEligibility_RequiresAuthentication` test (`feature/portfolio-reset-story-4`, #47)
- impl: portfolio reset story 4 **Task 2** — cooldown enforced in `ResetPortfolioCommandHandler`, `Error.Extensions` + RFC 7807 `nextEligibleAt`, 2 integration tests; build green (`feature/portfolio-reset-story-4`, #47)
- impl: portfolio reset story 4 **Task 1** — `IPortfolioResetReadRepository`, eligibility query/handler, `GET /api/portfolio/reset/eligibility`, OpenAPI + 2 integration tests; build green (`feature/portfolio-reset-story-4`, #47)
- plan: portfolio reset story 4 — respect the 24-hour cooldown (`docs/plans/20260527-231500-portfolio-reset-story-4.md`, GitHub #47)
- plan: portfolio reset story 3 — cancel open orders and clear activity history (`docs/plans/20260527-214600-portfolio-reset-story-3.md`, GitHub #46)
- impl: portfolio reset story 3 **Task 1** — added `/api/orders/open`, `/api/orders/history`, `/api/trades` read slice with reset cutoff filtering + integration tests; targeted tests + solution build green (`feature/portfolio-reset-story-3`)
- impl: portfolio reset story 3 **Task 2** — reset transaction now cancels current-user open orders and keeps reset invariants with integration coverage for cancellation scope + reservation-safe outcomes; targeted tests + solution build green (`feature/portfolio-reset-story-3`)
- impl: portfolio reset story 3 **Task 3** — reset command now publishes cancellation + market book refresh + balance realtime notifications, with integration verification via capturing publisher fake; targeted tests + solution build green (`feature/portfolio-reset-story-3`)
- impl: portfolio reset story 3 **Task 4** — added deterministic edge-case integration coverage for no-data reset, partially-filled history hidden after reset, and counterparty history visibility; targeted tests + solution build green (`feature/portfolio-reset-story-3`)
- impl: portfolio reset story 3 **Task 5** — ADR-007 added for read-cutoff history strategy; focused reset/order/trade integration regression green (20 tests); manual Aspire checklist explicitly handed off (`feature/portfolio-reset-story-3`)
- impl: portfolio reset story 1 **Tasks 1–5** (automation) — POST stub, dialog, 409 guard, eligibility UX, ADR-005, **13** wallet/reset integration tests + `api:verify` + `yarn build`; manual UI checklist pending; branch `feature/portfolio-reset-story-1`
- impl: portfolio reset story 2 **Task 1** — reset handler now uses write repository port, DI wired, build green (`feature/portfolio-reset-story-2-local`)
- impl: portfolio reset story 2 **Task 2** — repository now persists wallet reset + holdings delete + portfolio reset audit; added reset integration scenarios and helper; reset test subset + solution build green (`feature/portfolio-reset-story-2-local`)
- impl: portfolio reset story 2 **Task 3** — added forced-failure write-repository fake and rollback/unauthorized no-write integration coverage; reset test subset + solution build green (`feature/portfolio-reset-story-2-local`)
- impl: portfolio reset story 2 **Task 4** — ADR addendum recorded (ADR-006), contract-shape test aligned to seeded state, EC-02 zero-holdings test added; reset test subset + solution build green (`feature/portfolio-reset-story-2-local`)
- impl: portfolio reset story 1 **Task 4** — success toast, sessionStorage cooldown, disabled menu (`feature/portfolio-reset-story-1`)
- impl: portfolio reset story 1 **Task 3** — in-flight guard 409, error mapping, loading UX (`feature/portfolio-reset-story-1`)
- impl: portfolio reset story 1 **Task 2** — five consequence bullets, dismiss without POST (`feature/portfolio-reset-story-1`)
- impl: portfolio reset story 1 **Task 1** — POST stub, menu + dialog shell, integration tests, OpenAPI (`feature/portfolio-reset-story-1`)
- plan: portfolio reset story 1 — confirm before reset (`docs/plans/20260525-260000-portfolio-reset-story-1.md`, GitHub #44)
- spec: portfolio reset US-04 (`docs/specs/20260525-251500-portfolio-reset.md`)
- impl: virtual cash story 4 **Tasks 1–5** (automation) — login/refetch API tests, post-auth wallet prefetch, `staleTime: 0` + `refetchOnWindowFocus` on wallet query, refresh-path comment, **10** `GetMyWalletTests` + `yarn build` green
- plan: virtual cash balance story 4 — trust after login and refresh (`docs/plans/20260525-240000-virtual-cash-story-4.md`, GitHub #37)
- impl: virtual cash story 3 **Tasks 1–5** (automation) — privacy API tests, cache purge on auth change, user-scoped wallet query, display guards, **8** `GetMyWalletTests` + **59** Users Testcontainers + `yarn build` green
- plan: virtual cash balance story 3 — session-private wallet (`docs/plans/20260525-230000-virtual-cash-story-3.md`, GitHub #36)

## In Progress

- Operator: portfolio reset story 5 manual UI checklist (plan §Manual UI checklist) on Aspire
- Operator: portfolio reset story 4 manual UI checklist (plan §Task 5 Manual UI checklist) on Aspire
- Operator: portfolio reset story 1 manual UI checklist (plan §Manual UI checklist) on Aspire
- Operator: virtual cash story 4 manual UI checklist (plan §Manual UI checklist) on Aspire
- Operator: virtual cash story 3 manual UI checklist (plan §Manual UI checklist) on Aspire
- Operator: virtual cash story 2 manual UI checklist (plan §Manual UI checklist) on Aspire
- Operator: virtual cash story 1 manual UI checklist (plan §Manual UI checklist) on Aspire

## Next Up

- Open PR `feature/portfolio-reset-story-5` → `main` (closes #48 when merged)
- Operator: portfolio reset story 5 manual UI checklist (6 steps in plan §Manual UI checklist) on Aspire
- Open PR `feature/portfolio-reset-story-4` → `main` (closes #47 when merged)
- Operator: portfolio reset story 4 manual UI checklist (5 steps in plan §Task 5)
- Operator: portfolio reset story 3 manual Aspire checklist (Open Orders, Order History, Trade History, Holdings tabs)
- Manual: portfolio reset story 2 Aspire checklist (plan §Manual UI checklist)
- Open PR `feature/portfolio-reset-story-1` → `main` (closes #44 when merged)
- Manual: portfolio reset story 1 checklist (7 steps in plan)
- Open PR `feature/virtual-cash-story-4` → `main` (closes #37 when merged)
- Manual: virtual cash story 4 checklist (6 steps in plan)
- Open PR `feature/virtual-cash-story-3` → `main` (closes #36 when merged)
- Open PR `feature/virtual-cash-story-2` → `main` (closes #35 when merged)
- Open PR `feature/virtual-cash-story-1` → `main` (closes #34 when merged)

## Blockers

- None for code/CI.

## Known Issues

- MSB3277: EF Core Relational 10.0.7 vs 10.0.8 — warning only.
- Api integration tests require Docker (Testcontainers).
- `RegisterUserSessionTests` uses `WebApplicationFactory` without Testcontainers (needs local Postgres on :5432) — excluded from CI-style regression run.
- Rare concurrent register race may return **500** instead of **422** (EC-03 MVP).

## Session Start Checklist

- [x] Virtual cash story 4 Tasks 1–5 automation on `feature/virtual-cash-story-4`
- [ ] Manual virtual cash story 4 UI checklist (plan §Manual UI checklist)
- [ ] PR `feature/virtual-cash-story-4` opened and CI green
- [x] Virtual cash story 3 Tasks 1–5 automation on `feature/virtual-cash-story-3`
- [ ] Manual virtual cash story 3 UI checklist (plan §Manual UI checklist)
- [ ] PR `feature/virtual-cash-story-3` opened and CI green
