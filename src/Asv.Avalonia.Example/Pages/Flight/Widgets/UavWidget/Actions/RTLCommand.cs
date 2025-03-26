using System.Threading;
using System.Threading.Tasks;

namespace Asv.Avalonia.Example;

public class RTLCommand: NoContextCommand
{
    public override ICommandInfo Info { get; }
    protected override ValueTask<IPersistable?> InternalExecute(IPersistable newValue, CancellationToken cancel)
    {
        throw new System.NotImplementedException();
    }
}