# RDAT Guided Coding Plan Structure

RDAT stands for Rationale, Decisions, Acceptance Criteria, and Technical Details.

## Plan Structure

Start with `# Title`, followed by these four sections in order. Only the optional `## Notes for the Guided Learning Teacher` section described at the end may follow them.

### 1. `## Rationale`

In one or two sentences, state the background and the problem this plan solves.

### 2. `## Decisions`

A reviewer who reads only this section should know what will be built and why, and be able to approve the plan.

Record a Decision when an alternative choice would affect behavior, public contracts, scope, or dependencies. State the choice, not options left to the implementer, and give a one-sentence reason when the choice is not obvious. Leave routine implementation choices and what stays unchanged out.

Each Decision gets a **Decision ID**: `D1`, `D2`, etc. Keep existing Decision IDs when revising the draft. Use as few Decisions as the plan needs.

Check the assumptions behind each Decision against the code, including relevant claims made in the ticket and the behavior of existing code you plan to reuse. If reading the code is not enough, use temporary tests or projects to check the actual behavior. A simulation alone does not confirm it.

If an important assumption remains unverified, state it in the affected Decision, along with its consequence and what would resolve it. If a check disproves an assumption behind your own Decision, revise that Decision. If it disproves an assumption behind the user's Decision, ask the user.

The last Decision may be an `Out-of-Scope` Decision, listing what the ticket or the discussion mentioned and the plan deliberately does not address. Do not invent exclusions.

### 3. `## Acceptance Criteria`

List **observable, verifiable outcomes** as unchecked Markdown tasks (`- [ ]`). Each Decision, except the `Out-of-Scope` Decision, must have at least one acceptance criterion that verifies it. Criteria must not introduce behavior, contracts, scope, or dependencies absent from the Decisions.

Tests and routine documentation that verify or explain a Decision should refer to it. They do not need separate Decisions.

Choose feedback loops listed in `AGENTS.md` or its referenced documents that verify the criterion's outcome, such as compilers, static analyzers, automated tests, code coverage, mutation testing, and benchmarks. If no feedback loops are listed, stop and report this to the user.

- When behavior changes, require automated tests and name the cases they must cover, including negative and failure cases.
- Do not add acceptance criteria for checks every change must pass anyway, such as building or running the whole test suite.
- Require benchmarks only when performance is a material risk or requirement.
- Weigh each feedback loop's runtime against its benefits. Reserve slow checks, such as mutation testing, for criteria important enough to justify the time.
- Some acceptance criteria need human review, e.g., the quality of documentation or platform behavior that the available feedback loops do not cover. State what is checked and who verifies it.
- If a plan requires a missing feedback loop, adding it is a Decision with its own acceptance criterion.

### 4. `## Technical Details`

Provide only what the implementer, who has not seen the planning conversation, cannot quickly see in the code: where a change goes when it is not obvious, existing code to follow, interactions with other parts of the codebase, pitfalls, performance, threading or security aspects, and how to structure automated tests. Nothing here adds a behavior, a contract, or an exclusion: that is a Decision.

Avoid step-by-step instructions and exhaustive listings. Use minimal code samples only when they clarify an important contract, such as an API signature, interface, or DTO shape. Say whether an example is exact or illustrative. An exact example must compile. Avoid providing finished implementations.

## How to Keep Plans Short and to the Point

State each fact once. Each section adds information to the previous sections. In Acceptance Criteria and Technical Details, refer to Decisions by their Decision IDs instead of repeating them. Keep exact contracts, such as error messages or verdict tables, in one place and refer to them from other sections.

Acceptance Criteria explain how to verify outcomes, including test inputs, expected results, and failure cases. Technical Details explain how to implement them.

The same goes for general guidance in `AGENTS.md` or its referenced documents. Refer to them by a link to their section or by their rule ID instead of duplicating their information.

Leave out the history of your investigation and routine details the implementer can quickly find in the code.

## One Additional Section: `## Notes for the Guided Learning Teacher`

When a plan will be implemented using the `guided-coding-implement-show-me` or the `guided-coding-implement-coach-me` skill, it may include this additional section. Use it for further topics to teach during the Implementing Phase, such as SOLID, TDD, or technology internals.
