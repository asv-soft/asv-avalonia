using System.Collections.Generic;
using Asv.Common;
using Asv.Modeling;
using Microsoft.Extensions.DependencyInjection;
using R3;

namespace Asv.Avalonia.Example;

public class ControlsGalleryPageExtension : IExtensionFor<IControlsGalleryPage>
{
    public const string StaticId = "ext.controls-gallery.subpages";

    string ISupportId<string>.Id => StaticId;

    private readonly IEnumerable<ITreePage> _subPagesMenu;

    public ControlsGalleryPageExtension(
        [FromKeyedServices(Contract)] IEnumerable<ITreePage> subPagesMenu
    )
    {
        _subPagesMenu = subPagesMenu;
    }

    public const string Contract = "controls-gallery-subpage";

    public void Extend(IControlsGalleryPage context, CompositeDisposable contextDispose)
    {
        foreach (var treePage in _subPagesMenu)
        {
            context.Nodes.Add(treePage);
            treePage.DisposeItWith(contextDispose);
        }
    }
}
