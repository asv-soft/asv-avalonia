using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Asv.Avalonia.GeoMap;
using Asv.Common;
using Asv.Mavlink;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;

namespace Asv.Avalonia.Example
{
    public class PositionDialogViewModel : DialogViewModelBase
    {
        public const string DialogId = "dialog.position";

        private readonly List<INotifyDataErrorInfo> _validateProperties;
        private readonly BindableReactiveProperty<bool> _hasChanges;
        private readonly BindableReactiveProperty<bool> _hasValidationError;
        private readonly CompositeDisposable _disposables = new();
        private MapAnchor<IMapAnchor> _anchor;
        private readonly IUnit _xUnit;
        private readonly IUnit _yUnit;
        private readonly IUnit _zUnit;

        public ObservableCollection<IMapAnchor> Anchors { get; }
        public BindableReactiveProperty<string?> StepInput { get; private set; }
        public BindableReactiveProperty<string?> XCoord { get; }
        public BindableReactiveProperty<string?> YCoord { get; }
        public BindableReactiveProperty<string?> ZCoord { get; }
        public IReadOnlyBindableReactiveProperty<string> XUnitName { get; }
        public IReadOnlyBindableReactiveProperty<string> YUnitName { get; private set; }
        public IReadOnlyBindableReactiveProperty<string> ZUnitName { get; private set; }
        public IReadOnlyBindableReactiveProperty<double> SelectedStep { get; }
        public IEnumerable<string> StepOptions { get; set; }

        public ReactiveCommand MoveUpCommand { get; }
        public ReactiveCommand MoveDownCommand { get; }
        public ReactiveCommand MoveLeftCommand { get; }
        public ReactiveCommand MoveRightCommand { get; }
        public ReactiveCommand MoveTopLeftCommand { get; }
        public ReactiveCommand MoveTopRightCommand { get; }
        public ReactiveCommand MoveBottomLeftCommand { get; }
        public ReactiveCommand MoveBottomRightCommand { get; }
        public ReactiveCommand IncreaseZCommand { get; }
        public ReactiveCommand DecreaseZCommand { get; }

        public PositionDialogViewModel()
            : this(NullLoggerFactory.Instance, NullUnitService.Instance)
        {
            DesignTime.ThrowIfNotDesignMode();
        }

