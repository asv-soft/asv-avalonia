using Microsoft.Extensions.DependencyInjection;
using R3;

namespace Asv.Avalonia;

public sealed class DesignTimeShellViewModel : ShellViewModel
{
    public static DesignTimeShellViewModel Instance { get; } = new();

    public DesignTimeShellViewModel()
        : base(
            AppHost.Instance.Services,
            DesignTime.LoggerFactory,
            AppHost.Instance.Services.GetRequiredService<IAppPath>(),
            DesignTime.ThemeService,
            DesignTime.DialogService,
            DesignTime.ExtensionService
        )
    {
        DesignTime.ThrowIfNotDesignMode();
        StatusItems.Add(new NavigationStatusItemViewModel());

        int cnt = 0;
        Messages.ErrorState = ShellErrorState.Error;
        var all = Enum.GetValues<ShellErrorState>().Length;
        TimeProvider.System.CreateTimer(
            _ =>
            {
                cnt++;
                Messages.ErrorState = Enum.GetValues<ShellErrorState>()[cnt % all];
#pragma warning disable SA1117
            },
            null,
            TimeSpan.FromSeconds(3),
            TimeSpan.FromSeconds(3)
        );
#pragma warning restore SA1117

        InternalPages.Add(new SettingsPageViewModel());
        InternalPages.Add(new HomePageViewModel());

        var file = new MenuItem("open", RS.ShellView_Toolbar_Open)
        {
            Command = DesignTime.EmptyCommand,
        };
        MainMenu.Add(file);

        MainMenu.Add(new MenuItem("open-child", "Open", file.Id.TypeId));
        MainMenu.Add(new EditMenu(DesignTime.LoggerFactory));

        var addLeft = true;
        Observable
            .Timer(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(5))
            .Subscribe(x =>
            {
                if (addLeft)
                {
                    LeftMenu.Add(
                        new MenuItem($"open{x}", "Open")
                        {
                            Icon = DesignTime.RandomImage,
                            Command = DesignTime.EmptyCommand,
                        }
                    );
                    LeftMenu.Add(
                        new MenuItem($"open{x}_{x}", "Open", $"open{x}")
                        {
                            Icon = DesignTime.RandomImage,
                            Command = DesignTime.EmptyCommand,
                        }
                    );
                    LeftMenu.Add(
                        new MenuItem($"open{x}_{x}_2", "Open", $"open{x}")
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
                        new MenuItem($"open{x}", "Open")
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
