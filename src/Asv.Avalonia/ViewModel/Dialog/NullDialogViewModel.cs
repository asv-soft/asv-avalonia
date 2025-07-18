namespace Asv.Avalonia;

public class NullDialogViewModel : DialogViewModelBase
{
    public const string DialogId = "null-dialog-vm";

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
