using System;
using System.Collections.Generic;
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
public class DialogControlsPageViewModel
    : TreeSubpage<ControlsGalleryPageViewModel>,
        IControlsGallerySubPage
{
    public const string PageId = "dialog_controls";
    public const MaterialIconKind PageIcon = MaterialIconKind.Dialogue;

    private readonly SelectFolderDialogDesktopPrefab _selectFolderDialog;
    private readonly ObserveFolderDialogPrefab _observeFolderDialog;
    private readonly SaveFileDialogDesktopPrefab _saveFileDialog;
    private readonly OpenFileDialogDesktopPrefab _openFileDialog;
    private readonly SaveCancelDialogPrefab _saveCancelDialog;
    private readonly YesOrNoDialogPrefab _yesNoDialog;
    private readonly InputDialogPrefab _inputDialog;
    private readonly HotKeyCaptureDialogPrefab _keyCaptureDialog;

    public DialogControlsPageViewModel()
        : this(NullLoggerFactory.Instance, NullDialogService.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public DialogControlsPageViewModel(ILoggerFactory loggerFactory, IDialogService dialogService)
        : base(PageId, loggerFactory)
    {
        _selectFolderDialog = dialogService.GetDialogPrefab<SelectFolderDialogDesktopPrefab>();
        _observeFolderDialog = dialogService.GetDialogPrefab<ObserveFolderDialogPrefab>();
        _saveFileDialog = dialogService.GetDialogPrefab<SaveFileDialogDesktopPrefab>();
        _openFileDialog = dialogService.GetDialogPrefab<OpenFileDialogDesktopPrefab>();
        _saveCancelDialog = dialogService.GetDialogPrefab<SaveCancelDialogPrefab>();
        _yesNoDialog = dialogService.GetDialogPrefab<YesOrNoDialogPrefab>();
        _inputDialog = dialogService.GetDialogPrefab<InputDialogPrefab>();
        _inputDialog = dialogService.GetDialogPrefab<InputDialogPrefab>();
        _keyCaptureDialog = dialogService.GetDialogPrefab<HotKeyCaptureDialogPrefab>();

        LastResult = new BindableReactiveProperty<string>().DisposeItWith(Disposable);

        OpenFileCommand = new ReactiveCommand(OpenFileAsync).DisposeItWith(Disposable);
        SaveFileCommand = new ReactiveCommand(SaveFileAsync).DisposeItWith(Disposable);
        SelectFolderCommand = new ReactiveCommand(SelectFolderAsync).DisposeItWith(Disposable);
        ObserveFolderCommand = new ReactiveCommand(ObserveFolderAsync).DisposeItWith(Disposable);
        YesNoCommand = new ReactiveCommand(YesNoMessageAsync).DisposeItWith(Disposable);
        SaveCancelCommand = new ReactiveCommand(SaveCancelAsync).DisposeItWith(Disposable);
        ShowUnitInputCommand = new ReactiveCommand(ShowUnitInputAsync).DisposeItWith(Disposable);
        ShowKeyCaptureCommand = new ReactiveCommand(ShowKeyCaptureAsync).DisposeItWith(Disposable);
    }

    public ReactiveCommand OpenFileCommand { get; }
    public ReactiveCommand SaveFileCommand { get; }
    public ReactiveCommand SelectFolderCommand { get; }
    public ReactiveCommand ObserveFolderCommand { get; }
    public ReactiveCommand YesNoCommand { get; }
    public ReactiveCommand SaveCancelCommand { get; }
    public ReactiveCommand ShowUnitInputCommand { get; }
    public ReactiveCommand ShowKeyCaptureCommand { get; }
    public BindableReactiveProperty<string> LastResult { get; }

    public override ValueTask Init(ControlsGalleryPageViewModel context) => ValueTask.CompletedTask;

    private async ValueTask OpenFileAsync(Unit unit, CancellationToken cancellationToken)
    {
        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            var payload = new OpenFileDialogPayload
            {
                Title = RS.DialogPageViewModel_OpenFile_Title,
            };

            var result = await _openFileDialog.ShowDialogAsync(payload);
            var msg = $"OpenFile: {result ?? "(cancel)"}";
            LastResult.OnNext(msg);
            Logger.LogInformation("{msg}", msg);
        }
    }

    private async ValueTask SaveFileAsync(Unit unit, CancellationToken cancellationToken)
    {
        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            var payload = new SaveFileDialogPayload
            {
                Title = RS.DialogPageViewModel_SaveFile_Title,
            };

            var result = await _saveFileDialog.ShowDialogAsync(payload);
            var msg = $"SaveFile: {result ?? "(cancel)"}";
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
                Title = RS.DialogPageViewModel_OpenFolder_Title,
            };

            var result = await _selectFolderDialog.ShowDialogAsync(payload);
            var msg = $"SelectFolder: {result ?? "(cancel)"}";
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
                Title = RS.DialogPageViewModel_ObserveFolder_Title,
                DefaultPath = Environment.CurrentDirectory,
            };

            await _observeFolderDialog.ShowDialogAsync(payload);
            var msg = $"ObserveFolder: opened {payload.DefaultPath}";
            LastResult.OnNext(msg);
            Logger.LogInformation("{msg}", msg);
        }
    }

    private async ValueTask YesNoMessageAsync(Unit unit, CancellationToken cancellationToken)
    {
        var payload = new YesOrNoDialogPayload
        {
            Title = RS.DialogPageViewModel_Confirmation_Title,
            Message = RS.DialogPageViewModel_Confirmation_Message,
        };

        var res = await _yesNoDialog.ShowDialogAsync(payload);
        var msg = $"YesNo: {res}";
        LastResult.OnNext(msg);
        Logger.LogInformation("{msg}", msg);
    }

    private async ValueTask SaveCancelAsync(Unit unit, CancellationToken cancellationToken)
    {
        var payload = new SaveCancelDialogPayload
        {
            Title = RS.DialogPageViewModel_Save_Title,
            Message = RS.DialogPageViewModel_Save_Message,
        };

        var res = await _saveCancelDialog.ShowDialogAsync(payload);
        var msg = $"SaveCancel: {res}";
        LastResult.OnNext(msg);
        Logger.LogInformation("{msg}", msg);
    }

    private async ValueTask ShowUnitInputAsync(Unit unit, CancellationToken cancellationToken)
    {
        var payload = new InputDialogPayload
        {
            Title = RS.DialogPageViewModel_Input_Title,
            Message = RS.DialogPageViewModel_Input_Message,
        };

        var res = await _inputDialog.ShowDialogAsync(payload);
        var msg = $"Input: {res ?? "(cancel)"}";
        LastResult.OnNext(msg);
        Logger.LogInformation("{msg}", msg);
    }

    private async ValueTask ShowKeyCaptureAsync(Unit unit, CancellationToken cancellationToken)
    {
        var payload = new HotKeyCaptureDialogPayload
        {
            Title = RS.DialogPageViewModel_KeyCapture_Title,
            Message = RS.DialogPageViewModel_KeyCapture_Message,
        };

        var res = await _keyCaptureDialog.ShowDialogAsync(payload);
        var msg = $"HotKey: {res?.ToString() ?? "(cancel)"}";
        LastResult.OnNext(msg);
        Logger.LogInformation("{msg}", msg);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    public override IExportInfo Source => SystemModule.Instance;
}
