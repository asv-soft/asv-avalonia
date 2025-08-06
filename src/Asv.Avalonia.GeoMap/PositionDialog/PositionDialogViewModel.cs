using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Asv.Avalonia.GeoMap;
using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;

namespace Asv.Avalonia.GeoMap
{
    public class PositionDialogViewModel : DialogViewModelBase, IDisposable
    {
        public const string DialogId = "dialog.position";

        private readonly CompositeDisposable _disposables = new();
        private readonly IDisposable _subX;
        private readonly IDisposable _subY;
        private readonly IDisposable _subZ;

        private MapAnchor<IMapAnchor> _anchor;

        private readonly HistoricalUnitProperty _historicalXUnit;
        private readonly HistoricalUnitProperty _historicalYUnit;
        private readonly HistoricalUnitProperty _historicalZUnit;

        public ObservableCollection<IMapAnchor> Anchors { get; }
        public BindableReactiveProperty<string?> StepInput { get; private set; }
        public BindableReactiveProperty<string?> XCoord { get; }
        public BindableReactiveProperty<string?> YCoord { get; }
        public BindableReactiveProperty<string?> ZCoord { get; }
        public BindableReactiveProperty<double> SelectedStep { get; }
        public IReadOnlyBindableReactiveProperty<string> XUnitName { get; }
        public IReadOnlyBindableReactiveProperty<string> YUnitName { get; private set; }
        public IReadOnlyBindableReactiveProperty<string> ZUnitName { get; private set; }
        public IEnumerable<string> StepOptions { get; set; }

        public PositionDialogViewModel()
            : this(NullLoggerFactory.Instance, NullUnitService.Instance)
        {
            DesignTime.ThrowIfNotDesignMode();
        }

        public PositionDialogViewModel(ILoggerFactory loggerFactory, IUnitService unitService)
            : base(DialogId, loggerFactory)
        {
            _historicalXUnit = new HistoricalUnitProperty(
                LongitudeBase.Id,
                new ReactiveProperty<double>(0.0),
                unitService.Units[LongitudeBase.Id],
                loggerFactory,
                this
            );

            _historicalYUnit = new HistoricalUnitProperty(
                LatitudeBase.Id,
                new ReactiveProperty<double>(0.0),
                unitService.Units[LatitudeBase.Id],
                loggerFactory,
                this
            );

            _historicalZUnit = new HistoricalUnitProperty(
                AltitudeBase.Id,
                new ReactiveProperty<double>(0.0),
                unitService.Units[AltitudeBase.Id],
                loggerFactory,
                this
            );

            XCoord = new BindableReactiveProperty<string?>("0.0").DisposeItWith(Disposable);
            YCoord = new BindableReactiveProperty<string?>("0.0").DisposeItWith(Disposable);
            ZCoord = new BindableReactiveProperty<string?>("0.0").DisposeItWith(Disposable);

            XUnitName = _historicalXUnit
                .Unit.CurrentUnitItem.Select(u => u.Symbol)
                .ToBindableReactiveProperty<string>();

            YUnitName = _historicalYUnit
                .Unit.CurrentUnitItem.Select(u => u.Symbol)
                .ToBindableReactiveProperty<string>();

            ZUnitName = _historicalZUnit
                .Unit.CurrentUnitItem.Select(u => u.Symbol)
                .ToBindableReactiveProperty<string>();

            StepOptions = new List<string> { "1", "10", "50", "100", "5000", "10000", "50000" };
            SelectedStep = new BindableReactiveProperty<double>(1.0).DisposeItWith(Disposable);
            StepInput = new BindableReactiveProperty<string?>("1.0").DisposeItWith(Disposable);

            _anchor = new MapAnchor<IMapAnchor>("anchor", DesignTime.LoggerFactory)
            {
                Icon = MaterialIconKind.MapMarker,
            };

            Anchors = new ObservableCollection<IMapAnchor> { _anchor };

            _subX = XCoord.EnableValidationRoutable(
                value =>
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        return new Exception(
                            RS.PositionDialogViewModel_XValidation_ParamIsRequired
                        );
                    }

                    var fmt = _historicalXUnit.Unit.CurrentUnitItem.CurrentValue.ValidateValue(
                        value
                    );
                    if (fmt.IsFailed)
                    {
                        return new Exception(RS.PositionDialogViewModel_XValidation_InvalidFormat);
                    }

                    double si = _historicalXUnit.Unit.CurrentUnitItem.CurrentValue.ParseToSi(value);
                    return ValidationResult.Success;
                },
                this,
                true
            );

            _subY = YCoord.EnableValidationRoutable(
                value =>
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        return new Exception(
                            RS.PositionDialogViewModel_YValidation_ParamIsRequired
                        );
                    }

                    var fmt = _historicalYUnit.Unit.CurrentUnitItem.CurrentValue.ValidateValue(
                        value
                    );
                    if (fmt.IsFailed)
                    {
                        return new Exception(RS.PositionDialogViewModel_YValidation_InvalidFormat);
                    }

