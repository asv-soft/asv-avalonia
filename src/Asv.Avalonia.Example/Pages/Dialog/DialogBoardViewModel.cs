using System.Collections.Generic;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
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

    private readonly YesOrNoDialogPrefab _yesNoDialog;
    private readonly SaveCancelDialogPrefab _saveCancelDialog;
    private readonly InputDialogPrefab _inputDialog;

    private readonly ILogger<DialogBoardViewModel> _logger;

    public DialogBoardViewModel()
        : this(DesignTime.CommandService, NullLoggerFactory.Instance, null!)
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

        _yesNoDialog = dialogService.GetDialogPrefab<YesOrNoDialogPrefab>();
        _saveCancelDialog = dialogService.GetDialogPrefab<SaveCancelDialogPrefab>();
        _inputDialog = dialogService.GetDialogPrefab<InputDialogPrefab>();
        OpenFileMessage = new ReactiveCommand(OpenFileAsync);
        SaveFileMessage = new ReactiveCommand(SaveFileAsync);
        SelectFileMessage = new ReactiveCommand(SelectFileAsync);
        ObserveFolderMessage = new ReactiveCommand(ObserveFolderAsync);
        YesNoMessage = new ReactiveCommand(YesNoMessageAsync);
        SaveCancelMessage = new ReactiveCommand(SaveCancelAsync);
        ShowUnitInputMessage = new ReactiveCommand(ShowUnitInputAsync);
    }

    public ReactiveCommand OpenFileMessage { get; }
    public ReactiveCommand SaveFileMessage { get; }
    public ReactiveCommand SelectFileMessage { get; }
    public ReactiveCommand ObserveFolderMessage { get; }
    public ReactiveCommand YesNoMessage { get; }
    public ReactiveCommand SaveCancelMessage { get; }
    public ReactiveCommand ShowUnitInputMessage { get; }

    private ValueTask OpenFileAsync(Unit unit, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    private ValueTask SaveFileAsync(Unit unit, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    private ValueTask SelectFileAsync(Unit unit, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    private ValueTask ObserveFolderAsync(Unit unit, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
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
