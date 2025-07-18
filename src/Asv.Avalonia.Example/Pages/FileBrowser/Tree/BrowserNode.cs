using System;
using System.Collections.Generic;
using ObservableCollections;

namespace Asv.Avalonia.Example;

public class BrowserNode(
    IBrowserItem baseItem,
    IReadOnlyObservableList<IBrowserItem> source,
    Func<IBrowserItem, string> keySelector,
    Func<IBrowserItem, string> parentSelector,
    IComparer<IBrowserItem> comparer,
    CreateNodeDelegate<IBrowserItem, string> factory,
    ObservableTreeNode<IBrowserItem, string>? parentNode = null
)
    : ObservableTreeNode<IBrowserItem, string>(
        baseItem,
        source,
        keySelector,
        parentSelector,
        comparer,
        factory,
        parentNode
    );
