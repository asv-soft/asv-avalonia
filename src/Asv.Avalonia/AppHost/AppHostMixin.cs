using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace Asv.Avalonia;

public static class AppHostMixin
{
    public const string DesignTimeEnvironmentName = "Design";
    private const string PostConfigureCallbackKey = "PostConfigureCallback";

    extension(AppBuilder avaloniaBuilder)
    {
        public AppBuilder UseAsv(Action<AppHost.Builder> configure)
        {
            // this is for unhandled exception from R3 library
            avaloniaBuilder.UseR3(x =>
                AppHost
                    .Instance.Services.GetService<IUnhandledExceptionHandler>()
                    ?.R3UnhandledException(x)
            );

            var asvBuilder = AppHost.CreateBuilder();
            if (Design.IsDesignMode)
            {
                asvBuilder.Environment.EnvironmentName = DesignTimeEnvironmentName;
            }
            configure(asvBuilder);
            if (Design.IsDesignMode)
            {
                asvBuilder.ReplaceForDesignTime();
            }
            avaloniaBuilder.AfterPlatformServicesSetup(_ =>
            {
                var host = asvBuilder.Build();
                host.Start();
            });
            return avaloniaBuilder;
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
            builder.Environment.EnvironmentName == DesignTimeEnvironmentName;

        public IHostApplicationBuilder UseDefault()
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

            return builder
                .UseAppInfo()
                .UseJsonUserConfig()
                .UseTimeProvider()
                .UseThemeService()
                .UseSearchService()
                .UseLocalizationService()
                .UseExtensions()
                .UseHotKeys()
                .UseShellHost()
                .UseUndoStore()
                .UseViewLocator()
                .UseLayoutService()
                .UseDefaultControls()
                .UseUnitService()
                .UseFileAssociation()
                .UseDialogs()
                .UseUnhandledExceptionsHandler();
        }

        internal IHostApplicationBuilder ReplaceForDesignTime()
        {
            builder
                .UseNullExtension()
                .UseDesingTimeThemeService()
                .UseDesignTimeSearchService()
                .UseDesignTimeShell()
                .UseDesignUndoStore()
                .UseDesignTimeOptionalSoloRun()
                .UseDesignTimeLogReaderService()
                .UseDesignTimeLocalizationService()
                .UseDesignTimeLayoutService()
                .UseDesignTimeFileAssociation()
                .UseDesignTimeDialogs();
            return builder;
        }

        internal IHostApplicationBuilder UseTimeProvider()
        {
            // same type for design and runtime
            builder.Services.AddSingleton(TimeProvider.System);
            return builder;
        }
    }
}
