# Permanent supervisor handoff — MIXANIZM Mahou

Snapshot: **2026-11-23**  
Record: **AGZ-MAH-0021 / docs-only supervisor transfer**  
Authority: current Owner instructions > exact pinned project governance > verified current GitHub state > recorded evidence. Old chat messages are navigation, not authority.

## First actions for the incoming BOSS

1. Take the role of **the one Permanent BOSS / supervisor** for `MIXANIZM/Mahou`. Speak with the Owner in Russian. Keep repository records and technical handoffs in English.
2. Read `AGENTS.md`, `AGATZUB-RULES.md`, `PROJECT_STATE.md`, `UNIVERSAL-CHAT-HANDOFF.md`, `ISSUES.md`, `README-MIXANIZM.md`, `ARCHITECTURE.md`, `SECURITY.md`, `TESTING.md`, `docs/RELEASE-READINESS.md`, and `docs/PR2-DESCRIPTION-PROPOSAL.md`; follow all referenced relevant modules. Read the current `master` first to identify legacy status; the governance and modern development documentation are on `mixanizm-modern-v2.9.0.1` and the active PR #18 source.
3. Check current GitHub branch SHA, Draft PRs #1, #2, #3, #18, this stacked docs PR, exact CI records, artifact availability and new owner feedback. Do not assume the 2026-11-23 snapshot is still current.
4. Apply the **pinned** ruleset `v2.7.0`, repository `MIXANIZM/agatzub-development-rules`, commit `ba60623aec67c57d46bda7ce2b291a823de2d4ea`, tag `v2.7.0`. On 2026-11-23 the central `main/VERSION` and `v2.7.1` tag both say `2.7.1`: report the newer stable edition, but **do not upgrade the pin** without a separate reviewed branch/PR and Owner approval.
5. Do not launch new implementation or expand scope. The next bounded action is to restore a safely verifiable, downloadable Windows x64 candidate from the **exact accepted source commit**, then request physical **Stage A** only.

## Verified GitHub snapshot

