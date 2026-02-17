namespace Asv.Avalonia.Plugins;

public sealed class OpenInstalledPluginsCommand(INavigationService nav)
    : OpenPageCommandBase(InstalledPluginsPageViewModel.PageId, nav)
{
    #region Static

    public const string Id = $"{BaseId}.open.{InstalledPluginsPageViewModel.PageId}";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.OpenInstalledPluginsCommand_CommandInfo_Name,
        Description = RS.OpenInstalledPluginsCommand_CommandInfo_Description,
        Icon = InstalledPluginsPageViewModel.PageIcon,
        DefaultHotKey = null,
        Source = PluginManagerModule.Instance,
    };

    #endregion
    public override ICommandInfo Info => StaticInfo;
}
