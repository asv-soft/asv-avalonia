using Avalonia;
using Avalonia.Media;
using Material.Icons;

namespace Asv.Avalonia;

public partial class StartupSplashScreen
{
    public static readonly DirectProperty<StartupSplashScreen, string?> AppNameProperty =
        AvaloniaProperty.RegisterDirect<StartupSplashScreen, string?>(
            nameof(AppName),
            o => o.AppName,
            (o, v) => o.AppName = v
        );

    public string? AppName
    {
        get;
        set => SetAndRaise(AppNameProperty, ref field, value);
    }

    public static readonly DirectProperty<StartupSplashScreen, IImage?> AppIconProperty =
        AvaloniaProperty.RegisterDirect<StartupSplashScreen, IImage?>(
            nameof(AppIcon),
            o => o.AppIcon,
            (o, v) => o.AppIcon = v
        );

    public IImage? AppIcon
    {
        get;
        set => SetAndRaise(AppIconProperty, ref field, value);
    }

    public static readonly DirectProperty<StartupSplashScreen, string?> HeaderProperty =
        AvaloniaProperty.RegisterDirect<StartupSplashScreen, string?>(
            nameof(Header),
            o => o.Header,
            (o, v) => o.Header = v
        );

    public string? Header
    {
        get;
        set => SetAndRaise(HeaderProperty, ref field, value);
    }

    public static readonly DirectProperty<StartupSplashScreen, string?> DescriptionProperty =
        AvaloniaProperty.RegisterDirect<StartupSplashScreen, string?>(
            nameof(Description),
            o => o.Description,
            (o, v) => o.Description = v
        );

    public string? Description
    {
        get;
        set => SetAndRaise(DescriptionProperty, ref field, value);
    }

    public static readonly DirectProperty<StartupSplashScreen, string?> StatusTextProperty =
        AvaloniaProperty.RegisterDirect<StartupSplashScreen, string?>(
            nameof(StatusText),
            o => o.StatusText,
            (o, v) => o.StatusText = v
        );

    public string? StatusText
    {
        get;
        set => SetAndRaise(StatusTextProperty, ref field, value);
    }

    public static readonly DirectProperty<
        StartupSplashScreen,
        object?
    > SplashScreenContentProperty = AvaloniaProperty.RegisterDirect<StartupSplashScreen, object?>(
        nameof(SplashScreenContent),
        o => o.SplashScreenContent,
        (o, v) => o.SplashScreenContent = v
    );

    public object? SplashScreenContent
    {
        get;
        set => SetAndRaise(SplashScreenContentProperty, ref field, value);
    }

    public static readonly DirectProperty<StartupSplashScreen, MaterialIconKind> IconProperty =
        AvaloniaProperty.RegisterDirect<StartupSplashScreen, MaterialIconKind>(
            nameof(Icon),
            o => o.Icon,
            (o, v) => o.Icon = v
        );

    public MaterialIconKind Icon
    {
        get;
        set => SetAndRaise(IconProperty, ref field, value);
    } = MaterialIconKind.Abacus;

    public static readonly DirectProperty<StartupSplashScreen, bool> IsBusyProperty =
        AvaloniaProperty.RegisterDirect<StartupSplashScreen, bool>(
            nameof(IsBusy),
            o => o.IsBusy,
            (o, v) => o.IsBusy = v
        );

    public bool IsBusy
    {
        get;
        set => SetAndRaise(IsBusyProperty, ref field, value);
    }

    public static readonly DirectProperty<StartupSplashScreen, bool> HasErrorProperty =
        AvaloniaProperty.RegisterDirect<StartupSplashScreen, bool>(
            nameof(HasError),
            o => o.HasError,
            (o, v) => o.HasError = v
        );

    public bool HasError
    {
        get;
        set => SetAndRaise(HasErrorProperty, ref field, value);
    }

    public static readonly DirectProperty<StartupSplashScreen, string?> ErrorTextProperty =
        AvaloniaProperty.RegisterDirect<StartupSplashScreen, string?>(
            nameof(ErrorText),
            o => o.ErrorText,
            (o, v) => o.ErrorText = v
        );

    public string? ErrorText
    {
        get;
        set => SetAndRaise(ErrorTextProperty, ref field, value);
    }

    public static readonly DirectProperty<StartupSplashScreen, bool> IsIndeterminateProperty =
        AvaloniaProperty.RegisterDirect<StartupSplashScreen, bool>(
            nameof(IsIndeterminate),
            o => o.IsIndeterminate,
            (o, v) => o.IsIndeterminate = v
        );

    public bool IsIndeterminate
    {
        get;
        set => SetAndRaise(IsIndeterminateProperty, ref field, value);
    } = true;

    public static readonly DirectProperty<StartupSplashScreen, double> ProgressMinimumProperty =
        AvaloniaProperty.RegisterDirect<StartupSplashScreen, double>(
            nameof(ProgressMinimum),
            o => o.ProgressMinimum,
            (o, v) => o.ProgressMinimum = v
        );

    public double ProgressMinimum
    {
        get;
        set => SetAndRaise(ProgressMinimumProperty, ref field, value);
    }

    public static readonly DirectProperty<StartupSplashScreen, double> ProgressMaximumProperty =
        AvaloniaProperty.RegisterDirect<StartupSplashScreen, double>(
            nameof(ProgressMaximum),
            o => o.ProgressMaximum,
            (o, v) => o.ProgressMaximum = v
        );

    public double ProgressMaximum
    {
        get;
        set => SetAndRaise(ProgressMaximumProperty, ref field, value);
    } = 100;

    public static readonly DirectProperty<StartupSplashScreen, double> ProgressValueProperty =
        AvaloniaProperty.RegisterDirect<StartupSplashScreen, double>(
            nameof(ProgressValue),
            o => o.ProgressValue,
            (o, v) => o.ProgressValue = v
        );

    public double ProgressValue
    {
        get;
        set => SetAndRaise(ProgressValueProperty, ref field, value);
    }

    public static readonly DirectProperty<StartupSplashScreen, int> MinimumShowTimeProperty =
        AvaloniaProperty.RegisterDirect<StartupSplashScreen, int>(
            nameof(MinimumShowTimeMs),
            o => o.MinimumShowTimeMs,
            (o, v) => o.MinimumShowTimeMs = v
        );

    public int MinimumShowTimeMs
    {
        get;
        set => SetAndRaise(MinimumShowTimeProperty, ref field, Math.Max(0, value));
    } = 1200;
}
