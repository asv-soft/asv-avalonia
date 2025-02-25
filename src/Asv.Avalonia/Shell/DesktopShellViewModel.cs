using System.Composition;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using R3;

namespace Asv.Avalonia;

public class DesktopShellViewModelConfig { }

[Export(ShellId, typeof(IShell))]
public class DesktopShellViewModel : ShellViewModel
{
    public const string ShellId = "shell.desktop";

    private readonly IContainerHost _ioc;

    [ImportingConstructor]
    public DesktopShellViewModel(IContainerHost ioc)
        : base(ioc, ShellId)
    {
        _ioc = ioc;
        var wnd = ioc.GetExport<ShellWindow>();
        wnd.DataContext = this;
        if (
            Application.Current?.ApplicationLifetime
            is not IClassicDesktopStyleApplicationLifetime lifetime
        )
        {
            throw new Exception(
                "ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime"
            );
        }

        OpenFileCommand = new ReactiveCommand<string>(OpenFile);

        // Устанавливаем окно как drop target
        DragDrop.SetAllowDrop(wnd, true);
        wnd.AddHandler(DragDrop.DropEvent, OnFileDrop);
        lifetime.MainWindow = wnd;
        lifetime.MainWindow.Show();
    }

    #region Drop

    private ReactiveCommand<string> OpenFileCommand { get; }

    #endregion

    private void OnFileDrop(object? sender, DragEventArgs e)
    {
        var data = e.Data;

        if (data.Contains(DataFormats.Files))
        {
            var fileData = data.Get(DataFormats.Files);
            if (fileData is IEnumerable<IStorageItem> items)
            {
                foreach (var file in items)
                {
                    var path = file.TryGetLocalPath();
                    if (Path.Exists(path))
                    {
                        OpenFileCommand.Execute(path);
                    }
                }
            }
        }
    }

    protected override ValueTask CloseAsync(CancellationToken cancellationToken)
    {
        if (
            Application.Current != null
            && Application.Current.ApplicationLifetime
                is IClassicDesktopStyleApplicationLifetime lifetime
        )
        {
            lifetime.Shutdown();
        }

        return ValueTask.CompletedTask;
    }

    private void OpenFile(string filePath)
    {
        // Передаем файл сервису обработки файлов
    }
}
