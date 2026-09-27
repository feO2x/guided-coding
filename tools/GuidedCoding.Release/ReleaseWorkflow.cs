using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GuidedCoding.Release;

public sealed class ReleaseWorkflow(
    string repositoryRoot,
    ICommandRunner commands,
    TextReader input,
    TextWriter output,
    DateOnly today
)
{
    private static readonly Command[] Validations =
    [
        new(
            "dotnet",
            "run",
            "--project",
            "tools/GuidedCoding.ClaudeGenerator",
            "--configuration",
            "Release",
            "--",
            "--check"
        ),
        // The Release configuration keeps the build away from the Debug binaries of this running tool.
        new("dotnet", "test", "--configuration", "Release"),
        new("gh", "skill", "publish", "--dry-run")
    ];

    private readonly Git _git = new(repositoryRoot);

    public ReleaseResult Run(ReleaseOptions options)
    {
        EnsureReleasableCheckout();

        var version = DetermineVersion(options.Version, _git.FindLatestReleaseVersion());
        var tag = version.ToTag();
        if (_git.TagExists(tag))
        {
            throw new InvalidOperationException($"Tag {tag} already exists.");
        }

        var changes = PrepareChanges(version);
        if (options.DryRun)
        {
            output.WriteLine("Dry run: nothing was changed.");
            return ReleaseResult.DryRun;
        }

        if (!options.Confirmed && !Confirm(tag))
        {
            output.WriteLine("Release cancelled.");
            return ReleaseResult.Cancelled;
        }

        ApplyAndValidate(changes);
        _git.Commit(changes.Select(change => change.Path), $"chore(release): {version}");
        _git.CreateTag(tag);
        Push(tag);
        Publish(tag);

        output.WriteLine($"Released {tag}.");
        return ReleaseResult.Released;
    }

    private void EnsureReleasableCheckout()
    {
        var branch = _git.CurrentBranch();
        if (branch != Git.MainBranch)
        {
            throw new InvalidOperationException(
                $"Releases are made from {Git.MainBranch}, but the current branch is '{branch}'."
            );
        }

        if (_git.HasUncommittedChanges())
        {
            throw new InvalidOperationException("Commit or stash your changes before releasing.");
        }

        _git.FetchOrigin();
        var behind = _git.CountCommitsBehindOrigin();
        if (behind > 0)
        {
            throw new InvalidOperationException(
                $"{Git.MainBranch} is {behind} commit(s) behind {Git.Remote}/{Git.MainBranch}. Pull before releasing."
            );
        }
    }

    private SemanticVersion DetermineVersion(SemanticVersion? requested, SemanticVersion? lastRelease)
    {
        var lastTag = lastRelease?.ToTag();
        var commits = _git.CommitMessagesSince(lastRelease);
        if (commits.Count == 0)
        {
            throw new InvalidOperationException($"There are no commits since {lastTag}.");
        }

        output.WriteLine($"Last release: {lastTag ?? "none"}");
        output.WriteLine("Commits to release:");
        var releaseType = ReleaseType.None;
        foreach (var message in commits)
        {
            var commitReleaseType = ConventionalCommits.Classify(message);
            releaseType = commitReleaseType > releaseType ? commitReleaseType : releaseType;
            output.WriteLine($"  {Label(commitReleaseType),-5}  {message.Split('\n')[0]}");
        }

        if (requested is { } version)
        {
            if (lastRelease is { } last && version <= last)
            {
                throw new InvalidOperationException(
                    $"Version {version} must be greater than the last release {last}."
                );
            }

            output.WriteLine($"Next version: {version} (requested)");
            return version;
        }

        if (lastRelease is null)
        {
            throw new InvalidOperationException(
                "There is no release tag yet. Pass --version to choose the first version."
            );
        }

        if (releaseType == ReleaseType.None)
        {
            throw new InvalidOperationException(
                "No commit requires a release. Only feat, fix, perf, and breaking changes trigger one."
            );
        }

        var next = lastRelease.Value.Bump(releaseType);
        output.WriteLine($"Next version: {next} ({Label(releaseType)})");
        return next;
    }

    // Computes every file change up front so that problems surface before anything is written.
    private List<FileChange> PrepareChanges(SemanticVersion version)
    {
        var changes = Manifests
           .Paths
           .Select(path => new FileChange(path, Manifests.SetVersion(path, ReadFile(path), version)))
           .ToList();
        changes.Add(
            new FileChange(Changelog.FileName, Changelog.Release(ReadFile(Changelog.FileName), version, today))
        );
        return changes;
    }

    private void ApplyAndValidate(IReadOnlyList<FileChange> changes)
    {
        try
        {
            foreach (var change in changes)
            {
                File.WriteAllText(Path.Combine(repositoryRoot, change.Path), change.Content);
            }

            foreach (var command in Validations)
            {
                commands.Run(command);
            }
        }
        catch (Exception exception)
        {
            _git.Restore(changes.Select(change => change.Path));
            throw new InvalidOperationException(
                $"The release changes were reverted. {exception.Message}",
                exception
            );
        }
    }

    private void Push(string tag)
    {
        try
        {
            _git.Push(tag);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                $"The release commit and {tag} were created locally, but pushing failed. " +
                $"Run 'git push --atomic {Git.Remote} {Git.MainBranch} {tag}' and " +
                $"'gh skill publish --tag {tag}' to finish the release. {exception.Message}",
                exception
            );
        }

        output.WriteLine($"Pushed {Git.MainBranch} and {tag} to {Git.Remote}.");
    }

    private void Publish(string tag)
    {
        try
        {
            commands.Run(new("gh", "skill", "publish", "--tag", tag));
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                $"{tag} was pushed, but publishing failed. Run 'gh skill publish --tag {tag}' to finish the release. " +
                exception.Message,
                exception
            );
        }
    }

    private bool Confirm(string tag)
    {
        output.Write($"Release {tag}? [y/N] ");
        return input.ReadLine()?.Trim().ToLowerInvariant() is "y" or "yes";
    }

    private string ReadFile(string relativePath) => File.ReadAllText(Path.Combine(repositoryRoot, relativePath));

    private static string Label(ReleaseType releaseType) => releaseType.ToString().ToLowerInvariant();

    private sealed record FileChange(string Path, string Content);
}
