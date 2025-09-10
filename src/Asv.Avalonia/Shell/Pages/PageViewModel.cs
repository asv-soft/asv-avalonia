using System.Windows.Input;
using Asv.Cfg;
using Asv.Common;
using DotNext.Threading.Tasks;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;
using ZLogger;

namespace Asv.Avalonia;

public abstract class PageViewModel<TContext> : ExtendableViewModel<TContext>, IPage
    where TContext : class, IPage
{
    protected PageViewModel(
        NavigationId id,
        ICommandService cmd,
        IConfiguration cfgService,
        ILoggerFactory loggerFactory
    )
        : base(id, loggerFactory)
    {
        History = cmd.CreateHistory(this);
        Icon = MaterialIconKind.Window;
        Title = id.ToString();
        HasChanges = new BindableReactiveProperty<bool>(false);
        TryClose = new BindableAsyncCommand(ClosePageCommand.Id, this);
        _sub1 = HasChanges.SubscribeAwait(
            async (hasChanges, ct) =>
            {
                if (hasChanges)
                {
                    await SafeChanges(ct);
                    HasChanges.Value = false;
                }
            }
        );
    }

    public async ValueTask TryCloseAsync(bool isForce)
    {
        Logger.ZLogTrace($"Try close page {Title}[{Id}]");
        try
        {
            if (!isForce)
            {
                var reasons = await this.RequestChildCloseApproval();
                if (reasons.Count != 0)
                {
                    return;
                }
            }

            await this.RequestClose();
        }
        catch (Exception e)
        {
            Logger.ZLogError(e, $"Error on close page {Title}[{Id}]: {e.Message}");
        }
    }

    public ValueTask TrySaveStateAsync(bool force)
    {
        throw new NotImplementedException();
    }

    public MaterialIconKind Icon
    {
        get;
        set => SetField(ref field, value);
    }

    public string Title
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommandHistory History { get; }
    public BindableReactiveProperty<bool> HasChanges { get; }
    public ICommand TryClose { get; }

    protected virtual ValueTask SafeChanges(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return ValueTask.CompletedTask;
    }

    #region Dispose

    private readonly IDisposable _sub1;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            History.Dispose();
            HasChanges.Dispose();
            _sub1.Dispose();
        }

        base.Dispose(disposing);
    }

    #endregion

    public abstract IExportInfo Source { get; }
}
