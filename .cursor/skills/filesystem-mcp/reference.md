# Filesystem MCP — tool reference

Official package: `@modelcontextprotocol/server-filesystem`. All paths must lie under an entry from `list_allowed_directories`.

## `read_text_file`

```json
{ "path": "/absolute/path/to/file.md", "head": 80 }
```

```json
{ "path": "/absolute/path/to/log.txt", "tail": 50 }
```

Do not set both `head` and `tail`.

## `read_multiple_files`

```json
{ "paths": [
  "/absolute/path/docs/PRD.md",
  "/absolute/path/docs/TECHNICAL.md"
]}
```

## `list_directory`

```json
{ "path": "/absolute/path/src" }
```

## `directory_tree`

```json
{ "path": "/absolute/path/.cursor/skills" }
```

## `search_files`

Use for filename/path discovery when Grep is not attached. Pattern syntax follows the MCP server implementation (glob-style segments).

Example intents:

- Find AppHost: search under `src` for `*AppHost*`
- Find migrations: search under `src` for `**/Migrations/**`
- Find React routes: search under `web/src`

## `get_file_info`

```json
{ "path": "/absolute/path/docs/DATABASE.md" }
```

## Trading-simulator paths (relative → prefix with allowed root)

| Relative | Typical use |
|---|---|
| `docs/PRD.md` | Product scope |
| `docs/TECHNICAL.md` | Architecture, CQRS |
| `docs/DATABASE.md` | PostgreSQL schema |
| `docs/memory/current-status.md` | Session checklist |
| `src/` | Backend solution (when present) |
| `web/` | Frontend |
| `.cursor/rules/core.mdc` | Invariants |
| `.mcp.json` | MCP server definitions |

## Windows vs macOS root

`.mcp.json` may list:

```
/Users/duyvt/trading-simulator
```

On Windows, update the last `args` entry to:

```
c:/Users/duyvt/trading-simulator
```

Forward slashes are accepted by the Node server on Windows.
