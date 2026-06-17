using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using ScottPlot;
using ScottPlot.Interactivity;
using SkiaSharp;
using AvaCursor = Avalonia.Input.Cursor;
using AvaloniaImage = Avalonia.Controls.Image;
using ScottPlotCursor = ScottPlot.Cursor;
using ScottPlotImageFormat = ScottPlot.ImageFormat;
using UserActions = ScottPlot.Interactivity.UserActions;

namespace Asv.Avalonia.Charts;

public sealed class AvaPlot : UserControl, IPlotControl
{
    private readonly AvaloniaImage _image;
    private Bitmap? _bitmap;

    public AvaPlot()
    {
        Plot = new Plot { PlotControl = this };
        Multiplot = new Multiplot(Plot);
        DisplayScale = DetectDisplayScale();
        UserInputProcessor = new UserInputProcessor(this);
        Menu = new AvaPlotMenu(this);

        _image = new AvaloniaImage { Stretch = Stretch.Fill };
        Content = _image;
        ClipToBounds = true;
        Focusable = true;

        SizeChanged += (_, _) => Refresh();
        LostFocus += (_, _) => UserInputProcessor.ResetState(this);
        DetachedFromVisualTree += (_, _) =>
        {
            _bitmap?.Dispose();
            _bitmap = null;
        };
    }

    public Plot Plot { get; private set; }

    public IMultiplot Multiplot { get; set; }

    public IPlotMenu? Menu { get; set; }

    public UserInputProcessor UserInputProcessor { get; }

    public GRContext? GRContext => null;

    public float DisplayScale { get; set; }

    public void Reset()
    {
        var plot = new Plot { PlotControl = this };
        Reset(plot);
    }

    public void Reset(Plot plot)
    {
        var oldPlot = Plot;
        Plot = plot;
        Plot.PlotControl = this;
        oldPlot.Dispose();
        Multiplot.Reset(plot);
        Refresh();
    }

    public void Refresh()
    {
        if (Bounds.Width <= 0 || Bounds.Height <= 0)
        {
            return;
        }

        var width = Math.Max(1, (int)Math.Round(Bounds.Width * DisplayScale));
        var height = Math.Max(1, (int)Math.Round(Bounds.Height * DisplayScale));

        var bytes = Multiplot.Render(width, height).GetImageBytes(ScottPlotImageFormat.Png);
        using var stream = new MemoryStream(bytes);
        var bitmap = new Bitmap(stream);

        var oldBitmap = _bitmap;
        _bitmap = bitmap;
        _image.Source = bitmap;
        oldBitmap?.Dispose();
    }

    public void ShowContextMenu(Pixel position)
    {
        Menu?.ShowContextMenu(position);
    }

    public float DetectDisplayScale()
    {
        return 1.0f;
    }

    public void SetCursor(ScottPlotCursor cursor)
    {
        Cursor = cursor.GetCursor();
    }

