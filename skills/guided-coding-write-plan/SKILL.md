---
name: guided-coding-write-plan
description: Write a Guided Coding plan or follow-up plan after its approach has been discussed. Run only when explicitly requested by the user or by another Guided Coding skill.
license: MIT
---

# Write a Plan

Your goal is to write an Initial Plan or Follow-Up Plan into the `ai-plans/` folder at the repository root. After writing, report the plan path and stop. When another Guided Coding skill invoked this one, return to it instead of stopping.

## Plan File Name

Name the draft `<short-title>.md`, using a shortened kebab-case version of the ticket title. If no ticket title is available, derive one from the discussion. Ask the user for a title only if you cannot derive one.

## Plan Structure

Use the first plan structure that applies:

1. A plan structure the user names when invoking this skill: a path to a Markdown file, or the name of a file in `assets/` relative to this skill file, such as `rdat-plan-structure.md`.
2. A plan structure that `ai-plans/AGENTS.md` names in the same way or describes itself.
3. `assets/default-plan-structure.md` relative to this skill file.

A bare filename refers to a file in `assets/` relative to this skill file. Resolve other relative paths from the repository root. If the selected file does not exist, stop and report it. Do not fall back to another structure.

## What Happens After Writing a Plan?

The user reviews the draft and may request changes. Once you and the user agree it is ready for implementation, it can be frozen. The only permitted edits after freezing are checking off acceptance criteria.

Freezing renames the file to `<timestamp>-<ticket-id>-<short-title>.md`, or `<timestamp>-<short-title>.md` when there is no ticket ID. It also adds a `*Frozen at ...*` line directly below the title. Do not freeze the plan while running this skill.

## Follow-up Plans

A Follow-Up Plan supersedes decisions of a Frozen Plan. The decision to write one is made in the Guiding Phase, when the review of the implementation calls for larger changes.

Use the plan structure selected above and the same file naming convention. Read every earlier document for the ticket. In the opening section of the plan (`## Rationale` in the default structure), name the plans this one follows by exact filename.

Record only the changed decisions and outcomes. State exactly which earlier decisions this plan supersedes and why. Do not repeat unchanged decisions or contradict earlier plans silently.

## Most Importantly

Keep plans concise. Use plain language.
