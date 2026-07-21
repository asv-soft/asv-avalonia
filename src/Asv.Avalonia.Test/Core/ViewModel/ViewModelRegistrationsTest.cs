using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using R3;
using Xunit;

namespace Asv.Avalonia.Test;

[Collection(AvaloniaUiTestCollection.Name)]
public class ViewModelRegistrationsTest
{
    [Fact]
    public void TryCreateViewModel_FactoryIsRegistered_ReturnsViewModel()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<ViewModelFactoryDelegate<ITestViewModel, string>>(_ =>
            args => new TestViewModel(args)
        );
        using var provider = services.BuildServiceProvider();

        // Act
        var viewModel = provider.TryCreateViewModel<ITestViewModel, string>("test");

        // Assert
        var result = Assert.IsType<TestViewModel>(viewModel);
        Assert.Equal("test", result.Args);
    }

    [Fact]
    public void TryCreateViewModel_FactoryIsNotRegistered_ReturnsNull()
    {
        // Arrange
        using var provider = new ServiceCollection().BuildServiceProvider();

        // Act
        var viewModel = provider.TryCreateViewModel<ITestViewModel, string>("test");

        // Assert
        Assert.Null(viewModel);
    }

    [Fact]
    public void TryCreateViewModel_KeyedFactoryIsRegistered_ReturnsViewModel()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddKeyedTransient<ViewModelFactoryDelegate<ITestViewModel, string>>(
            "registered",
            (_, _) => args => new TestViewModel(args)
        );
        using var provider = services.BuildServiceProvider();

        // Act
        var viewModel = provider.TryCreateViewModel<ITestViewModel, string>("registered", "test");

        // Assert
        var result = Assert.IsType<TestViewModel>(viewModel);
        Assert.Equal("test", result.Args);
    }

    [Fact]
    public void TryCreateViewModel_KeyedFactoryIsNotRegistered_ReturnsNull()
    {
        // Arrange
        using var provider = new ServiceCollection().BuildServiceProvider();

        // Act
        var viewModel = provider.TryCreateViewModel<ITestViewModel, string>("missing", "test");

        // Assert
        Assert.Null(viewModel);
    }

    [AvaloniaFact]
    public void RegisterWithArgs_UiThreadConstruction_LoadsExtensionsAfterConstructor()
    {
        // Arrange
        var dispatcher = Dispatcher.UIThread;
        dispatcher.VerifyAccess();
        var builder = Host.CreateApplicationBuilder();
        var extensionService = new TestExtensionService();
        builder.Services.AddSingleton<IExtensionService>(extensionService);
        builder.Core.ViewModel.RegisterWithArgs<
            ITestExtendableViewModel,
            TestExtendableViewModel,
            string
        >();
        using var provider = builder.Services.BuildServiceProvider();
        var factory = provider.GetRequiredService<
            ViewModelFactoryDelegate<ITestExtendableViewModel, string>
        >();

        // Act
        var viewModel = factory("test");

        // Assert
        Assert.True(viewModel.ConstructorCompleted);
        Assert.False(viewModel.AfterLoadObservedCompletedConstructor);
        Assert.Equal(0, extensionService.CallCount);

        dispatcher.RunJobs();

        Assert.True(viewModel.AfterLoadObservedCompletedConstructor);
        Assert.True(extensionService.ObservedCompletedConstructor);
        Assert.Equal(1, extensionService.CallCount);
    }

    [AvaloniaFact]
    public void RegisterWithArgs_BackgroundConstruction_ThrowsInvalidOperationException()
    {
        // Arrange
        Dispatcher.UIThread.VerifyAccess();
        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddSingleton<IExtensionService>(new TestExtensionService());
        builder.Core.ViewModel.RegisterWithArgs<
            ITestExtendableViewModel,
            TestExtendableViewModel,
            string
        >();
        using var provider = builder.Services.BuildServiceProvider();
        var factory = provider.GetRequiredService<
            ViewModelFactoryDelegate<ITestExtendableViewModel, string>
        >();
        Exception? creationException = null;

        // Act
        var creationThread = new Thread(() =>
        {
            try
            {
                factory("test");
            }
            catch (Exception ex)
            {
                creationException = ex;
            }
        });
        creationThread.Start();

        // Assert
        Assert.True(creationThread.Join(TimeSpan.FromSeconds(5)));
        Assert.IsType<InvalidOperationException>(creationException);
    }

    [AvaloniaFact]
    public void Register_UiThreadConstruction_UsesStandardContainerActivation()
    {
        // Arrange
        Dispatcher.UIThread.VerifyAccess();
        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddSingleton<IExtensionService>(new TestExtensionService());
        builder.Core.ViewModel.Register<ITestExtendableViewModel, TestExtendableViewModel>();
        using var provider = builder.Services.BuildServiceProvider();

        // Act
        var viewModel = provider.GetRequiredService<ITestExtendableViewModel>();

        // Assert
        Assert.True(viewModel.ConstructorCompleted);
    }

    private interface ITestViewModel : IViewModel { }

    private interface ITestExtendableViewModel : IViewModel
    {
        bool ConstructorCompleted { get; }
        bool AfterLoadObservedCompletedConstructor { get; }
    }

    private sealed class TestViewModel(string args) : ViewModel("test"), ITestViewModel
    {
        public string Args { get; } = args;
    }

    private sealed class TestExtendableViewModel
        : ViewModel<ITestExtendableViewModel>,
            ITestExtendableViewModel
    {
        public TestExtendableViewModel(IExtensionService extensionService)
            : this("test", extensionService) { }

        public TestExtendableViewModel(string args, IExtensionService extensionService)
            : base("extendable-test", default, extensionService)
        {
            Args = args;
            ConstructorCompleted = true;
        }

        public string Args { get; }
        public bool ConstructorCompleted { get; }
        public bool AfterLoadObservedCompletedConstructor { get; private set; }

        protected override void AfterLoadExtensions()
        {
            AfterLoadObservedCompletedConstructor = ConstructorCompleted;
        }
    }

    private sealed class TestExtensionService : IExtensionService
    {
        public int CallCount { get; private set; }
        public bool ObservedCompletedConstructor { get; private set; }

        public void Extend<TInterface>(
            TInterface owner,
            string ownerTypeId,
            CompositeDisposable ownerDisposable
        )
        {
            CallCount++;
            ObservedCompletedConstructor =
                owner is ITestExtendableViewModel { ConstructorCompleted: true };
        }
    }
}
