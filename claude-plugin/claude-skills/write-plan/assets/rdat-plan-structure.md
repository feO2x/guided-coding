# RDAT Guided Coding Plan Structure

RDAT stands for Rationale, Decisions, Acceptance Criteria, and Technical Details and describes the main sections of this plan.

## Plan Structure

Start with `# Title`, followed by these four sections in order. Only the optional `## Notes for the Guided Learning Teacher` section described at the end may follow them.

### 1. `## Rationale`

In one or two sentences, state the background and the problem this plan solves.

### 2. `## Decisions`

Decisions are the main point for communication with superiors: a reader who reads only this section knows what will be built and why.

A Decision is a point where another choice was possible and would change the behavior, a public contract, the scope, or the dependencies. State the choice, not options left to the implementer, and give a one-sentence reason when the choice is not obvious. Leave routine implementation choices and what stays unchanged out.

Each Decision gets a **Decision ID** in the format `Dx` where `x` is a positive incrementing integer number. Keep existing Decision IDs when revising the draft. Be conservative with the number of Decisions, use as few as the plan needs.

Check the premises of each Decision against the code, including relevant claims of the ticket. Create throwaway probes (throwaway tests or throwaway projects) when reading the code is not enough. A probe that only simulates the behavior does not confirm it. State in the plan any premise you could not verify. If a check invalidates a premise of a Decision you made, refine the Decision. If it invalidates a premise of a Decision the user made, ask the user.

The last Decision may be an `Out-of-Scope` Decision, listing what the ticket or the discussion mentioned and the plan deliberately does not address. Do not invent exclusions.

### 3. `## Acceptance Criteria`

This is a list of **observable, verifiable outcomes** as unticked Markdown tasks (`- [ ]`). The implementer will use this list later as a major means to verify their changes to the codebase. Each Decision except the `Out-of-Scope` Decision must be verifiable through at least one AC. ACs must not introduce behavior, contracts, scope, or dependencies that no Decision states.

Choose the appropriate Feedback Loops of AGENTS.md and its referenced documents to make an AC verifiable in an automated fashion. Typical Feedback Loops run compilers, static code analyzers, automated tests and code coverage, mutation testing, automated benchmarks, etc. If no Feedback Loops are listed, stop and report this to the user. Make sure the Feedback Loop actually checks the outcome; a loop that passes without checking anything verifies nothing.

- When behavior changes, require automated tests and name the cases they must cover, including negative and failure cases.
- Do not add ACs for checks every change must pass anyway, such as building or running the whole test suite.
- Require benchmarks only when performance is a material risk or requirement.
- Consider how long a Feedback Loop runs and weigh it against its benefits. E.g., mutation testing typically takes a long time - the corresponding AC should have a high impact to warrant it.
- Some ACs need human review, e.g., the quality of documentation or platform behavior that the available Feedback Loops do not cover. State what is checked and who verifies it.
- If a plan requires a missing Feedback Loop, adding it is a Decision with its own AC.

### 4. `## Technical Details`

Provide only what the implementer, who has not seen the planning conversation, cannot quickly see in the code: where a change goes when it is not obvious, existing code to follow, interactions with other parts of the codebase, pitfalls, performance, threading or security aspects, and how to structure automated tests. Nothing here adds a behavior, a contract, or an exclusion: that is a Decision.

Avoid step-by-step instructions and exhaustive listings. Use minimal code samples only when they clarify an important contract, such as an API signature, interface, or DTO shape. Say whether an example is exact or illustrative; an exact example must compile. Avoid providing finished implementations.

## How to Keep Plans Short and to the Point

State each fact once. Decisions are not repeated in the Acceptance Criteria or Technical Details; refer to them by their Decision ID instead. An exact contract, such as an error message or a verdict table, appears in one place, and the other sections refer to it. Acceptance Criteria add how an outcome is verified, such as test inputs, expected results, and failure cases. Technical Details add how to implement it. This leads to the principle "a section adds information to the previous sections and avoids duplication".

The same goes for general guidance in AGENTS.md or its referenced documents. Refer to them by a link to their section or by their rule ID instead of duplicating their information.

Leave out the history of your investigation and routine details the implementer can quickly find in the code.

## One Additional Section: `## Notes for the Guided Learning Teacher`

When it is known that it is implemented using the `guided-coding-implement-show-me` or the `guided-coding-implement-coach-me` skills, a plan might hold this additional section. It usually contains additional instructions for the Coding Agent on further topics it should teach during the Implementing Phase, such as SOLID, TDD, technology internals, etc.
