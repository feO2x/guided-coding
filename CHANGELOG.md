# Changelog

All notable changes to Guided Coding are documented here.

## [2.0.0] - Unreleased

- Add UTC timestamps to plan and Plan Deviations filenames.
- Treat `ai-plans/` as an append-only, event-sourced decision record.
- Freeze plans when their Planning Phase ends, except for checking acceptance criteria.
- Require Plan Deviations documents for material departures from frozen plans.
- Distribute Guided Coding as portable Agent Skills and an Agent Plugin.
- Generate a Claude Code adapter with concise, manually invoked skill names.
- Add learning workflows for implementing plans through worked examples or coached problem solving.
