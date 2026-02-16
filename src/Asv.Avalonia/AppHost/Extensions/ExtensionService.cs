using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public class NullExtensionService : IExtensionService
{
    public static IExtensionService Instance { get; } = new NullExtensionService();

    public void Extend<TInterface>(
        TInterface owner,
        string ownerKey,
        CompositeDisposable ownerDisposable
    )
    {
        // No-op implementation for null extension service
    }
}

public class ExtensionService : IExtensionService
{
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;

    public ExtensionService(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
    {
        _serviceProvider = serviceProvider;
        _logger = loggerFactory.CreateLogger(nameof(ExtensionService));
    }

    public void Extend<TInterface>(
        TInterface owner,
        string ownerKey,
        CompositeDisposable ownerDisposable
    )
    {
        try
        {
            var services = _serviceProvider.GetServices<IExtensionFor<TInterface>>();
            foreach (var extensionFor in services)
            {
                extensionFor.Extend(owner, ownerDisposable);
                if (extensionFor is IDisposable disposable)
                {
                    ownerDisposable.Add(disposable);
                }
            }

            var keyedServices = _serviceProvider.GetKeyedServices<IExtensionFor<TInterface>>(
                ownerKey
            );
            foreach (var extensionFor in keyedServices)
            {
                extensionFor.Extend(owner, ownerDisposable);
                if (extensionFor is IDisposable disposable)
                {
                    ownerDisposable.Add(disposable);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                $"Failed to extend {ownerKey} of type {typeof(TInterface).FullName}:{e.Message}"
            );
        }
    }
}
