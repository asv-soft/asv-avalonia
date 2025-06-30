﻿using System.Composition;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

[ExportMainMenu]
public class ToolsHomeMenu : MenuItem
{
    public const string MenuId = $"{ToolsMenu.MenuId}.home";

    [ImportingConstructor]
    public ToolsHomeMenu(ILoggerFactory loggerFactory)
        : base(MenuId, RS.ToolsMenu_Home, loggerFactory, ToolsMenu.MenuId)
    {
        Command = new BindableAsyncCommand(OpenHomePageCommand.Id, this);
    }
}
