using System.Security.Cryptography;
using System.Text;
using Asv.Cfg;
using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Hosting;
using R3;

namespace Asv.Avalonia;

public class OpenMenuExtender(
    IFileAssociationService files,
    IDialogService dialogs,
    IHostEnvironment path,
    IConfiguration config,
    IHotKeyService hotKeys
) : IExtensionFor<IShell>
{
    private const string OpenDialogMenuId = $"{OpenMenu.MenuId}-dialog";
    private const string RecentMenuIdPrefix = $"{OpenMenu.MenuId}-recent-";

    public void Extend(IShell context, CompositeDisposable contextDispose)
    {
        var dialog = dialogs.GetDialogPrefab<OpenFileDialogDesktopPrefab>();
        var openMenu = new MenuItem(OpenDialogMenuId, RS.ShellView_Toolbar_Open, OpenMenu.MenuId)
        {
            Icon = MaterialIconKind.FileOutline,
            Order = 0,
            Command = new ReactiveCommand(
                async (_, cancel) =>
                {
                    await OpenFileAction.OpenAsync(files, dialog, path, config, cancel);
                    RefreshRecentFiles(context, contextDispose);
                }
            ),
        }.DisposeItWith(contextDispose);
        openMenu.BindHotKey(hotKeys, OpenFileAction.Id);

        context.MainMenu.Add(openMenu);
        RefreshRecentFiles(context, contextDispose);
    }

    private void RefreshRecentFiles(IShell context, CompositeDisposable contextDispose)
    {
        foreach (
            var item in context
                .MainMenu.Where(x => x.Id.TypeId.StartsWith(RecentMenuIdPrefix))
                .ToArray()
        )
        {
            context.MainMenu.Remove(item);
        }

        var recentFiles = OpenFileAction.GetRecentFiles(config).Where(Path.Exists).ToArray();

        for (var i = 0; i < recentFiles.Length; i++)
        {
            var filePath = recentFiles[i];
            var menu = new MenuItem(
                GetRecentMenuId(filePath),
                Path.GetFileName(filePath),
                OpenMenu.MenuId
            )
            {
                Description = filePath,
                Icon = MaterialIconKind.FileOutline,
                Order = 10 + i,
                Command = new ReactiveCommand(
                    async (_, cancel) =>
                    {
                        await OpenRecentFile(filePath, cancel);
                        RefreshRecentFiles(context, contextDispose);
                    }
                ),
            }.DisposeItWith(contextDispose);

            context.MainMenu.Add(menu);
        }
    }

    private async ValueTask OpenRecentFile(string filePath, CancellationToken cancel)
    {
        if (cancel.IsCancellationRequested || !Path.Exists(filePath))
        {
            return;
        }

        await files.Open(filePath);
        OpenFileAction.AddRecentFile(config, filePath);
    }

    private static string GetRecentMenuId(string filePath)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(filePath));
        return $"{RecentMenuIdPrefix}{Convert.ToHexString(hash)}";
    }
}
