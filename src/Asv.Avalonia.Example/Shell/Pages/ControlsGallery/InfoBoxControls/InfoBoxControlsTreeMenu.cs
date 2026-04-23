using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Example;

public class InfoBoxControlsTreeMenu : TreePage
{
    public InfoBoxControlsTreeMenu(ILoggerFactory loggerFactory)
        : base(
            InfoBoxControlsPageViewModel.PageId,
            RS.InfoBoxControlsPageViewModel_Title,
            InfoBoxControlsPageViewModel.PageIcon,
            InfoBoxControlsPageViewModel.PageId,
            NavId.Empty,
            loggerFactory
        ) { }
}
