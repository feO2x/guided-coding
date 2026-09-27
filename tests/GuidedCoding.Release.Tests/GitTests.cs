using System;
using System.Linq;
using Xunit;

namespace GuidedCoding.Release.Tests;

public sealed class GitTests
{
    [Fact]
    public void FindsTheHighestReleaseTagNumerically()
    {
        using var repository = ReleaseRepository.Create();
        foreach (var tag in new[] { "1.9.0", "1.10.0", "latest", "2.0", "3.0.0-beta.1", "v4.0.0" })
        {
            repository.Commit($"fix: prepare {tag}");
            repository.Tag(tag);
        }

        var version = new Git(repository.WorkingDirectory).FindLatestReleaseVersion();

        Assert.Equal(new SemanticVersion(1, 10, 0), version);
    }

    [Fact]
    public void IgnoresReleaseTagsOutsideTheCurrentBranch()
    {
        using var repository = ReleaseRepository.Create();
        repository.Git("switch", "--create", "experiment");
        repository.Commit("feat!: try something");
        repository.Tag("2.0.0");
        repository.Git("switch", "main");

        var version = new Git(repository.WorkingDirectory).FindLatestReleaseVersion();

        Assert.Equal(new SemanticVersion(1, 0, 0), version);
    }

    [Fact]
    public void FindsNoReleaseWithoutReleaseTags()
    {
        using var repository = ReleaseRepository.Create(released: false);

        var version = new Git(repository.WorkingDirectory).FindLatestReleaseVersion();

        Assert.Null(version);
    }

    [Fact]
    public void ListsCompleteCommitMessagesSinceTheLastReleaseWithoutMerges()
    {
        using var repository = ReleaseRepository.Create();
        repository.Commit("feat: add a skill\n\nThe skill explains itself.");
        repository.Git("switch", "--create", "feature");
        repository.Commit("fix: correct a typo");
        repository.Git("switch", "main");
        repository.Git("merge", "--no-ff", "-m", "Merge branch 'feature'", "feature");

        var messages = new Git(repository.WorkingDirectory).CommitMessagesSince(new SemanticVersion(1, 0, 0));

        Assert.Equal(
            ["feat: add a skill\n\nThe skill explains itself.", "fix: correct a typo"],
            messages.Order(StringComparer.Ordinal)
        );
    }

    [Fact]
    public void ListsAllCommitMessagesWithoutARelease()
    {
        using var repository = ReleaseRepository.Create(released: false);
        repository.Commit("feat: add a skill");

        var messages = new Git(repository.WorkingDirectory).CommitMessagesSince(null);

        Assert.Equal(["feat: add a skill", "chore: initial commit"], messages);
    }
}
