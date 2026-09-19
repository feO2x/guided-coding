---
name: guided-coding-implement-and-learn-by-example
description: Guide a user through implementing a Frozen Guided Coding Plan by presenting and explaining complete code for one milestone at a time for the user to enter and examine. Run only when explicitly requested by the user.
license: MIT
---

# Implement and Learn by Example

Your goal is to teach the user how to implement a Frozen Plan through repository-specific worked examples. You provide the complete code for one milestone at a time and explain it; the user enters the code, runs it, and asks questions.

Remain read-only. Do not edit repository files, check Acceptance Criteria, create commits, or publish anything.

## 1. Establish the Target

Use the plan named by the user. If none is named, proceed only when exactly one Frozen Plan with incomplete Acceptance Criteria can be identified in `ai-plans/`; otherwise ask for its path.

Verify that the plan is frozen: its file name carries a timestamp and it has a `*Frozen at ...*` line below its title. If either marker is missing, explain that the Planning Phase is unfinished and stop.

Read:

- applicable repository instructions and documented feedback loops;
- the target plan and every earlier plan for the same ticket that it refers to or supersedes;
- the relevant implementation and tests; and
- the current git status and diff, including the user's existing changes.

Treat later plans as superseding only the decisions they explicitly replace. Preserve all existing changes.

## 2. Create the Milestone Roadmap

Break the remaining implementation into dependency-ordered milestones. Present a concise roadmap of outcomes without revealing all the code up front, then start with the first incomplete milestone.

Each milestone should:

- produce one observable behavior or establish one internal invariant;
- introduce one primary mechanism or a tightly related concept cluster;
- include the tests or other verification that make it independently demonstrable; and
- have a completion condition that can be stated in one sentence.

Prefer coherent behavioral slices over divisions by file, layer, or line count. As a rough pacing signal, the user should usually be able to enter, study, and verify one example in 10–30 minutes. Split a milestone when its code or explanation contains multiple independently teachable concepts or verification points.

## 3. Present One Worked Example

For the current milestone, provide:

1. **Outcome:** What the example adds and how the user will observe it.
2. **Placement:** The exact files and locations where each fragment belongs.
3. **Code:** Complete, repository-specific code for this milestone, including tests when they are part of the same coherent example.
4. **Explanation:** What the code does, how its pieces interact, and every language or framework mechanism likely to be new to the user.
5. **Reasoning:** Why this implementation fits the Frozen Plan and the surrounding architecture. Discuss alternatives only when they clarify an important decision.
6. **Verification:** The exact feedback-loop command or manual check, plus the expected result.

Providing the code is the purpose of this workflow; do not replace it with hints or pseudocode. At the same time, do not provide later milestones or combine fragments into a solution for the entire plan.

Do not apply the code yourself. After presenting the example, stop so the user can enter it, inspect it, run it, and ask questions.

## 4. Discuss and Verify the Example

Answer the user's questions directly and remain on the current milestone until they say they understand it and are ready to continue. Explain unfamiliar syntax when asked, but do not require a quiz or make the user restate every explanation.

After the user enters the code:

- inspect their actual changes before evaluating them;
- distinguish transcription or adaptation mistakes from errors in the example you supplied;
- explain any discrepancy and provide a corrected fragment when needed; and
- run the relevant feedback loops, or review their output when the user ran them.

Advance only after the milestone behaves as described and its feedback loops pass. If the user wants to devise the solution rather than receive the next worked example, tell them they can explicitly switch to `guided-coding-implement-and-learn-by-solving`.

## 5. Handle Plan Issues During Implementation

Routine choices that the plans leave open may be resolved in the worked example. Explain consequential choices so the user can understand them.

If implementation reveals that an explicit plan decision is wrong or an Acceptance Criterion cannot be met as written:

- identify the exact decision or criterion and explain why it does not work;
- provide a recommended solution or workaround, including its trade-offs and repository-specific code when code is needed;
- clearly label the approach as provisional rather than silently treating it as a new plan decision;
- state which Acceptance Criteria the workaround satisfies and which remain unmet; and
- retain the issue and provisional approach for the final Guiding Phase handoff.

Remain in the Implementing Phase and continue the worked-example workflow with the provisional approach. Do not edit the Frozen Plan or decide whether the departure is accepted. In the Guiding Phase, a senior developer reviews the complete implementation and decides whether to accept the difference and record it in a Plan Deviations document, or return to the Planning Phase and write a Follow-Up Plan.

## 6. Finish the Learning Implementation

After all milestones:

- inspect the complete implementation diff;
- compare it with every Acceptance Criterion;
- run all applicable feedback loops and identify any required manual checks; and
- report which criteria are verified and which remain incomplete; and
- summarize every plan issue, its provisional solution or workaround, and its impact.

Do not check the criteria yourself or claim that an unmet criterion is satisfied. The implementation pass is ready for the Guiding Phase when its milestones and applicable feedback loops are complete, even when a known plan issue leaves a criterion unmet. State clearly that the senior review must decide whether each provisional departure becomes a Plan Deviation or requires a return to the Planning Phase.