    public Plot? GetPlotAtPixel(Pixel pixel)
    {
        return Multiplot.GetPlotAtPixel(pixel);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        Focus();

        var pixel = e.ToPixel(this);
        var kind = e.GetCurrentPoint(this).Properties.PointerUpdateKind;
        UserInputProcessor.ProcessMouseDown(pixel, kind);
        e.Pointer.Capture(this);
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        var pixel = e.ToPixel(this);
        var kind = e.GetCurrentPoint(this).Properties.PointerUpdateKind;
        UserInputProcessor.ProcessMouseUp(pixel, kind);
        e.Pointer.Capture(null);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        var pixel = e.ToPixel(this);
        UserInputProcessor.ProcessMouseMove(pixel);
        e.Handled = true;
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        var pixel = e.ToPixel(this);
        var delta = (float)e.Delta.Y;
        if (delta != 0)
        {
            UserInputProcessor.ProcessMouseWheel(pixel, delta);
            e.Handled = true;
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        UserInputProcessor.ProcessKeyDown(e);
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        UserInputProcessor.ProcessKeyUp(e);
    }
}

file static class AvaPlotExtensions
{
    public static Pixel ToPixel(this PointerEventArgs e, Visual visual)
    {
        var x = (float)e.GetPosition(visual).X;
        var y = (float)e.GetPosition(visual).Y;
        return new Pixel(x, y);
    }

    public static void ProcessMouseDown(
        this UserInputProcessor processor,
        Pixel pixel,
        PointerUpdateKind kind
    )
    {
        IUserAction action = kind switch
        {
            PointerUpdateKind.LeftButtonPressed => new UserActions.LeftMouseDown(pixel),
            PointerUpdateKind.MiddleButtonPressed => new UserActions.MiddleMouseDown(pixel),
            PointerUpdateKind.RightButtonPressed => new UserActions.RightMouseDown(pixel),
            _ => new UserActions.Unknown("mouse down", kind.ToString()),
        };
        processor.Process(action);
    }

    public static void ProcessMouseUp(
        this UserInputProcessor processor,
        Pixel pixel,
        PointerUpdateKind kind
    )
    {
        IUserAction action = kind switch
        {
            PointerUpdateKind.LeftButtonReleased => new UserActions.LeftMouseUp(pixel),
            PointerUpdateKind.MiddleButtonReleased => new UserActions.MiddleMouseUp(pixel),
            PointerUpdateKind.RightButtonReleased => new UserActions.RightMouseUp(pixel),
            _ => new UserActions.Unknown("mouse up", kind.ToString()),
        };
        processor.Process(action);
    }

    public static void ProcessMouseMove(this UserInputProcessor processor, Pixel pixel)
    {
        processor.Process(new UserActions.MouseMove(pixel));
    }

    public static void ProcessMouseWheel(
        this UserInputProcessor processor,
        Pixel pixel,
        double delta
    )
    {
        IUserAction action =
            delta > 0 ? new UserActions.MouseWheelUp(pixel) : new UserActions.MouseWheelDown(pixel);
        processor.Process(action);
    }

    public static void ProcessKeyDown(this UserInputProcessor processor, KeyEventArgs e)
    {
        var key = GetKey(e.Key);
        processor.Process(new UserActions.KeyDown(key));
    }

    public static void ProcessKeyUp(this UserInputProcessor processor, KeyEventArgs e)
    {
        var key = GetKey(e.Key);
        processor.Process(new UserActions.KeyUp(key));
    }

    public static ScottPlot.Interactivity.Key GetKey(global::Avalonia.Input.Key avaKey)
    {
        return avaKey switch
        {
            global::Avalonia.Input.Key.LeftAlt => StandardKeys.Alt,
            global::Avalonia.Input.Key.RightAlt => StandardKeys.Alt,
            global::Avalonia.Input.Key.LeftShift => StandardKeys.Shift,
            global::Avalonia.Input.Key.RightShift => StandardKeys.Shift,
            global::Avalonia.Input.Key.LeftCtrl => StandardKeys.Control,
            global::Avalonia.Input.Key.RightCtrl => StandardKeys.Control,
            _ => new ScottPlot.Interactivity.Key(avaKey.ToString()),
        };
    }

    public static AvaCursor GetCursor(this ScottPlotCursor cursor)
    {
        return cursor switch
        {
            ScottPlotCursor.Arrow => new AvaCursor(StandardCursorType.Arrow),
            ScottPlotCursor.No => new AvaCursor(StandardCursorType.No),
            ScottPlotCursor.Wait => new AvaCursor(StandardCursorType.Wait),
            ScottPlotCursor.Hand => new AvaCursor(StandardCursorType.Hand),
            ScottPlotCursor.Cross => new AvaCursor(StandardCursorType.Cross),
            ScottPlotCursor.SizeAll => new AvaCursor(StandardCursorType.SizeAll),
            ScottPlotCursor.SizeNorthSouth => new AvaCursor(StandardCursorType.SizeNorthSouth),
            ScottPlotCursor.SizeWestEast => new AvaCursor(StandardCursorType.SizeWestEast),
            _ => new AvaCursor(StandardCursorType.Arrow),
        };
    }
}

file sealed class AvaPlotMenu(AvaPlot avaPlot) : IPlotMenu
{
    public string DefaultSaveImageFilename { get; set; } = "Plot.png";

    public List<ContextMenuItem> ContextMenuItems { get; } = [];

    private readonly List<FilePickerFileType> _filePickerFileTypes =
    [
        new("PNG Files") { Patterns = ["*.png"] },
        new("JPEG Files") { Patterns = ["*.jpg", "*.jpeg"] },
        new("BMP Files") { Patterns = ["*.bmp"] },
        new("WebP Files") { Patterns = ["*.webp"] },
        new("SVG Files") { Patterns = ["*.svg"] },
        new("All Files") { Patterns = ["*"] },
    ];

    public void Reset()
    {
        Clear();
        ContextMenuItems.AddRange(GetDefaultContextMenuItems());
    }

    public void Clear()
    {
        ContextMenuItems.Clear();
    }

    public void Add(string label, Action action)
    {
        ContextMenuItems.Add(new ContextMenuItem { Label = label, OnInvoke = _ => action() });
    }

    public void Add(string label, Action<Plot> action)
    {
        ContextMenuItems.Add(new ContextMenuItem { Label = label, OnInvoke = action });
    }

    public void AddSeparator()
    {
        ContextMenuItems.Add(new ContextMenuItem { IsSeparator = true });
    }

    public ContextMenuItem[] GetDefaultContextMenuItems()
    {
        return
        [
            new ContextMenuItem { Label = "Save Image", OnInvoke = OpenSaveImageDialog },
            new ContextMenuItem { Label = "Autoscale", OnInvoke = Autoscale },
        ];
    }

    public ContextMenu GetContextMenu(Plot plot)
    {
        List<global::Avalonia.Controls.MenuItem> items = [];
        foreach (var contextMenuItem in ContextMenuItems)
        {
            if (contextMenuItem.IsSeparator)
            {
                items.Add(new global::Avalonia.Controls.MenuItem { Header = "-" });
                continue;
            }

            var menuItem = new global::Avalonia.Controls.MenuItem
            {
                Header = contextMenuItem.Label,
            };
            menuItem.Click += (_, _) => contextMenuItem.OnInvoke(plot);
            items.Add(menuItem);
        }

        return new ContextMenu { ItemsSource = items };
    }

    public async void OpenSaveImageDialog(Plot plot)
    {
        var topLevel = TopLevel.GetTopLevel(avaPlot);
        if (topLevel == null)
        {
            return;
        }

        var destinationFile = await topLevel.StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                SuggestedFileName = DefaultSaveImageFilename,
                FileTypeChoices = _filePickerFileTypes,
            }
        );
        var path = destinationFile?.TryGetLocalPath();
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        var size = plot.RenderManager.LastRender.FigureRect.Size;
        plot.Save(path, (int)size.Width, (int)size.Height, ImageFormats.FromFilename(path));
    }

    public void Autoscale(Plot plot)
    {
        plot.Axes.AutoScale();
        avaPlot.Refresh();
    }

    public void ShowContextMenu(Pixel pixel)
    {
        var plot = avaPlot.GetPlotAtPixel(pixel);
        if (plot == null)
        {
            return;
        }

        var contextMenu = GetContextMenu(plot);
        contextMenu.PlacementRect = new Rect(pixel.X, pixel.Y, 1, 1);
        contextMenu.Open(avaPlot);
    }
}