                    double si = _historicalYUnit.Unit.CurrentUnitItem.CurrentValue.ParseToSi(value);
                    return ValidationResult.Success;
                },
                this,
                true
            );

            _subZ = ZCoord.EnableValidationRoutable(
                value =>
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        return new Exception(
                            RS.PositionDialogViewModel_ZValidation_ParamIsRequired
                        );
                    }

                    var fmt = _historicalZUnit.Unit.CurrentUnitItem.CurrentValue.ValidateValue(
                        value
                    );
                    if (fmt.IsFailed)
                    {
                        return new Exception(RS.PositionDialogViewModel_ZValidation_InvalidFormat);
                    }

                    double si = _historicalZUnit.Unit.CurrentUnitItem.CurrentValue.ParseToSi(value);
                    return ValidationResult.Success;
                },
                this,
                true
            );

            _disposables.Add(XCoord.Subscribe(_ => OnXCoordChanged()));
            _disposables.Add(YCoord.Subscribe(_ => OnYCoordChanged()));
            _disposables.Add(ZCoord.Subscribe(_ => OnZCoordChanged()));
        }

        public void IncreaseZ() => ChangeZ(+SelectedStep.Value);

        public void DecreaseZ() => ChangeZ(-SelectedStep.Value);

        public void MoveUp() => Move(0, +SelectedStep.Value);

        public void MoveDown() => Move(0, -SelectedStep.Value);

        public void MoveLeft() => Move(-SelectedStep.Value, 0);

        public void MoveRight() => Move(+SelectedStep.Value, 0);

        public void MoveTopLeft() => Move(-SelectedStep.Value, +SelectedStep.Value);

        public void MoveTopRight() => Move(+SelectedStep.Value, +SelectedStep.Value);

        public void MoveBottomLeft() => Move(-SelectedStep.Value, -SelectedStep.Value);

        public void MoveBottomRight() => Move(+SelectedStep.Value, -SelectedStep.Value);

        public new void Dispose()
        {
            _subX.Dispose();
            _subY.Dispose();
            _subZ.Dispose();

            XUnitName.Dispose();
            _disposables.Clear();

            base.Dispose();
        }

        private void OnYCoordChanged()
        {
            UpdateAnchorLocation();
        }

        private void ChangeZ(double delta)
        {
            var currentSi = _historicalZUnit.ModelValue.Value;
            var updatedSi = currentSi + delta;
            var printedSi = _historicalZUnit.Unit.CurrentUnitItem.CurrentValue.PrintFromSi(
                updatedSi
            );

            if (
                _historicalZUnit
                    .Unit.CurrentUnitItem.CurrentValue.ValidateValue(printedSi)
                    .IsSuccess
            )
            {
                ZCoord.Value = printedSi;
            }

            UpdateAnchorLocation();
        }

        private void OnXCoordChanged()
        {
            UpdateAnchorLocation();
        }

        private void OnZCoordChanged()
        {
            UpdateAnchorLocation();
        }

        private void Move(double deltaX, double deltaY)
        {
            var currentLon = _historicalXUnit.Unit.CurrentUnitItem.CurrentValue.ParseToSi(
                XCoord.Value
            );
            var currentLat = _historicalYUnit.Unit.CurrentUnitItem.CurrentValue.ParseToSi(
                YCoord.Value
            );

            double distance = Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
            double azimuthRad = Math.Atan2(deltaX, deltaY);
            double azimuthDeg = GeoMath.RadiansToDegrees(azimuthRad);

            GeoPoint newPoint = GeoMath.RadialPoint(
                currentLat,
                currentLon,
                0,
                distance,
                azimuthDeg
            );

            string newXStr = _historicalXUnit.Unit.CurrentUnitItem.CurrentValue.PrintFromSi(
                newPoint.Longitude
            );
            string newYStr = _historicalYUnit.Unit.CurrentUnitItem.CurrentValue.PrintFromSi(
                newPoint.Latitude
            );

            if (_historicalXUnit.Unit.CurrentUnitItem.CurrentValue.ValidateValue(newXStr).IsSuccess)
            {
                XCoord.Value = newXStr;
            }

            if (_historicalYUnit.Unit.CurrentUnitItem.CurrentValue.ValidateValue(newYStr).IsSuccess)
            {
                YCoord.Value = newYStr;
            }
        }

        private bool TryParseCoord(string? value, IUnit unit, out double result)
        {
            result = 0;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var validationResult = unit.CurrentUnitItem.CurrentValue.ValidateValue(value);
            if (validationResult.IsFailed)
            {
                return false;
            }

            try
            {
                result = unit.CurrentUnitItem.CurrentValue.ParseToSi(value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public GeoPoint? GetResult()
        {
            if (
                !TryParseCoord(XCoord.Value, _historicalXUnit.Unit, out var x)
                || !TryParseCoord(YCoord.Value, _historicalYUnit.Unit, out var y)
                || !TryParseCoord(ZCoord.Value, _historicalZUnit.Unit, out var z)
            )
            {
                return null;
            }

            return new GeoPoint(x, y, z);
        }

        private void UpdateAnchorLocation()
        {
            if (
                !TryParseCoord(XCoord.Value, _historicalXUnit.Unit, out var x)
                || !TryParseCoord(YCoord.Value, _historicalYUnit.Unit, out var y)
                || !TryParseCoord(ZCoord.Value, _historicalZUnit.Unit, out var z)
            )
            {
                return;
            }

            _anchor.Location = new GeoPoint(y, x, z);
        }

        public void SetInitialCoordinates(double? x, double? y, double? z)
        {
            if (x.HasValue)
            {
                var xStr = _historicalXUnit.Unit.CurrentUnitItem.CurrentValue.PrintFromSi(x.Value);
                XCoord.Value = xStr;
            }

            if (y.HasValue)
            {
                var yStr = _historicalYUnit.Unit.CurrentUnitItem.CurrentValue.PrintFromSi(y.Value);
                YCoord.Value = yStr;
            }

            if (z.HasValue)
            {
                var zStr = _historicalZUnit.Unit.CurrentUnitItem.CurrentValue.PrintFromSi(z.Value);
                ZCoord.Value = zStr;
            }
        }

        public override IEnumerable<IRoutable> GetRoutableChildren()
        {
            return new List<IRoutable>();
        }
    }
}
