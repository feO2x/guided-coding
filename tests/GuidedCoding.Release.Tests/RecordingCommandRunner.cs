using System;
using System.Collections.Generic;

namespace GuidedCoding.Release.Tests;

// Stands in for the dotnet and gh commands, which are too slow or too far-reaching to run in tests.
internal sealed class RecordingCommandRunner : ICommandRunner
{
    private readonly List<string> _commands = [];

    public IReadOnlyList<string> Commands => _commands;

    public string? FailingCommand { get; set; }

    public Action<string>? OnRun { get; set; }

    // The output that Capture returns for a command line. Other commands produce no output.
    public Dictionary<string, string> Outputs { get; } = [];

    public void Run(Command command)
    {
        var commandLine = command.ToString();
        _commands.Add(commandLine);
        OnRun?.Invoke(commandLine);

        if (commandLine == FailingCommand)
        {
            throw new InvalidOperationException($"'{commandLine}' failed with exit code 1.");
        }
    }

    public string Capture(Command command)
    {
        Run(command);
        return Outputs.GetValueOrDefault(command.ToString(), "");
    }
}
