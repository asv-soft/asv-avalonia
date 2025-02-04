using System.Composition;
using Asv.Common;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

[ExportSettings(SubPageId)]
public class SettingsHotKeysViewModel : RoutableViewModel, ISettingsSubPage
{
    private readonly ObservableList<ICommandInfo> _observableList;
    private readonly ISynchronizedView<ICommandInfo, CommandViewModel> _view;
    private readonly IDisposable _sub1;
    public const string SubPageId = "settings.hotkeys";

    public SettingsHotKeysViewModel()
        : this(DesignTime.CommandService)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public SettingsHotKeysViewModel(ICommandService svc)
        : base(SubPageId)
    {
        SearchText = new BindableReactiveProperty<string>();
        _observableList = new ObservableList<ICommandInfo>(svc.Commands);
        SelectedItem = new BindableReactiveProperty<CommandViewModel>();
        _view = _observableList.CreateView(x => new CommandViewModel(x, svc) { Parent = this });
        Items = _view.ToNotifyCollectionChanged();
        _sub1 = SearchText
            .ThrottleLast(TimeSpan.FromMilliseconds(500))
            .Subscribe(x =>
            {
                if (x.IsNullOrWhiteSpace())
                {
                    _view.ResetFilter();
                }
                else
                {
                    _view.AttachFilter((_, model) => model.Fitler(x));
                }
            });
    }

    public NotifyCollectionChangedSynchronizedViewList<CommandViewModel> Items { get; }
    public BindableReactiveProperty<string> SearchText { get; }
    public IBindableReactiveProperty<CommandViewModel> SelectedItem { get; }

    public override ValueTask<IRoutable> Navigate(string id)
    {
        var item = _view.FirstOrDefault(x => x.Id == id);
        if (item != null)
        {
            SelectedItem.Value = item;
            return ValueTask.FromResult<IRoutable>(item);
        }

        return ValueTask.FromResult<IRoutable>(this);
    }

    public ValueTask Init(ISettingsPage context)
    {
        return ValueTask.CompletedTask;
    }
}

public class CommandViewModel : RoutableViewModel
{
    public CommandViewModel(ICommandInfo commandInfo, ICommandService svc)
        : base(commandInfo.Id)
    {
        Info = commandInfo;
    }

    public ICommandInfo Info { get; }

    public override ValueTask<IRoutable> Navigate(string id)
    {
        return ValueTask.FromResult<IRoutable>(this);
    }

    public bool Fitler(string text)
    {
        return Info.Name.Contains(text, StringComparison.OrdinalIgnoreCase);
    }
}
