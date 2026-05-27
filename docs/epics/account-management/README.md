# Epic: Account Management (PRD §5.1)

**Status:** Closed (2026-05-29) — automation shipped; operator runbook retained for post-close/manual audit  
**User stories:** US-01, US-02, US-03, US-04  
**Archived:** 2026-05-28 — individual `docs/specs/` and `docs/plans/` files removed after verbatim merge.  
**Close plan:** [`docs/plans/20260528-194500-account-management-epic-close.md`](../../plans/20260528-194500-account-management-epic-close.md) (operator sign-off + hygiene)  
**Operator runbook (P1 gate):** [`OPERATOR-SIGNOFF.md`](OPERATOR-SIGNOFF.md) — consolidated manual UI matrix for all 18 stories

## Where to read

| Document | Purpose |
|----------|---------|
| [`specs.md`](specs.md) | **Product record** — what we promised (PRD + 4 full specs, verbatim) |
| [`plans.md`](plans.md) | **Implementation record** — how we built it (18 full plans, verbatim) |
| [`../../reviews/20260528-180000-account-management.md`](../../reviews/20260528-180000-account-management.md) | Epic review — gaps, tests, hygiene |
| [`OPERATOR-SIGNOFF.md`](OPERATOR-SIGNOFF.md) | **Operator manual UI** — sign-off matrix (95 steps + E2E smoke) |
| [`../../plans/20260528-194500-account-management-epic-close.md`](../../plans/20260528-194500-account-management-epic-close.md) | Epic close plan — `/build` tasks |

## What this epic delivered (summary)

| US | PRD | Shipped capability |
|----|-----|-------------------|
| **US-01** | Register | `POST /api/users` — user + wallet $100k + session cookie; validation 422; duplicate 422; transient/retry UX |
| **US-02** | Login | `POST /api/auth/login`, `POST /api/auth/logout`, session cookie + Redis `session:{id}`; 401 invalid credentials; FluentValidation 422 |
| **US-03** | Cash balance | `GET /api/wallet` — total/reserved/available; trading dashboard + top-bar chip; user-scoped TanStack Query (ADR-008) |
| **US-04** | Portfolio reset | `POST /api/portfolio/reset`, `GET /api/portfolio/reset/eligibility`; 24h cooldown; cancel opens; history cutoff (ADR-007); panel invalidation |

**Tests (automated):** Domain Users **22**; Api Users + ResetPortfolio **85** (Testcontainers).

**ADRs:** ADR-004 (top-bar cash only), ADR-005→006 (reset evolution), ADR-007 (read cutoff), ADR-008 (query keys).

**GitHub epics:** #4 US-01, #21 US-02, #33 US-03, #43 US-04.

For acceptance criteria, business rules, edge cases, and per-story task checklists → open **`specs.md`** and **`plans.md`** below.
