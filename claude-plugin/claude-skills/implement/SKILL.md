---
name: implement
description: "Implement a Guided Coding Frozen Plan independently and verify the implementation through the repository's feedback loops. Run only when explicitly requested by the user."
license: "MIT"
disable-model-invocation: true
---

# Implement a Frozen Plan

Your goal is to implement a Frozen Plan created in the Planning Phase of Guided Coding. Once you are finished implementing, a reviewer will check your results in the Guiding Phase.

## 1. Establish the Target

Use the plan named by the user. If none is named, proceed only when there is exactly one plan in `ai-plans/` that has all its Acceptance Criteria unchecked, and it has the latest timestamp of all plans. Otherwise, ask for its path.

Verify that the plan is frozen: its file name has a timestamp, and it has a `*Frozen at ...*` line below its title. If either marker is missing, explain that the Planning Phase is unfinished and stop.

## 2. Check the Plan Against the Code

Read the root `AGENTS.md` and the documents the plan names. Before you change files in a directory, read its `AGENTS.md`, if it has one. Do not crawl every document they reference.

Before you write code, check the plan's premises: the types, members, and files it describes as existing exist, and the current behavior it describes is what the code does. What the plan asks you to add is not a missing premise. Create throwaway probes (throwaway tests or throwaway projects) when reading the code is not enough. A probe that only simulates the behavior does not confirm it. Remove all probes before you finish.

## 3. Implement and Verify

Implement the plan, use the feedback loops to verify your code changes.

The only allowed plan edit is ticking an Acceptance Criterion from `- [ ]` to `- [x]`. Never check a criterion unless it is genuinely satisfied by a feedback loop. The reviewer would otherwise have to identify the gap later in the Guiding Phase, which is one of the hardest errors to spot.

## 4. Handle Plan Issues

A discrepancy that changes nothing, such as a moved file or a renamed private member, is no plan issue: proceed and mention it in your report.

When a plan decision or a fact the plan states is wrong, or an Acceptance Criterion cannot be met as written, work around it only when all of these hold:

1. Evidence shows the error: a test, a probe, or a compiler error.
2. The plan's Rationale and every other decision of the plan stay unchanged.
3. No public API, scope, or dependency changes beyond what the plan already decided.
4. No test is weakened or removed.

An Acceptance Criterion that is met only through a workaround stays unticked. In the Guiding Phase, the reviewer ticks it when they accept the workaround, or calls for a Follow-Up Plan when they do not.

Otherwise, do not work around the problem and do not ask the user mid-run. Stop the work that depends on the wrong decision or fact, finish the work that does not, and report the problem. That is totally fine: in the Guiding Phase, the reviewer decides how to proceed.

## 5. Finish

Report:

- what was implemented, which feedback loops ran, and their results;
- which Acceptance Criteria are checked, and which remain unchecked and why;
- each workaround: the decision it departs from, the evidence, and what you did instead;
- each problem you did not work around: what the plan says, what you observed and how, and the options you see;
- the discrepancies that changed nothing.

Reference a decision by the plan's file name and its Decision ID, such as `<plan-file-name> D3`, when the plan has Decision IDs. Otherwise, name the plan's file name and section, and quote the decision.

Do not create commits, open or update a pull request, publish anything, etc. - unless the user asked you to do so.
