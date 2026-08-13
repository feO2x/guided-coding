using System.Text.Json;

namespace GuidedCoding.Tests;

public sealed class PackageValidationTests
{
    private const string ExplicitInvocation = "Run only when explicitly requested by the user.";
    private const string PluginName = "guided-coding";
    private const string Version = "2.0.0";

    private static readonly string[] ExpectedSkillNames =
    [
        "guided-coding-finish-plan",
        "guided-coding-prepare-issue-for-plan",
        "guided-coding-review-plan",
        "guided-coding-setup",
        "guided-coding-write-deviations",
        "guided-coding-write-plan"
    ];

    private static readonly string[] ForbiddenFrontmatterFields =
    [
        "allowed-tools",
        "argument-hint",
        "compatibility",
        "disable-model-invocation",
        "license",
        "metadata"
    ];

    public static TheoryData<string> SkillNames => new (ExpectedSkillNames);

    private static string RepositoryRoot { get; } = FindRepositoryRoot();

    [Fact]
    public void ManifestsUseTheSameIdentityAndVersion()
    {
        var portable = ReadJson("plugin.json");
        var claude = ReadJson(".claude-plugin/plugin.json");

        Assert.Equal(
            "https://agent-plugins.org/schemas/1.0.0/plugin.schema.json",
            portable.GetProperty("$schema").GetString()
        );

        Assert.Equal(PluginName, portable.GetProperty("name").GetString());
        Assert.Equal(Version, portable.GetProperty("version").GetString());
        Assert.Equal(PluginName, claude.GetProperty("name").GetString());
        Assert.Equal(Version, claude.GetProperty("version").GetString());
    }

    [Fact]
    public void MarketplacePublishesTheRootPlugin()
    {
        var marketplace = ReadJson(".claude-plugin/marketplace.json");
        var plugins = marketplace.GetProperty("plugins").EnumerateArray().ToArray();
        var plugin = Assert.Single(plugins);

        Assert.Equal(PluginName, marketplace.GetProperty("name").GetString());
        Assert.Equal(PluginName, plugin.GetProperty("name").GetString());
        Assert.Equal(".", plugin.GetProperty("source").GetString());
        Assert.Equal(Version, plugin.GetProperty("version").GetString());
    }

    [Fact]
    public void ExpectedSkillsArePresent()
    {
        var actualSkillNames = Directory
           .EnumerateFiles(Path.Combine(RepositoryRoot, "skills"), "SKILL.md", SearchOption.AllDirectories)
           .Where(path => Directory.GetParent(path)?.Parent?.FullName == Path.Combine(RepositoryRoot, "skills"))
           .Select(path => Directory.GetParent(path)!.Name)
           .Order(StringComparer.Ordinal)
           .ToArray();

        Assert.Equal(ExpectedSkillNames, actualSkillNames);
    }

    [Theory]
    [MemberData(nameof(SkillNames))]
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
    [MemberData(nameof(SkillNames))]
    public void CodexMetadataRequiresExplicitInvocation(string skillName)
    {
        var metadataPath = Path.Combine(RepositoryRoot, "skills", skillName, "agents", "openai.yaml");
        var metadata = File.ReadAllText(metadataPath);

        Assert.Contains($"${skillName}", metadata, StringComparison.Ordinal);
        Assert.Contains("allow_implicit_invocation: false", metadata, StringComparison.Ordinal);
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
}
