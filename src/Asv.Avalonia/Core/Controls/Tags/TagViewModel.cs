using Asv.Modeling;
using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public class TagViewModel(NavId id, ILoggerFactory loggerFactory)
    : ViewModelBase(id: id, loggerFactory)
{
    public AsvColorKind Color
    {
        get;
        set => SetField(ref field, value);
    }

    public string? Key
    {
        get;
        set => SetField(ref field, value);
    }

    public string? Value
    {
        get;
        set => SetField(ref field, value);
    }

    public MaterialIconKind? Icon
    {
        get;
        set => SetField(ref field, value);
    }

    protected override void Dispose(bool disposing)
    {
        // do nothing
    }
}
