using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public static class LogReaderRegistrations
{
    extension(ServicesRegistrations.Builder builder)
    {
        public ServicesRegistrations.Builder RegisterLogViewer(Action<Builder>? configure = null)
        {
            if (builder.AppBuilder.IsDesignTimeEnvironment)
            {
                return builder;
            }

            var logReaderOptions =
                builder
                    .AppBuilder.Configuration.GetSection(LogReaderOptions.Section)
                    .Get<LogReaderOptions>()
                ?? new LogReaderOptions();

            configure ??= b => b.RegisterDefault();
            configure.Invoke(new Builder(logReaderOptions));

            builder.AppBuilder.Services.AddSingleton<ILogReaderService, LogReaderService>();
            builder.AppBuilder.Services.AddSingleton(logReaderOptions);

            return builder;
        }
    }

    public class Builder(LogReaderOptions options)
    {
        public Builder RegisterDefault()
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
