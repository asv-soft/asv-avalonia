using System.Composition;
using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;
using ZLogger;

namespace Asv.Avalonia;

[Export]
public partial class ShellWindow : Window, IExportable
{
    private readonly ILayoutService? _layoutService;
    private readonly Subject<Unit>? _savePosition;
    private readonly IDisposable? _sub1;
    private readonly ILogger<ShellWindow> _logger;
    private ShellWindowConfig _config;
    private bool _internalChange;

    public ShellWindow()
        : this(NullLayoutService.Instance, NullLoggerFactory.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    static ShellWindow()
    {
        WindowStateProperty.Changed.Subscribe(x =>
        {
            if (x.Sender is ShellWindow window)
            {
                window._savePosition?.OnNext(Unit.Default);
                window.UpdateWindowStateUI();
            }
        });
    }

    [ImportingConstructor]
    public ShellWindow(ILayoutService layoutService, ILoggerFactory logger)
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            return;
        }

        _logger = logger.CreateLogger<ShellWindow>();

        _layoutService = layoutService;
        _savePosition = new Subject<Unit>();
        _sub1 = _savePosition
            .Where(_ => !_internalChange)
            .ThrottleLast(TimeSpan.FromSeconds(1))
            .Subscribe(_ => SaveLayout());
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

        ArgumentNullException.ThrowIfNull(_layoutService);

        _config = _layoutService.Get<ShellWindowConfig>(this);
        LoadLayout();
    }

    private void LoadLayout()
    {
        if (_layoutService is null)
        {
            return;
        }

        _internalChange = true;

        try
        {
            _logger.ZLogTrace($"Load {nameof(ShellWindow)} layout: {_config}");

            if (_config.IsMaximized)
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

            if (_config.PositionX > totalWidth || _config.PositionY > totalHeight)
            {
                Position = new PixelPoint(0, 0);
            }
            else
            {
                Position = new PixelPoint(_config.PositionX, _config.PositionY);
            }

            if (_config.Height > totalHeight || _config.Width > totalWidth)
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
                Height = _config.Height;
                Width = _config.Width;
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

    private void SaveLayout()
    {
        if (_layoutService == null)
        {
            return;
        }

        _config.Height = Height;
        _config.Width = Width;
        _config.PositionX = Position.X;
        _config.PositionY = Position.Y;
        _config.IsMaximized = WindowState == WindowState.Maximized;

        _logger.ZLogTrace($"Save {nameof(ShellWindow)} layout: {_config}");
        _layoutService.SetInMemory(this, _config);
        _layoutService.FlushFromMemory(this);
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

    public IExportInfo Source => SystemModule.Instance;
}
