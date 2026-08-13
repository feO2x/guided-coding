---
name: guided-coding-prepare-issue-for-plan
description: Create an empty tracker issue and a clean local branch for a Guided Coding plan. Run only when explicitly requested by the user.
license: MIT
---

# Prepare an Issue for a Plan

Create the issue and branch only. Leave planning to the user and the planning conversation.

## 1. Check prerequisites

Read the repository instructions. Before creating anything:

1. Run `git status`. If the worktree has uncommitted changes, stop before creating the issue.
2. Determine the repository's default branch.
3. Determine the issue tracker and target project from repository instructions. If they are not
   documented, infer them only when the git remote and tracker clearly agree, such as `gh` with a
   GitHub remote. State the inferred target and ask the user to confirm it.

Use the title supplied by the user. If none is provided, ask for a short title. Derive a lowercase,
hyphen-separated topic of at most four words.

Example: `Support cancelled events` becomes `cancelled-events`.

## 2. Create the issue

Create the issue with the agreed title and an empty description. Do not add a summary,
acceptance criteria, or placeholder text. Use the tracker's documented CLI and check its help
rather than guessing flags. For GitHub:

```sh
gh issue create --title "<title>" --body ""
```

Read the identifier and URL from the command output. Normalize the identifier for filenames and
branches: lowercase, no leading `#`, no zero padding, and spaces replaced by hyphens.

If the project has no issue tracker, skip issue creation and use the topic alone for the branch
and later plan filename.

## 3. Create the branch

Update the default branch with a fast-forward-only pull, then create `<issue-id>-<topic>` or,
without an issue, `<topic>`:

```sh
git switch <default-branch>
git pull --ff-only
git switch -c <branch-name>
```

Stop if the intended branch already exists; do not reuse or rename it implicitly.

## 4. Report

Report the issue identifier and URL, when present, and the branch name. Then stop. Do not write
the plan.
