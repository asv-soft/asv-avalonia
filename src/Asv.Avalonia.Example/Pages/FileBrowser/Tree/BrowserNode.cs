using System;
using System.Collections.Generic;
using ObservableCollections;

namespace Asv.Avalonia.Example;

public class BrowserNode : ObservableTreeNode<IBrowserItem, NavigationId>
{
    public BrowserNode(
        IBrowserItem baseItem,
        IReadOnlyObservableList<IBrowserItem> source,
        Func<IBrowserItem, NavigationId> keySelector,
        Func<IBrowserItem, NavigationId> parentSelector,
        IComparer<IBrowserItem> comparer,
        CreateNodeDelegate<IBrowserItem, NavigationId> factory,
        ObservableTreeNode<IBrowserItem, NavigationId>? parentNode = null
    )
        : base(baseItem, source, keySelector, parentSelector, comparer, factory, parentNode) { }
}
