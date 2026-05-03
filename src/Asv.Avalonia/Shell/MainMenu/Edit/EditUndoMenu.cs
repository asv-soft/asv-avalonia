using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public class EditUndoMenu : MenuItem
{
    public const string MenuId = $"{EditMenu.MenuId}.undo";

    public EditUndoMenu(ILoggerFactory loggerFactory)
        : base(MenuId, RS.UndoCommand_CommandInfo_Name, EditMenu.MenuId)
    {
        Icon = UndoCommand.StaticInfo.Icon;
        UndoAction.Create 
        Command = new BindableAsyncCommand(UndoCommand.Id, this);
        Order = 0;
    }
}
