using Asv.Modeling;

namespace Asv.Avalonia;

public interface IPageContext
{
    NavArgs NavArgs { get; }
    IUndoHistoryStore UndoStore { get; }
    ILayoutStore LayoutStore { get; }
}

public class PageContext(NavArgs navArgs, IUndoHistoryStore undoStore, ILayoutStore layoutStore)
    : IPageContext
{
    public NavArgs NavArgs => navArgs;
    public IUndoHistoryStore UndoStore => undoStore;
    public ILayoutStore LayoutStore => layoutStore;
}

public class NullPageContext : IPageContext
{
    public static IPageContext Instance { get; } = new NullPageContext();

    public NavArgs NavArgs { get; } = NavArgs.Empty;
    public IUndoHistoryStore UndoStore => NullUndoHistoryStore.Instance;
    public ILayoutStore LayoutStore => NullLayoutStore.Instance;
}
