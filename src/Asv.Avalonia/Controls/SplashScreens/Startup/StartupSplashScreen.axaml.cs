using Avalonia.Controls.Primitives;

namespace Asv.Avalonia;

public partial class StartupSplashScreen : TemplatedControl
{
    private CancellationTokenSource? _runCts;

    public async Task RunStartupAsync(
        Func<CancellationToken, Task> startupAction,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(startupAction);

        CancelStartup();
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _runCts = linkedCts;

        IsBusy = true;
        HasError = false;
        ErrorText = null;

        var startedAt = DateTime.UtcNow;
        try
        {
            await startupAction(linkedCts.Token);

            var elapsed = (int)(DateTime.UtcNow - startedAt).TotalMilliseconds;
            var delay = MinimumShowTimeMs - elapsed;
            if (delay > 0)
            {
                await Task.Delay(delay, linkedCts.Token);
            }
        }
        catch (OperationCanceledException) when (linkedCts.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorText = ex.Message;
            throw;
        }
        finally
        {
            if (ReferenceEquals(_runCts, linkedCts))
            {
                _runCts = null;
            }

            IsBusy = false;
            linkedCts.Dispose();
        }
    }

    public void CancelStartup()
    {
        _runCts?.Cancel();
    }
}
