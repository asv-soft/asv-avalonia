using System.Collections.Immutable;
using System.Composition;
using Asv.Cfg;
using Asv.Common;
using Avalonia.Controls;
using Avalonia.Input;
using Microsoft.Extensions.Logging;
using R3;
using ZLogger;
using Disposable = R3.Disposable;

namespace Asv.Avalonia;

public class CommandServiceConfig
{
    public Dictionary<string, string?> HotKeys { get; set; } = new();
}

[Export(typeof(ICommandService))]
[Shared]
public class CommandService : AsyncDisposableOnce, ICommandService
{
    private readonly INavigationService _nav;
    private readonly IConfiguration _cfg;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ImmutableDictionary<string, IAsyncCommand> _commands;
    private ImmutableDictionary<string, KeyGesture> _commandsVsGesture;
    private ImmutableDictionary<KeyGesture, IAsyncCommand> _gestureVsCommand;
    private readonly ILogger<CommandService> _logger;
    private readonly IDisposable _disposeId;
    private readonly Subject<CommandEventArgs> _onCommand;

    [ImportingConstructor]
    public CommandService(
        INavigationService nav,
        IConfiguration cfg,
        [ImportMany] IEnumerable<IAsyncCommand> factories,
        ILoggerFactory loggerFactory
    )
    {
        var dispose = Disposable.CreateBuilder();

        _nav = nav;
        _cfg = cfg;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<CommandService>();
        _commands = factories.ToImmutableDictionary(x => x.Info.Id);
        ReloadHotKeys(out _commandsVsGesture, out _gestureVsCommand);

        // global event handlers for key events
        InputElement
            .KeyDownEvent.AddClassHandler<TopLevel>(OnKeyDownHandler, handledEventsToo: true)
            .AddTo(ref dispose);

        _onCommand = new Subject<CommandEventArgs>().AddTo(ref dispose);

        _disposeId = dispose.Build();
    }

    private async void OnKeyDownHandler(TopLevel source, KeyEventArgs keyEventArgs)
    {
        try
        {
            if (keyEventArgs.KeyModifiers == KeyModifiers.None)
            {
                // we don't want to handle key events without modifiers
                return;
            }

            if (_nav.SelectedControl.CurrentValue == null)
            {
                return;
            }

            var gesture = new KeyGesture(keyEventArgs.Key, keyEventArgs.KeyModifiers);
            var commandFactory = _gestureVsCommand.GetValueOrDefault(gesture);
            if (commandFactory == null)
            {
                return;
            }

            // TODO: check if we need to request params through the QuickPick dialog
            await InternalExecute(
                commandFactory,
                _nav.SelectedControl.CurrentValue,
                Persistable.Empty,
                CancellationToken.None
            );
        }
        catch (Exception e)
        {
            _logger.ZLogError(
                e,
                $"Error on key down [{keyEventArgs.KeyModifiers:F} + {keyEventArgs.Key:G}] handler : {e.Message}"
            );
        }
    }

    private async ValueTask InternalExecute(
        IAsyncCommand factory,
        IRoutable context,
        IPersistable param,
        CancellationToken cancel
    )
    {
        if (factory.CanExecute(context, param, out var target))
        {
            var backup = await factory.Execute(target, param, cancel);
            var snapShot = new CommandSnapshot(
                factory.Info.Id,
                context.GetPathToRoot(),
                param,
                backup
            );
            _onCommand.OnNext(new CommandEventArgs(context, factory, snapShot));
            _logger.ZLogTrace($"Execute command {backup}: context: {context}, param: {param}");
            return;
        }

        throw new CommandCannotExecuteException(factory.Info, context);
    }

