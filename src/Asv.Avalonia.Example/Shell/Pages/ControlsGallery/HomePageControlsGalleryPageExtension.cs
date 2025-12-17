using System;
using System.Composition;
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Example;

[ExportExtensionFor<IHomePage>]
[method: ImportingConstructor]
public class HomePageControlsGalleryPageExtension(ILoggerFactory loggerFactory)
    : AsyncDisposableOnce,
        IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, DisposableBag contextDispose)
    {
        context.Tools.Add(
            OpenControlsGalleryPageCommand
                .StaticInfo.CreateAction(
                    loggerFactory,
                    RS.OpenControlsGalleryPageCommand_Action_Title,
                    RS.OpenControlsGalleryPageCommand_Action_Description
                )
                .DisposeItWith(contextDispose)
        );
    }
}
