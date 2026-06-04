using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia.GeoMap;
using Asv.Common;
using Asv.Modeling;
using Avalonia.Media.Imaging;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;

namespace Asv.Avalonia.Example;

public class DialogControlsPageViewModelConfig
{
    public string CustomDialogTitle { get; set; } = RS.DialogControlsPageView_ContentDialog_Title;
    public bool CustomDialogIsImageContent { get; set; }
    public string CustomDialogMessage { get; set; } =
        RS.DialogControlsPageView_ContentDialog_Content;
    public string? CustomDialogImagePath { get; set; }
    public string CustomDialogPrimaryButtonText { get; set; } =
        RS.DialogControlsPageView_ContentDialog_Content;
    public bool CustomDialogIsPrimaryButtonEnabled { get; set; } = true;
    public string CustomDialogSecondaryButtonText { get; set; } =
        RS.DialogControlsPageView_ContentDialog_Content;
    public bool CustomDialogIsSecondaryButtonEnabled { get; set; } = true;

    public ContentDialogResult CustomDialogResult { get; set; } = ContentDialogResult.None;
    public ConfirmationStatus YesOrNoDialogResult { get; set; } = ConfirmationStatus.Undefined;
    public ConfirmationStatus SaveCancelDialogResult { get; set; } = ConfirmationStatus.Undefined;
    public string? ShowInputDialogResult { get; set; }
    public string? ShowHotKeyCaptureDialogResult { get; set; }
    public GeoPoint GeoPointDialogResult { get; set; } = GeoPoint.Zero;
    public string? OpenFileDialogResult { get; set; }
    public string? SaveFileDialogResult { get; set; }
    public string? SelectFolderDialogResult { get; set; }
    public string? ObserveFolderDialogResult { get; set; }
}

public class DialogControlsPageViewModel : ControlsGallerySubPage
{
    public const string PageId = "dialog-controls";
    public const MaterialIconKind PageIcon = MaterialIconKind.Dialogue;

    private readonly ILoggerFactory _loggerFactory;
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
    private readonly Subject<Unit> _layoutChanged = new();

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
    private readonly ILogger<DialogControlsPageViewModel> _logger;

    #endregion

