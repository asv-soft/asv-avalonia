using System.Composition;
using System.Reflection.Metadata;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using R3;

namespace Asv.Avalonia;

[Export(typeof(IStartupTask))]
[Shared]
[method: ImportingConstructor]
public class ForwardArgsToSelectedControlTask(IFileAssociationService svc, INavigationService navigationService, IShellHost shellHost) : StartupTask
{
    public override void AppCtor()
    {
        if (!Design.IsDesignMode)
        {
            AppHost
                .Instance.Services.GetRequiredService<ISoloRunFeature>()
                .Args
                .SubscribeAwait(HandleEvent);
        }
    }

    private ValueTask HandleEvent(AppArgs appArgs, CancellationToken ct)
    {
        var context = navigationService.SelectedControl.CurrentValue ?? shellHost.Shell;
        
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (context == null)
        {
            return default;
        }
        return context.Rise(new DesktopPushArgsEvent(context, appArgs));
    }
}
