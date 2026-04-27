using Asv.Modeling;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Example;

public class MapControlsTreeMenu : TreePage
{
    public MapControlsTreeMenu(ILoggerFactory loggerFactory)
        : base(
            MapControlsPageViewModel.PageId,
            RS.MapControlsPageViewModel_Title,
            MapControlsPageViewModel.PageIcon,
            new NavId(MapControlsPageViewModel.PageId),
            NavId.Empty,
            loggerFactory
        ) { }
}
