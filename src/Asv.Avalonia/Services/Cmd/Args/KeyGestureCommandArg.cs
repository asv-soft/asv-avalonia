using Avalonia.Input;

namespace Asv.Avalonia;

public class KeyGestureCommandArg(KeyGesture? value) : ICommandArg
{
    public KeyGesture? Value => value;

    public override string ToString()
    {
        return value?.ToString() ?? string.Empty;
    }
}
