using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.GeoMap;

public class SettingsAppearanceExtension(IMapService mapService, ILoggerFactory loggerFactory)
    : IExtensionFor<ISettingsAppearanceSubPage>
{
    public void Extend(ISettingsAppearanceSubPage context, CompositeDisposable contextDispose)
    {
        context.Sections.Add(
            new GeoMapAppearanceSettingsSectionViewModel(mapService, loggerFactory).DisposeItWith(
                contextDispose
            )
        );
    }
}