    public DialogControlsPageViewModel()
        : this(
            NullTreeSubPageContext<ControlsGalleryPageViewModel>.Instance,
            NullLoggerFactory.Instance,
            NullDialogService.Instance,
            NullUnitService.Instance
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public DialogControlsPageViewModel(
        ITreeSubPageContext<IControlsGalleryPage> context,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        IUnitService unitService
    )
        : base(PageId, context)
    {
        _layoutChanged.DisposeItWith(Disposable);
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<DialogControlsPageViewModel>();

        #region Units

        var latUnit = unitService.GetRequiredUnitOfType<LatitudeUnit>(LatitudeUnit.Id);
        var lonUnit = unitService.GetRequiredUnitOfType<LongitudeUnit>(LongitudeUnit.Id);
        var altUnit = unitService.GetRequiredUnitOfType<AltitudeUnit>(AltitudeUnit.Id);

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
            "custom-dialog-title",
            _customDialogTitle,
            loggerFactory
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        IsCustomDialogImageContent = new HistoricalBoolProperty(
            "is-custom-dialog-image-content",
            _customDialogIsImageContent
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        CustomDialogMessage = new HistoricalStringProperty(
            "custom-dialog-message",
            _customDialogMessage,
            loggerFactory
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        CustomDialogImagePath = new HistoricalStringProperty(
            "custom-dialog-image-path",
            _customDialogImagePath,
            loggerFactory
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        CustomDialogPrimaryButtonText = new HistoricalStringProperty(
            "custom-dialog-primary-button-text",
            _customDialogPrimaryButtonText,
            loggerFactory
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        IsCustomDialogPrimaryButtonEnabled = new HistoricalBoolProperty(
            "is-custom-dialog-primary-button-enabled",
            _customDialogIsPrimaryButtonEnabled
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        CustomDialogSecondaryButtonText = new HistoricalStringProperty(
            "custom-dialog-secondary-button-text",
            _customDialogSecondaryButtonText,
            loggerFactory
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        IsCustomDialogSecondaryButtonEnabled = new HistoricalBoolProperty(
            "is-custom-dialog-secondary-button-enabled",
            _customDialogIsSecondaryButtonEnabled
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

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
            "custom-dialog-result",
            _customDialogResult
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        YesOrNoDialogResult = new HistoricalEnumProperty<ConfirmationStatus>(
            "yes-or-no-dialog-result",
            _yesOrNoDialogResult
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        SaveCancelDialogResult = new HistoricalEnumProperty<ConfirmationStatus>(
            "save-cancel-dialog-result",
            _saveCancelDialogResult
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        ShowInputDialogResult = new HistoricalStringProperty(
            "input-dialog-result",
            _showInputDialogResult,
            loggerFactory
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        ShowHotKeyCaptureDialogResult = new HistoricalStringProperty(
            "hotkey-capture-dialog-result",
            _showHotKeyCaptureDialogResult,
            loggerFactory
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        GeoPointDialogResult = new HistoricalGeoPointProperty(
            "geo-point-dialog-result",
            _geoPointDialogResult,
            unitService,
            loggerFactory
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        OpenFileDialogResult = new HistoricalStringProperty(
            "open-file-dialog-result",
            _openFileDialogResult,
            loggerFactory
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        SaveFileDialogResult = new HistoricalStringProperty(
            "save-file-dialog-result",
            _saveFileDialogResult,
            loggerFactory
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        SelectFolderDialogResult = new HistoricalStringProperty(
            "select-folder-dialog-result",
            _selectFolderDialogResult,
            loggerFactory
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        ObserveFolderDialogResult = new HistoricalStringProperty(
            "observe-folder-dialog-result",
            _observeFolderDialogResult,
            loggerFactory
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

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
        ResetYesOrNoCommand = new ReactiveCommand(_ =>
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
        ResetOpenGeoPointResultCommand = new ReactiveCommand(_ =>
        {
            GeoPointDialogResult.ModelValue.Value = GeoPoint.Zero;
        }).DisposeItWith(Disposable);

        #endregion

        GeoPointDialogResult.ForceValidate();

        TrackLayout(_customDialogTitle);
        TrackLayout(_customDialogIsImageContent);
        TrackLayout(_customDialogMessage);
        TrackLayout(_customDialogImagePath);
        TrackLayout(_customDialogPrimaryButtonText);
        TrackLayout(_customDialogIsPrimaryButtonEnabled);
        TrackLayout(_customDialogSecondaryButtonText);
        TrackLayout(_customDialogIsSecondaryButtonEnabled);
        TrackLayout(_customDialogResult);
        TrackLayout(_yesOrNoDialogResult);
        TrackLayout(_saveCancelDialogResult);
        TrackLayout(_showInputDialogResult);
        TrackLayout(_showHotKeyCaptureDialogResult);
        TrackLayout(_geoPointDialogResult);
        TrackLayout(_openFileDialogResult);
        TrackLayout(_saveFileDialogResult);
        TrackLayout(_selectFolderDialogResult);
        TrackLayout(_observeFolderDialogResult);
        Layout
            .Register(nameof(DialogControlsPageViewModel), LoadLayout, SaveLayout, _layoutChanged)
            .DisposeItWith(Disposable);
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
    public ReactiveCommand ResetYesOrNoCommand { get; }
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

            var dialogContent = new ContentDialog(vm)
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
            _logger.LogInformation("(CustomDialog) {msg}", msg);
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
            _logger.LogInformation("({dialogName}) {msg}", dialogName, msg);
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
            _logger.LogInformation("({dialogName}) {msg}", dialogName, msg);
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
            _logger.LogInformation("({dialogName}) {msg}", dialogName, msg);
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
            _logger.LogInformation("({dialogName}) {msg}", dialogName, msg);
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
        _logger.LogInformation("({dialogName}) {msg}", dialogName, msg);
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
        _logger.LogInformation("({dialogName}) {msg}", dialogName, msg);
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
        _logger.LogInformation("({dialogName}) {msg}", dialogName, msg);
    }

    private async ValueTask ShowHotKeyCaptureAsync(Unit unit, CancellationToken cancellationToken)
    {
        var payload = new HotKeyCaptureDialogPayload
        {
            Title = RS.DialogControlsPageViewModel_HotKeyCapturePrefab_Title,
            Message = RS.DialogControlsPageViewModel_HotKeyCapturePrefab_Message,
        };

        var result = await _hotKeyCaptureDialog.ShowDialogAsync(payload);

        ShowHotKeyCaptureDialogResult.ViewValue.Value = result?.ToString();

        var msg = string.Format(
            RS.DialogControlsPageViewModel_HotKeyCapturePrefab_Result,
            result?.ToString() ?? RS.DialogControlsPageViewModel_CancelResult
        );
        const string dialogName = nameof(HotKeyCaptureDialogPrefab);
        _logger.LogInformation("({dialogName}) {msg}", dialogName, msg);
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
        _logger.LogInformation("({dialogName}) {msg}", dialogName, msg);
    }

    private void TrackLayout<T>(Observable<T> observable)
    {
        observable
            .Skip(1)
            .Subscribe(_ => _layoutChanged.OnNext(Unit.Default))
            .DisposeItWith(Disposable);
    }

    private DialogControlsPageViewModelConfig SaveLayout()
    {
        return new DialogControlsPageViewModelConfig
        {
            CustomDialogTitle = CustomDialogTitle.ViewValue.Value ?? string.Empty,
            CustomDialogIsImageContent = IsCustomDialogImageContent.ViewValue.Value,
            CustomDialogMessage = CustomDialogMessage.ViewValue.Value ?? string.Empty,
            CustomDialogImagePath = CustomDialogImagePath.ViewValue.Value,
            CustomDialogPrimaryButtonText =
                CustomDialogPrimaryButtonText.ViewValue.Value ?? string.Empty,
            CustomDialogIsPrimaryButtonEnabled = IsCustomDialogPrimaryButtonEnabled.ViewValue.Value,
            CustomDialogSecondaryButtonText =
                CustomDialogSecondaryButtonText.ViewValue.Value ?? string.Empty,
            CustomDialogIsSecondaryButtonEnabled = IsCustomDialogSecondaryButtonEnabled
                .ViewValue
                .Value,
            CustomDialogResult = CustomDialogResult.ViewValue.Value,
            YesOrNoDialogResult = YesOrNoDialogResult.ViewValue.Value,
            SaveCancelDialogResult = SaveCancelDialogResult.ViewValue.Value,
            ShowInputDialogResult = ShowInputDialogResult.ViewValue.Value,
            ShowHotKeyCaptureDialogResult = ShowHotKeyCaptureDialogResult.ViewValue.Value,
            GeoPointDialogResult = GeoPointDialogResult.ModelValue.Value,
            OpenFileDialogResult = OpenFileDialogResult.ViewValue.Value,
            SaveFileDialogResult = SaveFileDialogResult.ViewValue.Value,
            SelectFolderDialogResult = SelectFolderDialogResult.ViewValue.Value,
            ObserveFolderDialogResult = ObserveFolderDialogResult.ViewValue.Value,
        };
    }

    private void LoadLayout(DialogControlsPageViewModelConfig config)
    {
        CustomDialogTitle.ModelValue.Value = config.CustomDialogTitle;
        IsCustomDialogImageContent.ModelValue.Value = config.CustomDialogIsImageContent;
        CustomDialogMessage.ModelValue.Value = config.CustomDialogMessage;
        CustomDialogImagePath.ModelValue.Value = config.CustomDialogImagePath;
        CustomDialogPrimaryButtonText.ModelValue.Value = config.CustomDialogPrimaryButtonText;
        IsCustomDialogPrimaryButtonEnabled.ModelValue.Value =
            config.CustomDialogIsPrimaryButtonEnabled;
        CustomDialogSecondaryButtonText.ModelValue.Value = config.CustomDialogSecondaryButtonText;
        IsCustomDialogSecondaryButtonEnabled.ModelValue.Value =
            config.CustomDialogIsSecondaryButtonEnabled;
        CustomDialogResult.ModelValue.Value = config.CustomDialogResult;
        YesOrNoDialogResult.ModelValue.Value = config.YesOrNoDialogResult;
        SaveCancelDialogResult.ModelValue.Value = config.SaveCancelDialogResult;
        ShowInputDialogResult.ModelValue.Value = config.ShowInputDialogResult;
        ShowHotKeyCaptureDialogResult.ModelValue.Value = config.ShowHotKeyCaptureDialogResult;
        GeoPointDialogResult.ModelValue.Value = config.GeoPointDialogResult;
        OpenFileDialogResult.ModelValue.Value = config.OpenFileDialogResult;
        SaveFileDialogResult.ModelValue.Value = config.SaveFileDialogResult;
        SelectFolderDialogResult.ModelValue.Value = config.SelectFolderDialogResult;
        ObserveFolderDialogResult.ModelValue.Value = config.ObserveFolderDialogResult;
    }

    public override IEnumerable<IViewModel> GetChildren()
    {
        foreach (var child in base.GetChildren())
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
}
