# Plan Checklist

Check the plan against every item below. Each item is a yes/no question answered from the plan text alone: it passes when its statement is true of the plan. An item that does not apply, such as the follow-up item for an initial plan, passes. Judgment beyond these items stays with the reviewer.

Report every item as pass or fail, and quote the passage for every failure.

- The sections are exactly Goal, Decisions, Out of Scope, Acceptance Criteria and optionally Implementation Context, in this order. Subheadings appear only inside Decisions, Acceptance Criteria and Implementation Context.
- The Goal has at most four sentences.
- Decisions are numbered consecutively from 1.
- Every "Decision N" reference points to an existing decision; follow-up plans reference earlier decisions as `<filename> Decision N`.
- Every acceptance criterion is an unticked `- [ ]` task.
- No decision or criterion offers the implementer a choice ("or" between alternatives, "e.g.", "for example", "such as", "etc.", "if needed", "is fine", "may", "optionally", "ideally", "where appropriate") unless it is a `Delegated:` item.
- Every `Delegated:` item names a boundary and a verification.
- Every `Rejected:` line has the form `Rejected: X, because Y.`
- Every `Rule:` line is followed by a `Members:` list, and every reference to "the members of Decision N" points to a decision with a `Members:` list.
- Each decision has at most one "because" clause, not counting its `Rejected:` lines.
- Implementation Context contains no requirements ("must", "should", "do not", "never").
- A follow-up plan names the plans it follows by filename and lists each superseded decision.
