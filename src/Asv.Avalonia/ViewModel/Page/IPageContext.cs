using Asv.Modeling;

namespace Asv.Avalonia;

public interface IPageContext
{
    NavArgs NavArgs { get; }
    IUndoHistoryStore UndoStore { get; }
}

public class PageContext(NavArgs navArgs, IUndoHistoryStore undoStore) : IPageContext
{
    public NavArgs NavArgs { get; } = navArgs;
    public IUndoHistoryStore UndoStore { get; } = undoStore;
}

public class NullPageContext : IPageContext
{
    public static IPageContext Instance { get; } = new NullPageContext();
    
    public NavArgs NavArgs { get; } = NavArgs.Empty;
    public IUndoHistoryStore UndoStore => NullHistoryStore.Instance;
}