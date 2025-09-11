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

    public void Set<TPocoType>(IRoutable source, TPocoType value)
        where TPocoType : class, new()
    {
        return;
    }
}
