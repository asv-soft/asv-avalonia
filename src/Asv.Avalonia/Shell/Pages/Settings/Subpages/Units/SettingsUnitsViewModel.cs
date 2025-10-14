using System.Composition;
using Asv.Common;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public class SettingsUnitsViewModelConfig
{
    public string SearchText { get; set; } = string.Empty;
    public string SelectedItemId { get; set; } = string.Empty;
}

[ExportSettings(PageId)]
public class SettingsUnitsViewModel : SettingsSubPage
{
    public const string PageId = "units";
    private readonly ISynchronizedView<IUnit, MeasureUnitViewModel> _view;
    private readonly ILayoutService _layoutService;

    private SettingsUnitsViewModelConfig _config;

    public SettingsUnitsViewModel()
        : this(DesignTime.UnitService, NullLayoutService.Instance, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public SettingsUnitsViewModel(
        IUnitService unit,
        ILayoutService layoutService,
        ILoggerFactory loggerFactory
    )
        : base(PageId, layoutService, loggerFactory)
    {
        _layoutService = layoutService;
        var observableList = new ObservableList<IUnit>(unit.Units.Values);
        _view = observableList
            .CreateView(x => new MeasureUnitViewModel(x, loggerFactory))
            .DisposeItWith(Disposable);
        _view.SetRoutableParent(this).DisposeItWith(Disposable);
        Items = _view.ToNotifyCollectionChanged().DisposeItWith(Disposable);
        SelectedItem = new BindableReactiveProperty<MeasureUnitViewModel?>().DisposeItWith(
            Disposable
        );

        Search = new SearchBoxViewModel(
            nameof(Search),
            loggerFactory,
            UpdateImpl,
            TimeSpan.FromMilliseconds(500)
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        Search.Refresh();
    }

    public NotifyCollectionChangedSynchronizedViewList<MeasureUnitViewModel> Items { get; }

    public BindableReactiveProperty<MeasureUnitViewModel?> SelectedItem { get; }

    public SearchBoxViewModel Search { get; }

    private Task UpdateImpl(string? query, IProgress<double> progress, CancellationToken cancel)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
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

        return Task.CompletedTask;
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        yield return Search;
        foreach (var model in _view)
        {
            yield return model;
        }

        foreach (var child in base.GetRoutableChildren())
        {
            yield return child;
        }
    }

    protected override ValueTask HandleSaveLayout()
    {
        _config.SearchText = Search.Text.ViewValue.Value ?? string.Empty;
        _config.SelectedItemId = SelectedItem.Value?.Id.ToString() ?? string.Empty;
        _layoutService.SetInMemory(this, _config);
        return base.HandleSaveLayout();
    }

    protected override ValueTask HandleLoadLayout()
    {
        _config = _layoutService.Get<SettingsUnitsViewModelConfig>(this);
        Search.Text.ModelValue.Value = _config.SearchText;
        var selected = _view.FirstOrDefault(x => x.Id.ToString() == _config.SelectedItemId);

        if (selected is not null)
        {
            SelectedItem.Value = selected;
        }

        return base.HandleLoadLayout();
    }

    public override IExportInfo Source => SystemModule.Instance;
}
