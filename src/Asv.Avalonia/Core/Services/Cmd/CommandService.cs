using System.Collections.Immutable;
using System.Diagnostics;
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

public class CommandService : AsyncDisposableOnce, ICommandService
{
    private readonly INavigationService _nav;
    private readonly IConfiguration _cfg;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ImmutableDictionary<string, IAsyncCommand> _commands;
    private ImmutableDictionary<string, HotKeyInfo?> _commandsVsGesture;
    private ImmutableDictionary<HotKeyInfo, ImmutableArray<IAsyncCommand>> _gestureVsCommand;
    private readonly ILogger<CommandService> _logger;
    private readonly IDisposable _disposeId;
    private readonly Subject<CommandSnapshot> _onCommand;
    private KeyGesture? _prevKeyGesture;
    private readonly IAppPath _path;
    private readonly Subject<HotKeyInfo> _hotKeySubject;
    private readonly ReactiveProperty<bool> _isHotKeyRecognitionEnabled;

    public CommandService(
        INavigationService nav,
        IConfiguration cfg,
        IAppPath path,
        IEnumerable<IAsyncCommand> factories,
        ILoggerFactory loggerFactory
    )
    {
        var dispose = Disposable.CreateBuilder();

        _nav = nav;
        _cfg = cfg;
        _path = path;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<CommandService>();
        _commands = factories.ToImmutableDictionary(x => x.Info.Id);
        ReloadHotKeys(out _commandsVsGesture, out _gestureVsCommand);

        // global event handlers for key events
        InputElement
            .KeyDownEvent.AddClassHandler<TopLevel>(OnKeyDownHandler, handledEventsToo: true)
            .AddTo(ref dispose);

        _onCommand = new Subject<CommandSnapshot>().AddTo(ref dispose);

        _hotKeySubject = new Subject<HotKeyInfo>().AddTo(ref dispose);
        _isHotKeyRecognitionEnabled = new ReactiveProperty<bool>(true).AddTo(ref dispose);

        _disposeId = dispose.Build();
    }

