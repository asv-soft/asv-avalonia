using Material.Icons;

namespace Asv.Avalonia.Example;

public class MarkdownPageViewModel : ControlsGallerySubPage
{
    public const string PageId = "markdown-example";
    public const MaterialIconKind PageIcon = MaterialIconKind.CodeBraces;

    public MarkdownPageViewModel()
        : this(NullTreeSubPageContext<ControlsGalleryPageViewModel>.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
        SetParent(DesignTime.Shell);
    }

    public MarkdownPageViewModel(ITreeSubPageContext<IControlsGalleryPage> context)
        : base(PageId, context) { }

    public string Documentation { get; } =
        """
            # [icon=CodeBraces;color=Info5;] MarkdownViewer

            MarkdownViewer displays a safe, small Markdown subset inside native Avalonia controls. Use it for help text, inline documentation, release notes, and short status explanations where full HTML Markdown is unnecessary.

            ## Basic usage

            - Add the control to XAML and bind Text to a string property.
            - Keep the source text trusted or sanitize it before assigning it.
            - The renderer ignores unsupported Markdown instead of executing markup.

            ### XAML

            <asv:MarkdownViewer Text="{Binding Documentation}" TextWrapping="Wrap" />

            ## Markdown tooltips

            Use MarkdownToolTipConverter when a tooltip must render the same subset instead of plain text.

            - Single binding: ToolTip.Tip="{Binding Description, Converter={x:Static asv:MarkdownToolTipConverter.Instance}}"
            - Header plus description: use MultiBinding with Header first and Description second.
            - ConverterParameter can be used as a static header for a single Description binding.

            ## Supported block syntax

            - Headings: # Title, ## Section, and ### Subsection.
            - Unordered lists: - item or * item.
            - Ordered lists: 1. item, 2. item, and so on.
            - Normal paragraphs: any non-empty line that is not a heading or list item.

            ## Inline tokens

            - Icon token: \[icon=CheckCircle;color=Success;]
            - Text color token: \[color=Warning;]warning text\[/color]
            - Bold text: **important text**
            - Attribute separator: use semicolon after each key-value pair.
            - Color flags: combine AsvColorKind values with |, for example Error|Blink.

            ## Rendered examples

            - [icon=CheckCircle;color=Success;] Success item with a Material icon.
            - [icon=AlertCircle;color=Warning;] Warning item using AsvColorKind.Warning.
            - [icon=Information;color=Info5;] Informational item using Info5.
            - Text can contain [color=Success;]green success fragments[/color], [color=Warning;]warning fragments[/color], and [color=Error|Blink;]blinking error fragments[/color].
            - Inline formatting can combine [color=Warning;]**bold warning fragments**[/color] with normal text.

            1. [icon=Cog;color=Info7;] Ordered item with an icon.
            2. [icon=CodeBraces;color=Info12;] Ordered item with another palette color.

            ## Escaping

            Prefix a token opening bracket with a backslash when the documentation must show the syntax instead of rendering it.

            - Literal icon token: \[icon=CheckCircle;color=Success;]
            - Literal color block: \[color=Success;]text\[/color]
            - Literal bold marker: \**not bold\**
            """;
}
