using System.Composition;

namespace Asv.Avalonia.Plugins;

[ExportCommand]
[method: ImportingConstructor]
public sealed class OpenPluginsMarketCommand(INavigationService nav)
    : OpenPageCommandBase(PluginsMarketPageViewModel.PageId, nav)
{
    #region Static

    public const string Id = $"{BaseId}.open.{PluginsMarketPageViewModel.PageId}";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.OpenPluginsMarketCommand_CommandInfo_Name,
        Description = RS.OpenPluginsMarketCommand_CommandInfo_Description,
        Icon = PluginsMarketPageViewModel.PageIcon,
        DefaultHotKey = null,
        Source = PluginManagerModule.Instance,
    };

    #endregion
    public override ICommandInfo Info => StaticInfo;
}
