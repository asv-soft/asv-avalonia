using System.Windows.Input;
using Avalonia.Input;
using Material.Icons;
using R3;

namespace Asv.Avalonia;

public interface IHotKeyService
{
    void SetHotKey(string commandId, HotKeyInfo? hotKey);
    HotKeyInfo? GetHotKey(string commandId);
    IEnumerable<IHotKeyInfo> GetAllActions();
}

public interface IHotKeyInfo
{
    string Id { get; }
    string Name { get; }
    string Description { get; }
    MaterialIconKind Icon { get; }
    KeyGesture DefaultHotKey { get; }
}

public interface IHotKeyAction : IHotKeyInfo
{
    ValueTask<bool> TryExecute(IViewModel context, CancellationToken cancel = default);
}

public abstract class HotKeyAction<TContext> : IHotKeyAction 
    where TContext : IViewModel
{
    public abstract string Id { get; }
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

public class NullActionsService : IHotKeyService
{
    public static IHotKeyService Instance { get; } = new NullActionsService();

    public void SetHotKey(string commandId, HotKeyInfo? hotKey)
    {
        // do nothing
    }

    public HotKeyInfo? GetHotKey(string commandId)
    {
        return null;
    }

    public IEnumerable<IHotKeyInfo> GetAllActions()
    {
        return [];
    }
}
