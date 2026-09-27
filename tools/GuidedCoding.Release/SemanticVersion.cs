using System;
using System.Globalization;

namespace GuidedCoding.Release;

// Releases use plain MAJOR.MINOR.PATCH versions without pre-release or build metadata.
public readonly record struct SemanticVersion(int Major, int Minor, int Patch) : IComparable<SemanticVersion>
{
    public int CompareTo(SemanticVersion other)
    {
        var major = Major.CompareTo(other.Major);
        if (major != 0)
        {
            return major;
        }

        var minor = Minor.CompareTo(other.Minor);
        return minor != 0 ? minor : Patch.CompareTo(other.Patch);
    }

    public static SemanticVersion Parse(string value) =>
        TryParse(value, out var version) ?
            version :
            throw new FormatException($"'{value}' is not a version in the form MAJOR.MINOR.PATCH.");

    public static bool TryParse(string value, out SemanticVersion version)
    {
        var parts = value.Split('.');
        if (
            parts.Length == 3 &&
            TryParseNumber(parts[0], out var major) &&
            TryParseNumber(parts[1], out var minor) &&
            TryParseNumber(parts[2], out var patch)
        )
        {
            version = new (major, minor, patch);
            return true;
        }

        version = default;
        return false;
    }

    public SemanticVersion Bump(ReleaseType releaseType) =>
        releaseType switch
        {
            ReleaseType.Major => new (Major + 1, 0, 0),
            ReleaseType.Minor => new (Major, Minor + 1, 0),
            ReleaseType.Patch => new (Major, Minor, Patch + 1),
            _ => throw new ArgumentOutOfRangeException(nameof(releaseType), releaseType, "Nothing to bump.")
        };

    public override string ToString() =>
        string.Create(CultureInfo.InvariantCulture, $"{Major}.{Minor}.{Patch}");

    public static bool operator <(SemanticVersion left, SemanticVersion right) => left.CompareTo(right) < 0;

    public static bool operator >(SemanticVersion left, SemanticVersion right) => left.CompareTo(right) > 0;

    public static bool operator <=(SemanticVersion left, SemanticVersion right) => left.CompareTo(right) <= 0;

    public static bool operator >=(SemanticVersion left, SemanticVersion right) => left.CompareTo(right) >= 0;

    // SemVer forbids leading zeros in numeric identifiers.
    private static bool TryParseNumber(string value, out int number) =>
        int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out number) &&
        (value.Length == 1 || value[0] != '0');
}
