using System.Composition;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public class DesktopShellViewModelConfig { }

[Export(ShellId, typeof(IShell))]
public class DesktopShellViewModel : ShellViewModel
{
    public const string ShellId = "shell.desktop";
    private readonly IContainerHost _ioc;

    [ImportingConstructor]
    public DesktopShellViewModel(IContainerHost ioc, ILoggerFactory loggerFactory)
        : base(ioc, loggerFactory, ShellId)
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

        // Set window as the drop target
        DragDrop.SetAllowDrop(wnd, true);
        wnd.AddHandler(DragDrop.DropEvent, OnFileDrop);

        WindowSateIconKind.Value =
            wnd.WindowState == WindowState.FullScreen
                ? MaterialIconKind.CollapseAll
                : MaterialIconKind.Maximize;

        WindowStateHeader.Value =
            wnd.WindowState == WindowState.FullScreen
                ? RS.ShellView_WindowControlButton_Minimize
                : RS.ShellView_WindowControlButton_Maximize;

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
            Application.Current?.ApplicationLifetime
            is IClassicDesktopStyleApplicationLifetime lifetime
        )
        {
            lifetime.Shutdown();
        }

        return ValueTask.CompletedTask;
    }

    protected override ValueTask ChangeWindowModeAsync(CancellationToken cancellationToken)
    {
        if (
            Application.Current?.ApplicationLifetime
            is not IClassicDesktopStyleApplicationLifetime lifetime
        )
        {
            return ValueTask.CompletedTask;
        }

        var window = lifetime.MainWindow;
        if (window == null)
        {
            return ValueTask.CompletedTask;
        }

        window.WindowState =
            window.WindowState == WindowState.FullScreen
                ? WindowState.Normal
                : WindowState.FullScreen;
        WindowSateIconKind.Value =
            window.WindowState == WindowState.FullScreen
                ? MaterialIconKind.CollapseAll
                : MaterialIconKind.Maximize;
        WindowStateHeader.Value =
            window.WindowState == WindowState.FullScreen
                ? RS.ShellView_WindowControlButton_Minimize
                : RS.ShellView_WindowControlButton_Maximize;

        return ValueTask.CompletedTask;
    }

    protected override ValueTask CollapseAsync(CancellationToken cancellationToken)
    {
        var appLifetime = Application.Current?.ApplicationLifetime;
        if (appLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
            var window = lifetime.MainWindow;
            if (window != null)
            {
                window.WindowState = WindowState.Minimized;
            }
        }

        return ValueTask.CompletedTask;
    }

    private void OpenFile(string filePath)
    {
        // TODO: Pass the file to the file processing service
    }
}
