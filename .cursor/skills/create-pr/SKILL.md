---
name: create-pr
description: End-to-end pull request workflow for this repo — create a prefixed branch, run local CI checks, commit with Conventional Commits, push, and open a GitHub PR. Use when the user asks to create a PR, open a pull request, ship changes, or run branch → pipeline → commit → PR.
---

# Create Pull Request

Orchestrates the full PR workflow for **trading-simulator**. Delegates commit messaging to `git-commit-writer` and PR body to `pr-description-writer`; uses `github-cli` for `gh pr create`.

## When to use

- User says: create PR, open PR, ship it, branch and PR, `/create-pr`
- After implementation is done and changes should land on `main` via review

**Do not use** for commit-only or description-only requests — use `git-commit-writer` or `pr-description-writer` alone.

## Prerequisites

- Clean intent: all changes belong in **one logical PR** (otherwise split first)
- `gh` installed and authenticated (`gh auth status`)
- On repo root; base branch is **`main`** unless user specifies another
- User explicitly requested commit/PR (project rule: no surprise commits)

## Workflow checklist

Copy and track progress:

```
- [ ] 1. Branch created from main
- [ ] 2. CI pipeline verified locally
- [ ] 3. Changes staged (no secrets)
- [ ] 4. Commit (git-commit-writer)
- [ ] 5. Push + PR (pr-description-writer + gh)
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
2. **Never stage:** `.env`, credentials, `**/bin/`, `**/obj/`, local IDE junk, user-specific paths.
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
3. Create PR with **github-cli**:

```powershell
gh pr create --title "<same as commit subject or concise title>" --body "<markdown from pr-description-writer>"
```

- Base branch: `main` (add `--base main` if needed)
- Return the PR URL to the user
- Optionally: `gh pr checks --watch` if user wants CI status

**PR title:** Prefer the commit subject line; expand only if too vague.

---

## Report to user

Include:

| Item | Value |
|------|--------|
| Branch | `feature/...` |
| Commit | hash + subject |
| Pipeline | pass/fail per step (brief) |
| PR | URL |

Mention deferred manual checks (Aspire smoke test, screenshots) in the PR Testing section, not as blockers unless CI failed.

---

## Related skills

| Step | Skill |
|------|--------|
| Commit message | `git-commit-writer` |
| PR body | `pr-description-writer` |
| `gh` commands | `github-cli` |
| Implement before PR | `/build` in [`.cursor/commands/build.md`](../../commands/build.md) |
