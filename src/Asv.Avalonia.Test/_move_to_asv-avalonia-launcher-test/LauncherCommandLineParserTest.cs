using Asv.Avalonia.Launcher.Api;
using Asv.Avalonia.Launcher.Orchestration;
using Xunit;

namespace Asv.Avalonia.Test;

public class LauncherCommandLineParserTest
{
    [Fact]
    public void WithDefaultTarget_TargetSpecified_ReturnsOriginalArguments()
    {
        // Arrange
        var args = new[] { LauncherCommandLineArguments.TargetArg, "custom.exe" };

        // Act
        var result = LauncherCommandLineParser.WithDefaultTarget(args, "default.exe");

        // Assert
        Assert.Same(args, result);
    }

    [Fact]
    public void WithDefaultTarget_TargetMissing_AddsResolvedDefaultTarget()
    {
        // Arrange
        var args = new[] { LauncherCommandLineArguments.TimeoutSecArg, "30" };
        var expectedPath = Path.GetFullPath("default.exe", AppContext.BaseDirectory);

        // Act
        var result = LauncherCommandLineParser.WithDefaultTarget(args, "default.exe");

        // Assert
        Assert.Equal(
            [
                LauncherCommandLineArguments.TimeoutSecArg,
                "30",
                LauncherCommandLineArguments.TargetArg,
                expectedPath,
            ],
            result
        );
    }

    [Fact]
    public void WithDefaultTarget_PassthroughArguments_InsertsTargetBeforeSeparator()
    {
        // Arrange
        var args = new[]
        {
            LauncherCommandLineArguments.PassthroughArgsSeparator,
            "--target-app-argument",
        };
        var expectedPath = Path.GetFullPath("default.exe", AppContext.BaseDirectory);

        // Act
        var result = LauncherCommandLineParser.WithDefaultTarget(args, "default.exe");

        // Assert
        Assert.Equal(
            [
                LauncherCommandLineArguments.TargetArg,
                expectedPath,
                LauncherCommandLineArguments.PassthroughArgsSeparator,
                "--target-app-argument",
            ],
            result
        );
    }

    [Fact]
    public void WithDefaultTarget_TargetTokenUsedAsOptionValue_AddsDefaultTarget()
    {
        // Arrange
        var args = new[]
        {
            LauncherCommandLineArguments.PipeArg,
            LauncherCommandLineArguments.TargetArg,
        };
        var expectedPath = Path.GetFullPath("default.exe", AppContext.BaseDirectory);

        // Act
        var result = LauncherCommandLineParser.WithDefaultTarget(args, "default.exe");

        // Assert
        Assert.Equal(
            [
                LauncherCommandLineArguments.PipeArg,
                LauncherCommandLineArguments.TargetArg,
                LauncherCommandLineArguments.TargetArg,
                expectedPath,
            ],
            result
        );
    }

    [Fact]
    public void WithDefaultTarget_SeparatorTokenUsedAsOptionValue_PreservesExplicitTarget()
    {
        // Arrange
        var args = new[]
        {
            LauncherCommandLineArguments.PipeArg,
            LauncherCommandLineArguments.PassthroughArgsSeparator,
            LauncherCommandLineArguments.TargetArg,
            "custom.exe",
        };

        // Act
        var result = LauncherCommandLineParser.WithDefaultTarget(args, "default.exe");

        // Assert
        Assert.Same(args, result);
    }

    [Fact]
    public void WithDefaultTarget_InvalidArguments_ReturnsOriginalArguments()
    {
        // Arrange
        var args = new[] { LauncherCommandLineArguments.TimeoutSecArg, "invalid" };

        // Act
        var result = LauncherCommandLineParser.WithDefaultTarget(args, "default.exe");

        // Assert
        Assert.Same(args, result);
    }
}
