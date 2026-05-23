# /build — Implement one plan task

Implement **one** unchecked task from a plan: locate it, prepare, confirm, implement, verify, and finalize. **One task per session.**

**PARALLEL-FIRST:** Use parallel reads and independent lookups while scoping work. **Implementation** stays a single task — parallelize only preparation (unrelated files, rules, pattern search).

**CLARIFICATION:** When you need to ask questions, use `AskQuestion` instead of dumping questions as plain text.

> **PLAN SYNC RULE (MANDATORY):** Any **deviation** from the plan (different approach, new sub-tasks, scope changes) **MUST** be written back to the plan file **immediately**. Task checkmarks (`[ ]` → `[x]`) can batch in Finalize; the plan must always reflect the actual approach. See Step 7.

## Step 1: Invoke Skills (conditional)

**Skills depend on who implements:**

| Scenario | Action |
|----------|--------|
| **Main agent implements** (default) | Read applicable `.cursor/skills/<name>/SKILL.md` before heavy implementation |
| **Delegating to subagents** | Put skill names in the subagent prompt — do not double-invoke here |

Invoke at most once per task per implementer.

### Skill routing (Trading Simulator)

| Trigger | Skill |
|---------|--------|
| Domain aggregates, invariants | `trading-domain-rules` |
| Layers, repositories, boundaries | `clean-architecture-rules` |
| Commands, queries, MediatR | `cqrs-handler-pattern` |
| Matching engine, channels | `matching-engine-patterns` |
| EF Core, migrations | `efcore-patterns` |
| Query / index performance | `database-performance` |
| C# style | `modern-csharp-coding-standards` |
| Channels / async | `csharp-concurrency-patterns` |
| DI in Api / MatchingEngine | `dependency-injection-patterns` |
| AppHost, Aspire, topology | `aspire`, `aspire-configuration` |
| Frontend data fetching | `tanstack-query` |
| React UI / performance | `react-expert`, `vercel-react-best-practices` |
| shadcn / Tailwind | `shadcn`, `tailwind-patterns` |
| Library API lookup | `context7-research` |
| Integration tests | `testcontainers-integration-tests` |
| Local env broken | `env-doctor` |
| User asks to **commit** | `git-commit-writer` |
| User asks for **PR** or `gh pr create` | `pr-description-writer`, `github-cli` |
| User asks for **code review** | `code-reviewer` |
| GitHub issue URL or `#<n>` in input | `github-cli` |

## Step 2: Read guidelines

**Always:**

- [`.cursor/rules/core.mdc`](mdc:.cursor/rules/core.mdc)
- [`docs/PRD.md`](docs/PRD.md), [`docs/TECHNICAL.md`](docs/TECHNICAL.md), [`docs/DATABASE.md`](docs/DATABASE.md) — skim sections the task touches

**Conditionally** (match task surface):

| Surface | Rules |
|---------|--------|
| .NET services | `backend.mdc` |
| Tests | `backend-testing.mdc` |
| React UI | `frontend.mdc`, `design-system.mdc` |
| HTTP / SignalR | `api-guidelines.mdc` |
| EF / schema | `migration.mdc` |
| AppHost | `aspire.mdc` |

## Step 3: Parse input

**Input:** `$ARGUMENTS` and the chat message.

| Input type | Detection | Action |
|------------|-----------|--------|
| Plan path | `docs/plans/<timestamp>-<name>.md` | Use that file |
| Task filter | `task 3`, `Task 2`, phase name | Scope to that task |
| GitHub issue | `#123` or `github.com/.../issues/123` | `gh issue view` for context (`github-cli`) |

If unclear, use `AskQuestion` for plan path or task id.

## Step 4: Build context

- **Plan:** `docs/plans/<timestamp>-<name>.md`
- **Spec:** path from plan frontmatter `related_spec` → `docs/specs/<timestamp>-<name>.md`
- **Memory:** `docs/memory/current-status.md`, `decisions.md`, `known-issues.md`
- **Task selection:** first unchecked acceptance item; if all done, report and stop

**Branch (mandatory per plan):**

- Use `> Branch: feature/<plan-slug>` from plan header when present.
- Else derive kebab slug from plan filename → `feature/<plan-slug>`.
- Create from `main` if missing; **never** implement plan work on `main` or unrelated branches.

**Artifact validation:**

- Filenames: `<timestamp>-<name>.md`, timestamp `YYYYMMDD-HHmmss`
- YAML frontmatter per `memory-artifacts.mdc`
- Fix metadata before coding if missing

**Exploration:** `Read`, `Grep`, `Glob` — reuse existing patterns; do not invent new stack choices.

## Step 5: Analyze parallelism

| Condition | Approach |
|-----------|----------|
| Independent reads (MODIFY/REUSE, rules) | Parallel |
| Writes / implementation for the one task | Sequential |
| Optional `explore` subagent | Parallel with reads if prompts are independent |

**Sequential signals:** task says "after Task X", "depends on", or blocked on migration.

## Step 6: Execute

Use `TodoWrite` for progress.

### 6.1 Prepare (then wait for confirmation)

