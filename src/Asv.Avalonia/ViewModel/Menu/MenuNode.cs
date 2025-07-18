using Asv.Avalonia;
using ObservableCollections;

namespace Asv.Avalonia;

public class MenuNode : ObservableTreeNode<IMenuItem, NavigationId>
{
    public MenuNode(
        IMenuItem baseItem,
        IReadOnlyObservableList<IMenuItem> source,
        Func<IMenuItem, NavigationId> keySelector,
        Func<IMenuItem, NavigationId> parentSelector,
        IComparer<IMenuItem> comparer,
        CreateNodeDelegate<IMenuItem, NavigationId> factory,
        ObservableTreeNode<IMenuItem, NavigationId>? parentNode = null
    )
        : base(baseItem, source, keySelector, parentSelector, comparer, factory, parentNode) { }
}
