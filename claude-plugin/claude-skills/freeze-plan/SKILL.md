---
name: freeze-plan
description: "Freeze a Guided Coding plan. Run only when explicitly requested by the user."
license: "MIT"
disable-model-invocation: true
---

# Freeze a Plan

Freezing a plan means providing a timestamp, both at the beginning of its filename and directly after its title. An optional `<ticket-id>` is also inserted into the filename after the timestamp.

The plan is now ready for implementation. The only permitted edits are checking off acceptance criteria after verifying them.

Use the plan named in the conversation, typically `<short-title>.md`. If none is named, use a plan in `ai-plans/` only if exactly one Markdown plan is untracked or modified. Otherwise, ask the user for its path.

If the plan already carries a timestamp in its filename or a `*Frozen at ...*` line below its title, it is frozen. Report this to the user and stop.

## 1. Determine the Timestamp

The `<timestamp>` is UTC in the format `YYYY-MM-DD-HHMM`. Use these commands to get it:

- `date -u +%F-%H%M` on Unix-based shells
- `(Get-Date).ToUniversalTime().ToString("yyyy-MM-dd-HHmm")` on PowerShell

## 2. Determine an Optional Ticket ID

Plans are typically associated with a `<ticket-id>`. This is the ID of a work item such as a GitHub issue or a Jira task. If you don't know it from the conversation yet, ask the user to provide it. A ticket ID is optional.

## 3. Change the Plan File Name

- `<timestamp>-<ticket-id>-<short-title>.md` if `<ticket-id>` is present
- `<timestamp>-<short-title>.md` otherwise

If a file with this name already exists in `ai-plans/`, report the collision and stop. Never overwrite an existing file.

## 4. Stamp the Plan Content

Directly below `# Title`, insert `*Frozen at <timestamp-utc>*`, where `<timestamp-utc>` is the filename's timestamp in ISO 8601 format, to the minute. For example, `2026-09-17-0231` becomes `2026-09-17T02:31Z`.
