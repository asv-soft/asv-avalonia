using System;
using System.Composition;
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Example;

[ExportExtensionFor<IHomePage>]
[method: ImportingConstructor]
public class HomePageControlsGalleryPageExtension(
    ILayoutService layoutService,
    ILoggerFactory loggerFactory
) : AsyncDisposableOnce, IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        context.Tools.Add(
            OpenControlsGalleryPageCommand
                .StaticInfo.CreateAction(
                    layoutService,
                    loggerFactory,
                    RS.OpenControlsGalleryPageCommand_Action_Title,
                    RS.OpenControlsGalleryPageCommand_Action_Description
                )
                .DisposeItWith(contextDispose)
        );
    }
}
