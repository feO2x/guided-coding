using System;
using System.Globalization;
using System.Linq;

namespace GuidedCoding.Release;

public static class Changelog
{
    public const string FileName = "CHANGELOG.md";
    public const string UnreleasedHeading = "## [Unreleased]";

    // Turns the Unreleased section into the released version and opens a new, empty Unreleased section.
    public static string Release(string content, SemanticVersion version, DateOnly date)
    {
        var lines = content.ReplaceLineEndings("\n").Split('\n');
        var headingIndex = Array.FindIndex(lines, line => line.TrimEnd() == UnreleasedHeading);
        if (headingIndex < 0)
        {
            throw new InvalidOperationException($"{FileName} has no '{UnreleasedHeading}' section.");
        }

        var hasEntries = lines
           .Skip(headingIndex + 1)
           .TakeWhile(line => !line.StartsWith("## ", StringComparison.Ordinal))
           .Any(line => !string.IsNullOrWhiteSpace(line) && !line.TrimStart().StartsWith('#'));
        if (!hasEntries)
        {
            throw new InvalidOperationException(
                $"{FileName} has no entries under '{UnreleasedHeading}'. Describe the release there first."
            );
        }

        var releasedHeading = $"## [{version}] - {date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";
        return string.Join(
            '\n',
            lines[..headingIndex].Concat([UnreleasedHeading, "", releasedHeading]).Concat(lines[(headingIndex + 1)..])
        );
    }

    // Returns the entries of a released version, which become the notes of its GitHub release.
    public static string Notes(string content, SemanticVersion version)
    {
        var lines = content.ReplaceLineEndings("\n").Split('\n');
        var heading = $"## [{version}]";
        var headingIndex = Array.FindIndex(lines, line => line.StartsWith(heading, StringComparison.Ordinal));
        if (headingIndex < 0)
        {
            throw new InvalidOperationException($"{FileName} has no '{heading}' section.");
        }

        var entries = lines
           .Skip(headingIndex + 1)
           .TakeWhile(line => !line.StartsWith("## ", StringComparison.Ordinal));
        return string.Join('\n', entries).Trim() + "\n";
    }
}
