# Changelog

All notable changes to Guided Coding are documented here.

## [Unreleased]

- Let the implement skills check a plan's premises against the code before implementing it, and bound workarounds for wrong plan decisions by a closed list of conditions: evidence shows the error, the Rationale and all other decisions stay unchanged, no public API, scope, or dependency changes beyond the plan, and no weakened or removed tests. Otherwise, `guided-coding-implement` stops the dependent work and reports the problem, and the Guided Learning skills ask the user. Acceptance Criteria met only through a workaround stay unticked for the reviewer.
- Keep Plan Deviations documents editable until they are merged into the target branch instead of freezing them once committed, so that Follow-Up Plans from PR/MR reviews can still be recorded. `guided-coding-write-deviations` now rewrites the work item's existing document under a fresh timestamp, keeping one document per work item that sorts after all of its plans. Run `guided-coding-setup` to bring `ai-plans/AGENTS.md` up to date.
- Add `guided-coding-plan-and-review`, which writes a plan and refines it through independent reviewers in bounded rounds. Reviewers run `guided-coding-review-plan` in fresh contexts; the planner addresses every finding but never changes a decision the user made explicitly, and reports every finding with its resolution.
- Let models invoke `guided-coding-write-plan` and `guided-coding-review-plan`, so other Guided Coding skills can use them. Their descriptions limit them to explicit requests by the user or by another Guided Coding skill.
- Add RDAT (Rationale, Decisions, Acceptance Criteria, Technical Details), an opt-in plan structure next to the default one: numbered Decisions summarize what a reviewer approves, and each section adds information instead of repeating earlier ones.
- Let `guided-coding-write-plan` take a plan structure when invoked, either a path to a Markdown file or the name of a bundled structure such as `rdat-plan-structure.md`. `ai-plans/AGENTS.md` can name one the same way.

## [2.0.0] - 2026-09-27

- Add UTC timestamps to plan and Plan Deviations filenames.
- Treat `ai-plans/` as an append-only, event-sourced decision record.
- Freeze plans when their Planning Phase ends, except for checking acceptance criteria.
- Require Plan Deviations documents for material departures from frozen plans.
- Distribute Guided Coding as portable Agent Skills and an Agent Plugin.
- Generate a Claude Code adapter with concise, manually invoked skill names.
- Add Guided Learning skills for Beginners and Advanced devs, track learning progress across sessions in a private, git-backed Guided Learning profile located at `$HOME/.guided-learning/profile.md`.