        public PositionDialogViewModel(ILoggerFactory loggerFactory, IUnitService unitService)
            : base(DialogId, loggerFactory)
        {
            _xUnit = unitService.Units[LongitudeBase.Id];
            _yUnit = unitService.Units[LatitudeBase.Id];
            _zUnit = unitService.Units[AltitudeBase.Id];

            _hasChanges = new BindableReactiveProperty<bool>().DisposeItWith(Disposable);
            _hasValidationError = new BindableReactiveProperty<bool>().DisposeItWith(Disposable);
            _validateProperties = new List<INotifyDataErrorInfo>();

            XCoord = new BindableReactiveProperty<string?>("0.0").DisposeItWith(Disposable);
            YCoord = new BindableReactiveProperty<string?>("0.0").DisposeItWith(Disposable);
            ZCoord = new BindableReactiveProperty<string?>("0.0").DisposeItWith(Disposable);

            XUnitName = _xUnit
                .CurrentUnitItem.Select(u => u.Symbol)
                .ToBindableReactiveProperty<string>();
            YUnitName = _yUnit
                .CurrentUnitItem.Select(u => u.Symbol)
                .ToBindableReactiveProperty<string>();
            ZUnitName = _zUnit
                .CurrentUnitItem.Select(u => u.Symbol)
                .ToBindableReactiveProperty<string>();

            StepOptions = new List<string> { "1", "10", "50", "100", "5000", "10000", "50000" };

            SelectedStep = new BindableReactiveProperty<double>(1.0).DisposeItWith(Disposable);

            StepInput = new BindableReactiveProperty<string?>("1.0").DisposeItWith(Disposable);

            IncreaseZCommand = new ReactiveCommand(_ => ChangeZ(+SelectedStep.Value)).AddTo(
                _disposables
            );
            DecreaseZCommand = new ReactiveCommand(_ => ChangeZ(-SelectedStep.Value)).AddTo(
                _disposables
            );

            MoveUpCommand = new ReactiveCommand(_ => Move(0, +SelectedStep.Value)).AddTo(
                _disposables
            );
            MoveDownCommand = new ReactiveCommand(_ => Move(0, -SelectedStep.Value)).AddTo(
                _disposables
            );
            MoveLeftCommand = new ReactiveCommand(_ => Move(-SelectedStep.Value, 0)).AddTo(
                _disposables
            );
            MoveRightCommand = new ReactiveCommand(_ => Move(+SelectedStep.Value, 0)).AddTo(
                _disposables
            );

            MoveTopLeftCommand = new ReactiveCommand(_ =>
                Move(-SelectedStep.Value, +SelectedStep.Value)
            ).AddTo(_disposables);
            MoveTopRightCommand = new ReactiveCommand(_ =>
                Move(+SelectedStep.Value, +SelectedStep.Value)
            ).AddTo(_disposables);
            MoveBottomLeftCommand = new ReactiveCommand(_ =>
                Move(-SelectedStep.Value, -SelectedStep.Value)
            ).AddTo(_disposables);
            MoveBottomRightCommand = new ReactiveCommand(_ =>
                Move(+SelectedStep.Value, -SelectedStep.Value)
            ).AddTo(_disposables);

            _anchor = new MapAnchor<IMapAnchor>("anchor", DesignTime.LoggerFactory)
            {
                Icon = MaterialIconKind.MapMarker,
            };

            Anchors = new ObservableCollection<IMapAnchor> { _anchor };

            AddToValidation(XCoord = new BindableReactiveProperty<string>(), XCoordValidate);
            AddToValidation(YCoord = new BindableReactiveProperty<string>(), YCoordValidate);
            AddToValidation(ZCoord = new BindableReactiveProperty<string>(), ZCoordValidate);

            _disposables.Add(XCoord.Subscribe(_ => OnXCoordChanged()));
            _disposables.Add(YCoord.Subscribe(_ => OnYCoordChanged()));
            _disposables.Add(ZCoord.Subscribe(_ => OnZCoordChanged()));
        }

        public new void Dispose()
        {
            XUnitName.Dispose();
            _disposables.Clear();

            base.Dispose();
        }

        private Exception? ZCoordValidate(string arg)
        {
            var validationResult = _zUnit.CurrentUnitItem.CurrentValue.ValidateValue(arg);

            if (validationResult.IsFailed)
            {
                return new Exception("Invalid format");
            }

            return null;
        }

        private Exception? XCoordValidate(string arg)
        {
            var validationResult = _xUnit.CurrentUnitItem.CurrentValue.ValidateValue(arg);

            if (validationResult.IsFailed)
            {
                return new Exception("Invalid format");
            }

            var doubleValue = _xUnit.CurrentUnitItem.CurrentValue.ParseToSi(arg);
            var printedSi = _xUnit.CurrentUnitItem.CurrentValue.PrintFromSi(doubleValue);

            XCoord.Value = printedSi;

            return null;
        }

        private Exception? YCoordValidate(string arg)
        {
            var validationResult = _yUnit.CurrentUnitItem.CurrentValue.ValidateValue(arg);

            if (validationResult.IsFailed)
            {
                return new Exception("Invalid format");
            }

            var doubleValue = _yUnit.CurrentUnitItem.CurrentValue.ParseToSi(arg);
            var printedSi = _yUnit.CurrentUnitItem.CurrentValue.PrintFromSi(doubleValue);

            YCoord.Value = printedSi;

            return null;
        }

        private void AddToValidation<T>(
            BindableReactiveProperty<T> validateProperty,
            Func<T, Exception?> validator
        )
        {
            _validateProperties.Add(validateProperty);
            validateProperty.EnableValidation(validator);
            validateProperty.DisposeItWith(Disposable);
            Observable
                .FromEventHandler<DataErrorsChangedEventArgs>(
                    h => validateProperty.ErrorsChanged += h,
                    h => validateProperty.ErrorsChanged -= h
                )
                .Subscribe(UpdateValidationStatus)
                .DisposeItWith(Disposable);
            validateProperty.Subscribe(x => _hasChanges.Value = true).DisposeItWith(Disposable);
        }

