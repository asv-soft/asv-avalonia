using System.Composition;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

[ExportMainMenu]
public sealed class ViewSaveAllMenu : MenuItem
{
    public const string MenuId = $"{ViewSaveMenu.MenuId}.all";

    [ImportingConstructor]
    public ViewSaveAllMenu(
        ILayoutService layoutService,
        ILoggerFactory loggerFactory,
        ICommandService cmd
    )
        : base(MenuId, "Save All", layoutService, loggerFactory, ViewMenu.MenuId)
    {
        Order = 1;
        Icon = SaveAllLayoutCommand.StaticInfo.Icon;
        HotKey = cmd.GetHotKey(SaveAllLayoutCommand.Id)?.Gesture;
        Command = new BindableAsyncCommand(SaveAllLayoutCommand.Id, this);
    }
}
