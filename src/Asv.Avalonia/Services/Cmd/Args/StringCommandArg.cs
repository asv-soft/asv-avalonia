namespace Asv.Avalonia;

public class StringCommandArg(string value) : ICommandArg
{
    public string Value => value;

    public override string ToString()
    {
        return value;
    }
}
