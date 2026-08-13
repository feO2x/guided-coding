---
name: guided-coding-finish-plan
description: Validate and freeze a Guided Coding plan, then optionally publish it to its tracker issue. Run only when explicitly requested by the user.
---

# Finish a Plan

Finish the plan named by the user. If none is named, proceed only when exactly one uncommitted plan
draft exists in `ai-plans/`; otherwise ask for its path.

## 1. Validate

Read the repository instructions and confirm that:

- The filename is either
  `YYYY-MM-DD-HHMM-<issue-id>-<kebab-case-description>.md` or, without an issue,
  `YYYY-MM-DD-HHMM-<kebab-case-description>.md`.
- The file starts with `# Title`, followed by exactly `## Rationale`,
  `## Acceptance Criteria`, and `## Technical Details`, in that order.
- Every acceptance criterion is an unticked task (`- [ ]`).
- Referenced plan documents exist, and claims about existing source files are accurate. Paths for
  files the plan intends to create are valid references when identified as planned work.

Report validation failures. Fix them only after the user agrees; the Planning Phase is still open
until the plan is committed.

## 2. Commit and freeze

Commit only the plan file, following repository commit conventions. Do not include unrelated
changes and do not push. The successful commit ends the Planning Phase and freezes the plan.

## 3. Optionally publish the first plan

The first plan for a tracked issue may become that issue's description. Follow-up plans are not
published there.

Use the tracker and target project documented by the repository. If neither is documented, infer
them only when the git remote and tracker clearly agree, state the inferred target, and ask the
user to confirm it. If the project has no tracker, skip this step.

Ask before changing the tracker. Publish the plan body only. For GitHub:

```sh
gh issue edit <issue-id> --body-file <plan-path>
```

If the plan and issue titles differ, report it without renaming either. Treat the tracker body as
a publication snapshot, never as the source from which the committed plan is amended.

## 4. Report

Report the committed plan path and state that its Planning Phase has ended and it is now frozen.
