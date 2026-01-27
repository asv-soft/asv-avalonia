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
        ILoggerFactory loggerFactory,
        TagViewModel? status = null
    )
        : base(id, loggerFactory)
    {
        NavigateTo = navigateTo;
        ParentId = parentId;
        Header = title;
        Icon = icon;
        Status = status;
    }

    public NavigationId NavigateTo { get; }
    public ReactiveCommand NavigateCommand { get; }
    public NavigationId ParentId { get; }

    public TagViewModel? Status
    {
        get;
        set => SetField(ref field, value);
    }
}
