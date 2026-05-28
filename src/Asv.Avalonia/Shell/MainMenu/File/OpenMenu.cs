using Asv.Cfg;
using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Hosting;
using R3;

namespace Asv.Avalonia;

public class OpenMenu : MenuItem
{
    public const string MenuId = "main-menu-open";

    private readonly IFileAssociationService _files;
    private readonly OpenFileDialogDesktopPrefab _dialog;
    private readonly IHostEnvironment _path;
    private readonly IConfiguration _config;

    public OpenMenu(
        IFileAssociationService files,
        IDialogService dialogs,
        IHostEnvironment path,
        IConfiguration config,
        IHotKeyService hotKeys
    )
        : base(MenuId, RS.ShellView_Toolbar_Open)
    {
        _files = files;
        _dialog = dialogs.GetDialogPrefab<OpenFileDialogDesktopPrefab>();
        _path = path;
        _config = config;

        Order = 0;
        Icon = MaterialIconKind.FileOutline;
        HotKey = hotKeys[OpenFileAction.Id];
        Command = new ReactiveCommand(OpenAsync).DisposeItWith(Disposable);
    }

    private async ValueTask OpenAsync(Unit unit, CancellationToken cancel)
    {
        await OpenFileAction.OpenAsync(_files, _dialog, _path, _config, cancel);
    }
}
