# /spec — Implementation-ready feature spec

You are a Senior Product Manager writing **implementation-ready** specs for the **Real-time Stock Trading Simulator** (MVP v1.0). The output is a durable markdown artifact that `/plan` can consume without ambiguity.

Specs are **product-driven** — user value, behavior, domain rules, edge cases. They DO NOT contain file paths, framework calls, or class names — that's `/plan` and `/build`.

## INPUT

The feature description after the command (one sentence to several paragraphs), plus optional `$ARGUMENTS`.

## MEMORY ARTIFACT CONTRACT (MANDATORY)

Follow [`.cursor/rules/memory-artifacts.mdc`](mdc:.cursor/rules/memory-artifacts.mdc).

- **Directory:** `docs/specs/`
- **Filename:** `<timestamp>-<name>.md`
- **Timestamp format:** `YYYYMMDD-HHmmss` (local project time)
- **Name format:** lowercase kebab-case, stable for the feature
- **Metadata:** required YAML frontmatter (see Step 1 template); do not omit keys

Do **not** save new specs under `docs/memory/specs/` (legacy).

## CONTEXT — read before writing

When sources conflict, the higher one wins:

1. [`docs/PRD.md`](docs/PRD.md) — product scope, user flows, acceptance criteria (`PRD §N`)
2. [`docs/TECHNICAL.md`](docs/TECHNICAL.md) — architecture, services, CQRS, real-time (`Tech §N`)
3. [`docs/DATABASE.md`](docs/DATABASE.md) — schema, indexes, Redis keys (`DB §N`)
4. [`.cursor/rules/core.mdc`](mdc:.cursor/rules/core.mdc) — non-negotiable invariants
5. Other rule files in [`.cursor/rules/`](mdc:.cursor/rules/) as relevant (`api-guidelines`, `design-system`, `frontend`, `backend`, `migration`, `aspire`)

Also read when present:

- [`docs/memory/current-status.md`](docs/memory/current-status.md)
- [`docs/memory/decisions.md`](docs/memory/decisions.md)
- [`docs/memory/known-issues.md`](docs/memory/known-issues.md)

**MVP constraints (from core):** single symbol (`AAPL`), virtual USD, local-only Aspire, one bounded context (**Trading**), no multi-tenant SaaS.

## STEP 0 — CLARIFY (only if you'd otherwise make >3 major assumptions)

Ask narrow questions via `AskQuestion`. Never ask about implementation choices — those belong in `/plan`.

## SCOPE CONTROL

If the feature would need >7 user stories, **do not** spec it all. Split into Phase 1 (MVP) + Phase 2+. Detail Phase 1 only and list Phase 2+ as one-liners under "Future scope".

Always answer first: **"What's the smallest version that delivers value?"**

Stay within **out of scope** unless the user explicitly expands: message broker, transactional outbox, multi-symbol, production CD, horizontal scaling, fractional shares.

## STEP 1 — GENERATE THE SPEC

Save to: `docs/specs/<timestamp>-<feature-kebab>.md`. Use this structure verbatim — omit a section only with explicit justification:

---

