namespace Asv.Avalonia;

public interface IStateSaverFactory
{
    public IStateSaver<TConfig> Create<TConfig>()
        where TConfig : new();
}
