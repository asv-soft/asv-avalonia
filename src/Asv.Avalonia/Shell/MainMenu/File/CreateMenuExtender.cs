using Asv.Cfg;
using Asv.Common;
using Microsoft.Extensions.Hosting;
using R3;

namespace Asv.Avalonia;

public class CreateMenuExtender(
    IFileAssociationService files,
    IDialogService dialogs,
    IHostEnvironment path,
    IConfiguration config
) : IExtensionFor<IShell>
{
    public const string StaticId = "ext.shell.menu.create";

    string Asv.Modeling.ISupportId<string>.Id => StaticId;

    public void Extend(IShell context, CompositeDisposable contextDispose)
    {
        foreach (var file in files.SupportedFiles.Where(x => x.CanCreate))
        {
            var menuId = $"{CreateMenu.MenuId}-{file.Id.Replace('.', '-')}";
            var menu = new MenuItem(menuId, file.Title, CreateMenu.MenuId)
            {
                Icon = file.Icon,
                Command = new ReactiveCommand((_, cancel) => CreateAsync(file, cancel)),
            }.DisposeItWith(contextDispose);

            context.MainMenu.Add(menu);
        }
    }

    private async ValueTask CreateAsync(FileTypeInfo fileType, CancellationToken cancel)
    {
        if (cancel.IsCancellationRequested)
        {
            return;
        }

        var lastDirectory = path.ContentRootPath;
        var cfg = config.Get<FileCommandConfig>();
        if (cfg.LastDirectory != null && Directory.Exists(cfg.LastDirectory))
        {
            lastDirectory = cfg.LastDirectory;
        }

        var dialog = dialogs.GetDialogPrefab<SaveFileDialogDesktopPrefab>();
        var filePath = await dialog.ShowDialogAsync(
            new SaveFileDialogPayload
            {
                Title = RS.CreateFileCommand_SelectFile,
                TypeFilter = fileType.Extension,
                InitialDirectory = lastDirectory,
            }
        );

        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        await files.Create(filePath, fileType);
        OpenFileAction.AddRecentFile(config, filePath);
    }
}
