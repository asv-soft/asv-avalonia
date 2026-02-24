using System.Diagnostics;
using System.Reflection;
using System.Text;
using Asv.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace Asv.Avalonia;

public class AppHost : AsyncDisposableWithCancel, IHost
{
    #region Static

    private static AppHost? _instance;

    public static AppHostBuilder CreateBuilder(string[] args)
    {
        if (_instance != null)
        {
            throw new InvalidOperationException(
                $"{nameof(AppHost)} already configured. Only one instance allowed."
            );
        }

        var builder = Host.CreateApplicationBuilder(
            new HostApplicationBuilderSettings
            {
#if DEBUG
                EnvironmentName = Environments.Development,
#else
                EnvironmentName = Environments.Production,
#endif
                Args = args,
            }
        );
        builder.Logging.ClearProviders();
        return new AppHostBuilder(builder);
    }

    public static AppHost Instance
    {
        get
        {
            if (_instance == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(AppHost)} not initialized. Please call {nameof(AppHost)}.{nameof(CreateBuilder)}().{nameof(AppHostBuilder.Build)}() through first."
                );
            }

            return _instance;
        }
    }

    public static void HandleApplicationCrash(Exception e)
    {
        var logger = _instance?.Services.GetService<ILoggerFactory>()?.CreateLogger<AppHost>();
        if (logger != null)
        {
            logger.LogCritical(e, $"Application crashed: {e.Message}");
        }

        var report = BuildExceptionReport(e);

        Console.WriteLine(report);

        const int maxCrashFiles = 10;
        var dir = AppContext.BaseDirectory;

        // Move files: N-1 -> N
        for (int i = maxCrashFiles - 1; i >= 0; i--)
        {
            var src = Path.Combine(dir, $"#crash_{i}.log");

            if (!File.Exists(src))
            {
                continue;
            }

            if (i == maxCrashFiles - 1)
            {
                File.Delete(src); // remove oldest
                continue;
            }

            var dst = Path.Combine(dir, $"#crash_{i + 1}.log");

            if (File.Exists(dst))
            {
                File.Delete(dst);
            }

            File.Move(src, dst);
        }

        var crashFileName = Path.Combine(dir, "#crash_0.log");
        File.WriteAllText(crashFileName, report, Encoding.UTF8);
    }

    private static string BuildExceptionReport(Exception ex)
    {
        var sb = new StringBuilder(32 * 1024);

        AppendHeader(sb);
        AppendEnvironment(sb);
        AppendExceptionRecursive(sb, ex, 0);

        return sb.ToString();
    }

    private static void AppendHeader(StringBuilder sb)
    {
        sb.AppendLine("====================================================");
        sb.AppendLine("                APPLICATION CRASH REPORT            ");
        sb.AppendLine("====================================================");
        sb.AppendLine($"Timestamp (UTC): {DateTime.UtcNow:O}");
        sb.AppendLine($"Timestamp (Local): {DateTime.Now:O}");
        sb.AppendLine($"Process: {Environment.ProcessId}");
        sb.AppendLine($"Thread: {Thread.CurrentThread.ManagedThreadId}");
        sb.AppendLine("====================================================");
        sb.AppendLine();
    }

    private static void AppendEnvironment(StringBuilder sb)
    {
        sb.AppendLine("ENVIRONMENT");
        sb.AppendLine("----------------------------------------------------");

        try
        {
            var entryAsm = Assembly.GetEntryAssembly();

            sb.AppendLine($"App: {entryAsm?.GetName().Name}");
            sb.AppendLine($"Version: {entryAsm?.GetName().Version}");
            sb.AppendLine($"BaseDirectory: {AppContext.BaseDirectory}");
            sb.AppendLine($"OS: {Environment.OSVersion}");
            sb.AppendLine($".NET: {Environment.Version}");
            sb.AppendLine($"64bit OS: {Environment.Is64BitOperatingSystem}");
            sb.AppendLine($"64bit Process: {Environment.Is64BitProcess}");
            sb.AppendLine($"Machine: {Environment.MachineName}");
            sb.AppendLine($"User: {Environment.UserName}");
            sb.AppendLine($"ProcessorCount: {Environment.ProcessorCount}");
            sb.AppendLine($"CurrentDirectory: {Environment.CurrentDirectory}");
            sb.AppendLine($"CommandLine: {Environment.CommandLine}");
        }
        catch
        {
            // ignore
        }

        sb.AppendLine();
    }

    private static void AppendExceptionRecursive(StringBuilder sb, Exception ex, int level)
    {
        var indent = new string(' ', level * 2);

        sb.AppendLine($"{indent}EXCEPTION LEVEL {level}");
        sb.AppendLine($"{indent}----------------------------------------------------");
        sb.AppendLine($"{indent}Type: {ex.GetType().FullName}");
        sb.AppendLine($"{indent}Message: {ex.Message}");
        sb.AppendLine($"{indent}HResult: 0x{ex.HResult:X8}");
        sb.AppendLine($"{indent}Source: {ex.Source}");

        if (ex.TargetSite != null)
        {
            sb.AppendLine(
                $"{indent}TargetSite: {ex.TargetSite.DeclaringType?.FullName}.{ex.TargetSite.Name}"
            );
        }

        if (ex.Data?.Count > 0)
        {
            sb.AppendLine($"{indent}Data:");
            foreach (var key in ex.Data.Keys)
            {
                sb.AppendLine($"{indent}  {key} = {ex.Data[key]}");
            }
        }

        if (!string.IsNullOrWhiteSpace(ex.StackTrace))
        {
            sb.AppendLine($"{indent}StackTrace:");
            AppendStackTrace(sb, ex.StackTrace, indent + "  ");
        }

        if (ex is AggregateException agg)
        {
            int i = 0;
            foreach (var inner in agg.InnerExceptions)
            {
                sb.AppendLine();
                sb.AppendLine($"{indent}Aggregate Inner #{i}");
                AppendExceptionRecursive(sb, inner, level + 1);
                i++;
            }
        }
        else if (ex.InnerException != null)
        {
            sb.AppendLine();
            sb.AppendLine($"{indent}InnerException:");
            AppendExceptionRecursive(sb, ex.InnerException, level + 1);
        }
    }

    private static void AppendStackTrace(StringBuilder sb, string stack, string indent)
    {
        using var reader = new StringReader(stack);

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            sb.AppendLine(indent + line.Trim());
        }
    }

    #endregion

    private readonly IHost _host;

    internal AppHost(IHost host)
    {
        _host = host;
        _instance = this;
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        return _host.StartAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        return _host.StopAsync(cancellationToken);
    }

    public IServiceProvider Services => _host.Services;
}
