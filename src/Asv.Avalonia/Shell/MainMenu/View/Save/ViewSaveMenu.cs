namespace Asv.Avalonia;

public sealed class ViewSaveMenu : MenuItem
{
    public const string MenuId = $"{ViewMenu.MenuId}.save";

    public ViewSaveMenu(IHotKeyService svc)
        : base(MenuId, RS.ViewSaveMenu_Header, ViewMenu.MenuId)
    {
        Order = 0;
         
        Icon = SaveLayoutToFileCommand.StaticInfo.Icon;
        HotKey = cmd.GetHotKey(SaveLayoutToFileCommand.Id)?.Gesture;
        Command = new BindableAsyncCommand(SaveLayoutToFileCommand.Id, this);
    }
}
