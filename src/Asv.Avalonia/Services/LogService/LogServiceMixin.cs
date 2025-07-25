﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class LogServiceMixin
{
    public static IHostApplicationBuilder UseLogService(this IHostApplicationBuilder builder)
    {
        builder
            .Services.AddSingleton<ILogService, LogService>()
            .AddOptions<LogServiceConfig>()
            .Bind(builder.Configuration.GetSection(LogServiceConfig.Section));
        return builder;
    }
}
