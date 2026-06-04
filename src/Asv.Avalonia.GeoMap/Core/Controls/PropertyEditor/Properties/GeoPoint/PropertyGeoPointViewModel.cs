using Asv.Avalonia;
using Asv.Common;
using Asv.Modeling;
using Material.Icons;
using R3;
using AvaloniaRS = Asv.Avalonia.RS;

namespace Asv.Avalonia.GeoMap;

public abstract class PropertyGeoPointViewModel : PropertyViewModel
{
    private readonly ReactiveProperty<double> _altitude;
    private readonly ReactiveProperty<double> _latitude;
    private readonly ReactiveProperty<double> _longitude;
    private readonly GeoPointDialogPrefab? _geoPointDialog;
    private readonly IUndoChangeSink<ValueUndoChange<string>> _undoValueSink;
    private GeoPoint _lastValue;
    private bool _updatingFromModel;

    protected PropertyGeoPointViewModel(
        string id,
        IUnitService unitService,
        IDialogService dialogService
    )
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(unitService);
        ArgumentNullException.ThrowIfNull(dialogService);

        _latitude = new ReactiveProperty<double>().AddTo(ref DisposableBag);
        _longitude = new ReactiveProperty<double>().AddTo(ref DisposableBag);
        _altitude = new ReactiveProperty<double>().AddTo(ref DisposableBag);

        _latitude.Skip(1).Subscribe(_ => ApplyValueFromComponents()).AddTo(ref DisposableBag);
        _longitude.Skip(1).Subscribe(_ => ApplyValueFromComponents()).AddTo(ref DisposableBag);
        _altitude.Skip(1).Subscribe(_ => ApplyValueFromComponents()).AddTo(ref DisposableBag);

        _undoValueSink = Undo.Register<ValueUndoChange<string>>("Value", OnUndoValue, OnRedoValue)
            .AddTo(ref DisposableBag);

        CanOpenGeoPointDialog =
            dialogService.TryGetDialogPrefab(out _geoPointDialog) && _geoPointDialog is not null;
        OpenGeoPointDialogCommand = new ReactiveCommand(
            (_, cancel) => OpenGeoPointDialog(cancel),
            AwaitOperation.Drop
        ).AddTo(ref DisposableBag);

        Latitude = new PropertyUnitReactive(
            nameof(Latitude),
            unitService[LatitudeUnit.Id] ?? throw new NullReferenceException("Latitude Unit"),
            _latitude,
            format: "F7",
            enableValueUndo: false
        )
        {
            ShortHeader = AvaloniaRS.Latitude_ShortName,
            Icon = MaterialIconKind.Latitude,
            IconColor = AsvColorKind.Unknown,
        }
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        Longitude = new PropertyUnitReactive(
            nameof(Longitude),
            unitService[LongitudeUnit.Id] ?? throw new NullReferenceException("Longitude Unit"),
            _longitude,
            format: "F7",
            enableValueUndo: false
        )
        {
            ShortHeader = AvaloniaRS.Longitude_ShortName,
            Icon = MaterialIconKind.Longitude,
            IconColor = AsvColorKind.Unknown,
        }
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        Altitude = new PropertyUnitReactive(
            nameof(Altitude),
            unitService[AltitudeUnit.Id] ?? throw new NullReferenceException("Altitude Unit"),
            _altitude,
            format: "F2",
            enableValueUndo: false
        )
        {
            ShortHeader = AvaloniaRS.Altitude_ShortName,
            Icon = MaterialIconKind.Altimeter,
            IconColor = AsvColorKind.Unknown,
        }
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
    }

    private ValueTask OnRedoValue(ValueUndoChange<string> change, CancellationToken cancel)
    {
        var value = GeoPoint.Parse(change.NewValue);
        ApplyValueFromModel(value);
        return ApplyFromUser(value, cancel);
    }

    private ValueTask OnUndoValue(ValueUndoChange<string> change, CancellationToken cancel)
    {
        var value = GeoPoint.Parse(change.OldValue);
        ApplyValueFromModel(value);
        return ApplyFromUser(value, cancel);
    }

    public PropertyUnitViewModel Latitude { get; }
    public PropertyUnitViewModel Longitude { get; }
    public PropertyUnitViewModel Altitude { get; }
    public ReactiveCommand OpenGeoPointDialogCommand { get; }
    public bool CanOpenGeoPointDialog { get; }

    public override IEnumerable<IViewModel> GetChildren()
    {
        yield return Latitude;
        yield return Longitude;
        yield return Altitude;
        foreach (var item in base.GetChildren())
        {
            yield return item;
        }
    }

    protected virtual void ApplyValueFromModel(GeoPoint value)
    {
        _lastValue = value;
        _updatingFromModel = true;
        try
        {
            _latitude.Value = value.Latitude;
            _longitude.Value = value.Longitude;
            _altitude.Value = value.Altitude;
        }
        finally
        {
            _updatingFromModel = false;
        }
    }

    protected abstract ValueTask ApplyFromUser(GeoPoint value, CancellationToken cancel);

    private async ValueTask OpenGeoPointDialog(CancellationToken cancel)
    {
        if (_geoPointDialog is null || IsBusy)
        {
            return;
        }

        ClearModelErrors();
        IsBusy = true;
        try
        {
            var value = await _geoPointDialog.ShowDialogAsync(
                new GeoPointDialogPayload { InitialLocation = GetCurrentValue() }
            );

            if (value is null)
            {
                return;
            }

            await ApplyValueFromUser(value.Value, true, cancel);
        }
        catch (OperationCanceledException) when (cancel.IsCancellationRequested) { }
        catch (Exception e)
        {
            ApplyErrorFromModel(e);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyValueFromComponents()
    {
        if (_updatingFromModel)
        {
            return;
        }

        _ = ApplyValueFromUser(GetCurrentValue(), false, CancellationToken.None);
    }

    private async ValueTask ApplyValueFromUser(
        GeoPoint value,
        bool updateView,
        CancellationToken cancel
    )
    {
        var oldValue = _lastValue;
        if (EqualityComparer<GeoPoint>.Default.Equals(oldValue, value))
        {
            return;
        }

        if (updateView)
        {
            ApplyValueFromModel(value);
        }

        _undoValueSink.Publish(oldValue.ToString(), value.ToString());
        await ApplyFromUser(value, cancel);
    }

    private GeoPoint GetCurrentValue()
    {
        return new GeoPoint(_latitude.Value, _longitude.Value, _altitude.Value);
    }
}
