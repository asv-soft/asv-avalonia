using Material.Icons;
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
