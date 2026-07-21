using Asv.Avalonia.GeoMap;

namespace Asv.Avalonia.Example.Api;

public interface IWellKnownCommands { }

public class ContextCommand<TContext>
{
    public interface WithArg<TArgument>
    {
        ValueTask Execute(TContext context, TArgument argument, CancellationToken cancel = default);
    }
}

public class Command1
    : ContextCommand<IViewModel>.WithArg<string>,
        ContextCommand<IViewModel>.WithArg<double>
{
    public ValueTask Execute(
        IViewModel context,
        string argument,
        CancellationToken cancel = default
    )
    {
        throw new NotImplementedException();
    }

    public ValueTask Execute(
        IViewModel context,
        double argument,
        CancellationToken cancel = default
    )
    {
        throw new NotImplementedException();
    }
}
