using Asv.Common;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace Asv.Avalonia;

/// <summary>
/// Represents a base class for a view model that supports extensibility using MEF2.
/// This class provides a mechanism to load and apply extensions dynamically.
/// </summary>
/// <typeparam name="TSelfInterface">
/// The interface type that the implementing class must inherit from.
/// </typeparam>
public abstract class ExtendableViewModel<TSelfInterface> : RoutableViewModel
    where TSelfInterface : class, ISupportId<NavigationId>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExtendableViewModel{TSelfInterface}"/> class.
    /// </summary>
    /// <param name="id">A unique identifier for the view model.</param>
    /// <param name="layoutService"> Service used to save layout.</param>
    /// <param name="loggerFactory"> The factory used to create loggers for error handling and debugging.</param>
    /// <param name="ext"> The extension service used to load and apply extensions to the view model.</param>
    protected ExtendableViewModel(
        NavigationId id,
        ILoggerFactory loggerFactory,
        IExtensionService ext
    )
        : base(id, loggerFactory)
    {
        var self =
            this as TSelfInterface
            ?? throw new Exception(
                $"The class {GetType().FullName} does not implement {typeof(TSelfInterface).FullName}"
            );

        // we load extensions on the UI thread to avoid deadlocks
        Dispatcher.UIThread.Post(
            () =>
            {
                if (IsDisposed)
                {
                    return;
                }

                ext.Extend(self, id.Id, Disposable);

                AfterLoadExtensions();
            },
            DispatcherPriority.Background
        );
    }

    /// <summary>
    /// Gets the current instance as <typeparamref name="TSelfInterface"/> or throws an exception if not implemented.
    /// </summary>
    /// <returns>The current instance cast to <typeparamref name="TSelfInterface"/>.</returns>
    /// <exception cref="Exception">
    /// Thrown if the class does not implement <typeparamref name="TSelfInterface"/>.
    /// </exception>
    protected virtual TSelfInterface GetContext()
    {
        return this as TSelfInterface
            ?? throw new Exception(
                $"The class {GetType().FullName} does not implement {typeof(TSelfInterface).FullName}"
            );
    }

    /// <summary>
    /// Called after all extensions have been loaded and applied.
    /// Derived classes must implement this method to provide additional logic after extension loading.
    /// </summary>
    protected abstract void AfterLoadExtensions();
}
