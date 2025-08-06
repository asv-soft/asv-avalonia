using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia.GeoMap;
using Asv.Avalonia.Plugins;
using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;

namespace Asv.Avalonia.Example;

// TODO:Localize
[ExportPage(PageId)]
public class DialogBoardViewModel : PageViewModel<DialogBoardViewModel>
{
    public const string PageId = "dialog";
    public const MaterialIconKind PageIcon = MaterialIconKind.Dialogue;

    private readonly ILoggerFactory _loggerFactory;
    private readonly INavigationService _navigationService;

    public BindableReactiveProperty<string?> XCoord { get; }
    public BindableReactiveProperty<string?> YCoord { get; }
    public BindableReactiveProperty<string?> ZCoord { get; }

    public BindableReactiveProperty<string> XUnitName { get; }
    public BindableReactiveProperty<string> YUnitName { get; }
    public BindableReactiveProperty<string> ZUnitName { get; }

    private readonly HistoricalUnitProperty _historicalXUnit;
    private readonly HistoricalUnitProperty _historicalYUnit;
    private readonly HistoricalUnitProperty _historicalZUnit;

    private readonly IDisposable _subX;
    private readonly IDisposable _subY;
    private readonly IDisposable _subZ;

    private readonly SelectFolderDialogDesktopPrefab _selectFolderDialog;
    private readonly ObserveFolderDialogPrefab _observeFolderDialog;
    private readonly SaveFileDialogDesktopPrefab _saveFileDialog;
    private readonly OpenFileDialogDesktopPrefab _openFileDialog;
    private readonly SaveCancelDialogPrefab _saveCancelDialog;
    private readonly YesOrNoDialogPrefab _yesNoDialog;
    private readonly InputDialogPrefab _inputDialog;
    private readonly PositionDialogPrefab _positionDialog;

    public DialogBoardViewModel()
        : this(
            DesignTime.CommandService,
            NullLoggerFactory.Instance,
            NullDialogService.Instance,
            NullNavigationService.Instance,
            NullUnitService.Instance
        )
    {
        DesignTime.ThrowIfNotDesignMode();
        Title = RS.DialogPageViewModel_Title;
    }

