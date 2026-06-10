using System.Globalization;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Layout;
using Avalonia.Media;
using Material.Icons;
using Material.Icons.Avalonia;

namespace Asv.Avalonia;

/// <summary>
/// Displays a small Markdown subset with headings, lists, Material icons, and Asv colors.
/// </summary>
public class MarkdownViewer : ContentControl
{
    public static readonly StyledProperty<string?> TextProperty = AvaloniaProperty.Register<
        MarkdownViewer,
        string?
    >(nameof(Text));

    public static readonly StyledProperty<TextWrapping> TextWrappingProperty =
        AvaloniaProperty.Register<MarkdownViewer, TextWrapping>(
            nameof(TextWrapping),
            TextWrapping.Wrap
        );

    public static readonly StyledProperty<double> BlockSpacingProperty = AvaloniaProperty.Register<
        MarkdownViewer,
        double
    >(nameof(BlockSpacing), 8);

    public static readonly StyledProperty<double> ListItemSpacingProperty =
        AvaloniaProperty.Register<MarkdownViewer, double>(nameof(ListItemSpacing), 4);

    public static readonly StyledProperty<double> ListIndentProperty = AvaloniaProperty.Register<
        MarkdownViewer,
        double
    >(nameof(ListIndent), 16);

    public static readonly StyledProperty<double> IconSizeProperty = AvaloniaProperty.Register<
        MarkdownViewer,
        double
    >(nameof(IconSize), 16);

    public MarkdownViewer()
    {
        Rebuild();
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public TextWrapping TextWrapping
    {
        get => GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }

    public double BlockSpacing
    {
        get => GetValue(BlockSpacingProperty);
        set => SetValue(BlockSpacingProperty, value);
    }

    public double ListItemSpacing
    {
        get => GetValue(ListItemSpacingProperty);
        set => SetValue(ListItemSpacingProperty, value);
    }

    public double ListIndent
    {
        get => GetValue(ListIndentProperty);
        set => SetValue(ListIndentProperty, value);
    }

    public double IconSize
    {
        get => GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (
            change.Property == TextProperty
            || change.Property == TextWrappingProperty
            || change.Property == BlockSpacingProperty
            || change.Property == ListItemSpacingProperty
            || change.Property == ListIndentProperty
            || change.Property == IconSizeProperty
        )
        {
            Rebuild();
        }
    }

    private void Rebuild()
    {
        var root = new StackPanel { Spacing = BlockSpacing };
        var lines = NormalizeLines(Text);
        var index = 0;

        while (index < lines.Length)
        {
            var line = lines[index];
            if (string.IsNullOrWhiteSpace(line))
            {
                index++;
                continue;
            }

            if (TryReadHeading(line, out var level, out var headingText))
            {
                root.Children.Add(CreateHeading(headingText, level));
                index++;
                continue;
            }

            if (TryReadListItem(line, out _))
            {
                var items = new List<MarkdownListItem>();
                while (index < lines.Length && TryReadListItem(lines[index], out var item))
                {
                    items.Add(item);
                    index++;
                }

                root.Children.Add(CreateList(items));
                continue;
            }

            var paragraph = new List<string> { line.Trim() };
            index++;
            while (
                index < lines.Length
                && !string.IsNullOrWhiteSpace(lines[index])
                && !TryReadHeading(lines[index], out _, out _)
                && !TryReadListItem(lines[index], out _)
            )
            {
                paragraph.Add(lines[index].Trim());
                index++;
            }

            root.Children.Add(CreateInlineTextBlock(string.Join(" ", paragraph), null));
        }

        Content = root;
    }

    private static string[] NormalizeLines(string? text)
    {
        return (text ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
    }

    private static bool TryReadHeading(string line, out int level, out string text)
    {
        var trimmed = line.TrimStart();
        level = 0;
        text = string.Empty;

        while (level < trimmed.Length && level < 3 && trimmed[level] == '#')
        {
            level++;
        }

        if (level == 0 || level >= trimmed.Length || !char.IsWhiteSpace(trimmed[level]))
        {
            level = 0;
            return false;
        }

        text = trimmed[(level + 1)..].Trim();
        return text.Length > 0;
    }

    private static bool TryReadListItem(string line, out MarkdownListItem item)
    {
        var trimmed = line.TrimStart();
        item = default;

        if (trimmed.StartsWith("- ", StringComparison.Ordinal))
        {
            item = new MarkdownListItem(MarkdownListKind.Unordered, "\u2022", trimmed[2..].Trim());
            return true;
        }

        if (trimmed.StartsWith("* ", StringComparison.Ordinal))
        {
            item = new MarkdownListItem(MarkdownListKind.Unordered, "\u2022", trimmed[2..].Trim());
            return true;
        }

        var digitCount = 0;
        while (digitCount < trimmed.Length && char.IsDigit(trimmed[digitCount]))
        {
            digitCount++;
        }

        if (
            digitCount == 0
            || digitCount + 1 >= trimmed.Length
            || trimmed[digitCount] != '.'
            || !char.IsWhiteSpace(trimmed[digitCount + 1])
        )
        {
            return false;
        }

        item = new MarkdownListItem(
            MarkdownListKind.Ordered,
            string.Create(CultureInfo.InvariantCulture, $"{trimmed[..digitCount]}."),
            trimmed[(digitCount + 2)..].Trim()
        );
        return true;
    }

    private SelectableTextBlock CreateHeading(string text, int level)
    {
        var styleClass = level switch
        {
            1 => "h1",
            2 => "h2",
            _ => "h3",
        };
        var block = CreateInlineTextBlock(text, styleClass);
        block.Margin = level == 1 ? new Thickness(0, 8, 0, 0) : new Thickness(0, 4, 0, 0);
        return block;
    }

    private Control CreateList(IReadOnlyList<MarkdownListItem> items)
    {
        var panel = new StackPanel
        {
            Spacing = ListItemSpacing,
            Margin = new Thickness(ListIndent, 0, 0, 0),
        };

        foreach (var item in items)
        {
            var row = new Grid { ColumnSpacing = 8 };
            row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            row.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));

            var marker = new SelectableTextBlock
            {
                Text = item.Marker,
                MinWidth = item.Kind == MarkdownListKind.Ordered ? 24 : 12,
                TextAlignment = TextAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
            };
            Grid.SetColumn(marker, 0);
            row.Children.Add(marker);

            var content = CreateInlineTextBlock(item.Text, null);
            Grid.SetColumn(content, 1);
            row.Children.Add(content);

            panel.Children.Add(row);
        }

        return panel;
    }

