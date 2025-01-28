namespace Asv.Avalonia;

public interface IOptionCollection
{
    public TOptions GetOptions<TOptions>()
        where TOptions : class;
}
