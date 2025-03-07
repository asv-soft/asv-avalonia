using System.Composition;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
    protected ExtendableViewModel(NavigationId id)
        : base(id) { }

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
    /// Gets or sets the logger factory used to create loggers for error handling and debugging.
    /// </summary>
    [Import]
    public ILoggerFactory? LoggerFactory { get; set; }

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
                var logger = LoggerFactory?.CreateLogger(GetType()) ?? NullLogger.Instance;

                var context = GetContext();
                foreach (var extension in Extensions)
                {
                    try
                    {
                        extension.Value.Extend(context);
                        logger.ZLogTrace($"Applying extension {extension} to {GetType().Name}");
                    }
                    catch (Exception e)
                    {
                        logger.ZLogError(
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
            LoggerFactory
                ?.CreateLogger(GetType())
                .ZLogError(e, $"Error while loading extensions for {GetType().FullName}");
        }
    }

    /// <summary>
    /// Called after all extensions have been loaded and applied.
    /// Derived classes must implement this method to provide additional logic after extension loading.
    /// </summary>
    protected abstract void AfterLoadExtensions();

    /// <summary>
    /// Disposes of the extensions when the view model is being disposed.
    /// Ensures that all created extensions are properly cleaned up.
    /// </summary>
    /// <param name="disposing">Indicates whether the object is being disposed explicitly.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (Extensions != null)
            {
                foreach (var item in Extensions)
                {
                    if (item.IsValueCreated)
                    {
                        item.Value.Dispose();
                    }
                }
            }
        }

        base.Dispose(disposing);
    }
}
