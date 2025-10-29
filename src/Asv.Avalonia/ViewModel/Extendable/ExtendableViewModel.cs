using System.Composition;
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
    where TSelfInterface : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExtendableViewModel{TSelfInterface}"/> class.
    /// </summary>
    /// <param name="id">A unique identifier for the view model.</param>
    /// <param name="layoutService"> Service used to save layout.</param>
    /// <param name="loggerFactory"> The factory used to create loggers for error handling and debugging.</param>
    protected ExtendableViewModel(NavigationId id, ILoggerFactory loggerFactory)
        : base(id, loggerFactory) { }

    /// <summary>
    /// Gets the current instance as <typeparamref name="TSelfInterface"/> or throws an exception if not implemented.
    /// </summary>
    /// <returns>The current instance cast to <typeparamref name="TSelfInterface"/>.</returns>
    /// <exception cref="Exception">
    /// Thrown if the class does not implement <typeparamref name="TSelfInterface"/>.
    /// </exception>
    protected virtual TSelfInterface GetContext()
    {
        if (this is TSelfInterface context)
        {
            return context;
        }

        throw new Exception(
            $"The class {GetType().FullName} does not implement {typeof(TSelfInterface).FullName}"
        );
    }

    /// <summary>
    /// Gets or sets a collection of extensions that enhance the functionality of the view model.
    /// Extensions are lazily imported using MEF2.
    /// </summary>
    [ImportMany]
    public IEnumerable<Lazy<IExtensionFor<TSelfInterface>>>? Extensions { get; set; }

    /// <summary>
    /// Called when MEF2 has completed importing dependencies.
    /// This method initializes and applies all available extensions to the current instance.
    /// </summary>
    [OnImportsSatisfied]
    public void Init()
    {
        try
        {
            if (Extensions != null)
            {
                var context = GetContext();
                foreach (var extension in Extensions)
                {
                    try
                    {
                        extension.Value.Extend(context, Disposable);
                        if (extension.Value is IDisposable disposable)
                        {
                            Disposable.Add(disposable);
                        }

                        Logger.ZLogTrace($"Applying extension {extension} to {this}");
                    }
                    catch (Exception e)
                    {
                        Logger.ZLogError(
                            e,
                            $"Error while loading extension {extension.Value.GetType().FullName} for {GetType().FullName}"
                        );
                    }
                }
            }

            AfterLoadExtensions();
        }
        catch (Exception e)
        {
            Logger.ZLogError(e, $"Error while loading extensions for {this}: {e.Message}");
        }
    }

    /// <summary>
    /// Called after all extensions have been loaded and applied.
    /// Derived classes must implement this method to provide additional logic after extension loading.
    /// </summary>
    protected abstract void AfterLoadExtensions();
}
