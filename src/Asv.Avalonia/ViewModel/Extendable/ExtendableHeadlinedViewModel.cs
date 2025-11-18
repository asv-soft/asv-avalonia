using Avalonia.Media;
using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public abstract class ExtendableHeadlinedViewModel<TSelfInterface>(
    NavigationId id,
    ILoggerFactory loggerFactory
) : ExtendableViewModel<TSelfInterface>(id, loggerFactory), IHeadlinedViewModel
    where TSelfInterface : class
{
    public MaterialIconKind? Icon
    {
        get;
        set => SetField(ref field, value);
    }

    public AsvColorKind IconColor
    {
        get;
        set => SetField(ref field, value);
    }

    public string? Header
    {
        get;
        set => SetField(ref field, value);
    }

    public string? Description
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsVisible
    {
        get;
        set => SetField(ref field, value);
    } = true;

    public int Order
    {
        get;
        set => SetField(ref field, value);
    }
}
