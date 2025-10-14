using System.Reactive.Linq;
using Material.Icons;
using Microsoft.Extensions.Logging.Abstractions;

namespace Asv.Avalonia;

public class DesignTimeShellViewModel : ShellViewModel
{
    public const string ShellId = "shell.design";
    public static DesignTimeShellViewModel Instance { get; } = new();

    public DesignTimeShellViewModel()
        : base(
            NullContainerHost.Instance,
            NullLayoutService.Instance,
            NullLoggerFactory.Instance,
            DesignTime.Configuration,
            ShellId
        )
    {
        int cnt = 0;
        ErrorState = ShellErrorState.Error;
        var all = Enum.GetValues<ShellErrorState>().Length;
        TimeProvider.System.CreateTimer(
            _ =>
            {
                cnt++;
                ErrorState = Enum.GetValues<ShellErrorState>()[cnt % all];
#pragma warning disable SA1117
            },
            null,
            TimeSpan.FromSeconds(3),
            TimeSpan.FromSeconds(3)
        );
#pragma warning restore SA1117

        InternalPages.Add(new SettingsPageViewModel());
        InternalPages.Add(new HomePageViewModel());

        var file = new OpenMenu(
            NullLayoutService.Instance,
            DesignTime.LoggerFactory,
            DesignTime.CommandService
        );
        MainMenu.Add(file);

        MainMenu.Add(
            new MenuItem(
                "open",
                "Open",
                NullLayoutService.Instance,
                DesignTime.LoggerFactory,
                file.Id.Id
            )
        );
        MainMenu.Add(new EditMenu(NullLayoutService.Instance, DesignTime.LoggerFactory));

        var addLeft = true;
        Observable
            .Timer(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(5))
            .Subscribe(x =>
            {
                if (addLeft)
                {
                    LeftMenu.Add(
                        new MenuItem(
                            $"open{x}",
                            "Open",
                            NullLayoutService.Instance,
                            DesignTime.LoggerFactory
                        )
                        {
                            Icon = DesignTime.RandomImage,
                            Command = DesignTime.EmptyCommand,
                        }
                    );
                    LeftMenu.Add(
                        new MenuItem(
                            $"open{x}_{x}",
                            "Open",
                            NullLayoutService.Instance,
                            DesignTime.LoggerFactory,
                            $"open{x}"
                        )
                        {
                            Icon = DesignTime.RandomImage,
                            Command = DesignTime.EmptyCommand,
                        }
                    );
                    LeftMenu.Add(
                        new MenuItem(
                            $"open{x}_{x}_2",
                            "Open",
                            NullLayoutService.Instance,
                            DesignTime.LoggerFactory,
                            $"open{x}"
                        )
                        {
                            Icon = DesignTime.RandomImage,
                            Command = DesignTime.EmptyCommand,
                        }
                    );
                    if (LeftMenu.Count > 10)
                    {
                        addLeft = false;
                    }
                }
                else
                {
                    LeftMenu.RemoveAt(0);
                    if (LeftMenu.Count <= 0)
                    {
                        addLeft = true;
                    }
                }
            });
        var addRight = true;
        Observable
            .Timer(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5))
            .Subscribe(x =>
            {
                if (addRight)
                {
                    RightMenu.Add(
                        new MenuItem(
                            $"open{x}",
                            "Open",
                            NullLayoutService.Instance,
                            DesignTime.LoggerFactory
                        )
                        {
                            Icon = DesignTime.RandomImage,
                            Command = DesignTime.EmptyCommand,
                        }
                    );
                    if (RightMenu.Count > 10)
                    {
                        addRight = false;
                    }
                }
                else
                {
                    RightMenu.RemoveAt(0);
                    if (RightMenu.Count <= 0)
                    {
                        addRight = true;
                    }
                }
            });
    }

    public override INavigationService Navigation => DesignTime.Navigation;
}
