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

`AGZ-MAH-0001`: revise optional Smart Caps after the first Windows smoke test.

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

Commit the localization and broader accidental-capital correction, run security and deterministic x86/x64 CI with the updated `SmartCapsRegression`, then provide a new test artifact. The smoke test must include a pattern Word does not autocorrect itself, such as `окоРОчка `, and confirm that the Mahou-only session counter increments.
