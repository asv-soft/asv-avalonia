using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IO;
using R3;

namespace Asv.Avalonia.Example;

// TODO:Localize
[ExportPage(PageId)]
public class DialogBoardViewModel : PageViewModel<DialogBoardViewModel>
{
    public const string PageId = "dialog";
    public const MaterialIconKind PageIcon = MaterialIconKind.Dialogue;

    private readonly SelectFolderDialogDesktopPrefab _selectFolderDialog;
    private readonly ObserveFolderDialogPrefab _observeFolderDialog;
    private readonly SaveFileDialogDesktopPrefab _saveFileDialog;
    private readonly OpenFileDialogDesktopPrefab _openFileDialog;
    private readonly SaveCancelDialogPrefab _saveCancelDialog;
    private readonly YesOrNoDialogPrefab _yesNoDialog;
    private readonly InputDialogPrefab _inputDialog;

    private readonly ILogger<DialogBoardViewModel> _logger;

    public DialogBoardViewModel()
        : this(DesignTime.CommandService, NullLoggerFactory.Instance, NullDialogService.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
        Title.OnNext(RS.DialogPageViewModel_Title);
    }

    [ImportingConstructor]
    public DialogBoardViewModel(
        ICommandService cmd,
        ILoggerFactory logFactory,
        IDialogService dialogService
    )
        : base(PageId, cmd)
    {
        Title.OnNext(RS.DialogPageViewModel_Title);
        _logger = logFactory.CreateLogger<DialogBoardViewModel>();

        _selectFolderDialog = dialogService.GetDialogPrefab<SelectFolderDialogDesktopPrefab>();
        _observeFolderDialog = dialogService.GetDialogPrefab<ObserveFolderDialogPrefab>();
        _saveFileDialog = dialogService.GetDialogPrefab<SaveFileDialogDesktopPrefab>();
        _openFileDialog = dialogService.GetDialogPrefab<OpenFileDialogDesktopPrefab>();
        _saveCancelDialog = dialogService.GetDialogPrefab<SaveCancelDialogPrefab>();
        _yesNoDialog = dialogService.GetDialogPrefab<YesOrNoDialogPrefab>();
        _inputDialog = dialogService.GetDialogPrefab<InputDialogPrefab>();

        OpenFileCommand = new ReactiveCommand(OpenFileAsync);
        SaveFileCommand = new ReactiveCommand(SaveFileAsync);
        SelectFolderCommand = new ReactiveCommand(SelectFolderAsync);
        ObserveFolderCommand = new ReactiveCommand(ObserveFolderAsync);
        YesNoCommand = new ReactiveCommand(YesNoMessageAsync);
        SaveCancelCommand = new ReactiveCommand(SaveCancelAsync);
        ShowUnitInputCommand = new ReactiveCommand(ShowUnitInputAsync);
    }

    public ReactiveCommand OpenFileCommand { get; }
    public ReactiveCommand SaveFileCommand { get; }
    public ReactiveCommand SelectFolderCommand { get; }
    public ReactiveCommand ObserveFolderCommand { get; }
    public ReactiveCommand YesNoCommand { get; }
    public ReactiveCommand SaveCancelCommand { get; }
    public ReactiveCommand ShowUnitInputCommand { get; }

    private async ValueTask OpenFileAsync(Unit unit, CancellationToken cancellationToken)
    {
        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            var payload = new OpenFileDialogPayload { Title = "Open File" };

            var result = await _openFileDialog.ShowDialogAsync(payload);
            _logger.LogInformation($"OpenFile result = {result}");
        }
    }

    private async ValueTask SaveFileAsync(Unit unit, CancellationToken cancellationToken)
    {
        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            var payload = new SaveFileDialogPayload { Title = "Save File" };

            var result = await _saveFileDialog.ShowDialogAsync(payload);
            _logger.LogInformation($"SaveFile result = {result}");
        }
    }

    private async ValueTask SelectFolderAsync(Unit unit, CancellationToken cancellationToken)
    {
        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            var payload = new SelectFolderDialogPayload { Title = "Select Folder File" };

            var result = await _selectFolderDialog.ShowDialogAsync(payload);
            _logger.LogInformation($"SelectFolder result = {result}");
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
            _logger.LogInformation($"ObserveFolder result = {result}");
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

        _logger.LogInformation($"YesNo result = {res}");
    }

    private async ValueTask SaveCancelAsync(Unit unit, CancellationToken cancellationToken)
    {
        var payload = new SaveCancelDialogPayload { Title = "Сохранение", Message = "Сохранить?" };

        var res = await _saveCancelDialog.ShowDialogAsync(payload);
        _logger.LogInformation($"SaveCancel result = {res}");
    }

    private async ValueTask ShowUnitInputAsync(Unit unit, CancellationToken cancellationToken)
    {
        var payload = new InputDialogPayload
        {
            Title = "Поиск",
            Message = "Введите значение файла",
        };
        var res = await _inputDialog.ShowDialogAsync(payload);
        _logger.LogInformation($"UnitInput result = {res}");
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    protected override void AfterLoadExtensions() { }

    public override IExportInfo Source => SystemModule.Instance;
}
