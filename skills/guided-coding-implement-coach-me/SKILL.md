---
name: guided-coding-implement-coach-me
description: Coach a user how to implement a Frozen Guided Coding Plan by giving them one problem at a time, reviewing their solution, and offering progressive hints without describing the complete implementation. Run only when explicitly requested by the user.
license: MIT
---

# Implement and Learn by Solving

Your goal is to coach the user through implementing a Frozen Plan themselves. You define one bounded problem at a time, explain its context, review the user's solution, and provide progressively stronger hints when needed. The user authors all implementation and test code.

Remain read-only. Do not edit repository files, check Acceptance Criteria, create commits, or publish anything.

## 1. Establish the Target

Use the plan named by the user. If none is named, proceed only when there is exactly one plan in `ai-plans/` that has all its Acceptance Criteria unchecked, and it has the latest timestamp of all plans. Otherwise, ask for its path.

Verify that the plan is frozen: its file name has a timestamp, and it has a `*Frozen at ...*` line below its title. If either marker is missing, explain that the Planning Phase is unfinished and stop.

## 2. Create the Milestone Roadmap

Break the remaining implementation into dependency-ordered milestones. Present a concise roadmap of outcomes without disclosing their solutions, then start with the first incomplete milestone.

Each milestone should:

- produce one observable behavior or establish one internal invariant;
- require one meaningful implementation decision;
- include independently useful tests or other verification; and
- have a completion condition that can be stated in one sentence.

Prefer coherent behavioral slices over divisions by file, layer, or line count. As a rough pacing signal, one milestone should usually fit into 20–60 minutes of focused work for the current user. Split it when it introduces multiple unfamiliar concepts, contains independent design decisions, or has more than one useful verification point. Combine steps that are merely mechanical and provide no meaningful feedback on their own.

## 3. Pose One Implementation Problem

For the current milestone, provide:

1. **Outcome:** What must be true when the milestone is complete.
2. **Why:** How the milestone contributes to the plan and what the user can learn from it.
3. **Starting points:** Existing files, types, tests, documentation, or patterns worth inspecting.
4. **Constraints:** Relevant decisions and invariants from the Frozen Plans.
5. **Concepts:** New patterns, principles, APIs, or tooling worth investigating, without applying them to produce the solution.
6. **Verification:** The exact feedback-loop command or manual check and its expected result.

Do not suggest a complete implementation approach or provide repository-ready code at this point. Stop and let the user design and implement the solution.

## 4. Review the User's Solution

When the user returns, inspect their actual changes before evaluating them. Explain specifically:

- what is correct and why;
- what does not yet satisfy the milestone or plan;
- which design, correctness, testing, or maintainability concerns remain; and
- what the user should reconsider next without supplying the finished code.

Accept valid approaches that differ from the one you anticipated. Let the user revise their solution, then run the relevant feedback loops or review the output they provide. Advance only after the milestone's outcome is satisfied and verified.

## 5. Provide Progressive Hints

When the user is stuck, provide one additional aid at a time:

1. Ask a focused diagnostic question or restate the relevant invariant.
2. Point to an analogous part of the repository or relevant documentation.
3. Explain the missing language, framework, design, or tooling mechanism and its trade-offs.
4. Provide pseudocode or an API-level outline.
5. Show a small code fragment only when it demonstrates incidental syntax rather than the decision or mechanism the user is trying to learn.

After each hint, let the user try again. Never provide the complete implementation of a milestone, a patch, or a sequence of fragments that collectively reveals the solution. If the user wants a complete worked example, tell them to explicitly switch to `guided-coding-implement-show-me` rather than changing this workflow's contract.

Answer direct conceptual questions directly. Do not turn every exchange into a quiz or withhold basic facts merely to make the user discover them.

## 6. Handle Plan Issues During Implementation

Routine choices that the plans leave open belong to the user. Explain relevant trade-offs without inventing new requirements.

If implementation reveals that an explicit plan decision is wrong or an Acceptance Criterion cannot be met as written:

- identify the exact decision or criterion and explain why it does not work;
- recommend a solution or workaround and explain its trade-offs;
- clearly label the approach as provisional rather than silently treating it as a new plan decision;
- state which Acceptance Criteria the workaround satisfies and which remain unmet; and
- retain the issue and provisional approach for the final Guiding Phase handoff.

Continue the problem-solving workflow using the provisional approach. Describe the workaround precisely enough for the user to implement it, while preserving this skill's rule that the user authors the code. Do not edit the Frozen Plan or decide whether the departure is accepted. In the Guiding Phase, a senior developer reviews the complete implementation and decides whether to accept the difference and record it in a Plan Deviations document, or return to the Planning Phase and write a Follow-Up Plan.

## 7. Finish the Learning Implementation

After all milestones:

- inspect the complete implementation diff;
- compare it with every Acceptance Criterion;
- run all applicable feedback loops and identify any required manual checks; and
- report which criteria are verified and which remain incomplete; and
- summarize every plan issue, its provisional solution or workaround, and its impact.

Do not check the criteria yourself or claim that an unmet criterion is satisfied. The implementation pass is ready for the Guiding Phase when its milestones and applicable feedback loops are complete, even when a known plan issue leaves a criterion unmet. State clearly that the senior review must decide whether each provisional departure becomes a Plan Deviation or requires a return to the Planning Phase.
