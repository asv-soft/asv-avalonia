using Asv.Common;
using Asv.IO;
using Asv.Modeling;
using Microsoft.Extensions.Logging;
using ObservableCollections;

namespace Asv.Avalonia;

public class SettingsAppearanceViewModel
    : ExtendableTreeSubpage<ISettingsAppearanceSubPage>,
        ISettingsAppearanceSubPage
{
    public const string PageId = "appearance";

    public SettingsAppearanceViewModel()
        : this(DesignTime.LoggerFactory, DesignTime.ExtensionService)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public SettingsAppearanceViewModel(ILoggerFactory loggerFactory, IExtensionService ext)
        : base(PageId, loggerFactory, ext)
    {
        Sections = [];
        Sections.SetRoutableParent(this).DisposeItWith(Disposable);
        Sections.DisposeRemovedItems().DisposeItWith(Disposable);

        Views = Sections.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);
    }

    public ObservableList<ISettingsAppearanceSection> Sections { get; }

    public NotifyCollectionChangedSynchronizedViewList<ISettingsAppearanceSection> Views { get; }

    public ValueTask Init(ISettingsPage context) => ValueTask.CompletedTask;

    protected override void AfterLoadExtensions()
    {
        // do nothing
    }
}