    [ImportingConstructor]
    public DialogBoardViewModel(
        ICommandService cmd,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        INavigationService navigationService,
        IUnitService unitService
    )
        : base(PageId, cmd, loggerFactory)
    {
        Title = RS.DialogPageViewModel_Title;

        _loggerFactory = loggerFactory;
        _navigationService = navigationService;

        _selectFolderDialog = dialogService.GetDialogPrefab<SelectFolderDialogDesktopPrefab>();
        _observeFolderDialog = dialogService.GetDialogPrefab<ObserveFolderDialogPrefab>();
        _saveFileDialog = dialogService.GetDialogPrefab<SaveFileDialogDesktopPrefab>();
        _openFileDialog = dialogService.GetDialogPrefab<OpenFileDialogDesktopPrefab>();
        _saveCancelDialog = dialogService.GetDialogPrefab<SaveCancelDialogPrefab>();
        _yesNoDialog = dialogService.GetDialogPrefab<YesOrNoDialogPrefab>();
        _inputDialog = dialogService.GetDialogPrefab<InputDialogPrefab>();
        _positionDialog = dialogService.GetDialogPrefab<PositionDialogPrefab>();

        OpenFileCommand = new ReactiveCommand(OpenFileAsync);
        SaveFileCommand = new ReactiveCommand(SaveFileAsync);
        SelectFolderCommand = new ReactiveCommand(SelectFolderAsync);
        ObserveFolderCommand = new ReactiveCommand(ObserveFolderAsync);
        YesNoCommand = new ReactiveCommand(YesNoMessageAsync);
        SaveCancelCommand = new ReactiveCommand(SaveCancelAsync);
        ShowUnitInputCommand = new ReactiveCommand(ShowUnitInputAsync);
        OpenPositionDialogCommand = new ReactiveCommand(ShowPositionDialog);

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

        _subX = XCoord.EnableValidationRoutable(
            value =>
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return new Exception(RS.PositionDialogViewModel_ParamIsRequired);
                }

                var fmt = _historicalXUnit.Unit.CurrentUnitItem.CurrentValue.ValidateValue(value);
                if (fmt.IsFailed)
                {
                    return new Exception(RS.PositionDialogViewModel_ParamIsRequired);
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
                    return new Exception(RS.PositionDialogViewModel_ParamIsRequired);
                }

                var fmt = _historicalYUnit.Unit.CurrentUnitItem.CurrentValue.ValidateValue(value);
                if (fmt.IsFailed)
                {
                    return new Exception(RS.PositionDialogViewModel_InvalidFormat);
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
                    return new Exception(RS.PositionDialogViewModel_ParamIsRequired);
                }

                var fmt = _historicalZUnit.Unit.CurrentUnitItem.CurrentValue.ValidateValue(value);
                if (fmt.IsFailed)
                {
                    return new Exception(RS.PositionDialogViewModel_InvalidFormat);
                }

                double si = _historicalZUnit.Unit.CurrentUnitItem.CurrentValue.ParseToSi(value);
                return ValidationResult.Success;
            },
            this,
            true
        );

        SetInitialCoordinates(1, 1, 1);
    }

    public ReactiveCommand OpenPositionDialogCommand { get; }
    public ReactiveCommand OpenFileCommand { get; }
    public ReactiveCommand SaveFileCommand { get; }
    public ReactiveCommand SelectFolderCommand { get; }
    public ReactiveCommand ObserveFolderCommand { get; }
    public ReactiveCommand YesNoCommand { get; }
    public ReactiveCommand SaveCancelCommand { get; }
    public ReactiveCommand ShowUnitInputCommand { get; }

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

    private async ValueTask OpenFileAsync(Unit unit, CancellationToken cancellationToken)
    {
        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            var payload = new OpenFileDialogPayload { Title = "Open File" };

            var result = await _openFileDialog.ShowDialogAsync(payload);
            Logger.LogInformation("OpenFile result = {result}", result);
        }
    }

    private async ValueTask SaveFileAsync(Unit unit, CancellationToken cancellationToken)
    {
        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            var payload = new SaveFileDialogPayload { Title = "Save File" };

            var result = await _saveFileDialog.ShowDialogAsync(payload);
            Logger.LogInformation("SaveFile result = {result}", result);
        }
    }

    private async ValueTask SelectFolderAsync(Unit unit, CancellationToken cancellationToken)
    {
        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            var payload = new SelectFolderDialogPayload { Title = "Select Folder File" };

            var result = await _selectFolderDialog.ShowDialogAsync(payload);
            Logger.LogInformation("SelectFolder result = {result}", result);
        }
    }

    private async ValueTask ObserveFolderAsync(Unit unit, CancellationToken cancellationToken)
    {
        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            var payload = new ObserveFolderDialogPayload
            {
                Title = "Observe Folder",
                DefaultPath = Environment.CurrentDirectory,
            };

            var result = await _observeFolderDialog.ShowDialogAsync(payload);
            Logger.LogInformation("ObserveFolder result = {result}", result);
        }
    }

    private async ValueTask YesNoMessageAsync(Unit unit, CancellationToken cancellationToken)
    {
        var payload = new YesOrNoDialogPayload
        {
            Title = "Предупреждение",
            Message = "Вы действительно хотите выйти?",
        };

        var res = await _yesNoDialog.ShowDialogAsync(payload);
        Logger.LogInformation("YesNo result = {res}", res);
    }

    private async ValueTask SaveCancelAsync(Unit unit, CancellationToken cancellationToken)
    {
        var payload = new SaveCancelDialogPayload { Title = "Сохранение", Message = "Сохранить?" };

        var res = await _saveCancelDialog.ShowDialogAsync(payload);
        Logger.LogInformation("SaveCancel result = {res}", res);
    }

    private async ValueTask ShowUnitInputAsync(Unit unit, CancellationToken cancellationToken)
    {
        var payload = new InputDialogPayload
        {
            Title = "Поиск",
            Message = "Введите значение файла",
        };

        var res = await _inputDialog.ShowDialogAsync(payload);
        Logger.LogInformation("UnitInput result = {res}", res);
    }

    private async ValueTask ShowPositionDialog(Unit unit, CancellationToken cancellationToken)
    {
        var doubleyValue = _historicalYUnit.Unit.CurrentUnitItem.CurrentValue.ParseToSi(
            YCoord.Value
        );
        var doublexValue = _historicalXUnit.Unit.CurrentUnitItem.CurrentValue.ParseToSi(
            XCoord.Value
        );
        var doublezValue = _historicalZUnit.Unit.CurrentUnitItem.CurrentValue.ParseToSi(
            ZCoord.Value
        );

        var payload = new PositionDialogPayload
        {
            X = doublexValue,
            Y = doubleyValue,
            Z = doublezValue,
        };

        var res = await _positionDialog.ShowDialogAsync(payload);
        Logger.LogInformation("Coordinates result = {res}", res);

        if (res is not null)
        {
            var xStr = _historicalYUnit.Unit.CurrentUnitItem.CurrentValue.PrintFromSi(
                res.Value.Longitude
            );
            var yStr = _historicalXUnit.Unit.CurrentUnitItem.CurrentValue.PrintFromSi(
                res.Value.Latitude
            );
            var zStr = _historicalZUnit.Unit.CurrentUnitItem.CurrentValue.PrintFromSi(
                res.Value.Altitude
            );

            XCoord.Value = xStr;
            YCoord.Value = yStr;
            ZCoord.Value = zStr;
        }
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    protected override void AfterLoadExtensions() { }

    public override IExportInfo Source => SystemModule.Instance;
}
