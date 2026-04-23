using Asv.Avalonia;
using Asv.Modeling;
using ObservableCollections;

namespace Asv.Avalonia;

public class MenuNode : ObservableTreeNode<IMenuItem, NavId>
{
    public MenuNode(
        IMenuItem baseItem,
        IReadOnlyObservableList<IMenuItem> source,
        Func<IMenuItem, NavId> keySelector,
        Func<IMenuItem, NavId> parentSelector,
        IComparer<IMenuItem> comparer,
        CreateNodeDelegate<IMenuItem, NavId> factory,
        ObservableTreeNode<IMenuItem, NavId>? parentNode = null
    )
        : base(baseItem, source, keySelector, parentSelector, comparer, factory, parentNode) { }
}
