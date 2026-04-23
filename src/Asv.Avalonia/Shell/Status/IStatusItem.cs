using System.Reactive.Disposables;
using Asv.Modeling;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public interface IStatusItem : IViewModel
{
    int Order { get; }
}

public abstract class StatusItem(string typeId, NavArgs args, ILoggerFactory loggerFactory)
    : ViewModelBase(typeId, args, loggerFactory),
        IStatusItem
{
    public abstract int Order { get; }
}
