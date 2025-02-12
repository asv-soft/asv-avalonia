using System.Collections.Generic;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using R3;

namespace Asv.Avalonia.Example.Pages.Dialog;

[ExportPage(PageId)]
public class DialogBoardViewModel : PageViewModel<DialogBoardViewModel>
{
    private readonly IContainerHost _container;
    public const string PageId = "dialog";

    public DialogBoardViewModel(IContainerHost container)
        : this(DesignTime.CommandService, container)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public DialogBoardViewModel(ICommandService cmd, IContainerHost container)
        : base(PageId, cmd)
    {
        _container = container;
        OpenFileMessage = new ReactiveCommand((_, c) => OpenFileAsync(c));
        SaveFileMessage = new ReactiveCommand((_, c) => SaveFileAsync(c));
        SelectFileMessage = new ReactiveCommand((_, c) => SelectFileAsync(c));
        ObserveFolderMessage = new ReactiveCommand((_, c) => ObserveFolderAsync(c));
        YesNoMessage = new ReactiveCommand((_, c) => YesNoMessageAsync(c));
        SaveCancelMessage = new ReactiveCommand((_, c) => SaveCancelAsync(c));
        ShowUnitInputMessage = new ReactiveCommand((_, c) => ShowUnitInputAsync(c));
    }

    #region Message

    public ReactiveCommand OpenFileMessage { get; }

    protected virtual ValueTask OpenFileAsync(CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    public ReactiveCommand SaveFileMessage { get; }

    protected virtual ValueTask SaveFileAsync(CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    public ReactiveCommand SelectFileMessage { get; }

    protected virtual ValueTask SelectFileAsync(CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    public ReactiveCommand ObserveFolderMessage { get; }

    protected virtual ValueTask ObserveFolderAsync(CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    public ReactiveCommand YesNoMessage { get; }

    protected virtual ValueTask YesNoMessageAsync(CancellationToken cancellationToken)
    {
        var mainWindow = (
            (IClassicDesktopStyleApplicationLifetime)Application.Current?.ApplicationLifetime!
        )?.MainWindow;
        var fileDialogServiceImplementation = _container.GetExport<IDialogService>();
        fileDialogServiceImplementation.ShowYesNoDialog(
            "Предупреждение",
            "Вы действительно хотите выйти?"
        );
        return ValueTask.CompletedTask;
    }

    public ReactiveCommand SaveCancelMessage { get; }

    protected virtual ValueTask SaveCancelAsync(CancellationToken cancellationToken)
    {
        var mainWindow = (
            (IClassicDesktopStyleApplicationLifetime)Application.Current?.ApplicationLifetime!
        )?.MainWindow;
        var fileDialogServiceImplementation = _container.GetExport<IDialogService>();
        fileDialogServiceImplementation.ShowSaveCancelDialog("Сохранение", "Сохранить?");
        return ValueTask.CompletedTask;
    }

    public ReactiveCommand ShowUnitInputMessage { get; }

    protected virtual ValueTask ShowUnitInputAsync(CancellationToken cancellationToken)
    {
        var mainWindow = (
            (IClassicDesktopStyleApplicationLifetime)Application.Current?.ApplicationLifetime!
        )?.MainWindow;
        var fileDialogServiceImplementation = _container.GetExport<IDialogService>();
        fileDialogServiceImplementation.ShowUnitInputDialog("Поиск", "Введите значение файла");
        return ValueTask.CompletedTask;
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
}
