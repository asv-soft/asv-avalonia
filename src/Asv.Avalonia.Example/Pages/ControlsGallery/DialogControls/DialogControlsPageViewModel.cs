using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia.GeoMap;
using Asv.Common;
using Avalonia.Media.Imaging;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;

namespace Asv.Avalonia.Example;

[ExportControlExamples(PageId)]
public class DialogControlsPageViewModel : ControlsGallerySubPage
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
            RS.DialogControlsPageView_ContentDialog_Title
        ).DisposeItWith(Disposable);
        _customDialogIsImageContent = new ReactiveProperty<bool>().DisposeItWith(Disposable);
        _customDialogMessage = new ReactiveProperty<string?>(
            RS.DialogControlsPageView_ContentDialog_Content
        ).DisposeItWith(Disposable);
        _customDialogImagePath = new ReactiveProperty<string?>().DisposeItWith(Disposable);
        _customDialogPrimaryButtonText = new ReactiveProperty<string?>(
            RS.DialogControlsPageView_ContentDialog_Content
        ).DisposeItWith(Disposable);
        _customDialogIsPrimaryButtonEnabled = new ReactiveProperty<bool>(true).DisposeItWith(
            Disposable
        );
        _customDialogSecondaryButtonText = new ReactiveProperty<string?>(
            RS.DialogControlsPageView_ContentDialog_Content
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
        IsCustomDialogImageContent = new HistoricalBoolProperty(
            nameof(IsCustomDialogImageContent),
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
        IsCustomDialogPrimaryButtonEnabled = new HistoricalBoolProperty(
            nameof(IsCustomDialogPrimaryButtonEnabled),
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
        IsCustomDialogSecondaryButtonEnabled = new HistoricalBoolProperty(
            nameof(IsCustomDialogSecondaryButtonEnabled),
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

        ResetCustomResultCommand = new ReactiveCommand(_ =>
        {
            CustomDialogResult.ViewValue.Value = ContentDialogResult.None;
        }).DisposeItWith(Disposable);
        ResetOpenFileCommand = new ReactiveCommand(_ =>
        {
            OpenFileDialogResult.ViewValue.Value = null;
        }).DisposeItWith(Disposable);
        ResetSaveFileCommand = new ReactiveCommand(_ =>
        {
            SaveFileDialogResult.ViewValue.Value = null;
        }).DisposeItWith(Disposable);
        ResetSelectFolderCommand = new ReactiveCommand(_ =>
        {
            SelectFolderDialogResult.ViewValue.Value = null;
        }).DisposeItWith(Disposable);
        ResetObserveFolderCommand = new ReactiveCommand(_ =>
        {
            ObserveFolderDialogResult.ViewValue.Value = null;
        }).DisposeItWith(Disposable);
        ResetYerOrNoCommand = new ReactiveCommand(_ =>
        {
            YesOrNoDialogResult.ViewValue.Value = ConfirmationStatus.Undefined;
        }).DisposeItWith(Disposable);
        ResetSaveCancelResultCommand = new ReactiveCommand(_ =>
        {
            SaveCancelDialogResult.ViewValue.Value = ConfirmationStatus.Undefined;
        }).DisposeItWith(Disposable);
        ResetShowInputResultCommand = new ReactiveCommand(_ =>
        {
            ShowInputDialogResult.ViewValue.Value = null;
        }).DisposeItWith(Disposable);
        ResetShowHotKeyResultCommand = new ReactiveCommand(_ =>
        {
            ShowHotKeyCaptureDialogResult.ViewValue.Value = null;
        }).DisposeItWith(Disposable);
        ResetOpenGeoPointResultCommand = new ReactiveCommand(
            async (_, ct) =>
            {
                await this.ExecuteCommand(
                    ResetGeoPointDialogResultCommand.Id,
                    ResetGeoPointDialogResultCommandArg.Empty,
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
    public HistoricalBoolProperty IsCustomDialogImageContent { get; }
    public HistoricalStringProperty CustomDialogMessage { get; }
    public HistoricalStringProperty CustomDialogImagePath { get; }
    public HistoricalStringProperty CustomDialogPrimaryButtonText { get; }
    public HistoricalBoolProperty IsCustomDialogPrimaryButtonEnabled { get; }
    public HistoricalStringProperty CustomDialogSecondaryButtonText { get; }
    public HistoricalBoolProperty IsCustomDialogSecondaryButtonEnabled { get; }

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
            if (IsCustomDialogImageContent.ViewValue.Value)
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
                IsPrimaryButtonEnabled = IsCustomDialogPrimaryButtonEnabled.ViewValue.Value,
                IsSecondaryButtonEnabled = IsCustomDialogSecondaryButtonEnabled.ViewValue.Value,
            };

            if (dialogContent is { IsPrimaryButtonEnabled: false, IsSecondaryButtonEnabled: false })
            {
                dialogContent.CloseButtonText = RS.DialogControlsPageViewModel_ContentDialog_Close;
            }

            var result = await dialogContent.ShowAsync();

            CustomDialogResult.ViewValue.Value = result;

            var resultToLog = result switch
            {
                ContentDialogResult.None => RS.DialogControlsPageViewModel_CancelResult,
                ContentDialogResult.Primary => RS.DialogControlsPageViewModel_ContentDialog_Primary,
                ContentDialogResult.Secondary =>
                    RS.DialogControlsPageViewModel_ContentDialog_Secondary,
                _ => string.Empty,
            };
            var msg = string.Format(
                RS.DialogControlsPageViewModel_ContentDialog_Result,
                resultToLog
            );
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
                Title = RS.DialogControlsPageViewModel_OpenFilePrefab_Title,
                TypeFilter = "jpeg,jpg,png",
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
                Title = RS.DialogControlsPageViewModel_OpenFilePrefab_Title,
            };

            var rawResult = await _openFileDialog.ShowDialogAsync(payload);
            var result = rawResult ?? $"({RS.DialogControlsPageViewModel_CancelResult})";

            OpenFileDialogResult.ViewValue.Value = result;

            var msg = string.Format(RS.DialogControlsPageViewModel_OpenFilePrefab_Result, result);
            const string dialogName = nameof(OpenFileDialogDesktopPrefab);
            Logger.LogInformation("({dialogName}) {msg}", dialogName, msg);
        }
    }

    private async ValueTask SaveFileAsync(Unit unit, CancellationToken cancellationToken)
    {
        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            var payload = new SaveFileDialogPayload
            {
                Title = RS.DialogControlsPageViewModel_SaveFilePrefab_Title,
            };

            var rawResult = await _saveFileDialog.ShowDialogAsync(payload);
            var result = rawResult ?? $"({RS.DialogControlsPageViewModel_CancelResult})";

            SaveFileDialogResult.ViewValue.Value = result;

            var msg = string.Format(RS.DialogControlsPageViewModel_SaveFilePrefab_Result, result);
            const string dialogName = nameof(SaveFileDialogDesktopPrefab);
            Logger.LogInformation("({dialogName}) {msg}", dialogName, msg);
        }
    }

    private async ValueTask SelectFolderAsync(Unit unit, CancellationToken cancellationToken)
    {
        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            var payload = new SelectFolderDialogPayload
            {
                Title = RS.DialogControlsPageViewModel_SelectFolderPrefab_Title,
            };

            var rawResult = await _selectFolderDialog.ShowDialogAsync(payload);
            var result = rawResult ?? $"({RS.DialogControlsPageViewModel_CancelResult})";

            SelectFolderDialogResult.ViewValue.Value = result;

            var msg = string.Format(
                RS.DialogControlsPageViewModel_SelectFolderPrefab_Result,
                result
            );
            const string dialogName = nameof(SelectFolderDialogDesktopPrefab);
            Logger.LogInformation("({dialogName}) {msg}", dialogName, msg);
        }
    }

    private async ValueTask ObserveFolderAsync(Unit unit, CancellationToken cancellationToken)
    {
        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            var payload = new ObserveFolderDialogPayload
            {
                Title = RS.DialogControlsPageViewModel_ObserveFolderPrefab_Title,
                DefaultPath = Environment.CurrentDirectory,
            };

            await _observeFolderDialog.ShowDialogAsync(payload);

            ObserveFolderDialogResult.ViewValue.Value =
                RS.DialogControlsPageViewModel_ObserveFolderPrefab_ResultShort;

            var msg = string.Format(
                RS.DialogControlsPageViewModel_ObserveFolderPrefab_Result,
                payload.DefaultPath
            );
            const string dialogName = nameof(ObserveFolderDialogPrefab);
            Logger.LogInformation("({dialogName}) {msg}", dialogName, msg);
        }
    }

    private async ValueTask YesOrNoMessageAsync(Unit unit, CancellationToken cancellationToken)
    {
        var payload = new YesOrNoDialogPayload
        {
            Title = RS.DialogControlsPageViewModel_YesOrNoPrefab_Title,
            Message = RS.DialogControlsPageViewModel_YesOrNoPrefab_Message,
        };

        var result = await _yesOrNoDialog.ShowDialogAsync(payload);

        YesOrNoDialogResult.ViewValue.Value = result
            ? ConfirmationStatus.Yes
            : ConfirmationStatus.No;

        var msg = string.Format(RS.DialogControlsPageViewModel_YesOrNoPrefab_Result, result);
        const string dialogName = nameof(YesOrNoDialogPrefab);
        Logger.LogInformation("({dialogName}) {msg}", dialogName, msg);
    }

    private async ValueTask SaveCancelAsync(Unit unit, CancellationToken cancellationToken)
    {
        var payload = new SaveCancelDialogPayload
        {
            Title = RS.DialogControlsPageViewModel_SavePrefab_Title,
            Message = RS.DialogControlsPageViewModel_SavePrefab_Message,
        };

        var result = await _saveCancelDialog.ShowDialogAsync(payload);

        SaveCancelDialogResult.ViewValue.Value = result
            ? ConfirmationStatus.Yes
            : ConfirmationStatus.No;

        var msg = string.Format(RS.DialogControlsPageViewModel_SavePrefab_Result, result);
        const string dialogName = nameof(SaveCancelDialogPrefab);
        Logger.LogInformation("({dialogName}) {msg}", dialogName, msg);
    }

    private async ValueTask ShowInputAsync(Unit unit, CancellationToken cancellationToken)
    {
        var payload = new InputDialogPayload
        {
            Title = RS.DialogControlsPageViewModel_InputPrefab_Title,
            Message = RS.DialogControlsPageViewModel_InputPrefab_Message,
        };

        var rawResult = await _inputDialog.ShowDialogAsync(payload);
        var result = rawResult ?? $"({RS.DialogControlsPageViewModel_CancelResult})";

        ShowInputDialogResult.ViewValue.Value = result;

        var msg = string.Format(RS.DialogControlsPageViewModel_InputPrefab_Result, result);
        const string dialogName = nameof(InputDialogPrefab);
        Logger.LogInformation("({dialogName}) {msg}", dialogName, msg);
    }

    private async ValueTask ShowHotKeyCaptureAsync(Unit unit, CancellationToken cancellationToken)
    {
        var payload = new HotKeyCaptureDialogPayload
        {
            Title = RS.DialogControlsPageViewModel_HotKeyCapturePrefab_Title,
            Message = RS.DialogControlsPageViewModel_HotKeyCapturePrefab_Message,
        };

        var result = await _hotKeyCaptureDialog.ShowDialogAsync(payload);

        ShowHotKeyCaptureDialogResult.ViewValue.Value = result;

        var msg = string.Format(
            RS.DialogControlsPageViewModel_HotKeyCapturePrefab_Result,
            result ?? RS.DialogControlsPageViewModel_CancelResult
        );
        const string dialogName = nameof(HotKeyCaptureDialogPrefab);
        Logger.LogInformation("({dialogName}) {msg}", dialogName, msg);
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
        var msg = string.Format(RS.DialogControlsPageViewModel_GeoPointPrefab_Result, result);
        const string dialogName = nameof(GeoPointDialogPrefab);
        Logger.LogInformation("({dialogName}) {msg}", dialogName, msg);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        foreach (var child in base.GetRoutableChildren())
        {
            yield return child;
        }

        yield return CustomDialogTitle;
        yield return IsCustomDialogImageContent;
        yield return CustomDialogMessage;
        yield return CustomDialogImagePath;
        yield return CustomDialogPrimaryButtonText;
        yield return IsCustomDialogPrimaryButtonEnabled;
        yield return CustomDialogSecondaryButtonText;
        yield return IsCustomDialogSecondaryButtonEnabled;

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

    public override IExportInfo Source => SystemModule.Instance;
}
