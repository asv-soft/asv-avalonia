using System.Reactive.Disposables;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public interface IStatusItem : IRoutable
{
    int Order { get; }
}

public abstract class StatusItem(NavigationId id, ILoggerFactory loggerFactory)
    : RoutableViewModel(id, loggerFactory),
        IStatusItem
{
    public const string DefaultId = "shell.status.item";
    public abstract int Order { get; }
}
