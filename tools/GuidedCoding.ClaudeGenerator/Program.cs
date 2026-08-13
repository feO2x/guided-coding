using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace GuidedCoding.ClaudeGenerator;

public static class Program
{
    private const string ConfigPath = "tools/GuidedCoding.ClaudeGenerator/claude-skills.json";
    private const string GeneratedSkillsPath = "claude-skills";
    private const string OutputPath = "claude-plugin";
    private const string SourceSkillsPath = "skills";

    private static readonly JsonSerializerOptions ConfigJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly JsonSerializerOptions YamlStringOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static int Main(string[] args)
    {
        try
        {
            var check = ParseArguments(args);
            var repositoryRoot = FindRepositoryRoot();
            var temporaryRoot = Path.Combine(
                Path.GetTempPath(),
                $"guided-coding-claude-{Guid.NewGuid():N}"
            );

            try
            {
                BuildAdapter(repositoryRoot, temporaryRoot);

                if (check)
                {
                    var differences = FindDifferences(repositoryRoot, temporaryRoot);
                    if (differences.Count == 0)
                    {
                        Console.WriteLine("Claude plugin adapter is up to date.");
                        return 0;
                    }

                    Console.Error.WriteLine("Claude plugin adapter is out of date:");
                    foreach (var difference in differences)
                    {
                        Console.Error.WriteLine($"  {difference}");
                    }

                    Console.Error.WriteLine(
                        "Run 'dotnet run --project tools/GuidedCoding.ClaudeGenerator' to regenerate it."
                    );
                    return 1;
                }

                WriteAdapter(repositoryRoot, temporaryRoot);
                Console.WriteLine($"Generated Claude plugin adapter at {OutputPath}.");
                return 0;
            }
            finally
            {
                if (Directory.Exists(temporaryRoot))
                {
                    Directory.Delete(temporaryRoot, recursive: true);
                }
            }
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }

    private static bool ParseArguments(string[] args)
    {
        if (args.Length == 0)
        {
            return false;
        }

        if (args is ["--check"])
        {
            return true;
        }

        throw new InvalidOperationException("Usage: GuidedCoding.ClaudeGenerator [--check]");
    }

    private static string FindRepositoryRoot()
    {
        foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            for (
                var directory = new DirectoryInfo(start);
                directory is not null;
                directory = directory.Parent
            )
            {
                if (
                    File.Exists(Path.Combine(directory.FullName, "plugin.json")) &&
                    Directory.Exists(Path.Combine(directory.FullName, SourceSkillsPath)) &&
                    File.Exists(Path.Combine(directory.FullName, ConfigPath))
                )
                {
                    return directory.FullName;
                }
            }
        }

        throw new InvalidOperationException("Could not locate the Guided Coding repository root.");
    }

    private static void BuildAdapter(string repositoryRoot, string temporaryRoot)
    {
        var config = ReadConfig(repositoryRoot);
        var sourceSkillsRoot = Path.Combine(repositoryRoot, SourceSkillsPath);
        var actualSkillNames = Directory
           .EnumerateDirectories(sourceSkillsRoot)
           .Where(path => File.Exists(Path.Combine(path, "SKILL.md")))
           .Select(Path.GetFileName)
           .Order(StringComparer.Ordinal)
           .ToArray();
        var configuredSkillNames = config.Skills.Keys.Order(StringComparer.Ordinal).ToArray();

        if (!actualSkillNames.SequenceEqual(configuredSkillNames, StringComparer.Ordinal))
        {
            throw new InvalidOperationException(
                "claude-skills.json must contain exactly one entry for every canonical skill."
            );
        }

        var duplicateClaudeName = config
           .Skills
           .GroupBy(pair => pair.Value.Name, StringComparer.Ordinal)
           .FirstOrDefault(group => group.Count() > 1);
        if (duplicateClaudeName is not null)
        {
            throw new InvalidOperationException(
                $"Claude skill name '{duplicateClaudeName.Key}' is configured more than once."
            );
        }

        var generatedSkillsRoot = Path.Combine(temporaryRoot, GeneratedSkillsPath);
        Directory.CreateDirectory(generatedSkillsRoot);

        foreach (var (canonicalName, claudeSkill) in config.Skills.OrderBy(pair => pair.Key))
        {
            ValidateSkillName(claudeSkill.Name, canonicalName);

            var sourceSkillRoot = Path.Combine(sourceSkillsRoot, canonicalName);
            var sourceSkillPath = Path.Combine(sourceSkillRoot, "SKILL.md");
            var sourceSkill = ParseCanonicalSkill(sourceSkillPath);
            if (!string.Equals(sourceSkill.Name, canonicalName, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Canonical skill '{canonicalName}' has frontmatter name '{sourceSkill.Name}'."
                );
            }

            var generatedSkillRoot = Path.Combine(generatedSkillsRoot, claudeSkill.Name);
            Directory.CreateDirectory(generatedSkillRoot);
            File.WriteAllText(
                Path.Combine(generatedSkillRoot, "SKILL.md"),
                RenderClaudeSkill(sourceSkill, claudeSkill)
            );
            CopySkillResources(sourceSkillRoot, generatedSkillRoot);
        }

        File.Copy(
            Path.Combine(repositoryRoot, "LICENSE"),
            Path.Combine(temporaryRoot, "LICENSE")
        );
    }

