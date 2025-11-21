using Avalonia;

namespace Asv.Avalonia.Example;

public partial class AboutExampleBlock : BaseExampleBlock
{
    public static readonly StyledProperty<string?> TitleProperty = AvaloniaProperty.Register<
        AboutExampleBlock,
        string?
    >(nameof(Title));

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly StyledProperty<string?> DescriptionProperty = AvaloniaProperty.Register<
        AboutExampleBlock,
        string?
    >(nameof(Description));

    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }
}
