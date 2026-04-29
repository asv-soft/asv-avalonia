using Asv.Common;
using Asv.IO;
using Asv.Modeling;
using ObservableCollections;

namespace Asv.Avalonia;

public class SettingsAppearanceViewModel
    : ExtendableTreeSubpage<ISettingsPage, ISettingsAppearanceSubPage>,
        ISettingsAppearanceSubPage
{
    public const string PageId = "appearance";

    public SettingsAppearanceViewModel()
        : this(NullTreeSubPageContext<SettingsPageViewModel>.Instance, DesignTime.ExtensionService)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public SettingsAppearanceViewModel(ITreeSubPageContext<ISettingsPage> context, IExtensionService ext)
        : base(PageId, context, ext)
    {
        Sections = [];
        Sections.SetRoutableParent(this).DisposeItWith(Disposable);
        Sections.DisposeRemovedItems().DisposeItWith(Disposable);

        Views = Sections.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);
    }

    public ObservableList<ISettingsAppearanceSection> Sections { get; }

    public NotifyCollectionChangedSynchronizedViewList<ISettingsAppearanceSection> Views { get; }

    protected override void AfterLoadExtensions()
    {
        // do nothing
    }
}
