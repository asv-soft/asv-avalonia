using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public sealed class ViewSaveAllMenu : MenuItem
{
    public const string MenuId = $"{ViewSaveMenu.MenuId}.all";

    public ViewSaveAllMenu(ILoggerFactory loggerFactory)
        : base(MenuId, RS.ViewSaveAllMenu_Header, ViewMenu.MenuId)
    {
        Order = 1;
        Icon = SaveAllLayoutToFileCommand.StaticInfo.Icon;
        HotKey = cmd.GetHotKey(SaveAllLayoutToFileCommand.Id)?.Gesture;
        Command = new BindableAsyncCommand(SaveAllLayoutToFileCommand.Id, this);
    }
}