    private async void OnKeyDownHandler(TopLevel source, KeyEventArgs keyEventArgs)
    {
        try
        {
            if (keyEventArgs.Handled)
            {
                return;
            }

            if (keyEventArgs.Key == Key.None)
            {
                return;
            }

            if (_nav.SelectedControl.CurrentValue is null)
            {
                Debug.Assert(false, "SelectedControl should never be null");
                return;
            }

            HotKeyInfo keyInfo;

            if (keyEventArgs.KeyModifiers == KeyModifiers.None)
            {
                if (_prevKeyGesture != null)
                {
                    // we have a gesture, but no key modifiers => maybe this is additional key for previous the gesture
                    keyInfo = new HotKeyInfo(_prevKeyGesture, keyEventArgs.Key);
                }
                else if (keyEventArgs.Key is >= Key.F1 and <= Key.F12)
                {
                    // we have a function key without modifiers, so we can treat it as a gesture
                    keyInfo = new HotKeyInfo(keyEventArgs.Key);
                }
                else
                {
                    // no previous gesture, so it's not a command gesture
                    return;
                }
            }
            else
            {
                keyInfo = new HotKeyInfo(keyEventArgs.Key, keyEventArgs.KeyModifiers);
            }

            // publish the hot key event
            _hotKeySubject.OnNext(keyInfo);

            if (_isHotKeyRecognitionEnabled.CurrentValue == false)
            {
                return;
            }

            var matchedCommands = _commandsVsGesture
                .Where(kvp => kvp.Value == keyInfo)
                .Select(kvp => _commands[kvp.Key])
                .ToImmutableArray();

            if (matchedCommands == default)
            {
                // we cant find a command for this gesture, but maybe we need additional key after this gesture
                _prevKeyGesture = keyInfo.Gesture;
                return;
            }

            if (matchedCommands.Length == 1)
            {
                // only one command for this gesture, so we can execute it immediately
                await _nav.SelectedControl.CurrentValue.ExecuteCommand(
                    matchedCommands[0].Info.Id,
                    CommandArg.Empty
                );
                _prevKeyGesture = null; // reset previous gesture after execution
                keyEventArgs.Handled = true;
            }
            else
            {
                // we have multiple commands for this gesture, so we need check can execute for each command
                foreach (var item in matchedCommands)
                {
                    // we execute first command that can be executed in the current context
                    // TODO: ask user which command to execute if multiple commands can be executed
                    if (item.CanExecute(_nav.SelectedControl.CurrentValue, CommandArg.Empty, out _))
                    {
                        await _nav.SelectedControl.CurrentValue.ExecuteCommand(
                            matchedCommands[0].Info.Id,
                            CommandArg.Empty
                        );
                        break;
                    }
                }
            }
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
        CommandArg param,
        CancellationToken cancel
    )
    {
        if (factory.CanExecute(context, param, out var target))
        {
            // we save here the path to the context, because it can be changed during command execution
            var navPath = context.GetPathFromRoot();

            var backup = await factory.Execute(target, param, cancel);
            var snapShot = new CommandSnapshot(factory.Info.Id, navPath, param, backup);
            _onCommand.OnNext(snapShot);
            _logger.ZLogTrace(
                $"Execute command {factory.Info.Id}(context: {target.Id}, arg: {param.ToString()}, rollback: {backup?.ToString()})"
            );
            return;
        }

        _logger.ZLogError(
            $"Command '{factory.Info.Id}' cannot be executed in the context of '{context.GetType().Name}'"
        );
        throw new CommandCannotExecuteException(factory.Info, context);
    }

    private void ReloadHotKeys(
        out ImmutableDictionary<string, HotKeyInfo?> commandVsGesture,
        out ImmutableDictionary<HotKeyInfo, ImmutableArray<IAsyncCommand>> gestureVsCommand
    )
    {
        var keyVsCommandBuilder = ImmutableDictionary.CreateBuilder<
            HotKeyInfo,
            ImmutableArray<IAsyncCommand>
        >();
        var commandVsKeyBuilder = ImmutableDictionary.CreateBuilder<string, HotKeyInfo?>();

        // define hotkeys by default values
        foreach (var value in _commands.Values)
        {
            SetKeyGesture(
                ref keyVsCommandBuilder,
                ref commandVsKeyBuilder,
                value,
                value.Info.DefaultHotKey
            );
        }

        var config = _cfg.Get<CommandServiceConfig>();
        var configChanged = false;

        // load hotkeys from config
        foreach (var (commandId, hotKey) in config.HotKeys)
        {
            if (string.IsNullOrWhiteSpace(hotKey))
            {
                // if custom key is null => load default value
                continue;
            }

            HotKeyInfo hotKeyInfo;
            try
            {
                hotKeyInfo = HotKeyInfo.Parse(hotKey); // ensure a value from config is valid
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

            SetKeyGesture(ref keyVsCommandBuilder, ref commandVsKeyBuilder, command, hotKeyInfo);
        }

        gestureVsCommand = keyVsCommandBuilder.ToImmutable();
        commandVsGesture = commandVsKeyBuilder.ToImmutable();

        if (configChanged)
        {
            _cfg.Set(config);
        }
    }

    private static void SetKeyGesture(
        ref ImmutableDictionary<
            HotKeyInfo,
            ImmutableArray<IAsyncCommand>
        >.Builder keyVsCommandBuilder,
        ref ImmutableDictionary<string, HotKeyInfo?>.Builder builder,
        IAsyncCommand command,
        HotKeyInfo? key
    )
    {
        if (key == null)
        {
            // remove key gesture for the command
            if (builder.TryGetValue(command.Info.Id, out var existKey) == false)
            {
                builder[command.Info.Id] = null;
                return;
            }

            if (existKey == null)
            {
                return;
            }

            if (!keyVsCommandBuilder.TryGetValue(existKey, out var commands))
            {
                return;
            }

            // key already exists, remove command from the list
            if (commands.Length > 1)
            {
                keyVsCommandBuilder[existKey] = commands.Remove(command);
            }
            else
            {
                // last command with this key, remove it
                keyVsCommandBuilder.Remove(existKey);
            }
        }
        else
        {
            // add or update key gesture for the command
            if (keyVsCommandBuilder.TryGetValue(key, out var commands))
            {
                // key already exists, add command to the list
                keyVsCommandBuilder[key] = commands.Add(command);
            }
            else
            {
                // key does not exist, create new array with command
                keyVsCommandBuilder[key] = [command];
            }

            // add command to the dictionary
            builder[command.Info.Id] = key;
        }
    }

    public ICommandInfo? GetCommandInfo(string commandId)
    {
        return _commands.TryGetValue(commandId, out var factory) ? factory.Info : null;
    }

    public IEnumerable<ICommandInfo> Commands => _commands.Values.Select(x => x.Info);

    public ICommandHistory CreateHistory(IRoutable? owner)
    {
        var history = new CommandHistory(owner, this, _path, _loggerFactory);
        return history;
    }

    public ValueTask Execute(
        string commandId,
        IRoutable context,
        CommandArg param,
        CancellationToken cancel = default
    )
    {
        if (_commands.TryGetValue(commandId, out var factory))
        {
            return InternalExecute(factory, context, param, cancel);
        }

        _logger.ZLogError($"Command with id '{commandId}' not found.");
        return ValueTask.FromException(new CommandNotFoundException(commandId));
    }

    public Observable<HotKeyInfo> OnHotKey => _hotKeySubject;

    public ReactiveProperty<bool> IsHotKeyRecognitionEnabled => _isHotKeyRecognitionEnabled;

    public void ResetAllHotKeys()
    {
        var config = _cfg.Get<CommandServiceConfig>();
        config.HotKeys.Clear();
        _cfg.Set(config);
        ReloadHotKeys(out _commandsVsGesture, out _gestureVsCommand);
    }

    public void SetHotKey(string commandId, HotKeyInfo? hotKey)
    {
        if (!_commands.ContainsKey(commandId))
        {
            throw new CommandNotFoundException(commandId);
        }
        var config = _cfg.Get<CommandServiceConfig>();
        config.HotKeys[commandId] = hotKey?.ToString();
        _cfg.Set(config);
        ReloadHotKeys(out _commandsVsGesture, out _gestureVsCommand);
    }

    public HotKeyInfo? GetHotKey(string commandId)
    {
        if (_commandsVsGesture.TryGetValue(commandId, out var gesture))
        {
            return gesture;
        }

        throw new CommandNotFoundException(commandId);
    }

    public Observable<CommandSnapshot> OnCommand => _onCommand;

    public async ValueTask Undo(CommandSnapshot command, CancellationToken cancel)
    {
        var context = await _nav.GoTo(command.ContextPath);
        if (_commands.TryGetValue(command.CommandId, out var factory) && command.OldValue != null)
        {
            if (factory.CanExecute(context, command.OldValue, out var target))
            {
                await factory.Execute(target, command.OldValue, cancel);
                _logger.ZLogTrace(
                    $"Undo command {factory.Info.Id}: context: {context}, param: {command.NewValue}"
                );
            }
            else
            {
                _logger.ZLogWarning(
                    $"Can't undo command {factory.Info.Id} in context {context}: {nameof(factory.CanExecute)} check failed."
                );
            }
        }
    }

    public async ValueTask Redo(CommandSnapshot command, CancellationToken cancel = default)
    {
        var context = await _nav.GoTo(command.ContextPath);
        if (_commands.TryGetValue(command.CommandId, out var factory))
        {
            if (factory.CanExecute(context, command.NewValue, out var target))
            {
                await factory.Execute(target, command.NewValue, cancel);
                _logger.ZLogTrace(
                    $"Redo command {factory.Info.Id}: context: {context}, param: {command.NewValue}"
                );
            }
            else
            {
                _logger.ZLogWarning(
                    $"Can't redo command {factory.Info.Id} in context {context}: {nameof(factory.CanExecute)} check failed."
                );
            }
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
}
