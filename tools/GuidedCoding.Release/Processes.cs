using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GuidedCoding.Release;

public static class Processes
{
    public static string Capture(string workingDirectory, string fileName, IEnumerable<string> arguments)
    {
        var startInfo = CreateStartInfo(workingDirectory, fileName, arguments);
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;

        using var process = Start(startInfo);
        var error = process.StandardError.ReadToEndAsync();
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        EnsureSuccess(process, startInfo, error.GetAwaiter().GetResult());
        return output;
    }

    // Shares the console with the child process so that its output streams to the user.
    public static void Run(string workingDirectory, string fileName, IEnumerable<string> arguments)
    {
        var startInfo = CreateStartInfo(workingDirectory, fileName, arguments);

        using var process = Start(startInfo);
        process.WaitForExit();
        EnsureSuccess(process, startInfo, error: "");
    }

    private static ProcessStartInfo CreateStartInfo(
        string workingDirectory,
        string fileName,
        IEnumerable<string> arguments
    )
    {
        var startInfo = new ProcessStartInfo(fileName)
        {
            WorkingDirectory = workingDirectory,
            UseShellExecute = false
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        return startInfo;
    }

    private static Process Start(ProcessStartInfo startInfo) =>
        Process.Start(startInfo) ??
        throw new InvalidOperationException($"Could not start '{startInfo.FileName}'.");

    private static void EnsureSuccess(Process process, ProcessStartInfo startInfo, string error)
    {
        if (process.ExitCode == 0)
        {
            return;
        }

        var command = string.Join(' ', startInfo.ArgumentList.Prepend(startInfo.FileName));
        var details = error.Trim();
        throw new InvalidOperationException(
            $"'{command}' failed with exit code {process.ExitCode}." + (details.Length > 0 ? $"\n{details}" : "")
        );
    }
}
