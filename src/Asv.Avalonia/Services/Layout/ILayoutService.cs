using Avalonia;

namespace Asv.Avalonia;

public interface ILayoutService : IExportable
{
    TPocoType Get<TPocoType>(IRoutable source)
        where TPocoType : class, new() => Get(source, new Lazy<TPocoType>());
    TPocoType Get<TPocoType>(IRoutable source, Lazy<TPocoType> defaultValue)
        where TPocoType : class, new();
    TPocoType Get<TPocoType>(StyledElement source, Lazy<TPocoType> defaultValue)
        where TPocoType : class, new();
    TPocoType Get<TPocoType>(StyledElement source)
        where TPocoType : class, new()
    {
        return Get(source, new Lazy<TPocoType>());
    }

    void SetInMemory<TPocoType>(IRoutable source, TPocoType value)
        where TPocoType : class, new();
    void SetInMemory<TPocoType>(StyledElement source, TPocoType value)
        where TPocoType : class, new();

    void RemoveFromMemory(IRoutable source);
    void RemoveFromMemory(StyledElement source);
    void RemoveFromMemoryViewModelAndView(IRoutable source);
    void RemoveFromMemoryViewModelAndView(StyledElement source);

    void FlushFromMemory(IReadOnlyCollection<IRoutable>? ignoreCollection = null);
    void FlushFromMemory(IRoutable target);
    void FlushFromMemory(StyledElement source);
    void FlushFromMemoryViewModelAndView(IRoutable target);
    void FlushFromMemoryViewModelAndView(StyledElement source);
}
