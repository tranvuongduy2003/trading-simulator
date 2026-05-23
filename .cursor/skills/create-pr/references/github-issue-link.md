# Link issues, Project, labels, and assignees after PR create

## Link issues to the PR

GitHub links issues from the PR description using closing keywords (shows under **Development** on the issue).

| Mode | PR footer line | On merge to `main` |
|------|----------------|-------------------|
| Default (`link-closes`) | `Closes #123` | Issue auto-closes |
| `link-only` | `Refs #123` | Issue stays open (mention only) |

## Metadata sync (one script)

After `gh pr create`, run:

```powershell
.cursor/skills/create-pr/scripts/sync-pr-github-metadata.ps1 -PrNumber 9 -IssueNumber 5
```

| Target | Action |
|--------|--------|
| **PR** | `gh pr edit --add-project`, `--add-label`, `--add-assignee` |
| **Issues** | `gh project item-add` if missing; Status field update; `--add-label`, `--add-assignee` |

### Config (`.github/github-project.json`)

```json
{
  "owner": "your-login",
  "projectNumber": 1,
  "projectTitle": "Your project name",
  "statusOnPrCreated": "In review",
  "addPrToProject": true,
  "ensureIssuesOnProject": true,
  "inheritFromIssues": { "prLabels": true, "prAssignees": true },
  "prLabels": [],
  "prAssignees": ["@me"],
  "issueLabels": [],
  "issueAssignees": []
}
```

- **`inheritFromIssues`:** When linking issue #5, copy its labels/assignees onto the PR (union with `prLabels` / `prAssignees`).
- **`projectTitle`:** Must match the board name for `gh pr edit --add-project` (fallback: fetch from `gh project view`).
- **`ensureIssuesOnProject`:** Calls `gh project item-add` when the issue is not on the board yet.

### CLI overrides (flags on `/create-pr`)

| Flag | Example |
|------|---------|
| `labels` | `labels enhancement,spec` |
| `assignee` | `assignee @me` or `assignee octocat` |
| `skip-metadata` | Skip Step 7 entirely |
| `skip-project-status` | Labels/assignees/project only; no Status field change |

Requires `gh auth refresh -h github.com -s read:project,project`.

### Status field

If `"In review"` is missing on the board, add it under Project **Settings → Status**, or set `statusOnPrCreated` to an existing option (e.g. `In Progress`).
