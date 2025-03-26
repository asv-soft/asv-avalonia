namespace Asv.Avalonia;

public class BoolCommandArg(bool value) : ICommandArg
{
    public bool Value => value;

    public override string ToString()
    {
        return $"{Value}";
    }
}
