---
name: guided-coding-review-plan
description: Review a completed Guided Coding plan draft against the codebase before its Planning Phase ends. Run only when explicitly requested by the user or by another Guided Coding skill.
license: MIT
---

# Review a Plan

Review the plan named by the user or by the invoking skill. If none is named, proceed only when exactly one uncommitted plan draft exists in `ai-plans/`. Otherwise, ask for its path. Report findings only. Never edit the plan or any other file.

Read the plan, applicable repository instructions, the code it discusses, and every earlier document for the same issue, including legacy filenames. Verify assertions about existing types, members, files, APIs, and extension points. Do not reject files or types merely because the plan intends to create them.

Check at least the following:

- **Acceptance criteria:** Each criterion describes an observable, verifiable outcome rather than an implementation step. Behavior changes require appropriate automated test coverage.
- **Verifiability:** The required feedback loops exist in repository instructions, build files, scripts, or CI configuration.
- **Technical decisions:** Data structures, abstractions, and framework mechanisms fit the surrounding architecture without unnecessary indirection.
- **Level of detail:** The plan records non-obvious decisions and interactions without prescribing routine implementation.
- **Omissions:** Check whether the plan addresses risks relevant to the change, including error handling, compatibility, migration, performance, security boundaries, and observability.
- **History:** A follow-up plan explicitly identifies and explains any earlier decisions it supersedes.

Say explicitly when there are no findings. If the plan is already frozen (timestamp in its filename and a `*Frozen at ...*` line below its title), note that it can no longer be edited. Changes are decided in the Guiding Phase and recorded in a Follow-Up Plan.
