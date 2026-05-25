# Known Issues

Track open bugs, risks, and technical debt that may impact planning/build quality.

## Open

### ISSUE-REG-CONCURRENT-500: Concurrent register race may return 500
- Severity: Low
- Area: Auth | API
- First seen: 2026-05-25
- Description: Two simultaneous `POST /api/users` with the same username can rarely yield **500** `INTERNAL_ERROR` instead of **422** `USERNAME_TAKEN` when both pass exists-check before either commits. DB unique indexes still prevent a second wallet.
- Expected behavior: At most one account; second request ideally **422**.
- Actual behavior: Second response may be **422** or **500**; wallet count stays ≤ 1 (covered by `RegisterUser_ParallelSameUsername_AtMostOneWallet`).
- Workaround: User retries; exists-check returns **422** on retry.
- Suggested fix (deferred): Postgres unique-violation mapper (Plan B in Story 4 plan).
- Related files: `RegisterUserCommandHandler.cs`, `RegisterUserTransientFailureTests.cs`, `register-form.tsx`

## Fixed

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
