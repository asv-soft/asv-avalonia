using System.Windows.Input;
using Material.Icons;
using R3;

namespace Asv.Avalonia;

public interface IHotKeyService
{
    void SetHotKey(string commandId, HotKeyInfo? hotKey);
    HotKeyInfo? GetHotKey(string commandId);
    IEnumerable<IActionInfo> GetAllActions();
}

public interface IActionInfo
{
    string Id { get; }
    string Name { get; }
    string Description { get; }
    MaterialIconKind Icon { get; }
    HotKeyInfo DefaultHotKey { get; }
}

public interface IHotKeyAction
{
    IActionInfo Info { get; }
}

public class HotKeyAction<TContext> : IHotKeyAction
{
    
    public IActionInfo Info { get; }
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

    public IEnumerable<IActionInfo> GetAllActions()
    {
        return [];
    }
}
