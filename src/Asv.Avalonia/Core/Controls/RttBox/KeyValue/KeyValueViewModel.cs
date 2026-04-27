using Asv.Modeling;

namespace Asv.Avalonia;

public sealed class KeyValueViewModel(int index)
    : ViewModel($"{BaseIdPart}_{index}")
{
    public const string BaseIdPart = "key-value";

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
