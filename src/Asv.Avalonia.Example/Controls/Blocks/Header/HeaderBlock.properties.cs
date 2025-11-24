using Avalonia;
using Avalonia.Controls;

namespace Asv.Avalonia.Example;

public partial class HeaderBlock : BaseExampleBlock
{
    public static readonly StyledProperty<string?> TitleH1Property = AvaloniaProperty.Register<
        HeaderBlock,
        string?
    >(nameof(TitleH1));

    public string? TitleH1
    {
        get => GetValue(TitleH1Property);
        set => SetValue(TitleH1Property, value);
    }

    public static readonly StyledProperty<string?> TitleH2Property = AvaloniaProperty.Register<
        HeaderBlock,
        string?
    >(nameof(TitleH2));

    public string? TitleH2
    {
        get => GetValue(TitleH2Property);
        set => SetValue(TitleH2Property, value);
    }

    public static readonly StyledProperty<string?> TitleH3Property = AvaloniaProperty.Register<
        HeaderBlock,
        string?
    >(nameof(TitleH3));

    public string? TitleH3
    {
        get => GetValue(TitleH3Property);
        set => SetValue(TitleH3Property, value);
    }

    public static readonly StyledProperty<string?> DescriptionProperty = AvaloniaProperty.Register<
        HeaderBlock,
        string?
    >(nameof(Description), defaultValue: null);

    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }
}
