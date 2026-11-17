# Changelog

All notable changes to Guided Coding are documented here.

## [Unreleased]

- Add RDAT (Rationale, Decisions, Acceptance Criteria, Technical Details), an opt-in plan structure next to the default one: numbered Decisions summarize what a reviewer approves, and each section adds information instead of repeating earlier ones.

## [2.0.0] - 2026-09-27

- Add UTC timestamps to plan and Plan Deviations filenames.
- Treat `ai-plans/` as an append-only, event-sourced decision record.
- Freeze plans when their Planning Phase ends, except for checking acceptance criteria.
- Require Plan Deviations documents for material departures from frozen plans.
- Distribute Guided Coding as portable Agent Skills and an Agent Plugin.
- Generate a Claude Code adapter with concise, manually invoked skill names.
- Add Guided Learning skills for Beginners and Advanced devs, track learning progress across sessions in a private, git-backed Guided Learning profile located at `$HOME/.guided-learning/profile.md`.
