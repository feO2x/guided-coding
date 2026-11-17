---
name: write-deviations
description: "Decide whether a completed Guided Coding cycle needs a Plan Deviations document and write or update it when required. Run only when explicitly requested by the user."
license: "MIT"
disable-model-invocation: true
---

# Write Plan Deviations

Your goal is to decide whether the completed implementation needs a Plan Deviations document and, if so, to write it into the `ai-plans/` folder at the repository root or update the existing one. After writing, report the document path and stop.

A Plan Deviations document summarizes how the completed implementation differs from the original plan. Reading the initial plan together with this document should be enough to understand the complete work. A work item has at most one Plan Deviations document. It stays editable until it is merged into the target branch, because reviews of the PR/MR can still lead to Follow-Up Plans. From then on, it is frozen.

## 1. Inspect the Plans and the Implementation

Read all plans for the current ticket in `ai-plans/`, oldest first. Identify them by the `<ticket-id>` or `<short-title>` in their file names, or by the plans mentioned in the conversation. Ask the user if the set of plans is unclear.

Determine the target branch and the implementation range. Use the PR/MR if there is one. Otherwise, use the merge base between the target branch and the implementation branch. Ask the user if the target branch is unclear. Inspect the commits in that range, all committed, staged, and unstaged changes, and any relevant earlier history. Verify every document and source-file reference you rely on.

## 2. Check for an Existing Plan Deviations Document

Look for an existing Plan Deviations document of the current ticket in `ai-plans/`. Its file name ends with `-<ticket-id>-plan-deviations.md` or `-<short-title>-plan-deviations.md`, using the same `<ticket-id>` or `<short-title>` as the Initial Plan. If you find more than one, report them and stop. The user decides which one to keep.

If there is an existing document, check whether it is already merged into the target branch:

1. Fetch the target branch from its remote into its remote-tracking branch with an explicit refspec, e.g., `git fetch origin +refs/heads/main:refs/remotes/origin/main`, and use that remote-tracking branch, e.g., `origin/main`. The explicit refspec updates it regardless of the remote's configured fetch mappings. Only if the repository has no remote at all, use the local target branch. If fetching fails, report the error and stop without changing anything: a stale local branch cannot prove that the document is unmerged.
2. Verify that the target branch resolves, e.g., `git rev-parse --verify origin/main`. If it does not, report this and stop without changing anything.
3. Check whether the document's path exists on the target branch, e.g., `git cat-file -e origin/main:ai-plans/<file-name>`. Check the path, not whether commits are ancestors of the target branch: squash and rebase merges rewrite commits but keep the file.

If the document exists on the target branch, it is frozen. Report that it was merged into the target branch, and that the user can delete it and run this skill again to recreate it from scratch. Do not delete or change it yourself, and stop.

Otherwise, you update the existing document in the following steps.

## 3. Decide Whether a Document Is Needed

Write a Plan Deviations document in either of these cases:

- **Follow-Up Plans exist.** Summarize the changes from the first plan so that the first plan and this document explain the complete work.
- **The implementation materially changes or rejects an explicit plan decision.** This applies to decisions about at least one Acceptance Criterion, a public contract or data model, an architectural or component boundary, a security or performance constraint, or another design decision future maintainers need to know about.

Routine choices the plans left open are not deviations. An Acceptance Criterion that is documented but not met remains incomplete. It does not count as a deviation unless an accepted Follow-Up Plan explicitly supersedes it.

If there is only one plan and no material deviations, report that conclusion to the user, create nothing, and stop. If an existing, unmerged document is no longer needed, also report this, leave the document unchanged, and let the user decide whether to delete it.

## 4. Determine the File Name

The file name is:

- `<timestamp>-<ticket-id>-plan-deviations.md` if a `<ticket-id>` is present
- `<timestamp>-<short-title>-plan-deviations.md` otherwise

Take `<ticket-id>` or `<short-title>` from the file name of the Initial Plan. The `<timestamp>` is the current time in UTC in the format `YYYY-MM-DD-HHMM`, also when you update an existing document. Use these commands to get it:

- `date -u +%F-%H%M` on Unix-based shells
- `(Get-Date).ToUniversalTime().ToString("yyyy-MM-dd-HHmm")` on PowerShell

The Plan Deviations document must sort after all plans of the ticket. Therefore, the `<timestamp>` must be strictly later than the timestamps in the file names of all plans of the ticket and of the existing Plan Deviations document. If it is not, wait until the next minute and get the timestamp again.

If a plan or another Plan Deviations document with this file name already exists, report the collision and stop. Never overwrite a plan or a frozen Plan Deviations document.

## 5. Write the Document

When you update an existing document, first move it to the new file name. Use `git mv <old> <new>` if the old file is tracked by Git, so that Git can detect the rename. Otherwise, rename the file. Then rewrite the whole document from the plans and the implementation you inspected in step 1. Do not append to the previous content, and do not mention earlier versions of the document.

Structure the document as follows:

1. `# <ticket-id or short-title> Plan Deviations` as the title. Prefer `<ticket-id>` if present.
2. An opening paragraph that names every compared plan by its exact file name, names the implementation branch, and identifies plans that were not implemented.
3. `## Summary`: what held up, and how many material decisions changed.
4. `## Changes Across Follow-Up Plans`, only when Follow-Up Plans exist: one numbered `###` section per superseded decision with **Original decision**, **Superseded by** (exact file name and the replacement decision), **Why**, and **Outcome**.
5. `## Deviations From the Accepted Plans`, only when material deviations exist: one numbered `###` section per deviation with **Plan decision** (exact file name and decision), **Implemented**, **Why** (required), and **Impact** (trade-offs, consequences, or deferred work; omit only if there are none).

Name the affected types, members, and files. Leave out work that matches the accepted plans. If the implementation matches all Follow-Up Plans, say so in the Summary and omit `## Deviations From the Accepted Plans`.

## 6. Stop

Report the document path. When you updated an existing document, also report its previous path and whether the rename is staged. Do not commit, publish, or create or update a PR/MR. The user reviews and finalizes the document.
