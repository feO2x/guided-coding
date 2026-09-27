using System;
using System.Collections.Generic;

namespace GuidedCoding.Release;

public sealed record ReleaseOptions(SemanticVersion? Version, bool DryRun, bool Confirmed)
{
    private const string Usage = "Usage: GuidedCoding.Release [--version MAJOR.MINOR.PATCH] [--dry-run] [--yes]";

    public static ReleaseOptions Parse(IReadOnlyList<string> args)
    {
        SemanticVersion? version = null;
        var dryRun = false;
        var confirmed = false;

        for (var index = 0; index < args.Count; index++)
        {
            switch (args[index])
            {
                case "--dry-run" when !dryRun:
                    dryRun = true;
                    break;
                case "--yes" when !confirmed:
                    confirmed = true;
                    break;
                case "--version" when version is null && index + 1 < args.Count:
                    version = SemanticVersion.Parse(args[++index]);
                    break;
                default:
                    throw new InvalidOperationException(Usage);
            }
        }

        return new(version, dryRun, confirmed);
    }
}