    private SelectableTextBlock CreateInlineTextBlock(string text, string? styleClass)
    {
        var block = new SelectableTextBlock { TextWrapping = TextWrapping };
        if (styleClass is not null)
        {
            block.Classes.Add(styleClass);
        }

        block.Inlines ??= [];
        AddInlineElements(block.Inlines, text, AsvColorKind.None, styleClass);
        return block;
    }

    private void AddInlineElements(
        InlineCollection inlines,
        string text,
        AsvColorKind color,
        string? styleClass
    )
    {
        var index = 0;
        while (index < text.Length)
        {
            var tokenStart = FindNextTokenStart(text, index);
            if (tokenStart < 0)
            {
                AddTextInline(inlines, text[index..], color, styleClass);
                return;
            }

            if (tokenStart > index)
            {
                AddTextInline(inlines, text[index..tokenStart], color, styleClass);
            }

            var tokenEnd = text.IndexOf(']', tokenStart + 1);
            if (tokenEnd < 0)
            {
                AddTextInline(inlines, text[tokenStart..], color, styleClass);
                return;
            }

            var token = text[(tokenStart + 1)..tokenEnd];
            var attributes = ParseAttributes(token);
            if (attributes.TryGetValue("icon", out var iconName))
            {
                if (Enum.TryParse<MaterialIconKind>(iconName, true, out var icon))
                {
                    var iconColor = TryReadColor(attributes, out var parsedColor)
                        ? parsedColor
                        : color;
                    AddIconInline(inlines, icon, iconColor);
                    index = tokenEnd + 1;
                    continue;
                }
            }

            if (attributes.ContainsKey("color"))
            {
                var closeStart = FindClosingColorTag(text, tokenEnd + 1);
                if (closeStart >= 0 && TryReadColor(attributes, out var parsedColor))
                {
                    var colorTextStart = tokenEnd + 1;
                    AddInlineElements(
                        inlines,
                        text[colorTextStart..closeStart],
                        parsedColor,
                        styleClass
                    );
                    index = closeStart + "[/color]".Length;
                    continue;
                }
            }

            var literalEnd = tokenEnd + 1;
            AddTextInline(inlines, text[tokenStart..literalEnd], color, styleClass);
            index = tokenEnd + 1;
        }
    }

