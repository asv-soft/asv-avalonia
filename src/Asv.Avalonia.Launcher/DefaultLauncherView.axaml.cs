using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Material.Icons;

namespace Asv.Avalonia.Launcher;

public partial class DefaultLauncherView : UserControl
{
    private Bitmap? _icon;

    public DefaultLauncherView()
        : this(new LauncherApplicationOptions())
    {
        if (!Design.IsDesignMode)
        {
            throw new Exception("Design constructor should not be used at runtime.");
        }
    }

    public DefaultLauncherView(LauncherApplicationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        InitializeComponent();
        ApplyOptions(options);
        DetachedFromVisualTree += (_, _) => DisposeIcon();
    }

    private void ApplyOptions(LauncherApplicationOptions options)
    {
        TitleTextBlock.Text = options.Title;
        DescriptionTextBlock.Text = options.Description;
        FooterTextBlock.Text = options.Footer;

        ApplyIcon(options);
    }

    private void ApplyIcon(LauncherApplicationOptions options)
    {
        IconImage.IsVisible = false;
        MaterialIcon.IsVisible = false;

        if (options.IconSource is not null && TryLoadIcon(options.IconSource, out _icon))
        {
            IconImage.Source = _icon;
            IconImage.IsVisible = true;
            return;
        }

        if (options.IconKind is not MaterialIconKind kind)
        {
            return;
        }

        MaterialIcon.Kind = kind;
        MaterialIcon.IsVisible = true;
    }

    private static bool TryLoadIcon(Uri source, out Bitmap? icon)
    {
        try
        {
            using var stream = AssetLoader.Open(source);
            icon = new Bitmap(stream);
            return true;
        }
        catch
        {
            icon = null;
            return false;
        }
    }

    private void DisposeIcon()
    {
        _icon?.Dispose();
        _icon = null;
    }
}
