using System.Collections.Immutable;
using System.Composition;
using Avalonia.Input;
using Material.Icons;

namespace Asv.Avalonia;

[Export(typeof(ICommandFactory))]
[Shared]
public class ChangeThemeCommandFactory : ICommandFactory
{
    private readonly IThemeService _svc;

    [ImportingConstructor]
    public ChangeThemeCommandFactory(IThemeService svc)
    {
        ArgumentNullException.ThrowIfNull(svc);
        _svc = svc;
    }

    public ICommandInfo Info => ChangeThemeCommand.StaticInfo;

    public IAsyncCommand Create()
    {
        return new ChangeThemeCommand(_svc);
    }

    public bool CanExecute(IRoutable context, out IRoutable? target)
    {
        target = context;
        return true;
    }
}

public class ChangeThemeCommand(IThemeService svc) : IUndoRedoCommand
{
    #region Static

    public const string Id = "theme.change";
    internal static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.ChangeThemeCommand_CommandInfo_Name,
        Description = RS.ChangeThemeCommand_CommandInfo_Description,
        Icon = MaterialIconKind.ThemeLightDark,
        DefaultHotKey = KeyGesture.Parse("Ctrl+T"),
        Order = 0,
        IsEditable = true,
    };

    #endregion

    private PersistableChange<string>? _state;

    public ICommandInfo Info => StaticInfo;

    public IPersistable Save()
    {
        return _state ?? throw new InvalidOperationException();
    }

    public void Restore(IPersistable state)
    {
        if (state is PersistableChange<string> memento)
        {
            _state = memento;
        }
    }

    public ValueTask Execute(
        IRoutable context,
        IPersistable? parameter = null,
        CancellationToken cancel = default
    )
    {
        if (parameter is Persistable<string> memento)
        {
            // execute with parameter
            var oldValue = svc.CurrentTheme.Value.Id;
            var theme = svc.Themes.FirstOrDefault(x => x.Id == memento.Value);
            if (theme != null)
            {
                svc.CurrentTheme.Value = theme;
            }

            _state = new PersistableChange<string>(oldValue, memento.Value);
        }
        else
        {
            // execute without parameter
            var oldValue = svc.CurrentTheme.Value.Id;
            var temp = svc.Themes.ToList();
            var index = temp.IndexOf(svc.CurrentTheme.Value);
            index++;
            if (index >= temp.Count)
            {
                index = 0;
            }

            var newValue = temp[index].Id;
            svc.CurrentTheme.Value = temp[index];
            _state = new PersistableChange<string>(oldValue, newValue);
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask Undo(IRoutable? context, CancellationToken cancel = default)
    {
        if (_state != null)
        {
            var theme = svc.Themes.FirstOrDefault(x => x.Id == _state.OldValue);
            if (theme != null)
            {
                svc.CurrentTheme.Value = theme;
            }
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask Redo(IRoutable context, CancellationToken cancel = default)
    {
        if (_state != null)
        {
            var theme = svc.Themes.FirstOrDefault(x => x.Id == _state.NewValue);
            if (theme != null)
            {
                svc.CurrentTheme.Value = theme;
            }
        }

        return ValueTask.CompletedTask;
    }
}
