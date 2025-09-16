using System;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
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

    private readonly OpenFileDialogDesktopPrefab _openFileDialog;
    private readonly SaveFileDialogDesktopPrefab _saveFileDialog;
    private readonly SelectFolderDialogDesktopPrefab _selectFolderDialog;
    private readonly ObserveFolderDialogPrefab _observeFolderDialog;

    private readonly YesOrNoDialogPrefab _yesOrNoDialog;
    private readonly SaveCancelDialogPrefab _saveCancelDialog;
    private readonly InputDialogPrefab _inputDialog;
    private readonly HotKeyCaptureDialogPrefab _hotKeyCaptureDialog;

    public DialogControlsPageViewModel()
        : this(NullLoggerFactory.Instance, NullDialogService.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public DialogControlsPageViewModel(ILoggerFactory loggerFactory, IDialogService dialogService)
        : base(PageId, loggerFactory)
    {
        _openFileDialog = dialogService.GetDialogPrefab<OpenFileDialogDesktopPrefab>();
        _saveFileDialog = dialogService.GetDialogPrefab<SaveFileDialogDesktopPrefab>();
        _selectFolderDialog = dialogService.GetDialogPrefab<SelectFolderDialogDesktopPrefab>();
        _observeFolderDialog = dialogService.GetDialogPrefab<ObserveFolderDialogPrefab>();

        _yesOrNoDialog = dialogService.GetDialogPrefab<YesOrNoDialogPrefab>();
        _saveCancelDialog = dialogService.GetDialogPrefab<SaveCancelDialogPrefab>();
        _inputDialog = dialogService.GetDialogPrefab<InputDialogPrefab>();
        _hotKeyCaptureDialog = dialogService.GetDialogPrefab<HotKeyCaptureDialogPrefab>();

        LastResult = new BindableReactiveProperty<string>().DisposeItWith(Disposable);

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
    }

    public ReactiveCommand OpenFileCommand { get; }
    public ReactiveCommand SaveFileCommand { get; }
    public ReactiveCommand SelectFolderCommand { get; }
    public ReactiveCommand ObserveFolderCommand { get; }

    public ReactiveCommand YesOrNoCommand { get; }
    public ReactiveCommand SaveCancelCommand { get; }
    public ReactiveCommand ShowInputCommand { get; }
    public ReactiveCommand ShowHotKeyCaptureCommand { get; }

    public BindableReactiveProperty<string> LastResult { get; }

    public override IExportInfo Source => SystemModule.Instance;

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
            var msg = string.Format(RS.DialogControlsPageViewModel_OpenFile_Result, result);
            LastResult.OnNext(msg);
            Logger.LogInformation("{msg}", msg); // TODO: Give more info. Example: OpenFileDialog result: {msg}
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
            var msg = string.Format(RS.DialogControlsPageViewModel_SaveFile_Result, result);
            LastResult.OnNext(msg);
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
            var msg = string.Format(RS.DialogControlsPageViewModel_SelectFolder_Result, result);
            LastResult.OnNext(msg);
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
            var msg = string.Format(
                RS.DialogControlsPageViewModel_ObserveFolder_Result,
                payload.DefaultPath
            );
            LastResult.OnNext(msg);
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
        var msg = string.Format(RS.DialogControlsPageViewModel_YesOrNo_Result, result);
        LastResult.OnNext(msg);
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
        var msg = string.Format(RS.DialogControlsPageViewModel_Save_Result, result);
        LastResult.OnNext(msg);
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
        var msg = string.Format(RS.DialogControlsPageViewModel_Input_Result, result);
        LastResult.OnNext(msg);
        Logger.LogInformation("{msg}", msg);
    }

    private async ValueTask ShowHotKeyCaptureAsync(Unit unit, CancellationToken cancellationToken)
    {
        var payload = new HotKeyCaptureDialogPayload
        {
            Title = RS.DialogControlsPageViewModel_HotKeyCapture_Title,
            Message = RS.DialogControlsPageViewModel_HotKeyCapture_Message,
        };

        var rawResult = await _hotKeyCaptureDialog.ShowDialogAsync(payload);
        var result = rawResult?.ToString() ?? $"({RS.DialogControlsPageViewModel_CancelResult})";
        var msg = string.Format(RS.DialogControlsPageViewModel_HotKeyCapture_Result, result);
        LastResult.OnNext(msg);
        Logger.LogInformation("{msg}", msg);
    }
}
