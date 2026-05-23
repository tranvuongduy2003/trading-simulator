# Link issues and Project status after PR create

## Link issues to the PR

GitHub links issues from the PR description using closing keywords (shows under **Development** on the issue).

| Mode | PR footer line | On merge to `main` |
|------|----------------|-------------------|
| Default (`link-closes`) | `Closes #123` | Issue auto-closes |
| `link-only` | `Refs #123` | Issue stays open (mention only; may not appear in Development) |

Append a **Linked issues** section when `--issue` is provided:

```markdown
## Linked issues

Closes #5
Closes #6
```

After `gh pr create`, if keywords were omitted, patch the body:

```powershell
$body = gh pr view $pr --json body -q .body
$footer = "## Linked issues`n`nCloses #5"
gh pr edit $pr --body ($body.TrimEnd() + "`n`n" + $footer)
```

## Project Status → In review

Requires `read:project` and `project` scopes (`gh auth refresh -h github.com -s read:project,project`).

Config (`.github/github-project.json`):

```json
{
  "owner": "your-login",
  "projectNumber": 1,
  "statusOnPrCreated": "In review"
}
```

Run after PR exists:

```powershell
.cursor/skills/create-pr/scripts/set-project-issue-status.ps1 -IssueNumber 5,6
```

The script matches the Status single-select option **case-insensitively** (spaces ignored). If `"In review"` is missing, add it to the Project board in GitHub UI (**Settings → Fields → Status**) or set `statusOnPrCreated` to an existing option (e.g. `In Progress`).

**Requires** the issue to already be on the project board (`gh project item-add`).
