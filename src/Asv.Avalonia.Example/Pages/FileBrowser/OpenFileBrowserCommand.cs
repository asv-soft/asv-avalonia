using System.Composition;

namespace Asv.Avalonia.Example;

[ExportCommand]
[method: ImportingConstructor]
public class OpenFileBrowserCommand(INavigationService nav)
    : OpenPageCommandBase(FileBrowserViewModel.PageId, nav)
{
    #region Static

    public const string Id = $"{BaseId}.open.{FileBrowserViewModel.PageId}";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = "Open file browser",
        Description = "Command that opens FTP file browser",
        Icon = FileBrowserViewModel.PageIcon,
        DefaultHotKey = null,
        Source = SystemModule.Instance,
    };

    #endregion

    public override ICommandInfo Info => StaticInfo;
}
