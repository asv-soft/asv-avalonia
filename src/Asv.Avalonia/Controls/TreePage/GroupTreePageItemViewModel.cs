namespace Asv.Avalonia;

public class GroupTreePageItemViewModel : RoutableViewModel
{
    public GroupTreePageItemViewModel(string id)
        : base(id)
    {
        
    }

    public override ValueTask<IRoutable> NavigateTo(string id)
    {
        return ValueTask.FromResult<IRoutable>(this);
    }
}