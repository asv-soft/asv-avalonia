using Avalonia.Input;
using Material.Icons;

namespace Asv.Avalonia;

public abstract class HotKeyAction<TContext> : IHotKeyAction 
    where TContext : IViewModel
{
    public abstract string ActionId { get; }
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract MaterialIconKind Icon { get; }
    public abstract KeyGesture DefaultHotKey { get; }
    public ValueTask<bool> TryExecute(IViewModel context, CancellationToken cancel = default)
    {
        var target = context.FindParentOfType<TContext>();
        return target == null ? ValueTask.FromResult(false) : Execute(target, cancel);
    }

    protected abstract ValueTask<bool> Execute(TContext target, CancellationToken cancel);
}