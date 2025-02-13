using System.Buffers;
using R3;

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
