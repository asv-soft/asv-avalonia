using ObservableCollections;

namespace Asv.Avalonia;

public class MenuNode : ObservableTreeNode<IMenuItem, string>
{
    public MenuNode(
        IMenuItem baseItem,
        IReadOnlyObservableList<IMenuItem> source,
        Func<IMenuItem, string> keySelector,
        Func<IMenuItem, string?> parentSelector,
        IComparer<IMenuItem> comparer,
        CreateNodeDelegate<IMenuItem, string> factory,
        ObservableTreeNode<IMenuItem, string>? parentNode = null
    )
        : base(baseItem, source, keySelector, parentSelector, comparer, factory, parentNode) { }
}
