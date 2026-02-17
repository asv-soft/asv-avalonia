namespace Asv.Avalonia.Example;

public class OpenControlsGalleryPageCommand(INavigationService nav)
    : OpenPageCommandBase(ControlsGalleryPageViewModel.PageId, nav)
{
    public override ICommandInfo Info => StaticInfo;

    #region Static

    public const string Id = $"{BaseId}.open.controlls_gallery";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.OpenControlsGalleryPageCommand_CommandInfo_Name,
        Description = RS.OpenControlsGalleryPageCommand_CommandInfo_Description,
        Icon = ControlsGalleryPageViewModel.PageIcon,
        IconColor = ControlsGalleryPageViewModel.PageIconColor,
        DefaultHotKey = null,
    };

    #endregion
}
