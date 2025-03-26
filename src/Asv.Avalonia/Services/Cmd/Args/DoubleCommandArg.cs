namespace Asv.Avalonia;

public class DoubleCommandArg(double value) : ICommandArg
{
    public double Value => value;

    public override string ToString()
    {
        return $"{Value:G}";
    }
}
