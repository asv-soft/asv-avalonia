using System.Collections.Generic;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Example;

[ExportPage(PageId)]
public class DialogBoardViewModel : PageViewModel<DialogBoardViewModel>
{
    public const string PageId = "dialog";

    private readonly IDialogService _dialogService;
    private readonly ILogger<DialogBoardViewModel> _logger;

    public DialogBoardViewModel()
        : this(DesignTime.CommandService, null!, NullLogService.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
        Title.OnNext(RS.DialogPageVIewModel_Title);
    }

    [ImportingConstructor]
    public DialogBoardViewModel(
        ICommandService cmd,
        IDialogService service,
        ILoggerFactory logFactory
    )
        : base(PageId, cmd)
    {
        Title.OnNext(RS.DialogPageVIewModel_Title);
        _dialogService = service;
        _logger = logFactory.CreateLogger<DialogBoardViewModel>();

        OpenFileMessage = new ReactiveCommand(OpenFileAsync);
        SaveFileMessage = new ReactiveCommand(SaveFileAsync);
        SelectFileMessage = new ReactiveCommand(SelectFileAsync);
        ObserveFolderMessage = new ReactiveCommand(ObserveFolderAsync);
        YesNoMessage = new ReactiveCommand(YesNoMessageAsync);
        SaveCancelMessage = new ReactiveCommand(SaveCancelAsync);
        ShowUnitInputMessage = new ReactiveCommand(ShowUnitInputAsync);
    }

    #region Message

    public ReactiveCommand OpenFileMessage { get; }

    public ValueTask OpenFileAsync(Unit unit, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    public ReactiveCommand SaveFileMessage { get; }

    public ValueTask SaveFileAsync(Unit unit, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    public ReactiveCommand SelectFileMessage { get; }

    public ValueTask SelectFileAsync(Unit unit, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    public ReactiveCommand ObserveFolderMessage { get; }

    public ValueTask ObserveFolderAsync(Unit unit, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    public ReactiveCommand YesNoMessage { get; }

    public async ValueTask YesNoMessageAsync(Unit unit, CancellationToken cancellationToken)
    {
        var res = await _dialogService.ShowYesNoDialog(
            "Предупреждение",
            "Вы действительно хотите выйти?"
        );

        _logger.LogInformation($"YesNo result = {res}");
    }

    public ReactiveCommand SaveCancelMessage { get; }

    public async ValueTask SaveCancelAsync(Unit unit, CancellationToken cancellationToken)
    {
        var res = await _dialogService.ShowSaveCancelDialog("Сохранение", "Сохранить?");
        _logger.LogInformation($"SaveCancel result = {res}");
    }

    public ReactiveCommand ShowUnitInputMessage { get; }

    public async ValueTask ShowUnitInputAsync(Unit unit, CancellationToken cancellationToken)
    {
        var res = await _dialogService.ShowInputDialog("Поиск", "Введите значение файла");
        _logger.LogInformation($"UnitInput result = {res}");
    }

    #endregion

    public override ValueTask<IRoutable> Navigate(string id)
    {
        return ValueTask.FromResult<IRoutable>(this);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    protected override void AfterLoadExtensions() { }

    public override IExportInfo Source => SystemModule.Instance;
}
