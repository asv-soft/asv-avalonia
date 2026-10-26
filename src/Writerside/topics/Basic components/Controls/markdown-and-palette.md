# Markdown and Palette

<img src="guide-markdown.png" dark-src="guide-markdown.png" alt="Native MarkdownViewer in the running gallery" thumbnail="true"/>

Open Home → Controls gallery → Markdown to inspect the supported syntax and rendered examples. This is native Avalonia content, rather than an HTML page.

`MarkdownViewer` renders short application documentation as native Avalonia controls. It is useful
for property descriptions, status explanations, exception details, and tooltips. It implements a small
syntax deliberately; it is not an HTML browser or a general Markdown engine.

## Show a native document

The control can be constructed directly, without a view-model registration:

```c#
using Asv.Avalonia;

var viewer = new MarkdownViewer
{
    MaxWidth = 480,
    BlockSpacing = 8,
    ListItemSpacing = 4,
    ListIndent = 16,
    IconSize = 16,
    Text = """
        # Receiver status

        [icon=CheckCircle;color=Success;] **Ready to connect**

        - Select a receiver.
        - Verify the connection settings.

        [color=Warning;]Changing settings may interrupt the connection.[/color]
        """
};
```

Assign `viewer` to a host's `Content`, or declare the control in AXAML. With
`xmlns:asv="clr-namespace:Asv.Avalonia;assembly=Asv.Avalonia"` and a view model exposing `Description`:

```xml
<asv:MarkdownViewer Text="{Binding Description}"
                    MaxWidth="480"
                    BlockSpacing="8"
                    TextWrapping="Wrap" />
```

Changing `Text`, wrapping, spacing, indentation, or icon size rebuilds the native document. Use it
for bounded text rather than continuously replacing a large document on every telemetry update.
The normal application theme must be loaded for heading classes and palette brushes to match the UI.

## Supported syntax

| Syntax | Result |
|--------|--------|
| `# Title`, `## Section`, `### Subsection` | Headings using the `h1`, `h2`, and `h3` classes. |
| A blank line | Separates blocks. |
| `- item` or `* item` | An unordered list item. |
| `1. item` | An ordered item; the supplied numeric marker is retained. |
| `**important**` | Bold inline text. |
| `[icon=CheckCircle;]` | A Material icon named by `MaterialIconKind`. |
| `[icon=CheckCircle;color=Success;]` | A Material icon with palette classes. |
| `[color=Warning;]text[/color]` | Text colored through a palette foreground brush. |

Adjacent ordinary lines form one paragraph with spaces between them. Lists are flat: indentation
does not create nested lists. Headings require whitespace after their hash prefix and support only
levels one through three.

Do not author tables, hyperlinks, images, code fences, raw HTML, single-asterisk emphasis, or nested
lists expecting their usual Markdown behavior. This subset has no hyperlink navigation or image
loading. If an explanation requires those features, use an appropriate document or browser surface.

Token attributes are semicolon-separated `key=value` pairs. Names are case-insensitive. Use the
actual Material icon enum name, and use `AsvColorKind` names for colors. Malformed or unrecognized
tokens can remain literal text; validate authored content in the running control.

## Escaping literal syntax

A backslash escapes brackets, asterisks, and backslashes. These examples are the actual text supplied
to the viewer; in an ordinary C# string, escape the backslash again or use a raw string literal.

```text
\[icon=CheckCircle;color=Success;]
\[color=Success;]literal token\[/color]
\**not bold\**
```

Avoid nested color spans: the parser finds the next closing color token and does not implement a
nested tag stack. Bold text can contain icons and a color span, but complex markup should be split
into simple, readable phrases.

## Markdown tooltips

Use `MarkdownToolTipConverter` when a normal tooltip binding should display the same native subset.
The single-value form uses the bound value as description and an optional converter parameter as
header:

```xml
<TextBlock Text="Connection">
    <ToolTip.Tip>
        <Binding Path="Description"
                 Converter="{x:Static asv:MarkdownToolTipConverter.Instance}"
                 ConverterParameter="Connection" />
    </ToolTip.Tip>
</TextBlock>
```

For independently bound header and description values:

```xml
<TextBlock Text="{Binding Header}">
    <ToolTip.Tip>
        <MultiBinding Converter="{x:Static asv:MarkdownToolTipConverter.Instance}">
            <Binding Path="Header" />
            <Binding Path="Description" />
        </MultiBinding>
    </ToolTip.Tip>
</TextBlock>
```

An empty or whitespace description returns no tooltip, even when a header is present. The converter
creates a `MarkdownViewer` with a default maximum width of 360, 14-pixel icons, and a level-three
heading. Its `HeaderLevel` is clamped to 1–3. For different defaults, declare a separate converter
instance in local resources instead of mutating the shared static `Instance`. Conversion is one-way.

## Palette colors and effects

`AsvColorKind` is a flags enum. Choose a semantic status (`Unknown`, `Error`, `Warning`, `Success`)
or an informational color (`Info1` through `Info20`). Effects include `Blink`, `BlinkOnce`, `Fadein`,
`FadeinBlink`, and `Fadeout`; size flags include `Smallest`, `Small`, `Medium`, and `Big`.

For a styled Avalonia element, `AsvPalette.Color` synchronizes flags to lowercase classes. For
example, `Success | Blink` produces `success` and `blink`:

```c#
using Asv.Avalonia;
using Avalonia.Controls;

var indicator = new TextBlock { Text = "Connection lost" };
AsvPalette.SetColor(indicator, AsvColorKind.Error | AsvColorKind.Blink);
```

The attached property adds and removes classes; the loaded styles determine how those classes
look for a particular control. Combine one base color with applicable effects rather than several
competing status colors. Changing to `None` removes classes associated with the previous flags.

Markdown icon tokens apply these palette flags to the embedded `MaterialIcon`. Color spans behave
differently: the viewer resolves the corresponding `AsvForeground...Brush` and assigns it to text
runs. It strips animation and size flags when choosing that brush. Consequently,
`[color=Error|Blink;]text[/color]` selects the error foreground but does **not** animate the text.
Use an icon or a separately styled control when animation is required.

## Use descriptions consistently

[Property Editor](property-editor.md) descriptions and [dashboard tiles](dashboard-workspace.md)
can reuse these tokens. Store application text in localization resources when the surrounding
feature is localized, preserving token and enum identifiers. Keep a textual status alongside color
so the explanation remains understandable without relying on hue alone.

The running example's **Controls Gallery → Markdown** shows the supported blocks, token syntax,
and tooltips. Use the palette gallery to compare status colors and effects in the actual theme.
`MarkdownViewerTest` and `MarkdownToolTipConverterTest` contain executable examples of supported
formatting and fallback behavior.
