using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Automation;
using Material.Icons;
using Material.Icons.Avalonia;

namespace Asv.Avalonia.IO;

public sealed class PacketDetailsTextBlock : UserControl
{
    public static readonly StyledProperty<IReadOnlyList<PacketFieldSnapshot>> FieldsProperty =
        AvaloniaProperty.Register<PacketDetailsTextBlock, IReadOnlyList<PacketFieldSnapshot>>(
            nameof(Fields), Array.Empty<PacketFieldSnapshot>());

    public static readonly StyledProperty<PacketSnapshot?> PacketProperty =
        AvaloniaProperty.Register<PacketDetailsTextBlock, PacketSnapshot?>(nameof(Packet));

    public PacketSnapshot? Packet
    {
        get => GetValue(PacketProperty);
        set => SetValue(PacketProperty, value);
    }

    public static readonly StyledProperty<ICommand?> ToChartCommandProperty =
        AvaloniaProperty.Register<PacketDetailsTextBlock, ICommand?>(nameof(ToChartCommand));

    public ICommand? ToChartCommand
    {
        get => GetValue(ToChartCommandProperty);
        set => SetValue(ToChartCommandProperty, value);
    }

    public IReadOnlyList<PacketFieldSnapshot> Fields
    {
        get => GetValue(FieldsProperty);
        set => SetValue(FieldsProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(UserControl);

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == FieldsProperty || change.Property == ToChartCommandProperty || change.Property == PacketProperty)
        {
            var rows = new StackPanel();
            AppendFields(rows, Fields, 0);
            Content = rows;
        }
    }

    private void AppendFields(StackPanel rows, IReadOnlyList<PacketFieldSnapshot> fields, int depth)
    {
        foreach (var field in fields)
        {
            var row = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto"), MinHeight = 18 };
            var inlines = new InlineCollection();
            inlines.Add(new Run(new string(' ', depth * 2) + field.Name + ": "));
            var value = new Run(field.Value);
            var resource = field.Type.EndsWith('?') && field.Value == "∅" ? "AsvForegroundUnknownBrush"
                : GetValueBrush(field.Type);
            if (resource != null)
            {
                value.Bind(TextElement.ForegroundProperty, new DynamicResourceExtension(resource));
            }
            inlines.Add(value);
            inlines.Add(new Run(" " + field.Units));
            if (ToChartCommand != null && Packet != null && PacketFieldChart.TryGetNumber(field, out _))
            {
                var button = new Button
                {
                    Content = new MaterialIcon { Kind = MaterialIconKind.ChartBoxPlusOutline, Width = 16, Height = 16 },
                    Command = ToChartCommand,
                    CommandParameter = new PacketChartRequest(Packet, field),
                    MinHeight = 18,
                    Padding = new Thickness(4, 0),
                    Margin = new Thickness(6, 0, 0, 0),
                };
                ToolTip.SetTip(button, RS.PacketInspector_ToChart);
                AutomationProperties.SetName(button, RS.PacketInspector_ToChart);
                button.Bind(ThemeProperty, new DynamicResourceExtension("TransparentButton"));
                button.VerticalAlignment = VerticalAlignment.Top;
                Grid.SetColumn(button, 1);
                row.Children.Add(button);
            }
            var text = new SelectableTextBlock
            {
                Inlines = inlines,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 18,
                VerticalAlignment = VerticalAlignment.Center,
            };
            row.Children.Add(text);
            rows.Children.Add(row);
            AppendFields(rows, field.Children, depth + 1);
        }
    }

    private static string? GetValueBrush(string type) => type.TrimEnd('?').ToLowerInvariant() switch
    {
        "int8" or "int16" or "int32" or "int64" or "uint8" or "uint16" or "uint32" or "uint64"
            or "half" or "float" or "double" => "AsvForegroundWarningBrush",
        "string" or "char" => "AsvForegroundSuccessBrush",
        "bool" or "boolean" => "AsvForegroundInfo1Brush",
        "date" or "date-time" or "time" or "time-span" => "AsvForegroundInfo2Brush",
        _ => null,
    };
}
