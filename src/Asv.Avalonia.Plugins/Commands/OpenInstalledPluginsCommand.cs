using System.Composition;

namespace Asv.Avalonia.Plugins;

[ExportCommand]
[method: ImportingConstructor]
public class OpenInstalledPluginsCommand(INavigationService nav)
    : OpenPageCommandBase(InstalledPluginsViewModel.PageId, nav)
{
    #region Static

    public const string Id = $"{BaseId}.open.{InstalledPluginsViewModel.PageId}";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = "Installed Plugins",
        Description = "Open installed plugins page",
        Icon = InstalledPluginsViewModel.PageIcon,
        DefaultHotKey = null,
        Source = SystemModule.Instance,
    };

    #endregion
    public override ICommandInfo Info => StaticInfo;
}
