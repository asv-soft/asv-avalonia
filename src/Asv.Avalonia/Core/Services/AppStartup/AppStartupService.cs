using System.Collections.Immutable;
using System.Composition;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace Asv.Avalonia;

[Export(typeof(IAppStartupService))]
[Shared]
[method: ImportingConstructor]
public class AppStartupService(
    [ImportMany] IEnumerable<IStartupTask> tasks,
    ILoggerFactory loggerFactory
) : IAppStartupService
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<AppStartupService>();
    private readonly ImmutableArray<IStartupTask> _tasks = [.. tasks];

    public void AppCtor()
    {
        foreach (var task in _tasks)
        {
            try
            {
                _logger.ZLogTrace($"Startup task '{task.GetType().Name}'");
                task.AppCtor();
            }
            catch (Exception e)
            {
                _logger.ZLogError($"Startup task '{task.GetType().Name}' failed: {e.Message}", e);
            }
        }
    }

    public void OnFrameworkInitializationCompleted()
    {
        foreach (var task in _tasks)
        {
            try
            {
                _logger.ZLogTrace($"Startup task '{task.GetType().Name}'");
                task.OnFrameworkInitializationCompleted();
            }
            catch (Exception e)
            {
                _logger.ZLogError($"Startup task '{task.GetType().Name}' failed : {e.Message}", e);
            }
        }
    }

    public void Initialize()
    {
        foreach (var task in _tasks)
        {
            try
            {
                _logger.ZLogTrace($"Startup task '{task.GetType().Name}'");
                task.Initialize();
            }
            catch (Exception e)
            {
                _logger.ZLogError($"Startup task '{task.GetType().Name}' failed:{e.Message}", e);
            }
        }
    }
}
