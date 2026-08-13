# Repository Instructions

This repository is the canonical distribution of Guided Coding skills. The root package supports
the Agent Plugins standard and includes a thin Claude Code marketplace adapter.

## Skill authoring

- Keep canonical skill instructions under `skills/<skill-name>/SKILL.md`.
- Use only `name` and `description` in `SKILL.md` frontmatter.
- Keep the skill directory and frontmatter name identical.
- State in every description that the skill runs only when explicitly requested.
- Put Codex-specific interface and invocation policy in `agents/openai.yaml`.
- Do not duplicate skill bodies for individual coding-agent harnesses.

## Manifests and versions

Keep the version synchronized across `plugin.json`, `.claude-plugin/plugin.json`,
`.claude-plugin/marketplace.json`, and release tags.

Use Conventional Commits messages.

## Feedback loops

- Run `dotnet test` after changing skills or manifests.
- Run `claude plugin validate . --strict` when Claude Code is installed.
- Run `gh skill publish --dry-run` before publishing a release.

## This is your space

If you find something noteworthy while working in this repository, add it here for discussion.
