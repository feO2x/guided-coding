# Guided Coding Plan Structure

Every plan has exactly this structure. No other sections are allowed.

```markdown
# Title

## Goal
## Decisions
## Out of Scope
## Acceptance Criteria
## Implementation Context
```

Goal, Decisions, Out of Scope and Acceptance Criteria are what the user reviews. Implementation Context is skimmable because it contains no requirements and its facts are verified.

Subheadings (`###`) are allowed inside Decisions, Acceptance Criteria and Implementation Context, for example to group a stacked change by layer. They are not allowed anywhere else.

## `# Title`

The ticket title.

## `## Goal`

Two to four sentences: the problem and the outcome. A follow-up plan names the plans it follows here, by exact filename.

## `## Decisions`

A numbered list with one independently reviewable choice per item. Number the decisions consecutively from 1 across the whole section, also across subheadings, so that "Decision N" stays unique. Write the numbers explicitly.

A decision can carry these lines as sub-items:

- `Rule:` what makes something a member of the set the decision defines, followed by `Members:` and the list of members.
- `Rejected: X, because Y.` for each seriously considered alternative.
- `Supersedes: <filename> Decision N` in a follow-up plan, for each earlier decision that this decision replaces or removes.

A delegated decision is written as `Delegated: X, within Y, verified by Z.`

The layout looks like this:

```markdown
1. `OrderService.Cancel` throws `InvalidOperationException` when the order has shipped, because a refund would let the customer keep the goods for free.
   - Rejected: returning `false`, because no caller checks the return value today.
2. `OrderImporter` skips rows whose status is not importable.
   - Rule: a status is importable when it maps to an `OrderStatus` value other than `Unknown`.
   - Members: `open`, `paid`, `shipped`.
3. Delegated: the order of sections in `docs/orders.md`, within "no information from the current page is lost", verified by the PR description mapping every old heading to its new location.
```

Code appears under Decisions only when it states a contract exactly, such as a signature, an interface, a DTO shape, a configuration snippet or a command.

## `## Out of Scope`

A list of exclusions that might otherwise get built. Name the issue that tracks an exclusion when there is one. If there are none, write `- None.`

## `## Acceptance Criteria`

A list of unticked Markdown tasks (`- [ ]`). Each criterion describes an observable, verifiable outcome, never an implementation step. A criterion may refer to decisions, for example "the members of Decision 2".

The root `AGENTS.md` and the docs it references should list the feedback loops of the repository, such as compilers, static analyzers, automated tests, test coverage, mutation testing and benchmarks. Use them to make the criteria verifiable. If no feedback loops are listed, stop and report this to the user. If the plan requires a missing feedback loop, adding it is a decision.

Require benchmarks only when performance is a material risk or requirement.

## `## Implementation Context`

Optional. What is true of the repository today and matters for a decision or criterion: current behavior, pitfalls, existing code to follow, and the repository docs that apply. Name the type, member or file. Every fact in this section is verified, and nothing in it is a requirement.
