# Universal chat handoff — MIXANIZM Mahou

## Project

- Repository: `MIXANIZM/Mahou`
- Development branch: `mixanizm-modern-v2.9.0.1`
- Draft PR: `#2`
- Last user-verified checkpoint: `d4b37a3dac8b4b35d682c82a9ced7b87eaf9adda`
- Last Smart Caps candidate tested by the user: `e2c19110e56e6ed7ab30f4b8cb503431e32b7537` — not verified
- Runtime line: `2.9.0.1-dev`
- Ruleset: `v2.6.3`, content commit `2f312c4adbb54ffd533d2877d49cfd39633460ab`

## Goal

Modernize and harden Mahou while preserving useful layout-switching behavior and making every automatic text mutation fail closed.

## Verified current behavior

- Collapsed-caret `Insert` never creates synthetic blue selection.
- Modern Windows Notepad/RichEdit, Chrome, Telegram, Discord, unknown controls, protected fields, and failed probes are no-op without a user-created selection.
- Modern Notepad was manually verified not to add, remove, highlight, or corrupt text.
- Microsoft Word and the exact classic Win32 `Edit` class remain the only supported direct collapsed-caret adapters.
- Draft PR remains open and unmerged.

## Active task

`AGZ-MAH-0004`: add a dedicated artifact provenance gate after a retained build from `0f9b75c37413af986aa92170f44b2fd5b397d5a5` was mistakenly handed to the user as `Mahou-x86-full.zip`.

Current task branch: `agz-mah-0004-artifact-provenance`, based exactly on development head `c636c10b55c3d3141487a9c00943cfb09082cdbc` and tree `c30a7457847944eae0ac2fda950ad36bbfc4ba9c`.

Scope is limited to CI/package scripts, provenance tests, and artifact handoff documentation. It must not change Insert, Smart Caps, classic Edit, Word, Notepad, browser/messenger, selection, or layout behavior. PR #3 is damaged transport history and must not be used or cleaned up without separate permission.

The new gate requires immutable runtime/platform/commit/run artifact names, full commit/tree manifests, complete package hashes, embedded executable commit/version verification, post-upload ID/digest evidence, and positive/negative/retained-legacy-ZIP tests. See `ARTIFACT-PROVENANCE.md`.

## Deferred product task

`AGZ-MAH-0001`: revised optional Smart Caps still requires the focused Windows smoke test described below.

User findings on `e2c1911...`:

- the new tab stayed in English while the rest of Mahou was Russian;
- `ПРивет → Привет` continued after Mahou was fully closed, proving that Microsoft Word's own autocorrect masked the test;
- Word did not correct a third initial capital or capitals in the middle of a word.

Revised required behavior:

- default off and configurable through the common Russian/English localization dictionaries;
- local-only, no network;
- correct accidental uppercase letters anywhere after the first letter of each word segment, including a third initial capital and capitals in the middle;
- preserve all-caps words; intentional mixed-case names are reversible and learnable as personal exceptions;
- show a session counter that changes only when Mahou itself performs or reverses a correction;
- use only verified direct classic `Edit` and Word range replacement;
- no selection, keyboard rewrite, Backspace injection, or clipboard mutation;
- skip mixed-script, numeric, URL/email/code-like, excluded, protected, stale, and unsupported contexts;
- immediate physical Backspace restores original casing;
- two explicit Backspace reversions add the normalized word to local exceptions.

## Important prohibitions

- Do not restore UI Automation `.Select()`, `Shift+Left`, `Ctrl+Shift+Left`, generated-selection collapse/reselect, stale round-trip, or tracked-buffer `ConvertLast` fallbacks.
- Do not merge PR #2, tag, release, or publish without explicit permission.
- Do not describe revised Smart Caps as verified until GitHub x86/x64 CI passes and the user completes the focused Windows smoke test.

## Exact next step

Finish the isolated AGZ-MAH-0004 provenance implementation, run local security and deterministic x86/x64 checks, open a Draft PR against `mixanizm-modern-v2.9.0.1`, and inspect its GitHub Actions evidence. Do not merge, tag, release, or hand off a ZIP until the exact commit/tree verifier passes, including rejection of the retained `0f9b75c...` incident archive.
