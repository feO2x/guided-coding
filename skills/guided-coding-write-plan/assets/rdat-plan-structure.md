# RDAT Guided Coding Plan Structure

RDAT stands for Rationale, Decisions, Acceptance Criteria, and Technical Details and describes the main sections of this plan.

## Plan Structure

Start with `# Title`, followed by exactly these sections in order.

### 1. `## Rationale`

Briefly state the background and the overarching problem this plan is trying to solve.

### 2. `## Decisions`

Each plan must have at least one high-level Decision that drives the changes of the plan. Consider Decisions as the main point for communication with superiors on why the plan exists and should be implemented.

Try to validate each Decision by inspecting the code. Create throwaway probes (throwaway tests or throwaway projects) for further validation if required. If any of your checks fail, try to refine the Decision by yourself, or ask for input if you deem it necessary.

The last Decision may be an `Out-of-Scope` Decision, defining what the plan will explicitly not address.

Each Decision gets a **Decision ID** in the format `Dx` where `x` is a positive incrementing integer number.

Be conservative with the number of Decisions, use as few as the plan needs.

### 3. `## Acceptance Criteria`

This is a list of **observable, verifiable outcomes** as unticked Markdown tasks (`- [ ]`). The implementer will use this list later as a major means to verify their changes to the codebase.

Choose the appropriate Feedback Loops of AGENTS.md and its referenced documents for this plan to make an Acceptance Criterion verifiable in an automated fashion. Typical Feedback Loops run compilers, static code analyzers, automated tests and code coverage, mutation testing, automated benchmarks, etc. When behavior changes, require automated tests in the Acceptance Criteria. Require benchmarks only when performance is a material risk or requirement.

Some ACs cannot be automatically verified, e.g., the quality of documentation. It's OK that a human needs to review these ACs.

Consider how long a Feedback Loop runs and weigh it against its benefits. E.g., mutation testing typically takes a long time - the corresponding AC should have a high impact to warrant it.

If a plan requires a missing feedback loop, make this explicit in the Decisions and Acceptance Criteria.

Each Decision must have at least one AC which provides the means to check whether the Decision has been implemented. List the affected AC at the end in the format `(D1, D3)`. Each AC must reference at least one Decision. 

### 4. `## Technical Details`

Provide additional information here that doesn't belong in the Decisions or Acceptance Criteria sections. These might be hints on how to implement certain aspects, like Principles or Patterns to follow, which code artifacts to create or extend, performance or threading insights, security aspects, how to structure automated tests and/or documentation.

Avoid step-by-step instructions and exhaustive listings. Use minimal code samples only when they clarify an important contract, such as an API signature, interface, or DTO shape. Say whether an example is exact or illustrative. Avoid providing finished implementations.

## How to Keep Plans Short and to the Point

The main idea is to not repeat information that has already been stated. Decisions shouldn't be repeated in the Acceptance Criteria or Technical Details, they should be referenced instead. Acceptance Criteria statements shouldn't be repeated in the Technical Details. This leads to the principle "a section adds information to the previous sections and avoids duplication".

The same goes for general guidance in AGENTS.md or its referenced documents. Refer to them by a link to their section or by their rule ID instead of duplicating their information.

## One Additional Section: `## Notes for the Guided Learning Teacher`

When it is known that it is implemented using the `guided-coding-implement-show-me` or the `guided-coding-implement-coach-me` skills, a plan might hold this additional section. It usually contains additional instructions for the Coding Agent on further topics it should teach during the Implementing Phase, such as SOLID, TDD, technology internals, etc.
