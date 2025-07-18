using ObservableCollections;

namespace Asv.Avalonia;

public class TreePageMenuNode : ObservableTreeNode<ITreePage, NavigationId>
{
    public TreePageMenuNode(
        ITreePage baseItem,
        IReadOnlyObservableList<ITreePage> source,
        Func<ITreePage, NavigationId> keySelector,
        Func<ITreePage, NavigationId> parentSelector,
        IComparer<ITreePage> comparer,
        CreateNodeDelegate<ITreePage, NavigationId> createNodeFactory,
        ObservableTreeNode<ITreePage, NavigationId>? parentNode
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
