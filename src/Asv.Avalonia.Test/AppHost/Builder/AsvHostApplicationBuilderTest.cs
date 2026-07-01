using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Asv.Avalonia.Test;

public class AsvHostApplicationBuilderTest
{
    [Fact]
    public void Builder_WithDefaultSetup_RegistersDefault()
    {
        // Arrange / Act
        using var host = MockAppHost.Build(builder => builder.UseDefault());

        // Assert
        Assert.NotNull(host.Services.GetRequiredService<IAppInfo>());
        Assert.NotNull(host.Services.GetRequiredService<IAppArgsStore>());
        Assert.NotNull(host.Services.GetRequiredService<IAppPath>());
        Assert.NotNull(host.Services.GetRequiredService<IExtensionService>());
        Assert.NotNull(host.Services.GetRequiredService<IHotKeyService>());
        Assert.NotNull(host.Services.GetRequiredService<IShellHost>());
        Assert.NotNull(host.Services.GetRequiredService<ViewLocator>());
        Assert.NotNull(host.Services.GetRequiredService<IDialogService>());
        Assert.NotEmpty(host.Services.GetRequiredService<IUnitService>().Units);
    }
}
