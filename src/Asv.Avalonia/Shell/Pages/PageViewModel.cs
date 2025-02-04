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
    }

    public BindableReactiveProperty<MaterialIconKind> Icon { get; }
    public BindableReactiveProperty<string> Title { get; }
    public ICommandHistory History { get; }

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
        }

        base.Dispose(disposing);
    }
}
