using Asv.Modeling;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Example;

public class WorkspaceTreeMenu : TreePageMenuItem
{
    public WorkspaceTreeMenu(ILoggerFactory loggerFactory)
        : base(
            WorkspacePageViewModel.PageId,
            "Workspace",
            WorkspacePageViewModel.PageIcon,
            new NavId(WorkspacePageViewModel.PageId),
            NavId.Empty,
            new TagViewModel("status")
            {
                Value = "Active",
                Color = AsvColorKind.Success | AsvColorKind.Blink,
            }
        ) { }
}
