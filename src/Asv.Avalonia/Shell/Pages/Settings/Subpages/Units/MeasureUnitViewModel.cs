using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public class MeasureUnitViewModel : RoutableViewModel
{
    private readonly ISearchService _searchService;
    private bool _internalChange;

    public MeasureUnitViewModel(
        IUnit item,
        ISearchService searchService,
        ILoggerFactory loggerFactory
    )
        : base(item.UnitId, loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(searchService);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _searchService = searchService;

        SelectedItem = new BindableReactiveProperty<IUnitItem>(item.CurrentUnitItem.CurrentValue);
        Base = item;
        Name = Base
            .CurrentUnitItem.Select(u => u.Name)
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);
        Symbol = Base
            .CurrentUnitItem.Select(u => u.Symbol)
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);
        _internalChange = true;
        _sub1 = SelectedItem.SubscribeAwait(OnChangedByUser);
        _sub2 = item.CurrentUnitItem.Subscribe(OnChangeByModel);
        _internalChange = false;

        ResetCommand = new ReactiveCommand(ResetImpl).DisposeItWith(Disposable);
    }

    public BindableReactiveProperty<IUnitItem> SelectedItem { get; }
    public IUnit Base { get; }
    public IReadOnlyBindableReactiveProperty<string> Name { get; }
    public IReadOnlyBindableReactiveProperty<string> Symbol { get; }

    public ReactiveCommand ResetCommand { get; }

    public Selection NameSelection
    {
        get;
        private set => SetField(ref field, value);
    }

    public Selection DescriptionSelection
    {
        get;
        private set => SetField(ref field, value);
    }

    public Selection CurrentNameSelection
    {
        get;
        private set => SetField(ref field, value);
    }

    public Selection CurrentSymbolSelection
    {
        get;
        private set => SetField(ref field, value);
    }

    public Selection DefaultNameSelection
    {
        get;
        private set => SetField(ref field, value);
    }

    public Selection DefaultSymbolSelection
    {
        get;
        private set => SetField(ref field, value);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    internal async ValueTask ResetImpl(Unit arg, CancellationToken arg2)
    {
        if (Base.InternationalSystemUnit.UnitItemId == Base.CurrentUnitItem.Value.UnitItemId)
        {
            return;
        }

        _internalChange = true;
        await ChangeMeasureUnitCommand.ExecuteCommand(this, Base, Base.InternationalSystemUnit);
        _internalChange = false;
    }

    private async ValueTask OnChangedByUser(IUnitItem userValue, CancellationToken cancel)
    {
        if (_internalChange)
        {
            return;
        }

        _internalChange = true;
        await ChangeMeasureUnitCommand.ExecuteCommand(this, Base, userValue);
        _internalChange = false;
    }

    private void OnChangeByModel(IUnitItem modelValue)
    {
        _internalChange = true;
        SelectedItem.Value = modelValue;
        _internalChange = false;
    }

    public bool Filter(string search)
    {
        var isNameMatch = _searchService.Match(Base.Name, search, out var nameMatch);
        var isDescriptionMatch = _searchService.Match(
            Base.Description,
            search,
            out var descriptionMatch
        );
        var isCurrentNameMatch = _searchService.Match(
            Base.CurrentUnitItem.CurrentValue.Name,
            search,
            out var selectedUnitItemNameMatch
        );
        var isCurrentSymbolMatch = _searchService.Match(
            Base.CurrentUnitItem.CurrentValue.Symbol,
            search,
            out var selectedUnitItemSymbolMatch
        );

        var isDefaultNameMatch = _searchService.Match(
            Base.InternationalSystemUnit.Name,
            search,
            out var defaultUnitItemNameMatch
        );
        var isDefaultSymbolMatch = _searchService.Match(
            Base.InternationalSystemUnit.Symbol,
            search,
            out var defaultUnitItemSymbolMatch
        );

        // TODO: implement selections for every column
        NameSelection = nameMatch;
        DescriptionSelection = descriptionMatch;

        return isNameMatch
            || isDescriptionMatch
            || isCurrentNameMatch
            || isCurrentSymbolMatch
            || isDefaultNameMatch
            || isDefaultSymbolMatch;
    }

    #region Dispose

    private readonly IDisposable _sub1;
    private readonly IDisposable _sub2;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sub1.Dispose();
            _sub2.Dispose();
            SelectedItem.Dispose();
        }

        base.Dispose(disposing);
    }

    #endregion
}
