﻿using System.Composition;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

[ExportMainMenu]
public class EditUndoMenu : MenuItem
{
    public const string MenuId = $"{EditMenu.MenuId}.undo";

    [ImportingConstructor]
    public EditUndoMenu(IShellHost host, ILoggerFactory loggerFactory)
        : base(MenuId, RS.UndoCommand_CommandInfo_Name, loggerFactory, EditMenu.MenuId)
    {
        Command = new BindableAsyncCommand(UndoCommand.Id, this);
        Order = 0;
    }
}
