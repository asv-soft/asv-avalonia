using System.Composition;

namespace Asv.Avalonia.Example;

[ExportCommand]
[method: ImportingConstructor]
public class OpenControlsGalleryPageCommand(INavigationService nav)
    : OpenPageCommandBase(ControlsGalleryPageViewModel.PageId, nav)
{
    #region Static

    public const string Id = $"{BaseId}.open.controlls_gallery";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.OpenControlsGalleryCommand_Action_Title,
        Description = RS.OpenControlsGalleryCommand_Action_Description,
        Icon = ControlsGalleryPageViewModel.PageIcon,
        DefaultHotKey = null,
        Source = SystemModule.Instance,
    };

    #endregion
    public override ICommandInfo Info => StaticInfo;
}
