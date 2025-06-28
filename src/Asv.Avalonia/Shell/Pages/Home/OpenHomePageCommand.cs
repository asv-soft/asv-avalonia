using System.Composition;
using Material.Icons;

namespace Asv.Avalonia;

[ExportCommand]
public class OpenHomePageCommand : StatelessCommand
{
    private INavigationService _nav;

    #region Static

    public const string Id = $"{BaseId}.open.{HomePageViewModel.PageId}";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.OpenHomePageCommand_CommandInfo_Name,
        Description = RS.OpenHomePageCommand_CommandInfo_Description,
        Icon = MaterialIconKind.Home,
        DefaultHotKey = "Ctrl+H",
        Source = SystemModule.Instance,
    };

    #endregion

    [ImportingConstructor]
    public OpenHomePageCommand(INavigationService nav)
    {
        _nav = nav;
    }

    public override ICommandInfo Info => StaticInfo;

    protected override async ValueTask<CommandArg?> InternalExecute(
        CommandArg newValue,
        CancellationToken cancel
    )
    {
        try
        {
            await _nav.GoHomeAsync();
            return newValue;
        }
        catch (Exception ex)
        {
            return null;
        }
    }
}