    private static ClaudeGeneratorConfig ReadConfig(string repositoryRoot)
    {
        var path = Path.Combine(repositoryRoot, ConfigPath);
        var config = JsonSerializer.Deserialize<ClaudeGeneratorConfig>(
            File.ReadAllText(path),
            ConfigJsonOptions
        );

        if (config is null || config.Skills.Count == 0)
        {
            throw new InvalidOperationException($"{ConfigPath} does not define any skills.");
        }

        return config;
    }

    private static CanonicalSkill ParseCanonicalSkill(string path)
    {
        var content = NormalizeLineEndings(File.ReadAllText(path));
        if (!content.StartsWith("---\n", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"{path} has no YAML frontmatter.");
        }

        var closingDelimiter = content.IndexOf("\n---\n", 4, StringComparison.Ordinal);
        if (closingDelimiter < 0)
        {
            throw new InvalidOperationException($"{path} has no closing frontmatter delimiter.");
        }

        var frontmatter = content[4..closingDelimiter];
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var line in frontmatter.Split('\n'))
        {
            var separator = line.IndexOf(':', StringComparison.Ordinal);
            if (separator <= 0)
            {
                throw new InvalidOperationException(
                    $"{path} has an unsupported frontmatter line: {line}"
                );
            }

            var name = line[..separator];
            var value = line[(separator + 1)..].Trim();
            if (value.Length == 0 || !values.TryAdd(name, value))
            {
                throw new InvalidOperationException(
                    $"{path} has invalid frontmatter field '{name}'."
                );
            }
        }

        if (!values.Keys.Order(StringComparer.Ordinal).SequenceEqual(["description", "license", "name"]))
        {
            throw new InvalidOperationException(
                $"{path} must contain only name, description, and license frontmatter fields."
            );
        }

