using System.Composition;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

[ExportMainMenu]
public sealed class ViewSaveMenu : MenuItem
{
    public const string MenuId = $"{ViewMenu.MenuId}.save";

    [ImportingConstructor]
    public ViewSaveMenu(ILoggerFactory loggerFactory, ICommandService cmd)
        : base(MenuId, "Save", loggerFactory, ViewMenu.MenuId)
    {
        Order = 0;
        Icon = SaveLayoutCommand.StaticInfo.Icon;
        HotKey = cmd.GetHotKey(SaveLayoutCommand.Id)?.Gesture;
        Command = new BindableAsyncCommand(SaveLayoutCommand.Id, this);
    }
}
