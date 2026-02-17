using Avalonia;

namespace Asv.Avalonia;

public sealed class NullLayoutService : ILayoutService
{
    public static ILayoutService Instance { get; } = new NullLayoutService();

    private NullLayoutService() { }

    public TPocoType Get<TPocoType>(IRoutable source, Lazy<TPocoType> defaultValue)
        where TPocoType : class, new()
    {
        return defaultValue.Value;
    }

    public void SetInMemory<TPocoType>(IRoutable source, TPocoType value)
        where TPocoType : class, new()
    {
        return;
    }

    public void SetToFile<TPocoType>(IRoutable source, TPocoType value)
        where TPocoType : class, new()
    {
        return;
    }

    public void FlushFromMemory(IReadOnlyCollection<IRoutable>? keysToIgnore = null)
    {
        return;
    }

    public void FlushFromMemory(IRoutable target)
    {
        return;
    }

    public void RemoveFromMemory(StyledElement source)
    {
        return;
    }

    public void RemoveFromMemoryViewModelAndView(IRoutable source)
    {
        return;
    }

    public void RemoveFromMemoryViewModelAndView(StyledElement source)
    {
        return;
    }

    public TPocoType Get<TPocoType>(StyledElement source, Lazy<TPocoType> defaultValue)
        where TPocoType : class, new()
    {
        return defaultValue.Value;
    }

    public void SetInMemory<TPocoType>(StyledElement source, TPocoType value)
        where TPocoType : class, new()
    {
        return;
    }

    public void RemoveFromMemory(IRoutable source)
    {
        return;
    }

    public void FlushFromMemory(StyledElement source)
    {
        return;
    }

    public void FlushFromMemoryViewModelAndView(IRoutable target)
    {
        return;
    }

    public void FlushFromMemoryViewModelAndView(StyledElement source)
    {
        return;
    }
}
