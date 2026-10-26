using Avalonia;
using Avalonia.Headless;
using Xunit;

namespace Asv.Avalonia.Test;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class WorkspaceUiTestCollection : ICollectionFixture<WorkspaceUiTestFixture>
{
    public const string Name = "Workspace UI";
}

public sealed class WorkspaceUiTestFixture : IAsyncLifetime
{
    public HeadlessUnitTestSession Session { get; } =
        HeadlessUnitTestSession.StartNew(typeof(WorkspaceUiTestAppBuilder));

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public ValueTask DisposeAsync() => Session.DisposeAsync();
}

public static class WorkspaceUiTestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<Application>().UseHeadless(new AvaloniaHeadlessPlatformOptions());
}
