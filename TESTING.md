# Testing

## Fast gates

```text
python .github/scripts/security-regression.py
python .github/scripts/ui-resource-regression.py
```

These gates check source-level safety invariants, common Russian/English Smart Caps localization markers, and UI resource consistency. They do not replace runtime testing.

## Windows CI

The `Modern Windows build` workflow:

- builds Release x86 twice and compares controlled files byte-for-byte;
- builds Release x64 twice and compares controlled files byte-for-byte;
- runs `InsertSafetyRegression` against the built executable;
- runs `SmartCapsRegression`, including third-initial, interior-capital, hyphenated-word, all-caps, mixed-script, numeric, URL and email cases;
- packages manifests, SHA-256 sums, security report, and test documentation.

## Manual Windows checks

Follow `TEST-PLAN-WINDOWS11.md`. For text mutation, use disposable documents and verify text, caret, selection, keyboard layout, clipboard and the Mahou-only session counter before and after each operation.

A feature is `VERIFIED` only after its exact commit passes applicable CI and the user confirms its real Windows behavior.
