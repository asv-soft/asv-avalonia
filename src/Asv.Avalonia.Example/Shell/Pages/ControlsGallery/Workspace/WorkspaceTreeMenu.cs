using Asv.Modeling;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Example;

public class WorkspaceTreeMenu : TreePage
{
    public WorkspaceTreeMenu(ILoggerFactory loggerFactory)
        : base(
            WorkspacePageViewModel.PageId,
            "Workspace",
            WorkspacePageViewModel.PageIcon,
            new NavId(WorkspacePageViewModel.PageId),
            NavId.Empty,
            loggerFactory,
            new TagViewModel("status")
            {
                Value = "Active",
                Color = AsvColorKind.Success | AsvColorKind.Blink,
            }
        ) { }
}
