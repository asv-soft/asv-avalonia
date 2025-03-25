using Material.Icons;

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

public class TagViewModel(NavigationId id) : ViewModelBase(id)
{
    private TagType? _tagType;
    private string? _key;
    private string? _value;
    private MaterialIconKind? _icon;

    public TagType? TagType
    {
        get => _tagType;
        set => SetField(ref _tagType, value);
    }

    public string? Key
    {
        get => _key;
        set => SetField(ref _key, value);
    }

    public string? Value
    {
        get => _value;
        set => SetField(ref _value, value);
    }

    public MaterialIconKind? Icon
    {
        get => _icon;
        set => SetField(ref _icon, value);
    }

    protected override void Dispose(bool disposing)
    {
        // do nothing
    }
}
