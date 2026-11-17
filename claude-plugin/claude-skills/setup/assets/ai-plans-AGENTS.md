# AGENTS.md for AI plans

<!-- guided-coding-version: 2.1.0 -->

This directory records how work in this repository was planned and how it turned out. Preserve frozen documents and record later decisions in new plans. It follows the [Guided Coding](https://kenny-codes.net/docs/guided-coding/) approach.

Frozen Plan filenames use `YYYY-MM-DD-HHMM-<ticket-id>-<short-title>.md`, timestamped in UTC. The `<ticket-id>` is omitted when work has no ticket. Each frozen plan also contains a `*Frozen at <timestamp-utc>*` line directly below its title. When a plan is not frozen yet, its filename is typically `<short-title>.md`. Plan Deviations filenames use `YYYY-MM-DD-HHMM-<ticket-id>-plan-deviations.md` or `YYYY-MM-DD-HHMM-<short-title>-plan-deviations.md`.

Plans are frozen when their Planning Phase ends. From then on, the only permitted edit is checking off an acceptance criterion (`- [ ]` to `- [x]`) after the implementation and relevant feedback loops verify it. Never reword, add, remove, or reorder criteria in a frozen plan. Plan Deviations documents are frozen when merged into the target branch. Until then, rename and rewrite the document with a new timestamp whenever it changes, for example, when a PR/MR review leads to another Follow-Up Plan. Its filename must sort after all plans for the work item.

A Follow-Up Plan supersedes decisions of a Frozen Plan. The decision to write one is made in the Guiding Phase, when the review of the implementation calls for larger changes.

A work item has at most one Plan Deviations document. It records all changes from the Initial Plan to the final implementation. It is only written when Follow-Up Plans exist or the implementation materially deviates from the plans. Reading the Initial Plan and, if present, the Plan Deviations document is enough to understand all work related to a ticket.
