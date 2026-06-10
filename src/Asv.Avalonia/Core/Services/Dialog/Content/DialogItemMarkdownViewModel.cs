namespace Asv.Avalonia;

public class DialogItemMarkdownViewModel : DialogViewModelBase
{
    public const string DialogId = $"{BaseId}-item-markdown";

    public DialogItemMarkdownViewModel()
        : this("# Markdown")
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public DialogItemMarkdownViewModel(string markdownText)
        : base(DialogId)
    {
        MarkdownText = markdownText;
    }

    public string MarkdownText { get; }

    public override IEnumerable<IViewModel> GetChildren()
    {
        return [];
    }
}
