using Asv.Common;
using Asv.IO;
using Asv.Modeling;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public class SettingsUnitsViewModelConfig
{
    public string SearchText { get; set; } = string.Empty;
    public string SelectedItemId { get; set; } = string.Empty;
}

public class SettingsUnitsViewModel : SettingsSubPage
{
    public const string PageId = "units";

    private readonly ISynchronizedView<IUnit, MeasureUnitViewModel> _view;
    private readonly IUnitService _unitsService;
    private readonly Subject<Unit> _layoutChanged = new();
    private readonly IUndoChangeSink<ValueUndoChange<Dictionary<string, string>>> _undoHandler;

    public SettingsUnitsViewModel()
        : this(
            NullTreeSubPageContext<SettingsPageViewModel>.Instance,
            NullSearchService.Instance,
            DesignTime.UnitService,
            DesignTime.LoggerFactory
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public SettingsUnitsViewModel(
        ITreeSubPageContext<ISettingsPage> context,
        ISearchService searchService,
        IUnitService unitsService,
        ILoggerFactory loggerFactory
    )
        : base(PageId, context)
    {
        ArgumentNullException.ThrowIfNull(searchService);
        ArgumentNullException.ThrowIfNull(unitsService);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _unitsService = unitsService;
        _layoutChanged.DisposeItWith(Disposable);

        var observableList = new ObservableList<IUnit>(unitsService.Units.Values);
        _view = observableList
            .CreateView(u => new MeasureUnitViewModel(u, searchService))
            .DisposeItWith(Disposable);
        _view.SetParent(this).DisposeItWith(Disposable);
        _view.DisposeMany().DisposeItWith(Disposable);
        Items = _view
            .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current)
            .DisposeItWith(Disposable);
        SelectedItem = new BindableReactiveProperty<MeasureUnitViewModel?>();
        SelectedItem
            .Skip(1)
            .Subscribe(_ => _layoutChanged.OnNext(Unit.Default))
            .DisposeItWith(Disposable);

        Search = new SearchBoxViewModel(
            nameof(Search),
            loggerFactory,
            UpdateImpl,
            TimeSpan.FromMilliseconds(500)
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        Search
            .Text.ViewValue.Skip(1)
            .Subscribe(_ => _layoutChanged.OnNext(Unit.Default))
            .DisposeItWith(Disposable);

        ResetAllCommand = new ReactiveCommand(ResetAll).DisposeItWith(Disposable);

        Search.Refresh();

        var menu = new MenuItem("reset", RS.SettingsUnitsViewModel_Button_ResetAll)
        {
            Order = 1,
            Icon = MaterialIconKind.Refresh,
            Command = ResetAllCommand,
        };
        Menu.Add(menu);

        _undoHandler = Undo.RegisterValue<Dictionary<string, string>>(
                "default",
                ApplyUnits,
                ApplyUnits
            )
            .DisposeItWith(Disposable);
        Layout
            .Register(nameof(SettingsUnitsViewModel), LoadLayout, SaveLayout, _layoutChanged)
            .DisposeItWith(Disposable);
    }

    public NotifyCollectionChangedSynchronizedViewList<MeasureUnitViewModel> Items { get; }

    public BindableReactiveProperty<MeasureUnitViewModel?> SelectedItem { get; }

    public SearchBoxViewModel Search { get; }

    public ReactiveCommand ResetAllCommand { get; }

    private Task UpdateImpl(string? query, IProgress<double> progress, CancellationToken cancel)
    {
        progress.Report(0);
        if (string.IsNullOrWhiteSpace(query))
        {
            _view.ForEach(vm => vm.Filter(query ?? string.Empty));
            _view.ResetFilter();
        }
        else
        {
            _view.AttachFilter(
                new SynchronizedViewFilter<IUnit, MeasureUnitViewModel>(
                    (_, model) => model.Filter(query)
                )
            );
        }

        progress.Report(1);
        return Task.CompletedTask;
    }

    private ValueTask ResetAll(Unit arg, CancellationToken cancel)
    {
        try
        {
            cancel.ThrowIfCancellationRequested();

            var defaultKv = _unitsService.Units.ToDictionary(
                x => x.Key,
                x => x.Value.InternationalSystemUnit.UnitItemId
            );

            var currentKv = _unitsService.Units.ToDictionary(
                x => x.Key,
                x => x.Value.CurrentUnitItem.Value.UnitItemId
            );

            _undoHandler.PublishUpdate(currentKv, defaultKv);
            ApplyUnits(defaultKv);
            return ValueTask.CompletedTask;
        }
        catch (Exception exception)
        {
            return ValueTask.FromException(exception);
        }
    }

    private void ApplyUnits(IReadOnlyDictionary<string, string> units)
    {
        foreach (var (unitId, unitItemId) in units)
        {
            if (!_unitsService.Units.TryGetValue(unitId, out var unit))
            {
                continue;
            }

            unit.CurrentUnitItem.Value = unit[unitItemId];
        }
    }

    public override IEnumerable<IViewModel> GetChildren()
    {
        yield return Search;
        foreach (var model in _view)
        {
            yield return model;
        }

        foreach (var child in base.GetChildren())
        {
            yield return child;
        }
    }

    private SettingsUnitsViewModelConfig SaveLayout()
    {
        return new SettingsUnitsViewModelConfig
        {
            SearchText = Search.Text.ViewValue.Value ?? string.Empty,
            SelectedItemId = SelectedItem.Value?.Id.ToString() ?? string.Empty,
        };
    }

    private void LoadLayout(SettingsUnitsViewModelConfig config)
    {
        Search.Text.ModelValue.Value = config.SearchText;
        var selected = _view.FirstOrDefault(x => x.Id.ToString() == config.SelectedItemId);
        if (selected is not null)
        {
            SelectedItem.Value = selected;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            SelectedItem.Value = null;
            SelectedItem.Dispose();
        }

        base.Dispose(disposing);
    }
}
