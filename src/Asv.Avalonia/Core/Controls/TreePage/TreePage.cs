using Asv.Modeling;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public class TreePage : HeadlinedViewModel, ITreePage
{
    public TreePage(
        string typeId,
        string title,
        MaterialIconKind? icon,
        NavId navigateTo,
        NavId parentId,
        ILoggerFactory loggerFactory,
        TagViewModel? status = null
    )
        : base(typeId)
    {
        NavigateTo = navigateTo;
        ParentId = parentId;
        Header = title;
        Icon = icon;
        Status = status;
    }

    public NavId NavigateTo { get; }
    public ReactiveCommand NavigateCommand { get; }
    public NavId ParentId { get; }

    public TagViewModel? Status
    {
        get;
        set => SetField(ref field, value);
    }
}