    private void ReloadHotKeys(
        out ImmutableDictionary<string, KeyGesture> commandVsGesture,
        out ImmutableDictionary<KeyGesture, IAsyncCommand> gestureVsCommand,
        Action<IDictionary<string, string?>>? modifyConfig = null
    )
    {
        var keyVsCommandBuilder = ImmutableDictionary.CreateBuilder<KeyGesture, IAsyncCommand>();
        var commandVsKeyBuilder = ImmutableDictionary.CreateBuilder<string, KeyGesture>();
        
        // define hotkeys according to loaded CommandInfo
        foreach (var value in _commands.Values)
        {
            if (value.Info.DefaultHotKey == null)
            {
                continue;
            }

            keyVsCommandBuilder.Add(value.Info.DefaultHotKey, value);
            commandVsKeyBuilder.Add(value.Info.Id, value.Info.DefaultHotKey);
        }

        var config = _cfg.Get<CommandServiceConfig>();
        var configChanged = false;
        if (modifyConfig != null)
        {
            modifyConfig(config.HotKeys);
            configChanged = true;
        }
        
        // load hotkeys from config
        foreach (var (commandId, hotKey) in config.HotKeys)
        {
            if (string.IsNullOrWhiteSpace(hotKey))
            {
                if (keyVsCommandBuilder.Remove(commandVsKeyBuilder[commandId]))
                {
                    commandVsKeyBuilder.Remove(commandId);
                }

                continue;
            }

            KeyGesture keyGesture; 
            try
            {
                keyGesture = KeyGesture.Parse(hotKey); // ensure a value from config is valid
            }
            catch (Exception)
            {
                _logger.LogWarning(
                    "Invalid hot key {hotKey} for command {commandId} at config",
                    hotKey,
                    commandId
                );
                config.HotKeys.Remove(commandId);
                configChanged = true;
                continue;
            }

            if (_commands.TryGetValue(commandId, out var command) == false)
            {
                _logger.LogWarning(
                    "Command {commandId} not found => remove it from config",
                    commandId
                );
                config.HotKeys.Remove(commandId);
                configChanged = true;
                continue;
            }

            if (command.Info.DefaultHotKey == keyGesture) 
            {
                _logger.LogWarning(
                    "Hot key {hotKey} for command {commandId} is default => remove it from config",
                    hotKey,
                    commandId
                );
                config.HotKeys.Remove(commandId);
                configChanged = true;
                continue;
            }

            command.Info.CustomHotKey = keyGesture; // set the custom value manualy
            commandVsKeyBuilder[commandId] = keyGesture;
            keyVsCommandBuilder[keyGesture] = command;
        }

        gestureVsCommand = keyVsCommandBuilder.ToImmutable();
        commandVsGesture = commandVsKeyBuilder.ToImmutable();

        if (configChanged)
        {
            _cfg.Set(config);
        }
    }

    public IEnumerable<ICommandInfo> Commands => _commands.Values.Select(x => x.Info);

    public ICommandHistory CreateHistory(IRoutable? owner)
    {
        var history = new CommandHistory(owner, this, _loggerFactory);
        return history;
    }

    public ValueTask Execute(
        string commandId,
        IRoutable context,
        IPersistable param,
        CancellationToken cancel = default
    )
    {
        if (_commands.TryGetValue(commandId, out var factory))
        {
            return InternalExecute(factory, context, param, cancel);
        }

        return ValueTask.FromException(new CommandNotFoundException(commandId));
    }

    public void SetHotKey(string commandId, KeyGesture hotKey)
    {
        ArgumentNullException.ThrowIfNull(hotKey);
        if (_commands.ContainsKey(commandId) == false)
        {
            throw new CommandNotFoundException(commandId);
        }

        ReloadHotKeys(
            out _commandsVsGesture,
            out _gestureVsCommand,
            config => config[commandId] = hotKey?.ToString()
        );
    }

    public KeyGesture? GetHostKey(string commandId)
    {
        if (_commandsVsGesture.TryGetValue(commandId, out var gesture))
        {
            return gesture;
        }

        throw new CommandNotFoundException(commandId);
    }

    public Observable<CommandEventArgs> OnCommand => _onCommand;

    public async ValueTask Undo(CommandSnapshot command, CancellationToken cancel)
    {
        var context = await _nav.GoTo(command.ContextPath);
        if (_commands.TryGetValue(command.CommandId, out var factory) && command.OldValue != null)
        {
            await factory.Execute(context, command.OldValue, cancel);
            _logger.ZLogTrace(
                $"Undo command {factory.Info.Id}: context: {context}, param: {command.NewValue}"
            );
        }
    }

    public async ValueTask Redo(CommandSnapshot command, CancellationToken cancel = default)
    {
        var context = await _nav.GoTo(command.ContextPath);
        if (_commands.TryGetValue(command.CommandId, out var factory))
        {
            await factory.Execute(context, command.NewValue, cancel);
            _logger.ZLogTrace(
                $"Redo command {factory.Info.Id}: context: {context}, param: {command.NewValue}"
            );
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _disposeId.Dispose();
        }

        base.Dispose(disposing);
    }

    public IExportInfo Source => SystemModule.Instance;
}
