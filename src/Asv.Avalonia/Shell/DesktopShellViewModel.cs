using System.Composition;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using Asv.Cfg;
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
    private readonly IFileAssociationService _fileService;
    private readonly IContainerHost _ioc;
    private readonly INavigationService _navigationService;

    [ImportingConstructor]
    public DesktopShellViewModel(
        IFileAssociationService fileService,
        IConfiguration cfg,
        IContainerHost ioc,
        ILoggerFactory loggerFactory,
        INavigationService navigationService
    )
        : base(ioc, loggerFactory, cfg, ShellId)
    {
        _fileService = fileService;
        _ioc = ioc;
        _navigationService = navigationService;
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

        // Set window as the drop target
        DragDrop.SetAllowDrop(wnd, true);
        wnd.AddHandler(DragDrop.DropEvent, OnFileDrop);

        UpdateWindowStateUi(wnd.WindowState);

        lifetime.MainWindow = wnd;
        lifetime.MainWindow.Show();
    }

    private void OnFileDrop(object? sender, DragEventArgs e)
    {
        var selected = _navigationService.SelectedControl.CurrentValue ?? this;
        selected.Rise(new DesktopDragEvent(selected, args: e));
    }

    protected override ValueTask InternalCatchEvent(AsyncRoutedEvent e)
    {
        if (e is DesktopDragEvent eve)
        {
            var data = eve.Args.Data;

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
                            return _fileService.Open(path);
                        }
                    }
                }
            }
        }

        return base.InternalCatchEvent(e);
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

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            window.WindowState =
                window.WindowState == WindowState.FullScreen
                    ? WindowState.Normal
                    : WindowState.FullScreen;
        }
        else
        {
            window.WindowState =
                window.WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;

            UpdateWindowStateUi(window.WindowState);
        }

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

    public void UpdateWindowStateUi(WindowState state)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            WindowSateIconKind.Value =
                state == WindowState.FullScreen
                    ? MaterialIconKind.CollapseAll
                    : MaterialIconKind.Maximize;

            WindowStateHeader.Value =
                state == WindowState.FullScreen
                    ? RS.ShellView_WindowControlButton_Minimize
                    : RS.ShellView_WindowControlButton_Maximize;
        }
        else
        {
            WindowSateIconKind.Value =
                state == WindowState.Maximized
                    ? MaterialIconKind.CollapseAll
                    : MaterialIconKind.Maximize;

            WindowStateHeader.Value =
                state == WindowState.Maximized
                    ? RS.ShellView_WindowControlButton_Minimize
                    : RS.ShellView_WindowControlButton_Maximize;
        }
    }

    private void OpenFile(string filePath)
    {
        // TODO: Pass the file to the file processing service
    }
}
