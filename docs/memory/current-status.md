# Current Status

Last updated: 2026-05-29 (US-05 spec)

Owner: @tranvuongduy2003

## Epic: Account Management (PRD §5.1)

- **Review:** ✅ Closed administratively (follow-up hygiene tasks continue) — [`docs/reviews/20260528-180000-account-management.md`](../reviews/20260528-180000-account-management.md)
- **Close plan:** [`docs/plans/20260528-194500-account-management-epic-close.md`](../plans/20260528-194500-account-management-epic-close.md) — 6 tasks (operator sign-off, merge, test split, docs, register 422, verification)
- **Archive:** [`docs/epics/account-management/README.md`](../epics/account-management/README.md) (specs + plans merged; scattered `docs/specs/` / `docs/plans/` removed)
- **Tests (2026-05-28):** Domain Users **22** passed; Api Users + ResetPortfolio **85** passed
- **CI update (2026-05-28):** GitHub Actions now runs `Api.IntegrationTests` (Testcontainers on `ubuntu-latest` Docker environment)
- **Epic status:** `docs/epics/account-management/specs.md` promoted to `status: approved` (all 4 archived specs)

## Active Focus

- Account-management epic close plan marked complete (`feature/account-management-epic-close`)
- Optional/manual audit remains available: `docs/epics/account-management/OPERATOR-SIGNOFF.md`

## Latest Completed

- spec: Best bid and ask display US-05 (`docs/specs/20260529-010501-best-bid-ask.md`)
- verify: account-management epic close **Task 6** — ran final verification matrix (Domain Users **22**, Api Users + ResetPortfolio **85**, `web api:verify`, `web build`), synced memory/changelog, and promoted close plan status to `approved` (administrative close; manual sign-off still deferred by prior waiver)
- impl: account-management epic close **Task 5** — mapped Postgres unique-violation (`23505`) for register constraints (`ux_users_username`, `ux_users_email`) to 422 (`USERNAME_TAKEN` / `EMAIL_TAKEN`) in UoW path; tightened parallel register race test to require 422; `RegisterUserTransientFailureTests` green (**4**)
- docs: account-management epic close **Task 4** — synced `frontend.mdc` wallet query example to ADR-008 (`['wallet', userId]`, `staleTime: 0`) and removed stale RegisterUserSession local-Postgres note from memory docs
- impl: account-management epic close **Task 3** — split `ResetPortfolioTests.cs` into focused suites (`Auth`, `Eligibility`, `Wallet`, `OrdersHistory`, `Notifications`) with shared `PortfolioResetTestHelpers`; `FullyQualifiedName~ResetPortfolio` green (**23** tests)
- verify: account-management epic close **Task 2** — confirmed `main` already contains US-03/US-04 story merges (#39–#55); promoted archived specs to `approved`; epic archive README marked closed; regression checks green (Domain Users **22**, Api Users + ResetPortfolio **85**, `web api:verify`, `web build`)
- docs: Account Management epic close **Task 1** — `OPERATOR-SIGNOFF.md` (95 manual rows + E2E smoke); README links runbook as P1 gate (`feature/account-management-epic-close`)
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

- Optional: complete `docs/epics/account-management/OPERATOR-SIGNOFF.md` manual checklist for additional operator evidence
- If desired: open PR for `feature/account-management-epic-close` to merge finalized hygiene updates

## Blockers

- None for code/CI.

## Known Issues

- MSB3277: EF Core Relational 10.0.7 vs 10.0.8 — warning only.
- Api integration tests require Docker (Testcontainers).
- Local consecutive test runs can intermittently hit CS2012 file-lock errors; rerun usually succeeds.

## Session Start Checklist

- [x] Virtual cash story 4 Tasks 1–5 automation on `feature/virtual-cash-story-4`
- [ ] Manual virtual cash story 4 UI checklist (plan §Manual UI checklist)
- [ ] PR `feature/virtual-cash-story-4` opened and CI green
- [x] Virtual cash story 3 Tasks 1–5 automation on `feature/virtual-cash-story-3`
- [ ] Manual virtual cash story 3 UI checklist (plan §Manual UI checklist)
- [ ] PR `feature/virtual-cash-story-3` opened and CI green
