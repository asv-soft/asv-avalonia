namespace Asv.Avalonia.Example;

public class OpenMapTestPageCommand(INavigationService nav)
    : OpenPageCommandBase(MapTestPageViewModel.PageId, nav)
{
    public override ICommandInfo Info => StaticInfo;

    #region Static

    public const string Id = $"{BaseId}.open.map_test";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.OpenMapTestPageCommand_CommandInfo_Name,
        Description = RS.OpenMapTestPageCommand_CommandInfo_Description,
        Icon = MapTestPageViewModel.PageIcon,
        DefaultHotKey = null,
    };

    #endregion
}
