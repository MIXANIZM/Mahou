# Agent instructions

Before changing this repository, read:

1. `AGATZUB-RULES.md` and the exact pinned central rules content;
2. `PROJECT_STATE.md`;
3. `UNIVERSAL-CHAT-HANDOFF.md`;
4. `ISSUES.md`;
5. `README-MIXANIZM.md`;
6. `ARCHITECTURE.md`, `SECURITY.md`, and `TESTING.md` for affected subsystems.

## Working rules

- GitHub is the source of truth.
- The current development line is `mixanizm-modern-v2.9.0.1`, represented by Draft PR #2 against the legacy `master` branch.
- Substantial work uses a separate bounded task branch or an explicitly assigned existing task branch. Do not push substantial changes directly to `master` or casually extend the Draft PR #2 branch.
- Do not merge, enable auto-merge, tag, publish a release, close or rebase existing PRs, or alter production without explicit user permission.
- Use small, reviewable commits and update applicable documentation with behavior changes.
- Never restore synthetic text selection, keyboard-selection fallback, or clipboard mutation to the collapsed-caret `Insert` path.
- Unsupported text controls must fail closed without changing text, selection, caret, layout, or clipboard.
- Runtime claims require an exact commit, automated checks, and the applicable real Windows smoke test.

## Chat roles

Project coordination follows `standards/CHAT-ROLES-AND-WORKFLOW.md` from Agatzub Development Ruleset `v2.7.0` at the exact commit pinned in `AGATZUB-RULES.md`.

- One active project supervisor chat keeps the overall state, chooses the next bounded task, prepares executor handoffs, and accepts or rejects results. It does not perform long implementation or debugging cycles by default.
- One temporary executor chat handles one bounded task, reports the result to the supervisor, and does not select or start the next task.
- PR #1 and PR #3 remain separate open Draft PRs and must not be changed, closed, or rebased without a dedicated decision.
