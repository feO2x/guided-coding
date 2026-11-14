---
name: write-plan
description: "Write a Guided Coding plan or follow-up plan after its approach has been discussed. Run only when explicitly requested by the user."
license: "MIT"
disable-model-invocation: true
---

# Write a Plan

Your goal is to write an Initial Plan or Follow-Up Plan into the `ai-plans/` folder at the repository root. After writing, report as described in "Report" and stop.

## Plan File Name

The initial file name of the new plan file is `<short-title>.md`. `<short-title>` is a shortened kebab-case version of the ticket title. At this point in the conversation, you should be able to derive a full title from the discussion that you can shorten. If not, ask the user to provide one and shorten it.

## Plan Structure

Use the fixed structure in `assets/default-plan-structure.md` relative to this skill file.

Read `ai-plans/AGENTS.md` and apply its content rules for plans, such as required kinds of criteria or naming conventions. Content rules add to the rules of this skill and never override them. On a conflict, follow this skill and report the conflict.

## Rules

### 1. Decisions, Not Options

A decision belongs to one of these categories: public API (types, members, signatures, visibility); observable behavior (results, exceptions and their types, messages, status codes, side effects, and ordering that affects any of them); wire and storage formats; configuration and options; dependencies; scope (which components change); test cases beyond routine coverage (bypass paths, negative cases, "nothing was written" assertions). Everything else is routine craft and stays out of the plan.

- If the discussion did not settle a decision in these categories, ask the user before writing when it is expensive to reverse or has no clear best option. Otherwise choose, and list the numbers of the decisions you chose in your report.
- Delegation is written as `Delegated: X, within Y, verified by Z`. Only the user delegates; never delegate to avoid asking.
- For messages, decide what the message must contain. Fix the exact wording only when it is a contract.

### 2. Sets

A decision that applies to several things lists them in a `Members:` line. A criterion may refer to "the members of Decision N".

Add a `Rule:` line only when membership follows from a property the implementer can check in the repository, so that a member the plan missed can turn up during implementation. Check that the rule, applied to the repository today, yields exactly the listed members. When the implementer finds a difference, the rule applies and the difference is recorded as a deviation.

A list that is complete because the discussion settled it, such as documentation topics, rejected inputs or renamed types, has no `Rule:` line. Never write a rule to describe a list afterwards; a reason for the list is a "because" clause.

### 3. Acceptance Criteria

Every decision that changes behavior has a criterion that fails without it. When a decision is justified by a claim about how the system behaves ("X is safe because Y runs first"), a criterion tests that claim. Bypass paths, negative cases and "nothing was written" assertions are named explicitly; routine coverage is one criterion. One criterion per independently verifiable outcome.

A criterion adds something to its decision: a case, a command, an observable result. Never write a criterion that only says a decision was carried out. Decisions that are checked by reading the result, such as documentation, comments and renames, share one closing criterion: `- [ ] Every decision without a criterion of its own is implemented as written.`

### 4. Reasons

At most one "because" clause per decision, and only when the decision would look wrong, inconsistent or incomplete without it, or would otherwise be re-litigated or quietly undone. Where a mistake is costly and hard to notice, the clause states what is at stake. Record each seriously considered alternative as `Rejected: X, because Y.` Reasons come from the discussion or from evidence, never invented afterwards.

### 5. Write for an Implementer Without Your Context

The implementer has not seen the discussion, may be a small model, and makes no decisions.

- Anything that changes what is built or what a test asserts is a decision and goes under Decisions, including placement, ordering that affects behavior, and changed behavior.
- Under Implementation Context, state what is true of the repository today and matters for a decision or criterion: current behavior, pitfalls, existing code to follow. Name the type, member or file. Nothing in this section is a requirement.
- Name the repository docs that apply; do not restate them.
- Constrain the result, never the sequence of edits. Leave out routine code and navigation.

Test for decision vs. context: true of the repository today → Implementation Context; true only after the change → Decision. A new pattern to introduce is a decision; an existing pattern to follow is context.

### 6. Verify Premises

Before writing a claim about current behavior, check it by reading or running the code. Report claims you could not verify.

### 7. Brevity

Shorten by removing repetition, routine craft and nonessential explanation, never by removing decisions, delegation boundaries, test cases, required reasons or facts that a criterion needs to be checked. Write full sentences and use the qualified name on first mention.

### 8. Self-Check

Before reporting, go through the plan checklist in `assets/plan-checklist.md` relative to this skill file and fix every failing item.

## Follow-Up Plans

A Follow-Up Plan supersedes decisions of a Frozen Plan. The decision to write one is made in the Guiding Phase, when the review of the implementation calls for larger changes.

Use the same structure and file naming convention. Read every earlier document for the ticket. In `## Goal`, name the plans this one follows by exact filename.

Record only the changed decisions and outcomes. Each decision that replaces or removes an earlier decision names it in a `Supersedes: <filename> Decision N` line, and its "because" clause states why. Do not repeat unchanged decisions or contradict earlier plans silently. Unchanged earlier decisions stay valid, and implementers read the referenced predecessors.

## What Happens After Writing a Plan?

The user reviews the plan. They will provide feedback, aspects of the plan might change. Once both you and the user agree on the plan being ready for implementation, it is frozen and becomes largely immutable (except for checking off Acceptance Criteria). Freezing runs the plan checklist and refuses plans that fail it. When a plan is frozen, its file name is changed to `<timestamp>-<ticket-id>-<short-title>.md`, or `<timestamp>-<short-title>.md` (if no `<ticket-id>` is present). Also, a `*Frozen at ...*` line is inserted right below the `# Title` of the plan. All this doesn't happen in this step of the conversation.

## Report

Report:

- the plan path
- the numbers of the decisions you chose because the discussion did not settle them
- the claims about current behavior you could not verify
- conflicts between content rules in `ai-plans/AGENTS.md` and this skill

Then stop.
