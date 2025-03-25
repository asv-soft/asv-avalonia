using Asv.Common;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public interface ISettingsPage : IPage
{
    ObservableList<ITreePage> Nodes { get; }
    BindableReactiveProperty<bool> IsCompactMode { get; }
}

public interface ISettingsSubPage : ITreeSubpage<ISettingsPage> { }

public abstract class SettingsSubPage : RoutableViewModel, ISettingsSubPage
{
    protected SettingsSubPage(NavigationId id)
        : base(id)
    {
        Menu.SetRoutableParent(this, true).DisposeItWith(Disposable);
        MenuView = new MenuTree(Menu).DisposeItWith(Disposable);
    }

    public virtual ValueTask Init(ISettingsPage context) => ValueTask.CompletedTask;

    public override IEnumerable<IRoutable> GetRoutableChildren() => Menu;

    public abstract IExportInfo Source { get; }
    public MenuTree MenuView { get; }
    public ObservableList<IMenuItem> Menu { get; } = [];

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Menu.Clear();
        }

        base.Dispose(disposing);
    }
}