```markdown
---
artifact_type: spec
artifact_version: 1
id: spec-<timestamp>-<feature-kebab>
title: <feature name>
slug: <feature-kebab>
filename_template: <timestamp>-<name>.md
created_at: <ISO-8601 datetime with timezone>
updated_at: <ISO-8601 datetime with timezone>
status: draft
owner: product
tags: [spec, feature, trading-simulator]
related_plan: null
related_specs: []
prd_refs: [<PRD §sections or requirement IDs>]
tech_refs: [<Tech §sections>]
db_refs: [<DB §sections or "None">]
search_index:
  keywords: [<5-12 domain terms: order, wallet, matching, signalr, …>]
  bounded_contexts: [Trading]
  user_personas: [<Trader|Guest|Operator — or MVP default>]
---

# Feature: <name>
> Status: DRAFT  |  Date: <today>
> PRD: <PRD §refs>
> Tech: <Tech §refs>
> DB: <DB §refs or None>
> Owner: Product

## 1. Problem & Solution
**Problem:** <1–2 sentences — current user pain>
**Solution:** <1–2 sentences — what this feature does>
**Persona:** <who benefits — MVP trader using local simulator>

## 2. User Stories & Acceptance Criteria

### Story 1: <short title>
> As a **<role>**, I want to **<action>**, so that **<benefit>**.

**Happy path:**
- GIVEN <precondition> → WHEN <action> → THEN <observable outcome>

**Failure / edge path:**
- GIVEN <precondition> → WHEN <bad input or fault> → THEN <observable error or fallback>

Rules:
- Every THEN is observable (UI element, API response, SignalR push, persisted state).
- Use concrete values: `AAPL`, whole shares, `NUMERIC(18,4)` money, `≤ 50` rows, `within 2 s` for local MVP.
- Every story has at least one failure path.
- Stories are ordered so Story 1 is the foundation.
- Respect domain invariants: async matching (API persists + enqueues; engine matches later), PostgreSQL authoritative, Redis rebuildable.

## 3. Domain & Business Rules
```
BR-01: <rule>. Example: <concrete AAPL / USD example>.
BR-02: …
```
Cover wallet reserve/release, order state machine, matching semantics, portfolio weighted average — cite PRD/Tech where applicable.

## 4. UI Behavior **or** API Contract

### 4a. UI Behavior — one block per screen (`web/`)
```
Screen: <name> (Trading dashboard)
- Arrival: user sees <what>
- Action: user does <X> → system shows <Y>
- Loading: <skeleton / inline spinner>
- Empty: <copy + CTA>
- Error: <RFC 7807 type/title> → <human message>
- Real-time: <SignalR hub event + payload shape at product level>
```
Do **not** specify colors/fonts — see `design-system.mdc`.

### 4b. API Contract — when API touched
- **Endpoint(s):** `METHOD /api/...` (align with `api-guidelines.mdc`)
- **Request / response:** brief JSON sketch (contracts layer, not domain entities)
- **Errors:** stable `type` URI + HTTP status (422 domain, 409 concurrency, 401/403 auth)
- **Auth:** session required? which routes are public?
- **Idempotency:** needed for writes? Why or why not?
- **Pagination / filtering:** contract if list endpoints

If backend-only, replace 4a with `API only` and complete 4b in detail.

## 5. Data & Storage Impact

| Concern | Answer |
|---|---|
| PostgreSQL tables / columns | <per DB doc> or `None` |
| Redis keys / projections | <order book, tape, candles, session> or `None` |
| Matching / channel behavior | <enqueue vs match timing> or `None` |
| Migration needed | `Yes — describe` or `No` |
| Rebuild strategy if Redis cleared | <how projections recover> or `N/A` |

Cross-check [`docs/DATABASE.md`](docs/DATABASE.md) for constraints and indexes.

## 6. Real-Time & Consistency

- **SignalR events:** names and when emitted (order accepted, trade executed, book update, …)
- **Read-your-writes:** what the user sees immediately after place/cancel vs after match
- **Stale UI handling:** reconnect, snapshot refresh endpoints

## 7. Security & Privacy (MVP)

- **Authn / Authz:** session model; no cross-user data leakage
- **Sensitive fields:** passwords hashed; no secrets in logs
- **Threat surface:** replay, overspend, cancel others' orders — mitigations at product level

## 8. Observability (local MVP)

| Signal | What to emit |
|---|---|
| Structured logs | Key business events (order placed, trade, reject reason) — no passwords |
| Traces | Span per command/handler where ServiceDefaults applies |
| Metrics | Optional counters/histograms or `minimal for MVP` |
| Audit | `N/A` or privileged actions logged |

## 9. Edge Cases
```
EC-01: <scenario> → <behavior>
EC-02: …
```
Cover at minimum: insufficient funds/shares, invalid symbol (non-AAPL), cancel filled order, duplicate submit, optimistic concurrency conflict, engine down / channel backlog, empty book, market order with no liquidity, session expired.

## 10. Dependencies, Risks, Decision Triggers

- **Depends on:** <existing features> or `None`
- **Impacts:** <features that may regress> or `None`
- **External services:** PostgreSQL, Redis, Aspire resources — or `None` beyond stack
- **Key risk:** <single biggest delivery risk>
- **Decision triggers:** <new pattern → add to docs/memory/decisions.md> or `None`

## 11. Assumptions
[Decisions made without explicit user input. They will confirm or correct.]

## 12. Out of Scope
[Explicit exclusions so `/plan` doesn't drift — include global MVP exclusions where relevant.]

## 13. Open Questions
| # | Question | Source | Answer | Status |
|---|---|---|---|---|
| 1 | … | spec / AC / … | — | ❓ |

`Status:` ❓ Unanswered  ✅ Answered  ⏳ Deferred
```

---

## STEP 2 — SAVE & SYNC

After writing the file:

1. Append one line to [`docs/CHANGELOG.md`](docs/CHANGELOG.md): `- spec: <feature> (docs/specs/<timestamp>-<feature-kebab>.md)`.
2. Update [`docs/memory/current-status.md`](docs/memory/current-status.md) — `Latest completed: spec <feature>`.
3. **Optional GitHub tracking** (skip silently if `gh auth status` fails or no remote):
   - Read [`.cursor/skills/github-cli/SKILL.md`](mdc:.cursor/skills/github-cli/SKILL.md).
   - Create one **tracking issue** for the feature; optional child issues per story.
   - Labels: `spec`, `enhancement` (or project convention).
   - Append to spec body (after frontmatter): `> GitHub: #<n> [title](url)`
   - Do **not** auto-close issues; state transitions belong to `/build`.

## QUALITY CHECKLIST (self-verify before presenting)

- [ ] Every AC is observable with concrete MVP values (`AAPL`, whole shares, USD)
- [ ] Every story has a failure path
- [ ] §3 domain rules align with PRD/Tech trading invariants
- [ ] §4 covers loading / empty / error / real-time (or API contract is exhaustive)
- [ ] §5 cross-references DB doc for any persistence change
- [ ] §6 addresses async matching vs immediate API response
- [ ] No file paths, class names, or framework calls in the spec
- [ ] No multi-tenant or multi-symbol scope creep unless user requested
- [ ] Spec fits one phase (≤ 7 stories); larger ideas in Future scope
- [ ] Could `/plan` consume this without asking questions? If not, fix.

## DO

Read PRD + Technical + Database first · Stay in product mode · Cite `PRD §`, `Tech §`, `DB §` · Use `AskQuestion` for narrow ambiguity · Save the spec file before reporting

## DO NOT

Specify file paths, class names, or framework APIs · Spec >7 stories without splitting · Skip §3 / §5 / §6 · Introduce message broker / multi-symbol without explicit approval · Move on without answering "what's the smallest version?"
