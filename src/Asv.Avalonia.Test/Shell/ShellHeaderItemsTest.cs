using Asv.Avalonia.Example;
using Avalonia;
using System.Globalization;
using Asv.Modeling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using R3;
using Xunit;

namespace Asv.Avalonia.Test;

public class ShellHeaderItemsTest
{
    [Fact]
    public void HeaderItems_AttachToShellAndKeepInsertionOrder()
    {
        using var fixture = new Fixture();
        var first = new HeaderItem("first");
        var second = new HeaderItem("second");
        fixture.Shell.HeaderItems.Add(first);
        fixture.Shell.HeaderItems.Add(second);
        Assert.Same(fixture.Shell, first.Parent);
        Assert.Equal(new IViewModel[] { first, second }, fixture.Shell.HeaderItemsView);
        Assert.Contains(first, fixture.Shell.GetChildren());
        Assert.Contains(second, fixture.Shell.GetChildren());
        fixture.Shell.HeaderItems.Remove(first);
        Assert.Null(first.Parent);
        Assert.True(first.IsDisposed);
        Assert.False(second.IsDisposed);
        Assert.Same(second, Assert.Single(fixture.Shell.HeaderItemsView));
    }

    [Fact]
    public void HeaderItems_ClearDisposesAllRemovedModels()
    {
        using var fixture = new Fixture();
        var item = new HeaderItem("header");
        fixture.Shell.HeaderItems.Add(item);
        fixture.Shell.HeaderItems.Clear();
        Assert.Empty(fixture.Shell.HeaderItemsView);
        Assert.True(item.IsDisposed);
        Assert.Null(item.Parent);
    }

    [Fact]
    public void HeaderItems_ShellDisposalDisposesRemainingModels()
    {
        using var fixture = new Fixture();
        var item = new HeaderItem("header");
        fixture.Shell.HeaderItems.Add(item);
        fixture.Shell.Dispose();
        Assert.True(item.IsDisposed);
        Assert.Null(item.Parent);
    }

    [Fact]
    public void ExampleExtension_UnloadRemovesAndDisposesOnlyItsAvatar()
    {
        using var fixture = new Fixture();
        var other = new HeaderItem("other-plugin");
        fixture.Shell.HeaderItems.Add(other);
        using var lifetime = new CompositeDisposable();
        new ExampleUserHeaderExtension().Extend(fixture.Shell, lifetime);
        var avatar = Assert.Single(fixture.Shell.HeaderItems.OfType<ExampleUserHeaderViewModel>());
        Assert.Same(fixture.Shell, avatar.Parent);
        lifetime.Dispose();
        Assert.True(avatar.IsDisposed);
        Assert.Same(other, Assert.Single(fixture.Shell.HeaderItems));
        Assert.False(other.IsDisposed);
    }

    [Theory]
    [InlineData(0, 0, 210)]
    [InlineData(120, 0, 210)]
    [InlineData(28, 1, 246)]
    [InlineData(400, 2, 618)]
    public void HeaderPadding_ReservesTheMeasuredWidthAndRestoresEmptyLayout(double width, int count, double expected)
    {
        var padding = ShellHeaderPaddingConverter.Instance.Convert([width, count], typeof(Thickness), null, CultureInfo.InvariantCulture);
        Assert.Equal(new Thickness(100, 0, expected, 0), Assert.IsType<Thickness>(padding));
    }

    [Fact]
    public async Task NavigateToAlreadySelectedPage_RequestsActivationWithoutCreatingAnotherPage()
    {
        using var fixture = new Fixture();
        var page = (IPage)await fixture.Shell.Navigate(new NavId("focus-test"));
        var requests = 0;
        using var subscription = page.Events.Catch<PageActivationRequestedEvent>(_ => requests++);
        Assert.Same(page, await fixture.Shell.Navigate(page.Id));
        Assert.Same(page, await fixture.Shell.Navigate(page.Id));
        Assert.Equal(2, requests);
        Assert.Same(page, fixture.Shell.SelectedPage.Value);
        Assert.Single(fixture.Shell.GetChildren().OfType<IPage>());
    }

    private sealed class FocusTestPage(IPageContext context)
        : PageViewModel<IPage>("focus-test", context, NullLoggerFactory.Instance,
            NullDialogService.Instance, NullExtensionService.Instance)
    {
        protected override void AfterLoadExtensions() { }
    }

    private sealed class HeaderItem(string id) : ViewModel(id);

    private sealed class TestShell(IServiceProvider services, IAppPath paths)
        : ShellViewModel(services, NullLoggerFactory.Instance, paths, NullThemeService.Instance,
            NullDialogService.Instance, NullExtensionService.Instance);

    private sealed class Fixture : IDisposable, IAppPath
    {
        private readonly string _root = Path.Combine(Path.GetTempPath(), $"asv-header-{Guid.NewGuid():N}");
        private readonly ServiceProvider _services;
        private readonly SynchronizationContext? _previous = SynchronizationContext.Current;
        public ShellViewModel Shell { get; }
        public Fixture()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            _services = new ServiceCollection().AddOptions().Configure<HomePageOptions>(_ => { })
                .AddKeyedTransient<ViewModelFactoryDelegate<IPage, IPageContext>>("focus-test", (_, _) => context => new FocusTestPage(context))
                .BuildServiceProvider();
            Shell = new TestShell(_services, this);
        }
        public string GetAppPathFolder(string prefix)
        {
            var path = Path.Combine(_root, prefix);
            Directory.CreateDirectory(path);
            return path;
        }
        public string GetAppPathFile(string name) => Path.Combine(GetAppPathFolder("files"), name);
        public string GetPageFolder(NavId id, string prefix) => GetAppPathFolder(prefix);
        public void Dispose()
        {
            Shell.Dispose();
            _services.Dispose();
            SynchronizationContext.SetSynchronizationContext(_previous);
            Directory.Delete(_root, true);
        }
    }
}
