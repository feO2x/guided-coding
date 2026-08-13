# Repository Instructions

This repository is the canonical distribution of Guided Coding skills. The root package supports
the Agent Plugins standard and generates a dedicated Claude Code marketplace adapter.

## Skill authoring

- Keep canonical skill instructions under `skills/<skill-name>/SKILL.md`.
- Use only `name`, `description`, and `license` in `SKILL.md` frontmatter.
- Keep the skill directory and frontmatter name identical.
- State in every description that the skill runs only when explicitly requested.
- Put Codex-specific interface and invocation policy in `agents/openai.yaml`.
- Configure Claude-specific names and frontmatter in
  `tools/GuidedCoding.ClaudeGenerator/claude-skills.json`.
- Do not edit `claude-plugin/claude-skills` directly. Regenerate it with
  `dotnet run --project tools/GuidedCoding.ClaudeGenerator`.
- Do not hand-author duplicate skill bodies for individual coding-agent harnesses.
- Keep the generated directory named `claude-skills`. A standard `claude-plugin/skills` directory
  is also discovered by GitHub CLI and would duplicate the portable skills during publication.

## Manifests and versions

Keep the version synchronized across `plugin.json`, `claude-plugin/.claude-plugin/plugin.json`,
`.claude-plugin/marketplace.json`, and release tags.

Use Conventional Commits messages.

## Feedback loops

- Regenerate the Claude adapter and run `dotnet test` after changing skills or manifests.
- Run `dotnet run --project tools/GuidedCoding.ClaudeGenerator -- --check` to detect drift.
- Run `claude plugin validate . --strict` when Claude Code is installed.
- Run `gh skill publish --dry-run` before publishing a release.

## This is your space

If you find something noteworthy while working in this repository, add it here for discussion.
