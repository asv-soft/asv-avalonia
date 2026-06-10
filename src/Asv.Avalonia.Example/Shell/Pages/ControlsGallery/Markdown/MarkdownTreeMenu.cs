using Asv.Modeling;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Example;

public class MarkdownTreeMenu : TreePage
{
    public MarkdownTreeMenu(ILoggerFactory loggerFactory)
        : base(
            MarkdownPageViewModel.PageId,
            GetTitle(),
            MarkdownPageViewModel.PageIcon,
            new NavId(MarkdownPageViewModel.PageId),
            NavId.Empty,
            loggerFactory
        ) { }

    private static string GetTitle()
    {
        return RS.ResourceManager.GetString("MarkdownPageViewModel_Title", RS.Culture)
            ?? "Markdown";
    }
}
