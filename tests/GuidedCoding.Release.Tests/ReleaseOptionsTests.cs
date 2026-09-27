using System;
using Xunit;

namespace GuidedCoding.Release.Tests;

public sealed class ReleaseOptionsTests
{
    [Fact]
    public void ProposesTheVersionAndReleasesByDefault()
    {
        Assert.Equal(new ReleaseOptions(null, false, false), ReleaseOptions.Parse([]));
    }

    [Theory]
    [InlineData("--version", "2.1.0", "--dry-run", "--yes")]
    [InlineData("--yes", "--dry-run", "--version", "2.1.0")]
    public void ParsesAllOptionsInAnyOrder(params string[] args)
    {
        Assert.Equal(new ReleaseOptions(new SemanticVersion(2, 1, 0), true, true), ReleaseOptions.Parse(args));
    }

    [Theory]
    [InlineData("--version")]
    [InlineData("--dry-run", "--dry-run")]
    [InlineData("--yes", "--yes")]
    [InlineData("--version", "1.0.0", "--version", "2.0.0")]
    [InlineData("--tag", "v1.0.0")]
    public void RejectsUnknownOrIncompleteArguments(params string[] args)
    {
        var exception = Assert.Throws<InvalidOperationException>(() => ReleaseOptions.Parse(args));

        Assert.Equal(
            "Usage: GuidedCoding.Release [--version MAJOR.MINOR.PATCH] [--dry-run] [--yes]",
            exception.Message
        );
    }

    [Fact]
    public void RejectsInvalidVersions()
    {
        Assert.Throws<FormatException>(() => ReleaseOptions.Parse(["--version", "v2.1.0"]));
    }
}
