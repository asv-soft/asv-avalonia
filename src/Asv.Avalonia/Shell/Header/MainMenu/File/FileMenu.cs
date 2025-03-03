using System.Composition;
using Asv.Common;

namespace Asv.Avalonia;

[ExportMainMenuAttribute]
public class FileMenu : MenuItem
{
    public const string MenuId = "shell.menu.file";

    public FileMenu()
        : base(MenuId, RS.ShellView_Toolbar_File) { }
}
