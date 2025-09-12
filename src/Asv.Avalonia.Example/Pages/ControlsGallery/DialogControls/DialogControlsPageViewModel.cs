using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia.Example.Commands;
using Asv.Avalonia.GeoMap;
using Asv.Common;
using Avalonia.Media.Imaging;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;

namespace Asv.Avalonia.Example;

[ExportControlExamples(PageId)]
public class DialogControlsPageViewModel : ControlsGallerySubPage, IResettable
{
    public const string PageId = "dialog_controls";
    public const MaterialIconKind PageIcon = MaterialIconKind.Dialogue;

    private readonly ILoggerFactory _loggerFactory;
    private readonly INavigationService _navigationService;

    private readonly ReactiveProperty<string?> _customDialogTitle;
    private readonly ReactiveProperty<bool> _customDialogIsImageContent;
    private readonly ReactiveProperty<string?> _customDialogMessage;
    private readonly ReactiveProperty<string?> _customDialogImagePath;
    private readonly ReactiveProperty<string?> _customDialogPrimaryButtonText;
    private readonly ReactiveProperty<bool> _customDialogIsPrimaryButtonEnabled;
    private readonly ReactiveProperty<string?> _customDialogSecondaryButtonText;
    private readonly ReactiveProperty<bool> _customDialogIsSecondaryButtonEnabled;

    private readonly ReactiveProperty<Enum> _customDialogResult;
    private readonly ReactiveProperty<Enum> _yesOrNoDialogResult;
    private readonly ReactiveProperty<Enum> _saveCancelDialogResult;
    private readonly ReactiveProperty<string?> _showInputDialogResult;
    private readonly ReactiveProperty<string?> _showHotKeyCaptureDialogResult;
    private readonly ReactiveProperty<GeoPoint> _geoPointDialogResult;
    private readonly ReactiveProperty<string?> _openFileDialogResult;
    private readonly ReactiveProperty<string?> _saveFileDialogResult;
    private readonly ReactiveProperty<string?> _selectFolderDialogResult;
    private readonly ReactiveProperty<string?> _observeFolderDialogResult;

    #region Prefabs

    private readonly HotKeyCaptureDialogPrefab _hotKeyCaptureDialog;
    private readonly InputDialogPrefab _inputDialog;
    private readonly ObserveFolderDialogPrefab _observeFolderDialog;
    private readonly OpenFileDialogDesktopPrefab _openFileDialog;
    private readonly SaveCancelDialogPrefab _saveCancelDialog;
    private readonly SaveFileDialogDesktopPrefab _saveFileDialog;
    private readonly SelectFolderDialogDesktopPrefab _selectFolderDialog;
    private readonly YesOrNoDialogPrefab _yesOrNoDialog;
    private readonly GeoPointDialogPrefab _geoPointDialog;

    #endregion

