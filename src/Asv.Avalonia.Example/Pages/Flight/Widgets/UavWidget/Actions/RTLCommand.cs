using System.Threading;
using System.Threading.Tasks;

namespace Asv.Avalonia.Example;

public class RTLCommand: NoContextCommand
{
    public override ICommandInfo Info { get; }

    protected override ValueTask<ICommandArg?> InternalExecute(ICommandArg newValue, CancellationToken cancel)
    {
        throw new System.NotImplementedException();
    }
}