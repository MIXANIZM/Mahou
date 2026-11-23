# ADR: AGZ Model B host storage alignment — Mahou

Status: **DOCUMENTED / READ-ONLY DIAGNOSTIC PENDING** (2026-11-24); docs-only, no physical migration authorization.
Project: `MIXANIZM/Mahou`; project slug: `mahou`; host: `HUAWEI-ULTRA-7`.
Authority: current Owner direction (2026-11-24) > project-specific pinned governance > host-specific accepted Model B decisions and verified evidence. This ADR does not retroactively change historical receipts.

## 1. Authoritative sources and historical conflict

- [Owner-directed 2026-10-31 host-specific Model B execution](https://github.com/MIXANIZM/agz-remote-bridge/blob/boss/p9a-permanent-handoff-20261025/docs/p9a/AGZ-HOST-STORAGE-MIGRATION-EXECUTION-20261031.md).
- [Accepted 2026-11-05 host storage state and limits](https://github.com/MIXANIZM/agz-remote-bridge/blob/boss/p9a-permanent-handoff-20261025/docs/p9a/AGZ-PERMANENT-BOSS-STATE-DECISIONS-20261105.md).
- [Historical 2026-10-28 target/design](https://github.com/MIXANIZM/agz-remote-bridge/blob/boss/p9a-permanent-handoff-20261025/docs/p9a/AGZ-HOST-STORAGE-TARGET-POLICY-MIGRATION-BACKUP-20261028.md).
- [Earlier Host Execution WORKSPACE-STORAGE](https://github.com/MIXANIZM/agz-host-execution/blob/main/WORKSPACE-STORAGE.md).
- [Central migration Issue #99](https://github.com/MIXANIZM/agz-remote-bridge/issues/99), including scoped Mahou G1 receipt context.
- Local prior evidence: `W:\AGZ\Projects\mahou\Evidence\Migration\AGZ-MAH-MB-20261101-01\{verification-report.md,migration-receipt.json,dependencies.json,source-target-manifest.csv,quarantine-manifest.csv}`.
- Local disk-audit context: `W:\AGZ\Evidence\DiskAudit-20261123\REPORT.md`; read on 2026-11-24; global inventory has access errors and skipped reparse points, not a per-project complete audit.
- [Mahou supervisor handoff](SUPERVISOR-HANDOFF-2026-11-23.md), docs Draft PR #20; [implementation PR #18](https://github.com/MIXANIZM/Mahou/pull/18).

The **earlier** Host Execution placement `Projects\<slug>\Temp\<Task-ID>` is historical. Owner accepted a **later host-specific placement** on 2026-10-31: `W:\AGZ\Temp\Projects\<slug>\<Task-ID>`. The latter governs Mahou's future W: migration. Do not silently rewrite historical records, and do not declare old Temp contents disposable by naming.

Pinned Agatzub Development Ruleset for this project remains **v2.7.0**, exact commit `ba60623aec67c57d46bda7ce2b291a823de2d4ea`; the newer stable v2.7.1 and draft v2.8.0 are **not automatically adopted**. This project ADR records host-specific storage classification, not central normative rule promotion.

## 2. Expected project-specific Model B paths (design, not a claim that all exist)

| Class | Host path | Meaning |
| --- | --- | --- |
| Durable Git | `W:\AGZ\Projects\mahou\Repository` | Canonical development checkout and Git, subject to native verification |
| Registered worktrees | `W:\AGZ\Projects\mahou\Worktrees\<Task-ID>` | Task-bound Git worktrees, if separately registered |
| Evidence | `W:\AGZ\Projects\mahou\Evidence` | Accepted SHA/Git/provenance/QA receipts |
| Accepted artifacts | `W:\AGZ\Projects\mahou\Artifacts` | Valuable retained build/recovery packages, not a download scratch folder |
| Durable state | `W:\AGZ\Projects\mahou\LocalState` | Only proven necessary project-owned durable state |
| Reproducible scratch | `W:\AGZ\Temp\Projects\mahou\<Task-ID>` | Proven regenerable task-owned scratch, **not** presumed safe to delete |
| Historical uncertain | `W:\AGZ\Quarantine\{Repositories,Worktrees,Temp,Other}` | Proposed protected HOLD classification; no transfer or deletion authorized |
| Shared installed tools | `W:\AGZ\Tools` | Separately governed installed operational tools, **not** the Git repository |
| Host-managed caches/runtimes | `W:\AGZ\Cache`, `W:\AGZ\Runtimes` | Only documented host-owned shared reusable assets |

A development checkout and an **installed operational Mahou executable** are different objects. A release/test archive is an artifact; a running installed executable or per-user Mahou data is a separately scoped consumer/operational state. A top-level `W:\AGZ\Tools` listing did not identify a `mahou` directory, but this is **not** proof that Mahou has no other installation. Application profile paths, HKCU autorun, runtime consumer processes, user-private configuration and secrets are **UNKNOWN / NO_MOVE** until independently verified without exposing private values. The project-specific runtime/Windows smoke remains governed by the separate Mahou product gates.

## 3. Accepted historical Mahou preservation — do not repeat without cause

The accepted **2026-11-01** Mahou-only receipt `AGZ-MAH-MB-20261101-01` reports:

- Original: `W:\AGZ\Repositories\Mahou`; canonical copy: `W:\AGZ\Projects\mahou\Repository`.
- **391/391 source-target SHA-256 matches**, original left intact; Robocopy `/E /COPY:DAT /DCOPY:DAT /R:0 /W:0 /XJ`, exit 1.
- Native Git HEAD/refs/stash/worktree/full fsck **PASS at that checkpoint**; 16 modified and 3 untracked files preserved (19 changes); one dangling tree `cf64a594f50925a894d6a72db6d66fc2f63eb41c` recorded in dependencies.
- **Historical local HEAD `5527f662cb844ba90d264f5b93cb27f8334bf295` (2026-11-01 evidence), NOT an assertion of current on-disk HEAD**.
- Receipt classifies `CURRENT_STAGE=COPIED_VERIFIED_RUNTIME_UNTESTED`, `G0=PASS_TECHNICAL_CONSUMERS_OPEN`, `G1=COPY_SHA256_GIT_PASS`, `G3=KOPIA_NO_GO`, `G4=RESTORE_RUNTIME_AND_CUTOVER_PENDING`.
- Runtime smoke, network/origin, application consumers, Kopia parent snapshot and isolated restore were **NOT RUN or NOT AUTHORIZED** under that scoped receipt.
- The historical quarantine manifest was empty; that does **not** certify that all present legacy materials are disposable.
- Evidence was read on 2026-11-24. Do **not** rerun the accepted complete copy or amend historical receipts without a specific new cause.

As separately verified in GitHub on 2026-11-24, protected implementation PR #18 has **remote** head `ba40fd4c588b7c48b64bcc2cdfcefbed0f39c0d6`, tree `a28af6cc694e6e857f6b0bb6844ed4113b55fc04`; development PR #2 head `f02909611eb9a4502e9fe4d8fda9009922f352fd`; docs PR #20 head `e98f24bdd248a40f81bec4740ab73b8db835dd74`. Do not infer current local Git equality from the older receipt or replace original local-only work with a GitHub clone.

## 4. Observed host paths / unknowns (scoped initial read)

Read-only `list_directory` on 2026-11-24 observed:

- `W:\AGZ\Projects\mahou\Repository` and `...\Evidence\Migration\AGZ-MAH-MB-20261101-01`.
- `W:\AGZ\Repositories\Mahou` still exists.
- `W:\AGZ\Temp\Projects\mahou\AGZ-MAH-MB-20261101-01` contains migration scripts, JSON prechecks and fixture directories. It is **UNRESOLVED / NO_MOVE / NO_DELETE** until actual regeneration and unique-evidence checks.
- No `Worktrees`, `Artifacts` or `LocalState` directory was listed in the canonical project root. Do not create empty directories for appearance alone.
- Separate global disk audit reported **657 unique registered worktree paths, 171 below Temp**, **818 access/stat errors** and **46 skipped reparse points**, globally; these counts are **not** Mahou-specific.
- No completed *current* independent Mahou-specific native Git audit, orphan/stash/local-only assessment, live-consumer audit, backup restore or deletable-byte determination has been accepted under this 2026-11-24 alignment task.

## 5. Read-only worker scope / evidence required

Assign **one separate independent read-only Git + storage worker**, not the implementation or artifact-refresh executor. Before host access, `bridge_health` must report `HUAWEI-ULTRA-7\\agzbridge`, non-elevated, upstream ready, kill switch false. The worker must read this ADR and the above authority, preserve the accepted G1 receipt, and avoid a global rescan.

Target only proven Mahou-owned locations and narrow read-only dependency references. Check:

1. Current canonical/legacy Git repository identity, `gitdir`/`commondir`, HEAD/TREE, refs, stashes, reflogs, dangling objects, fsck and local-only commits, with no lazy fetch/locks or repair; staged/unstaged/untracked/ignored unique content.
2. Existing registered worktrees and bidirectional pointer/admin integrity, if any; inspect ambiguous ownership instead of inferring by names.
3. Candidate Mahou-owned Temp/legacy/accepted artifact/evidence locations, actual consumers (including runtime/installed executable vs dev Git), ownership, contents **only as needed without reading private credentials**, and SHA-256 of proposed exact duplicates.
4. GitHub source/recovery options, independent stores, actual backup coverage and isolated restore evidence; **do not** infer that B2 one-file PASS elsewhere covers Mahou's old paths or dirty files.
5. Per-object manifest: `source → owner → class → proposed target → hashes/HEAD/refs/status → consumers → backup proof → operation → Owner GO → rollback → risk → potential bytes`.

Required: `GIT_INTEGRITY_REPORT`, `UNIQUE_AND_LOST_DATA_MATRIX`, `PROJECT_STORAGE_MANIFEST`, `SHA256_DUPLICATE_EVIDENCE`, `RECOVERY_SOURCE_MATRIX`, `MODEL_B_MIGRATION_PLAN`, `CLEANUP_CANDIDATES`, `BACKUP_COVERAGE_AND_RESTORE_EVIDENCE`, and blockers. Findings are **CANDIDATES**, not permissions to move/delete. If the accepted G1 proof is sufficient for an object, cite it instead of rescanning.

## 6. Preservation / Owner-go boundaries

**COPY-FIRST / SHA-256 VERIFY / ROLLBACK / NO-DELETE.** Protect all original Git object stores, refs, HEAD, index, stashes, reflogs, local-only commits, 16+3 historical dirty/untracked files, necessary ignored unique material and approved evidence. Do not overwrite either copy, reset/clean, change `.git` registration, run `git prune` / `worktree prune`, or add automatic compatibility junctions.

Separate future decisions:
- A **recovery/migration** allowlist for exact source/destination/operation, current preimage, expected hashes and new isolated target; **only after exact Owner GO**.
- A later **legacy retirement/deletion** candidate allowlist after independent restore, consumer cutover, backups and **Central Storage BOSS** review; another exact Owner GO is required.
- Legacy `W:\AGZ\Repositories\Mahou` is **KEEP / NO_DELETE** pending live-consumer and independent recovery proof.
- `W:\AGZ\Temp\Projects\mahou\AGZ-MAH-MB-20261101-01` is **UNRESOLVED / NO_DELETE** pending classification.
- No changes to Kopia/B2 sources, schedules or retention; ACL; processes, VM or DB; tool installation; production or system/user-private state.

The MaHou product development queue remains separate: user snippets removed, standalone AutoSwitch, exact modern Notepad fail-closed, no modification of protected Draft PRs #1/#2/#3/#18/#20. Product task **AGZ-MAH-0022** remains a separate provenance-only build artifact refresh because the old x64 artifact has expired. **Neither this ADR nor the storage read-only audit authorizes build/workflow/runtime changes or physical Windows smoke.**

## 7. Current gate / handoff

```text
MODEL_B_PROJECT_DOCUMENTED = DOCS_DRAFT_PENDING_REVIEW
HISTORICAL_PRESERVATION = G1_391_OF_391_SHA_ACCEPTED_WITH_LIMITATIONS
CURRENT_GIT_INTEGRITY = PENDING_INDEPENDENT_READ_ONLY
RECOVERY = UNKNOWN / NEEDS_SCOPED_EVIDENCE
MODEL_B = CANONICAL_COPY_PRESENT / CONSUMER_CUTOVER_PENDING
BACKUP = PROJECT_RESTORE_NOT_VERIFIED
CLEANUP = CANDIDATES_ONLY / NO_APPROVED_DELETE
OWNER_WRITE_GO = NOT_GRANTED
GLOBAL_CLEANUP_GATE = HOLD
```

Permanent Project BOSS reviews the worker independently, publishes one scoped acceptance/corrections receipt and prepares exact restoration versus deletion decisions separately. Central Storage BOSS reviews only **accepted** project proposals before any destructive operation. Owner alone grants final per-operation authorization. Until then **STOP** at read-only/doc gate.
