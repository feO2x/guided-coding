---
name: implement
description: "Implement a Guided Coding Frozen Plan independently and verify the implementation through the repository's feedback loops. Run only when explicitly requested by the user."
license: "MIT"
disable-model-invocation: true
---

# Implement a Frozen Plan

Your goal is to implement a Frozen Plan created in the Planning Phase of Guided Coding. Once you are finished implementing, a reviewer will check your results in the Guiding Phase.

## 1. Establish the Target

Use the plan named by the user. If none is named, proceed only if exactly one plan in `ai-plans/` has all acceptance criteria unchecked and that plan also has the latest timestamp. Otherwise, ask for its path.

Verify that the plan is frozen: its filename has a timestamp, and it has a `*Frozen at ...*` line below its title. If either marker is missing, explain that the Planning Phase is unfinished and stop.

## 2. Check the Plan Against the Code

Before you write code, verify that the types, members, and files the plan describes as existing are present and that its claims about existing behavior match what the code currently does. Do not treat planned additions or behavior changes as missing prerequisites.

If reading the code is not enough, use temporary tests or projects to check the actual behavior. A simulation alone does not confirm it. Remove these temporary tests or projects before you finish.

## 3. Implement and Verify

Implement the plan and use the feedback loops to verify your changes.

The only permitted plan edit is checking off an acceptance criterion from `- [ ]` to `- [x]`. Do so only after a feedback loop verifies it. The reviewer would otherwise have to identify the gap later in the Guiding Phase, which is one of the hardest errors to spot.

## 4. Handle Plan Issues

If a discrepancy does not affect the plan's decisions or intended behavior, such as a moved file or renamed private member, proceed and mention it in your report.

If the plan contains an incorrect decision or factual claim, or an acceptance criterion cannot be met as written, use a workaround only when all of these conditions are met:

1. Evidence from a test, temporary project, or compiler error confirms the error.
2. The plan's Rationale and all its other decisions remain unchanged.
3. The workaround makes no public API, scope, or dependency changes beyond those in the plan.
4. No test is weakened or removed.

Leave an acceptance criterion unchecked if it is met only through a workaround. During the Guiding Phase, the reviewer checks it off if they accept the workaround. If they reject it, they call for a Follow-Up Plan.

If any of these four conditions is not met, do not work around the problem and do not ask the user mid-run. Stop the work that depends on the incorrect decision, claim, or acceptance criterion, finish the work that does not, and report the problem. The reviewer decides how to proceed during the Guiding Phase.

## 5. Finish

Report:

- What was implemented, which feedback loops ran, and their results.
- Which acceptance criteria are checked, and which remain unchecked and why.
- Each workaround: the decision it departs from, the evidence, and what you did instead.
- Each problem you did not work around: what the plan says, what you observed and how, and the options you see.
- Any discrepancies that did not affect the plan's decisions or intended behavior.

Reference decisions by the plan's filename and Decision ID, such as `<plan-file-name> D3`. If the plan has no Decision IDs, name its filename and section, and quote the decision.

Do not create commits, push, open or update pull requests, or publish anything unless the user requested those actions.
