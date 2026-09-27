using System;
using Xunit;

namespace GuidedCoding.Release.Tests;

public sealed class SemanticVersionTests
{
    [Theory]
    [InlineData("0.0.0", 0, 0, 0)]
    [InlineData("1.2.3", 1, 2, 3)]
    [InlineData("10.20.30", 10, 20, 30)]
    public void ParsesVersions(string value, int major, int minor, int patch)
    {
        Assert.Equal(new SemanticVersion(major, minor, patch), SemanticVersion.Parse(value));
    }

    [Theory]
    [InlineData("")]
    [InlineData("1")]
    [InlineData("1.2")]
    [InlineData("1.2.3.4")]
    [InlineData("v1.2.3")]
    [InlineData("01.2.3")]
    [InlineData("1.2.03")]
    [InlineData("1.2.3-beta.1")]
    [InlineData("1.2.3+build")]
    [InlineData("1.-2.3")]
    [InlineData(" 1.2.3")]
    [InlineData("1.2.99999999999")]
    public void RejectsInvalidVersions(string value)
    {
        var exception = Assert.Throws<FormatException>(() => SemanticVersion.Parse(value));

        Assert.Equal($"'{value}' is not a version in the form MAJOR.MINOR.PATCH.", exception.Message);
    }

    [Theory]
    [InlineData("Patch", "1.2.4")]
    [InlineData("Minor", "1.3.0")]
    [InlineData("Major", "2.0.0")]
    public void BumpsVersions(string releaseType, string expected)
    {
        var version = new SemanticVersion(1, 2, 3).Bump(Enum.Parse<ReleaseType>(releaseType));

        Assert.Equal(expected, version.ToString());
    }

    [Fact]
    public void RefusesToBumpWithoutAReleaseType()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SemanticVersion(1, 2, 3).Bump(ReleaseType.None));
    }

    [Theory]
    [InlineData("1.10.0", "1.9.0")]
    [InlineData("2.0.0", "1.99.99")]
    [InlineData("1.2.10", "1.2.9")]
    public void ComparesVersionsNumerically(string greater, string lesser)
    {
        var greaterVersion = SemanticVersion.Parse(greater);
        var lesserVersion = SemanticVersion.Parse(lesser);

        Assert.True(greaterVersion > lesserVersion);
        Assert.True(greaterVersion >= lesserVersion);
        Assert.True(lesserVersion < greaterVersion);
        Assert.True(lesserVersion <= greaterVersion);
        Assert.True(greaterVersion >= SemanticVersion.Parse(greater));
    }

    [Fact]
    public void UsesAVPrefixForTags()
    {
        Assert.Equal("v1.2.3", new SemanticVersion(1, 2, 3).ToTag());
    }

    [Theory]
    [InlineData("v1.2.3", true)]
    [InlineData("1.2.3", false)]
    [InlineData("v1.2", false)]
    [InlineData("latest", false)]
    public void ParsesOnlyReleaseTags(string tag, bool isReleaseTag)
    {
        Assert.Equal(isReleaseTag, SemanticVersion.TryParseTag(tag, out _));
    }
}
