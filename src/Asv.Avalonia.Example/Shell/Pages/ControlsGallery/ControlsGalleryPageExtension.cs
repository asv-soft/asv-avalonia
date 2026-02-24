using System.Collections.Generic;
using Asv.Common;
using Microsoft.Extensions.DependencyInjection;
using R3;

namespace Asv.Avalonia.Example;

public class ControlsGalleryPageExtension : IExtensionFor<IControlsGalleryPage>
{
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
