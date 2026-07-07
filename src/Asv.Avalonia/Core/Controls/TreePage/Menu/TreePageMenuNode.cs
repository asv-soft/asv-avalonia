using Asv.Modeling;
using ObservableCollections;

namespace Asv.Avalonia;

public class TreePageMenuNode : ObservableTreeNode<ITreePageMenuItem, NavId>
{
    public TreePageMenuNode(
        ITreePageMenuItem baseItem,
        IReadOnlyObservableList<ITreePageMenuItem> source,
        Func<ITreePageMenuItem, NavId> keySelector,
        Func<ITreePageMenuItem, NavId> parentSelector,
        IComparer<ITreePageMenuItem> comparer,
        CreateNodeDelegate<ITreePageMenuItem, NavId> createNodeFactory,
        ObservableTreeNode<ITreePageMenuItem, NavId>? parentNode
    )
        : base(
            baseItem,
            source,
            keySelector,
            parentSelector,
            comparer,
            createNodeFactory,
            parentNode
        ) { }

    public bool IsExpanded
    {
        get;
        set => SetField(ref field, value);
    } = true;
}
