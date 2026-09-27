using System;
using System.IO;
using Xunit;

namespace GuidedCoding.Release.Tests;

public sealed class ReleaseWorkflowTests
{
    private const string GeneratorCheck =
        "dotnet run --project tools/GuidedCoding.ClaudeGenerator --configuration Release -- --check";
    private const string Tests = "dotnet test --configuration Release";
    private const string PublishDryRun = "gh skill publish --dry-run";

    private static readonly DateOnly Today = new(2026, 9, 26);

    private readonly RecordingCommandRunner _commands = new();
    private readonly StringWriter _output = new();

    private string Output => _output.ToString().ReplaceLineEndings("\n");

    [Fact]
    public void ReleasesTheNextVersionDerivedFromConventionalCommits()
    {
        using var repository = ReleaseRepository.Create();
        repository.Commit("fix: correct a typo");
        repository.Commit("feat: add a skill");

        var result = Release(repository);

        Assert.Equal(ReleaseResult.Released, result);
        foreach (var path in ReleaseRepository.ManifestPaths)
        {
            Assert.Equal(ReleaseRepository.Manifest(path, "1.1.0"), repository.ReadFile(path));
        }

        Assert.Equal(
            "# Changelog\n\n## [Unreleased]\n\n## [1.1.0] - 2026-09-26\n\n- Add a skill.\n",
            repository.ReadFile("CHANGELOG.md")
        );
        Assert.Equal("chore(release): 1.1.0", repository.Git("log", "-1", "--format=%s").Trim());
        Assert.True(repository.IsClean);
        Assert.Equal(repository.Head, repository.Origin("rev-parse", "main").Trim());
        Assert.Equal(repository.Head, repository.Origin("rev-parse", "v1.1.0^{commit}").Trim());
        Assert.Equal(
            [GeneratorCheck, Tests, PublishDryRun, "gh skill publish --tag v1.1.0"],
            _commands.Commands
        );
        Assert.EndsWith("Pushed main and v1.1.0 to origin.\nReleased v1.1.0.\n", Output);
    }

    [Theory]
    [InlineData("fix: correct a typo", "1.0.1 (patch)")]
    [InlineData("perf: shorten a skill", "1.0.1 (patch)")]
    [InlineData("feat: add a skill", "1.1.0 (minor)")]
    [InlineData("feat!: rename a skill", "2.0.0 (major)")]
    [InlineData("refactor: restructure a skill\n\nBREAKING CHANGE: the skill was renamed", "2.0.0 (major)")]
    public void ProposesTheLargestBumpOfAllCommits(string message, string expectedVersion)
    {
        using var repository = ReleaseRepository.Create();
        repository.Commit("docs: explain the change");
        repository.Commit(message);

        Release(repository, "--dry-run");

        Assert.Contains($"Next version: {expectedVersion}\n", Output);
    }

    [Fact]
    public void DryRunReportsTheProposalWithoutChangingAnything()
    {
        using var repository = ReleaseRepository.Create();
        repository.Commit("feat: add a skill");
        repository.Commit("docs: explain the skill");
        var head = repository.Head;

        var result = Release(repository, "--dry-run");

        Assert.Equal(ReleaseResult.DryRun, result);
        Assert.Equal(
            """
            Last release: v1.0.0
            Commits to release:
              none   docs: explain the skill
              minor  feat: add a skill
            Next version: 1.1.0 (minor)
            Dry run: nothing was changed.

            """,
            Output
        );
        AssertNothingChanged(repository, head);
    }

    [Fact]
    public void UsesTheRequestedVersionInsteadOfTheProposal()
    {
        using var repository = ReleaseRepository.Create();
        repository.Commit("fix: correct a typo");

        var result = Release(repository, "--version 3.0.0");

        Assert.Equal(ReleaseResult.Released, result);
        Assert.Contains("Next version: 3.0.0 (requested)\n", Output);
        Assert.Equal(ReleaseRepository.Manifest("plugin.json", "3.0.0"), repository.ReadFile("plugin.json"));
        Assert.Equal(repository.Head, repository.Origin("rev-parse", "v3.0.0^{commit}").Trim());
    }

    [Fact]
    public void ReleasesTheFirstVersionWhenItIsRequested()
    {
        using var repository = ReleaseRepository.Create(released: false);

        var result = Release(repository, "--version 1.0.0");

        Assert.Equal(ReleaseResult.Released, result);
        Assert.StartsWith("Last release: none\n", Output);
        foreach (var path in ReleaseRepository.ManifestPaths)
        {
            Assert.Equal(ReleaseRepository.Manifest(path, "1.0.0"), repository.ReadFile(path));
        }

        Assert.Contains("## [1.0.0] - 2026-09-26\n", repository.ReadFile("CHANGELOG.md"));
        Assert.Equal(repository.Head, repository.Origin("rev-parse", "v1.0.0^{commit}").Trim());
    }

