using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class LogReaderMixin
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UseOptionalLogViewer(Action<Builder>? configure = null)
        {
            if (builder.IsDesignTimeEnvironment)
            {
                return builder;
            }

            var logReaderOptions =
                builder.Configuration.GetSection(LogReaderOptions.Section).Get<LogReaderOptions>()
                ?? new LogReaderOptions();
            configure?.Invoke(new Builder(logReaderOptions));

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

    public class Builder(LogReaderOptions options)
    {
        public Builder UseDefault()
        {
            return WithFolder("logs");
        }

        public Builder WithFolder(string folder)
        {
            ArgumentNullException.ThrowIfNull(folder);
            options.Folder = folder;
            return this;
        }
    }
}
