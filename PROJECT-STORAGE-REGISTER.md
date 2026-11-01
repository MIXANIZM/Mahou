# PROJECT-STORAGE-REGISTER — Mahou (Model B v3)
**Batch:** `AGZ-MAH-MB-20261101-01`  
**Observed:** 2026-11-01, Windows host `HUAWEI-ULTRA-7`  
**CURRENT_STAGE:** `COPIED_VERIFIED_RUNTIME_UNTESTED`  
**CURRENT_STATUS:** `G1_COPY_SHA_GIT_PASS; G2_FIVE_EVIDENCE_VERIFIED; G3_KOPIA_NO_GO`  
**COPY_GATE:** `OWNER_COPY_FIRST_SUCCESS_NO_DELETE`  
**ORIGINALS:** preserved; no source modification, move, cleanup, or rollback performed by this worker.

## Canonical identity and scope
- Product GitHub repository: `MIXANIZM/Mahou` (legacy `master`; development `mixanizm-modern-v2.9.0.1`; protected separate product Draft PRs #2 and #18).
- Local source observed: `W:\AGZ\Repositories\Mahou`.
- Verified copied checkout: `W:\AGZ\Projects\mahou\Repository`.
- Verified Evidence location: `W:\AGZ\Projects\mahou\Evidence\Migration\AGZ-MAH-MB-20261101-01`.
- Scratch reserved: `W:\AGZ\Temp\Projects\mahou\AGZ-MAH-MB-20261101-01`.
- Proposed separate `Artifacts`, `LocalState` under `W:\AGZ\Projects\mahou` only if required, with provenance/security review.
- Quarantine **HOLD in place**, no blind material classification/move.
- Parent Kopia is owned by central BOSS, `W:\AGZ\Projects`; **do not launch snapshot, create per-project sources, or change retention**.

This route is from [M0 249-row source→target map](https://github.com/MIXANIZM/agz-remote-bridge/blob/adc3a59e8b3a0af88d1977239ae72ddf1e222d74/docs/p9a/storage-001/MODEL-B-M0-TARGET-ROUTING-249-20261031.csv). M0 initially marked `verified_slug=UNKNOWN`; the real local documents and GitHub metadata agree on project identity, and original local Git graph was subsequently verified under the actual Windows owner (see the current post-copy checkpoint).

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

## Owner-authenticated Git PRECHECK receipt — VERIFIED 2026-11-01

User ran SHA-pinned `AGZ-MAH-OwnerGit-Precheck-v1.ps1` under `HUAWEI-ULTRA-7\user` with all eight commands exit 0. Independently re-read the **actual on-host JSON** through AGZ Project Reader:
`W:\AGZ\Temp\Projects\mahou\AGZ-MAH-MB-20261101-01\AGZ-MAH-OwnerGit-Precheck-v1.json`
SHA-256 **`F1CD82AB4EC37612A13D5BE05878914C62E2940D9131344DCF8378DA65698827`** (Reader recomputed hash independently). Actor `HUAWEI-ULTRA-7\user`, UTC 2026-11-01 19:22:46.

- **LOCAL HEAD `5527f662cb844ba90d264f5b93cb27f8334bf295`**, *not* GitHub PR #18 head `ba40fd4c...`.
- Local branch `refs/heads/agz-mah-0019-remove-snippets-decouple-autoswitch`; native top-level `W:/AGZ/Repositories/Mahou`; common-dir `.git`; exactly **one registered worktree** pointing to that root.
- `git status --porcelain=v1 --untracked-files=all`: **16 tracked unstaged modifications** and **3 new untracked files**. All local-only bytes MUST survive. None of the 19 may be overwritten, reset, cleaned, stashed or substituted with GitHub remote output.
- Stash output empty. Refs enumerated with exit 0, do not assume remote refs equal local. `git fsck --full --no-reflogs` exit 0 with one informational **dangling tree `cf64a594f50925a894d6a72db6d66fc2f63eb41c`**; protect complete Git object store including dangling object. **No pruning**.
- Git owner-access gate now **PASS_GIT_READ_ONLY_8_OF_8**; this DOES NOT grant full `PRECHECK_PASS` or permission to COPY before reparse/hardlink/ADS/hash/consumer checks. No source modification or production cutover.

Remaining G0: private complete source SHA-256 inventory with hidden .git/ignored/dirty/untracked, hardlink/reparse/ADS containment and bounded Windows consumer/task/service inventory. Separate `AGZ-MAH-Inventory-Precheck-v1.ps1` staged in same own project Temp, SHA `B4CF100AB97E00F35AADD8DB45C12150512EF39FE902E0182E58EEC811F0386F`, PS5.1 AST 0, isolated fixture self-test PASS. This second helper performs **no source write/copy**.

## G0 source bytes, reparse, ADS and hardlink proof — VERIFIED 2026-11-01
**Supersedes the earlier G0 Git ownership blocker.** The owner-native Git check 8/8 PASSED; read-only filesystem inventory ran separately under `HUAWEI-ULTRA-7\AGZ-Agent`. This is a **technical pre-copy PASS with unresolved consumer checks**, not a migration completion receipt.

- Exact manifest: `W:\AGZ\Temp\Projects\mahou\AGZ-MAH-MB-20261101-01\AGZ-MAH-source-hash-baseline-v1.csv`. SHA-256 `5D28EA9D3A846067DA5C7374B1643FD59640CCB46E5EAAC873D76ACDC7250005`, independently revalidated by AGZ Project Reader.
- Private scan report: `AGZ-MAH-inventory-precheck-v1.json`, SHA-256 `9B6002C38A72F5E4CA92E91B652E65470DBF759E6057F910B7DCDECC9A99FB72`, independently revalidated.
- File coverage: **391/391 files including 171 in .git**, 104 directories, **41,404,389 bytes**. Per-file SHA-256, file size and mtime UTC baseline. Zero reparse points anywhere traversed; zero ADS; zero multiple hardlink aliases; zero external hardlink paths; zero fsutil failures; zero unstable hashes or hash errors. No files were copied or modified. At inventory time, source manifest stable.
- Consumer checks: visible processes PASS no Mahou; scheduled tasks COM scan PASS visible only; HKLM Run PASS no Mahou; alternate machine service registry path scan read 878 service entries with 0 Mahou references. Agent's Win32_Service CIM returned `CimException` (superseded by read-only service registry fallback). The original `user` HKCU Run hive is inaccessible to AGZ-Agent (`SecurityException`), so **OWNER HKCU and indirect/dynamic consumers OPEN**; no path cutover permission.
- W drive free-space observed **39,274,942,464 bytes**, target directory absent at time of check. Current standing authority is only independent **new-location COPY-FIRST preserving original**, never switching user app, Windows autorun, services or legacy paths.

### Historical owner-only COPY-FIRST v2 preparation — SUPERSEDED BY SUCCESSFUL EXECUTION
- On-host script: `W:\AGZ\Temp\Projects\mahou\AGZ-MAH-MB-20261101-01\AGZ-MAH-CopyFirst-v2.ps1`.
- [Exact source in this Draft PR](docs/host-migration/AGZ-MAH-MB-20261101-01/AGZ-MAH-CopyFirst-v2.ps1).
- SHA-256 `A71331AABBFD9728BF01E496A2312B7F0B1AA88AFD33CF623680B7E2E8EB84A2` independently verified by AGZ Project Reader. Windows PS5.1 AST errors=0. Fixture run with `robocopy` exit 1 and matching SHA-256: PASS. Separate self-test of tree enumeration and hash baseline: PASS. Initial `v1` superseded by `v2` after a self-test coverage correction, v1 preserved.
- Script requires `HUAWEI-ULTRA-7\user` (never agent). It rejects an existing project target, source identity drift, Git HEAD/status/refs/stash/worktree mismatch, any mismatch from 391 hashed original files, reparse changes or missing expected artifacts before COPY. Then only `robocopy /E /COPY:DAT /DCOPY:DAT /R:0 /W:0 /XJ` under the original owner; verify 391 source and target hashes, Git HEAD/refs/stash/status/fsck and preserve dangling Git tree and all 19 local changes.
- **On success only**, create five canonical on-host Evidence in `W:\AGZ\Projects\mahou\Evidence\Migration\AGZ-MAH-MB-20261101-01`. On failure, preserve original and any partial target; save a fail receipt in project Temp and prohibit cleanup.
- **Historical pre-execution statement, superseded by the verified 2026-11-01 post-copy receipt below.** `KOPIA_PARENT_NOT_RUN`. Owner's explicit prompt authorizes scoped project-local COPY-FIRST only after PASS; exact interactive principal is required for safe target ownership. Runtime/remote/consumers/restore remain OPEN.


## Post-copy verification — 2026-11-01 (CURRENT, supersedes G0 and ready-only sections)

**G0 = PASS_TECHNICAL_CONSUMERS_OPEN; G1 = COPY_SHA256_GIT_PASS; G2 = FIVE_EVIDENCE_CREATED_AND_INDEPENDENTLY_HASH_VERIFIED; G3 = KOPIA_NO_GO; G4 = RESTORE_RUNTIME_AND_CUTOVER_PENDING.**

Under authenticated Windows operator `HUAWEI-ULTRA-7\user`, SHA-pinned `AGZ-MAH-CopyFirst-v2.ps1` ran with all phases PASS. Source remained at `W:\AGZ\Repositories\Mahou`, target exists `W:\AGZ\Projects\mahou\Repository`. Robocopy exit 1 = successful copy. Source pre-hash 391/391, source post-hash 391/391, target 391/391 match against immutable pre-copy manifest; all local Git status/HEAD/refs/stash/fsck preserved. Exact local HEAD `5527f662cb844ba90d264f5b93cb27f8334bf295`; 16 modified tracked + 3 untracked, one dangling tree retained. **No merge, Git repair or production modification.**

**Independent read-only audit after Owner execution:**
- AGZ Project Reader re-hashed all five on-host Evidence; SHA-256 values match the exact Owner console.
- Windows Remote Desktop Commander (`HUAWEI-ULTRA-7\AGZ-Agent`) separately confirmed original and target exist and file ownership of original checkout, target checkout and target `.git` is `HUAWEI-ULTRA-7\user`. Read-only parse of canonical manifest: 391 records, 0 mismatches in copied/matched/original-preserved and source-vs-target hash claims, 171 `.git` files, 41,404,389 bytes. Exactly five Evidence files exist.
- Operator UTC in receipt: `2026-11-01T19:37:20.5435370Z`. This is **project-scope copy proof**, not proof of an independent off-drive backup.

**The exact FIVE on-host Evidence** under `W:\AGZ\Projects\mahou\Evidence\Migration\AGZ-MAH-MB-20261101-01`:

| Evidence | SHA-256 (recomputed by Reader) |
|---|---|
| `source-target-manifest.csv` | `062039A7810A9C388E748C6E88B92832A54279BE4F6E91F60CC64D8C9C465D88` |
| `quarantine-manifest.csv` | `8F0F8A0DD45EA9F8C52411D85210D76EB656BC33102EEFC814BCFDE38F506705` |
| `dependencies.json` | `8FEBD477483DAF99316BBCD5C87A3AAEC0951A2C41FAF3100147460F5BDD38D9` |
| `verification-report.md` | `64099E56770B4FD9A19A08924829E902CA4F0C75B3A9A8799ABA2BFB73DEF63F` |
| `migration-receipt.json` | `E61330E3511D844E5C948321C76C25FC469AF4AA79E9E1393838AEA9573179AB` |

**Still OPEN / NO-GO:** consumer path audit in original owner's `HKCU` and indirect runtime dependencies; no service or user autostart cutover, no application launch/smoke from copied checkout; origin/upstream network read-only check not run; the single shared parent Kopia snapshot `W:\AGZ\Projects` has **not** been authorized or run, snapshot/root IDs remain null; no isolated restore performed. No cleanup, no source deletion, no Quarantine move, no promotion of Draft PR, no release. Parent Kopia requires new explicit Owner GO and central BOSS coordination. An older parent snapshot cannot be treated as coverage of this new copy.

**Owner physical preservation:** original `W:\AGZ\Repositories\Mahou` and new `W:\AGZ\Projects\mahou\Repository` both stay in place without deleting either. Retain local-only uncommitted work and both .git stores until independently recovered/backed up and consumers safely reviewed. **CURRENT_STAGE=COPIED_VERIFIED_RUNTIME_UNTESTED.**
