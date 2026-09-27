using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GuidedCoding.Release;

public static partial class Manifests
{
    public static readonly IReadOnlyList<string> Paths =
    [
        "plugin.json",
        "claude-plugin/.claude-plugin/plugin.json",
        ".claude-plugin/marketplace.json"
    ];

    // Replaces only the version value so that the rest of the file keeps its formatting.
    public static string SetVersion(string path, string json, SemanticVersion version)
    {
        var matches = VersionPropertyPattern().Matches(json);
        if (matches.Count != 1)
        {
            throw new InvalidOperationException(
                $"{path} must contain exactly one \"version\" property, but contains {matches.Count}."
            );
        }

        var value = matches[0].Groups["value"];
        return string.Concat(json.AsSpan(0, value.Index), version.ToString(), json.AsSpan(value.Index + value.Length));
    }

    [GeneratedRegex("\"version\"\\s*:\\s*\"(?<value>[^\"]*)\"")]
    private static partial Regex VersionPropertyPattern();
}
