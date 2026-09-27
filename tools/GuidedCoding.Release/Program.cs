using System;
using System.IO;

namespace GuidedCoding.Release;

public static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            var options = ReleaseOptions.Parse(args);
            var repositoryRoot = FindRepositoryRoot();
            var workflow = new ReleaseWorkflow(
                repositoryRoot,
                new ProcessCommandRunner(repositoryRoot),
                Console.In,
                Console.Out,
                DateOnly.FromDateTime(DateTime.UtcNow)
            );

            return workflow.Run(options) == ReleaseResult.Cancelled ? 1 : 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }

    private static string FindRepositoryRoot()
    {
        foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            for (
                var directory = new DirectoryInfo(start);
                directory is not null;
                directory = directory.Parent
            )
            {
                if (
                    File.Exists(Path.Combine(directory.FullName, "plugin.json")) &&
                    File.Exists(Path.Combine(directory.FullName, Changelog.FileName)) &&
                    Directory.Exists(Path.Combine(directory.FullName, "skills"))
                )
                {
                    return directory.FullName;
                }
            }
        }

        throw new InvalidOperationException("Could not locate the Guided Coding repository root.");
    }
}
