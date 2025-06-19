using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public sealed class DialogItemHotKeyCaptureViewModel : DialogViewModelBase
{
    public const string DialogId = "dialog.hotkey.capture";

    public DialogItemHotKeyCaptureViewModel()
        : this(DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
        HotKey = new BindableReactiveProperty<HotKeyInfo?>(HotKeyInfo.Parse("Ctrl+C"));
    }

    public DialogItemHotKeyCaptureViewModel(ILoggerFactory loggerFactory)
        : base(DialogId, loggerFactory)
    {
        HotKey = new BindableReactiveProperty<HotKeyInfo?>();
    }

    public BindableReactiveProperty<HotKeyInfo?> HotKey { get; set; }

    public override IEnumerable<IRoutable> GetRoutableChildren() => [];
}
