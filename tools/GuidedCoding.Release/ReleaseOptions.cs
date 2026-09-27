using System;
using System.Collections.Generic;

namespace GuidedCoding.Release;

public sealed record ReleaseOptions(SemanticVersion? Version, bool DryRun)
{
    private const string Usage = "Usage: GuidedCoding.Release [--version MAJOR.MINOR.PATCH] [--dry-run]";

    public static ReleaseOptions Parse(IReadOnlyList<string> args)
    {
        SemanticVersion? version = null;
        var dryRun = false;

        for (var index = 0; index < args.Count; index++)
        {
            switch (args[index])
            {
                case "--dry-run" when !dryRun:
                    dryRun = true;
                    break;
                case "--version" when version is null && index + 1 < args.Count:
                    version = SemanticVersion.Parse(args[++index]);
                    break;
                default:
                    throw new InvalidOperationException(Usage);
            }
        }

        return new(version, dryRun);
    }
}
