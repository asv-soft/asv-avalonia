using Asv.Modeling;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Example;

public class WorkspaceTreeMenu : TreePageMenuItem
{
    public WorkspaceTreeMenu(ILoggerFactory loggerFactory)
        : base(
            WorkspacePageViewModel.PageId,
            RS.WorkspaceTreeMenu_Name,
            WorkspacePageViewModel.PageIcon,
            new NavId(WorkspacePageViewModel.PageId),
            NavId.Empty,
            new TagViewModel("status")
            {
                Value = RS.WorkspaceTreeMenu_StatusTag_Value,
                Color = AsvColorKind.Success | AsvColorKind.Blink,
            }
        ) { }
}
