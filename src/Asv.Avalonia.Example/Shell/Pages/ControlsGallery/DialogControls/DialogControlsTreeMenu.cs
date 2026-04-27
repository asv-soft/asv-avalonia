using Asv.Modeling;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Example;

public class DialogControlsTreeMenu : TreePage
{
    public DialogControlsTreeMenu(ILoggerFactory loggerFactory)
        : base(
            DialogControlsPageViewModel.PageId,
            RS.DialogControlsPageViewModel_Title,
            DialogControlsPageViewModel.PageIcon,
            new NavId(DialogControlsPageViewModel.PageId),
            NavId.Empty,
            loggerFactory
        ) { }
}
