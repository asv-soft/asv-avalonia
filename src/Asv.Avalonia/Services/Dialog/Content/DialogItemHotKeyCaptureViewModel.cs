using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public sealed class DialogItemHotKeyCaptureViewModel : DialogViewModelBase
{
    public const string DialogId = $"{BaseId}.hotkey.capture";

    public DialogItemHotKeyCaptureViewModel()
        : this(DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
        HotKey = new BindableReactiveProperty<HotKeyInfo?>(
            HotKeyInfo.Parse("Ctrl+C")
        ).DisposeItWith(Disposable);
    }

    public DialogItemHotKeyCaptureViewModel(ILoggerFactory loggerFactory)
        : base(DialogId, loggerFactory)
    {
        HotKey = new BindableReactiveProperty<HotKeyInfo?>().DisposeItWith(Disposable);
    }

    public BindableReactiveProperty<HotKeyInfo?> HotKey { get; }

    public override IEnumerable<IRoutable> GetRoutableChildren() => [];
}
