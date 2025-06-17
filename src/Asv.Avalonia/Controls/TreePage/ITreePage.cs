using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public interface ITreePage : IHeadlinedViewModel
{
    NavigationId ParentId { get; }
    string? Status { get; }
    NavigationId NavigateTo { get; }
    ReactiveCommand NavigateCommand { get; }
}

public class TreePage : HeadlinedViewModel, ITreePage
{
    public TreePage(
        NavigationId id,
        string title,
        MaterialIconKind? icon,
        NavigationId navigateTo,
        NavigationId parentId,
        ILoggerFactory loggerFactory
    )
        : base(id, loggerFactory)
    {
        NavigateTo = navigateTo;
        ParentId = parentId;
        Header = title;
        Icon = icon;
    }

    public NavigationId NavigateTo { get; }
    public ReactiveCommand NavigateCommand { get; }
    public NavigationId ParentId { get; }

    public string? Status
    {
        get;
        set => SetField(ref field, value);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}
