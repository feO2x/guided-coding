---
name: guided-coding-finish-plan
description: Validate and freeze a Guided Coding plan, then optionally publish it to its tracker issue. Run only when explicitly requested by the user.
license: MIT
---

# Finish a Plan

Finishing a plan means freezing it and providing a timestamp, both at the beginning of its file name and directly after its title. The plan is then considered ready for implementation.

The target plan should be already mentioned in the conversation, typically in the format `<topic>.md`. If not, ask the user to provide the path to the plan .md file.

## 1. Determine the Timestamp

The `<timestamp>` is UTC in the format `YYYY-MM-DD-HHMM`. Use these commands to get it:

- `date -u +%F-%H%M` on Unix-based shells
- `(Get-Date).ToUniversalTime().ToString("yyyy-MM-dd-HHmm")` on PowerShell

## 2. Determine an Optional Ticket ID

Plans are typically associated with a `<ticket-id>`. This is the ID of a work item such as a GitHub issue or a Jira Task. If you don't know it from the conversation yet, ask the user to provide it. Note that not each plan needs to be associated with a ticket.

## 3. Change the Plan File Name

- `<timestamp>-<ticket-id>-<title>.md` if `<ticket-id>` is present
- `<timestamp>-<title>.md` otherwise
