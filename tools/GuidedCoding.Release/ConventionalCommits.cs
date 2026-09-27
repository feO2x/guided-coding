using System.Linq;
using System.Text.RegularExpressions;

namespace GuidedCoding.Release;

public static partial class ConventionalCommits
{
    public static ReleaseType Classify(string message)
    {
        var lines = message.ReplaceLineEndings("\n").Split('\n');
        var subject = SubjectPattern().Match(lines[0]);
        if (!subject.Success)
        {
            return ReleaseType.None;
        }

        if (subject.Groups["breaking"].Success || lines.Skip(1).Any(BreakingChangeFooterPattern().IsMatch))
        {
            return ReleaseType.Major;
        }

        return subject.Groups["type"].Value.ToLowerInvariant() switch
        {
            "feat" => ReleaseType.Minor,
            "fix" or "perf" => ReleaseType.Patch,
            _ => ReleaseType.None
        };
    }

    // Types are case-insensitive, but BREAKING CHANGE must be uppercase (Conventional Commits 1.0.0).
    [GeneratedRegex(@"^(?<type>[A-Za-z]+)(\([^()]*\))?(?<breaking>!)?: \S")]
    private static partial Regex SubjectPattern();

    [GeneratedRegex("^BREAKING[ -]CHANGE: ")]
    private static partial Regex BreakingChangeFooterPattern();
}
