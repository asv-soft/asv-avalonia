using Avalonia;
using Avalonia.Controls.Templates;
using Avalonia.Media;

namespace Asv.Avalonia.GeoMap;

public partial class AnnotationLayer
{
    #region ItemTemplate

    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<AnnotationLayer, IDataTemplate?>(nameof(ItemTemplate));

    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    #endregion

    #region Connector line

    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<AnnotationLayer, double>(
            nameof(StrokeThickness),
            defaultValue: 1
        );

    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public static readonly StyledProperty<IBrush?> StrokeProperty = AvaloniaProperty.Register<
        AnnotationLayer,
        IBrush?
    >(nameof(Stroke), defaultValue: Brushes.Violet);

    public IBrush? Stroke
    {
        get => GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    #endregion

    #region Source

    public static readonly StyledProperty<MapItemsControl?> SourceProperty =
        AvaloniaProperty.Register<AnnotationLayer, MapItemsControl?>(nameof(Source));

    public MapItemsControl? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    #endregion
}
