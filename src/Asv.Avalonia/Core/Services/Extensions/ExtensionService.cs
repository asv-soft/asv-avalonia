using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using R3;
using ZLogger;

namespace Asv.Avalonia;

public class NullExtensionService : IExtensionService
{
    public static IExtensionService Instance { get; } = new NullExtensionService();

    public void Extend<TInterface>(
        TInterface owner,
        string ownerTypeId,
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
        string ownerTypeId,
        CompositeDisposable ownerDisposable
    )
    {
        try
        {
            var extensions = ResolveExtensions<TInterface>(ownerTypeId).ToArray();
            foreach (var policy in ResolvePolicies<TInterface>(ownerTypeId))
            {
                var filteredExtensions = policy.Filter(owner, extensions).ToArray();
                var removedExtensions = GetRemovedExtensions(extensions, filteredExtensions);
                LogFilteredExtensions(
                    ownerTypeId,
                    policy,
                    extensions,
                    filteredExtensions,
                    removedExtensions
                );
                DisposeRemovedExtensions(ownerTypeId, policy, removedExtensions);
                extensions = filteredExtensions;
            }

            foreach (var extensionFor in extensions)
            {
                ApplyExtension(ownerTypeId, extensionFor, owner, ownerDisposable);
            }
        }
        catch (Exception e)
        {
            _logger.ZLogError(
                e,
                $"Failed to extend {ownerTypeId} of type {typeof(TInterface).FullName}: {e.Message}"
            );
        }
    }

    private IEnumerable<IExtensionFor<TInterface>> ResolveExtensions<TInterface>(string ownerTypeId)
    {
        return _serviceProvider
            .GetServices<IExtensionFor<TInterface>>()
            .Concat(_serviceProvider.GetKeyedServices<IExtensionFor<TInterface>>(ownerTypeId));
    }

    private IEnumerable<IExtensionPolicyFor<TInterface>> ResolvePolicies<TInterface>(
        string ownerTypeId
    )
    {
        return _serviceProvider
            .GetServices<IExtensionPolicyFor<TInterface>>()
            .Concat(_serviceProvider.GetKeyedServices<IExtensionPolicyFor<TInterface>>(ownerTypeId))
            .OrderBy(policy => policy.Order);
    }

    private static IReadOnlyCollection<IExtensionFor<TInterface>> GetRemovedExtensions<TInterface>(
        IReadOnlyCollection<IExtensionFor<TInterface>> previous,
        IReadOnlyCollection<IExtensionFor<TInterface>> current
    )
    {
        var currentExtensions = new HashSet<object>(
            current.Cast<object>(),
            ReferenceEqualityComparer.Instance
        );
        var removedExtensionSet = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var removedExtensions = new List<IExtensionFor<TInterface>>();

        foreach (var extension in previous)
        {
            if (
                currentExtensions.Contains(extension)
                || removedExtensionSet.Add(extension) == false
            )
            {
                continue;
            }

            removedExtensions.Add(extension);
        }

        return removedExtensions;
    }

    private void LogFilteredExtensions<TInterface>(
        string ownerTypeId,
        IExtensionPolicyFor<TInterface> policyFor,
        IReadOnlyCollection<IExtensionFor<TInterface>> previous,
        IReadOnlyCollection<IExtensionFor<TInterface>> current,
        IReadOnlyCollection<IExtensionFor<TInterface>> removed
    )
    {
        if (_logger.IsEnabled(LogLevel.Trace) == false)
        {
            return;
        }

        _logger.ZLogTrace(
            $"Extension policy {policyFor.Id} filtered extensions for {ownerTypeId} ({typeof(TInterface).FullName}). Before: {previous.Count}; After: {current.Count}; Removed: {GetExtensionIds(removed)}; Result: {GetExtensionIds(current)}"
        );
    }

    private void DisposeRemovedExtensions<TInterface>(
        string ownerTypeId,
        IExtensionPolicyFor<TInterface> policyFor,
        IEnumerable<IExtensionFor<TInterface>> removedExtensions
    )
    {
        foreach (var extension in removedExtensions)
        {
            _logger.ZLogTrace(
                $"Extension {extension.Id} was removed by policy {policyFor.Id} for {ownerTypeId}"
            );
            DisposeExtension(ownerTypeId, extension);
        }
    }

    private void ApplyExtension<TInterface>(
        string ownerTypeId,
        IExtensionFor<TInterface> extensionFor,
        TInterface owner,
        CompositeDisposable ownerDisposable
    )
    {
        _logger.ZLogTrace(
            $"Extend {typeof(TInterface).FullName}(id:{ownerTypeId}) by {extensionFor.Id}"
        );

        extensionFor.Extend(owner, ownerDisposable);

        if (extensionFor is IDisposable disposable)
        {
            ownerDisposable.Add(disposable);
        }
    }

    private void DisposeExtension<TInterface>(
        string ownerTypeId,
        IExtensionFor<TInterface> extension
    )
    {
        if (extension is not IDisposable disposable)
        {
            return;
        }

        try
        {
            disposable.Dispose();
            _logger.ZLogTrace($"Disposed filtered extension {extension.Id} for {ownerTypeId}");
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e, $"Failed to dispose filtered extension {extension.Id}");
        }
    }

    private static string GetExtensionIds<TInterface>(
        IEnumerable<IExtensionFor<TInterface>> extensions
    )
    {
        return string.Join(", ", extensions.Select(extension => extension.Id));
    }
}
