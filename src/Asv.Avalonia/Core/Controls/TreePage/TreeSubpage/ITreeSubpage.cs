using Asv.Modeling;
using ObservableCollections;

namespace Asv.Avalonia;

public interface ITreeSubpage : IViewModel
{
    MenuTree MenuView { get; }
    ObservableList<IMenuItem> Menu { get; }
}

public interface ITreeSubPageContext<out TContext>
    where TContext : class, IPage
{
    NavArgs Args { get; }
    TContext Context { get; }
}

public class TreeSubPageContext<TContext>(NavArgs args, TContext context) : ITreeSubPageContext<TContext>
    where TContext : class, ITreePageViewModel
{
    public NavArgs Args { get; } = args;
    public TContext Context { get; } = context;
}

public class NullTreeSubPageContext<TContext> : ITreeSubPageContext<TContext>
    where TContext : class, ITreePageViewModel, new()
{
    public static ITreeSubPageContext<TContext> Instance { get; } = new NullTreeSubPageContext<TContext>();
    
    public NavArgs Args => default;
    public TContext Context { get; } = new();
}
