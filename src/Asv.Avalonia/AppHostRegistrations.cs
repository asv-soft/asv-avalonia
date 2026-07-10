using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace Asv.Avalonia;

public static class AppHostRegistrations
{
    private const string PostConfigureCallbackKey = "PostConfigureCallback";

    extension(AppBuilder avaloniaBuilder)
    {
        public AppBuilder UseAsv(Action<AppHost.Builder> configure)
        {
            // this is for unhandled exception from R3 library
            avaloniaBuilder.UseR3(HandleR3UnhandledException);

            var asvBuilder = AppHost.CreateBuilder();
            configure(asvBuilder);

            avaloniaBuilder.AfterPlatformServicesSetup(_ =>
            {
                var host = asvBuilder.Build();
                host.Services.GetService<ILocalizationService>();
                host.Start();
            });
            return avaloniaBuilder;
        }
    }

    private static void HandleR3UnhandledException(Exception ex)
    {
        try
        {
            AppHost
                .Instance.Services.GetService<IUnhandledExceptionHandler>()
                ?.R3UnhandledException(ex);
        }
        catch (ObjectDisposedException)
        {
            // R3 may report late exceptions while the host service provider is already disposed.
        }
    }

    extension(IHostApplicationBuilder builder)
    {
        public void ExecutePostConfigureCallbacks()
        {
            if (builder.Properties.TryGetValue(PostConfigureCallbackKey, out var callback))
            {
                var list = (List<Action<IHostApplicationBuilder>>)callback;
                foreach (var action in list)
                {
                    action(builder);
                }
                builder.Properties.Remove(PostConfigureCallbackKey);
            }
        }

        public void AddPostConfigureCallbacks(Action<IHostApplicationBuilder> action)
        {
            if (builder.Properties.TryGetValue(PostConfigureCallbackKey, out var callback))
            {
                var list = (List<Action<IHostApplicationBuilder>>)callback;
                list.Add(action);
            }
            else
            {
                builder.Properties[PostConfigureCallbackKey] = new List<
                    Action<IHostApplicationBuilder>
                >
                {
                    action,
                };
            }
        }

        public bool IsDesignTimeEnvironment =>
            builder.Environment.EnvironmentName == DesignTime.EnvironmentName;

        public IHostApplicationBuilder RegisterDefault()
        {
            return builder.EnableLogging().RegisterCore();
        }

        public IHostApplicationBuilder EnableLogging()
        {
            builder.Logging.ClearProviders();
            builder.Logging.AddZLoggerConsole(options =>
            {
                options.IncludeScopes = true;
                options.OutputEncodingToUtf8 = false;
                options.UsePlainTextFormatter(formatter =>
                {
                    formatter.SetPrefixFormatter(
                        $"{0:HH:mm:ss.fff} | {3:00} | ={1:short}= | {2, -40} ",
                        (in template, in info) =>
                            template.Format(
                                info.Timestamp,
                                info.LogLevel,
                                info.Category,
                                Environment.CurrentManagedThreadId
                            )
                    );
                });
            });

            return builder;
        }
    }
}
