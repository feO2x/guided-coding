# Guided Coding

Guided Coding is a lightweight approach to working with Coding Agents. Version 2 treats `ai-plans/` as an
append-only decision record: plans become immutable when their Planning Phase ends, acceptance
criteria record verified progress, and material implementation changes are captured in Plan
Deviations documents.

The repository is both an [Agent Plugin](https://agent-plugins.org/) and a Claude Code plugin. Its
skills follow the [Agent Skills specification](https://agentskills.io/specification) and can also
be installed independently into `.agents/skills`.

The full method is documented at
[kenny-codes.net/docs/guided-coding](https://kenny-codes.net/docs/guided-coding/).

## Skills

| Skill | Purpose |
| --- | --- |
| `guided-coding-setup` | Set up or upgrade Guided Coding in a repository. |
| `guided-coding-write-plan` | After discussing an issue with your agent, write a plan or follow-up plan. |
| `guided-coding-review-plan` | Review a plan draft against the repository (use in fresh conversation). |
| `guided-coding-freeze-plan` | Freeze a plan by timestamping its file name and title. |
| `guided-coding-implement-and-learn-by-example` | Implement a frozen plan through worked code examples that you enter and discuss. |
| `guided-coding-implement-and-learn-by-solving` | Implement a frozen plan yourself through coached, verifiable milestones. |
| `guided-coding-write-deviations` | Summarize follow-up plans and record material implementation differences. |

All workflows require explicit user invocation.

## Install

### Agent Skills

Install all skills into the shared project-level `.agents/skills` directory with the [GitHub CLI](https://cli.github.com/):

```sh
gh skill install feO2x/guided-coding --all --agent universal --scope project
```

Install one skill by naming it, or add `--scope user` to make the installation available across
repositories. GitHub CLI's skill commands are currently in preview.

If you want to update, use the following command: 

```sh
gh skill update --dir .agents/skills --dry-run # checks for changes
gh skill update --dir .agents/skills --all # updates all local skills
```

### Claude Code marketplace

Add this repository as a marketplace and install the plugin:

```text
/plugin marketplace add feO2x/guided-coding
/plugin install guided-coding@guided-coding
```

Claude namespaces plugin skills with the plugin name. For example, invoke the setup workflow as
`/guided-coding:setup`. The other skill names similarly omit the redundant `guided-coding-`
prefix used by the portable Agent Skills.

If you want to update, use the following commands:

```sh
claude plugin marketplace update guided-coding
claude plugin update guided-coding@guided-coding --scope project
```

Restart Claude Code afterwards. Only then will the updated skills be picked up.

### Agent Plugin clients

Clients supporting the Agent Plugins standard can install this repository as a plugin package.
Installation and marketplace commands are client-specific.

## Development

Run the .NET 10 xUnit v3 validation suite with Microsoft.Testing.Platform v2:

```sh
dotnet test
```

Generate the Claude Code adapter from the canonical portable skills, or verify that the committed
adapter is current:

```sh
dotnet run --project tools/GuidedCoding.ClaudeGenerator
dotnet run --project tools/GuidedCoding.ClaudeGenerator -- --check
```

Configure Claude-specific skill names and frontmatter in
`tools/GuidedCoding.ClaudeGenerator/claude-skills.json`. Do not edit
`claude-plugin/claude-skills` directly.

When the corresponding tools are installed, also run:

```sh
claude plugin validate . --strict
gh skill publish --dry-run
```

Keep `plugin.json`, `claude-plugin/.claude-plugin/plugin.json`,
`.claude-plugin/marketplace.json`, and the release tag on the same Semantic Version.