    [Fact]
    public void RequiresAVersionForTheFirstRelease()
    {
        using var repository = ReleaseRepository.Create(released: false);
        var head = repository.Head;

        var exception = Assert.Throws<InvalidOperationException>(() => Release(repository));

        Assert.Contains("Pass --version", exception.Message);
        AssertNothingChanged(repository, head);
    }

    [Fact]
    public void RefusesToReleaseWhenNoCommitRequiresIt()
    {
        using var repository = ReleaseRepository.Create();
        repository.Commit("docs: explain a skill");
        repository.Commit("chore: tidy up");
        var head = repository.Head;

        var exception = Assert.Throws<InvalidOperationException>(() => Release(repository));

        Assert.StartsWith("No commit requires a release.", exception.Message);
        AssertNothingChanged(repository, head);
    }

    [Fact]
    public void RefusesToReleaseWithoutNewCommits()
    {
        using var repository = ReleaseRepository.Create();
        var head = repository.Head;

        var exception = Assert.Throws<InvalidOperationException>(() => Release(repository, "--version 2.0.0"));

        Assert.Equal("There are no commits since v1.0.0.", exception.Message);
        AssertNothingChanged(repository, head);
    }

    [Theory]
    [InlineData("0.9.0")]
    [InlineData("1.0.0")]
    public void RefusesARequestedVersionThatIsNotNewerThanTheLastRelease(string version)
    {
        using var repository = ReleaseRepository.Create();
        repository.Commit("fix: correct a typo");
        var head = repository.Head;

        var exception = Assert.Throws<InvalidOperationException>(() => Release(repository, $"--version {version}"));

        Assert.Equal($"Version {version} must be greater than the last release 1.0.0.", exception.Message);
        AssertNothingChanged(repository, head);
    }

    [Fact]
    public void RefusesToReuseAnExistingTag()
    {
        using var repository = ReleaseRepository.Create();
        repository.Git("switch", "--create", "experiment");
        repository.Commit("fix: try something");
        repository.Tag("v1.0.1");
        repository.Git("switch", "main");
        repository.Commit("fix: correct a typo");
        var head = repository.Head;

        var exception = Assert.Throws<InvalidOperationException>(() => Release(repository));

        Assert.Equal("Tag v1.0.1 already exists.", exception.Message);
        Assert.True(repository.IsClean);
        Assert.Equal(head, repository.Head);
        Assert.Empty(_commands.Commands);
    }

    [Fact]
    public void RefusesToReleaseFromAnotherBranch()
    {
        using var repository = ReleaseRepository.Create();
        repository.Git("switch", "--create", "feature");
        repository.Commit("feat: add a skill");
        var head = repository.Head;

        var exception = Assert.Throws<InvalidOperationException>(() => Release(repository));

        Assert.Equal("Releases are made from main, but the current branch is 'feature'.", exception.Message);
        AssertNothingChanged(repository, head);
    }

    [Fact]
    public void RefusesToReleaseWithUncommittedChanges()
    {
        using var repository = ReleaseRepository.Create();
        repository.Commit("feat: add a skill");
        repository.WriteFile("notes.md", "Work in progress");
        var head = repository.Head;

        var exception = Assert.Throws<InvalidOperationException>(() => Release(repository));

        Assert.Equal("Commit or stash your changes before releasing.", exception.Message);
        Assert.Equal(head, repository.Head);
        Assert.Equal(ReleaseRepository.Manifest("plugin.json", "1.0.0"), repository.ReadFile("plugin.json"));
        Assert.Empty(_commands.Commands);
    }

    [Fact]
    public void RefusesToReleaseWhenMainIsBehindOrigin()
    {
        using var repository = ReleaseRepository.Create();
        repository.PushCommitFromAnotherClone("fix: correct a typo elsewhere");
        repository.Commit("feat: add a skill");
        var head = repository.Head;

        var exception = Assert.Throws<InvalidOperationException>(() => Release(repository));

        Assert.Equal("main is 1 commit(s) behind origin/main. Pull before releasing.", exception.Message);
        AssertNothingChanged(repository, head);
    }

    [Fact]
    public void RefusesToReleaseWithoutChangelogEntries()
    {
        using var repository = ReleaseRepository.Create();
        repository.WriteFile("CHANGELOG.md", "# Changelog\n\n## [Unreleased]\n\n## [1.0.0] - 2026-01-01\n\n- Start.\n");
        repository.Commit("fix: correct a typo");
        var head = repository.Head;

        var exception = Assert.Throws<InvalidOperationException>(() => Release(repository));

        Assert.StartsWith("CHANGELOG.md has no entries under '## [Unreleased]'.", exception.Message);
        AssertNothingChanged(repository, head);
    }

    [Theory]
    [InlineData("n")]
    [InlineData("")]
    public void ChangesNothingWhenTheReleaseIsNotConfirmed(string answer)
    {
        using var repository = ReleaseRepository.Create();
        repository.Commit("feat: add a skill");
        var head = repository.Head;

        var result = Release(repository, answer: answer);

        Assert.Equal(ReleaseResult.Cancelled, result);
        Assert.EndsWith("Release v1.1.0? [y/N] Release cancelled.\n", Output);
        AssertNothingChanged(repository, head);
    }