        return new(
            values["name"],
            values["description"],
            values["license"],
            content[(closingDelimiter + "\n---\n".Length)..]
        );
    }

    private static string RenderClaudeSkill(CanonicalSkill source, ClaudeSkillConfig target)
    {
        var frontmatter = new List<string>
        {
            "---",
            $"name: {target.Name}",
            $"description: {QuoteYamlString(source.Description)}",
            $"license: {QuoteYamlString(source.License)}"
        };

        if (!string.IsNullOrWhiteSpace(target.ArgumentHint))
        {
            frontmatter.Add($"argument-hint: {QuoteYamlString(target.ArgumentHint)}");
        }

        frontmatter.Add(
            $"disable-model-invocation: {target.DisableModelInvocation.ToString().ToLowerInvariant()}"
        );
        frontmatter.Add("---");

        return string.Join('\n', frontmatter) + "\n" + source.Body;
    }

    private static string QuoteYamlString(string value) =>
        JsonSerializer.Serialize(value, YamlStringOptions);

    private static void ValidateSkillName(string name, string canonicalName)
    {
        if (
            name.Length is < 1 or > 64 ||
            name[0] == '-' ||
            name[^1] == '-' ||
            name.Contains("--", StringComparison.Ordinal) ||
            name.Any(
                character =>
                    character is not (>= 'a' and <= 'z') and
                                 not (>= '0' and <= '9') and
                                 not '-'
            )
        )
        {
            throw new InvalidOperationException(
                $"Claude skill name '{name}' for '{canonicalName}' is not valid kebab-case."
            );
        }
    }

    private static void CopySkillResources(string sourceRoot, string destinationRoot)
    {
        foreach (var entry in Directory.EnumerateFileSystemEntries(sourceRoot))
        {
            var name = Path.GetFileName(entry);
            if (name is "SKILL.md" or "agents")
            {
                continue;
            }

            EnsureNotSymlink(entry);
            var destination = Path.Combine(destinationRoot, name);
            if (Directory.Exists(entry))
            {
                CopyDirectory(entry, destination);
            }
            else
            {
                File.Copy(entry, destination);
            }
        }
    }

    private static void CopyDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);
        foreach (var entry in Directory.EnumerateFileSystemEntries(source))
        {
            EnsureNotSymlink(entry);
            var target = Path.Combine(destination, Path.GetFileName(entry));
            if (Directory.Exists(entry))
            {
                CopyDirectory(entry, target);
            }
            else
            {
                File.Copy(entry, target);
            }
        }
    }

    private static void EnsureNotSymlink(string path)
    {
        if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
        {
            throw new InvalidOperationException($"Skill resource symlinks are not supported: {path}");
        }
    }

    private static List<string> FindDifferences(string repositoryRoot, string temporaryRoot)
    {
        var outputRoot = Path.Combine(repositoryRoot, OutputPath);
        var differences = CompareDirectories(
            Path.Combine(temporaryRoot, GeneratedSkillsPath),
            Path.Combine(outputRoot, GeneratedSkillsPath)
        );

        var expectedLicense = Path.Combine(temporaryRoot, "LICENSE");
        var actualLicense = Path.Combine(outputRoot, "LICENSE");
        if (!FilesEqual(expectedLicense, actualLicense))
        {
            differences.Add("LICENSE differs");
        }

        return differences;
    }

    private static List<string> CompareDirectories(string expectedRoot, string actualRoot)
    {
        if (!Directory.Exists(actualRoot))
        {
            return [$"{GeneratedSkillsPath}/ is missing"];
        }

        var expectedEntries = EnumerateRelativeEntries(expectedRoot);
        var actualEntries = EnumerateRelativeEntries(actualRoot);
        var differences = new List<string>();

        foreach (var missing in expectedEntries.Keys.Except(actualEntries.Keys, StringComparer.Ordinal))
        {
            differences.Add($"{GeneratedSkillsPath}/{missing} is missing");
        }

        foreach (var unexpected in actualEntries.Keys.Except(expectedEntries.Keys, StringComparer.Ordinal))
        {
            differences.Add($"{GeneratedSkillsPath}/{unexpected} is unexpected");
        }

        foreach (var path in expectedEntries.Keys.Intersect(actualEntries.Keys, StringComparer.Ordinal))
        {
            if (expectedEntries[path] != actualEntries[path])
            {
                differences.Add($"{GeneratedSkillsPath}/{path} has a different kind");
            }
            else if (
                expectedEntries[path] == EntryKind.File &&
                !FilesEqual(Path.Combine(expectedRoot, path), Path.Combine(actualRoot, path))
            )
            {
                differences.Add($"{GeneratedSkillsPath}/{path} differs");
            }
        }

        differences.Sort(StringComparer.Ordinal);
        return differences;
    }

    private static Dictionary<string, EntryKind> EnumerateRelativeEntries(string root)
    {
        var entries = new Dictionary<string, EntryKind>(StringComparer.Ordinal);
        foreach (var directory in Directory.EnumerateDirectories(root, "*", SearchOption.AllDirectories))
        {
            entries.Add(Path.GetRelativePath(root, directory), EntryKind.Directory);
        }

        foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
        {
            entries.Add(Path.GetRelativePath(root, file), EntryKind.File);
        }

        return entries;
    }

    private static bool FilesEqual(string expected, string actual)
    {
        if (!File.Exists(actual))
        {
            return false;
        }

        var expectedInfo = new FileInfo(expected);
        var actualInfo = new FileInfo(actual);
        return expectedInfo.Length == actualInfo.Length &&
               File.ReadAllBytes(expected).SequenceEqual(File.ReadAllBytes(actual));
    }

    private static void WriteAdapter(string repositoryRoot, string temporaryRoot)
    {
        var outputRoot = Path.Combine(repositoryRoot, OutputPath);
        Directory.CreateDirectory(outputRoot);

        var outputSkills = Path.Combine(outputRoot, GeneratedSkillsPath);
        var stagedSkills = Path.Combine(
            outputRoot,
            $".{GeneratedSkillsPath}.generated-{Guid.NewGuid():N}"
        );
        try
        {
            CopyDirectory(Path.Combine(temporaryRoot, GeneratedSkillsPath), stagedSkills);
            if (Directory.Exists(outputSkills))
            {
                Directory.Delete(outputSkills, recursive: true);
            }

            Directory.Move(stagedSkills, outputSkills);
        }
        finally
        {
            if (Directory.Exists(stagedSkills))
            {
                Directory.Delete(stagedSkills, recursive: true);
            }
        }

        File.Copy(
            Path.Combine(temporaryRoot, "LICENSE"),
            Path.Combine(outputRoot, "LICENSE"),
            overwrite: true
        );
    }

    private static string NormalizeLineEndings(string value) =>
        value.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');

    private enum EntryKind
    {
        Directory,
        File
    }

    private sealed record CanonicalSkill(string Name, string Description, string License, string Body);

    private sealed class ClaudeGeneratorConfig
    {
        public required Dictionary<string, ClaudeSkillConfig> Skills { get; init; }
    }

    private sealed class ClaudeSkillConfig
    {
        public required string Name { get; init; }

        public bool DisableModelInvocation { get; init; }

        public string? ArgumentHint { get; init; }
    }
}
