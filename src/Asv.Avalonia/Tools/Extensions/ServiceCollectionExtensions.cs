using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Asv.Avalonia;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ReplaceSingleton<TService, TImplementation>(
        this IServiceCollection services)
        where TImplementation : class, TService 
        where TService : class
    {
        services.Replace(ServiceDescriptor.Singleton<TService, TImplementation>());
        return services;
    }
    
    public static bool IsRegistered<T>(this IServiceCollection services)
        => services.Any(sd => sd.ServiceType == typeof(T));

    public static bool IsRegistered(this IServiceCollection services, Type serviceType)
        => services.Any(sd => sd.ServiceType == serviceType);

    public static void RequireRegistered<T>(this IServiceCollection services, string? message = null)
    {
        if (!services.IsRegistered<T>())
        {
            throw new InvalidOperationException(
                message ?? $"The service of type {typeof(T).FullName} is not registered.");
        }
    }
}