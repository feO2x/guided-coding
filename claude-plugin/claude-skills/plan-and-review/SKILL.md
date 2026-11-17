---
name: plan-and-review
description: "Write a Guided Coding plan, improve it through independent reviews within a round limit, and present it to the user. Run only when explicitly requested by the user."
license: "MIT"
disable-model-invocation: true
---

# Plan and Review

Write a plan draft in `ai-plans/`, improve it through independent review rounds, report the result, and stop. You are the planner. Only you have access to the planning conversation. Reviewers must not see it. Never freeze the plan.

Harnesses may shorten or namespace skill names, e.g., `guided-coding:write-plan` in Claude Code. Use the name under which each skill is installed.

## 1. Read the Configuration

Read the `## Plan Review` section of `ai-plans/AGENTS.md` if it exists. Options supplied when invoking this skill override the configuration. Use these defaults for values absent from both the invocation options and the configuration:

- **Reviewers:** one reviewer on the harness default model. The configuration can name several, each with a model preference.
- **Round limit:** 3.
- **User decisions:** do not change a decision the user explicitly made in the conversation without their approval. Use the policy set by the invocation options or configuration:

  - `collect` (default): continue and report the finding as open for the user.
  - `pause`: stop the round, ask the user, and continue after they answer.

Model names are harness-specific. If a configured model is unavailable, run that reviewer on the harness default and say so in the report.

## 2. Get the Plan Draft

If the user named a plan draft, use it. Otherwise use an uncommitted draft in `ai-plans/` only if it clearly belongs to the work discussed in this conversation. If several drafts could match the current work, ask the user which one to use.

Without a draft, invoke `guided-coding-write-plan` to write it, passing on any plan structure the user named. If you cannot invoke it, stop and tell the user to run `guided-coding-write-plan` first and then this skill. Never write the plan without that skill.

Record the plan's word count.

## 3. Check for Independent Reviewers

A reviewer must not share this conversation: it runs as a subagent, thread, or separate session, whichever this harness provides. If this harness provides none, report the plan path, state that no review ran, and point the user to `guided-coding-review-plan` in a fresh session. Never review the plan in your own context as a substitute.

## 4. Run the Review Rounds

In each round, start a fresh reviewer for every configured reviewer and run them in parallel where the harness allows. Never reuse a reviewer from an earlier round. Give each reviewer:

- the instruction to invoke `guided-coding-review-plan` on the plan path, return its findings, and write nothing.
- the paths of the earlier documents for the same work.
- the finding classes below, asking it to classify every finding.
- from round 2 on, every earlier finding with its resolution. This is context, not a limit: the reviewer may report anything, but repeats a rejected finding only with a new argument.

Never give a reviewer the planning conversation or your reasoning.

The finding classes are:

- `blocking`: a false premise, an open decision, an acceptance criterion that cannot be verified, or a contradiction with the code.
- `gap`: a missing scenario or fact.
- `nit`: everything else.

A round is complete when every reviewer has returned its findings. If a reviewer fails or times out, start one replacement reviewer. If the replacement also fails or times out, do not retry. Address the findings already returned, then report the incomplete round and stop.

Then address every finding of every class:

- Handle a finding against a decision the user made explicitly as configured under **User decisions**.
- Address every other finding by changing the plan or rejecting the finding with a one-line reason. You may change anything else in the plan, including decisions you made alone and the scope.
- When findings contradict each other, apply at most one and reject the others with a reason. Treat findings that say the same thing as one, with the highest class among them.
- When revising the plan, correct the decision, criterion, or fact and include only the explanation the plan structure requires for a changed decision. Keep review commentary out of the plan. Do not turn a `nit` into a new rule.
- The plan keeps following the rules of `guided-coding-write-plan` and the plan structure it selected.

Keep the reviewers' classifications. After addressing all findings, stop if the completed round reported no `blocking` or `gap` findings. Otherwise, start another round unless the round limit has been reached.

## 5. Report

Report to the user:

- the plan path, the rounds run, and why the run stopped.
- the reviewers and the models actually used, including fallbacks.
- every finding with its round, class, a one-line summary, and its resolution: applied, rejected with the reason, or open for the user.
- a separate list of the decision and scope changes you applied, including those the user approved during a pause, so the user can confirm them without comparing versions.
- the plan's word count before and after review.
- whether the plan changed after the last review round. Those changes are unreviewed.

Then stop. The user reviews the plan before it is frozen.
