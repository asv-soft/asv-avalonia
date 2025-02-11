using System.Composition;
using Asv.Cfg;
using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.Logging;
using R3;
using ZLogger;

namespace Asv.Avalonia;

[Export]
public partial class ShellWindow : Window
{
    private readonly IConfiguration? _configuration;
    private readonly Subject<Unit>? _savePosition;
    private readonly IDisposable? _sub1;
    private readonly ILogger<ShellWindow> _logger;
    private bool _internalChange;

    [ImportingConstructor]
    public ShellWindow(IConfiguration configuration, ILoggerFactory logger)
    {
        _logger = logger.CreateLogger<ShellWindow>();
        InitializeComponent();
#if DEBUG
       // this.AttachDevTools();
#endif
        _configuration = configuration;
        _savePosition = new Subject<Unit>();
        _sub1 = _savePosition.ThrottleLast(TimeSpan.FromSeconds(1)).Subscribe(_ => SaveLayout());
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        SaveLayout();
        _savePosition?.Dispose();
        _sub1?.Dispose();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        LoadLayout();
    }

    private void LoadLayout()
    {
        if (_configuration == null)
        {
            return;
        }

        _internalChange = true;
        _logger.ZLogTrace($"Load {nameof(ShellWindow)} layout");

        var shellViewConfig = _configuration.Get<ShellWindowConfig>();

        if (shellViewConfig.IsMaximized)
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

        if (shellViewConfig.PositionX > totalWidth || shellViewConfig.PositionY > totalHeight)
        {
            Position = new PixelPoint(0, 0);
        }
        else
        {
            Position = new PixelPoint(shellViewConfig.PositionX, shellViewConfig.PositionY);
        }

        if (shellViewConfig.Height > totalHeight || shellViewConfig.Width > totalWidth)
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
            Height = shellViewConfig.Height;
            Width = shellViewConfig.Width;
        }

        _internalChange = false;
    }

    private void SaveLayout()
    {
        if (_configuration == null)
        {
            return;
        }

        _logger.ZLogTrace($"Save {nameof(ShellWindow)} layout");
        ShellWindowConfig shellViewConfig;
        if (WindowState == WindowState.Maximized)
        {
            shellViewConfig = new ShellWindowConfig { IsMaximized = true };
        }
        else
        {
            shellViewConfig = new ShellWindowConfig
            {
                Height = Height,
                Width = Width,
                PositionX = Position.X,
                PositionY = Position.Y,
            };
        }

        _configuration.Set(shellViewConfig);
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
