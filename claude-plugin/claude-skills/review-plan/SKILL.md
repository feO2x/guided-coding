---
name: review-plan
description: "Review a completed Guided Coding plan draft against the codebase before its Planning Phase ends. Run only when explicitly requested by the user."
license: "MIT"
disable-model-invocation: true
---

# Review a Plan

Review the plan named by the user. If none is named, proceed only when exactly one uncommitted plan draft exists in `ai-plans/`; otherwise ask for its path. Report findings only.

You may build and run the repository's code and tests, and write throwaway probe programs outside the repository, for example in a temporary directory. Never edit the plan or any other file in the repository: the working tree stays unchanged, so `git status` shows the same result before and after the review.

Read the plan, applicable repository instructions including the content rules in `ai-plans/AGENTS.md`, the code it discusses, and every earlier document for the same issue, including legacy filenames. Do not reject files or types merely because the plan intends to create them.

## Run the Plan Checklist

Go through the plan checklist in `assets/plan-checklist.md` relative to this skill file and report its results as the checklist describes.

## Verify Claims About Current Behavior

Verify every claim about existing types, members, files, APIs, extension points and current behavior against the code. Where reading is not enough, build and run the code or write a small probe program. Check the claims that justify a decision first.

Report false claims with the evidence that contradicts them. Report claims you could not verify separately, with what prevented verification.

## Check the Content

Amongst other things, check for:

- **Open decisions:** a choice in the decision categories of `guided-coding-write-plan` (public API, observable behavior, wire and storage formats, configuration and options, dependencies, scope, test cases beyond routine coverage) that the plan leaves to the implementer without a `Delegated:` item.
- **Decisions hidden in Implementation Context:** statements that are true only after the change, or a new pattern to introduce. These belong under Decisions.
- **Untested decisions:** a decision that changes behavior without a criterion that fails without it, or a decision justified by a claim about how the system behaves without a criterion that tests the claim.
- **Sets without a rule:** a decision or criterion that refers to a set without a decision that defines it with a `Rule:` line and a `Members:` list.
- **Acceptance criteria:** each criterion describes an observable, verifiable outcome rather than an implementation step. Bypass paths, negative cases and "nothing was written" assertions are named explicitly.
- **Verifiability:** the required feedback loops exist in repository instructions, build files, scripts, or CI configuration.
- **Technical decisions:** data structures, abstractions, and framework mechanisms fit the surrounding architecture without unnecessary indirection.
- **Level of detail:** the plan records decisions without prescribing routine craft or the sequence of edits, and states each decision once.
- **Omissions:** judge relevant risks such as error handling, compatibility, migration, performance, security boundaries, and observability against the change at hand.
- **History:** a follow-up plan names the plans it follows by filename and names each superseded decision as `<filename> Decision N`.

Say explicitly when there are no findings. If the plan is already frozen (timestamp in its file name and a `*Frozen at ...*` line below its title), note that it can no longer be edited; changes are decided in the Guiding Phase and recorded in a Follow-Up Plan.
