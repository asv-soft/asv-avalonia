using Asv.Avalonia.Launcher.Contracts;
using Avalonia;
using Avalonia.Controls;

namespace Asv.Avalonia.Example.Launcher;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
        }
        catch (Exception ex)
        {
            Environment.ExitCode = (int)LauncherExitCode.UnexpectedError;
            ReportFatalError(ex, args);
        }
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace();
    }

    private static void ReportFatalError(Exception ex, IReadOnlyList<string> args)
    {
        try
        {
            var logFile = Path.Combine(AppContext.BaseDirectory, "launcher-fatal.log");
            var content =
                $"[{DateTimeOffset.Now:O}] Launcher fatal startup error{Environment.NewLine}"
                + $"Args: {string.Join(' ', args)}{Environment.NewLine}{ex}";
            File.WriteAllText(logFile, content);
            Console.Error.WriteLine(content);
            Console.Error.WriteLine($"Saved to: {logFile}");
        }
        catch
        {
            // Ignore secondary failures during fatal error reporting.
        }
    }
}
