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

    private static readonly Dictionary<string, string> ExpectedClaudeSkillNames = new (
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
        "metadata"
    ];

    public static TheoryData<string> PortableSkillNames => new (ExpectedPortableSkillNames);

    public static TheoryData<string, string> PortableAndClaudeSkillNames => new (
        ExpectedClaudeSkillNames.Select(pair => (pair.Key, pair.Value))
    );

    private static string RepositoryRoot { get; } = FindRepositoryRoot();

    [Fact]
    public void ManifestsUseTheSamePackageMetadata()
    {
        var portable = ReadJson("plugin.json");
        var claude = ReadJson("claude-plugin/.claude-plugin/plugin.json");

        Assert.Equal(
            "https://agent-plugins.org/schemas/1.0.0/plugin.schema.json",
            portable.GetProperty("$schema").GetString()
        );

        AssertPluginMetadataEqual(portable, claude);
        Assert.Equal(PluginName, portable.GetProperty("name").GetString());
        Assert.Equal(Version, portable.GetProperty("version").GetString());
        Assert.Equal(
            ReadStringArray(portable, "keywords"),
            ReadStringArray(claude, "keywords")
        );
        Assert.Equal("./claude-skills", claude.GetProperty("skills").GetString());
    }

    [Fact]
    public void MarketplacePublishesTheClaudeAdapter()
    {
        var marketplace = ReadJson(".claude-plugin/marketplace.json");
        var plugins = marketplace.GetProperty("plugins").EnumerateArray().ToArray();
        var plugin = Assert.Single(plugins);
        var portable = ReadJson("plugin.json");

        Assert.Equal(portable.GetProperty("name").GetString(), marketplace.GetProperty("name").GetString());
        Assert.Equal(
            portable.GetProperty("description").GetString(),
            marketplace.GetProperty("description").GetString()
        );
        AssertAuthorEqual(portable.GetProperty("author"), marketplace.GetProperty("owner"));
        AssertPluginMetadataEqual(portable, plugin);
        Assert.Equal(ReadStringArray(portable, "keywords"), ReadStringArray(plugin, "tags"));
        Assert.Equal("./claude-plugin", plugin.GetProperty("source").GetString());
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

        Assert.Equal(["description", "license", "name"], frontmatter.Keys.Order(StringComparer.Ordinal));
        Assert.Equal(skillName, frontmatter["name"]);
        Assert.Contains(ExplicitInvocation, frontmatter["description"], StringComparison.Ordinal);
        Assert.NotEqual(
            ReadJson("plugin.json").GetProperty("description").GetString(),
            frontmatter["description"]
        );

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

        Assert.Contains("display_name:", metadata, StringComparison.Ordinal);
        Assert.Contains("short_description:", metadata, StringComparison.Ordinal);
        Assert.Contains("default_prompt:", metadata, StringComparison.Ordinal);
        Assert.Contains($"${skillName}", metadata, StringComparison.Ordinal);
        Assert.Contains("allow_implicit_invocation: false", metadata, StringComparison.Ordinal);
        Assert.DoesNotContain(
            ReadJson("plugin.json").GetProperty("description").GetString()!,
            metadata,
            StringComparison.Ordinal
        );
    }

    [Fact]
    public void SkillDescriptionsRemainSpecificToEachWorkflow()
    {
        var packageDescription = ReadJson("plugin.json").GetProperty("description").GetString();
        var portableDescriptions = ExpectedPortableSkillNames
           .Select(
                skillName =>
                {
                    var path = Path.Combine(RepositoryRoot, "skills", skillName, "SKILL.md");
                    return ParseFrontmatter(path, File.ReadAllText(path))["description"];
                }
            )
           .ToArray();
        var codexDescriptions = ExpectedPortableSkillNames
           .Select(
                skillName =>
                    ReadQuotedYamlValue(
                        Path.Combine(
                            RepositoryRoot,
                            "skills",
                            skillName,
                            "agents",
                            "openai.yaml"
                        ),
                        "short_description"
                    )
            )
           .ToArray();

        Assert.Equal(
            ExpectedPortableSkillNames.Length,
            portableDescriptions.Distinct(StringComparer.Ordinal).Count()
        );
        Assert.Equal(
            ExpectedPortableSkillNames.Length,
            codexDescriptions.Distinct(StringComparer.Ordinal).Count()
        );
        Assert.DoesNotContain(packageDescription, portableDescriptions);
        Assert.DoesNotContain(packageDescription, codexDescriptions);
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
                    new[] { "argument-hint", "description", "disable-model-invocation", "name", "license" }
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

    private static void AssertPluginMetadataEqual(JsonElement expected, JsonElement actual)
    {
        foreach (
            var property in new[]
            {
                "name",
                "version",
                "description",
                "homepage",
                "repository",
                "license"
            }
        )
        {
            Assert.Equal(
                expected.GetProperty(property).GetString(),
                actual.GetProperty(property).GetString()
            );
        }

        AssertAuthorEqual(expected.GetProperty("author"), actual.GetProperty("author"));
    }

    private static void AssertAuthorEqual(JsonElement expected, JsonElement actual)
    {
        Assert.Equal(expected.GetProperty("name").GetString(), actual.GetProperty("name").GetString());
        Assert.Equal(expected.GetProperty("url").GetString(), actual.GetProperty("url").GetString());
    }

    private static string[] ReadStringArray(JsonElement element, string property)
    {
        return element
           .GetProperty(property)
           .EnumerateArray()
           .Select(item => item.GetString())
           .Cast<string>()
           .ToArray();
    }

    private static string ReadQuotedYamlValue(string path, string property)
    {
        var prefix = $"  {property}: ";
        var line = Assert.Single(
            File.ReadLines(path),
            candidate => candidate.StartsWith(prefix, StringComparison.Ordinal)
        );
        return JsonSerializer.Deserialize<string>(line[prefix.Length..])!;
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
        return new (
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
