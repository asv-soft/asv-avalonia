using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Example;

public class DialogControlsTreeMenu : TreePage
{
    public DialogControlsTreeMenu(ILoggerFactory loggerFactory)
        : base(
            DialogControlsPageViewModel.PageId,
            RS.DialogControlsPageViewModel_Title,
            DialogControlsPageViewModel.PageIcon,
            DialogControlsPageViewModel.PageId,
            NavigationId.Empty,
            loggerFactory
        ) { }
}
