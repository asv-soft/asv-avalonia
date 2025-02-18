namespace Asv.Avalonia;

public class SettingsKeyMapItemViewModel : RoutableViewModel
{
    public SettingsKeyMapItemViewModel(ICommandInfo commandInfo, ICommandService svc)
        : base(commandInfo.Id)
    {
        Info = commandInfo;
    }

    public ICommandInfo Info { get; }

    public override ValueTask<IRoutable> Navigate(string id)
    {
        return ValueTask.FromResult<IRoutable>(this);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    public bool Fitler(string text)
    {
        return Info.Name.Contains(text, StringComparison.OrdinalIgnoreCase);
    }
}
