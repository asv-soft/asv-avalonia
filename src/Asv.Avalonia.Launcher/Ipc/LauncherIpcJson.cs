using System.Text.Json;
using Asv.Avalonia.Launcher.Api;

namespace Asv.Avalonia.Launcher.Ipc;

internal static class LauncherIpcJson
{
    public static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    public static string Serialize(LauncherIpcMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        return JsonSerializer.Serialize(message, SerializerOptions);
    }

    public static LauncherIpcMessage Deserialize(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            throw new InvalidOperationException("IPC payload is empty.");
        }

        var message = JsonSerializer.Deserialize<LauncherIpcMessage>(payload, SerializerOptions);
        if (message == null)
        {
            throw new InvalidOperationException("IPC message is invalid.");
        }

        return message;
    }
}
