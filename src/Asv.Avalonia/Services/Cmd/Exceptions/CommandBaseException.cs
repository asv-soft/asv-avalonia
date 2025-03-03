namespace Asv.Avalonia;

#pragma warning disable RCS1194
public class CommandBaseException : Exception
#pragma warning restore RCS1194
{
    public ICommandInfo CommandInfo { get; }

    public CommandBaseException(ICommandInfo commandInfo)
    {
        CommandInfo = commandInfo;
    }

    public CommandBaseException(ICommandInfo commandInfo, string message)
        : base(message)
    {
        CommandInfo = commandInfo;
    }

    public CommandBaseException(ICommandInfo commandInfo, string message, Exception inner)
        : base(message, inner)
    {
        CommandInfo = commandInfo;
    }
}
