namespace Asv.Avalonia;

public class GroupTreePageItemViewModel : RoutableViewModel
{
    public GroupTreePageItemViewModel(string id)
        : base(id) { }

    public override ValueTask<IRoutable> Navigate(string id)
    {
        return ValueTask.FromResult<IRoutable>(this);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}
