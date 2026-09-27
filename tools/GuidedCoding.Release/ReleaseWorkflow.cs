using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace GuidedCoding.Release;

public sealed class ReleaseWorkflow(
    string repositoryRoot,
    ICommandRunner commands,
    TextReader input,
    TextWriter output,
    DateOnly today
)
{
    private const string DiscoveryTopic = "agent-skills";

    // Git ignores its own directory, so the notes stay out of the release commit and the checkout stays clean.
    private const string NotesFileName = "RELEASE_NOTES.md";

    private static readonly Command[] Validations =
    [
        new (
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
        new ("dotnet", "test", "--configuration", "Release"),
        new ("gh", "skill", "publish", "--dry-run")
    ];

    private readonly Git _git = new (repositoryRoot);

    public ReleaseResult Run(ReleaseOptions options)
    {
        EnsureReleasableCheckout();

        var version = DetermineVersion(options.Version, _git.FindLatestReleaseVersion());
        var tag = version.ToString();
        if (_git.TagExists(tag))
        {
            throw new InvalidOperationException($"Tag {tag} already exists.");
        }

        var (changes, notes) = PrepareChanges(version);
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

        EnsureDiscoverable();
        var notesFile = WriteNotes(notes);
        CommitRelease(changes, version, tag);
        Push(tag, notesFile);
        Publish(tag, notesFile);
        File.Delete(Path.Combine(repositoryRoot, notesFile));

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
        var (behind, ahead) = _git.CompareWithOrigin();
        if (behind > 0)
        {
            throw new InvalidOperationException(
                $"{Git.MainBranch} is {behind} commit(s) behind {Git.Remote}/{Git.MainBranch}. Pull before releasing."
            );
        }

        // Pushing the release would also push these commits, bypassing pull requests and CI.
        if (ahead > 0)
        {
            throw new InvalidOperationException(
                $"{Git.MainBranch} is {ahead} commit(s) ahead of {Git.Remote}/{Git.MainBranch}. " +
                "Merge them through a pull request or drop them before releasing."
            );
        }
    }

    private SemanticVersion DetermineVersion(SemanticVersion? requested, SemanticVersion? lastRelease)
    {
        var lastTag = lastRelease?.ToString();
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

    // Computes every file change and the release notes up front so that problems surface before anything is written.
    private (List<FileChange> Changes, string Notes) PrepareChanges(SemanticVersion version)
    {
        var changelog = Changelog.Release(ReadFile(Changelog.FileName), version, today);
        var changes = Manifests
           .Paths
           .Select(path => new FileChange(path, Manifests.SetVersion(path, ReadFile(path), version)))
           .Append(new (Changelog.FileName, changelog))
           .ToList();
        return (changes, Changelog.Notes(changelog, version));
    }

    // 'gh skill publish' checks the topic, but it cannot publish the pushed tag, so the release checks it instead.
    // The tool does not add the topic because GitHub's workflow token lacks the required admin permission.
    private void EnsureDiscoverable()
    {
        using var repository = JsonDocument.Parse(
            commands.Capture(new ("gh", "repo", "view", "--json", "repositoryTopics"))
        );
        var topics = repository.RootElement.GetProperty("repositoryTopics");
        var hasTopic = topics.ValueKind == JsonValueKind.Array &&
                       topics.EnumerateArray().Any(topic => topic.GetProperty("name").GetString() == DiscoveryTopic);
        if (!hasTopic)
        {
            throw new InvalidOperationException(
                $"The repository lacks the {DiscoveryTopic} topic that makes the skills discoverable. " +
                $"Add it with 'gh repo edit --add-topic {DiscoveryTopic}' before releasing."
            );
        }
    }

    // The file outlives a failed push or publish, so the commands to finish the release can still use it.
    private string WriteNotes(string notes)
    {
        var notesFile = _git.GitPath(NotesFileName);
        File.WriteAllText(Path.Combine(repositoryRoot, notesFile), notes);
        return notesFile;
    }

    // Everything up to the push stays local, so a failure, such as a failing hook or signature, is rolled back.
    private void CommitRelease(IReadOnlyList<FileChange> changes, SemanticVersion version, string tag)
    {
        var paths = changes.Select(change => change.Path).ToList();
        var head = _git.Head();
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

            _git.Commit(paths, $"chore(release): {version}");
            _git.CreateTag(tag);
        }
        catch (Exception exception)
        {
            // The tag is created last, so a failure never leaves one behind.
            _git.ResetSoft(head);
            _git.Restore(paths);
            throw new InvalidOperationException(
                $"The release changes were reverted. {exception.Message}",
                exception
            );
        }
    }

    private void Push(string tag, string notesFile)
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
                $"'{PublishCommand(tag, notesFile)}' to finish the release. {exception.Message}",
                exception
            );
        }

        output.WriteLine($"Pushed {Git.MainBranch} and {tag} to {Git.Remote}.");
    }

    // 'gh skill publish --tag' refuses tags that already exist on the remote, because it creates the tag itself
    // from the branch head. The tag is pushed atomically with main, so publishing creates the release for it instead.
    // 'gh skill publish --dry-run' already validated the skills, and EnsureDiscoverable checked the topic.
    private void Publish(string tag, string notesFile)
    {
        try
        {
            commands.Run(PublishCommand(tag, notesFile));
        }
        catch (Exception exception)
        {
            // gh can fail after GitHub created the release, and creating it again then fails as well.
            throw new InvalidOperationException(
                $"{tag} was pushed, but publishing failed. Run 'gh release view {tag}' to check whether the release " +
                $"exists. If it does not, run '{PublishCommand(tag, notesFile)}' to finish the release. " +
                exception.Message,
                exception
            );
        }
    }

    // The notes come from the changelog, because generated notes list pull request titles, not user-facing changes.
    private static Command PublishCommand(string tag, string notesFile) =>
        new ("gh", "release", "create", tag, "--verify-tag", "--notes-file", notesFile);

    private bool Confirm(string tag)
    {
        output.Write($"Release {tag}? [y/N] ");
        return input.ReadLine()?.Trim().ToLowerInvariant() is "y" or "yes";
    }

    private string ReadFile(string relativePath) => File.ReadAllText(Path.Combine(repositoryRoot, relativePath));

    private static string Label(ReleaseType releaseType) => releaseType.ToString().ToLowerInvariant();

    private sealed record FileChange(string Path, string Content);
}
