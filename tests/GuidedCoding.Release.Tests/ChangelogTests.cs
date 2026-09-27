using System;
using Xunit;

namespace GuidedCoding.Release.Tests;

public sealed class ChangelogTests
{
    private static readonly SemanticVersion Version = new (1, 1, 0);
    private static readonly DateOnly Date = new (2026, 9, 26);

    [Fact]
    public void MovesTheUnreleasedEntriesIntoTheReleasedVersion()
    {
        const string changelog =
            "# Changelog\n\n## [Unreleased]\n\n- Add a skill.\n- Fix a typo.\n\n## [1.0.0] - 2026-01-01\n\n- Start.\n";

        var released = Changelog.Release(changelog, Version, Date);

        Assert.Equal(
            "# Changelog\n\n## [Unreleased]\n\n## [1.1.0] - 2026-09-26\n\n- Add a skill.\n- Fix a typo.\n\n" +
            "## [1.0.0] - 2026-01-01\n\n- Start.\n",
            released
        );
    }

    [Fact]
    public void KeepsSubsectionHeadingsWithTheirEntries()
    {
        var released = Changelog.Release("## [Unreleased]\n\n### Added\n\n- Add a skill.\n", Version, Date);

        Assert.Equal("## [Unreleased]\n\n## [1.1.0] - 2026-09-26\n\n### Added\n\n- Add a skill.\n", released);
    }

    [Fact]
    public void NormalizesLineEndings()
    {
        var released = Changelog.Release("## [Unreleased]\r\n\r\n- Add a skill.\r\n", Version, Date);

        Assert.Equal("## [Unreleased]\n\n## [1.1.0] - 2026-09-26\n\n- Add a skill.\n", released);
    }

    [Fact]
    public void RequiresAnUnreleasedSection()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => Changelog.Release("# Changelog\n\n## [1.0.0] - 2026-01-01\n\n- Start.\n", Version, Date)
        );

        Assert.Equal("CHANGELOG.md has no '## [Unreleased]' section.", exception.Message);
    }

    [Theory]
    [InlineData("# Changelog\n\n## [Unreleased]\n")]
    [InlineData("# Changelog\n\n## [Unreleased]\n\n  \n## [1.0.0] - 2026-01-01\n\n- Start.\n")]
    [InlineData("# Changelog\n\n## [Unreleased]\n\n### Added\n\n### Fixed\n\n## [1.0.0] - 2026-01-01\n\n- Start.\n")]
    public void RequiresUnreleasedEntries(string changelog)
    {
        var exception = Assert.Throws<InvalidOperationException>(() => Changelog.Release(changelog, Version, Date));

        Assert.StartsWith("CHANGELOG.md has no entries under '## [Unreleased]'.", exception.Message);
    }
}
