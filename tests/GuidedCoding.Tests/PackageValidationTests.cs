using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace GuidedCoding.Tests;

public sealed class PackageValidationTests
{
    private const string ExplicitInvocation = "Run only when explicitly requested by the user.";
    private const string PluginName = "guided-coding";
    private const string Version = "2.0.0";

    private static readonly string[] ExpectedPortableSkillNames =
    [
        "guided-coding-finish-plan",
        "guided-coding-prepare-issue-for-plan",
        "guided-coding-review-plan",
        "guided-coding-setup",
        "guided-coding-write-deviations",
        "guided-coding-write-plan"
    ];

    private static readonly Dictionary<string, string> ExpectedClaudeSkillNames = new(
        StringComparer.Ordinal
    )
    {
        ["guided-coding-finish-plan"] = "finish-plan",
        ["guided-coding-prepare-issue-for-plan"] = "prepare-issue-for-plan",
        ["guided-coding-review-plan"] = "review-plan",
        ["guided-coding-setup"] = "setup",
        ["guided-coding-write-deviations"] = "write-deviations",
        ["guided-coding-write-plan"] = "write-plan"
    };

    private static readonly string[] ForbiddenFrontmatterFields =
    [
        "allowed-tools",
        "argument-hint",
        "compatibility",
        "disable-model-invocation",
        "license",
        "metadata"
    ];

    public static TheoryData<string> PortableSkillNames => new(ExpectedPortableSkillNames);

    public static TheoryData<string, string> PortableAndClaudeSkillNames => new(
        ExpectedClaudeSkillNames.Select(pair => (pair.Key, pair.Value))
    );

    private static string RepositoryRoot { get; } = FindRepositoryRoot();

    [Fact]
    public void ManifestsUseTheSameIdentityAndVersion()
    {
        var portable = ReadJson("plugin.json");
        var claude = ReadJson("claude-plugin/.claude-plugin/plugin.json");

        Assert.Equal(
            "https://agent-plugins.org/schemas/1.0.0/plugin.schema.json",
            portable.GetProperty("$schema").GetString()
        );

        Assert.Equal(PluginName, portable.GetProperty("name").GetString());
        Assert.Equal(Version, portable.GetProperty("version").GetString());
        Assert.Equal(PluginName, claude.GetProperty("name").GetString());
        Assert.Equal(Version, claude.GetProperty("version").GetString());
        Assert.Equal("./claude-skills", claude.GetProperty("skills").GetString());
    }

    [Fact]
    public void MarketplacePublishesTheClaudeAdapter()
    {
        var marketplace = ReadJson(".claude-plugin/marketplace.json");
        var plugins = marketplace.GetProperty("plugins").EnumerateArray().ToArray();
        var plugin = Assert.Single(plugins);

        Assert.Equal(PluginName, marketplace.GetProperty("name").GetString());
        Assert.Equal(PluginName, plugin.GetProperty("name").GetString());
        Assert.Equal("./claude-plugin", plugin.GetProperty("source").GetString());
        Assert.Equal(Version, plugin.GetProperty("version").GetString());
    }

    [Fact]
    public void ExpectedPortableSkillsArePresent()
    {
        var actualSkillNames = Directory
           .EnumerateFiles(Path.Combine(RepositoryRoot, "skills"), "SKILL.md", SearchOption.AllDirectories)
           .Where(path => Directory.GetParent(path)?.Parent?.FullName == Path.Combine(RepositoryRoot, "skills"))
           .Select(path => Directory.GetParent(path)!.Name)
           .Order(StringComparer.Ordinal)
           .ToArray();

        Assert.Equal(ExpectedPortableSkillNames, actualSkillNames);
    }

    [Fact]
    public void ExpectedClaudeSkillsArePresent()
    {
        var actualSkillNames = Directory
           .EnumerateFiles(
                Path.Combine(RepositoryRoot, "claude-plugin", "claude-skills"),
                "SKILL.md",
                SearchOption.AllDirectories
            )
           .Where(
                path =>
                    Directory.GetParent(path)?.Parent?.FullName ==
                    Path.Combine(RepositoryRoot, "claude-plugin", "claude-skills")
            )
           .Select(path => Directory.GetParent(path)!.Name)
           .Order(StringComparer.Ordinal)
           .ToArray();

        Assert.Equal(ExpectedClaudeSkillNames.Values.Order(StringComparer.Ordinal), actualSkillNames);
    }

