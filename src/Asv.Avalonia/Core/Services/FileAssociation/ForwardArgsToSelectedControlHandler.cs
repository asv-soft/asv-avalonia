using Asv.Modeling;
using Avalonia.Controls;
using Microsoft.Extensions.Hosting;
using R3;

namespace Asv.Avalonia;

public class ForwardArgsToSelectedControlHandler(
    ISoloRunFeature soloRunFeature,
    IFileAssociationService svc,
    
    IShellHost shellHost
) : IHostedService
{
    private ValueTask HandleEvent(IAppArgs appArgs, CancellationToken ct)
    {
        var context = shellHost.Shell?.Navigation.SelectedControl.CurrentValue;

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (context == null)
        {
            return default;
        }
        return context.Rise(new DesktopPushArgsEvent(context, appArgs), ct);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!Design.IsDesignMode)
        {
            soloRunFeature.Args.SubscribeAwait(HandleEvent);
        }
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
