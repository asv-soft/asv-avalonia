using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Asv.Avalonia.Test;

public class UnhandledExceptionsHandlerTest
{
    [Fact]
    public async Task R3UnhandledException_AfterStop_DoesNotThrow()
    {
        // Arrange
        using var loggerFactory = LoggerFactory.Create(_ => { });
        var handler = new UnhandledExceptionsHandler(
            Options.Create(new UnhandledExceptionsHandlerOptions()),
            new ShellHost(),
            loggerFactory
        );

        await handler.StartAsync(CancellationToken.None);
        await handler.StopAsync(CancellationToken.None);

        // Act
        var exception = Record.Exception(() =>
            handler.R3UnhandledException(new InvalidOperationException("late R3 exception"))
        );

        // Assert
        Assert.Null(exception);
    }
}
