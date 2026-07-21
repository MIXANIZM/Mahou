# Agent instructions

Before changing this repository, read:

1. `AGATZUB-RULES.md` and the pinned central rules commit;
2. `UNIVERSAL-CHAT-HANDOFF.md`;
3. `ISSUES.md`;
4. `README-MIXANIZM.md`;
5. `ARCHITECTURE.md`, `SECURITY.md`, and `TESTING.md` for affected subsystems.

## Working rules

- GitHub is the source of truth.
- Substantial work stays on a task branch or the existing Draft PR branch; never push substantial changes directly to `master`.
- Do not merge, enable auto-merge, tag, publish a release, or alter production without explicit user permission.
- Use small, reviewable commits and update applicable documentation with behavior changes.
- Never restore synthetic text selection, keyboard-selection fallback, or clipboard mutation to the collapsed-caret `Insert` path.
- Unsupported text controls must fail closed without changing text, selection, caret, layout, or clipboard.
- Runtime claims require an exact commit, automated checks, and the applicable real Windows smoke test.
