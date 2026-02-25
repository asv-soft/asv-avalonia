using System.Reflection;
using System.Text;

namespace Asv.Avalonia;

public static class ExceptionReport
{
    public static void WriteToFile(
        string dir,
        Exception ex,
        out string reportContent,
        int maxCrashFiles = 10,
        string filePrefix = "#crash_",
        string ext = "log"
    )
    {
        var crashFileName = ShiftCrashFileHistory(dir, maxCrashFiles, filePrefix, ext);
        reportContent = Build(ex);
        File.WriteAllText(crashFileName, reportContent, Encoding.UTF8);
    }

    public static void WriteToFile(
        string dir,
        string reportContent,
        int maxCrashFiles = 10,
        string filePrefix = "#crash_",
        string ext = "log"
    )
    {
        var crashFileName = ShiftCrashFileHistory(dir, maxCrashFiles, filePrefix, ext);
        File.WriteAllText(crashFileName, reportContent, Encoding.UTF8);
    }

    private static string ShiftCrashFileHistory(
        string dir,
        int maxCrashFiles,
        string filePrefix,
        string ext
    )
    {
        // Move files: N-1 -> N
        for (var i = maxCrashFiles - 1; i >= 0; i--)
        {
            var src = Path.Combine(dir, $"{filePrefix}{i}.{ext}");

            if (!File.Exists(src))
            {
                continue;
            }

            if (i == maxCrashFiles - 1)
            {
                File.Delete(src); // remove oldest
                continue;
            }

            var dst = Path.Combine(dir, $"{filePrefix}_{i + 1}.{ext}");

            if (File.Exists(dst))
            {
                File.Delete(dst);
            }

            File.Move(src, dst);
        }

        return Path.Combine(dir, $"#{filePrefix}_0.{ext}");
    }

    public static string Build(Exception ex)
    {
        var sb = new StringBuilder(32 * 1024);
        Build(ex, sb);
        return sb.ToString();
    }

    public static void Build(Exception ex, Action<string> appendLine)
    {
        AppendHeader(appendLine);
        AppendEnvironment(appendLine);
        AppendExceptionRecursive(appendLine, ex, 0);
    }

    public static void Build(Exception ex, StreamWriter wrt)
    {
        AppendHeader(wrt.WriteLine);
        AppendEnvironment(wrt.WriteLine);
        AppendExceptionRecursive(wrt.WriteLine, ex, 0);
    }

    public static void Build(Exception ex, StringBuilder sb)
    {
        AppendHeader(str => sb.AppendLine(str));
        AppendEnvironment(str => sb.AppendLine(str));
        AppendExceptionRecursive(str => sb.AppendLine(str), ex, 0);
    }

    private static void AppendHeader(Action<string> appendLine)
    {
        appendLine("====================================================");
        appendLine("                APPLICATION CRASH REPORT            ");
        appendLine("====================================================");
        appendLine($"Timestamp (UTC): {DateTime.UtcNow:O}");
        appendLine($"Timestamp (Local): {DateTime.Now:O}");
        appendLine($"Process: {Environment.ProcessId}");
        appendLine($"Thread: {Thread.CurrentThread.ManagedThreadId}");
        appendLine("====================================================");
        appendLine(string.Empty);
    }

    private static void AppendEnvironment(Action<string> appendLine)
    {
        appendLine("ENVIRONMENT");
        appendLine("----------------------------------------------------");

        try
        {
            var entryAsm = Assembly.GetEntryAssembly();

            appendLine($"App: {entryAsm?.GetName().Name}");
            appendLine($"Version: {entryAsm?.GetName().Version}");
            appendLine($"BaseDirectory: {AppContext.BaseDirectory}");
            appendLine($"OS: {Environment.OSVersion}");
            appendLine($".NET: {Environment.Version}");
            appendLine($"64bit OS: {Environment.Is64BitOperatingSystem}");
            appendLine($"64bit Process: {Environment.Is64BitProcess}");
            appendLine($"Machine: {Environment.MachineName}");
            appendLine($"User: {Environment.UserName}");
            appendLine($"ProcessorCount: {Environment.ProcessorCount}");
            appendLine($"CurrentDirectory: {Environment.CurrentDirectory}");
            appendLine($"CommandLine: {Environment.CommandLine}");
        }
        catch
        {
            // ignore
        }

        appendLine(string.Empty);
    }

    private static void AppendExceptionRecursive(Action<string> appendLine, Exception ex, int level)
    {
        var indent = new string(' ', level * 2);

        appendLine($"{indent}EXCEPTION LEVEL {level}");
        appendLine($"{indent}----------------------------------------------------");
        appendLine($"{indent}Type: {ex.GetType().FullName}");
        appendLine($"{indent}Message: {ex.Message}");
        appendLine($"{indent}HResult: 0x{ex.HResult:X8}");
        appendLine($"{indent}Source: {ex.Source}");

        if (ex.TargetSite != null)
        {
            appendLine(
                $"{indent}TargetSite: {ex.TargetSite.DeclaringType?.FullName}.{ex.TargetSite.Name}"
            );
        }

        if (ex.Data?.Count > 0)
        {
            appendLine($"{indent}Data:");
            foreach (var key in ex.Data.Keys)
            {
                appendLine($"{indent}  {key} = {ex.Data[key]}");
            }
        }

        if (!string.IsNullOrWhiteSpace(ex.StackTrace))
        {
            appendLine($"{indent}StackTrace:");
            AppendStackTrace(appendLine, ex.StackTrace, indent + "  ");
        }

        if (ex is AggregateException agg)
        {
            int i = 0;
            foreach (var inner in agg.InnerExceptions)
            {
                appendLine(string.Empty);
                appendLine($"{indent}Aggregate Inner #{i}");
                AppendExceptionRecursive(appendLine, inner, level + 1);
                i++;
            }
        }
        else if (ex.InnerException != null)
        {
            appendLine(string.Empty);
            appendLine($"{indent}InnerException:");
            AppendExceptionRecursive(appendLine, ex.InnerException, level + 1);
        }
    }

    private static void AppendStackTrace(Action<string> appendLine, string stack, string indent)
    {
        using var reader = new StringReader(stack);

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            appendLine(indent + line.Trim());
        }
    }
}
