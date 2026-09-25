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
        "guided-coding-freeze-plan",
        "guided-coding-implement",
        "guided-coding-implement-coach-me",
        "guided-coding-implement-show-me",
        "guided-coding-review-plan",
        "guided-coding-setup",
        "guided-coding-write-deviations",
        "guided-coding-write-plan"
    ];

    private static readonly Dictionary<string, string> ExpectedClaudeSkillNames = new (
        StringComparer.Ordinal
    )
    {
        ["guided-coding-freeze-plan"] = "freeze-plan",
        ["guided-coding-implement"] = "implement",
        ["guided-coding-implement-coach-me"] = "implement-coach-me",
        ["guided-coding-implement-show-me"] = "implement-show-me",
        ["guided-coding-review-plan"] = "review-plan",
        ["guided-coding-setup"] = "setup",
        ["guided-coding-write-deviations"] = "write-deviations",
        ["guided-coding-write-plan"] = "write-plan"
    };

    private static readonly string[] GuidedLearningSkillNames =
    [
        "guided-coding-implement-coach-me",
        "guided-coding-implement-show-me"
    ];

    // The Guided Learning skills share every section except these, which differ on purpose.
    private static readonly string[] GuidedLearningSectionsThatDiffer =
    [
        "Create the Milestone Roadmap",
        "How to Work Through a Single Milestone",
        "Reveal Help Progressively",
        "Update the Learning Profile"
    ];

    private static readonly (string Section, string[] SkillNames)[] SharedSections =
    [
        ("Establish the Target", ["guided-coding-implement", .. GuidedLearningSkillNames])
    ];

    // A snippet is the list that follows its lead-in line.
    private static readonly (string LeadIn, string[] SkillNames)[] SharedSnippets =
    [
        (
            "Use these commands to get it:",
            ["guided-coding-freeze-plan", "guided-coding-write-deviations"]
        )
    ];

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

    public static TheoryData<string> SharedSectionNames => new (
        SharedSections.Select(shared => shared.Section)
    );

    public static TheoryData<string> SharedSnippetLeadIns => new (
        SharedSnippets.Select(shared => shared.LeadIn)
    );

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

    [Theory]
    [MemberData(nameof(SharedSectionNames))]
    public void SkillsSharingASectionKeepItIdentical(string sectionName)
    {
        var skillNames = SharedSections.Single(shared => shared.Section == sectionName).SkillNames;
        var sections = skillNames
           .Select(
                skillName => (
                    SkillName: skillName,
                    Body: ReadSectionBody(
                        Path.Combine(RepositoryRoot, "skills", skillName, "SKILL.md"),
                        sectionName
                    )
                )
            )
           .ToArray();
        var reference = sections[0];

        foreach (var section in sections[1..])
        {
            Assert.True(
                string.Equals(reference.Body, section.Body, StringComparison.Ordinal),
                $"\"{sectionName}\" differs between {reference.SkillName} and {section.SkillName}. " +
                "The section is duplicated on purpose because skills are standalone; " +
                "apply the change to every skill that shares it."
            );
        }
    }

    [Fact]
    public void GuidedLearningSkillsShareAllUndeclaredSections()
    {
        var skills = GuidedLearningSkillNames
           .Select(
                skillName => (
                    SkillName: skillName,
                    Path: Path.Combine(RepositoryRoot, "skills", skillName, "SKILL.md")
                )
            )
           .Select(
                skill => (
                    skill.SkillName,
                    skill.Path,
                    Headings: ReadSectionHeadings(skill.Path)
                       .Where(heading => !GuidedLearningSectionsThatDiffer.Contains(heading))
                       .ToArray()
                )
            )
           .ToArray();
        var reference = skills[0];

        foreach (var skill in skills[1..])
        {
            var unmatched = reference
               .Headings
               .Except(skill.Headings)
               .Concat(skill.Headings.Except(reference.Headings))
               .ToArray();
            Assert.True(
                reference.Headings.SequenceEqual(skill.Headings),
                $"{reference.SkillName} and {skill.SkillName} have different sections " +
                (unmatched.Length > 0 ?
                    $"(unmatched: {string.Join(", ", unmatched.Select(heading => $"\"{heading}\""))}). " :
                    "(same sections in a different order). ") +
                "Guided Learning skills share every section by default; add the section to the other " +
                $"skill or declare it in {nameof(GuidedLearningSectionsThatDiffer)}."
            );

            foreach (var heading in reference.Headings)
            {
                Assert.True(
                    string.Equals(
                        ReadSectionBody(reference.Path, heading),
                        ReadSectionBody(skill.Path, heading),
                        StringComparison.Ordinal
                    ),
                    $"\"{heading}\" differs between {reference.SkillName} and {skill.SkillName}. " +
                    "Apply the change to both skills, or declare the section in " +
                    $"{nameof(GuidedLearningSectionsThatDiffer)} if it should differ."
                );
            }
        }
    }

    [Fact]
    public void GuidedLearningSectionsThatDifferExist()
    {
        var headings = GuidedLearningSkillNames
           .SelectMany(
                skillName => ReadSectionHeadings(
                    Path.Combine(RepositoryRoot, "skills", skillName, "SKILL.md")
                )
            )
           .ToHashSet(StringComparer.Ordinal);

        foreach (var heading in GuidedLearningSectionsThatDiffer)
        {
            Assert.True(
                headings.Contains(heading),
                $"\"{heading}\" is declared in {nameof(GuidedLearningSectionsThatDiffer)} " +
                "but no Guided Learning skill has this section anymore."
            );
        }
    }

    [Theory]
    [MemberData(nameof(SharedSnippetLeadIns))]
    public void SkillsSharingASnippetKeepItIdentical(string leadIn)
    {
        var skillNames = SharedSnippets.Single(shared => shared.LeadIn == leadIn).SkillNames;
        var snippets = skillNames
           .Select(
                skillName => (
                    SkillName: skillName,
                    Lines: ReadSnippet(
                        Path.Combine(RepositoryRoot, "skills", skillName, "SKILL.md"),
                        leadIn
                    )
                )
            )
           .ToArray();
        var reference = snippets[0];

        foreach (var snippet in snippets[1..])
        {
            Assert.True(
                reference.Lines.SequenceEqual(snippet.Lines, StringComparer.Ordinal),
                $"The list after \"{leadIn}\" differs between {reference.SkillName} and " +
                $"{snippet.SkillName}. The snippet is duplicated on purpose because skills are " +
                "standalone; apply the change to every skill that shares it."
            );
        }
    }

    [Fact]
    public void GuidedLearningSkillsShipTheSameProfileTemplate()
    {
        var templates = GuidedLearningSkillNames
           .Select(
                skillName => (
                    SkillName: skillName,
                    Content: File.ReadAllText(
                        Path.Combine(RepositoryRoot, "skills", skillName, "assets", "profile.md")
                    )
                )
            )
           .ToArray();
        var reference = templates[0];

        foreach (var template in templates[1..])
        {
            Assert.True(
                string.Equals(reference.Content, template.Content, StringComparison.Ordinal),
                $"assets/profile.md differs between {reference.SkillName} and {template.SkillName}. " +
                "The template is duplicated on purpose because skills are standalone; " +
                "apply the change to every skill that ships it."
            );
        }
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

    private static string ReadSectionBody(string path, string heading)
    {
        var lines = File.ReadAllText(path).Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        var start = Array.FindIndex(lines, line => IsSectionHeading(line, heading));
        Assert.True(start >= 0, $"\"{heading}\" is missing from {path}.");

        var end = Array.FindIndex(
            lines,
            start + 1,
            line => line.StartsWith("## ", StringComparison.Ordinal)
        );
        var body = end < 0 ? lines[(start + 1)..] : lines[(start + 1)..end];
        return string.Join('\n', body).Trim();
    }

    private static bool IsSectionHeading(string line, string heading)
    {
        return string.Equals(ReadHeadingText(line), heading, StringComparison.Ordinal);
    }

    private static string[] ReadSectionHeadings(string path)
    {
        return File
           .ReadAllText(path)
           .Replace("\r\n", "\n", StringComparison.Ordinal)
           .Split('\n')
           .Select(ReadHeadingText)
           .OfType<string>()
           .ToArray();
    }

    private static string? ReadHeadingText(string line)
    {
        if (!line.StartsWith("## ", StringComparison.Ordinal))
        {
            return null;
        }

        // Section numbers differ between skills, so match on the heading text alone.
        var text = line[3..].Trim();
        var separator = text.IndexOf(". ", StringComparison.Ordinal);
        if (separator > 0 && text[..separator].All(char.IsDigit))
        {
            text = text[(separator + 2)..];
        }

        return text;
    }

    private static string[] ReadSnippet(string path, string leadIn)
    {
        var lines = File.ReadAllText(path).Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        var matches = lines
           .Select((line, index) => (Line: line, Index: index))
           .Where(candidate => candidate.Line.TrimEnd().EndsWith(leadIn, StringComparison.Ordinal))
           .ToArray();
        Assert.True(matches.Length == 1, $"{path} must contain \"{leadIn}\" exactly once.");

        var snippet = lines
           .Skip(matches[0].Index + 1)
           .SkipWhile(string.IsNullOrWhiteSpace)
           .TakeWhile(line => line.StartsWith("- ", StringComparison.Ordinal))
           .ToArray();
        Assert.True(snippet.Length > 0, $"No list follows \"{leadIn}\" in {path}.");
        return snippet;
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
