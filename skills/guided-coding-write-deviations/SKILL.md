---
name: guided-coding-write-deviations
description: Decide whether a completed Guided Coding cycle needs a Plan Deviations document and write it when required. Run only when explicitly requested by the user.
license: MIT
---

# Write Plan Deviations

Use the issue or topic named by the user. Write the document into `ai-plans/`. Before writing, read
every plan for the work oldest first, including legacy filenames. Inspect the final branch diff and
relevant git history to determine what was actually implemented. Verify every document and
source-file reference.

## Decide whether the document is required

Write a Plan Deviations document when the final implementation materially changes or rejects an
explicit plan decision concerning:

- an acceptance outcome;
- a public contract or data model;
- an architectural or component boundary;
- a security or performance constraint; or
- another decision future maintainers need in order to understand the design.

Do not record routine implementation choices the plan deliberately left open. An unmet acceptance
criterion is not resolved merely by documenting it: it must remain incomplete or be explicitly
superseded by an accepted follow-up plan.

If there are no material deviations, report that conclusion and do not create a file.

## File name

Use one of these forms:

```text
YYYY-MM-DD-HHMM-<issue-id>-plan-deviations.md
YYYY-MM-DD-HHMM-<topic>-plan-deviations.md
```

Use the second form only when there is no tracker issue. Obtain the current UTC timestamp from
the shell; never infer it from conversation context. Normalize issue identifiers exactly as the
related plans do.

## Structure

Use `# <issue-id or topic> Plan Deviations` as the title.

Open with a paragraph naming every plan in the comparison by exact filename and the implementation
branch. Explicitly identify plans that were not implemented.

Add:

1. `## Summary`: state what held up and how many material decisions changed.
2. `## Deviations From the Original Plan`: add one numbered `###` subsection per deviation with:
   - **Original plan:** what the original plan specified.
   - **Implemented:** what the final code does.
   - **Why:** why the decision changed. Never omit this.
   - **Impact:** relevant trade-offs, consequences, or deferred work. Omit only when there is none.

Name the affected types, members, and files. Do not add entries for work that matched the plan.

## Finalize

Allow corrections while the document is being reviewed in the current conversation. After the
user accepts it, commit only that file following repository conventions. From that commit onward,
the document is frozen: never edit, rename, or delete it.

When a PR or MR exists, offer to publish the accepted document as its description. Never update
the tracker without the user's approval. Never edit the plans being compared.
