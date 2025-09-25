using Asv.Common;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Asv.Avalonia;

public class DialogItemImageViewModel(ILoggerFactory loggerFactory)
    : DialogViewModelBase(DialogId, loggerFactory)
{
    public const string DialogId = $"{BaseId}.item.image";

    public DialogItemImageViewModel()
        : this(NullLoggerFactory.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
        if (Design.IsDesignMode)
        {
            Image = new Bitmap(Stream.Null).DisposeItWith(Disposable);
        }
    }

    public required Bitmap Image { get; set; }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}
