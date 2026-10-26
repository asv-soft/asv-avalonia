using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace Asv.Avalonia;

internal partial class DockDragPreview : Window
{
    private readonly RenderTargetBitmap? _snapshot;
    private readonly ScaleTransform _scale = new(0.96, 0.96);
    private IDisposable? _closeTimer;
    private bool _closing;
    private bool _closed;

    public DockDragPreview(IPage page, RenderTargetBitmap? snapshot)
    {
        InitializeComponent();
        DataContext = page;
        _snapshot = snapshot;
        PART_Image.Source = snapshot;
        PART_Image.IsVisible = snapshot != null;
        PART_Fallback.IsVisible = snapshot == null;
        PART_Card.RenderTransform = _scale;
        PART_Card.Transitions = new Transitions
        {
            new DoubleTransition { Property = OpacityProperty, Duration = TimeSpan.FromMilliseconds(120) },
        };
        _scale.Transitions = new Transitions
        {
            new DoubleTransition { Property = ScaleTransform.ScaleXProperty, Duration = TimeSpan.FromMilliseconds(120), Easing = new CubicEaseOut() },
            new DoubleTransition { Property = ScaleTransform.ScaleYProperty, Duration = TimeSpan.FromMilliseconds(120), Easing = new CubicEaseOut() },
        };
        Opened += (_, _) =>
        {
            PART_Card.Opacity = 1;
            _scale.ScaleX = _scale.ScaleY = 1;
        };
        Closed += (_, _) =>
        {
            _closed = true;
            _closeTimer?.Dispose();
            _closeTimer = null;
            _snapshot?.Dispose();
        };
    }

    public void MoveTo(PixelPoint pointer, double scaling, bool outside)
    {
        Position = new PixelPoint(pointer.X + (int)(12 * scaling), pointer.Y + (int)(16 * scaling));
        PART_Hint.Text = RS.ResourceManager.GetString(outside ? "DockDrag_OpenInWindow" : "DockDrag_KeepInWindow", RS.Culture);
    }

    public void ClosePreview(bool immediately)
    {
        if (_closing || _closed)
        {
            return;
        }
        _closing = true;
        if (immediately)
        {
            Close();
            return;
        }
        PART_Card.Opacity = 0;
        _scale.ScaleX = _scale.ScaleY = 0.96;
        _closeTimer = DispatcherTimer.RunOnce(Close, TimeSpan.FromMilliseconds(120));
    }
}
