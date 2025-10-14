namespace Asv.Avalonia;

public sealed class NullDialogViewModel : DialogViewModelBase
{
    public const string DialogId = $"{BaseId}.null";

    public static NullDialogViewModel Instance { get; } = new();

    private NullDialogViewModel()
        : base(DialogId, NullLayoutService.Instance, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}
