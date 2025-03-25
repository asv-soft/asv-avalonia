using System.Composition;
using Asv.Common;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

[ExportSettings(SubPageId)]
public class SettingsKeymapViewModel : SettingsSubPage
{
    private readonly ISynchronizedView<ICommandInfo, SettingsKeyMapItemViewModel> _view;
    public const string SubPageId = "settings.hotkeys";

    public SettingsKeymapViewModel()
        : this(DesignTime.CommandService)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public SettingsKeymapViewModel(ICommandService svc)
        : base(SubPageId)
    {
        SearchText = new BindableReactiveProperty<string>().DisposeItWith(Disposable);
        var observableList = new ObservableList<ICommandInfo>(svc.Commands);
        SelectedItem = new BindableReactiveProperty<SettingsKeyMapItemViewModel>().DisposeItWith(
            Disposable
        );
        ResetHotKeysToDefaultCommand = new ReactiveCommand(ResetHotkeys).DisposeItWith(Disposable);

        _view = observableList
            .CreateView(x => new SettingsKeyMapItemViewModel(x, svc))
            .DisposeItWith(Disposable);
        _view.SetRoutableParentForView(this, true).DisposeItWith(Disposable);
        Items = _view.ToNotifyCollectionChanged().DisposeItWith(Disposable);
        SearchText
            .ThrottleLast(TimeSpan.FromMilliseconds(500))
            .Subscribe(x =>
            {
                if (x.IsNullOrWhiteSpace())
                {
                    _view.ResetFilter();
                }
                else
                {
                    _view.AttachFilter((_, model) => model.Filter(x));
                }
            })
            .DisposeItWith(Disposable);
    }

    public ReactiveCommand ResetHotKeysToDefaultCommand { get; set; }
    public NotifyCollectionChangedSynchronizedViewList<SettingsKeyMapItemViewModel> Items { get; }
    public BindableReactiveProperty<string> SearchText { get; }
    public IBindableReactiveProperty<SettingsKeyMapItemViewModel> SelectedItem { get; }

    public override ValueTask<IRoutable> Navigate(NavigationId id)
    {
        var item = _view.FirstOrDefault(x => x.Id == id);
        if (item != null)
        {
            SelectedItem.Value = item;
            return ValueTask.FromResult<IRoutable>(item);
        }

        return base.Navigate(id);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        foreach (var child in base.GetRoutableChildren())
        {
            yield return child;
        }

        foreach (var model in _view)
        {
            yield return model;
        }
    }

    private void ResetHotkeys(Unit unit)
    {
        foreach (var item in Items)
        {
            item.IsReset.Value = true;
        }
    }

    public override IExportInfo Source => SystemModule.Instance;
}
