using Avalonia.Controls;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Asv.Avalonia;

public class DialogItemTextViewModel(ILoggerFactory loggerFactory)
    : DialogViewModelBase(DialogId)
{
    public const string DialogId = $"{BaseId}.item.text";

    public DialogItemTextViewModel()
        : this(NullLoggerFactory.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
        Message = "Example";
    }

    public required string Message { get; set; }

    public override IEnumerable<IViewModel> GetChildren()
    {
        return [];
    }
}
