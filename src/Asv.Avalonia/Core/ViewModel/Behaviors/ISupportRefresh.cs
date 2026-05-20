namespace Asv.Avalonia;

public interface ISupportRefresh : IViewModel
{
    ValueTask Refresh(CancellationToken cancel = default);
}
