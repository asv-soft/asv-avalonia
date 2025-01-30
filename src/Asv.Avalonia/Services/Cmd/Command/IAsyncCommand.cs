using System.Buffers;

namespace Asv.Avalonia;

public interface IAsyncCommand : IStatePersistor
{
    ICommandInfo Info { get; }
    ValueTask Execute(
        IRoutable context,
        IPersistable? parameter = null,
        CancellationToken cancel = default
    );
}