    [Theory]
    [MemberData(nameof(PortableSkillNames))]
    public void SkillIsPortableAndRequiresExplicitInvocation(string skillName)
    {
        var skillPath = Path.Combine(RepositoryRoot, "skills", skillName, "SKILL.md");
        var skill = File.ReadAllText(skillPath);
        var frontmatter = ParseFrontmatter(skillPath, skill);

        Assert.Equal(["description", "name"], frontmatter.Keys.Order(StringComparer.Ordinal));
        Assert.Equal(skillName, frontmatter["name"]);
        Assert.Contains(ExplicitInvocation, frontmatter["description"], StringComparison.Ordinal);

        foreach (var field in ForbiddenFrontmatterFields)
        {
            Assert.DoesNotContain(field, frontmatter.Keys);
        }

        Assert.DoesNotContain("$ARGUMENTS", skill, StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(PortableSkillNames))]
    public void CodexMetadataRequiresExplicitInvocation(string skillName)
    {
        var metadataPath = Path.Combine(RepositoryRoot, "skills", skillName, "agents", "openai.yaml");
        var metadata = File.ReadAllText(metadataPath);

        Assert.Contains($"${skillName}", metadata, StringComparison.Ordinal);
        Assert.Contains("allow_implicit_invocation: false", metadata, StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(PortableAndClaudeSkillNames))]
    public void ClaudeSkillIsGeneratedFromPortableSkill(string portableName, string claudeName)
    {
        var portablePath = Path.Combine(RepositoryRoot, "skills", portableName, "SKILL.md");
        var claudePath = Path.Combine(
            RepositoryRoot,
            "claude-plugin",
            "claude-skills",
            claudeName,
            "SKILL.md"
        );
        var portable = File.ReadAllText(portablePath);
        var claude = File.ReadAllText(claudePath);
        var portableFrontmatter = ParseFrontmatter(portablePath, portable);
        var claudeFrontmatter = ParseFrontmatter(claudePath, claude);

        Assert.Contains("description", claudeFrontmatter.Keys);
        Assert.Contains("disable-model-invocation", claudeFrontmatter.Keys);
        Assert.Contains("name", claudeFrontmatter.Keys);
        Assert.All(
            claudeFrontmatter.Keys,
            field =>
                Assert.Contains(
                    field,
                    new[] { "argument-hint", "description", "disable-model-invocation", "name" }
                )
        );
        Assert.Equal(claudeName, claudeFrontmatter["name"]);
        Assert.Equal("true", claudeFrontmatter["disable-model-invocation"]);
        Assert.Equal(
            portableFrontmatter["description"],
            JsonSerializer.Deserialize<string>(claudeFrontmatter["description"])
        );
        Assert.Equal(ParseBody(portable), ParseBody(claude));
        Assert.False(
            Directory.Exists(
                Path.Combine(
                    RepositoryRoot,
                    "claude-plugin",
                    "claude-skills",
                    claudeName,
                    "agents"
                )
            )
        );
    }

    [Theory]
    [MemberData(nameof(PortableAndClaudeSkillNames))]
    public void ClaudeSkillResourcesMatchPortableSkill(string portableName, string claudeName)
    {
        var portableRoot = Path.Combine(RepositoryRoot, "skills", portableName);
        var claudeRoot = Path.Combine(
            RepositoryRoot,
            "claude-plugin",
            "claude-skills",
            claudeName
        );
        var portableResources = EnumerateResourceFiles(portableRoot, excludeAgents: true);
        var claudeResources = EnumerateResourceFiles(claudeRoot, excludeAgents: false);

        Assert.Equal(portableResources.Keys, claudeResources.Keys);
        foreach (var path in portableResources.Keys)
        {
            Assert.Equal(
                File.ReadAllBytes(portableResources[path]),
                File.ReadAllBytes(claudeResources[path])
            );
        }
    }

    [Fact]
    public void ClaudePluginContainsTheRepositoryLicense()
    {
        Assert.Equal(
            File.ReadAllBytes(Path.Combine(RepositoryRoot, "LICENSE")),
            File.ReadAllBytes(Path.Combine(RepositoryRoot, "claude-plugin", "LICENSE"))
        );
    }

    private static string FindRepositoryRoot()
    {
        for (
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            directory is not null;
            directory = directory.Parent
        )
        {
            if (
                File.Exists(Path.Combine(directory.FullName, "plugin.json")) &&
                Directory.Exists(Path.Combine(directory.FullName, "skills"))
            )
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException("Could not locate the Guided Coding repository root.");
    }

    private static JsonElement ReadJson(string relativePath)
    {
        var path = Path.Combine(RepositoryRoot, relativePath);
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return document.RootElement.Clone();
    }

    private static Dictionary<string, string> ParseFrontmatter(string path, string content)
    {
        var lines = content.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        Assert.NotEmpty(lines);
        Assert.Equal("---", lines[0]);

        var end = Array.IndexOf(lines, "---", 1);
        Assert.True(end > 1, $"{path} has no closing frontmatter delimiter.");

        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var line in lines[1..end])
        {
            var separator = line.IndexOf(':', StringComparison.Ordinal);
            Assert.True(separator > 0, $"{path} has an unsupported frontmatter line: {line}");

            var name = line[..separator];
            var value = line[(separator + 1)..].Trim();
            Assert.NotEmpty(value);
            Assert.True(values.TryAdd(name, value), $"{path} repeats frontmatter field {name}.");
        }

        return values;
    }

    private static string ParseBody(string content)
    {
        var lines = content.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        var end = Array.IndexOf(lines, "---", 1);
        Assert.True(end > 1);
        return string.Join('\n', lines[(end + 1)..]);
    }

    private static SortedDictionary<string, string> EnumerateResourceFiles(
        string root,
        bool excludeAgents
    )
    {
        return new(
            Directory
               .EnumerateFiles(root, "*", SearchOption.AllDirectories)
               .Where(path => Path.GetFileName(path) != "SKILL.md")
               .Where(
                    path =>
                        !excludeAgents ||
                        !Path.GetRelativePath(root, path)
                           .Split(Path.DirectorySeparatorChar)
                           .Contains("agents", StringComparer.Ordinal)
                )
               .ToDictionary(path => Path.GetRelativePath(root, path), StringComparer.Ordinal),
            StringComparer.Ordinal
        );
    }
}
