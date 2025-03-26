using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ZLogger;

namespace Asv.Avalonia;

public static class LogServiceMixin
{
    public static IHostApplicationBuilder UseLogService(
        this IHostApplicationBuilder builder,
        Action<LogServiceBuilder>? configure = null
    )
    {
        var loggingBuilder = new LogServiceBuilder();
        configure?.Invoke(loggingBuilder);

        var options = builder
            .Services.AddSingleton<ILogService, LogService>()
            .AddOptions<LogServiceConfig>()
            .Bind(builder.Configuration);
        loggingBuilder.Build(builder, options);
        return builder;
    }
}

public class LogServiceBuilder
{
    private string _folderName = "logs";
    private int _rollingSizeKb = 1_000_000;

    internal LogServiceBuilder() { }

    public LogServiceBuilder WithRollingFileSizeKb(int sizeKb)
    {
        _rollingSizeKb = sizeKb;
        return this;
    }

    public LogServiceBuilder WithRelativeFolder(string folderName)
    {
        _folderName = folderName;
        return this;
    }

    internal void Build(IHostApplicationBuilder builder, OptionsBuilder<LogServiceConfig> opt)
    {
        opt.Configure(cfg => cfg.LogFolder = _folderName);
        builder.Logging.AddZLoggerRollingFile(options =>
        {
            options.FilePathSelector = (dt, index) => $"{_folderName}/{dt:yyyy-MM-dd}_{index}.logs";
            options.UseJsonFormatter();
            options.RollingSizeKB = _rollingSizeKb;
        });
    }
}
