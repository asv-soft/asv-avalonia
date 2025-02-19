using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Asv.Avalonia.Map;

internal static class MapCore
{
    public static ILoggerFactory LoggerFactory { get; } = NullLoggerFactory.Instance;
    public static ITileCache FastCache { get; } 
}

public static class MapBuilder
{
    public static AppBuilder UseAsvMap(this AppBuilder builder)
    {
        return builder.AfterSetup(_ =>
        {
            
        });
    }
}