using ObservableCollections;

namespace Asv.Avalonia.Example;

public class BrowserTree : ObservableTree<IBrowserItem, NavigationId>
{
    public BrowserTree(IReadOnlyObservableList<IBrowserItem> flatList)
        : base(
            flatList,
            NavigationId.NormalizeTypeId("_"),
            x => x.Id,
            x => x.ParentId,
            BrowserItemComparer.Instance,
            (item, list, key, parent, comparer, transform, node) =>
                new BrowserNode(item, list, key, parent, comparer, transform, node)
        ) { }
}
