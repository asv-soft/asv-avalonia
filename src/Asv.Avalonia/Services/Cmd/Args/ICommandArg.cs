using System.Buffers;
using Newtonsoft.Json;
using R3;

namespace Asv.Avalonia;

public interface ICommandArg
{
    // TODO: add save load functions for persistable
}

public static class CommandArg
{
    public static ICommandArg Empty { get; } = new EmptyCommandArg();

    public static bool IsEmpty(ICommandArg arg) => arg is EmptyCommandArg;

    public static ICommandArg FromString(string value) => new StringCommandArg(value);

    public static bool TryGetString(ICommandArg commandArg, out string? value)
    {
        if (commandArg is StringCommandArg s)
        {
            value = s.Value;
            return true;
        }

        value = null;
        return false;
    }

    public static ICommandArg FromDouble(double value) => new DoubleCommandArg(value);

    public static bool TryGetDouble(ICommandArg commandArg, out double value)
    {
        if (commandArg is DoubleCommandArg s)
        {
            value = s.Value;
            return true;
        }

        value = 0;
        return false;
    }

    public static ICommandArg FromAddAction(string value) =>
        new ActionCommandArg(null, value, CommandParameterActionType.Add);

    public static ICommandArg FromRemoveAction(string id) =>
        new ActionCommandArg(id, null, CommandParameterActionType.Remove);

    public static ICommandArg FromChangeAction(string id, string value) =>
        new ActionCommandArg(id, value, CommandParameterActionType.Change);
}
