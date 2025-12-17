using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

/// <summary>
/// Represents a base view model that supports disposable resources and cancellation handling.
/// This class ensures proper cleanup of resources when the view model is disposed.
/// </summary>
public class DisposableViewModel(NavigationId id, ILoggerFactory loggerFactory)
    : ViewModelBase(id: id, loggerFactory)
{
    private volatile CancellationTokenSource? _cancel;
    private DisposableBag? _dispose;
    private readonly Lock _sync = new();

    /// <summary>
    /// Gets a cancellation token that is linked to the disposal state of the view model.
    /// If the view model is disposed, the token is set to <see cref="CancellationToken.None"/>.
    /// </summary>
    protected CancellationToken DisposeCancel
    {
        get
        {
            if (_cancel != null)
            {
                return IsDisposed ? CancellationToken.None : _cancel.Token;
            }

            using (_sync.EnterScope())
            {
                if (_cancel != null)
                {
                    return IsDisposed ? CancellationToken.None : _cancel.Token;
                }

                _cancel = new CancellationTokenSource();
                return _cancel.Token;
            }
        }
    }

    /// <summary>
    /// Gets a <see cref="DisposableBag"/> struct for managing disposable resources.
    /// This ensures that all registered disposables are cleaned up when the view model is disposed.
    /// </summary>
    protected DisposableBag Disposable
    {
        get
        {
            if (_dispose is not null)
            {
                return _dispose.Value;
            }

            using (_sync.EnterScope())
            {
                if (_dispose is not null)
                {
                    return _dispose.Value;
                }

                var dispose = default(DisposableBag);
                _dispose = dispose;
                return dispose;
            }
        }
    }

    /// <summary>
    /// Releases unmanaged resources and cancels any pending operations when disposing.
    /// </summary>
    /// <param name="disposing">
    /// <c>true</c> if disposing managed resources; otherwise, <c>false</c>.
    /// </param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Cancel any pending tasks if possible
            if (_cancel?.Token.CanBeCanceled == true)
            {
                _cancel.Cancel(false);
            }

            // Dispose of cancellation token and composite disposable
            _cancel?.Dispose();
            _dispose?.Dispose();
            _cancel = null;
            _dispose = null;
        }
    }
}
