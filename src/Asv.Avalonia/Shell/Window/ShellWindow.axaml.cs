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
        _layout = this.RegisterLayout<ShellWindowConfig, Unit>(
            nameof(ShellWindow),
            LoadLayout,
            SaveLayout,
            _savePosition.Where(_ => !_internalChange)
        );
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        _savePosition?.OnNext(Unit.Default);
        _savePosition?.Dispose();
        _layout?.Dispose();
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

            if (config.PositionX > totalWidth || config.PositionY > totalHeight)
            {
                Position = new PixelPoint(0, 0);
            }
            else
            {
                Position = new PixelPoint(config.PositionX, config.PositionY);
            }

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
        if (_internalChange || !IsValidSize(Width) || !IsValidSize(Height))
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
