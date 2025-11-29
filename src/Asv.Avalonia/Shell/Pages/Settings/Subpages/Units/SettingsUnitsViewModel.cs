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
    private SettingsUnitsViewModelConfig? _config;

    public SettingsUnitsViewModel()
        : this(NullSearchService.Instance, DesignTime.UnitService, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public SettingsUnitsViewModel(
        ISearchService searchService,
        IUnitService unit,
        ILoggerFactory loggerFactory
    )
        : base(PageId, loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(searchService);
        ArgumentNullException.ThrowIfNull(unit);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        var observableList = new ObservableList<IUnit>(unit.Units.Values);
        _view = observableList
            .CreateView(u => new MeasureUnitViewModel(u, searchService, loggerFactory))
            .DisposeItWith(Disposable);
        _view.SetRoutableParent(this).DisposeItWith(Disposable);
        _view.DisposeMany().DisposeItWith(Disposable);
        Items = _view
            .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current)
            .DisposeItWith(Disposable);
        SelectedItem = new BindableReactiveProperty<MeasureUnitViewModel?>();

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

    protected override ValueTask InternalCatchEvent(AsyncRoutedEvent e)
    {
        switch (e)
        {
            case SaveLayoutEvent saveLayoutEvent:
                if (_config is null)
                {
                    break;
                }

                this.HandleSaveLayout(
                    saveLayoutEvent,
                    _config,
                    cfg =>
                    {
                        cfg.SearchText = Search.Text.ViewValue.Value ?? string.Empty;
                        cfg.SelectedItemId = SelectedItem.Value?.Id.ToString() ?? string.Empty;
                    }
                );
                break;
            case LoadLayoutEvent loadLayoutEvent:
                _config = this.HandleLoadLayout<SettingsUnitsViewModelConfig>(
                    loadLayoutEvent,
                    cfg =>
                    {
                        Search.Text.ModelValue.Value = cfg.SearchText;
                        var selected = _view.FirstOrDefault(x =>
                            x.Id.ToString() == cfg.SelectedItemId
                        );

                        if (selected is not null)
                        {
                            SelectedItem.Value = selected;
                        }
                    }
                );
                break;
        }

        return base.InternalCatchEvent(e);
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

    public override IExportInfo Source => SystemModule.Instance;
}
