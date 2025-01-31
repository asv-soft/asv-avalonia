using R3;

namespace Asv.Avalonia;

public class DesignTimeShellViewModel : ShellViewModel
{
    public DesignTimeShellViewModel()
        : base(NullContainerHost.Instance)
    {
        int cnt = 0;
        var all = Enum.GetValues<ShellStatus>().Length;
        Observable.Timer(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3)).Subscribe(_ =>
        {
            cnt++;
            Status.Value = Enum.GetValues<ShellStatus>()[cnt % all];
        });
    }

    public override ValueTask<IRoutable> NavigateTo(string id)
    {
        return ValueTask.FromResult<IRoutable>(this);
    }
}