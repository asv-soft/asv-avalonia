using System.Composition;
using System.Runtime.InteropServices;
using Asv.Cfg;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace Asv.Avalonia;

public class DesktopShellViewModelConfig : ShellViewModelConfig { }

[Export(ShellId, typeof(IShell))]
public class DesktopShellViewModel : ShellViewModel
{
    public const string ShellId = "shell.desktop";

    private readonly IFileAssociationService _fileService;
    private readonly UnsavedChangesDialogPrefab _unsavedChangesDialogPrefab;
    private readonly INavigationService _navigationService;

    [ImportingConstructor]
    public DesktopShellViewModel(
        IFileAssociationService fileService,
        IConfiguration cfg,
        IContainerHost ioc,
        ILayoutService layoutService,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        INavigationService navigationService
    )
        : base(ioc, layoutService, loggerFactory, cfg, ShellId)
    {
        _fileService = fileService;
        _navigationService = navigationService;
        _unsavedChangesDialogPrefab = dialogService.GetDialogPrefab<UnsavedChangesDialogPrefab>();

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
        wnd.AddHandler(DragDrop.DragOverEvent, OnDragOver);

        UpdateWindowStateUi(wnd.WindowState);

        lifetime.MainWindow = wnd;
        lifetime.MainWindow.Show();
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = DragDropEffects.Copy;
    }

    private void OnFileDrop(object? sender, DragEventArgs e)
    {
        var selected = Navigation.SelectedControl.CurrentValue ?? this;
        selected.Rise(new DesktopDragEvent(selected, args: e));
    }

    protected override ValueTask InternalCatchEvent(AsyncRoutedEvent e)
    {
        if (e is DesktopDragEvent eve)
        {
            var files = eve.Args.DataTransfer.TryGetFiles();
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
        }

        if (e is DesktopPushArgsEvent argsEvent)
        {
            if (argsEvent.Args.Tags.Count > 1)
            {
                return _fileService.Open(argsEvent.Args.Tags.Skip(1).First());
            }
        }

        return base.InternalCatchEvent(e);
    }

    protected override async ValueTask CloseAsync(CancellationToken cancellationToken)
    {
        var pages = Pages.ToArray();

        List<IPage> restrictedPages = [];
        Dictionary<string, List<string>> reasonsForRestrictedPages = [];
        var redirectedToFirstRestrictedPage = false;

        foreach (var page in pages)
        {
            try
            {
                var reasons = await page.RequestChildCloseApproval();

                if (reasons.Count != 0)
                {
                    if (!redirectedToFirstRestrictedPage)
                    {
                        await _navigationService.GoTo(page.GetPathToRoot());
                        redirectedToFirstRestrictedPage = true;
                    }

                    restrictedPages.Add(page);

                    reasonsForRestrictedPages[page.Id.ToString()] = reasons
                        .Select(r => r.Message ?? "—")
                        .ToList();
                }
            }
            catch (Exception e)
            {
                Logger.ZLogTrace(
                    e,
                    $"Error on requesting approval for the page {page.Title}[{page.Id}]: {e.Message}"
                );
            }
        }

        if (restrictedPages.Count != 0)
        {
            var isForceExit = await _unsavedChangesDialogPrefab.ShowDialogAsync(
                new UnsavedChangesDialogPayload
                {
                    Changes = restrictedPages.ConvertAll(p => new UnsavedChangeMeta
                    {
                        Page = p,
                        Restrictions = reasonsForRestrictedPages[p.Id.ToString()],
                    }),
                }
            );

            if (!isForceExit)
            {
                return;
            }
        }

        await base.CloseAsync(cancellationToken);

        foreach (var page in pages.ToArray())
        {
            await page.TryCloseAsync(true);
        }

        if (
            Application.Current?.ApplicationLifetime
            is IClassicDesktopStyleApplicationLifetime lifetime
        )
        {
            lifetime.Shutdown();
        }
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

        return base.ChangeWindowModeAsync(cancellationToken);
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

        return base.CollapseAsync(cancellationToken);
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
}
