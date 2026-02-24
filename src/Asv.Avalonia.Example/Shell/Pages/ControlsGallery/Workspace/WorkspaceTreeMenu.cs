using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Example;

public class WorkspaceTreeMenu : TreePage
{
    public WorkspaceTreeMenu(ILoggerFactory loggerFactory)
        : base(
            WorkspacePageViewModel.PageId,
            "Workspace",
            WorkspacePageViewModel.PageIcon,
            WorkspacePageViewModel.PageId,
            NavigationId.Empty,
            loggerFactory,
            new TagViewModel("status", loggerFactory)
            {
                Value = "Active",
                Color = AsvColorKind.Success | AsvColorKind.Blink,
            }
        ) { }
}
