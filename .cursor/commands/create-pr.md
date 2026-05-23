# /create-pr — Branch, CI, commit, PR, GitHub metadata

Orchestrates the full PR workflow. Read and follow [`.cursor/skills/create-pr/SKILL.md`](mdc:.cursor/skills/create-pr/SKILL.md).

## INPUT

Optional arguments after the command:

| Form | Example | Meaning |
|------|---------|---------|
| Issue number(s) | `/create-pr 5` | Link issue **#5**; sync metadata |
| Multiple | `/create-pr 5 6` | Link **#5** and **#6** |
| Hash form | `/create-pr #5` | Same as `5` |
| Comma list | `/create-pr 5,6` | Same as `5 6` |
| Natural language | `create PR for issue 5` | Parse issue numbers from the message |

If omitted, Step 7 still runs **PR-only** metadata when configured (`addPrToProject`, `prLabels`, `prAssignees`). Otherwise Steps 1–5 only.

## FLAGS (parse from user message)

| Flag | Default | Meaning |
|------|---------|---------|
| `link-closes` | **yes** | Append `Closes #N` (closes on merge to `main`) |
| `link-only` | no | Append `Refs #N` instead |
| `skip-metadata` | no | Skip Step 7 (no project / labels / assignees / status) |
| `skip-project-status` | no | Step 7 without Status field change |
| `status` | config or `In review` | Override Project Status option name |
| `labels` | — | Extra PR labels, comma-separated (e.g. `labels enhancement,spec`) |
| `assignee` | — | Extra PR assignee (e.g. `assignee @me`) |
| `issue-labels` | — | Extra labels on linked issues |
| `issue-assignee` | — | Extra assignee on linked issues |

## WORKFLOW

Execute the skill checklist in order:

1. Steps 1–5 — branch → CI → commit → push → `gh pr create`.
2. **Step 6** — append linked issues to PR body (`Closes #N` unless `link-only`).
3. **Step 7** — `sync-pr-github-metadata.ps1`: PR → Project + labels + assignees; issues → board + Status + optional labels/assignees.

Report PR URL, linked issues, and what metadata was applied (or skipped / failed).
