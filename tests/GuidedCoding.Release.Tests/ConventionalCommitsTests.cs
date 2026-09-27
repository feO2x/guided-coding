using Xunit;

namespace GuidedCoding.Release.Tests;

public sealed class ConventionalCommitsTests
{
    [Theory]
    [InlineData("feat!: rename a skill")]
    [InlineData("fix(setup)!: drop an option")]
    [InlineData("refactor!: restructure the plugin")]
    [InlineData("docs: explain a skill\n\nBREAKING CHANGE: the skill was renamed")]
    [InlineData("fix: correct a typo\n\nBREAKING-CHANGE: the output moved")]
    [InlineData("feat: add a skill\r\n\r\nBREAKING CHANGE: the skill replaces another one")]
    public void ClassifiesBreakingChangesAsMajor(string message)
    {
        Assert.Equal(ReleaseType.Major, ConventionalCommits.Classify(message));
    }

    [Theory]
    [InlineData("feat: add a skill")]
    [InlineData("feat(setup): support another agent")]
    [InlineData("Feat: add a skill")]
    [InlineData("feat: add a skill\n\nThe body mentions BREAKING CHANGE: only inline.")]
    public void ClassifiesFeaturesAsMinor(string message)
    {
        Assert.Equal(ReleaseType.Minor, ConventionalCommits.Classify(message));
    }

    [Theory]
    [InlineData("fix: correct a typo")]
    [InlineData("fix(write-plan): clarify a step")]
    [InlineData("perf: shorten a skill")]
    [InlineData("fix: correct a typo\n\nbreaking change: footers must be uppercase")]
    public void ClassifiesFixesAndPerformanceImprovementsAsPatch(string message)
    {
        Assert.Equal(ReleaseType.Patch, ConventionalCommits.Classify(message));
    }

    [Theory]
    [InlineData("docs: explain a skill")]
    [InlineData("chore(release): 1.0.0")]
    [InlineData("test: cover a skill")]
    [InlineData("Merge pull request #1 from feO2x/guided-learning")]
    [InlineData("feat:missing space")]
    [InlineData("feat add a skill")]
    [InlineData("Add a skill\n\nBREAKING CHANGE: the subject is not conventional")]
    [InlineData("")]
    public void ClassifiesOtherCommitsAsNone(string message)
    {
        Assert.Equal(ReleaseType.None, ConventionalCommits.Classify(message));
    }
}
