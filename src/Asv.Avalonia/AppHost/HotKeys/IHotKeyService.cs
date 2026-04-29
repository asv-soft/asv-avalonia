using System.Windows.Input;
using Material.Icons;
using R3;

namespace Asv.Avalonia;

public interface IHotKeyService
{
    
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

public class NullActionsService : IHotKeyService
{
    public static IHotKeyService Instance { get; } = new NullActionsService();
    
}



public interface IActionFactory
{
    
}

public class ActionFactory
{
    
}