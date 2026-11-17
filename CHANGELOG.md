# Changelog

All notable changes to Guided Coding are documented here.

## [Unreleased]

- Make the implementation skills verify a plan's assumptions against the code before implementing it.
- Allow workarounds for incorrect plan decisions or factual claims, or acceptance criteria that cannot be met as written. All of these conditions must be met:

  - Evidence confirms the error.
  - The Rationale and all other decisions remain unchanged.
  - No public API, scope, or dependency changes go beyond the plan.
  - No tests are weakened or removed.

- When a workaround is not allowed, `guided-coding-implement` stops the affected work and reports the problem. The Guided Learning skills ask the user how to proceed. Acceptance criteria met only through a workaround remain unchecked for the reviewer.
- Keep Plan Deviations documents editable until they are merged into the target branch, instead of freezing them once committed, so Follow-Up Plans from PR/MR reviews can still be recorded. When updating a document, `guided-coding-write-deviations` renames it with a new timestamp and rewrites its contents. Each work item keeps one document whose filename sorts after all its plans. Run `guided-coding-setup` to update `ai-plans/AGENTS.md`.
- Add `guided-coding-plan-and-review`, which writes a plan and improves it through independent reviews, up to a configured round limit. Reviewers run `guided-coding-review-plan` in fresh contexts. The planner addresses every finding, preserves the user's explicit decisions unless the user approves a change, and reports each finding with its resolution.
- Let models invoke `guided-coding-write-plan` and `guided-coding-review-plan`, so other Guided Coding skills can use them. Their descriptions limit them to explicit requests by the user or by another Guided Coding skill.
- Add RDAT (Rationale, Decisions, Acceptance Criteria, Technical Details) as an optional alternative to the default plan structure. Numbered Decisions summarize what a reviewer approves. Each section adds information instead of repeating earlier ones.
- Let `guided-coding-write-plan` take a plan structure when invoked, either a path to a Markdown file or the name of a bundled structure such as `rdat-plan-structure.md`. `ai-plans/AGENTS.md` can name one the same way.
- Rewrite the skill instructions and plan templates in plain English. Split dense rules into shorter instructions and use consistent terms without changing the workflows.

## [2.0.0] - 2026-09-27

- Add UTC timestamps to plan and Plan Deviations filenames.
- Use `ai-plans/` to keep a history of planning decisions that preserves frozen plans and records later decisions in new plans.
- Freeze plans when their Planning Phase ends, except for checking off acceptance criteria.
- Require Plan Deviations documents for material departures from frozen plans.
- Distribute Guided Coding as portable Agent Skills and an Agent Plugin.
- Generate a Claude Code adapter with concise, manually invoked skill names.
- Add Guided Learning skills for beginners and more experienced developers. Track learning progress across sessions in a private Git repository, with the profile at `$HOME/.guided-learning/profile.md`.
