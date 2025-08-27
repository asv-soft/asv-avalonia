using System.Composition;
using Asv.Cfg;
using Asv.Common;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public sealed class SettingsUnitsViewModelConfig : TreeSubpageConfig
{
    public string SearchText { get; set; } = string.Empty;
    public string SelectedUnitId { get; set; } = string.Empty;
}

[ExportSettings(PageId)]
public class SettingsUnitsViewModel : SettingsSubPage<SettingsUnitsViewModelConfig>
{
    public const string PageId = "units";
    private readonly ISynchronizedView<IUnit, MeasureUnitViewModel> _view;

    public SettingsUnitsViewModel()
        : this(DesignTime.UnitService, DesignTime.Configuration, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public SettingsUnitsViewModel(
        IUnitService unit,
        IConfiguration cfg,
        ILoggerFactory loggerFactory
    )
        : base(PageId, cfg, loggerFactory)
    {
        var observableList = new ObservableList<IUnit>(unit.Units.Values);
        _view = observableList
            .CreateView(x => new MeasureUnitViewModel(x, loggerFactory))
            .DisposeItWith(Disposable);
        _view.SetRoutableParent(this).DisposeItWith(Disposable);
        Items = _view.ToNotifyCollectionChanged().DisposeItWith(Disposable);

        SelectedItem = new BindableReactiveProperty<MeasureUnitViewModel?>().DisposeItWith(
            Disposable
        );

        var selectedUnit = _view.FirstOrDefault(u => u.Id == Config.SelectedUnitId);
        if (selectedUnit is not null)
        {
            SelectedItem.OnNext(selectedUnit);
        }

        Search = new SearchBoxViewModel(
            nameof(Search),
            loggerFactory,
            UpdateImpl,
            TimeSpan.FromMilliseconds(500)
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        Search.Text.Value = Config.SearchText;
        Search.Refresh();

        Observable
            .Merge(
                Search.Text.Skip(1).Select(_ => Unit.Default),
                SelectedItem.Skip(1).Select(_ => Unit.Default)
            )
            .Subscribe(_ => HasChanges.Value = true)
            .DisposeItWith(Disposable);
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
        foreach (var model in _view)
        {
            yield return model;
        }

        foreach (var children in base.GetRoutableChildren())
        {
            yield return children;
        }
    }

    public override ValueTask SaveChanges(CancellationToken cancellationToken)
    {
        Config.SearchText = Search.Text.CurrentValue;
        Config.SelectedUnitId = SelectedItem.Value?.Id.ToString() ?? string.Empty;
        return base.SaveChanges(cancellationToken);
    }

    public override IExportInfo Source => SystemModule.Instance;
}
