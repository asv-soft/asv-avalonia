using System.Composition;

namespace Asv.Avalonia.Plugins;

[ExportCommand]
[method: ImportingConstructor]
public class OpenPluginsMarketCommand(INavigationService nav)
    : OpenPageCommandBase(PluginsMarketViewModel.PageId, nav)
{
    #region Static

    public const string Id = $"{BaseId}.open.{PluginsMarketViewModel.PageId}";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.OpenPluginsMarketCommand_Name,
        Description = RS.OpenPluginsMarketCommand_Description,
        Icon = PluginsMarketViewModel.PageIcon,
        DefaultHotKey = null,
        Source = SystemModule.Instance,
    };

    #endregion
    public override ICommandInfo Info => StaticInfo;
}