    public DialogControlsPageViewModel()
        : this(
            NullLoggerFactory.Instance,
            NullDialogService.Instance,
            NullNavigationService.Instance,
            NullUnitService.Instance
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public DialogControlsPageViewModel(
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        INavigationService navigationService,
        IUnitService unitService
    )
        : base(PageId, loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _navigationService = navigationService;

        #region Units

        var latUnit = unitService.Units[LatitudeBase.Id];
        var lonUnit = unitService.Units[LongitudeBase.Id];
        var altUnit = unitService.Units[AltitudeBase.Id];

        LonUnitName = lonUnit
            .CurrentUnitItem.Select(item => item.Symbol)
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);
        LatUnitName = latUnit
            .CurrentUnitItem.Select(item => item.Symbol)
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);
        AltUnitName = altUnit
            .CurrentUnitItem.Select(item => item.Symbol)
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);

        #endregion

        #region Fetching prefabs

        _openFileDialog = dialogService.GetDialogPrefab<OpenFileDialogDesktopPrefab>();
        _saveFileDialog = dialogService.GetDialogPrefab<SaveFileDialogDesktopPrefab>();
        _selectFolderDialog = dialogService.GetDialogPrefab<SelectFolderDialogDesktopPrefab>();
        _observeFolderDialog = dialogService.GetDialogPrefab<ObserveFolderDialogPrefab>();
        _yesOrNoDialog = dialogService.GetDialogPrefab<YesOrNoDialogPrefab>();
        _saveCancelDialog = dialogService.GetDialogPrefab<SaveCancelDialogPrefab>();
        _inputDialog = dialogService.GetDialogPrefab<InputDialogPrefab>();
        _hotKeyCaptureDialog = dialogService.GetDialogPrefab<HotKeyCaptureDialogPrefab>();
        _geoPointDialog = dialogService.GetDialogPrefab<GeoPointDialogPrefab>();

        #endregion

        #region Custom dialog properties

        _customDialogTitle = new ReactiveProperty<string?>(
            RS.DialogControlsPageView_CustomDialog_Title
        ).DisposeItWith(Disposable);
        _customDialogIsImageContent = new ReactiveProperty<bool>().DisposeItWith(Disposable);
        _customDialogMessage = new ReactiveProperty<string?>(
            RS.DialogControlsPageView_CustomDialog_Content
        ).DisposeItWith(Disposable);
        _customDialogImagePath = new ReactiveProperty<string?>().DisposeItWith(Disposable);
        _customDialogPrimaryButtonText = new ReactiveProperty<string?>(
            RS.DialogControlsPageView_CustomDialog_Content
        ).DisposeItWith(Disposable);
        _customDialogIsPrimaryButtonEnabled = new ReactiveProperty<bool>(true).DisposeItWith(
            Disposable
        );
        _customDialogSecondaryButtonText = new ReactiveProperty<string?>(
            RS.DialogControlsPageView_CustomDialog_Content
        ).DisposeItWith(Disposable);
        _customDialogIsSecondaryButtonEnabled = new ReactiveProperty<bool>(true).DisposeItWith(
            Disposable
        );

        CustomDialogTitle = new HistoricalStringProperty(
            nameof(CustomDialogTitle),
            _customDialogTitle,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        CustomDialogIsImageContent = new HistoricalBoolProperty(
            nameof(CustomDialogIsImageContent),
            _customDialogIsImageContent,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        CustomDialogMessage = new HistoricalStringProperty(
            nameof(CustomDialogMessage),
            _customDialogMessage,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        CustomDialogImagePath = new HistoricalStringProperty(
            nameof(CustomDialogImagePath),
            _customDialogImagePath,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        CustomDialogPrimaryButtonText = new HistoricalStringProperty(
            nameof(CustomDialogPrimaryButtonText),
            _customDialogPrimaryButtonText,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        CustomDialogIsPrimaryButtonEnabled = new HistoricalBoolProperty(
            nameof(CustomDialogIsPrimaryButtonEnabled),
            _customDialogIsPrimaryButtonEnabled,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        CustomDialogSecondaryButtonText = new HistoricalStringProperty(
            nameof(CustomDialogSecondaryButtonText),
            _customDialogSecondaryButtonText,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        CustomDialogIsSecondaryButtonEnabled = new HistoricalBoolProperty(
            nameof(CustomDialogIsSecondaryButtonEnabled),
            _customDialogIsSecondaryButtonEnabled,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);

        #endregion

        #region Dialog results

        _customDialogResult = new ReactiveProperty<Enum>(ContentDialogResult.None).DisposeItWith(
            Disposable
        );
        _yesOrNoDialogResult = new ReactiveProperty<Enum>(
            ConfirmationStatus.Undefined
        ).DisposeItWith(Disposable);
        _saveCancelDialogResult = new ReactiveProperty<Enum>(
            ConfirmationStatus.Undefined
        ).DisposeItWith(Disposable);
        _showInputDialogResult = new ReactiveProperty<string?>().DisposeItWith(Disposable);
        _showHotKeyCaptureDialogResult = new ReactiveProperty<string?>().DisposeItWith(Disposable);
        _geoPointDialogResult = new ReactiveProperty<GeoPoint>(GeoPoint.Zero).DisposeItWith(
            Disposable
        );
        _openFileDialogResult = new ReactiveProperty<string?>().DisposeItWith(Disposable);
        _saveFileDialogResult = new ReactiveProperty<string?>().DisposeItWith(Disposable);
        _selectFolderDialogResult = new ReactiveProperty<string?>().DisposeItWith(Disposable);
        _observeFolderDialogResult = new ReactiveProperty<string?>().DisposeItWith(Disposable);

        CustomDialogResult = new HistoricalEnumProperty<ContentDialogResult>(
            nameof(CustomDialogResult),
            _customDialogResult,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        YesOrNoDialogResult = new HistoricalEnumProperty<ConfirmationStatus>(
            nameof(YesOrNoDialogResult),
            _yesOrNoDialogResult,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        SaveCancelDialogResult = new HistoricalEnumProperty<ConfirmationStatus>(
            nameof(SaveCancelDialogResult),
            _saveCancelDialogResult,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        ShowInputDialogResult = new HistoricalStringProperty(
            nameof(ShowInputDialogResult),
            _showInputDialogResult,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        ShowHotKeyCaptureDialogResult = new HistoricalStringProperty(
            nameof(ShowHotKeyCaptureDialogResult),
            _showHotKeyCaptureDialogResult,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        GeoPointDialogResult = new HistoricalGeoPointProperty(
            nameof(GeoPointDialogResult),
            _geoPointDialogResult,
            latUnit,
            lonUnit,
            altUnit,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        OpenFileDialogResult = new HistoricalStringProperty(
            nameof(OpenFileDialogResult),
            _openFileDialogResult,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        SaveFileDialogResult = new HistoricalStringProperty(
            nameof(SaveFileDialogResult),
            _saveFileDialogResult,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        SelectFolderDialogResult = new HistoricalStringProperty(
            nameof(SelectFolderDialogResult),
            _selectFolderDialogResult,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        ObserveFolderDialogResult = new HistoricalStringProperty(
            nameof(ObserveFolderDialogResult),
            _observeFolderDialogResult,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);

        #endregion

        #region Show commands

        ShowCustomDialogCommand = new ReactiveCommand(OpenCustomAsync).DisposeItWith(Disposable);
        OpenImageForCustomDialogAsyncCommand = new ReactiveCommand(
            SelectImageForCustomDialogAsync
        ).DisposeItWith(Disposable);
        OpenFileCommand = new ReactiveCommand(OpenFileAsync).DisposeItWith(Disposable);
        SaveFileCommand = new ReactiveCommand(SaveFileAsync).DisposeItWith(Disposable);
        SelectFolderCommand = new ReactiveCommand(SelectFolderAsync).DisposeItWith(Disposable);
        ObserveFolderCommand = new ReactiveCommand(ObserveFolderAsync).DisposeItWith(Disposable);
        YesOrNoCommand = new ReactiveCommand(YesOrNoMessageAsync).DisposeItWith(Disposable);
        SaveCancelCommand = new ReactiveCommand(SaveCancelAsync).DisposeItWith(Disposable);
        ShowInputCommand = new ReactiveCommand(ShowInputAsync).DisposeItWith(Disposable);
        ShowHotKeyCaptureCommand = new ReactiveCommand(ShowHotKeyCaptureAsync).DisposeItWith(
            Disposable
        );
        OpenGeoPointDialogCommand = new ReactiveCommand(ShowGeoPointDialog).DisposeItWith(
            Disposable
        );

        #endregion

        #region Reset commands

        ResetCustomResultCommand = new ReactiveCommand(
            (_, _) =>
            {
                CustomDialogResult.ViewValue.Value = ContentDialogResult.None;
                return ValueTask.CompletedTask;
            }
        ).DisposeItWith(Disposable);
        ResetOpenFileCommand = new ReactiveCommand(
            (_, _) =>
            {
                OpenFileDialogResult.ViewValue.Value = null;
                return ValueTask.CompletedTask;
            }
        ).DisposeItWith(Disposable);
        ResetSaveFileCommand = new ReactiveCommand(
            (_, _) =>
            {
                SaveFileDialogResult.ViewValue.Value = null;
                return ValueTask.CompletedTask;
            }
        ).DisposeItWith(Disposable);
        ResetSelectFolderCommand = new ReactiveCommand(
            (_, _) =>
            {
                SelectFolderDialogResult.ViewValue.Value = null;
                return ValueTask.CompletedTask;
            }
        ).DisposeItWith(Disposable);
        ResetObserveFolderCommand = new ReactiveCommand(
            (_, _) =>
            {
                ObserveFolderDialogResult.ViewValue.Value = null;
                return ValueTask.CompletedTask;
            }
        ).DisposeItWith(Disposable);
        ResetYerOrNoCommand = new ReactiveCommand(
            (_, _) =>
            {
                YesOrNoDialogResult.ViewValue.Value = ConfirmationStatus.Undefined;
                return ValueTask.CompletedTask;
            }
        ).DisposeItWith(Disposable);
        ResetSaveCancelResultCommand = new ReactiveCommand(
            (_, _) =>
            {
                SaveCancelDialogResult.ViewValue.Value = ConfirmationStatus.Undefined;
                return ValueTask.CompletedTask;
            }
        ).DisposeItWith(Disposable);
        ResetShowInputResultCommand = new ReactiveCommand(
            (_, _) =>
            {
                ShowInputDialogResult.ViewValue.Value = null;
                return ValueTask.CompletedTask;
            }
        ).DisposeItWith(Disposable);
        ResetShowHotKeyResultCommand = new ReactiveCommand(
            (_, _) =>
            {
                ShowHotKeyCaptureDialogResult.ViewValue.Value = null;
                return ValueTask.CompletedTask;
            }
        ).DisposeItWith(Disposable);
        ResetOpenGeoPointResultCommand = new ReactiveCommand(
            async (_, ct) =>
            {
                await this.ExecuteCommand(
                    ResetGeoPointDialogResultCommand.Id,
                    CommandArg.CreateDictionary(
                        new Dictionary<string, CommandArg>
                        {
                            ["lon"] = CommandArg.Double(0),
                            ["lat"] = CommandArg.Double(0),
                            ["alt"] = CommandArg.Double(0),
                        }
                    ),
                    ct
                );
            }
        ).DisposeItWith(Disposable);

        #endregion

        GeoPointDialogResult.ForceValidate();
    }

    public ReactiveCommand ShowCustomDialogCommand { get; }
    public ReactiveCommand OpenImageForCustomDialogAsyncCommand { get; }
    public ReactiveCommand YesOrNoCommand { get; }
    public ReactiveCommand SaveCancelCommand { get; }
    public ReactiveCommand ShowInputCommand { get; }
    public ReactiveCommand ShowHotKeyCaptureCommand { get; }
    public ReactiveCommand OpenGeoPointDialogCommand { get; }
    public ReactiveCommand OpenFileCommand { get; }
    public ReactiveCommand SaveFileCommand { get; }
    public ReactiveCommand SelectFolderCommand { get; }
    public ReactiveCommand ObserveFolderCommand { get; }

    public ReactiveCommand ResetCustomResultCommand { get; }
    public ReactiveCommand ResetYerOrNoCommand { get; }
    public ReactiveCommand ResetSaveCancelResultCommand { get; }
    public ReactiveCommand ResetShowInputResultCommand { get; }
    public ReactiveCommand ResetShowHotKeyResultCommand { get; }
    public ReactiveCommand ResetOpenGeoPointResultCommand { get; }
    public ReactiveCommand ResetOpenFileCommand { get; }
    public ReactiveCommand ResetSaveFileCommand { get; }
    public ReactiveCommand ResetSelectFolderCommand { get; }
    public ReactiveCommand ResetObserveFolderCommand { get; }

    public HistoricalEnumProperty<ContentDialogResult> CustomDialogResult { get; }
    public HistoricalEnumProperty<ConfirmationStatus> YesOrNoDialogResult { get; }
    public HistoricalEnumProperty<ConfirmationStatus> SaveCancelDialogResult { get; }
    public HistoricalStringProperty ShowInputDialogResult { get; }
    public HistoricalStringProperty ShowHotKeyCaptureDialogResult { get; }
    public HistoricalGeoPointProperty GeoPointDialogResult { get; }
    public HistoricalStringProperty OpenFileDialogResult { get; }
    public HistoricalStringProperty SaveFileDialogResult { get; }
    public HistoricalStringProperty SelectFolderDialogResult { get; }
    public HistoricalStringProperty ObserveFolderDialogResult { get; }

    public HistoricalStringProperty CustomDialogTitle { get; }
    public HistoricalBoolProperty CustomDialogIsImageContent { get; }
    public HistoricalStringProperty CustomDialogMessage { get; }
    public HistoricalStringProperty CustomDialogImagePath { get; }
    public HistoricalStringProperty CustomDialogPrimaryButtonText { get; }
    public HistoricalBoolProperty CustomDialogIsPrimaryButtonEnabled { get; }
    public HistoricalStringProperty CustomDialogSecondaryButtonText { get; }
    public HistoricalBoolProperty CustomDialogIsSecondaryButtonEnabled { get; }

    public IReadOnlyBindableReactiveProperty<string> LonUnitName { get; }
    public IReadOnlyBindableReactiveProperty<string> LatUnitName { get; }
    public IReadOnlyBindableReactiveProperty<string> AltUnitName { get; }

    private async ValueTask OpenCustomAsync(Unit unit, CancellationToken cancellationToken)
    {
        var vmMessage = CustomDialogMessage.ViewValue.Value ?? string.Empty;

        DialogViewModelBase? vm = null;
        Bitmap? bitmap = null;

        try
        {
            if (CustomDialogIsImageContent.ViewValue.Value)
            {
                bitmap = new Bitmap(CustomDialogImagePath.ViewValue.Value ?? string.Empty);
                vm = new DialogItemImageViewModel(_loggerFactory) { Image = bitmap };
            }
            else
            {
                vm = new DialogItemTextViewModel(_loggerFactory) { Message = vmMessage };
            }

            var dialogContent = new ContentDialog(vm, _navigationService)
            {
                Title = CustomDialogTitle.ViewValue.Value ?? string.Empty,
                PrimaryButtonText = CustomDialogPrimaryButtonText.ViewValue.Value ?? string.Empty,
                SecondaryButtonText =
                    CustomDialogSecondaryButtonText.ViewValue.Value ?? string.Empty,
                IsPrimaryButtonEnabled = CustomDialogIsPrimaryButtonEnabled.ViewValue.Value,
                IsSecondaryButtonEnabled = CustomDialogIsSecondaryButtonEnabled.ViewValue.Value,
            };

            if (dialogContent is { IsPrimaryButtonEnabled: false, IsSecondaryButtonEnabled: false })
            {
                dialogContent.CloseButtonText = RS.DialogControlsPageViewModel_Custom_Close;
            }

            var result = await dialogContent.ShowAsync();

            CustomDialogResult.ViewValue.Value = result;

            var resultToLog = result switch
            {
                ContentDialogResult.None => RS.DialogControlsPageViewModel_CancelResult,
                ContentDialogResult.Primary => RS.DialogControlsPageViewModel_Custom_Primary,
                ContentDialogResult.Secondary => RS.DialogControlsPageViewModel_Custom_Secondary,
                _ => string.Empty,
            };
            var msg = string.Format(RS.DialogControlsPageViewModel_Custom_Result, resultToLog);
            Logger.LogInformation("(CustomDialog) {msg}", msg);
        }
        finally
        {
            vm?.Dispose();
            bitmap?.Dispose();
        }
    }

    private async ValueTask SelectImageForCustomDialogAsync(
        Unit unit,
        CancellationToken cancellationToken
    )
    {
        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            var payload = new OpenFileDialogPayload
            {
                Title = RS.DialogControlsPageViewModel_OpenFile_Title,
            };

            CustomDialogImagePath.ViewValue.Value = await _openFileDialog.ShowDialogAsync(payload);
        }
    }

    private async ValueTask OpenFileAsync(Unit unit, CancellationToken cancellationToken)
    {
        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            var payload = new OpenFileDialogPayload
            {
                Title = RS.DialogControlsPageViewModel_OpenFile_Title,
            };

            var rawResult = await _openFileDialog.ShowDialogAsync(payload);
            var result = rawResult ?? $"({RS.DialogControlsPageViewModel_CancelResult})";

            OpenFileDialogResult.ViewValue.Value = result;

            var msg = string.Format(RS.DialogControlsPageViewModel_OpenFile_Result, result);
            Logger.LogInformation("(OpenFileDialog) {msg}", msg);
        }
    }

    private async ValueTask SaveFileAsync(Unit unit, CancellationToken cancellationToken)
    {
        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            var payload = new SaveFileDialogPayload
            {
                Title = RS.DialogControlsPageViewModel_SaveFile_Title,
            };

            var rawResult = await _saveFileDialog.ShowDialogAsync(payload);
            var result = rawResult ?? $"({RS.DialogControlsPageViewModel_CancelResult})";

            SaveFileDialogResult.ViewValue.Value = result;

            var msg = string.Format(RS.DialogControlsPageViewModel_SaveFile_Result, result);
            Logger.LogInformation("(SaveFileDialog) {msg}", msg);
        }
    }

    private async ValueTask SelectFolderAsync(Unit unit, CancellationToken cancellationToken)
    {
        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            var payload = new SelectFolderDialogPayload
            {
                Title = RS.DialogControlsPageViewModel_SelectFolder_Title,
            };

            var rawResult = await _selectFolderDialog.ShowDialogAsync(payload);
            var result = rawResult ?? $"({RS.DialogControlsPageViewModel_CancelResult})";

            SelectFolderDialogResult.ViewValue.Value = result;

            var msg = string.Format(RS.DialogControlsPageViewModel_SelectFolder_Result, result);
            Logger.LogInformation("(SelectFolderDialog) {msg}", msg);
        }
    }

    private async ValueTask ObserveFolderAsync(Unit unit, CancellationToken cancellationToken)
    {
        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            var payload = new ObserveFolderDialogPayload
            {
                Title = RS.DialogControlsPageViewModel_ObserveFolder_Title,
                DefaultPath = Environment.CurrentDirectory,
            };

            await _observeFolderDialog.ShowDialogAsync(payload);

            ObserveFolderDialogResult.ViewValue.Value =
                RS.DialogControlsPageViewModel_ObserveFolder_ResultShort;

            var msg = string.Format(
                RS.DialogControlsPageViewModel_ObserveFolder_Result,
                payload.DefaultPath
            );
            Logger.LogInformation("(ObserveFolderDialog) {msg}", msg);
        }
    }

    private async ValueTask YesOrNoMessageAsync(Unit unit, CancellationToken cancellationToken)
    {
        var payload = new YesOrNoDialogPayload
        {
            Title = RS.DialogControlsPageViewModel_YesOrNo_Title,
            Message = RS.DialogControlsPageViewModel_YesOrNo_Message,
        };

        var result = await _yesOrNoDialog.ShowDialogAsync(payload);

        YesOrNoDialogResult.ViewValue.Value = result
            ? ConfirmationStatus.Yes
            : ConfirmationStatus.No;

        var msg = string.Format(RS.DialogControlsPageViewModel_YesOrNo_Result, result);
        Logger.LogInformation("(YesOrNoDialog) {msg}", msg);
    }

    private async ValueTask SaveCancelAsync(Unit unit, CancellationToken cancellationToken)
    {
        var payload = new SaveCancelDialogPayload
        {
            Title = RS.DialogControlsPageViewModel_Save_Title,
            Message = RS.DialogControlsPageViewModel_Save_Message,
        };

        var result = await _saveCancelDialog.ShowDialogAsync(payload);

        SaveCancelDialogResult.ViewValue.Value = result
            ? ConfirmationStatus.Yes
            : ConfirmationStatus.No;

        var msg = string.Format(RS.DialogControlsPageViewModel_Save_Result, result);
        Logger.LogInformation("(SaveCancelDialog) {msg}", msg);
    }

    private async ValueTask ShowInputAsync(Unit unit, CancellationToken cancellationToken)
    {
        var payload = new InputDialogPayload
        {
            Title = RS.DialogControlsPageViewModel_Input_Title,
            Message = RS.DialogControlsPageViewModel_Input_Message,
        };

        var rawResult = await _inputDialog.ShowDialogAsync(payload);
        var result = rawResult ?? $"({RS.DialogControlsPageViewModel_CancelResult})";

        ShowInputDialogResult.ViewValue.Value = result;

        var msg = string.Format(RS.DialogControlsPageViewModel_Input_Result, result);
        Logger.LogInformation("(InputDialog) {msg}", msg);
    }

    private async ValueTask ShowHotKeyCaptureAsync(Unit unit, CancellationToken cancellationToken)
    {
        var payload = new HotKeyCaptureDialogPayload
        {
            Title = RS.DialogControlsPageViewModel_HotKeyCapture_Title,
            Message = RS.DialogControlsPageViewModel_HotKeyCapture_Message,
        };

        var result = await _hotKeyCaptureDialog.ShowDialogAsync(payload);

        ShowHotKeyCaptureDialogResult.ViewValue.Value = result;

        var msg = string.Format(
            RS.DialogControlsPageViewModel_HotKeyCapture_Result,
            result ?? RS.DialogControlsPageViewModel_CancelResult
        );
        Logger.LogInformation("(HotKeyCaptureDialog) {msg}", msg);
    }

    private async ValueTask ShowGeoPointDialog(Unit unit, CancellationToken cancellationToken)
    {
        var payload = new GeoPointDialogPayload
        {
            InitialLocation = GeoPointDialogResult.ModelValue.CurrentValue,
        };

        var rawResult = await _geoPointDialog.ShowDialogAsync(payload);

        if (rawResult is not null)
        {
            GeoPointDialogResult.ModelValue.Value = rawResult.Value;
        }

        var result = rawResult?.ToString() ?? $"({RS.DialogControlsPageViewModel_CancelResult})";
        var msg = string.Format(RS.DialogControlsPageViewModel_GeoPoint_Result, result);
        Logger.LogInformation("(GeoPointDialog) {msg}", msg);
    }

    #region Reset implementation

    public CommandArg Reset(CommandArg value)
    {
        var oldValue = CommandArg.CreateDictionary(
            new Dictionary<string, CommandArg>
            {
                [nameof(CustomDialogTitle)] = CommandArg.CreateString(
                    CustomDialogTitle.ModelValue.Value ?? string.Empty
                ),
                [nameof(CustomDialogIsImageContent)] = CommandArg.CreateBool(
                    CustomDialogIsImageContent.ModelValue.Value
                ),
                [nameof(CustomDialogMessage)] = CommandArg.CreateString(
                    CustomDialogMessage.ModelValue.Value ?? string.Empty
                ),
                [nameof(CustomDialogImagePath)] = CommandArg.CreateString(
                    CustomDialogImagePath.ModelValue.Value ?? string.Empty
                ),
                [nameof(CustomDialogPrimaryButtonText)] = CommandArg.CreateString(
                    CustomDialogPrimaryButtonText.ModelValue.Value ?? string.Empty
                ),
                [nameof(CustomDialogIsPrimaryButtonEnabled)] = CommandArg.CreateBool(
                    CustomDialogIsPrimaryButtonEnabled.ModelValue.Value
                ),
                [nameof(CustomDialogSecondaryButtonText)] = CommandArg.CreateString(
                    CustomDialogSecondaryButtonText.ModelValue.Value ?? string.Empty
                ),
                [nameof(CustomDialogIsSecondaryButtonEnabled)] = CommandArg.CreateBool(
                    CustomDialogIsSecondaryButtonEnabled.ModelValue.Value
                ),

                [nameof(CustomDialogResult)] = CommandArg.CreateInteger(
                    (int)CustomDialogResult.ViewValue.Value
                ),
                [nameof(YesOrNoDialogResult)] = CommandArg.CreateInteger(
                    (int)YesOrNoDialogResult.ViewValue.Value
                ),
                [nameof(SaveCancelDialogResult)] = CommandArg.CreateInteger(
                    (int)SaveCancelDialogResult.ViewValue.Value
                ),
                [nameof(ShowInputDialogResult)] = CommandArg.CreateString(
                    ShowInputDialogResult.ModelValue.Value ?? string.Empty
                ),
                [nameof(ShowHotKeyCaptureDialogResult)] = CommandArg.CreateString(
                    ShowHotKeyCaptureDialogResult.ModelValue.Value ?? string.Empty
                ),
                [nameof(OpenFileDialogResult)] = CommandArg.CreateString(
                    OpenFileDialogResult.ModelValue.Value ?? string.Empty
                ),
                [nameof(SaveFileDialogResult)] = CommandArg.CreateString(
                    SaveFileDialogResult.ModelValue.Value ?? string.Empty
                ),
                [nameof(SelectFolderDialogResult)] = CommandArg.CreateString(
                    SelectFolderDialogResult.ModelValue.Value ?? string.Empty
                ),
                [nameof(ObserveFolderDialogResult)] = CommandArg.CreateString(
                    ObserveFolderDialogResult.ModelValue.Value ?? string.Empty
                ),

                [nameof(GeoPointDialogResult)] = CommandArg.CreateDictionary(
                    CommandArg.CreateDictionary(
                        new Dictionary<string, CommandArg>
                        {
                            ["lon"] = CommandArg.CreateDouble(
                                GeoPointDialogResult.Longitude.ModelValue.Value
                            ),
                            ["lat"] = CommandArg.CreateDouble(
                                GeoPointDialogResult.Latitude.ModelValue.Value
                            ),
                            ["alt"] = CommandArg.CreateDouble(
                                GeoPointDialogResult.Altitude.ModelValue.Value
                            ),
                        }
                    )
                ),
            }
        );

        if (value is not DictArg dictArg)
        {
            if (value is not EmptyArg)
            {
                throw new CommandArgMismatchException(typeof(DictArg));
            }

            dictArg = CommandArg.CreateDictionary(
                new Dictionary<string, CommandArg>
                {
                    [nameof(CustomDialogTitle)] = CommandArg.CreateString(
                        RS.DialogControlsPageView_CustomDialog_Title
                    ),
                    [nameof(CustomDialogIsImageContent)] = CommandArg.CreateBool(false),
                    [nameof(CustomDialogMessage)] = CommandArg.CreateString(
                        RS.DialogControlsPageView_CustomDialog_Content
                    ),
                    [nameof(CustomDialogImagePath)] = CommandArg.CreateString(string.Empty),
                    [nameof(CustomDialogPrimaryButtonText)] = CommandArg.CreateString(
                        RS.DialogControlsPageView_CustomDialog_Content
                    ),
                    [nameof(CustomDialogIsPrimaryButtonEnabled)] = CommandArg.CreateBool(true),
                    [nameof(CustomDialogSecondaryButtonText)] = CommandArg.CreateString(
                        RS.DialogControlsPageView_CustomDialog_Content
                    ),
                    [nameof(CustomDialogIsSecondaryButtonEnabled)] = CommandArg.CreateBool(true),

                    [nameof(CustomDialogResult)] = CommandArg.CreateInteger(
                        (int)ContentDialogResult.None
                    ),
                    [nameof(YesOrNoDialogResult)] = CommandArg.CreateInteger(
                        (int)ConfirmationStatus.Undefined
                    ),
                    [nameof(SaveCancelDialogResult)] = CommandArg.CreateInteger(
                        (int)ConfirmationStatus.Undefined
                    ),
                    [nameof(ShowInputDialogResult)] = CommandArg.CreateString(string.Empty),
                    [nameof(ShowHotKeyCaptureDialogResult)] = CommandArg.CreateString(string.Empty),
                    [nameof(OpenFileDialogResult)] = CommandArg.CreateString(string.Empty),
                    [nameof(SaveFileDialogResult)] = CommandArg.CreateString(string.Empty),
                    [nameof(SelectFolderDialogResult)] = CommandArg.CreateString(string.Empty),
                    [nameof(ObserveFolderDialogResult)] = CommandArg.CreateString(string.Empty),

                    [nameof(GeoPointDialogResult)] = CommandArg.CreateDictionary(
                        CommandArg.CreateDictionary(
                            new Dictionary<string, CommandArg>
                            {
                                ["lon"] = CommandArg.CreateDouble(0),
                                ["lat"] = CommandArg.CreateDouble(0),
                                ["alt"] = CommandArg.CreateDouble(0),
                            }
                        )
                    ),
                }
            );
        }

        CustomDialogTitle.ModelValue.Value = dictArg[nameof(CustomDialogTitle)].AsString();
        CustomDialogIsImageContent.ModelValue.Value = dictArg[nameof(CustomDialogIsImageContent)]
            .AsBool();
        CustomDialogMessage.ModelValue.Value = dictArg[nameof(CustomDialogMessage)].AsString();
        CustomDialogImagePath.ModelValue.Value = dictArg[nameof(CustomDialogImagePath)].AsString();
        CustomDialogPrimaryButtonText.ModelValue.Value = dictArg[
            nameof(CustomDialogPrimaryButtonText)
        ]
            .AsString();
        CustomDialogIsPrimaryButtonEnabled.ModelValue.Value = dictArg[
            nameof(CustomDialogIsPrimaryButtonEnabled)
        ]
            .AsBool();
        CustomDialogSecondaryButtonText.ModelValue.Value = dictArg[
            nameof(CustomDialogSecondaryButtonText)
        ]
            .AsString();
        CustomDialogIsSecondaryButtonEnabled.ModelValue.Value = dictArg[
            nameof(CustomDialogIsSecondaryButtonEnabled)
        ]
            .AsBool();

        CustomDialogResult.ModelValue.Value = (ContentDialogResult)
            dictArg[nameof(CustomDialogResult)].AsInt();
        YesOrNoDialogResult.ModelValue.Value = (ConfirmationStatus)
            dictArg[nameof(YesOrNoDialogResult)].AsInt();
        SaveCancelDialogResult.ModelValue.Value = (ConfirmationStatus)
            dictArg[nameof(SaveCancelDialogResult)].AsInt();
        ShowInputDialogResult.ModelValue.Value = dictArg[nameof(ShowInputDialogResult)].AsString();
        ShowHotKeyCaptureDialogResult.ModelValue.Value = dictArg[
            nameof(ShowHotKeyCaptureDialogResult)
        ]
            .AsString();
        OpenFileDialogResult.ModelValue.Value = dictArg[nameof(OpenFileDialogResult)].AsString();
        SaveFileDialogResult.ModelValue.Value = dictArg[nameof(SaveFileDialogResult)].AsString();
        SelectFolderDialogResult.ModelValue.Value = dictArg[nameof(SelectFolderDialogResult)]
            .AsString();
        ObserveFolderDialogResult.ModelValue.Value = dictArg[nameof(ObserveFolderDialogResult)]
            .AsString();

        var geopoint = dictArg[nameof(GeoPointDialogResult)].AsDictionary();
        if (
            geopoint["lon"] is not DoubleArg lonArg
            || geopoint["lat"] is not DoubleArg latArg
            || geopoint["alt"] is not DoubleArg altArg
        )
        {
            throw new CommandArgMismatchException(typeof(StringArg));
        }
        GeoPointDialogResult.ModelValue.Value = new GeoPoint(
            latArg.Value,
            lonArg.Value,
            altArg.Value
        );

        return oldValue;
    }

    #endregion

    public override IExportInfo Source => SystemModule.Instance;

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        foreach (var child in base.GetRoutableChildren())
        {
            yield return child;
        }

        yield return CustomDialogTitle;
        yield return CustomDialogIsImageContent;
        yield return CustomDialogMessage;
        yield return CustomDialogImagePath;
        yield return CustomDialogPrimaryButtonText;
        yield return CustomDialogIsPrimaryButtonEnabled;
        yield return CustomDialogSecondaryButtonText;
        yield return CustomDialogIsSecondaryButtonEnabled;

        yield return CustomDialogResult;
        yield return YesOrNoDialogResult;
        yield return SaveCancelDialogResult;
        yield return ShowInputDialogResult;
        yield return ShowHotKeyCaptureDialogResult;
        yield return GeoPointDialogResult;
        yield return OpenFileDialogResult;
        yield return SaveFileDialogResult;
        yield return SelectFolderDialogResult;
        yield return ObserveFolderDialogResult;
    }
}
