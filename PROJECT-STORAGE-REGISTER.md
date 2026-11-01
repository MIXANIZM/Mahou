# PROJECT-STORAGE-REGISTER — Mahou (Model B v3)
**Batch:** `AGZ-MAH-MB-20261101-01`  
**Observed:** 2026-11-01, Windows host `HUAWEI-ULTRA-7`  
**CURRENT_STAGE:** `G0_PRECHECK_INCOMPLETE`  
**CURRENT_STATUS:** `BLOCKED_GIT_OWNERSHIP`  
**COPY_GATE:** `BLOCKED_NO_COPY`  
**ORIGINALS:** preserved; no source modification, move, cleanup, or rollback performed by this worker.

## Canonical identity and scope
- Product GitHub repository: `MIXANIZM/Mahou` (legacy `master`; development `mixanizm-modern-v2.9.0.1`; protected separate product Draft PRs #2 and #18).
- Local source observed: `W:\AGZ\Repositories\Mahou`.
- Exact proposed new checkout: `W:\AGZ\Projects\mahou\Repository`.
- Own Evidence destination only **after valid copy**: `W:\AGZ\Projects\mahou\Evidence\Migration\AGZ-MAH-MB-20261101-01`.
- Scratch reserved: `W:\AGZ\Temp\Projects\mahou\AGZ-MAH-MB-20261101-01`.
- Proposed separate `Artifacts`, `LocalState` under `W:\AGZ\Projects\mahou` only if required, with provenance/security review.
- Quarantine **HOLD in place**, no blind material classification/move.
- Parent Kopia is owned by central BOSS, `W:\AGZ\Projects`; **do not launch snapshot, create per-project sources, or change retention**.

This route is from [M0 249-row source→target map](https://github.com/MIXANIZM/agz-remote-bridge/blob/adc3a59e8b3a0af88d1977239ae72ddf1e222d74/docs/p9a/storage-001/MODEL-B-M0-TARGET-ROUTING-249-20261031.csv). M0 initially marked `verified_slug=UNKNOWN`; the real local documents and GitHub metadata agree on project identity, but **local Git graph is still unverified**.

## Instrumented read-only findings
| Check | Value | Authority / limitations |
|---|---|---|
| Source | Exists, 48 first-level items including physical `.git` | AGZ Project Reader (read-only) |
| Target | Absent, including `W:\AGZ\Projects\mahou` parent | Remote Desktop Commander, Windows PowerShell |
| Temp task project | Absent at time of check | Windows PowerShell |
| Caller | `HUAWEI-ULTRA-7\AGZ-Agent` | Windows identity |
| Git version | 2.55.0.windows.3 | Native `git --version` |
| Git PRECHECK | `fatal: detected dubious ownership in repository` | Native Git invocation as AGZ-Agent, **not proof of repository corruption** |
| Git HEAD marker | `ref: refs/heads/agz-mah-0019-remove-snippets-decouple-autoswitch` | Raw `.git/HEAD`; commit SHA **UNKNOWN** |
| Git worktree admin dir | `.git\worktrees` not present | File existence only, not exhaustive cross-repository link proof |
| Alternate object store marker | `.git\objects\info\alternates` not present | File existence only |
| Reparse at direct source children | None observed | First level only, **not** recursive clearance |
| Running `Mahou` process | None observed | Point-in-time Windows query; consumer/integration unknown |
| GitHub PR #18 | Remote head `ba40fd4c588b7c48b64bcc2cdfcefbed0f39c0d6` | GitHub only; **not** the local HEAD |
| GitHub product PR #2 | Dev head `f02909611eb9a4502e9fe4d8fda9009922f352fd` | Separate product Draft PR, left untouched |

**Known protection:** Mahou keyboard hooks/autorun/configuration and potentially hardcoded paths need independent consumer review. Functional product evidence from PR #18 is not migration smoke. No `safe.directory` overrides (even ephemeral), ACL changes, impersonation or Git metadata repair.

## Gates and acceptance
- **G0:** BLOCKED — interactive owner-native Git PRECHECK, complete `HEAD/common-dir/worktrees/refs/stash/status/dirty/untracked/ignored/fsck`, all file SHA-256, reparse/hardlink/ADS checks, task/process/path consumers and stable manifest are missing. Do not infer PASS from raw `.git/HEAD`.
- **G1 COPY-FIRST:** NOT STARTED. No copy to absent target, no GitHub clone substituted for local bytes, no `robocopy`, no source change. Eligible only after full owner-authenticated G0 PASS, scope isolation, and backup/recoverability gates per v3.
- **G2 five physical Evidence:** NOT CREATED / PENDING. After verified copy create genuine on-host: `source-target-manifest.csv`; `quarantine-manifest.csv`; `dependencies.json`; `verification-report.md`; `migration-receipt.json`. Do not create fabricated empty success receipts.
- **G3 Kopia:** NOT STARTED / NO OWNER GO. A snapshot predating this copy cannot cover it; only central shared parent snapshot may be scheduled on independent GO, followed by scoped restored-file SHA/Git proof.
- **G4 recovery/cleanup:** NOT STARTED, no deletes, no test folder cleanup, no producer/consumer cutover.
- **Runtime:** NOT RUN from new target. Product Draft PR #18 still needs its separate physical two-stage smoke. Do not confuse with this migration.
- **PRODUCTION:** UNCHANGED.

## Next smallest safe action
Run **one reviewed read-only PRECHECK under the interactive original repository owner** `HUAWEI-ULTRA-7\user`, avoiding `safe.directory` or ACL changes. Capture exact Windows Git output, all source paths/sha and consumer coverage. If ownership or external Git links cannot be verified, keep `BLOCKED_SHARED_DEPENDENCY` / `PRECHECK_INCOMPLETE`. Continue COPY-FIRST only after technical PASS and proven project-scope independence. This worker has not requested any parent Kopia snapshot.

## References
- [Universal Project WORKER v3](https://github.com/MIXANIZM/agz-remote-bridge/blob/boss/p9a-permanent-handoff-20261025/docs/p9a/AGZ-MODEL-B-UNIVERSAL-PROJECT-WORKER-PROMPT-20261031.md)
- [Permanent BOSS handoff](https://github.com/MIXANIZM/agz-remote-bridge/blob/boss/p9a-permanent-handoff-20261025/docs/p9a/AGZ-PERMANENT-BOSS-RECOVERY-HANDOFF-20261101.md)
- [Central BOSS issue #99](https://github.com/MIXANIZM/agz-remote-bridge/issues/99)
- [Product Draft PR #18](https://github.com/MIXANIZM/Mahou/pull/18)
- Issues are **disabled** in `MIXANIZM/Mahou` (GitHub API 410); this dedicated Draft PR plus `ISSUES.md` is the project-local tracking record.

## Prepared next-gate owner helper — tested, NOT executed under Owner

The only new host file staged by the worker is an **independent Git-read helper** under its own isolated scratch:
`W:\AGZ\Temp\Projects\mahou\AGZ-MAH-MB-20261101-01\AGZ-MAH-OwnerGit-Precheck-v1.ps1`.

Its exact source is [AGZ-MAH-OwnerGit-Precheck-v1.ps1](docs/host-migration/AGZ-MAH-MB-20261101-01/AGZ-MAH-OwnerGit-Precheck-v1.ps1). Host SHA-256 **`57102A52A4A0520B0460D64674BCE16E4E9D5F7313B881CA8301CFDCDBA38E44`**; Windows PowerShell 5.1 AST `0 errors`; native Git self-test PASS; negative-principal test under `AGZ-Agent` correctly returned `USER_AUTH_REQUIRED` exit 1. Those are tooling checks only, **not** actual original repository Git PRECHECK and not a five-Evidence success.

The helper refuses to run except as `HUAWEI-ULTRA-7\user`, checks target vacancy, runs native Git read-only HEAD/top-level/common-dir/worktrees/status/stash/refs/fsck with bounded timeouts and writes its **private receipt** only to own task Temp. It never copies source files, modifies ACL/safe.directory, runs parent Kopia or does producer cutover. **Output remains G0 incomplete** until full SHA/reparse/hardlink/dependency/consumer review, even when Git-read reports PASS.
