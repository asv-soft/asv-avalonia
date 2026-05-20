namespace Asv.Avalonia;

public interface ISupportRemove : IViewModel
{
    ValueTask Remove(CancellationToken ct);
}
