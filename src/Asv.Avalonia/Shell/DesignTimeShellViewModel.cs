using Microsoft.Extensions.Logging.Abstractions;
using R3;

namespace Asv.Avalonia;

public class DesignTimeShellViewModel : ShellViewModel
{
    public const string ShellId = "shell.design";
    public static DesignTimeShellViewModel Instance { get; } = new();

    public DesignTimeShellViewModel()
        : base(NullContainerHost.Instance, NullLoggerFactory.Instance, ShellId)
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

        var file = new FileMenu();
        MainMenu.Add(file);
        MainMenu.Add(new MenuItem("open","Open", file.Id.Id));
        MainMenu.Add(new EditMenu());
    }
}
