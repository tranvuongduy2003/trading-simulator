# Known Issues

Track open bugs, risks, and technical debt that may impact planning/build quality.

## Open

### ISSUE-TEST-FILELOCK-CS2012: Intermittent build artifact lock in consecutive test runs
- Severity: Low
- Area: Process | Tests
- First seen: 2026-05-28
- Description: Back-to-back `dotnet test` commands can occasionally hit CS2012 file lock errors under `obj/Release/net10.0` before retry succeeds.
- Workaround: Re-run the same test command; usually succeeds immediately.
- Suggested fix: Investigate concurrent process/file-handle contention in local environment.
- Related files: `src/**/obj/Release/net10.0/*.dll`

## Fixed

### ISSUE-REG-CONCURRENT-500: Concurrent register race may return 500
- Severity: Low
- Area: Auth | API
- First seen: 2026-05-25
- Fixed on: 2026-05-29
- Description: Two simultaneous `POST /api/users` with the same username could rarely return **500** `INTERNAL_ERROR` for the loser request.
- Resolution: Added persistence-exception mapping for Postgres unique-violation (`23505`) constraints `ux_users_username` / `ux_users_email` to return **422** (`USERNAME_TAKEN` / `EMAIL_TAKEN`) from command pipeline.
- Verification: `RegisterUser_ParallelSameUsername_AtMostOneWallet` now requires non-created response status **422** and keeps wallet count invariant (`<= 1`).
- Related files: `src/Application/Behaviors/UnitOfWorkBehavior.cs`, `src/Infrastructure/Persistence/UnitOfWork.cs`, `tests/Api.IntegrationTests/Users/RegisterUserTransientFailureTests.cs`

## Template
### ISSUE-XXX: Short title
- Severity: High | Medium | Low
- Area: API | UI | Auth | DB | Infra | Process | Other
- First seen: YYYY-MM-DD
- Description:
- Repro steps:
  1.
  2.
  3.
- Expected behavior:
- Actual behavior:
- Workaround:
- Suggested fix:
- Related files:
