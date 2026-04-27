using Asv.Modeling;

namespace Asv.Avalonia;

public abstract class SettingsSubPage(string typeId, ITreeSubPageContext<ISettingsPage> context)
    : TreeSubpage<ISettingsPage>(typeId, context),
        ISettingsSubPage
{
}
