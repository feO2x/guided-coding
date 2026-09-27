using System;
using System.Collections.Generic;
using System.Globalization;

namespace GuidedCoding.Release;

public sealed class Git(string repositoryRoot)
{
    public const string MainBranch = "main";
    public const string Remote = "origin";

    public string CurrentBranch() => Run("branch", "--show-current").Trim();

    public string Head() => Run("rev-parse", "HEAD").Trim();

    public bool HasUncommittedChanges() => Run("status", "--porcelain").Length > 0;

    public void FetchOrigin() => Run("fetch", "--tags", Remote);

    // Counts the commits that only origin/main contains and the commits that only HEAD contains.
    public (int Behind, int Ahead) CompareWithOrigin()
    {
        var counts = Run("rev-list", "--left-right", "--count", $"{Remote}/{MainBranch}...HEAD")
           .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return (int.Parse(counts[0], CultureInfo.InvariantCulture), int.Parse(counts[1], CultureInfo.InvariantCulture));
    }

    // Release tags are plain MAJOR.MINOR.PATCH versions. Only tags reachable from HEAD count, so a tag on
    // another branch cannot become the base of a release.
    public SemanticVersion? FindLatestReleaseVersion()
    {
        SemanticVersion? latest = null;
        foreach (var tag in Lines(Run("tag", "--list", "--merged", "HEAD")))
        {
            if (SemanticVersion.TryParse(tag, out var version) && (latest is null || version > latest.Value))
            {
                latest = version;
            }
        }

        return latest;
    }

    public bool TagExists(string tag) => Lines(Run("tag", "--list", tag)).Length > 0;

    public IReadOnlyList<string> CommitMessagesSince(SemanticVersion? release)
    {
        var range = release is { } version ? $"refs/tags/{version}..HEAD" : "HEAD";
        return Run("log", "--no-merges", "--format=%B%x1e", range)
           .Split('\x1e', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    public void Restore(IEnumerable<string> paths) => Run(["checkout", "HEAD", "--", .. paths]);

    // Moves the branch back to the commit, leaving the index and working tree as they are.
    public void ResetSoft(string commit) => Run("reset", "--soft", commit);

    public void Commit(IEnumerable<string> paths, string message)
    {
        Run(["add", "--", .. paths]);
        Run("commit", "-m", message);
    }

    public void CreateTag(string tag) => Run("tag", "-a", tag, "-m", tag);

    // Pushes the branch and the tag together, or neither of them.
    public void Push(string tag) => Run("push", "--atomic", Remote, MainBranch, $"refs/tags/{tag}");

    private string Run(params string[] arguments) => Processes.Capture(repositoryRoot, "git", arguments);

    private static string[] Lines(string output) =>
        output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
