using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;
using ZLogger;

namespace Asv.Avalonia;

public partial class ShellWindow : Window
{
    private readonly Subject<Unit>? _savePosition;
    private readonly IDisposable? _layout;
    private readonly ILogger<ShellWindow> _logger;
    private bool _internalChange;
    private bool _isCloseRequested;

    public ShellWindow()
        : this(NullLoggerFactory.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    static ShellWindow()
    {
        WindowStateProperty
            .Changed.ToObservable()
            .Subscribe(x =>
            {
                if (x.Sender is ShellWindow window)
                {
                    window._savePosition?.OnNext(Unit.Default);
                    window.UpdateWindowStateUI();
                }
            });
    }

    public ShellWindow(ILoggerFactory logger)
    {
        InitializeComponent();
        if (Design.IsDesignMode)
        {
            return;
        }

        _logger = logger.CreateLogger<ShellWindow>();

        _savePosition = new Subject<Unit>();
        _layout = this.RegisterLayout(
            nameof(ShellWindow),
            LoadLayout,
            SaveLayout,
            _savePosition.ThrottleLast(TimeSpan.FromSeconds(1)).Where(_ => !_internalChange)
        );
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (TryHandleUserCloseRequest(e))
        {
            return;
        }

        base.OnClosing(e);
        _savePosition?.OnNext(Unit.Default);
        _savePosition?.Dispose();
        _layout?.Dispose();
    }

    private bool TryHandleUserCloseRequest(WindowClosingEventArgs e)
    {
        if (e.CloseReason == WindowCloseReason.ApplicationShutdown)
        {
            return false;
        }

        if (DataContext is not DesktopShellViewModel vm)
        {
            return false;
        }

        e.Cancel = true;
        if (_isCloseRequested)
        {
            return true;
        }

        _isCloseRequested = true;
        TryCloseFromViewModel(vm);
        return true;
    }

    private async void TryCloseFromViewModel(DesktopShellViewModel vm)
    {
        try
        {
            if (!await vm.TryCloseFromWindowAsync(CancellationToken.None))
            {
                _isCloseRequested = false;
            }
        }
        catch (Exception e)
        {
            _isCloseRequested = false;
            _logger.ZLogError(e, $"Error while closing {nameof(ShellWindow)}: {e.Message}");
        }
    }

    private void LoadLayout(ShellWindowConfig config)
    {
        if (!IsValidSize(config.Width) || !IsValidSize(config.Height))
        {
            return;
        }

        _internalChange = true;

        try
        {
            _logger.ZLogTrace($"Load {nameof(ShellWindow)} layout: {config}");

            if (config.IsMaximized)
            {
                WindowState = WindowState.Maximized;
                return;
            }

            var totalWidth = 0;
            var totalHeight = 0;

            foreach (var scr in Screens.All)
            {
                totalWidth += scr.Bounds.Width;
                totalHeight += scr.Bounds.Height;
            }

            var restoredBounds = new PixelRect(
                config.PositionX,
                config.PositionY,
                (int)config.Width,
                (int)config.Height
            );

            Position = Screens.All.Any(scr => scr.Bounds.Intersects(restoredBounds))
                ? new PixelPoint(config.PositionX, config.PositionY)
                : new PixelPoint(0, 0);

            if (config.Height > totalHeight || config.Width > totalWidth)
            {
                if (Screens.Primary != null)
                {
                    var scrBounds = Screens.Primary.Bounds;

                    Height = scrBounds.Height * 0.9;
                    Width = scrBounds.Width * 0.9;
                }

                Position = new PixelPoint(0, 0);
            }
            else
            {
                Height = config.Height;
                Width = config.Width;
            }
        }
        catch (Exception e)
        {
            _logger.ZLogError(
                e,
                $"Error while loading layout for {nameof(ShellWindow)}: {e.Message}"
            );
        }
        finally
        {
            _internalChange = false;
        }
    }

    private ShellWindowConfig? SaveLayout()
    {
        if (
            _internalChange
            || WindowState == WindowState.Minimized
            || !IsValidSize(Width)
            || !IsValidSize(Height)
        )
        {
            return null;
        }

        var config = new ShellWindowConfig
        {
            Height = Height,
            Width = Width,
            PositionX = Position.X,
            PositionY = Position.Y,
            IsMaximized = WindowState == WindowState.Maximized,
        };

        _logger.ZLogTrace($"Save {nameof(ShellWindow)} layout: {config}");
        return config;
    }

    private static bool IsValidSize(double value)
    {
        return double.IsFinite(value) && value > 0;
    }

    private void UpdateWindowStateUI()
    {
        if (DataContext is DesktopShellViewModel vm)
        {
            vm.UpdateWindowStateUi(WindowState);
        }
    }

    private void WindowBase_OnPositionChanged(object? sender, PixelPointEventArgs e)
    {
        if (_internalChange)
        {
            return;
        }

        _savePosition?.OnNext(Unit.Default);
    }

    private void Control_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (_internalChange)
        {
            return;
        }

        _savePosition?.OnNext(Unit.Default);
    }
}
