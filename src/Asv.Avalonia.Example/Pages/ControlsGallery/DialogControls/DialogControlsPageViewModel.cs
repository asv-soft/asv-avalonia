using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia.GeoMap;
using Asv.Common;
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

    private readonly ReactiveProperty<string?> _customDialogMessage;
    private readonly ReactiveProperty<string?> _customDialogPrimaryButtonText;
    private readonly ReactiveProperty<string?> _customDialogSecondaryButtonText;
    private readonly ReactiveProperty<string?> _customDialogTitle;

    private readonly ReactiveProperty<GeoPoint> _geoPointProperty;

    private readonly ILoggerFactory _loggerFactory;
    private readonly INavigationService _navigationService;

    private readonly HotKeyCaptureDialogPrefab _hotKeyCaptureDialog;
    private readonly InputDialogPrefab _inputDialog;
    private readonly ObserveFolderDialogPrefab _observeFolderDialog;
    private readonly OpenFileDialogDesktopPrefab _openFileDialog;
    private readonly SaveCancelDialogPrefab _saveCancelDialog;
    private readonly SaveFileDialogDesktopPrefab _saveFileDialog;
    private readonly SelectFolderDialogDesktopPrefab _selectFolderDialog;
    private readonly YesOrNoDialogPrefab _yesOrNoDialog;
    private readonly GeoPointDialogPrefab _geoPointDialog;

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

        _openFileDialog = dialogService.GetDialogPrefab<OpenFileDialogDesktopPrefab>();
        _saveFileDialog = dialogService.GetDialogPrefab<SaveFileDialogDesktopPrefab>();
        _selectFolderDialog = dialogService.GetDialogPrefab<SelectFolderDialogDesktopPrefab>();
        _observeFolderDialog = dialogService.GetDialogPrefab<ObserveFolderDialogPrefab>();

        _yesOrNoDialog = dialogService.GetDialogPrefab<YesOrNoDialogPrefab>();
        _saveCancelDialog = dialogService.GetDialogPrefab<SaveCancelDialogPrefab>();
        _inputDialog = dialogService.GetDialogPrefab<InputDialogPrefab>();
        _hotKeyCaptureDialog = dialogService.GetDialogPrefab<HotKeyCaptureDialogPrefab>();
        _geoPointDialog = dialogService.GetDialogPrefab<GeoPointDialogPrefab>();

        _customDialogMessage = new ReactiveProperty<string?>(
            RS.DialogControlsPageView_CustomDialog_Content
        ).DisposeItWith(Disposable);
        _customDialogTitle = new ReactiveProperty<string?>(
            RS.DialogControlsPageView_CustomDialog_Title
        ).DisposeItWith(Disposable);
        _customDialogPrimaryButtonText = new ReactiveProperty<string?>(
            RS.DialogControlsPageView_CustomDialog_Content
        ).DisposeItWith(Disposable);
        _customDialogSecondaryButtonText = new ReactiveProperty<string?>(
            RS.DialogControlsPageView_CustomDialog_Content
        ).DisposeItWith(Disposable);

        var latUnit = unitService.Units[LatitudeBase.Id];
        var lonUnit = unitService.Units[LongitudeBase.Id];
        var altUnit = unitService.Units[AltitudeBase.Id];

        _geoPointProperty = new ReactiveProperty<GeoPoint>(GeoPoint.Zero).DisposeItWith(Disposable);
        GeoPointDialogResult = new HistoricalGeoPointProperty(
            nameof(GeoPointDialogResult),
            _geoPointProperty,
            latUnit,
            lonUnit,
            altUnit,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        GeoPointDialogResult.ForceValidate();

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

        CustomDialogMessage = new HistoricalStringProperty(
            nameof(CustomDialogMessage),
            _customDialogMessage,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        CustomDialogTitle = new HistoricalStringProperty(
            nameof(CustomDialogTitle),
            _customDialogTitle,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        CustomDialogPrimaryButtonText = new HistoricalStringProperty(
            nameof(CustomDialogPrimaryButtonText),
            _customDialogPrimaryButtonText,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        CustomDialogSecondaryButtonText = new HistoricalStringProperty(
            nameof(CustomDialogSecondaryButtonText),
            _customDialogSecondaryButtonText,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);

        CustomDialogResult = new BindableReactiveProperty<ContentDialogResult?>().DisposeItWith(
            Disposable
        );
        OpenFileDialogResult = new BindableReactiveProperty<string>().DisposeItWith(Disposable);
        SaveFileDialogResult = new BindableReactiveProperty<string>().DisposeItWith(Disposable);
        SelectFolderDialogResult = new BindableReactiveProperty<string>().DisposeItWith(Disposable);
        ObserveFolderDialogResult = new BindableReactiveProperty<string>().DisposeItWith(
            Disposable
        );
        YesOrNoDialogResult = new BindableReactiveProperty<bool?>().DisposeItWith(Disposable);
        SaveCancelDialogResult = new BindableReactiveProperty<bool?>().DisposeItWith(Disposable);
        ShowInputDialogResult = new BindableReactiveProperty<string>().DisposeItWith(Disposable);
        ShowHotKeyCaptureDialogResult = new BindableReactiveProperty<HotKeyInfo?>().DisposeItWith(
            Disposable
        );

        ShowCustomDialogCommand = new ReactiveCommand(OpenCustomAsync).DisposeItWith(Disposable);
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
    }

    public ReactiveCommand ShowCustomDialogCommand { get; }
    public ReactiveCommand OpenFileCommand { get; }
    public ReactiveCommand SaveFileCommand { get; }
    public ReactiveCommand SelectFolderCommand { get; }
    public ReactiveCommand ObserveFolderCommand { get; }
    public ReactiveCommand YesOrNoCommand { get; }
    public ReactiveCommand SaveCancelCommand { get; }
    public ReactiveCommand ShowInputCommand { get; }
    public ReactiveCommand ShowHotKeyCaptureCommand { get; }
    public ReactiveCommand OpenGeoPointDialogCommand { get; }

    public BindableReactiveProperty<string> OpenFileDialogResult { get; }
    public BindableReactiveProperty<string> SaveFileDialogResult { get; }
    public BindableReactiveProperty<string> SelectFolderDialogResult { get; }
    public BindableReactiveProperty<string> ObserveFolderDialogResult { get; }
    public BindableReactiveProperty<bool?> YesOrNoDialogResult { get; }
    public BindableReactiveProperty<bool?> SaveCancelDialogResult { get; }
    public BindableReactiveProperty<string> ShowInputDialogResult { get; }
    public BindableReactiveProperty<HotKeyInfo?> ShowHotKeyCaptureDialogResult { get; }
    public HistoricalGeoPointProperty GeoPointDialogResult { get; }
    public BindableReactiveProperty<ContentDialogResult?> CustomDialogResult { get; }

    public HistoricalStringProperty CustomDialogMessage { get; }
    public HistoricalStringProperty CustomDialogTitle { get; }
    public HistoricalStringProperty CustomDialogPrimaryButtonText { get; }
    public HistoricalStringProperty CustomDialogSecondaryButtonText { get; }

    public BindableReactiveProperty<string> LonUnitName { get; }
    public BindableReactiveProperty<string> LatUnitName { get; }
    public BindableReactiveProperty<string> AltUnitName { get; }

    public override IExportInfo Source => SystemModule.Instance;

    private async ValueTask OpenCustomAsync(Unit unit, CancellationToken cancellationToken)
    {
        using var vm = new DialogItemTextViewModel(_loggerFactory)
        {
            Message = CustomDialogMessage.ViewValue.Value ?? string.Empty,
        };

        var dialogContent = new ContentDialog(vm, _navigationService)
        {
            Title = CustomDialogTitle.ViewValue.Value ?? string.Empty,
            PrimaryButtonText = CustomDialogPrimaryButtonText.ViewValue.Value ?? string.Empty,
            SecondaryButtonText = CustomDialogSecondaryButtonText.ViewValue.Value ?? string.Empty,
        };

        var result = await dialogContent.ShowAsync();

        CustomDialogResult.Value = result;

        var resultToLog = result switch
        {
            ContentDialogResult.None => RS.DialogControlsPageViewModel_CancelResult,
            ContentDialogResult.Primary => RS.DialogControlsPageViewModel_Custom_Primary,
            ContentDialogResult.Secondary => RS.DialogControlsPageViewModel_Custom_Secondary,
            _ => string.Empty,
        };
        var msg = string.Format(RS.DialogControlsPageViewModel_Custom_Result, resultToLog);
        Logger.LogInformation("{msg}", msg);
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

            OpenFileDialogResult.OnNext(result);

            var msg = string.Format(RS.DialogControlsPageViewModel_OpenFile_Result, result);
            Logger.LogInformation("{msg}", msg);
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

            SaveFileDialogResult.OnNext(result);

            var msg = string.Format(RS.DialogControlsPageViewModel_SaveFile_Result, result);
            Logger.LogInformation("{msg}", msg);
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

            SelectFolderDialogResult.OnNext(result);

            var msg = string.Format(RS.DialogControlsPageViewModel_SelectFolder_Result, result);
            Logger.LogInformation("{msg}", msg);
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

            ObserveFolderDialogResult.OnNext(
                RS.DialogControlsPageViewModel_ObserveFolder_ResultShort
            );

            var msg = string.Format(
                RS.DialogControlsPageViewModel_ObserveFolder_Result,
                payload.DefaultPath
            );
            Logger.LogInformation("{msg}", msg);
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

        YesOrNoDialogResult.OnNext(result);

        var msg = string.Format(RS.DialogControlsPageViewModel_YesOrNo_Result, result);
        Logger.LogInformation("{msg}", msg);
    }

    private async ValueTask SaveCancelAsync(Unit unit, CancellationToken cancellationToken)
    {
        var payload = new SaveCancelDialogPayload
        {
            Title = RS.DialogControlsPageViewModel_Save_Title,
            Message = RS.DialogControlsPageViewModel_Save_Message,
        };

        var result = await _saveCancelDialog.ShowDialogAsync(payload);

        SaveCancelDialogResult.OnNext(result);

        var msg = string.Format(RS.DialogControlsPageViewModel_Save_Result, result);
        Logger.LogInformation("{msg}", msg);
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

        ShowInputDialogResult.OnNext(result);

        var msg = string.Format(RS.DialogControlsPageViewModel_Input_Result, result);
        Logger.LogInformation("{msg}", msg);
    }

    private async ValueTask ShowHotKeyCaptureAsync(Unit unit, CancellationToken cancellationToken)
    {
        var payload = new HotKeyCaptureDialogPayload
        {
            Title = RS.DialogControlsPageViewModel_HotKeyCapture_Title,
            Message = RS.DialogControlsPageViewModel_HotKeyCapture_Message,
        };

        var result = await _hotKeyCaptureDialog.ShowDialogAsync(payload);

        ShowHotKeyCaptureDialogResult.OnNext(result);

        var msg = string.Format(
            RS.DialogControlsPageViewModel_HotKeyCapture_Result,
            result ?? RS.DialogControlsPageViewModel_CancelResult
        );
        Logger.LogInformation("{msg}", msg);
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
        Logger.LogInformation("{msg}", msg);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        yield return GeoPointDialogResult;
    }
}
