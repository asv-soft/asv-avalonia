namespace Asv.Avalonia;

public interface ISupportSave : IViewModel
{
    ValueTask Save(CancellationToken cancel = default);
}
