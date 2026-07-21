# Changelog

## [Unreleased]

### Added

- Opt-in local Smart Caps settings and personal exceptions for correcting two accidental initial capitals through verified direct text adapters. Runtime verification is still pending.

### Security

- Smart Caps is prohibited from using synthetic selection, keyboard rewriting, clipboard mutation, or unsupported text controls.

### Fixed

- Collapsed-caret `Insert` no longer uses generated blue selection.
- Modern Windows Notepad/RichEdit is contained as a safe no-op until a dedicated direct adapter exists.
