# Default Guided Coding Plan Structure

Start with `# Title`, followed by exactly these sections in order:

1. `## Rationale`: briefly state the problem and the goal of this issue.
2. `## Acceptance Criteria`: list **observable, verifiable outcomes** as unchecked Markdown tasks (`- [ ]`). Describe results, not implementation steps.
3. `## Technical Details`: record the technical guidance the implementer needs, such as APIs to extend, design patterns to follow, testing and benchmark guidance, links to relevant documentation, and any performance, threading, or security constraints. Include only what is relevant. Avoid step-by-step instructions and exhaustive listings. Assume the implementer is a senior software developer.

Use minimal code examples only when they clarify an important contract, such as an API signature, interface, or DTO shape. Say whether an example is exact or illustrative. Avoid method bodies, step-by-step instructions, exhaustive file lists, and routine background.

When behavior changes, require automated test coverage in the acceptance criteria. Require benchmarks only when performance is a material risk or requirement.

The root `AGENTS.md` and its referenced documents should list the repository's feedback loops, such as compilers, static analyzers, automated tests, code coverage, mutation testing, and benchmarks. Use them to make acceptance criteria verifiable. If there are no feedback loops listed, stop and report this to the user.

If the plan requires a missing feedback loop, make adding it explicit in the plan.
