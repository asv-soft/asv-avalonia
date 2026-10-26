using Asv.Common;
using Asv.Modeling;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using R3;

namespace Asv.Avalonia.IO;

public static class FileInspectorRegistrations
{
    extension(PagesRegistrations.Builder builder)
    {
        public PagesRegistrations.Builder RegisterFileInspector()
        {
            builder.AppBuilder.Services.TryAddSingleton<PacketFileReader>();
            builder.AppBuilder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPacketFieldSource, DefaultPacketFieldSource>());
            builder.AppBuilder.Pages.Register<FileInspectorPageViewModel, FileInspectorPageView>(FileInspectorPageViewModel.PageId);
            builder.AppBuilder.Extensions.Register<IHomePage, HomePageFileInspectorExtension>();
            return builder;
        }
    }
}

public sealed class HomePageFileInspectorExtension : IExtensionFor<IHomePage>
{
    public string Id => "io.file-inspector.home";

    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        var action = new ActionViewModel("open-file-inspector")
        {
            Header = RS.FileInspector_Title,
            Description = RS.FileInspector_Description,
            Icon = MaterialIconKind.FileSearchOutline,
            IconColor = FileInspectorPageViewModel.PageIconColor,
            Command = new ReactiveCommand(_ => context.GoTo(new NavPath(new NavId(FileInspectorPageViewModel.PageId))))
                .DisposeItWith(contextDispose),
        }.DisposeItWith(contextDispose);
        context.Tools.Add(action);
        R3.Disposable.Create(() => context.Tools.Remove(action)).DisposeItWith(contextDispose);
    }
}
