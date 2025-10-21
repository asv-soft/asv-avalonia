using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public enum TagType
{
    Error,
    Warning,
    Info,
    Success,
    Unknown,
    Info2,
    Info3,
    Info4,
}

public class TagViewModel(NavigationId id, ILoggerFactory loggerFactory)
    : ViewModelBase(id: id, loggerFactory)
{
    public TagType? TagType
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