    [Fact]
    public void ReleasesWithoutAskingWhenConfirmedUpFront()
    {
        using var repository = ReleaseRepository.Create();
        repository.Commit("feat: add a skill");

        var result = Release(repository, "--yes", answer: "");

        Assert.Equal(ReleaseResult.Released, result);
        Assert.DoesNotContain("[y/N]", Output);
        Assert.Equal(repository.Head, repository.Origin("rev-parse", "v1.1.0^{commit}").Trim());
    }

    [Fact]
    public void ValidatesTheBumpedFilesBeforeCommitting()
    {
        using var repository = ReleaseRepository.Create();
        repository.Commit("fix: correct a typo");
        var head = repository.Head;
        string? manifestDuringTests = null;
        string? headDuringTests = null;
        _commands.OnRun = command =>
        {
            if (command == Tests)
            {
                manifestDuringTests = repository.ReadFile("plugin.json");
                headDuringTests = repository.Head;
            }
        };

        Release(repository);

        Assert.Equal(ReleaseRepository.Manifest("plugin.json", "1.0.1"), manifestDuringTests);
        Assert.Equal(head, headDuringTests);
    }

    [Fact]
    public void RevertsTheReleaseChangesWhenValidationFails()
    {
        using var repository = ReleaseRepository.Create();
        repository.Commit("fix: correct a typo");
        var head = repository.Head;
        _commands.FailingCommand = Tests;

        var exception = Assert.Throws<InvalidOperationException>(() => Release(repository));

        Assert.Equal(
            $"The release changes were reverted. '{Tests}' failed with exit code 1.",
            exception.Message
        );
        Assert.True(repository.IsClean);
        Assert.Equal(head, repository.Head);
        Assert.Equal("v1.0.0", repository.Git("tag", "--list").Trim());
        Assert.Equal([GeneratorCheck, Tests], _commands.Commands);
    }

    [Fact]
    public void ExplainsHowToFinishWhenPushingFails()
    {
        using var repository = ReleaseRepository.Create();
        repository.Commit("fix: correct a typo");
        repository.RejectPushes();

        var exception = Assert.Throws<InvalidOperationException>(() => Release(repository));

        Assert.StartsWith(
            "The release commit and v1.0.1 were created locally, but pushing failed. " +
            "Run 'git push --atomic origin main v1.0.1' and 'gh skill publish --tag v1.0.1' to finish the release.",
            exception.Message
        );
        Assert.Equal(repository.Head, repository.Git("rev-parse", "v1.0.1^{commit}").Trim());
        Assert.Equal("v1.0.0", repository.Origin("tag", "--list").Trim());
        Assert.Equal([GeneratorCheck, Tests, PublishDryRun], _commands.Commands);
    }

    [Fact]
    public void PublishesAfterTheTagReachedOrigin()
    {
        using var repository = ReleaseRepository.Create();
        repository.Commit("fix: correct a typo");
        string? originTagsDuringPublish = null;
        _commands.OnRun = command =>
        {
            if (command == "gh skill publish --tag v1.0.1")
            {
                originTagsDuringPublish = repository.Origin("tag", "--list").ReplaceLineEndings("\n");
            }
        };

        Release(repository);

        Assert.Equal("v1.0.0\nv1.0.1\n", originTagsDuringPublish);
    }

    [Fact]
    public void ExplainsHowToFinishWhenPublishingFails()
    {
        using var repository = ReleaseRepository.Create();
        repository.Commit("fix: correct a typo");
        _commands.FailingCommand = "gh skill publish --tag v1.0.1";

        var exception = Assert.Throws<InvalidOperationException>(() => Release(repository));

        Assert.StartsWith(
            "v1.0.1 was pushed, but publishing failed. Run 'gh skill publish --tag v1.0.1' to finish the release.",
            exception.Message
        );
        Assert.Equal(repository.Head, repository.Origin("rev-parse", "v1.0.1^{commit}").Trim());
    }

    private ReleaseResult Release(ReleaseRepository repository, string arguments = "", string answer = "y")
    {
        var workflow = new ReleaseWorkflow(
            repository.WorkingDirectory,
            _commands,
            new StringReader(answer + "\n"),
            _output,
            Today
        );

        return workflow.Run(ReleaseOptions.Parse(arguments.Split(' ', StringSplitOptions.RemoveEmptyEntries)));
    }

    private void AssertNothingChanged(ReleaseRepository repository, string head)
    {
        Assert.True(repository.IsClean);
        Assert.Equal(head, repository.Head);
        Assert.Equal(ReleaseRepository.Manifest("plugin.json", "1.0.0"), repository.ReadFile("plugin.json"));
        Assert.All(
            repository.Git("tag", "--list").Split('\n', StringSplitOptions.RemoveEmptyEntries),
            tag => Assert.Equal("v1.0.0", tag)
        );
        Assert.Empty(_commands.Commands);
    }
}
