# /create-pr — Branch, CI, commit, PR, link issues

Orchestrates the full PR workflow. Read and follow [`.cursor/skills/create-pr/SKILL.md`](mdc:.cursor/skills/create-pr/SKILL.md).

## INPUT

Optional arguments after the command:

| Form | Example | Meaning |
|------|---------|---------|
| Issue number(s) | `/create-pr 5` | Link issue **#5**; set Project Status |
| Multiple | `/create-pr 5 6` | Link **#5** and **#6** |
| Hash form | `/create-pr #5` | Same as `5` |
| Comma list | `/create-pr 5,6` | Same as `5 6` |
| Natural language | `create PR for issue 5` | Parse issue numbers from the message |

If omitted, skip issue linking and Project status update (Steps 5–6 only).

## FLAGS (parse from user message)

| Flag | Default | Meaning |
|------|---------|---------|
| `link-closes` | **yes** | Append `Closes #N` (links issue; closes on merge to `main`) |
| `link-only` | no | Append `Refs #N` instead (no auto-close on merge) |
| `skip-project-status` | no | Do not update Project Status field |
| `status` | config or `In review` | Override Project Status option name |

## WORKFLOW

Execute the skill checklist in order. When `--issue` / issue argument is present:

1. Complete Steps 1–5 (branch → CI → commit → push → `gh pr create`).
2. **Step 6** — append linked issues to PR body (`Closes #N` unless `link-only`).
3. **Step 7** — run `set-project-issue-status.ps1` for each issue → **In review** (or configured name).

Report linked issue numbers and whether Project status was updated (or which option was missing).
