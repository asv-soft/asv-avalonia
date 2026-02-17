using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

[ExportMainMenu]
public sealed class ViewSaveMenu : MenuItem
{
    public const string MenuId = $"{ViewMenu.MenuId}.save";

    public ViewSaveMenu(ILoggerFactory loggerFactory, ICommandService cmd)
        : base(MenuId, RS.ViewSaveMenu_Header, loggerFactory, ViewMenu.MenuId)
    {
        Order = 0;
        Icon = SaveLayoutToFileCommand.StaticInfo.Icon;
        HotKey = cmd.GetHotKey(SaveLayoutToFileCommand.Id)?.Gesture;
        Command = new BindableAsyncCommand(SaveLayoutToFileCommand.Id, this);
    }
}
