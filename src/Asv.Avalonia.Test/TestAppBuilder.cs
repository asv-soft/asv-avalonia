using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Xunit;

[assembly: AvaloniaTestApplication(typeof(Asv.Avalonia.Test.TestAppBuilder))]

namespace Asv.Avalonia.Test;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class AvaloniaUiTestCollection
{
    public const string Name = "Avalonia UI";
}

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder
            .Configure<TestApplication>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
    }

    public sealed class TestApplication : Application;
}
