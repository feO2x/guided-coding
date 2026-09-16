---
name: guided-coding-write-plan
description: Write a Guided Coding plan or follow-up plan after its approach has been discussed. Run only when explicitly requested by the user.
license: MIT
---

# Write a Plan

Your goal is to write an initial plan or follow-up plan into the `ai-plans/` subfolder. Which one to write should already be clear from the existing conversation. Read `ai-plans/AGENTS.md` if you haven't already.

After writing, report the plan path and stop.

## Plan File Name

The initial file name of the new plan file is `<topic>.md`. `<topic>` is a shortened kebab-case version of the issue title. At this point in the conversation, you should be able to derive a title from the discussion. If not, ask the user to provide one and shorten it.

## Plan Structure

Start with `# Title`, followed by exactly these `##` sections in order:

1. `## Rationale`: briefly state the problem and overarching goal this issue is solving.
2. `## Acceptance Criteria`: list **observable, verifiable outcomes** as unticked Markdown tasks (`- [ ]`). Describe results, not implementation steps.
3. `## Technical Details`: Provide relevant infos for the implementer, e.g., existing APIs to extend, Design Patterns to follow, performance considerations, async and multi-threading aspects, and links to relevant docs. Avoid step-by-step instructions, exhaustive listings - assume the implementer is a senior software developer.

Use minimal code examples only when they clarify an important contract, such as an API signature, interface, or DTO shape. Say whether an example is exact or illustrative. Avoid method bodies, step-by-step instructions, exhaustive file lists, and routine background.

When behavior changes, require appropriate automated test coverage in the acceptance criteria.
Require benchmarks only when performance is a material risk or requirement.

The root AGENTS.md and the referenced docs should define feedback loops (such as compilers, static code analyzers, automated tests, test coverage, mutation testing, automated benchmarks, etc.) available in the repository. Use them to make acceptance criteria verifiable. If there are no feedback loops listed, stop and report this to the user.

If the plan requires a missing feedback loop, make adding it explicit in the plan.

## Follow-up Plans

Use the same format and file naming convention. Read every earlier document for the issue. In the Rationale, name the plans this one follows by exact filename.

Record only the changed decisions and outcomes. State exactly which earlier decisions this plan supersedes and why. Do not repeat unchanged decisions or contradict earlier plans silently.

## Most Importantly

Try to keep each plan as concise as possible. Simpler language are preferred over elaborate prose.
