using Asv.Avalonia.GeoMap;

namespace Asv.Avalonia.Example.Api;

public interface IWellKnownCommands { }

public class ContextCommand<TContext>
{
    public interface WithArg<TArgument>
    {
        ValueTask Execute(TContext context, TArgument argument);
    }
}

public class Command1
    : ContextCommand<IRoutable>.WithArg<StringArg>,
        ContextCommand<IRoutable>.WithArg<DoubleArg>
{
    public ValueTask Execute(IRoutable context, StringArg argument)
    {
        throw new NotImplementedException();
    }

    public ValueTask Execute(IRoutable context, DoubleArg argument)
    {
        throw new NotImplementedException();
    }
}
