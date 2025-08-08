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

    private readonly IUnit _xUnit;
    private readonly IUnit _yUnit;
    private readonly IUnit _zUnit;

    private readonly List<INotifyDataErrorInfo> _validateProperties = new();
    private readonly BindableReactiveProperty<bool> _hasChanges = new();
    private readonly BindableReactiveProperty<bool> _hasValidationError = new();

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

        _xUnit = unitService.Units[LongitudeBase.Id];
        _yUnit = unitService.Units[LatitudeBase.Id];
        _zUnit = unitService.Units[AltitudeBase.Id];

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

        AddToValidation(XCoord = new BindableReactiveProperty<string>(), XCoordValidate);
        AddToValidation(YCoord = new BindableReactiveProperty<string>(), YCoordValidate);
        AddToValidation(ZCoord = new BindableReactiveProperty<string>(), ZCoordValidate);

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

    private Exception? ZCoordValidate(string arg)
    {
        var validationResult = _zUnit.CurrentUnitItem.CurrentValue.ValidateValue(arg);

        if (validationResult.IsFailed)
        {
            return new Exception("Invalid format");
        }

        return null;
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

    private void UpdateValidationStatus((object? sender, DataErrorsChangedEventArgs e) valueTuple)
    {
        _hasValidationError.Value = _validateProperties.Any(x => x.HasErrors);
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
        var doubleyValue = _yUnit.CurrentUnitItem.CurrentValue.ParseToSi(YCoord.Value);
        var doublexValue = _xUnit.CurrentUnitItem.CurrentValue.ParseToSi(XCoord.Value);
        var doublezValue = _xUnit.CurrentUnitItem.CurrentValue.ParseToSi(ZCoord.Value);

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
            var xStr = _xUnit.CurrentUnitItem.CurrentValue.PrintFromSi(res.X);
            var yStr = _yUnit.CurrentUnitItem.CurrentValue.PrintFromSi(res.Y);
            var zStr = _zUnit.CurrentUnitItem.CurrentValue.PrintFromSi(res.Z);

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