        private void UpdateValidationStatus(
            (object? sender, DataErrorsChangedEventArgs e) valueTuple
        )
        {
            _hasValidationError.Value = _validateProperties.Any(x => x.HasErrors);
        }

        public void SetInitialCoordinates(double? x, double? y, double? z)
        {
            if (x.HasValue)
            {
                var xStr = _xUnit.CurrentUnitItem.CurrentValue.PrintFromSi(x.Value);
                XCoord.Value = xStr;
            }

            if (y.HasValue)
            {
                var yStr = _yUnit.CurrentUnitItem.CurrentValue.PrintFromSi(y.Value);
                YCoord.Value = yStr;
            }

            if (z.HasValue)
            {
                var zStr = _zUnit.CurrentUnitItem.CurrentValue.PrintFromSi(z.Value);
                ZCoord.Value = zStr;
            }
        }

        private void OnYCoordChanged()
        {
            UpdateAnchorLocation();
        }

        private void ChangeZ(double delta)
        {
            var currentSi = _zUnit.CurrentUnitItem.CurrentValue.ParseToSi(ZCoord.Value);
            var updatedSi = currentSi + delta;
            var printedSi = _zUnit.CurrentUnitItem.CurrentValue.PrintFromSi(updatedSi);

            if (_zUnit.CurrentUnitItem.CurrentValue.ValidateValue(printedSi).IsSuccess)
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
            var currentLon = _xUnit.CurrentUnitItem.CurrentValue.ParseToSi(XCoord.Value);
            var currentLat = _yUnit.CurrentUnitItem.CurrentValue.ParseToSi(YCoord.Value);

            double distance = Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
            double azimuthRad = Math.Atan2(deltaX, deltaY); // X = east, Y = north
            double azimuthDeg = GeoMath.RadiansToDegrees(azimuthRad);

            GeoPoint newPoint = GeoMath.RadialPoint(
                currentLat,
                currentLon,
                0,
                distance,
                azimuthDeg
            );

            string newXStr = _xUnit.CurrentUnitItem.CurrentValue.PrintFromSi(newPoint.Longitude);
            string newYStr = _yUnit.CurrentUnitItem.CurrentValue.PrintFromSi(newPoint.Latitude);

            if (_xUnit.CurrentUnitItem.CurrentValue.ValidateValue(newXStr).IsSuccess)
            {
                XCoord.Value = newXStr;
            }

            if (_yUnit.CurrentUnitItem.CurrentValue.ValidateValue(newYStr).IsSuccess)
            {
                YCoord.Value = newYStr;
            }
        }

        private double? TryParseCoord(BindableReactiveProperty<string?> coord, IUnit unit)
        {
            var value = coord.Value;

            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var validationResult = unit.CurrentUnitItem.CurrentValue.ValidateValue(value);
            if (validationResult.IsFailed)
            {
                return null;
            }

            try
            {
                return unit.CurrentUnitItem.CurrentValue.ParseToSi(value);
            }
            catch
            {
                return null;
            }
        }

        public CoordinatesDialogResult? GetResult()
        {
            var x = TryParseCoord(XCoord, _xUnit);
            var y = TryParseCoord(YCoord, _yUnit);
            var z = TryParseCoord(ZCoord, _zUnit);

            if (x is null || y is null || z is null)
            {
                return null;
            }

            return new CoordinatesDialogResult
            {
                X = x.Value,
                Y = y.Value,
                Z = z.Value,
            };
        }

        private void UpdateAnchorLocation()
        {
            var x = TryParseCoord(XCoord, _xUnit);
            var y = TryParseCoord(YCoord, _yUnit);
            var z = TryParseCoord(ZCoord, _zUnit);

            if (x != null && y != null && z != null)
            {
                _anchor.Location = new GeoPoint(y.Value, x.Value, z.Value);
            }
        }

        public override IEnumerable<IRoutable> GetRoutableChildren()
        {
            return [];
        }
    }
}
