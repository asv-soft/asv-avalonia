using Asv.Common;
using Asv.Modeling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using R3;

namespace Asv.Avalonia.IO;

public static class PacketInspectorRegistrations
{
    extension(PagesRegistrations.Builder builder)
    {
        public PagesRegistrations.Builder RegisterPacketInspector()
        {
            builder.AppBuilder.Services.AddOptions<PacketInspectorOptions>();
            builder.AppBuilder.Services.TryAddSingleton(TimeProvider.System);
            builder.AppBuilder.Services.TryAddSingleton<PacketCaptureService>();
            builder.AppBuilder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPacketFieldSource, DefaultPacketFieldSource>());
            builder.AppBuilder.Pages.Register<PacketInspectorPageViewModel, PacketInspectorPageView>(PacketInspectorPageViewModel.PageId);
            builder.AppBuilder.Extensions.Register<IHomePage, HomePagePacketInspectorExtension>();
            return builder.RegisterFileInspector();
        }
    }
}

public sealed class HomePagePacketInspectorExtension : IExtensionFor<IHomePage>
{
    public string Id => "io.packet-inspector.home";

    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        var action = new ActionViewModel("open-packet-inspector")
        {
            Header = RS.PacketInspector_Title,
            Description = RS.PacketInspector_Description,
            Icon = PacketInspectorPageViewModel.PageIcon,
            Command = new ReactiveCommand(_ => context.GoTo(new NavPath(new NavId(PacketInspectorPageViewModel.PageId))))
                .DisposeItWith(contextDispose),
        }.DisposeItWith(contextDispose);
        context.Tools.Add(action);
        R3.Disposable.Create(() => context.Tools.Remove(action)).DisposeItWith(contextDispose);
    }
}
