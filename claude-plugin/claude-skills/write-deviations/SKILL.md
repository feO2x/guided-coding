---
name: write-deviations
description: "Decide whether a completed Guided Coding cycle needs a Plan Deviations document and write it when required. Run only when explicitly requested by the user."
license: "MIT"
disable-model-invocation: true
---

# Write Plan Deviations

Use the issue or topic named by the user. Write the document into `ai-plans/`. Before writing, read
every plan for the work oldest first, including legacy filenames. Determine the implementation
range from the associated PR or MR when available. Otherwise, identify the target branch and its
merge base with the implementation branch. Inspect the complete implementation delta, including
committed, staged, and unstaged changes. Inspect the commits in that range—and earlier history when
relevant—to understand why the implementation differs from the plans. Verify every document and
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
related plans do. Resolve the complete destination path before writing and confirm it does not
exist. Never overwrite or reuse an existing plan or Plan Deviations document; report a collision
and stop.

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
user accepts it, inspect `git status` and the staged diff, preserve unrelated changes, and use a
path-limited commit so the commit contains only that file. Verify the resulting commit's file list;
if it contains anything else, stop and report the problem without rewriting history. Follow
repository commit conventions and do not push. From that commit onward, the document is frozen:
never edit, rename, or delete it.

When a PR or MR exists, read its current description and offer to publish the accepted document as
a replacement. Explain that publishing replaces the complete description. If it is non-empty,
show or summarize what would be replaced and require explicit confirmation to overwrite it. Never
update the tracker without the user's approval. Never edit the plans being compared.
