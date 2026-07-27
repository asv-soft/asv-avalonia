using Asv.Modeling;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Example;

public class RttBoxesTreeMenu : TreePageMenuItem
{
    public RttBoxesTreeMenu(ILoggerFactory loggerFactory)
        : base(
            RttBoxesPageViewModel.PageId,
            RS.RttBoxesTreeMenu_Name,
            RttBoxesPageViewModel.PageIcon,
            new NavId(RttBoxesPageViewModel.PageId),
            NavId.Empty
        ) { }
}
