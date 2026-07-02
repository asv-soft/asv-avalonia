using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Asv.XUnit;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;
using UserConfiguration = Asv.Cfg.IConfiguration;

namespace Asv.Avalonia.Test;

public class AsvHostApplicationBuilderTest
{
    private readonly ILogger _logger;

    public static TheoryData<
        string,
        Action<IHostApplicationBuilder>,
        Type[]
    > UseRegistrations { get; } =
        new()
        {
            { "UseAppInfo", builder => builder.UseAppInfo(), new[] { typeof(IAppInfo) } },
            {
                "UseAppArgsStore",
                builder => builder.UseAppArgsStore(),
                new[] { typeof(IAppArgsStore) }
            },
            {
                "UseDesignTimeAppArgsStore",
                builder => builder.UseDesignTimeAppArgsStore(),
                new[] { typeof(IAppArgsStore) }
            },
            {
                "UseRestartFeature",
                builder => builder.UseRestartFeature(),
                new[]
                {
                    typeof(IAppRestartScheduler),
                    typeof(IAppRestartFeature),
                    typeof(IHostedService),
                }
            },
            {
                "UseJsonUserConfig",
                builder => builder.UseJsonUserConfig(),
                new[] { typeof(UserConfiguration) }
            },
            {
                "UseInMemoryUserConfig",
                builder => builder.UseJsonUserConfig(config => config.UseInMemoryConfig()),
                new[] { typeof(UserConfiguration) }
            },
            {
                "UseThemeService",
                builder => builder.UseThemeService(),
                new[] { typeof(IThemeService) }
            },
            {
                "UseDesingTimeThemeService",
                builder => builder.UseDesingTimeThemeService(),
                new[] { typeof(IThemeService) }
            },
            {
                "UseSearchService",
                builder => builder.UseSearchService(),
                new[] { typeof(ISearchService) }
            },
            {
                "UseDesignTimeSearchService",
                builder => builder.UseDesignTimeSearchService(),
                new[] { typeof(ISearchService) }
            },
            {
                "UseLocalizationService",
                builder => builder.UseLocalizationService(),
                new[] { typeof(ILocalizationService) }
            },
            {
                "UseDesignTimeLocalizationService",
                builder => builder.UseDesignTimeLocalizationService(),
                new[] { typeof(ILocalizationService) }
            },
            {
                "UseExtensions",
                builder => builder.UseExtensions(),
                new[] { typeof(IExtensionService) }
            },
            {
                "UseNullExtension",
                builder => builder.UseNullExtension(),
                new[] { typeof(IExtensionService) }
            },
            {
                "UseHotKeys",
                builder => builder.UseHotKeys(),
                new[] { typeof(IHotKeyService), typeof(IHotKeyAction) }
            },
            { "UseAppPath", builder => builder.UseAppPath(), new[] { typeof(IAppPath) } },
            {
                "UseViewLocator",
                builder => builder.UseViewLocator(),
                new[] { typeof(ViewLocator) }
            },
            {
                "UseDefaultControls",
                builder => builder.UseDefaultControls(),
                new[] { typeof(Control) }
            },
            {
                "UseUnitService",
                builder => builder.UseUnitService(),
                new[] { typeof(IUnitService), typeof(IUnit), typeof(IUnitItem) }
            },
            {
                "UseFileAssociation",
                builder => builder.UseFileAssociation(),
                new[] { typeof(IFileAssociationService), typeof(IHostedService) }
            },
            {
                "UseDesignTimeFileAssociation",
                builder => builder.UseDesignTimeFileAssociation(),
                new[] { typeof(IFileAssociationService) }
            },
            {
                "UseDialogs",
                builder => builder.UseDialogs(),
                new[] { typeof(IDialogService), typeof(ICustomDialog), typeof(Control) }
            },
            {
                "UseDesignTimeDialogs",
                builder => builder.UseDesignTimeDialogs(),
                new[] { typeof(IDialogService) }
            },
            {
                "UseUnhandledExceptionsHandler",
                builder => builder.UseUnhandledExceptionsHandler(),
                new[] { typeof(IUnhandledExceptionHandler), typeof(IHostedService) }
            },
            {
                "UseDesignTimeShell",
                builder => builder.UseDesignTimeShell(),
                new[] { typeof(IShell), typeof(IExtensionFor<IShell>) }
            },
            {
                "UseMobileShell",
                builder => builder.UseMobileShell(),
                new[] { typeof(IShell), typeof(IExtensionFor<IShell>) }
            },
            {
                "UseDesktopShell",
                builder => builder.UseDesktopShell(),
                new[]
                {
                    typeof(IShell),
                    typeof(IExtensionFor<IShell>),
                    typeof(ShellWindow),
                    typeof(IDebugWindow),
                    typeof(Control),
                }
            },
        };

    public AsvHostApplicationBuilderTest(ITestOutputHelper output)
    {
        _logger = new TestLoggerFactory(
            output,
            TimeProvider.System,
            "composition"
        ).CreateLogger<AsvHostApplicationBuilderTest>();
    }

    [Fact]
    public void Build_WithDefaultSetup_RegistersDefault()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        IServiceCollection? registeredServices = null;

        // Act
        using var host = MockAppHost.Build(
            builder => builder.UseDefault(),
            services =>
            {
                registeredServices = services;
                CompositionServiceLogger.Log(services, _logger);
            },
            fileSystem
        );

        // Assert
        Assert.NotNull(registeredServices);
        Assert.True(registeredServices.IsRegistered<IAppPath>());
        Assert.NotNull(host.Services.GetRequiredService<IAppInfo>());
        Assert.NotNull(host.Services.GetRequiredService<IAppArgsStore>());
        Assert.Same(fileSystem, host.Services.GetRequiredService<IFileSystem>());
        Assert.NotNull(host.Services.GetRequiredService<IExtensionService>());
        Assert.NotNull(host.Services.GetRequiredService<IHotKeyService>());
        Assert.NotNull(host.Services.GetRequiredService<IShellHost>());
        Assert.NotNull(host.Services.GetRequiredService<ViewLocator>());
        Assert.NotNull(host.Services.GetRequiredService<IDialogService>());
        Assert.NotEmpty(host.Services.GetRequiredService<IUnitService>().Units);
    }

    [Theory]
    [MemberData(nameof(UseRegistrations))]
    public void Build_WithSingleUse_RegistersExpectedServices(
        string name,
        Action<IHostApplicationBuilder> configure,
        Type[] expectedServices
    )
    {
        // Arrange
        IServiceCollection? registeredServices = null;

        // Act
        using var host = MockAppHost.Build(
            configure,
            services =>
            {
                registeredServices = services;
                _logger.LogInformation("Use registration case: {Name}", name);
                CompositionServiceLogger.Log(services, _logger);
            }
        );

        // Assert
        Assert.NotNull(registeredServices);
        foreach (var expectedService in expectedServices)
        {
            Assert.True(
                registeredServices.IsRegistered(expectedService),
                $"Expected service '{expectedService.FullName}' was not registered by '{name}'."
            );
        }
    }
}
