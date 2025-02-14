using System.Windows.Input;
using Material.Icons;
using R3;

namespace Asv.Avalonia;

public abstract class PageViewModel<TContext> : ExtendableViewModel<TContext>, IPage
    where TContext : class, IPage
{
    protected PageViewModel(string id, ICommandService cmd)
        : base(id)
    {
        History = cmd.CreateHistory(this);
        Icon = new BindableReactiveProperty<MaterialIconKind>(MaterialIconKind.Window);
        Title = new BindableReactiveProperty<string>(id);
        HasChanges = new BindableReactiveProperty<bool>(false);
        TryClose = new BindableAsyncCommand(ClosePageCommand.Id, this);
    }

    public async ValueTask TryCloseAsync()
    {
        var reasons = await this.RequestChildCloseApproval();
        if (reasons.Count != 0)
        {
            // TODO: ask user to save changes
        }

        await this.RequestClose();
    }

    public BindableReactiveProperty<MaterialIconKind> Icon { get; }
    public BindableReactiveProperty<string> Title { get; }
    public ICommandHistory History { get; }
    public BindableReactiveProperty<bool> HasChanges { get; }
    public ICommand TryClose { get; }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Icon.Dispose();
            Title.Dispose();
            History.Dispose();
            HasChanges.Dispose();
        }

        base.Dispose(disposing);
    }

    public abstract IExportInfo Source { get; }
}