    private void AddIconInline(
        InlineCollection inlines,
        MaterialIconKind iconKind,
        AsvColorKind color
    )
    {
        var icon = new MaterialIcon
        {
            Kind = iconKind,
            Width = IconSize,
            Height = IconSize,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(1, 0, 3, -2),
        };
        if (color != AsvColorKind.None)
        {
            AsvPallete.SetColor(icon, color);
        }

        inlines.Add(new InlineUIContainer(icon));
    }

    private static void AddTextInline(
        InlineCollection inlines,
        string text,
        AsvColorKind color,
        string? styleClass
    )
    {
        var unescapedText = Unescape(text);
        if (unescapedText.Length == 0)
        {
            return;
        }

        if (color == AsvColorKind.None)
        {
            inlines.Add(new Run(unescapedText));
            return;
        }

        var block = new SelectableTextBlock { Text = unescapedText };
        if (styleClass is not null)
        {
            block.Classes.Add(styleClass);
        }

        AsvPallete.SetColor(block, color);
        inlines.Add(new InlineUIContainer(block));
    }

    private static int FindNextTokenStart(string text, int start)
    {
        for (var i = start; i < text.Length; i++)
        {
            if (text[i] == '[' && !IsEscaped(text, i))
            {
                return i;
            }
        }

        return -1;
    }

    private static int FindClosingColorTag(string text, int start)
    {
        const string closeTag = "[/color]";
        var index = start;
        while (index < text.Length)
        {
            var closeStart = text.IndexOf(closeTag, index, StringComparison.OrdinalIgnoreCase);
            if (closeStart < 0)
            {
                return -1;
            }

            if (!IsEscaped(text, closeStart))
            {
                return closeStart;
            }

            index = closeStart + closeTag.Length;
        }

        return -1;
    }

    private static bool IsEscaped(string text, int index)
    {
        var slashCount = 0;
        for (var i = index - 1; i >= 0 && text[i] == '\\'; i--)
        {
            slashCount++;
        }

        return slashCount % 2 == 1;
    }

    private static string Unescape(string text)
    {
        if (text.IndexOf('\\') < 0)
        {
            return text;
        }

        var result = new StringBuilder(text.Length);
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == '\\' && i + 1 < text.Length && IsEscapable(text[i + 1]))
            {
                result.Append(text[i + 1]);
                i++;
                continue;
            }

            result.Append(text[i]);
        }

        return result.ToString();
    }

    private static bool IsEscapable(char value)
    {
        return value is '[' or ']' or '\\';
    }

    private static Dictionary<string, string> ParseAttributes(string token)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var part in token.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = part.IndexOf('=');
            if (separator <= 0 || separator == part.Length - 1)
            {
                continue;
            }

            result[part[..separator].Trim()] = part[(separator + 1)..].Trim();
        }

        return result;
    }

    private static bool TryReadColor(
        IReadOnlyDictionary<string, string> attributes,
        out AsvColorKind color
    )
    {
        color = AsvColorKind.None;
        if (!attributes.TryGetValue("color", out var value))
        {
            return false;
        }

        var hasAnyFlag = false;
        foreach (var part in value.Split('|', StringSplitOptions.RemoveEmptyEntries))
        {
            if (Enum.TryParse<AsvColorKind>(part.Trim(), true, out var parsed))
            {
                color |= parsed;
                hasAnyFlag = true;
            }
        }

        return hasAnyFlag;
    }

    private readonly record struct MarkdownListItem
    {
        public MarkdownListItem(MarkdownListKind kind, string marker, string text)
        {
            Kind = kind;
            Marker = marker;
            Text = text;
        }

        public MarkdownListKind Kind { get; }
        public string Marker { get; }
        public string Text { get; }
    }

    private enum MarkdownListKind
    {
        Unordered,
        Ordered,
    }
}
