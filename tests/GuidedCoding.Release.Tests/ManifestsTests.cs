using System;
using Xunit;

namespace GuidedCoding.Release.Tests;

public sealed class ManifestsTests
{
    private static readonly SemanticVersion Version = new(2, 1, 0);

    [Fact]
    public void ReplacesOnlyTheVersionValue()
    {
        const string manifest =
            "{\n  \"name\": \"guided-coding\",\n  \"version\" :  \"2.0.0\",\n  \"description\": \"Since version 1.0.0\"\n}\n";

        var updated = Manifests.SetVersion("plugin.json", manifest, Version);

        Assert.Equal(
            "{\n  \"name\": \"guided-coding\",\n  \"version\" :  \"2.1.0\",\n  \"description\": \"Since version 1.0.0\"\n}\n",
            updated
        );
    }

    [Theory]
    [InlineData("{ \"name\": \"guided-coding\" }", 0)]
    [InlineData("{ \"version\": \"1.0.0\", \"plugins\": [{ \"version\": \"1.0.0\" }] }", 2)]
    public void RequiresExactlyOneVersionProperty(string manifest, int count)
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => Manifests.SetVersion("plugin.json", manifest, Version)
        );

        Assert.Equal(
            $"plugin.json must contain exactly one \"version\" property, but contains {count}.",
            exception.Message
        );
    }
}
