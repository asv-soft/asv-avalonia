using System.Composition;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

[ExportMainMenu]
public sealed class ViewSaveAllMenu : MenuItem
{
    public const string MenuId = $"{ViewSaveMenu.MenuId}.all";

    [ImportingConstructor]
    public ViewSaveAllMenu(ILoggerFactory loggerFactory, ICommandService cmd)
        : base(MenuId, RS.ViewSaveAllMenu_Header, loggerFactory, ViewMenu.MenuId)
    {
        Order = 1;
        Icon = SaveAllLayoutToFileCommand.StaticInfo.Icon;
        HotKey = cmd.GetHotKey(SaveAllLayoutToFileCommand.Id)?.Gesture;
        Command = new BindableAsyncCommand(SaveAllLayoutToFileCommand.Id, this);
    }
}
