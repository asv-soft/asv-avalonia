using Asv.Common;
using R3;

namespace Asv.Avalonia.GeoMap;

public class PropertyGeoPointReactive : PropertyGeoPointViewModel
{
    private readonly ReactiveProperty<GeoPoint> _model;

    public PropertyGeoPointReactive(
        string id,
        ReactiveProperty<GeoPoint> model,
        IUnitService unitService
    )
        : this(id, model, unitService, DesignTime.DialogService) { }

    public PropertyGeoPointReactive(
        string id,
        ReactiveProperty<GeoPoint> model,
        IUnitService unitService,
        IDialogService dialogService
    )
        : base(id, unitService, dialogService)
    {
        ArgumentNullException.ThrowIfNull(model);

        _model = model;
        _model
            .DistinctUntilChanged()
            .ObserveOnUIThreadDispatcher()
            .Subscribe(ApplyValueFromModel)
            .AddTo(ref DisposableBag);
    }

    protected override ValueTask ApplyFromUser(GeoPoint value, CancellationToken cancel)
    {
        _model.OnNext(value);
        return ValueTask.CompletedTask;
    }
}
