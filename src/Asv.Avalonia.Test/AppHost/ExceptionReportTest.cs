using Xunit;

namespace Asv.Avalonia.Test;

public class ExceptionReportTest
{
    public static bool IsWindows => OperatingSystem.IsWindows();

    [Fact]
    public void WriteToFile_DefaultPrefix_CreatesExpectedCrashFileName()
    {
        // Arrange
        var dir = CreateTempDirectory();

        try
        {
            // Act
            ExceptionReport.WriteToFile(dir, "test report");

            // Assert
            Assert.True(File.Exists(Path.Combine(dir, "#crash_0.log")));
            Assert.False(File.Exists(Path.Combine(dir, "##crash__0.log")));
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void WriteToFile_DefaultPrefix_ShiftsExistingCrashFiles()
    {
        // Arrange
        var dir = CreateTempDirectory();

        try
        {
            ExceptionReport.WriteToFile(dir, "old report");

            // Act
            ExceptionReport.WriteToFile(dir, "new report");

            // Assert
            Assert.Equal("new report", File.ReadAllText(Path.Combine(dir, "#crash_0.log")));
            Assert.Equal("old report", File.ReadAllText(Path.Combine(dir, "#crash_1.log")));
            Assert.False(File.Exists(Path.Combine(dir, "#crash__1.log")));
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public async Task WriteToFile_DefaultPrefix_AllowsConcurrentWrites()
    {
        // Arrange
        var dir = CreateTempDirectory();

        try
        {
            var tasks = Enumerable
                .Range(0, 50)
                .Select(index =>
                    Task.Run(() => ExceptionReport.WriteToFile(dir, $"report {index}"))
                );

            // Act
            await Task.WhenAll(tasks);

            // Assert
            Assert.True(File.Exists(Path.Combine(dir, "#crash_0.log")));
            Assert.InRange(Directory.EnumerateFiles(dir, "#crash_*.log").Count(), 1, 10);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void WriteToFile_DefaultPrefix_PreservesReportWhenCurrentCrashFileIsLocked()
    {
        // Arrange
        var dir = CreateTempDirectory();
        var lockedCrashFile = Path.Combine(dir, "#crash_0.log");

        try
        {
            using var lockStream = new FileStream(
                lockedCrashFile,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.None
            );

            // Act
            ExceptionReport.WriteToFile(dir, "new report");

            // Assert
            Assert.Contains(
                Directory.EnumerateFiles(dir, "#crash_*.log"),
                file => TryReadAllText(file) == "new report"
            );
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact(
        Skip = "FileShare.None lock behavior is Windows-specific.",
        SkipUnless = nameof(IsWindows)
    )]
    public void WriteToFile_DefaultPrefix_FallsBackWhenCurrentCrashFileIsLocked()
    {
        // Arrange
        var dir = CreateTempDirectory();
        var lockedCrashFile = Path.Combine(dir, "#crash_0.log");

        try
        {
            using var lockStream = new FileStream(
                lockedCrashFile,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.None
            );

            // Act
            ExceptionReport.WriteToFile(dir, "new report");

            // Assert
            var fallbackFiles = Directory
                .EnumerateFiles(dir, "#crash_*.log")
                .Where(file =>
                    !string.Equals(file, lockedCrashFile, StringComparison.OrdinalIgnoreCase)
                )
                .ToArray();
            Assert.Contains(fallbackFiles, file => TryReadAllText(file) == "new report");
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    private static string CreateTempDirectory()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"asv-avalonia-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static string? TryReadAllText(string path)
    {
        try
        {
            return File.ReadAllText(path);
        }
        catch (IOException)
        {
            return null;
        }
    }
}
