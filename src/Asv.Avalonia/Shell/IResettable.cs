namespace Asv.Avalonia.Example;

public interface IResettable : IRoutable
{
    CommandArg Reset(CommandArg value);
}
