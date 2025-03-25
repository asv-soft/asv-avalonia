using System.Buffers;
using Newtonsoft.Json;
using R3;

namespace Asv.Avalonia;

public interface ICommandParameter { }

public class StringCommandParameter(string value) : ICommandParameter
{
    public string Value => value;
}

public class BoolCommandParameter(bool value) : ICommandParameter
{
    public bool Value => value;
}

public class DoubleCommandParameter(double value) : ICommandParameter
{
    public double Value => value;
}

public static class CommandParameter
{
    public static ICommandParameter Empty { get; } = new EmptyCommandParameter();

    public static ICommandParameter FromString(string value) => new StringCommandParameter(value);

    public static bool TryGetString(ICommandParameter commandParameter, out string? value)
    {
        if (commandParameter is StringCommandParameter s)
        {
            value = s.Value;
            return true;
        }

        value = null;
        return false;
    }
}
