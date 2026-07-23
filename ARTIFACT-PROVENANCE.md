# Artifact provenance and handoff

Mahou test artifacts are accepted only when their source commit, source tree, executable metadata, and file hashes agree. A green build alone is not sufficient evidence for handoff.

## Immutable artifact names

Every downloadable Windows build artifact uses this format:

```text
Mahou-<runtime-version>-win-<platform>-<short-sha>-run<run-id>.zip
```

Examples:

```text
Mahou-2.9.0.1-dev-win-x86-<short-sha>-run<run-id>.zip
Mahou-2.9.0.1-dev-win-x64-<short-sha>-run<run-id>.zip
```

Generic names such as `Mahou-x86.zip`, `Mahou-modern-x86.zip`, or `Mahou-x86-full.zip` must never be used for handoff.

GitHub Actions log and evidence artifacts use the same immutable base name with `-logs` or `-evidence` appended.

## Evidence inside the build archive

Each Windows build archive contains:

- `build-manifest.json` with repository, full commit, full tree, ref, workflow, run, attempt, platform, configuration, timestamp, deterministic-build status, security-regression status, and a `manifest.files` inventory of payload paths and SHA-256 values;
- `SHA256SUMS.txt` covering every issued file except `SHA256SUMS.txt` itself;
- `Mahou.exe` with the full source commit embedded by the build;
- `Mahou.exe.config` and the other controlled package files;
- deterministic-build, security, provenance, and test documentation.

The package is created only after two isolated Release builds for its platform match byte-for-byte and all source/regression checks pass.

## Post-upload evidence

`actions/upload-artifact` returns the immutable artifact ID, URL, and SHA-256 digest after upload. The workflow then creates `artifact-evidence-<platform>.json` containing:

- artifact name;
- artifact ID;
- artifact digest;
- workflow run URL;
- full source commit and tree;
- downloadable artifact ZIP SHA-256;
- `Mahou.exe` SHA-256.

This JSON is uploaded as a separate evidence artifact with the same immutable name prefix. Workflows have read-only repository permissions and never commit or push evidence back to Git.

## Mandatory handoff verification

Run the verifier against the complete expected source and build identity:

```powershell
pwsh -File .github/scripts/verify-artifact-provenance.ps1 `
  -ExpectedCommit <full-commit-sha> `
  -ExpectedTree <full-tree-sha> `
  -ExpectedPlatform <x86-or-x64> `
  -ExpectedRepository MIXANIZM/Mahou `
  -ExpectedRuntimeVersion 2.9.0.1-dev `
  -ZipPath <downloaded-artifact.zip>
```

The script extracts into a new temporary directory and rejects the archive if:

- commit or tree differs;
- platform, repository, or runtime version differs from the exact expected value;
- a required manifest field is absent;
- deterministic/security status is not true;
- any issued file is missing from `SHA256SUMS.txt`;
- any hash differs or an unsafe path is present;
- `manifest.files` is missing, duplicated, unsafe, inconsistent with `SHA256SUMS.txt`, or does not exactly cover the payload files;
- the manifest is nested or the archive contains sibling content outside its package root;
- executable file version differs from the manifest runtime version;
- the expected full commit is not embedded in `Mahou.exe`.

The archive must not be renamed or handed to a user when verification fails.

## Regression tests

The test wrapper runs:

1. a positive verification with the correct commit and tree;
2. a negative verification of the same ZIP with an intentionally wrong expected commit;
3. a negative verification with the correct commit but an intentionally wrong expected tree;
4. a negative verification with an intentionally wrong expected platform;
5. a negative verification after changing one byte in `Mahou.exe.config`;
6. a negative verification of a nested manifest with untracked sibling content;
7. a negative verification where `manifest.files` contains a false payload hash while the modified manifest itself remains correctly covered by `SHA256SUMS.txt`;
8. a negative verification of a legacy archive when `-LegacyZipPath` is supplied.

The provenance incident archive is:

- commit: `0f9b75c37413af986aa92170f44b2fd5b397d5a5`;
- workflow run: `29175456612`;
- artifact ID: `8254770086`;
- handed-off filename: `Mahou-x86-full.zip`.

It must always be rejected when the expected source is a newer development head. The CI regression uses `.github/tests/fixtures/legacy-build-manifest-v1.json`, which reproduces the real incident archive's schema-version-1 manifest fields and recorded hashes without committing the old ZIP or executable. Before candidate handoff, the actual retained incident ZIP must additionally be checked locally when it is available.

## Handoff after merge

Artifacts built from a pull request head prove that PR head only. After merge, a PR-head artifact must not be renamed, described, or handed off as an artifact of the merge head, even when the source diff appears equivalent.

Wait until `mixanizm-modern-v2.9.0.1` points at the exact merged commit, then start a new `workflow_dispatch` run on that branch. Confirm that the new run's `head_sha`, manifest commit/tree, embedded executable commit, artifact evidence, and expected handoff values all identify that exact merged commit before issuing any ZIP.

## Scope and limitations

- GitHub Actions artifacts are temporary test artifacts, not Releases.
- Every third-party action is pinned to a full commit SHA; adjacent comments record its human-readable release version.
- The Actions artifact digest is the SHA-256 of the downloadable artifact ZIP generated by `upload-artifact`.
- No tag, Release, merge, or public distribution is implied by a successful provenance check.
- Provenance proves source identity and package integrity; it does not replace Windows runtime testing or Authenticode signing.
