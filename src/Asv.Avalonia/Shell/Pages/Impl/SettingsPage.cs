using System.Composition;
using Avalonia.Controls.Templates;
using R3;

namespace Asv.Avalonia;

[ExportPage(PageId)]
public class SettingsPage : Page
{
    private readonly BindableReactiveProperty<string> _title = new(PageId);
    public const string PageId = "settings";

    public SettingsPage()
        : base(PageId, DesignTime.CommandService)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public SettingsPage(IThemeService themeService, ICommandService svc)
        : base(PageId, svc)
    {
        Theme = new ThemeProperty(themeService)
        {
            Parent = this,
        };
    }

    public ThemeProperty Theme { get; }

    public override IEnumerable<IRoutable> Children
    {
        get
        {
            yield return Theme;
        }
    }

    public override IReadOnlyBindableReactiveProperty<string> Title => _title;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _title.Dispose();
            Theme.Dispose();
        }

        base.Dispose(disposing);
    }
}