using System.Reflection.Metadata;
using Asv.Common;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using R3;

namespace Asv.Avalonia;

public class ForwardArgsToSelectedControlTask(
    ISoloRunFeature soloRunFeature,
    IFileAssociationService svc,
    INavigationService navigationService,
    IShellHost shellHost
) : IHostedService
{
    private ValueTask HandleEvent(AppArgs appArgs, CancellationToken ct)
    {
        var context = navigationService.SelectedControl.CurrentValue ?? shellHost.Shell;

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