Before any write:

1. Read every `CREATE` / `MODIFY` / `REUSE` file for the selected task
2. Search for patterns to reuse
3. If adding a package, verify version and breaking changes

Output a short **pre-flight** block:

- Task id and title
- Files: CREATE / MODIFY / REUSE
- Approach (1–3 sentences)
- Tests to add/run
- PRD / Tech / DB sections touched
- Aspire / migration / SignalR notes
- Risks

**Stop for explicit user confirmation** (`ok` / `go`) before edits or commits unless the user waived confirmation this session.

### 6.2 Implement

- **One task, one session** — minimal scope
- **Invariants:** Clean Architecture, pure Domain, async matching, PostgreSQL authoritative, Redis projections rebuildable, ServiceDefaults on hosts
- **AppHost:** `src/TradingSimulator.AppHost` (or current AppHost path) — no hand-authored `docker-compose` for orchestration
- **Migrations:** `migration.mdc` — never edit applied EF migrations
- **API:** RFC 7807, contracts DTOs, routes per `api-guidelines.mdc`
- **UI:** loading, empty, error, reconnecting states per `design-system.mdc`
- **Decisions:** significant architecture choices → `docs/memory/decisions.md`
- **Plan sync:** deviations → update plan immediately (Step 7)

If scope explodes, use `AskQuestion` to split or narrow.

### 6.3 Main agent responsibilities

- Orchestration, pre-flight, user confirmation
- Build / test / lint
- Plan updates (especially deviations)
- Final validation

### 6.4 Optional exploration subagent

```
Explore [area] for patterns related to [task from <plan file>].
Return: file paths, patterns, snippets. Do not modify files.
```

## Step 7: Update plan (critical)

The plan file is the source of truth for execution.

| Category | When | Examples |
|----------|------|----------|
| **Deviations** | Immediately | Different approach, extra files, split task |
| **Completions** | End of task (can batch in Finalize) | `[ ]` → `[x]` |

- Blocked: `[SKIP]` + reason in plan Notes
- **Memory sync:** `current-status.md` (last/next task); `CHANGELOG.md` if material; `known-issues.md` if new bug found
- **GitHub:** if plan §GitHub Links maps this task to `#<n>`, optionally comment progress via `gh issue comment` — do not close unless user asks
- **Do not commit** unless the user explicitly asks (then use `git-commit-writer`)

## Step 8: Validate

Run from repo root. Match the task's **Tests required** line.

**Backend** (adjust solution path when present):

```powershell
dotnet build TradingSimulator.sln -c Release --nologo
dotnet test  TradingSimulator.sln -c Release --nologo
```

If no `.sln` yet, build/test the projects listed in the task.

**Per `backend-testing.mdc` when applicable:**

```powershell
dotnet test tests/TradingSimulator.Domain.UnitTests -c Release --nologo
dotnet test tests/TradingSimulator.MatchingEngine.UnitTests -c Release --nologo
dotnet test tests/TradingSimulator.Api.IntegrationTests -c Release --nologo
```

**Format** (when solution exists):

```powershell
dotnet format TradingSimulator.sln --verify-no-changes
```

**Aspire** (AppHost / topology changed):

```powershell
aspire run --project src/TradingSimulator.AppHost
```

Use **aspire-mcp** or `aspire` skill if dashboard/resources fail.

**Frontend** (`web/` when task touches UI):

```powershell
cd web
npm run typecheck
npm run lint
npm run build
```

**EF** (schema changed — paths per task / `migration.mdc`):

```powershell
dotnet ef migrations add <Name> -p src/TradingSimulator.Infrastructure -s src/TradingSimulator.Api
dotnet ef database update      -p src/TradingSimulator.Infrastructure -s src/TradingSimulator.Api
```

Never modify existing migration files that were already applied.

## Step 9: Summary

Report:

- Completed task id and title
- Plan checkbox status
- Tests run and result
- Deviations recorded in plan
- Next unchecked task
- Suggested next action (`/build` again, open PR, etc.)

---

## Examples

| Command | Action |
|---------|--------|
| `/build` | First incomplete task in latest / linked plan |
| `/build docs/plans/20260523-140000-place-order.md` | That plan |
| `/build docs/plans/20260523-140000-place-order.md task 2` | Task 2 only |
| `/build` (all done) | Report completion; do not invent work |

### Execution pattern

```
1. Skills + rules + plan + spec + DB sections
2. Parallel reads (MODIFY / REUSE / patterns)
3. Pre-flight → user confirmation
4. Implement one task + immediate plan update on deviation
5. Validate (dotnet + targeted tests + web if needed)
6. Finalize checkboxes + memory files
```

### Dependency rule

**One plan task in flight** per session unless the user expands scope. Within a task, respect order: domain → application → infrastructure → API/engine → web when the task implies it.

---

## Finalize

- Plan: completed checkboxes; deviation notes current
- `docs/memory/current-status.md` updated
- Git: commit only on explicit request (`git-commit-writer`)
- PR: only on explicit request (`pr-description-writer` + `gh pr create` per user PR workflow)
