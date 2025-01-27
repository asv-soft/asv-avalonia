using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using R3;
using ZLogger;

namespace Asv.Avalonia;

public interface ILogService : ILoggerFactory
{
    ReadOnlyReactiveProperty<LogMessage?> OnMessage { get; }
    void SaveMessage(LogMessage logMessage);
    void DeleteLogFile();
    IEnumerable<LogMessage> LoadItemsFromLogFile();

    public void Fatal(string sender, string message, Exception? ex = null)
    {
        SaveMessage(new LogMessage(DateTime.Now, LogLevel.Critical, sender, message, ex?.Message));
    }

    public void Error(string sender, string message, Exception? ex = null)
    {
        SaveMessage(new LogMessage(DateTime.Now, LogLevel.Error, sender, message, ex?.Message));
    }

    public void Info(string sender, string message)
    {
        SaveMessage(new LogMessage(DateTime.Now, LogLevel.Information, sender, message, null));
    }

    public void Warning(string sender, string message)
    {
        SaveMessage(new LogMessage(DateTime.Now, LogLevel.Warning, sender, message, null));
    }

    public void Trace(string sender, string message)
    {
        SaveMessage(new LogMessage(DateTime.Now, LogLevel.Trace, sender, message, null));
    }

    public void Debug(string sender, string message)
    {
        SaveMessage(new LogMessage(DateTime.Now, LogLevel.Debug, sender, message, null));
    }
}

// public class LogService : ILogService TODO: fix какая-то срань с лог сервисом
// {
//     private readonly ILoggerFactory _factory;
//     private readonly ReactiveProperty<LogMessage?> _onMessage = new();
//     private readonly ILogger<LogService> _logger;
//     private readonly string _logsFolder;
//
//     public LogService(string logFolder, int rollingSizeKb, LogLevel minLevel, bool logToConsole)
//     {
//         _logsFolder = logFolder;
//         ArgumentNullException.ThrowIfNull(logFolder);
//         if (!Directory.Exists(logFolder))
//         {
//             Directory.CreateDirectory(logFolder);
//         }
//
//         _factory = LoggerFactory.Create(builder =>
//         {
//             builder.ClearProviders();
//             builder.SetMinimumLevel(minLevel);
//             builder.AddZLoggerRollingFile(options =>
//             {
//                 options.FilePathSelector = (dt, index) =>
//                     $"{logFolder}/{dt:yyyy-MM-dd}_{index}.logs";
//                 options.UseJsonFormatter();
//                 options.RollingSizeKB = rollingSizeKb;
//             });
//             if (logToConsole)
//             {
//                 builder.AddZLoggerConsole(options =>
//                 {
//                     options.IncludeScopes = true;
//                     options.OutputEncodingToUtf8 = false;
//                     options.UsePlainTextFormatter(formatter =>
//                     {
//                         formatter.SetPrefixFormatter(
//                             $"{0:HH:mm:ss.fff} | ={1:short}= | {2, -40} ",
//                             (in MessageTemplate template, in LogInfo info) =>
//                                 template.Format(info.Timestamp, info.LogLevel, info.Category)
//                         );
//
//                         // formatter.SetExceptionFormatter((writer, ex) => Utf8StringInterpolation.Utf8String.Format(writer, $"{ex.Message}"));
//                     });
//                 });
//             }
//         });
//         _logger = _factory.CreateLogger<LogService>();
//     }
//
//     public void SaveMessage(LogMessage logMessage)
//     {
//         _onMessage.OnNext(logMessage);
//         switch (logMessage.LogLevel)
//         {
//             case LogLevel.Trace:
//                 _logger.LogTrace(logMessage.Message);
//                 break;
//             case LogLevel.Debug:
//                 _logger.LogDebug(logMessage.Message);
//                 break;
//             case LogLevel.Information:
//                 _logger.LogInformation(logMessage.Message);
//                 break;
//             case LogLevel.Warning:
//                 _logger.LogWarning(logMessage.Message);
//                 break;
//             case LogLevel.Error:
//                 _logger.LogError(logMessage.Message);
//                 break;
//             case LogLevel.Critical:
//                 _logger.LogCritical(logMessage.Message);
//                 break;
//             case LogLevel.None:
//             default:
//                 throw new ArgumentOutOfRangeException();
//         }
//     }
//
//     public void DeleteLogFile()
//     {
//         foreach (var logFilePath in Directory.EnumerateFiles(_logsFolder, "*.logs"))
//         {
//             File.Delete(logFilePath);
//         }
//     }
//
//     public IEnumerable<LogMessage> LoadItemsFromLogFile()
//     {
//         foreach (var logFilePath in Directory.EnumerateFiles(_logsFolder, "*.logs"))
//         {
//             using var fs = new FileStream(
//                 logFilePath,
//                 FileMode.Open,
//                 FileAccess.Read,
//                 FileShare.ReadWrite
//             );
//             var rdr = new JsonTextReader(new StreamReader(fs)) { SupportMultipleContent = true };
//             var serializer = new JsonSerializer();
//
//             while (rdr.Read())
//             {
//                 if (rdr.TokenType == JsonToken.StartObject)
//                 {
//                     var item = serializer.Deserialize<LogMessage>(rdr);
//                     if (item != null)
//                     {
//                         yield return item;
//                     }
//                 }
//             }
//         }
//     }
//
//     public ILogger CreateLogger(string categoryName)
//     {
//         return _factory.CreateLogger(categoryName);
//     }
//
//     public void AddProvider(ILoggerProvider provider)
//     {
//         _factory.AddProvider(provider);
//     }
//
//     public ReadOnlyReactiveProperty<LogMessage?> OnMessage => _onMessage;
//
//     public void Dispose()
//     {
//         _onMessage.Dispose();
//         _factory.Dispose();
//     }
// }
