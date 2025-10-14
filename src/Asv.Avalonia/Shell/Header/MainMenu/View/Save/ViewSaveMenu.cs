using System.Composition;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

[ExportMainMenu]
public sealed class ViewSaveMenu : MenuItem
{
    public const string MenuId = $"{ViewMenu.MenuId}.save";

    [ImportingConstructor]
    public ViewSaveMenu(
        ILayoutService layoutService,
        ILoggerFactory loggerFactory,
        ICommandService cmd
    )
        : base(MenuId, "Save", layoutService, loggerFactory, ViewMenu.MenuId)
    {
        Order = 0;
        Icon = SaveLayoutCommand.StaticInfo.Icon;
        HotKey = cmd.GetHotKey(SaveLayoutCommand.Id)?.Gesture;
        Command = new BindableAsyncCommand(SaveLayoutCommand.Id, this);
    }
}
