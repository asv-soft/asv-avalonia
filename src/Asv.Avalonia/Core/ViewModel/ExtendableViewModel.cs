using Asv.Modeling;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public abstract class ExtendableViewModel<TSharedInterface> : ViewModelBase
    where TSharedInterface : class
{
    protected ExtendableViewModel(string typeId, NavArgs args, ILoggerFactory loggerFactory, IExtensionService extensionService)
        : base(typeId, args, loggerFactory)
    {
        var self =
            this as TSharedInterface
            ?? throw new Exception(
                $"The class {GetType().FullName} does not implement {typeof(TSharedInterface).FullName}"
            );

        // we load extensions on the UI thread to avoid deadlocks
        Dispatcher.UIThread.Post(
            () =>
            {
                if (IsDisposed)
                {
                    return;
                }

                extensionService.Extend(self, Id.TypeId, Disposable);

                AfterLoadExtensions();
            },
            DispatcherPriority.Background
        );
    }

    /// <summary>
    /// Gets the current instance as <typeparamref name="TSharedInterface"/> or throws an exception if not implemented.
    /// </summary>
    /// <returns>The current instance cast to <typeparamref name="TSharedInterface"/>.</returns>
    /// <exception cref="Exception">
    /// Thrown if the class does not implement <typeparamref name="TSharedInterface"/>.
    /// </exception>
    protected virtual TSharedInterface GetContext()
    {
        return this as TSharedInterface
               ?? throw new Exception(
                   $"The class {GetType().FullName} does not implement {typeof(TSharedInterface).FullName}"
               );
    }

    /// <summary>
    /// Called after all extensions have been loaded and applied.
    /// Derived classes must implement this method to provide additional logic after extension loading.
    /// </summary>
    protected abstract void AfterLoadExtensions();
}