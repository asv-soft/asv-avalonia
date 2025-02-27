using Material.Icons;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public interface ITreePage : ITitledViewModel
{
    string? ParentId { get; }
    string? Status { get; }
    string? NavigateTo { get; }
    int Order { get; }
}

public class TreePage : TitledViewModel, ITreePage
{
    private string? _status;

    public TreePage(
        string id,
        string title,
        MaterialIconKind? icon,
        string? navigateTo,
        string? parentId = null
    )
        : base(id)
    {
        NavigateTo = navigateTo;
        ParentId = parentId;
        Title = title;
        Icon = icon;
    }

    public string? NavigateTo { get; }
    public int Order { get; } = 0;
    public string? ParentId { get; }

    public string? Status
    {
        get => _status;
        set => SetField(ref _status, value);
    }

    public override ValueTask<IRoutable> Navigate(string id)
    {
        return ValueTask.FromResult<IRoutable>(this);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}

public class TreePageMenu : ObservableTree<ITreePage, string>
{
    public TreePageMenu(IReadOnlyObservableList<ITreePage> flatList)
        : base(
            flatList,
            x => x.Id,
            x => x.ParentId,
            TreePageComparer.Instance,
            (item, list, key, parent, comparer, transform, node) =>
                new TreePageMenuNode(item, list, key, parent, comparer, transform, node)
        ) { }
}

public class TreePageMenuNode : ObservableTreeNode<ITreePage, string>
{
    public TreePageMenuNode(
        ITreePage baseItem,
        IReadOnlyObservableList<ITreePage> source,
        Func<ITreePage, string> keySelector,
        Func<ITreePage, string?> parentSelector,
        IComparer<ITreePage> comparer,
        CreateNodeDelegate<ITreePage, string> createNodeFactory,
        ObservableTreeNode<ITreePage, string>? parentNode
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

        return string.Compare(x.Id, y.Id, StringComparison.InvariantCultureIgnoreCase);
    }
}
