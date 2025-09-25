using System.Composition;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using R3;

namespace Asv.Avalonia;

[Export(typeof(IStartupTask))]
[Shared]
[method: ImportingConstructor]
public class ForwardArgsToFileServiceStartupTask(IFileAssociationService svc) : StartupTask
{
    public override void AppCtor()
    {
        if (!Design.IsDesignMode)
        {
            AppHost
                .Instance.Services.GetRequiredService<ISoloRunFeature>()
                .Args.Where(x => x.Tags.Count > 1)
                .SubscribeAwait(async (x, ct) => await svc.Open(x.Tags.Skip(1).First()));
        }
    }
}
