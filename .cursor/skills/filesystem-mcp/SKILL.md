---
name: filesystem-mcp
description: Reads, searches, and lists files in the repo via the Cursor filesystem MCP server within an allowed directory root. Use when batch-reading paths, searching the tree with MCP search_files, listing allowed roots, or when the user mentions filesystem MCP — not for normal single-file edits (use built-in Read/Grep tools first).
---

# Filesystem MCP (trading-simulator)

Scoped file access through the **`filesystem`** MCP server in [`.mcp.json`](../../../.mcp.json).

## Configuration

```json
"filesystem": {
  "command": "npx",
  "args": [
    "-y",
    "@modelcontextprotocol/server-filesystem",
    "/Users/duyvt/trading-simulator"
  ]
}
```

| Item | Value |
|---|---|
| Server id | `filesystem` |
| Package | `@modelcontextprotocol/server-filesystem` |
| Allowed root | Last CLI arg (single directory tree) |
| Prerequisites | Node/npx available |

**Path must match this machine.** The sample root uses a macOS path. On Windows, set the last arg to the workspace absolute path (e.g. `c:/Users/duyvt/trading-simulator`). Call **`list_allowed_directories`** if access is denied.

## MCP vs built-in tools

| Prefer built-in | Prefer filesystem MCP |
|---|---|
| Read / edit one file | `read_multiple_files` for many paths at once |
| Grep / Glob / SemanticSearch | `search_files` with MCP-specific patterns |
| Write / StrReplace / Delete in repo | Rarely — MCP `write_file` overwrites without merge |
| Explore known paths | `directory_tree`, `list_directory_with_sizes` |

Default: use Cursor **Read**, **Grep**, and **Glob** for implementation work. Use this MCP when the user attaches the skill, asks for MCP filesystem operations, or batch/tree search is clearer through MCP.

## Read tools (safe default)

| Tool | Use |
|---|---|
| `list_allowed_directories` | Confirm sandbox root before other calls |
| `read_text_file` | One file; optional `head` or `tail` (not both) for large logs |
| `read_multiple_files` | Batch read; failures per file do not stop the batch |
| `read_media_file` | Images/binary |
| `list_directory` | One level listing (`[FILE]` / `[DIR]` prefixes) |
| `list_directory_with_sizes` | Listing + sizes |
| `directory_tree` | Recursive tree under a path |
| `search_files` | Find paths by pattern under allowed root |
| `get_file_info` | Size, mtime, type |

Use **absolute paths** under the allowed root.

## Write tools (confirm first)

| Tool | Risk |
|---|---|
| `write_file` | Overwrites entire file |
| `edit_file` | Patched edits; can fail if file changed |
| `create_directory` | Creates dirs |
| `move_file` | Rename/move |

Do **not** use MCP writes for routine code changes — use the agent edit tools so diffs stay reviewable. Use MCP writes only when the user explicitly requests MCP file operations.

## Repo map (under allowed root)

| Path | Contents |
|---|---|
| `docs/` | PRD, TECHNICAL, DATABASE, memory |
| `src/` | .NET AppHost, Api, MatchingEngine, Domain, Infrastructure |
| `web/` | React + Vite frontend |
| `tests/` | Unit and integration tests |
| `.cursor/` | Rules, skills, commands |

Source of truth pointers: `core.mdc` → `docs/PRD.md`, `docs/TECHNICAL.md`, `docs/DATABASE.md`.

## Workflow

1. `list_allowed_directories` if paths fail.
2. Narrow with `search_files` or `list_directory` before reading huge trees.
3. `read_text_file` with `head`/`tail` for logs and generated output.
4. Summarize findings; do not dump entire large files into chat.

## Safety

- Operations cannot escape the configured root.
- Skip secrets: `.env`, credentials, user-specific paths outside the repo.
- Do not read `node_modules/`, `bin/`, `obj/` unless debugging — prefer targeted paths.

## Tool reference

See [reference.md](reference.md) for parameter notes and example paths.
