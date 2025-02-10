using System.Composition;
using Material.Icons;

namespace Asv.Avalonia;

[Export(typeof(ICommandFactory))]
[Shared]
public class ChangeLanguageCommandFactory : ICommandFactory
{
    private readonly ILocalizationService _svc;

    [ImportingConstructor]
    public ChangeLanguageCommandFactory(ILocalizationService svc)
    {
        ArgumentNullException.ThrowIfNull(svc);
        _svc = svc;
    }

    public ICommandInfo Info => ChangeLanguageCommand.StaticInfo;

    public IAsyncCommand Create()
    {
        return new ChangeLanguageCommand(_svc);
    }

    public bool CanExecute(IRoutable context, out IRoutable? target)
    {
        target = context;
        return true;
    }
}

public class ChangeLanguageCommand(ILocalizationService svc) : IUndoRedoCommand
{
    #region Static

    public const string Id = "language.change";
    internal static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.ChangeLanguageCommand_CommandInfo_Name,
        Description = RS.ChangeLanguageCommand_CommandInfo_Description,
        Icon = MaterialIconKind.Translate,
        DefaultHotKey = null,
        Order = 0,
        IsEditable = false,
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
            var oldValue = svc.CurrentLanguage.Value.Id;
            var language = svc.AvailableLanguages.FirstOrDefault(x => x.Id == memento.Value);
            if (language is not null)
            {
                svc.CurrentLanguage.Value = language;
            }

            _state = new PersistableChange<string>(oldValue, memento.Value);
        }
        else
        {
            // execute without parameter
            var oldValue = svc.CurrentLanguage.Value.Id;
            var temp = svc.AvailableLanguages.ToList();
            var index = temp.IndexOf(svc.CurrentLanguage.Value);
            index++;
            if (index >= temp.Count)
            {
                index = 0;
            }

            var newValue = temp[index].Id;
            svc.CurrentLanguage.Value = temp[index];
            _state = new PersistableChange<string>(oldValue, newValue);
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask Undo(IRoutable? context, CancellationToken cancel = default)
    {
        if (_state is null)
        {
            return ValueTask.CompletedTask;
        }

        var language = svc.AvailableLanguages.FirstOrDefault(x => x.Id == _state.OldValue);
        if (language is not null)
        {
            svc.CurrentLanguage.Value = language;
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask Redo(IRoutable context, CancellationToken cancel = default)
    {
        if (_state is null)
        {
            return ValueTask.CompletedTask;
        }

        var language = svc.AvailableLanguages.FirstOrDefault(x => x.Id == _state.NewValue);
        if (language is not null)
        {
            svc.CurrentLanguage.Value = language;
        }

        return ValueTask.CompletedTask;
    }
}
