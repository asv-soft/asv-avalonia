using System.Globalization;
using Avalonia.Controls;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Asv.Avalonia.Map;

public static class MapCore
{
    public static ILoggerFactory LoggerFactory { get; } = NullLoggerFactory.Instance;
}
