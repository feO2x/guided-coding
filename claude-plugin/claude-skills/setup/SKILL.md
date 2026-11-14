---
name: setup
description: "Set up or upgrade Guided Coding repository instructions, plan storage, and documented feedback-loop commands. Run only when explicitly requested by the user."
license: "MIT"
disable-model-invocation: true
---

# Set Up Guided Coding

Your goal is to set up or upgrade Guided Coding in the current repository. After finishing, report what you changed, give the customization tips below, and stop.

Guided Coding needs these artifacts:

- `AGENTS.md` at the repository root, listing the feedback loops and pointing to `ai-plans/AGENTS.md`.
- `ai-plans/`, the folder holding all plans and Plan Deviations documents.
- `ai-plans/AGENTS.md`, describing the folder, its file naming rules, how Frozen Plans are treated, and any repository-specific content rules for plans.

The outcome must be idempotent. Running this skill on a repository sets the artifacts up from scratch, brings outdated ones up to date, or leaves current ones untouched. Content unrelated to Guided Coding, such as project-specific instructions or user-authored notes, is never changed.

## 1. Inspect the Repository

Read the root `AGENTS.md` and `ai-plans/AGENTS.md` if they exist, plus build manifests, task runners, scripts, and CI configuration.

Identify the feedback loops and their exact commands. Look for:

- compilers, type checkers, static analyzers, and linters;
- automated tests, code coverage, and mutation testing tools;
- benchmarks; and
- dependency, secret, container, and source-code security scans.

Only select commands that are immediately executable in the repository. Do not list tools merely because they are common for the detected language.

## 2. Ensure the Root `AGENTS.md`

Create `AGENTS.md` in the repository root if it does not exist. Otherwise, make the smallest update that adds what is missing and corrects what is outdated. Match existing sections by meaning, not by exact heading.

Ensure it contains:

1. `## Feedback Loops`: each command and what it verifies. Report to the user when no feedback loops could be found, and warn that `guided-coding-write-plan` refuses to write plans until at least one is listed.
2. `## Guided Coding`: a link to `ai-plans/AGENTS.md`, noting that it holds the file naming conventions, the rules for working with Frozen Plans, and the content rules for plans. Do not restate those rules here; they live next to the plans they govern, and agents pick them up when they read the folder.

If both sections already exist and are current, leave the file alone.

## 3. Ensure `ai-plans/AGENTS.md`

Create `ai-plans/` if it is missing. Compare `ai-plans/AGENTS.md` with `assets/ai-plans-AGENTS.md` relative to this skill file.

- If the file is missing, copy the asset.
- If the file exists with an older `guided-coding-version` marker or without one, update the marker and the Guided Coding paragraphs to the text of the asset. Keep every repository-specific addition, such as content rules for plans, naming conventions, or notes about legacy file names.

The plan structure is fixed since Guided Coding 3.0.0. If `ai-plans/AGENTS.md` describes its own plan structure, such as its sections, their order, or what each section contains:

1. Remove that description and keep the repository-specific content rules it contains, under a `## Plan Content Rules` heading.
2. This interprets the user's text, so show the user the resulting file and ask before writing it.

Report every content rule that conflicts with the plan rules below: quote the content rule and name the plan rule. On a conflict, the plan rule applies. For example, "it is usually enough to note 'automated tests need to be written'" conflicts with the rule that every decision that changes behavior has a criterion that fails without it.

Never rewrite plans or Plan Deviations documents in `ai-plans/`, even when they use an older plan structure.

### Plan Rules

Content rules are checked against these rules of `guided-coding-write-plan`:

- **Structure:** `# Title`, then exactly `## Goal`, `## Decisions`, `## Out of Scope`, `## Acceptance Criteria`, and optionally `## Implementation Context`, in this order. Subheadings appear only inside Decisions, Acceptance Criteria, and Implementation Context.
- **Decisions:** numbered consecutively, one reviewable choice each. The plan leaves no choice to the implementer except `Delegated:` decisions, and only the user delegates.
- **Acceptance criteria:** every decision that changes behavior has a criterion that fails without it. Bypass paths, negative cases, and "nothing was written" assertions are named explicitly; routine coverage is one criterion.
- **Reasons:** at most one "because" clause per decision; seriously considered alternatives are recorded as `Rejected: X, because Y.`
- **Implementation Context:** only verified facts about the repository today, never requirements.
- **Implementer:** plans are written for an implementer who has not seen the discussion, may be a small model, and makes no decisions.
- **Brevity:** plans are shortened by removing repetition, routine craft, and nonessential explanation, never decisions, delegation boundaries, test cases, or required reasons.

## 4. Report and Give Customization Tips

List the files you created, updated, or left unchanged, the feedback loops you documented, and which commands you ran. Report to the user that they should verify the changes.

Then explain how Guided Coding can be customized, so the user knows the defaults are only a starting point:

- **Plan structure:** `guided-coding-write-plan` uses a fixed structure with `## Goal`, `## Decisions`, `## Out of Scope`, `## Acceptance Criteria`, and an optional `## Implementation Context`. It cannot be changed.
- **Plan content rules:** rules such as "every behavior change gets a benchmark criterion" belong under `## Plan Content Rules` in `ai-plans/AGENTS.md`. They add to the plan rules and never override them.
- **File names:** the skills use the placeholders `<timestamp>` (UTC, `YYYY-MM-DD-HHMM`), `<ticket-id>`, and `<short-title>` (kebab-case, shortened ticket title). Conventions for these, e.g., a `<ticket-id>` format or a maximum length for `<short-title>`, belong in `ai-plans/AGENTS.md`.
- **Feedback loops:** plans should reference the feedback loops in the root `AGENTS.md` to make Acceptance Criteria verifiable. Adding or removing a feedback loop later only requires updating that list.
- **Deviations:** the threshold for what counts as a material deviation can be tightened or relaxed in `ai-plans/AGENTS.md`.

Repository-specific conventions in `AGENTS.md` files take precedence over the skills' defaults, except for the plan structure and the plan rules, which content rules only add to.
