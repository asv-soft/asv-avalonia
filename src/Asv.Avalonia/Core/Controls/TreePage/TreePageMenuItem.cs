using Asv.Modeling;
using Material.Icons;
using R3;

namespace Asv.Avalonia;

public class TreePageMenuItem : HeadlinedViewModel, ITreePageMenuItem
{
    public TreePageMenuItem(
        string typeId,
        string title,
        MaterialIconKind? icon,
        NavId navigateTo,
        NavId parentId,
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

    public NavId ParentId { get; }

    public TagViewModel? Status
    {
        get;
        set => SetField(ref field, value);
    }
}
