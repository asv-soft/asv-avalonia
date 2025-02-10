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
        TryClose = new ReactiveCommand(TryCloseAsync);
    }
    
    public ValueTask TryCloseAsync(Unit arg1, CancellationToken cancel)
    {
        return ValueTask.CompletedTask;
    }

    public BindableReactiveProperty<MaterialIconKind> Icon { get; }
    public BindableReactiveProperty<string> Title { get; }
    public ICommandHistory History { get; }
    public BindableReactiveProperty<bool> HasChanges { get; }
    public ReactiveCommand TryClose { get; }

    protected override ValueTask InternalCatchEvent(AsyncRoutedEvent e)
    {
        if (e is ExecuteCommandEvent cmd)
        {
            return History.Execute(cmd.CommandId, cmd.Source, cmd.CommandParameter);
        }

        return ValueTask.CompletedTask;
    }

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
}
