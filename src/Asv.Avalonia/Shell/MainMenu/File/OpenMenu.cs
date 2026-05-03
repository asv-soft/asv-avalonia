using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public class OpenMenu : MenuItem
{
    public const string MenuId = $"{MainMenuDefaultMenuExtender.Contract}.open";

    public OpenMenu(ILoggerFactory loggerFactory)
        : base(MenuId, RS.ShellView_Toolbar_Open)
    {
        Order = -100;
        Icon = OpenFileCommand.StaticInfo.Icon;
        HotKey = cmd.GetHotKey(OpenFileCommand.Id)?.Gesture;
        Command = new BindableAsyncCommand(OpenFileCommand.Id, this);
    }
}
