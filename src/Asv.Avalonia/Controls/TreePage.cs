using R3;

namespace Asv.Avalonia;

public class TreePage : Page
{
    private readonly BindableReactiveProperty<string> _title = new();

    public TreePage(string id, ICommandService cmd)
        : base(id, cmd) { }

    public override IEnumerable<IRoutable> Children { get; } = [];

    public override IReadOnlyBindableReactiveProperty<string> Title => _title;
}
