using Asv.Modeling;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Example;

public class HistoricalControlsTreeMenu : TreePage
{
    public HistoricalControlsTreeMenu(ILoggerFactory loggerFactory)
        : base(
            HistoricalControlsPageViewModel.PageId,
            RS.HistoricalControlsPageViewModel_Title,
            HistoricalControlsPageViewModel.PageIcon,
            new NavId(HistoricalControlsPageViewModel.PageId),
            NavId.Empty,
            loggerFactory
        ) { }
}
