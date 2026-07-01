using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Asv.Avalonia.Example;
using Asv.Avalonia.GeoMap;
using Asv.Avalonia.IO;
using Asv.Avalonia.Plugins;
using Asv.Common;
using Asv.IO;
using Asv.XUnit;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Asv.Avalonia.Test;

public class AsvHostApplicationBuilderTest
{
    private readonly ILogger _logger;

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
            builder => builder.RegisterDefault(),
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
        Assert.Contains(
            registeredServices,
            descriptor =>
                descriptor.ServiceType == typeof(Control)
                && Equals(
                    descriptor.ServiceKey,
                    ViewLocator.GetServiceKeyForView<PropertyEditorViewModel>()
                )
        );
        Assert.NotNull(host.Services.GetRequiredService<IDialogService>());
        Assert.NotEmpty(host.Services.GetRequiredService<IUnitService>().Units);
    }

    [Fact]
    public void Build_WithEmptyCoreConfiguration_DoesNotRegisterDefaultCoreFeatures()
    {
        // Arrange
        IServiceCollection? registeredServices = null;

        // Act
        using var host = MockAppHost.Build(
            builder => builder.RegisterCore(_ => { }),
            services => registeredServices = services
        );

        // Assert
        Assert.NotNull(registeredServices);
        Assert.False(registeredServices.IsRegistered<IAppPath>());
        Assert.False(registeredServices.IsRegistered<ViewLocator>());
        Assert.False(registeredServices.IsRegistered<IDialogService>());
        Assert.False(HasViewFor<PropertyEditorViewModel>(registeredServices));
        Assert.False(HasPage(HomePageViewModel.PageId, registeredServices));
        Assert.False(HasPage(SettingsPageViewModel.PageId, registeredServices));
        Assert.False(HasPage(LogViewerViewModel.PageId, registeredServices));
        Assert.Throws<InvalidOperationException>(() =>
            host.Services.GetRequiredService<ViewLocator>()
        );
    }

    [Fact]
    public void Build_WithCustomCoreServices_CanRegisterViewLocatorWithoutControls()
    {
        // Arrange
        IServiceCollection? registeredServices = null;

        // Act
        using var host = MockAppHost.Build(
            builder =>
                builder.RegisterCore(core =>
                    core.RegisterServices(services =>
                    {
                        services.RegisterAppInfo();
                        services.RegisterViewLocator();
                    })
                ),
            services => registeredServices = services
        );

        // Assert
        Assert.NotNull(registeredServices);
        Assert.NotNull(host.Services.GetRequiredService<IAppInfo>());
        Assert.NotNull(host.Services.GetRequiredService<ViewLocator>());
        Assert.False(registeredServices.IsRegistered<IAppPath>());
        Assert.False(registeredServices.IsRegistered<IDialogService>());
        Assert.False(HasViewFor<PropertyEditorViewModel>(registeredServices));
        Assert.False(HasViewFor<DashboardViewModel>(registeredServices));
    }

    [Fact]
    public void Build_WithCustomControls_CanRegisterOnlyPropertyEditorViews()
    {
        // Arrange
        IServiceCollection? registeredServices = null;

        // Act
        using var host = MockAppHost.Build(
            builder =>
                builder.RegisterCore(core =>
                {
                    core.RegisterServices(services => services.RegisterViewLocator());
                    core.RegisterControls(controls => controls.RegisterPropertyEditor());
                }),
            services => registeredServices = services
        );

        // Assert
        Assert.NotNull(registeredServices);
        Assert.NotNull(host.Services.GetRequiredService<ViewLocator>());
        Assert.True(HasViewFor<PropertyEditorViewModel>(registeredServices));
        Assert.True(HasViewFor<PropertyUnitViewModel>(registeredServices));
        Assert.False(HasViewFor<DashboardViewModel>(registeredServices));
        Assert.False(HasViewFor<StackPanelWidgetViewModel>(registeredServices));
    }

    [Fact]
    public void Build_WithDesktopShellCustomPages_CanDisableSettingsPage()
    {
        // Arrange
        IServiceCollection? registeredServices = null;

        // Act
        using var host = MockAppHost.Build(
            builder =>
                builder
                    .RegisterDefault()
                    .RegisterDesktopShell(shell =>
                        shell.RegisterPages(pages => pages.RegisterHomePage())
                    ),
            services => registeredServices = services
        );

        // Assert
        Assert.NotNull(registeredServices);
        Assert.True(registeredServices.IsRegistered<IShell>());
        Assert.True(HasPage(HomePageViewModel.PageId, registeredServices));
        Assert.False(HasPage(SettingsPageViewModel.PageId, registeredServices));
    }

    [Fact]
    public void Build_WithDefaultCore_RegistersLogViewerPageWithoutShellPages()
    {
        // Arrange
        IServiceCollection? registeredServices = null;

        // Act
        using var host = MockAppHost.Build(
            builder => builder.RegisterDefault(),
            services => registeredServices = services
        );

        // Assert
        Assert.NotNull(registeredServices);
        Assert.NotNull(host.Services.GetRequiredService<ILogReaderService>());
        Assert.False(HasPage(LogViewerViewModel.PageId, registeredServices));
        Assert.False(HasPage(HomePageViewModel.PageId, registeredServices));
        Assert.False(HasPage(SettingsPageViewModel.PageId, registeredServices));
    }

    [Fact]
    public void Build_WithDefaultSoloRun_UsesApplicationNameAsMutexName()
    {
        // Arrange
        var expectedName = typeof(AsvHostApplicationBuilderTest).Assembly.GetName().Name;

        // Act
        using var host = MockAppHost.Build(builder =>
            builder.RegisterCore(core =>
                core.RegisterServices(services => services.RegisterSoloRun())
            )
        );

        // Assert
        var options = host.Services.GetRequiredService<IOptions<SoloRunFeatureOptions>>().Value;
        Assert.Equal(expectedName, options.Mutex);
        Assert.False(options.ArgForward);
        Assert.Null(options.Pipe);
    }

    [Fact]
    public void Build_WithDefaultSoloRunAndArgumentForwarding_UsesApplicationNameAsPipeName()
    {
        // Arrange
        var expectedName = typeof(AsvHostApplicationBuilderTest).Assembly.GetName().Name;

        // Act
        using var host = MockAppHost.Build(builder =>
            builder.RegisterCore(core =>
                core.RegisterServices(services =>
                    services.RegisterSoloRun(soloRun => soloRun.WithArgumentForwarding())
                )
            )
        );

        // Assert
        var options = host.Services.GetRequiredService<IOptions<SoloRunFeatureOptions>>().Value;
        Assert.Equal(expectedName, options.Mutex);
        Assert.True(options.ArgForward);
        Assert.Equal(expectedName, options.Pipe);
    }

    [Fact]
    public void Build_WithConfiguredModules_RegistersOnlySelectedModuleFeatures()
    {
        // Arrange
        const string appName = "Composition Test App";
        const string appDescription = "Custom module composition test";
        const string appCompanyName = "ASV";
        const string appVersion = "1.2.3";
        const string avaloniaVersion = "test";
        const string pluginApiPackageId = "Asv.Avalonia.Test.Api";
        const string pluginApiVersion = "1.2.3";
        const string pluginPrefix = "Asv.Avalonia.Test.Plugin.";
        const string pluginFolder = "custom-plugins";
        const string pluginNugetFolder = "custom-nuget";
        const string pluginNugetCacheFolder = "custom-nuget-cache";
        const string pluginSalt = "test-salt";
        const string pluginServerName = "Local";
        const string pluginServerUrl = "https://example.test/nuget/index.json";
        var testAssembly = typeof(AsvHostApplicationBuilderTest).Assembly;
        var pluginApiSemVersion = SemVersion.Parse(pluginApiVersion);
        IServiceCollection? registeredServices = null;

        // Act
        using var host = MockAppHost.Build(
            builder =>
                builder
                    .RegisterCore(core =>
                    {
                        core.RegisterServices(services =>
                        {
                            services.RegisterAppArgsStore();
                            services.RegisterAppInfo(appInfo =>
                                appInfo
                                    .WithProductName(appName)
                                    .WithProductTitleFrom(testAssembly)
                                    .WithProductDescriptionFrom(appDescription)
                                    .WithCompanyName(appCompanyName)
                                    .WithVersion(appVersion)
                                    .WithAvaloniaVersion(avaloniaVersion)
                            );
                            services.RegisterAppPath();
                            services.RegisterDialogs(dialogs => dialogs.RegisterDefault());
                            services.RegisterExtensions();
                            services.RegisterFileAssociation();
                            services.RegisterLocalizationService();
                            services.RegisterSearchService();
                            services.RegisterThemeService();
                            services.RegisterTimeProvider();
                            services.RegisterUnitService();
                            services.RegisterUserConfig(config => config.RegisterInMemoryConfig());
                            services.RegisterViewLocator();
                        });
                        core.RegisterControls(controls =>
                            controls.RegisterTreePage().RegisterPropertyEditor()
                        );
                    })
                    .RegisterDesktopShell(shell =>
                        shell.RegisterPages(pages =>
                            pages
                                .RegisterHomePage()
                                .RegisterSettingsPage(settings => settings.RegisterUnitsSubPage())
                        )
                    )
                    .RegisterModuleGeoMap(geoMap =>
                    {
                        geoMap.RegisterCore(core =>
                        {
                            core.RegisterServices();
                            core.RegisterControls();
                            core.RegisterDialogs();
                        });
                        geoMap.RegisterShell(shell =>
                            shell.RegisterPages(pages => pages.RegisterGeoMapSettingsSubPage())
                        );
                    })
                    .RegisterModuleIo(io =>
                    {
                        io.RegisterCore(core =>
                            core.RegisterServices(services => services.RegisterDeviceManager())
                        );
                        io.RegisterShell(shell =>
                            shell.RegisterPages(pages =>
                                pages.RegisterSettings(settings =>
                                    settings.RegisterConnectionSettingsSubPage(connections =>
                                        connections.RegisterSerialPort().RegisterUdpPort()
                                    )
                                )
                            )
                        );
                    })
                    .RegisterModulePlugins(plugins =>
                    {
                        plugins.RegisterCore(core =>
                            core.RegisterServices(services =>
                            {
                                services.RegisterPluginBootloader(bootloader =>
                                    bootloader.WithApiPackage(testAssembly)
                                );
                                services.RegisterPluginManager(pluginManager =>
                                    pluginManager
                                        .WithApiPackage(pluginApiPackageId, pluginApiSemVersion)
                                        .WithPluginPrefix(pluginPrefix)
                                        .WithRelativePluginFolder(pluginFolder)
                                        .WithRelativeNugetFolder(pluginNugetFolder)
                                        .WithRelativeNugetCacheFolder(pluginNugetCacheFolder)
                                        .WithSalt(pluginSalt)
                                        .WithServer(
                                            new PluginServer(pluginServerName, pluginServerUrl)
                                        )
                                );
                            })
                        );
                        plugins.RegisterShell(shell =>
                            shell.RegisterPages(pages =>
                                pages.RegisterSettings(settings => settings.RegisterInstalled())
                            )
                        );
                    })
                    .RegisterExampleApp(example =>
                    {
                        example.RegisterCore(core =>
                            core.RegisterControls(controls =>
                                controls.RegisterDialogItemImageView()
                            )
                        );
                        example.RegisterShell(shell =>
                            shell.RegisterPages(pages =>
                                pages.RegisterControlsGallery(gallery =>
                                    gallery
                                        .RegisterMarkdownSubPage()
                                        .RegisterPropertyEditorSubPage()
                                )
                            )
                        );
                    }),
            services => registeredServices = services
        );

        // Assert
        Assert.NotNull(registeredServices);

        Assert.False(HasPage(LogViewerViewModel.PageId, registeredServices));
        Assert.False(HasViewFor<DashboardViewModel>(registeredServices));
        Assert.False(HasViewFor<StackPanelWidgetViewModel>(registeredServices));

        Assert.True(registeredServices.IsRegistered<IMapService>());
        Assert.True(registeredServices.IsRegistered<ITileLoader>());
        Assert.True(HasViewFor<MapViewModel>(registeredServices));
        Assert.True(HasViewFor<PropertyGeoPointViewModel>(registeredServices));
        Assert.True(
            HasTreeSubPage<ISettingsPage, ISettingsSubPage>(
                SettingsGeoMapViewModel.PageId,
                registeredServices
            )
        );
        Assert.False(
            HasKeyedService<IStatusItem>(registeredServices, DefaultStatusExtender.Contract)
        );

        Assert.True(registeredServices.IsRegistered<IDeviceManager>());
        Assert.True(
            HasTreeSubPage<ISettingsPage, ISettingsSubPage>(
                SettingsConnectionViewModel.SubPageId,
                registeredServices
            )
        );
        Assert.True(HasKeyedService<IPortViewModel>(registeredServices, SerialProtocolPort.Scheme));
        Assert.True(HasKeyedService<IPortViewModel>(registeredServices, UdpProtocolPort.Scheme));
        Assert.False(
            HasKeyedService<IPortViewModel>(registeredServices, TcpClientProtocolPort.Scheme)
        );
        Assert.False(
            HasKeyedService<IPortViewModel>(registeredServices, TcpServerProtocolPort.Scheme)
        );
        Assert.False(HasViewFor<ConnectionRateStatusViewModel>(registeredServices));

        Assert.True(registeredServices.IsRegistered<IPluginBootloader>());
        Assert.True(registeredServices.IsRegistered<IPluginManager>());
        Assert.True(
            HasTreeSubPage<ISettingsPage, ISettingsSubPage>(
                InstalledPluginsPageViewModel.PageId,
                registeredServices
            )
        );
        Assert.False(
            HasTreeSubPage<ISettingsPage, ISettingsSubPage>(
                PluginsMarketPageViewModel.PageId,
                registeredServices
            )
        );
        Assert.False(
            HasTreeSubPage<ISettingsPage, ISettingsSubPage>(
                SettingsPluginsSourcesViewModel.PageId,
                registeredServices
            )
        );

        var pluginOptions = host
            .Services.GetRequiredService<IOptions<PluginManagerOptions>>()
            .Value;
        Assert.Equal(pluginApiPackageId, pluginOptions.ApiPackageId);
        Assert.Equal(pluginApiVersion, pluginOptions.ApiVersion);
        Assert.Equal(pluginPrefix, pluginOptions.NugetPluginPrefix);
        Assert.Equal(pluginSalt, pluginOptions.Salt);
        Assert.EndsWith(pluginFolder, pluginOptions.PluginDirectory);
        Assert.EndsWith(pluginNugetFolder, pluginOptions.NugetDirectory);
        Assert.EndsWith(pluginNugetCacheFolder, pluginOptions.NugetCacheDirectory);
        Assert.Contains(
            pluginOptions.DefaultServers,
            server => server.Name == pluginServerName && server.SourceUri == pluginServerUrl
        );

        Assert.True(HasPage(ControlsGalleryPageViewModel.PageId, registeredServices));
        Assert.True(
            HasTreeSubPage<IControlsGalleryPage, IControlsGallerySubPage>(
                MarkdownPageViewModel.PageId,
                registeredServices
            )
        );
        Assert.True(
            HasTreeSubPage<IControlsGalleryPage, IControlsGallerySubPage>(
                PropertyEditorPageViewModel.PageId,
                registeredServices
            )
        );
        Assert.False(
            HasTreeSubPage<IControlsGalleryPage, IControlsGallerySubPage>(
                WorkspacePageViewModel.PageId,
                registeredServices
            )
        );
        Assert.False(
            HasTreeSubPage<IControlsGalleryPage, IControlsGallerySubPage>(
                DialogControlsPageViewModel.PageId,
                registeredServices
            )
        );
        Assert.False(HasPage(TextFilePageViewModel.PageId, registeredServices));
    }

    private static bool HasViewFor<TViewModel>(IServiceCollection services)
    {
        return services.Any(descriptor =>
            descriptor.ServiceType == typeof(Control)
            && Equals(descriptor.ServiceKey, ViewLocator.GetServiceKeyForView<TViewModel>())
        );
    }

    private static bool HasPage(string pageId, IServiceCollection services)
    {
        return services.Any(descriptor =>
            descriptor.ServiceType == typeof(ViewModelFactoryDelegate<IPage, IPageContext>)
            && Equals(descriptor.ServiceKey, pageId)
        );
    }

    private static bool HasKeyedService<TService>(IServiceCollection services, string key)
    {
        return services.Any(descriptor =>
            descriptor.ServiceType == typeof(TService) && Equals(descriptor.ServiceKey, key)
        );
    }

    private static bool HasTreeSubPage<TContext, TTreeSubpage>(
        string pageId,
        IServiceCollection services
    )
        where TContext : class, ITreePageViewModel
        where TTreeSubpage : class, ITreeSubpage
    {
        return services.Any(descriptor =>
            descriptor.ServiceType
                == typeof(ViewModelFactoryDelegate<TTreeSubpage, ITreeSubPageContext<TContext>>)
            && Equals(descriptor.ServiceKey, pageId)
        );
    }
}
