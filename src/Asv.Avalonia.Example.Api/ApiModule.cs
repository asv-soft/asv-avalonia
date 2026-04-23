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
    : ContextCommand<IViewModel>.WithArg<StringArg>,
        ContextCommand<IViewModel>.WithArg<DoubleArg>
{
    public ValueTask Execute(IViewModel context, StringArg argument)
    {
        throw new NotImplementedException();
    }

    public ValueTask Execute(IViewModel context, DoubleArg argument)
    {
        throw new NotImplementedException();
    }
}
