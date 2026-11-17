---
name: guided-coding-write-plan
description: Write a Guided Coding plan or follow-up plan after its approach has been discussed. Run only when explicitly requested by the user.
license: MIT
---

# Write a Plan

Your goal is to write an Initial Plan or Follow-Up Plan into the `ai-plans/` folder at the repository root. After writing, report the plan path and stop.

## Plan File Name

The initial file name of the new plan file is `<short-title>.md`. `<short-title>` is a shortened kebab-case version of the ticket title. At this point in the conversation, you should be able to derive a full title from the discussion that you can shorten. If not, ask the user to provide one and shorten it.

## Plan Structure

Use the first plan structure that applies:

1. A plan structure the user names when invoking this skill: a path to a Markdown file, or the name of a file in `assets/` relative to this skill file, such as `rdat-plan-structure.md`.
2. A plan structure that `ai-plans/AGENTS.md` names in the same way or describes itself.
3. `assets/default-plan-structure.md` relative to this skill file.

A bare file name refers to a file in `assets/` relative to this skill file. Resolve other relative paths from the repository root. If the selected file does not exist, stop and report it; do not fall back.

## What Happens After Writing a Plan?

The user reviews the plan. They will provide feedback, aspects of the plan might change. Once both you and the user agree on the plan being ready for implementation, it is frozen and becomes largely immutable (except for checking off Acceptance Criteria). When a plan is frozen, its file name is changed to `<timestamp>-<ticket-id>-<short-title>.md`, or `<timestamp>-<short-title>.md` (if no `<ticket-id>` is present). Also, a `*Frozen at ...*` line is inserted right below the `# Title` of the plan. All this doesn't happen in this step of the conversation.

## Follow-up Plans

A Follow-Up Plan supersedes decisions of a Frozen Plan. The decision to write one is made in the Guiding Phase, when the review of the implementation calls for larger changes.

Use the plan structure selected above and the same file naming convention. Read every earlier document for the ticket. In the opening section of the plan (`## Rationale` in the default structure), name the plans this one follows by exact filename.

Record only the changed decisions and outcomes. State exactly which earlier decisions this plan supersedes and why. Do not repeat unchanged decisions or contradict earlier plans silently.

## Most Importantly

Try to keep each plan as concise as possible. Simpler language is preferred over elaborate prose.
