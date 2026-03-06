using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ZLogger;

namespace Asv.Avalonia;



public static class LogReaderMixin
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UseOptionalLogViewer(Action<Builder>? configure = null)
        {
            var logReaderOptions = builder.Configuration.GetSection(LogReaderOptions.Section).Get<LogReaderOptions>() ?? new LogReaderOptions();
            configure ??= x => x.UseDefault();
            var logReaderBuilder = new Builder(builder, logReaderOptions);
            configure.Invoke(logReaderBuilder);
            builder.Logging.AddZLoggerRollingFile(options =>
            {
                options.FilePathSelector = (dt, index) =>
                    $"{logReaderOptions.Folder}/{dt:yyyy-MM-dd}_{index}.logs";
                options.UseJsonFormatter();
                options.RollingSizeKB = logReaderOptions.RollingSizeKb;
            });
            
            builder.Services.AddSingleton<ILogReaderService, LogReaderService>();
            builder.Services.AddSingleton(logReaderOptions);
            builder.Shell.Pages.UseLogViewerPage();
            
            return builder;
        }

        public IHostApplicationBuilder UseDesignTimeLogReaderService()
        {
            builder.Services.ReplaceSingleton(NullLogReaderService.Instance);
            return builder;
        }
    }
    
    public class Builder(IHostApplicationBuilder builder, LogReaderOptions options)
    {
        public void UseDefault()
        {
            WithFolder("logs");
            WithRollingSizeKb(50 * 1024);
        }

        private void WithFolder(string logs)
        {
            ArgumentNullException.ThrowIfNull(logs);
            options.Folder = logs;
        }
        
        private void WithRollingSizeKb(int rollingSizeKb)
        {
            options.RollingSizeKb = rollingSizeKb;
        }
    }
}
