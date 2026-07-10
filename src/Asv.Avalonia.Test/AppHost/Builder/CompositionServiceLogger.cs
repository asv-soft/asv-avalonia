using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Test;

internal static class CompositionServiceLogger
{
    public static void Log(IServiceCollection services, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(logger);

        logger.LogInformation("Registered services: {ServiceCount}", services.Count);

        for (var i = 0; i < services.Count; i++)
        {
            var descriptor = services[i];
            logger.LogInformation(
                "{Index:000} | {Lifetime,-9} | {ServiceType} | {Implementation} | {Key}",
                i + 1,
                descriptor.Lifetime,
                GetTypeName(descriptor.ServiceType),
                GetImplementationName(descriptor),
                GetKeyName(descriptor)
            );
        }
    }

    private static string GetImplementationName(ServiceDescriptor descriptor)
    {
        if (descriptor.IsKeyedService)
        {
            if (descriptor.KeyedImplementationType is { } keyedType)
            {
                return GetTypeName(keyedType);
            }

            if (descriptor.KeyedImplementationInstance is { } keyedInstance)
            {
                return $"instance:{GetTypeName(keyedInstance.GetType())}";
            }

            if (descriptor.KeyedImplementationFactory is not null)
            {
                return "keyed-factory";
            }

            return "keyed-unknown";
        }

        if (descriptor.ImplementationType is { } type)
        {
            return GetTypeName(type);
        }

        if (descriptor.ImplementationInstance is { } instance)
        {
            return $"instance:{GetTypeName(instance.GetType())}";
        }

        if (descriptor.ImplementationFactory is not null)
        {
            return "factory";
        }

        return "unknown";
    }

    private static string GetKeyName(ServiceDescriptor descriptor)
    {
        return descriptor.IsKeyedService ? descriptor.ServiceKey?.ToString() ?? "<null>" : "-";
    }

    private static string GetTypeName(Type type)
    {
        return type.FullName ?? type.Name;
    }
}
