# Changelog

All notable changes to Guided Coding are documented here.

## [Unreleased]

- **Breaking:** Plans have a fixed, decisions-first structure: `## Goal`, `## Decisions` (numbered), `## Out of Scope`, `## Acceptance Criteria`, and an optional `## Implementation Context`. Repositories can no longer define their own plan structure in `ai-plans/AGENTS.md`; they can add content rules, which add to the Guided Coding rules and never override them.
- `guided-coding-write-plan` writes decisions instead of options, asks about unsettled decisions that are expensive to reverse or have no clear best option, and accepts deliberate delegation only as `Delegated: X, within Y, verified by Z`. Every decision that changes behavior gets a criterion that fails without it. Plans are written for an implementer without the planning context, which may be a small model, and claims about current behavior are verified first.
- Add a plan checklist of yes/no items, shipped with `guided-coding-write-plan`, `guided-coding-review-plan`, and `guided-coding-freeze-plan`. `guided-coding-write-plan` fixes every failing item before reporting.
- **Breaking:** `guided-coding-freeze-plan` refuses to freeze a plan while a checklist item fails and lists the failing items.
- `guided-coding-review-plan` reports the checklist results, open decisions, decisions hidden in Implementation Context, decisions without a criterion that fails without them, and lists of affected things without a `Members:` list or with a `Rule:` that does not match them. It verifies claims about current behavior, builds and runs code and throwaway probes outside the repository where reading is not enough, and separates false claims from claims it could not verify.
- Follow-up plans and Plan Deviations documents cite decisions as `<filename> Decision N`.
- `guided-coding-setup` removes a custom plan structure from `ai-plans/AGENTS.md` after showing the result and asking, keeps the repository's content rules under `## Plan Content Rules`, and reports content rules that conflict with the Guided Coding rules.

### Migration from 2.x

- Finish work on plans in the 2.x format before upgrading, or plan that work again in the new structure. The skills do not support 2.x plans, and existing plans are never rewritten.
- Run `guided-coding-setup` to update `ai-plans/AGENTS.md`. If it describes its own plan structure, setup converts it into content rules and asks before writing.

## [2.0.0] - 2026-09-27

- Add UTC timestamps to plan and Plan Deviations filenames.
- Treat `ai-plans/` as an append-only, event-sourced decision record.
- Freeze plans when their Planning Phase ends, except for checking acceptance criteria.
- Require Plan Deviations documents for material departures from frozen plans.
- Distribute Guided Coding as portable Agent Skills and an Agent Plugin.
- Generate a Claude Code adapter with concise, manually invoked skill names.
- Add Guided Learning skills for Beginners and Advanced devs, track learning progress across sessions in a private, git-backed Guided Learning profile located at `$HOME/.guided-learning/profile.md`.
