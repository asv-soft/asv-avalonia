namespace Asv.Avalonia.Example.Plugin.PluginExample;

public class OpenExamplePageCommand(INavigationService nav)
    : OpenPageCommandBase(ExamplePageViewModel.PageId, nav)
{
    public override ICommandInfo Info => StaticInfo;

    #region Static

    public const string Id = $"{BaseId}.open.example";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = "Example",
        Description = "Open Example Page",
        Icon = ExamplePageViewModel.PageIcon,
        DefaultHotKey = null,
    };

    #endregion
}
