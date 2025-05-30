using System.Windows.Input;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;
using ZLogger;

namespace Asv.Avalonia;

public abstract class PageViewModel<TContext> : ExtendableViewModel<TContext>, IPage
    where TContext : class, IPage
{
    private string _title;
    private MaterialIconKind _icon;

    protected PageViewModel(NavigationId id, ICommandService cmd)
        : base(id)
    {
        History = cmd.CreateHistory(this);
        Icon = MaterialIconKind.Window;
        Title = id.ToString();
        HasChanges = new BindableReactiveProperty<bool>(false);
        TryClose = new BindableAsyncCommand(ClosePageCommand.Id, this);
    }

    public async ValueTask TryCloseAsync()
    {
        try
        {
            var reasons = await this.RequestChildCloseApproval();
            if (reasons.Count != 0)
            {
                // TODO: send a callback with restrictions. If you have any restriction just call its callback before you close the page.
                // TODO: ask user to save changes
            }

            await this.RequestClose();
        }
        catch (Exception e)
        {
            LoggerFactory
                ?.CreateLogger<PageViewModel<TContext>>()
                .ZLogError(e, $"Error on close page {Title}[{Id}]: {e.Message}");
        }
    }

    public MaterialIconKind Icon
    {
        get => _icon;
        set => SetField(ref _icon, value);
    }

    public string Title
    {
        get => _title;
        set => SetField(ref _title, value);
    }

    public ICommandHistory History { get; }
    public BindableReactiveProperty<bool> HasChanges { get; }
    public ICommand TryClose { get; }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            History.Dispose();
            HasChanges.Dispose();
        }

        base.Dispose(disposing);
    }

    public abstract IExportInfo Source { get; }
}
