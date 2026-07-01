using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Asv.Avalonia.Test;

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

    private interface ITestViewModel : IViewModel { }

    private sealed class TestViewModel(string args) : ViewModel("test"), ITestViewModel
    {
        public string Args { get; } = args;
    }
}
