using Asv.Modeling;
using ObservableCollections;

namespace Asv.Avalonia;

public class TreePageMenuNode : ObservableTreeNode<ITreePage, NavId>
{
    public TreePageMenuNode(
        ITreePage baseItem,
        IReadOnlyObservableList<ITreePage> source,
        Func<ITreePage, NavId> keySelector,
        Func<ITreePage, NavId> parentSelector,
        IComparer<ITreePage> comparer,
        CreateNodeDelegate<ITreePage, NavId> createNodeFactory,
        ObservableTreeNode<ITreePage, NavId>? parentNode
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
