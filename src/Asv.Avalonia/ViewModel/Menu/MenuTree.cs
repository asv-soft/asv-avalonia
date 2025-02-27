using ObservableCollections;

namespace Asv.Avalonia;

public class MenuTree : ObservableTree<IMenuItem, string>
{
    public MenuTree(IReadOnlyObservableList<IMenuItem> flatList)
        : base(
            flatList,
            x => x.Id,
            x => x.ParentId,
            MenuComparer.Instance,
            (item, list, key, parent, comparer, transform, node) =>
                new MenuNode(item, list, key, parent, comparer, transform, node)
        ) { }
}
