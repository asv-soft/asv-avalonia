using Avalonia;

namespace Asv.Avalonia;

public interface ILayoutService
{
    TPocoType Get<TPocoType>(IViewModel source)
        where TPocoType : class, new() => Get(source, new Lazy<TPocoType>());
    TPocoType Get<TPocoType>(IViewModel source, Lazy<TPocoType> defaultValue)
        where TPocoType : class, new();
    TPocoType Get<TPocoType>(StyledElement source, Lazy<TPocoType> defaultValue)
        where TPocoType : class, new();
    TPocoType Get<TPocoType>(StyledElement source)
        where TPocoType : class, new()
    {
        return Get(source, new Lazy<TPocoType>());
    }

    void SetInMemory<TPocoType>(IViewModel source, TPocoType value)
        where TPocoType : class, new();
    void SetInMemory<TPocoType>(StyledElement source, TPocoType value)
        where TPocoType : class, new();

    void RemoveFromMemory(IViewModel source);
    void RemoveFromMemory(StyledElement source);
    void RemoveFromMemoryViewModelAndView(IViewModel source);
    void RemoveFromMemoryViewModelAndView(StyledElement source);

    void FlushFromMemory(IReadOnlyCollection<IViewModel>? ignoreCollection = null);
    void FlushFromMemory(IViewModel target);
    void FlushFromMemory(StyledElement source);
    void FlushFromMemoryViewModelAndView(IViewModel target);
    void FlushFromMemoryViewModelAndView(StyledElement source);
}
