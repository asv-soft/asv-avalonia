using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public class TreePage : HeadlinedViewModel, ITreePage
{
    public TreePage(
        NavigationId id,
        string title,
        MaterialIconKind? icon,
        NavigationId navigateTo,
        NavigationId parentId,
        ILayoutService layoutService,
        ILoggerFactory loggerFactory
    )
        : base(id, layoutService, loggerFactory)
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
}
