# AGENTS.md for AI plans

<!-- guided-coding-version: 2.0.0 -->

This directory is the append-only record of how work in this repository was planned and how it turned out, similar to Event Sourcing. It follows the [Guided Coding](https://kenny-codes.net/docs/guided-coding/) approach.

Frozen Plan file names use `YYYY-MM-DD-HHMM-<ticket-id>-<short-title>.md`, timestamped in UTC when written. The `ticket-id` is omitted when work has no tracker issue. When a plan is not frozen yet, its file name is typically `<short-title>.md`. Plan Deviations filenames use `YYYY-MM-DD-HHMM-<ticket-id>-plan-deviations.md` or `YYYY-MM-DD-HHMM-<short-title>-plan-deviations.md`.

Plans are frozen when their Planning Phase ends. From then on, the only permitted edit is checking an acceptance criterion (`- [ ]` to `- [x]`) after the implementation and relevant feedback loops verify it. Never reword, add, remove, or reorder criteria in a frozen plan. Plan Deviations documents are frozen once committed.

A Follow-Up Plan is used to correct or supersede a Frozen Plan. Plan Deviation files document all changes from the initial plan to when the final implementation is being merged. Only the Initial Plan and the Plan Deviations document need to be read to understand all work related to a Ticket.
