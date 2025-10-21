namespace Asv.Avalonia;

public sealed class NullDialogViewModel : DialogViewModelBase
{
    public const string DialogId = $"{BaseId}.null";

    public static NullDialogViewModel Instance { get; } = new();

    private NullDialogViewModel()
        : base(DialogId, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}
