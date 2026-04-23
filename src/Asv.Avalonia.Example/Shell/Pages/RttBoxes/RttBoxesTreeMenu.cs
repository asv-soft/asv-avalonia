using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Example;

public class RttBoxesTreeMenu : TreePage
{
    public RttBoxesTreeMenu(ILoggerFactory loggerFactory)
        : base(
            RttBoxesPageViewModel.PageId,
            "Rtt boxes",
            RttBoxesPageViewModel.PageIcon,
            RttBoxesPageViewModel.PageId,
            NavId.Empty,
            loggerFactory
        ) { }
}
