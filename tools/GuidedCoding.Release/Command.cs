using System.Linq;

namespace GuidedCoding.Release;

public sealed record Command(string FileName, params string[] Arguments)
{
    public override string ToString() => string.Join(' ', Arguments.Prepend(FileName));
}

public interface ICommandRunner
{
    void Run(Command command);
}

public sealed class ProcessCommandRunner(string workingDirectory) : ICommandRunner
{
    public void Run(Command command) => Processes.Run(workingDirectory, command.FileName, command.Arguments);
}
