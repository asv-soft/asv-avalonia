namespace Asv.Avalonia;

public class NullStateSaverFactory : IStateSaverFactory
{
    public static NullStateSaverFactory Instance => new();

    private NullStateSaverFactory() { }

    public IStateSaver<TConfig> Create<TConfig>()
        where TConfig : new()
    {
        return new NullStateFactory<TConfig>();
    }
}
