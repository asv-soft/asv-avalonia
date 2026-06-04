using Asv.Modeling;

namespace Asv.Avalonia;

public sealed class KeyValueViewModel : ViewModel
{
    public KeyValueViewModel(int index)
        : this($"row-{index}") { }

    public KeyValueViewModel(string typeId)
        : base(typeId) { }

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
