using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

[ExportMainMenu]
public class OpenMenu : MenuItem
{
    public const string MenuId = $"{ExportMainMenuAttribute.Contract}.open";

    public OpenMenu(ILoggerFactory loggerFactory, ICommandService cmd)
        : base(MenuId, RS.ShellView_Toolbar_Open, loggerFactory)
    {
        Order = -100;
        Icon = OpenFileCommand.StaticInfo.Icon;
        HotKey = cmd.GetHotKey(OpenFileCommand.Id)?.Gesture;
        Command = new BindableAsyncCommand(OpenFileCommand.Id, this);
    }
}
