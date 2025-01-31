using System.Composition;
using R3;

namespace Asv.Avalonia;

[ExportExtensionFor<ISettingsPage>]
[method: ImportingConstructor]
public class SettingsExtension() : IExtensionFor<ISettingsPage>
{
    private TreePageNode? _node1;
    private TreePageNode _node2;

    public void Extend(ISettingsPage context)
    {
        _node1 = new TreePageNode(SettingsAppearanceViewModel.PageId, SettingsAppearanceViewModel.PageId );
        _node2 = new TreePageNode(SettingsUnitsViewModel.PageId, SettingsUnitsViewModel.PageId);
        context.Nodes.Add(_node1);
        context.Nodes.Add(_node2);
    }

    public void Dispose()
    {
        _node1?.Dispose();
        _node2?.Dispose();
    }
}

[ExportSettings(PageId)]
public class SettingsAppearanceViewModel : RoutableViewModel, ISettingsSubPage
{
    public const string PageId = "appearance";

    #region DesignTime

    public SettingsAppearanceViewModel()
        : this(DesignTime.ThemeService)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    #endregion

    [ImportingConstructor]
    public SettingsAppearanceViewModel(IThemeService themeService)
        : base(PageId)
    {
        Theme = new ThemeProperty(themeService) { Parent = this };
    }

    public ThemeProperty Theme { get; }
    public ValueTask Init(ISettingsPage context)
    {
        return ValueTask.CompletedTask;
    }

    public override ValueTask<IRoutable> NavigateTo(string id)
    {
        if (id == Theme.Id)
        {
            return ValueTask.FromResult<IRoutable>(Theme);
        }
        
        return ValueTask.FromResult<IRoutable>(this);
    }
}