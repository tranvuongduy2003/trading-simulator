---
name: create-pr
description: End-to-end pull request workflow for trading-simulator — branch, local CI, Conventional Commits, push, open PR, optionally link GitHub issues (Closes #N) and set Project board Status to In review. Use when the user runs /create-pr, create PR, open pull request, ship changes, or mentions attaching an issue to a PR.
---

# Create Pull Request

Orchestrates the full PR workflow for **trading-simulator**. Delegates commit messaging to `git-commit-writer` and PR body to `pr-description-writer`; uses `github-cli` for `gh pr create`.

## When to use

- User says: create PR, open PR, ship it, branch and PR, `/create-pr`
- User says: create PR **for issue #N** / attach issue / link issue
- After implementation is done and changes should land on `main` via review

**Do not use** for commit-only or description-only requests — use `git-commit-writer` or `pr-description-writer` alone.

## Parameters (from `/create-pr` command or user message)

Parse **before** Step 5 when present:

| Input | Example | Effect |
|-------|---------|--------|
| Issue number(s) | `5`, `#5`, `5,6`, `5 6` | Link those issues to the PR; update Project Status |
| `link-only` | "link only don't close" | Use `Refs #N` instead of `Closes #N` |
| `skip-project-status` | "skip project status" | Skip Step 7 |
| `status <name>` | `status In Progress` | Override Project Status option (default: `statusOnPrCreated` in config or `In review`) |

If no issue numbers are given, run Steps 1–5 only.

## Prerequisites

- Clean intent: all changes belong in **one logical PR** (otherwise split first)
- `gh` installed and authenticated (`gh auth status`)
- On repo root; base branch is **`main`** unless user specifies another
- User explicitly requested commit/PR (project rule: no surprise commits)
- **Issue linking + Project status:** `gh auth refresh -h github.com -s read:project,project` and [`.github/github-project.json`](../../.github/github-project.json) (copy from [`github-project.json.example`](../../.github/github-project.json.example))

## Workflow checklist

```
- [ ] 1. Branch created from main
- [ ] 2. CI pipeline verified locally
- [ ] 3. Changes staged (no secrets)
- [ ] 4. Commit (git-commit-writer)
- [ ] 5. Push + PR (pr-description-writer + gh)
- [ ] 6. Link issues on PR (if --issue)
- [ ] 7. Project Status → In review (if --issue)
```

---

## Step 1: Create branch

1. `git fetch origin` and ensure `main` is current: `git checkout main` then `git pull origin main` (if user allows pull).
2. **Branch name** — default prefix `feature/` unless the user gives another (`fix/`, `chore/`):
   - `feature/<short-kebab-slug>` — e.g. `feature/frontend-mvp-shell`
   - Slug from plan header `> Branch: feature/...`, task title, or primary change area
3. If already on a non-`main` branch with the right work, **reuse it**; do not recreate.
4. Create and switch: `git checkout -b feature/<slug>`

**PowerShell:** chain with `;` not `&&`.

---

## Step 2: Verify pipeline

Run the same checks as [`.github/workflows/ci.yml`](../../.github/workflows/ci.yml). Full command list: [references/ci-pipeline.md](references/ci-pipeline.md).

**Summary (repo root):**

```powershell
dotnet restore TradingSimulator.slnx
dotnet format TradingSimulator.slnx --verify-no-changes
dotnet build TradingSimulator.slnx --no-restore -c Release
dotnet test TradingSimulator.slnx --no-build -c Release --verbosity normal

yarn --cwd web install --frozen-lockfile
yarn --cwd web lint
yarn --cwd web format:check
yarn --cwd web build
```

**If `yarn format:check` fails:** run `yarn --cwd web format`, then re-check.

**If `dotnet format` fails on Windows (CRLF):** CI runs on Ubuntu with LF — note in PR Testing section; fix only if the **changed** files fail on CI, not pre-existing tree-wide CRLF noise.

**If any step fails:** fix, re-run failed steps, then continue. Do not open a PR with known red checks unless the user accepts it.

**Scope:** Skip `web/*` steps when the PR touches only backend/docs; skip .NET steps when the PR is frontend-only.

---

## Step 3: Stage changes

1. `git status` — review untracked and modified files.
2. **Never stage:** `.env`, credentials, `**/bin/`, `**/obj/`, `.github/github-project.json`, local IDE junk.
3. Stage intentionally: `git add` paths or `git add -A` after confirming no secrets.
4. If nothing to commit, stop and tell the user.

---

## Step 4: Commit

**Read and follow** [`.cursor/skills/git-commit-writer/SKILL.md`](../git-commit-writer/SKILL.md):

1. `git diff --cached --stat` and analyze `git diff --cached`
2. Write Conventional Commits message (`type(scope): imperative subject` + body if needed)
3. Commit (user already invoked create-pr, so committing is in scope)

**PowerShell commit** (no bash heredoc):

```powershell
git commit -m "feat(scope): short subject" -m "Optional body paragraph."
```

**Git safety:** no `--no-verify`, no `git config` changes, no force-push to `main`, no amend unless user rules allow.

---

## Step 5: Push and create PR

1. `git push -u origin HEAD`
2. **Read and follow** [`.cursor/skills/pr-description-writer/SKILL.md`](../pr-description-writer/SKILL.md) to draft the body from `git diff main...HEAD` and `git log --oneline main..HEAD`.
3. If issue numbers are known, include a **Linked issues** section in the initial body (see Step 6).
4. Create PR:

```powershell
gh pr create --base main --title "<subject>" --body-file $env:TEMP\pr-body.md
```

5. Capture PR number from output URL.

**PR title:** Prefer the commit subject line; expand only if too vague.

---

## Step 6: Link issues to the PR (when issue numbers provided)

Append to the PR description so GitHub shows the link under the issue **Development** panel.

**Default (`link-closes`):**

```markdown
## Linked issues

Closes #5
```

**Alternate (`link-only`):** use `Refs #5` (does not auto-close on merge).

If the section was not in the initial body, patch after create:

```powershell
$pr = 9   # from gh pr create output
$existing = gh pr view $pr --json body --jq .body
$footer = "## Linked issues`n`nCloses #5"
gh pr edit $pr --body ($existing.TrimEnd() + "`n`n" + $footer)
```

**Epic + stories:** pass every issue to link (e.g. `5` for the story in progress; avoid `Closes #4` on the epic until all stories ship unless the user asks).

Details: [references/github-issue-link.md](references/github-issue-link.md).

---

## Step 7: Project Status → In review (when issue numbers provided)

Unless `skip-project-status`:

1. Ensure each issue is on the Project board (`gh project item-add` from `/spec` Step 3.3).
2. Run from repo root:

```powershell
.cursor/skills/create-pr/scripts/set-project-issue-status.ps1 -IssueNumber 5
```

Multiple issues: `-IssueNumber 5,6,7`

3. Status option name comes from `.github/github-project.json` → `statusOnPrCreated` (default **In review**). Match is case-insensitive.

**If the option does not exist** on the board (e.g. only Todo / In Progress / Done): tell the user to add **In review** to the Project **Status** field in GitHub, or set `statusOnPrCreated` to an existing option in `github-project.json`.

**Requires** `project` scope on `gh auth`.

---

## Report to user

| Item | Value |
|------|--------|
| Branch | `feature/...` |
| Commit | hash + subject |
| Pipeline | pass/fail per step (brief) |
| PR | URL |
| Linked issues | `#5` … or `none` |
| Project status | `In review` set / skipped / failed (reason) |

Mention deferred manual checks (Aspire smoke test, screenshots) in the PR Testing section, not as blockers unless CI failed.

---

## Related skills

| Step | Skill |
|------|--------|
| Commit message | `git-commit-writer` |
| PR body | `pr-description-writer` |
| `gh` commands | `github-cli` |
| Command entry | [`.cursor/commands/create-pr.md`](../../commands/create-pr.md) |
| Implement before PR | `/build` in [`.cursor/commands/build.md`](../../commands/build.md) |