| Subject | Snapshot |
| --- | --- |
| Legacy default | `master`; not the modern development line |
| Main development | branch `mixanizm-modern-v2.9.0.1` at `f02909611eb9a4502e9fe4d8fda9009922f352fd` |
| Main development PR | #2, open / Draft / unmerged, target `master` |
| Protected historical PR | #1, open / Draft / unmerged, head `8c8a79686952717344ca70cf3cfc58e68e57b039` |
| Protected Notepad PR | #3, open / Draft / unmerged, head `b547c0e00488b2e588e06cc25a57fdc3a5b4d1d3`; **not accepted as a positive adapter** |
| Active implementation | [Draft PR #18](https://github.com/MIXANIZM/Mahou/pull/18), open / Draft / unmerged, base `mixanizm-modern-v2.9.0.1` |
| PR #18 exact head | `ba40fd4c588b7c48b64bcc2cdfcefbed0f39c0d6` |
| PR #18 exact tree | `a28af6cc694e6e857f6b0bb6844ed4113b55fc04` |
| Runtime line | `2.9.0.1-dev`; unchanged |
| This handoff | separate documentation-only branch and stacked Draft PR targeting PR #18 branch; PR #18's tested source/head **must not move** just to record handoff |

This documentation branch is intentionally stacked onto PR #18, not merged into or directly pushed onto PR #18, PR #2 or `master`. Reconcile/merge only with permission and after applicable smoke gates.

## Owner's accepted product decisions

- `USER_SNIPPETS_REMOVED`: user-defined snippets are unwanted, not repaired or reintroduced. `AGZ-MAH-0018` found `SNIPPETS_TRIGGER_REPLACEMENT_FAIL` in physical Windows testing; further snippets smoke stopped when Owner chose removal.
- `AUTOSWITCH_DECOUPLED`: AutoSwitch remains, independent of `SnippetsEnabled`, snippets UI, parser, expressions, trigger expansion, hotkeys and persistence. It uses standalone `AS_dict.txt`, literal replacement values and its own source buffer.
- Existing `snippets.txt` and `snippets.txt.bak` are **untouched inactive legacy data**. Runtime must not read, create, parse, execute, modify or delete them; a clean profile must not create snippets files.
- Exact modern Notepad surface `notepad.exe` + `RichEditD2DPT` remains an **explicit fail-closed safe no-op** for AutoSwitch; no positive modern Notepad support is authorized. Preserve source-context revalidation before all mutations and deferred callbacks.
- Existing AutoSwitch routes for Chrome and Microsoft Word must remain functional and be retested on the remediated candidate.
- Manual collapsed-caret `Insert` and Smart Caps use only verified exact classic Win32 `Edit` and Word `Range` direct adapters. No synthetic selection, clipboard rewrite, keyboard-selection or generic RichEdit/Qt/Chromium fallback.
- Previously verified selected-text conversion and OLE clipboard preservation, Smart Caps boundaries, existing security rules and runtime `2.9.0.1-dev` remain unchanged by this task.

## Rejected startup candidate — immutable evidence

`AGZ-MAH-0019: USER_SMOKE_FAILED / STARTUP_HANG_HIGH_CPU`:

- rejected source `5527f662cb844ba90d264f5b93cb27f8334bf295`;
- build run `30469006963`, x64 artifact ID `8730807263`;
- ZIP SHA-256 `4627c32bffe76ed3df71b8f35cfaa922783c31ee65770ea034207b78b2695214`;
- `Mahou.exe` SHA-256 `87d86a215f0ee57fa95567a2a74550b4e1190b495bb4d8127601fb24a1407abb`.

Physical Windows launch never reached a usable UI/tray and showed sustained high CPU. **Never request another user run of this rejected candidate**.

Root cause: the old dictionary loader repeatedly copied and rescanned a remaining suffix for each rule (quadratic-scale work); there was an additional count rescan, three character-sized Boolean allocations and configuration-load `TextChanged` work.

## AGZ-MAH-0020 remediation — accepted as automated, not physically verified

Source ancestry: first remediation commit `8e858ea4e89079c8701516fed58861b82aa7448a` has parent exactly `5527f662cb844ba90d264f5b93cb27f8334bf295`; follow-up source restore `27ade961aead0d293cf74902e1718a7cee67b01a`; final docs/CI evidence commit `ba40fd4c588b7c48b64bcc2cdfcefbed0f39c0d6`.

- New monotonic forward-only `AutoSwitchDictionaryParser`: bounded indexing, no repeated suffix copying, no separate count traversal, no character-sized arrays; preserves alias order, duplicates, first-match behavior and literal values; malformed input fails closed without publishing partial arrays.
- A single parse during configuration load, with the count derived from that result; programmatic `TextChanged` is suppressed. Disabled AutoSwitch clears active arrays without parsing or altering `AS_dict.txt`.
- Actual bundled dictionary regression locks **5,188,519 characters, 151,429 complete rules/aliases and 18 comment lines**, along with mappings, order, LF/CRLF, malformed input and literal values.
- A separate **150,000-rule synthetic dictionary** is also exercised under a 12-second bound.
- Exact-head GitHub Actions: Security `31135413924` SUCCESS; Chrome extension editing core `31135413926` SUCCESS; Input surface probe `31135413919` SUCCESS; Modern Windows build `31135413918` SUCCESS. Windows x86 job `92733540991` SUCCESS and x64 job `92733541034` SUCCESS; deterministic double builds, four MSBuilds with zero warnings/errors and candidate provenance passed.
- x86 timings: real 72 ms / synthetic 56 ms; x64: real 48 ms / synthetic 65 ms.
- Supervisor acceptance: **`IMPLEMENTED / EXACT-HEAD_CI_PASSED / USER_SMOKE_PENDING`**. Prior executor's implementation was accepted; its chat may be closed. This is **not** `VERIFIED`, does not authorize PR #18 merge/Ready, and does not establish real Windows behavior.

## Artifact availability blocker as of 2026-11-23

The original replacement x64 candidate metadata:

- `Mahou-2.9.0.1-dev-win-x64-ba40fd4-run31135413918`;
- artifact ID `8977741913`, workflow run `31135413918`;
- originally recorded ZIP SHA-256 `a3abb5973530741bb092100a14a4cb6a41ec3a0b1cc0f8e4d85ca1be6646b0b1`;
- original `Mahou.exe` SHA-256 `01834bd7a9f288db1035c90389e40bdd461ef0fd873bcdcd7ba1290fc607ff06`;
- original download link: https://github.com/MIXANIZM/Mahou/actions/runs/31135413918/artifacts/8977741913.

**Critical latest observation:** on **2026-11-23**, the GitHub run's available-artifact list was empty, and direct artifact download via GitHub returned **404 Not Found**. Hence **do not give that old link as a working download**. Its historical hashes remain provenance evidence, not the hashes of a new archive. A fresh exact-source candidate must have its **own** recorded run/artifact ID and ZIP hash.

## Immediate next bounded action (before manual Stage A)

`AGZ-MAH-0022` can be assigned as a narrowly scoped **artifact refresh / provenance-only** executor task if the Owner has not already supplied a verified candidate. Do not invent a completed task or an issue number.

Scope:
1. Verify PR #18 unchanged at `ba40fd4c588b7c48b64bcc2cdfcefbed0f39c0d6` / tree `a28af6cc694e6e857f6b0bb6844ed4113b55fc04` and rule pin unchanged.
2. Rebuild or otherwise regenerate an x64 test archive from **exactly that source** using authorized isolated CI/build infrastructure. Do not change runtime source, configurations, PR branches, workflow semantics or package contents just to bypass a check. If an exact-source run is not possible, stop and report a blocker.
3. Complete required deterministic x64/x86 source gates and provenance for the new archive as applicable. Record the new workflow run, artifact ID, tree, `Mahou.exe` SHA-256, ZIP SHA-256 and actual downloadable link. Do not relabel the August artifact as available.
4. Hand back to the supervisor. The supervisor verifies artifact identity/availability **before** sending it to the Owner. No signing, tag, publication, merge or PR transition.

## Physical Windows smoke — do not combine stages

**Stage A, only after a verified downloadable fresh candidate is available:** extract to a new folder; create/use a clean isolated profile without importing legacy configuration; start `Mahou.exe`; check usable UI/tray without sustained high CPU after initial load; exit normally and confirm process terminates. Owner must report `Stage A PASS` or precise failure symptoms. **Do not test functional AutoSwitch yet.**

**Stage B, only after Owner-confirmed Stage A PASS:** verify snippets UI absent; clean profile creates no `snippets.txt`; pre-existing legacy `snippets.txt` stays untouched/inactive; enable AutoSwitch independently; Chrome `ghbdtn + Space` converts to `привет`; Microsoft Word same; modern Notepad `ghbdtn + Space` remains unchanged; rapid focus/window switching causes no deferred mutation; restart preserves expected AutoSwitch functionality. Record all failures separately and stop on unsafe mutation.

After both stages pass, supervisor may recognize the physical evidence and assign a bounded documentation closeout. **Still no PR #18 Ready/merge** absent explicit permission and remaining gates.

## GitHub and release permission gates

Do not modify/close/rebase/retarget PR #1 or PR #3; do not change PR #2 metadata/Ready/merge; do not merge/mark Ready PR #18; do not change runtime version, add positive Notepad support or restore snippets; do not erase legacy snippet data. Never enable auto-merge, tag, sign, create Release, distribute/publicly publish or change production without Owner approval. Review `docs/RELEASE-READINESS.md` for separate PR #2 merge gates and public-release gates.

## Next-BOSS reporting contract

- Start by reporting the exact GitHub state, existing candidate status, current artifact availability and one next bounded action.
- Do not rely on earlier chat text in place of checking GitHub.
- For each worker, provide one bounded handoff, inspect its exact commits/tests/diff and conclude: **"Результат принят. Чат исполнителя можно закрывать."** or **"Чат пока не закрывать. Нужно исправить: ..."**.
- Keep this handoff and `ISSUES.md` / `PROJECT_STATE.md` / `UNIVERSAL-CHAT-HANDOFF.md` synchronized with later events.
- This document is a docs-only handoff PR; it neither claims a physical smoke PASS nor alters any protected implementation PR.
