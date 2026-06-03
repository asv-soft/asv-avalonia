using Xunit;

namespace Asv.Avalonia.Test;

public class ExceptionReportTest
{
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

    private static string CreateTempDirectory()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"asv-avalonia-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        return dir;
    }
}
