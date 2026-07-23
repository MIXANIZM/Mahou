# Testing

## Fast gates

```text
python .github/scripts/security-regression.py
python .github/scripts/ui-resource-regression.py
python .github/scripts/artifact-provenance-regression.py
```

These gates check source-level safety invariants, common Russian/English Smart Caps localization markers, and UI resource consistency. They do not replace runtime testing.

## Windows CI

The `Modern Windows build` workflow:

- builds Release x86 twice and compares controlled files byte-for-byte;
- builds Release x64 twice and compares controlled files byte-for-byte;
- runs `InsertSafetyRegression` against the built executable;
- runs `SmartCapsRegression`, including third-initial, interior-capital, hyphenated-word, all-caps, mixed-script, numeric, URL and email cases;
- packages manifests, SHA-256 sums, security report, and test documentation.
- names every build/log/evidence artifact with the runtime version, platform, short source commit, and workflow run ID;
- verifies the generated archive with the exact full commit and tree, rejects an intentionally wrong expected commit, and rejects a legacy-manifest fixture before upload;
- creates a separate post-upload evidence JSON containing artifact ID/digest, run URL, source commit/tree, ZIP SHA-256, and executable SHA-256.

## Artifact handoff

Follow `ARTIFACT-PROVENANCE.md`. Before giving a ZIP to a user, run `.github/scripts/verify-artifact-provenance.ps1` with the expected full commit and tree. When the retained incident archive is available, `.github/scripts/test-artifact-provenance.ps1 -LegacyZipPath <path>` must reject the real `0f9b75c...` ZIP for the current expected source.

## Manual Windows checks

Follow `TEST-PLAN-WINDOWS11.md`. For text mutation, use disposable documents and verify text, caret, selection, keyboard layout, clipboard and the Mahou-only session counter before and after each operation.

A feature is `VERIFIED` only after its exact commit passes applicable CI and the user confirms its real Windows behavior.
