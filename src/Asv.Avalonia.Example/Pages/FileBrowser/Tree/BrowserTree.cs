using ObservableCollections;

namespace Asv.Avalonia.Example;

public class BrowserTree(IReadOnlyObservableList<IBrowserItem> flatList)
    : ObservableTree<IBrowserItem, NavigationId>(
        flatList,
        NavigationId.NormalizeTypeId("_"),
        x => x.Id,
        x => x.ParentId,
        BrowserItemComparer.Instance,
        (item, list, key, parent, comparer, transform, node) =>
            new BrowserNode(item, list, key, parent, comparer, transform, node)
    );
