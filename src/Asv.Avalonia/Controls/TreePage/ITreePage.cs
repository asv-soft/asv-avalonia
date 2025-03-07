using Material.Icons;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public interface ITreePage : IHeadlinedViewModel
{
    NavigationId ParentId { get; }
    string? Status { get; }
    NavigationId NavigateTo { get; }
}

public class TreePage : HeadlinedViewModel, ITreePage
{
    private string? _status;

    public TreePage(
        NavigationId id,
        string title,
        MaterialIconKind? icon,
        NavigationId navigateTo,
        NavigationId parentId
    )
        : base(id)
    {
        NavigateTo = navigateTo;
        ParentId = parentId;
        Header = title;
        Icon = icon;
    }

    public NavigationId NavigateTo { get; }
    public NavigationId ParentId { get; }

    public string? Status
    {
        get => _status;
        set => SetField(ref _status, value);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}

public class TreePageMenu : ObservableTree<ITreePage, NavigationId>
{
    public TreePageMenu(IReadOnlyObservableList<ITreePage> flatList)
        : base(
            flatList,
            NavigationId.Empty,
            x => x.Id,
            x => x.ParentId,
            TreePageComparer.Instance,
            (item, list, key, parent, comparer, transform, node) =>
                new TreePageMenuNode(item, list, key, parent, comparer, transform, node)
        ) { }
}

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
}

public class TreePageComparer : IComparer<ITreePage>
{
    public static IComparer<ITreePage> Instance { get; } = new TreePageComparer();

    private TreePageComparer() { }

    public int Compare(ITreePage? x, ITreePage? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        if (y is null)
        {
            return 1;
        }

        if (x is null)
        {
            return -1;
        }

        var orderComparison = x.Order.CompareTo(y.Order);
        if (orderComparison != 0)
        {
            return orderComparison;
        }

        return x.Id.CompareTo(y.Id);
    }
}
