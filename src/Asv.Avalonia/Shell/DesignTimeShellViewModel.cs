using R3;

namespace Asv.Avalonia;

public class DesignTimeShellViewModel : ShellViewModel
{
    public static DesignTimeShellViewModel Instance { get; } = new();

    public DesignTimeShellViewModel()
        : base(NullContainerHost.Instance)
    {
        int cnt = 0;
        var all = Enum.GetValues<ShellErrorState>().Length;
        Observable
            .Timer(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3))
            .Subscribe(_ =>
            {
                cnt++;
                ErrorState.Value = Enum.GetValues<ShellErrorState>()[cnt % all];
            });
        InternalPages.Add(new SettingsPageViewModel());
        InternalPages.Add(new HomePageViewModel());
    }

    public override ValueTask<IRoutable> Navigate(string id)
    {
        return ValueTask.FromResult<IRoutable>(this);
    }
}
