using System.Diagnostics;
using System.Runtime.InteropServices;
using Asv.Cfg;
using Asv.Common;
using Asv.Modeling;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using R3;
using ZLogger;

namespace Asv.Avalonia;

public sealed class DesktopShellViewModel : ShellViewModel
{
    private readonly IFileAssociationService _fileService;

    public DesktopShellViewModel(
        IFileAssociationService fileService,
        IServiceProvider ioc,
        ILoggerFactory loggerFactory,
        IAppPath appPath,
        IThemeService themeService,
        IDialogService dialogService,
        IExtensionService ext
    )
        : base(ioc, loggerFactory, appPath, themeService, dialogService, ext)
    {
        _fileService = fileService;

        var wnd = ioc.GetRequiredService<ShellWindow>();
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
        wnd.AddHandler(DragDrop.DragOverEvent, OnDragOver);

        Events.Catch<DesktopDragEvent>(OnDesktopDragEvent).AddTo(ref DisposableBag);
        Events.Catch<DesktopPushArgsEvent>(OnDesktopPushArgsEvent).AddTo(ref DisposableBag);

        UpdateWindowStateUi(wnd.WindowState);

        lifetime.MainWindow = wnd;
        lifetime.MainWindow.Show();
    }

    private ValueTask OnDesktopPushArgsEvent(
        IViewModel owner,
        DesktopPushArgsEvent e,
        CancellationToken cancel
    )
    {
        if (e.Args.Tags.Count > 1)
        {
            return _fileService.Open(e.Args.Tags.Skip(1).First());
        }
        return ValueTask.CompletedTask;
    }

    private ValueTask OnDesktopDragEvent(
        IViewModel owner,
        DesktopDragEvent e,
        CancellationToken cancel
    )
    {
        var files = e.Args.DataTransfer.TryGetFiles();
        if (files == null)
        {
            return ValueTask.CompletedTask;
        }
        foreach (var file in files)
        {
            var path = file.TryGetLocalPath();
            if (Path.Exists(path))
            {
                return _fileService.Open(path);
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

    protected override async ValueTask<bool> TryCloseAsync(CancellationToken cancellationToken)
    {
        if (!await base.TryCloseAsync(cancellationToken))
        {
            return false;
        }

        if (
            Application.Current?.ApplicationLifetime
            is IClassicDesktopStyleApplicationLifetime lifetime
        )
        {
            lifetime.Shutdown();
        }

        return true;
    }

    public override void ChangeWindowMode()
    {
        if (
            Application.Current?.ApplicationLifetime
            is not IClassicDesktopStyleApplicationLifetime lifetime
        )
        {
            return;
        }

        var window = lifetime.MainWindow;
        if (window == null)
        {
            return;
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
    }

    public override void CollapseWindow()
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
    }

    protected override void RestartApplication(string[] args)
    {
        var exePath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(exePath))
        {
            Logger.LogError("Failed to get path of the application");
            return;
        }

        StartProcess(exePath, args);
    }

    private void StartProcess(string exePath, string[] args)
    {
        var psi = new ProcessStartInfo { FileName = exePath, UseShellExecute = false };

        foreach (var arg in args)
        {
            psi.ArgumentList.Add(arg);
        }

        Process.Start(psi);
        Logger.ZLogInformation(
            $"Application restarted successfully with arguments: {string.Join(" ", args)} and path {exePath}."
        );
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = DragDropEffects.Copy;
    }

    private void OnFileDrop(object? sender, DragEventArgs e)
    {
        var selected = Navigation.SelectedControl.CurrentValue ?? this;
        selected.Rise(new DesktopDragEvent(selected, args: e)).SafeFireAndForget();
    }
}
