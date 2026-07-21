# Universal chat handoff — MIXANIZM Mahou

## Project

- Repository: `MIXANIZM/Mahou`
- Development branch: `mixanizm-modern-v2.9.0.1`
- Draft PR: `#2`
- Last user-verified checkpoint: `d4b37a3dac8b4b35d682c82a9ced7b87eaf9adda`
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

`AGZ-MAH-0001`: optional Smart Caps is implemented locally and awaits GitHub CI plus Windows runtime verification.

Required behavior:

- default off and configurable in the UI;
- local-only, no network;
- runs after a freshly typed word is completed;
- uses only verified direct classic `Edit` and Word range replacement;
- no selection, keyboard rewrite, Backspace injection, or clipboard mutation;
- skips all-caps, mixed-script, mixed-case, numeric, URL/email/code-like, excluded, protected, stale, and unsupported contexts;
- immediate physical Backspace restores original casing;
- two explicit Backspace reversions add the normalized word to local exceptions.

## Important prohibitions

- Do not restore UI Automation `.Select()`, `Shift+Left`, `Ctrl+Shift+Left`, generated-selection collapse/reselect, stale round-trip, or tracked-buffer `ConvertLast` fallbacks.
- Do not merge PR #2, tag, release, or publish without explicit permission.
- Do not describe Smart Caps as verified until GitHub x86/x64 CI passes and the user completes the Windows smoke test.

## Exact next step

Finish Smart Caps implementation and regression coverage, perform a semantic diff review, commit and push to the Draft PR branch, run security and deterministic x86/x64 CI, then provide a test artifact and focused smoke checklist.
