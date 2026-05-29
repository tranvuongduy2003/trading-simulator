# Current Status

Last updated: 2026-05-30 (US-06 Story 2 implementation complete)

Owner: @tranvuongduy2003

## Epic: Account Management (PRD §5.1)

- **Review:** ✅ Closed administratively (follow-up hygiene tasks continue) — [`docs/reviews/20260528-180000-account-management.md`](../reviews/20260528-180000-account-management.md)
- **Close plan:** [`docs/plans/20260528-194500-account-management-epic-close.md`](../plans/20260528-194500-account-management-epic-close.md) — 6 tasks (operator sign-off, merge, test split, docs, register 422, verification)
- **Archive:** [`docs/epics/account-management/README.md`](../epics/account-management/README.md) (specs + plans merged; scattered `docs/specs/` / `docs/plans/` removed)
- **Tests (2026-05-28):** Domain Users **22** passed; Api Users + ResetPortfolio **85** passed
- **CI update (2026-05-28):** GitHub Actions now runs `Api.IntegrationTests` (Testcontainers on `ubuntu-latest` Docker environment)
- **Epic status:** `docs/epics/account-management/specs.md` promoted to `status: approved` (all 4 archived specs)

## Active Focus

- US-06 Story 2: real-time depth table updates — **implementation complete** on `feature/live-order-book-depth-story-2` (#71); 5 SignalR + 11 snapshot integration tests passed; manual UI checklist pending operator
- US-06 Story 1: order book depth table — **implementation complete** on `feature/live-order-book-depth-story-1` (#70); integration tests compile; Docker + manual UI checklist pending operator
- US-05 Story 4: empty / one-sided book — **implementation complete** on `feature/best-bid-ask-story-4` (#61); manual UI checklist + integration tests (Docker) pending operator/CI
- US-05 Story 3: spread and mid-price — **implementation complete** on `feature/best-bid-ask-story-3` (#60); manual UI checklist pending operator
- US-05 Story 2: real-time top-of-book — **implementation complete** on `feature/best-bid-ask-story-2` (GitHub #59); manual UI checklist pending operator
- US-05 Story 1: best bid/ask — **implementation complete** on `feature/best-bid-ask-story-1` (GitHub #58); manual UI checklist pending operator

## Latest Completed

- impl: US-06 Story 2 — depth reconnect badge, hub `orderCount`, client normalize/dedupe, SignalR integration tests (`feature/live-order-book-depth-story-2`, #71); 16 market tests passed; `yarn lint` + `yarn build` + `api:verify` green
- plan: US-06 Story 2 — real-time depth updates (`docs/plans/20260530-210000-live-order-book-depth-story-2.md`, #71)
- impl: US-06 Story 1 — depth panel, `depth=10` client param, multi-level/aggregation integration tests (`feature/live-order-book-depth-story-1`, #70); `yarn lint` + `yarn build` + `api:verify` green
- spec: Live order book depth US-06 (`docs/specs/20260530-002008-live-order-book-depth.md`)
- impl: US-05 Story 4 — liquidity state, empty strip copy, logout orderbook cache purge, bid/ask-only integration tests (`feature/best-bid-ask-story-4`, #61); `yarn lint` + `yarn build` green
- plan: US-05 Story 4 — empty and one-sided book (`docs/plans/20260530-010000-best-bid-ask-story-4.md`, #61)
- impl: US-05 Story 3 — spread/mid derivation + strip UI (`feature/best-bid-ask-story-3`, #60); Api integration **97** passed; `yarn lint` + `yarn build` green
- plan: US-05 Story 3 — spread and mid-price (`docs/plans/20260529-220000-best-bid-ask-story-3.md`, #60)
- impl: US-05 Story 2 — `IOrderBookMarketDataNotifier`, hub `setQueryData`, reconnect badge, SignalR integration test (`feature/best-bid-ask-story-2`, #59); Api integration **97** passed; `web api:verify` + `yarn build` green
- plan: US-05 Story 2 — real-time top-of-book (`docs/plans/20260529-203000-best-bid-ask-story-2.md`, #59)
- impl: US-05 Story 1 — market order book snapshot API + top-of-book strip (`feature/best-bid-ask-story-1`, #58); Api integration **96** passed; `web api:verify` + `yarn build` green
- plan: Best bid and ask Story 1 — initial snapshot (`docs/plans/20260529-120000-best-bid-ask-story-1.md`, #58)
- spec: Best bid and ask display US-05 (`docs/specs/20260529-010501-best-bid-ask.md`)
- verify: account-management epic close **Task 6** — ran final verification matrix (Domain Users **22**, Api Users + ResetPortfolio **85**, `web api:verify`, `web build`), synced memory/changelog, and promoted close plan status to `approved` (administrative close; manual sign-off still deferred by prior waiver)

## In Progress

- Operator: US-06 Story 2 manual UI checklist (plan §Manual UI checklist) on Aspire
- Operator: US-06 Story 1 manual UI checklist (plan §Manual UI checklist) on Aspire
- Operator: US-06 Story 1 integration tests with Docker (local Docker unavailable during build)
- Operator: US-05 Story 4 manual UI checklist (plan §Manual UI checklist) on Aspire
- Operator: US-05 Story 3 manual UI checklist (plan §Manual UI checklist) on Aspire
- Operator: US-05 Story 2 manual UI checklist (plan §Manual UI checklist) on Aspire
- Operator: US-05 Story 1 manual UI checklist (plan §Manual UI checklist) on Aspire
- Operator: portfolio reset story 5 manual UI checklist (plan §Manual UI checklist) on Aspire

## Next Up

- PR `feature/live-order-book-depth-story-2` → `main` (closes #71)
- PR `feature/live-order-book-depth-story-1` → `main` (closes #70)
- `/plan` for US-06 Stories 3–4 (#72–#73) when ready
- PR `feature/best-bid-ask-story-4` → `main` (closes #61)
- PR `feature/best-bid-ask-story-3` → `main` (closes #60)
- PR `feature/best-bid-ask-story-2` → `main` (closes #59)
- PR `feature/best-bid-ask-story-1` → `main` (closes #58)

## Blockers

- None for code/CI.

## Known Issues

- MSB3277: EF Core Relational 10.0.7 vs 10.0.8 — warning only.
- Api integration tests require Docker (Testcontainers).
- Local consecutive test runs can intermittently hit CS2012 file-lock errors; rerun usually succeeds.

## Session Start Checklist

- [x] US-05 Story 2 Tasks 1–5 automation on `feature/best-bid-ask-story-2`
- [ ] Manual US-05 Story 2 UI checklist (plan §Manual UI checklist)
- [ ] PR `feature/best-bid-ask-story-2` opened and CI green
- [x] US-05 Story 1 Tasks 1–5 automation on `feature/best-bid-ask-story-1`
- [ ] Manual US-05 Story 1 UI checklist (plan §Manual UI checklist)
- [ ] PR `feature/best-bid-ask-story-1` opened and CI green
