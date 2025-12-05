using R3;

namespace Asv.Avalonia;

public sealed class DesignTimeShellViewModel : ShellViewModel
{
    public const string ShellId = "shell.design";
    public static DesignTimeShellViewModel Instance { get; } = new();

    public DesignTimeShellViewModel()
        : base(
            ShellId,
            NullContainerHost.Instance,
            DesignTime.LoggerFactory,
            DesignTime.Configuration
        )
    {
        DesignTime.ThrowIfNotDesignMode();
        StatusItems.Add(new NavigationStatusItemViewModel());

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

        var file = new OpenMenu(DesignTime.LoggerFactory, DesignTime.CommandService);
        MainMenu.Add(file);

        MainMenu.Add(new MenuItem("open", "Open", DesignTime.LoggerFactory, file.Id.Id));
        MainMenu.Add(new EditMenu(DesignTime.LoggerFactory));

        var addLeft = true;
        Observable
            .Timer(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(5))
            .Subscribe(x =>
            {
                if (addLeft)
                {
                    LeftMenu.Add(
                        new MenuItem($"open{x}", "Open", DesignTime.LoggerFactory)
                        {
                            Icon = DesignTime.RandomImage,
                            Command = DesignTime.EmptyCommand,
                        }
                    );
                    LeftMenu.Add(
                        new MenuItem($"open{x}_{x}", "Open", DesignTime.LoggerFactory, $"open{x}")
                        {
                            Icon = DesignTime.RandomImage,
                            Command = DesignTime.EmptyCommand,
                        }
                    );
                    LeftMenu.Add(
                        new MenuItem($"open{x}_{x}_2", "Open", DesignTime.LoggerFactory, $"open{x}")
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
            .Timer(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(5))
            .Subscribe(x =>
            {
                if (addRight)
                {
                    RightMenu.Add(
                        new MenuItem($"open{x}", "Open", DesignTime.LoggerFactory)
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

        Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5))
            .ObserveOnUIThreadDispatcher()
            .Subscribe(x =>
            {
                ShowMessage(
                    new ShellMessage(
                        DesignTime.RandomShortName(10, 20),
                        DesignTime.RandomMessageText(5, 60),
                        DesignTime.RandomEnum<ShellErrorState>(),
                        DesignTime.RandomMessageText(50, 100),
                        DesignTime.RandomImage
                    )
                );
            });
    }
}
