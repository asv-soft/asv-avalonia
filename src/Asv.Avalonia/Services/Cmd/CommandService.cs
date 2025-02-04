using System.Collections.Immutable;
using System.Composition;
using Asv.Cfg;
using Avalonia.Controls;
using Avalonia.Input;
using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public class CommandServiceConfig
{
    public Dictionary<string, string?> HotKeys { get; set; } = new();
}

[Export(typeof(ICommandService))]
[Shared]
public class CommandService : ICommandService
{
    private readonly IConfiguration _cfg;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ImmutableDictionary<string, ICommandFactory> _commands;
    private ImmutableDictionary<string, KeyGesture> _commandsVsGesture;
    private ImmutableDictionary<KeyGesture, ICommandFactory> _gestureVsCommand;
    private readonly ILogger<CommandService> _logger;

    [ImportingConstructor]
    public CommandService(
        IConfiguration cfg,
        [ImportMany] IEnumerable<ICommandFactory> factories,
        ILoggerFactory loggerFactory
    )
    {
        _cfg = cfg;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<CommandService>();
        _commands = factories.ToImmutableDictionary(x => x.Info.Id);
        ReloadHotKeys();
    }

    private void ReloadHotKeys(Action<IDictionary<string, string?>>? modifyConfig = null)
    {
        var keyVsCommandBuilder = ImmutableDictionary.CreateBuilder<KeyGesture, ICommandFactory>();
        var commandVsKeyBuilder = ImmutableDictionary.CreateBuilder<string, KeyGesture>();

        // load default hot keys
        foreach (var value in _commands.Values)
        {
            if (value.Info.DefaultHotKey == null)
            {
                // skip commands without hot keys
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

        // load custom hot keys from config
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
                keyGesture = KeyGesture.Parse(hotKey);
            }
            catch (Exception e)
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

            commandVsKeyBuilder[commandId] = keyGesture;
            keyVsCommandBuilder[keyGesture] = command;
        }

        _gestureVsCommand = keyVsCommandBuilder.ToImmutable();
        _commandsVsGesture = commandVsKeyBuilder.ToImmutable();

        if (configChanged)
        {
            _cfg.Set(config);
        }
    }

    public IEnumerable<ICommandInfo> Commands => _commands.Values.Select(x => x.Info);

    public IAsyncCommand? CreateCommand(string commandId)
    {
        return _commands.TryGetValue(commandId, out var command) ? command.Create() : null;
    }

    public ICommandHistory CreateHistory(IRoutable owner)
    {
        var history = new CommandHistory(owner, this, _loggerFactory);
        return history;
    }

    public bool CanExecuteCommand(string commandId, IRoutable context, out IRoutable? target)
    {
        if (_commands.TryGetValue(commandId, out var command))
        {
            return command.CanExecute(context, out target);
        }

        target = null;
        return false;
    }

    public void ChangeHotKey(string commandId, KeyGesture? hotKey)
    {
        ReloadHotKeys(config => config[commandId] = hotKey?.ToString());
    }

    public bool CanExecuteCommand(
        KeyGesture hotKey,
        IRoutable context,
        out IAsyncCommand? command,
        out IRoutable? target
    )
    {
        if (_gestureVsCommand.TryGetValue(hotKey, out var cmdFactory))
        {
            command = cmdFactory.Create();
            return cmdFactory.CanExecute(context, out target);
        }

        command = null;
        target = null;
        return false;
    }
}
