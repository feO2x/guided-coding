# Repository Instructions

This repository contains skills for Guided Coding. The root package supports the Agent Plugins standard and generates a dedicated Claude Code marketplace adapter.

## Skill authoring

- Keep canonical skill instructions under `skills/<skill-name>/SKILL.md`.
- Use only `name`, `description`, and `license` in `SKILL.md` frontmatter.
- Keep the skill directory and frontmatter name identical.
- State in every description that the skill runs only when explicitly requested.
- Put Codex-specific interface and invocation policy in `agents/openai.yaml`.
- Configure Claude-specific names and frontmatter in `tools/GuidedCoding.ClaudeGenerator/claude-skills.json`.
- Do not edit `claude-plugin/claude-skills` directly. Regenerate it with `dotnet run --project tools/GuidedCoding.ClaudeGenerator`.
- Keep the generated directory named `claude-skills`. A standard `claude-plugin/skills` directory is also discovered by GitHub CLI and would duplicate the portable skills during publication.

## Manifests and versions

- Keep the version synchronized across `plugin.json`, `claude-plugin/.claude-plugin/plugin.json`, `.claude-plugin/marketplace.json`, and `MAJOR.MINOR.PATCH` release tags without a `v` prefix. Every tag is a release tag: the release tool refuses to run while any tag has another form.
- Do not bump versions manually. Between releases, the manifests hold the last released version. Release with `dotnet run --project tools/GuidedCoding.Release` or the manually triggered Release workflow in GitHub Actions. The tool derives the next version from the commits since the last release tag, updates the manifests and `CHANGELOG.md`, validates, commits, tags, pushes, and creates the GitHub release with `gh release create`, using the version's `CHANGELOG.md` entries as notes. `gh skill publish --tag` cannot publish a tag that already exists, so the tool only runs `gh skill publish --dry-run`.
- Describe user-facing changes under `## [Unreleased]` in `CHANGELOG.md`.

## Commit messages

Use Conventional Commits messages. The commit type decides the next version:

- `feat` releases a minor version, `fix` and `perf` release a patch version.
- `!` after the type or a `BREAKING CHANGE:` footer releases a major version. Removing or renaming a skill is a breaking change.
- Other types, such as `docs`, `test`, `refactor`, and `chore`, do not trigger a release. Changes to shipped skill content are therefore `feat` or `fix`, never `docs` or `chore`.
- Dependency updates do not reach users, so Dependabot commits as `build(deps)`. Keep this prefix in `.github/dependabot.yml`.
- Pin third-party actions in workflows to a commit SHA with a version comment. Dependabot keeps both up to date.

## Feedback loops

- Regenerate the Claude adapter and run `dotnet test` after changing skills or manifests.
- Run `dotnet run --project tools/GuidedCoding.ClaudeGenerator -- --check` to detect drift.
- Run `claude plugin validate . --strict` when Claude Code is installed.
- Run `gh skill publish --dry-run` before publishing a release. The release tool runs it for you.

## This is your space

If you find something noteworthy while working in this repository, add it here for discussion.
