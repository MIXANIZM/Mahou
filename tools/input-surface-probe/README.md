# Mahou Input Surface Probe

A standalone, read-only Windows x64 diagnostic tool for `AGZ-MAH-0009`.

The probe waits for a short countdown, captures one snapshot of the foreground process and focused control, writes a redacted JSON report, and exits. It is not loaded by Mahou and does not change Mahou runtime behavior or version.

## Privacy and safety boundary

The executable does not collect actual typed text, window titles, UIA Name content, document/chat/contact names, browser URLs, clipboard data, or passwords. It does not install hooks, monitor continuously, simulate keyboard input, inject into another process, create selection, or call writable accessibility/text methods.

An interface or pattern reported as present is metadata only. The report always keeps write capability unverified.

## Run

1. Extract the immutable x64 probe ZIP.
2. Start `Mahou.InputSurfaceProbe.exe` as the ordinary user. Administrator rights are neither requested nor required.
3. During the countdown, focus the exact target text field and leave the caret/selection unchanged.
4. After capture, find `input-surface-probe-YYYYMMDD-HHMMSS.json` beside the executable.

Optional command line:

```text
Mahou.InputSurfaceProbe.exe --countdown 5 --output chrome-input.json
Mahou.InputSurfaceProbe.exe --version-json
```

See `docs/INPUT-SURFACE-PROBE.md` in the repository/package for the capture set and interpretation rules.
