using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Metadata;
using R3;

namespace Asv.Avalonia;

public partial class DockControl
{
    public static StyledProperty<ReactiveCommand> UnSplitAllCommandProperty =
        AvaloniaProperty.Register<DockControl, ReactiveCommand>(nameof(UnSplitAllCommand));

    public ReactiveCommand UnSplitAllCommand
    {
        get => GetValue(UnSplitAllCommandProperty);
        set => SetValue(UnSplitAllCommandProperty, value);
    }

    public static readonly StyledProperty<IDataTemplate?> TabControlStripItemTemplateProperty =
        AvaloniaProperty.Register<DockControl, IDataTemplate?>(nameof(TabControlStripItemTemplate));

    [InheritDataTypeFromItems("ItemsSource")]
    public IDataTemplate? TabControlStripItemTemplate
    {
        get => GetValue(TabControlStripItemTemplateProperty);
        set => SetValue(TabControlStripItemTemplateProperty, value);
    }

    public static readonly StyledProperty<int> MaxSplitAmountProperty = AvaloniaProperty.Register<
        DockControl,
        int
    >(nameof(MaxSplitAmount), 4);

    public int MaxSplitAmount
    {
        get => GetValue(MaxSplitAmountProperty);
        set => SetValue(MaxSplitAmountProperty, value);
    }

    public static readonly StyledProperty<IBrush> BorderHighLightColorProperty =
        AvaloniaProperty.Register<DockControl, IBrush>(
            nameof(BorderHighLightColor),
            Brushes.LightBlue
        );

    public IBrush BorderHighLightColor
    {
        get => GetValue(BorderHighLightColorProperty);
        set => SetValue(BorderHighLightColorProperty, value);
    }
}