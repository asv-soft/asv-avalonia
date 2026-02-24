using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Example;

public class MapControlsTreeMenu : TreePage
{
    public MapControlsTreeMenu(ILoggerFactory loggerFactory)
        : base(
            MapControlsPageViewModel.PageId,
            RS.MapControlsPageViewModel_Title,
            MapControlsPageViewModel.PageIcon,
            MapControlsPageViewModel.PageId,
            NavigationId.Empty,
            loggerFactory
        ) { }
}
