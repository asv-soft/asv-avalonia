using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public sealed class KeyValueViewModel : DisposableViewModel
{
    public static string BaseIdPart = "key-value";

    public KeyValueViewModel()
        : this(DesignTime.LoggerFactory) { }

    public KeyValueViewModel(ILoggerFactory loggerFactory)
        : base(new NavigationId(BaseIdPart, Guid.NewGuid().ToString()), loggerFactory) { }

    public string? Header
    {
        get;
        set => SetField(ref field, value);
    }

    public string? ValueString
    {
        get;
        set => SetField(ref field, value);
    }

    public string? UnitSymbol
    {
        get;
        set => SetField(ref field, value);
    }
}
