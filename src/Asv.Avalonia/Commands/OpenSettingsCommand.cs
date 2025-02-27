using System.Composition;
using Avalonia.Input;
using Material.Icons;

namespace Asv.Avalonia;

[ExportCommand]
[method: ImportingConstructor]
public class OpenSettingsCommand() : OpenPageCommandBase(SettingsPageViewModel.PageId)
{
    #region Static

    public const string Id = $"{BaseId}.open.settings";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = "Settings",
        Description = "Open settings",
        Icon = MaterialIconKind.Cog,
        DefaultHotKey = KeyGesture.Parse("Ctrl+F1"),
        Source = SystemModule.Instance,
    };

    #endregion
    public override ICommandInfo Info => StaticInfo;
}
