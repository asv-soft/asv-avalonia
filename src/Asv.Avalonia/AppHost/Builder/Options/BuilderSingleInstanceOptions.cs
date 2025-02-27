namespace Asv.Avalonia;

public sealed class BuilderSingleInstanceOptions
{
    public Func<IAppInfo, string?> MutexName { get; set; } = info => info.Name;
    public Func<IAppInfo, string?> NamedPipe { get; set; } = _ => null;
}

public static class BuilderSingleInstanceOptionsExtensions
{
    /// <summary>
    /// Ensures that only a single instance of the application is running on the system.
    /// If another instance is already running, the current instance will not proceed further.
    /// </summary>
    /// <param name="options">The options of the single instance to add the mutexName to.</param>
    /// <param name="mutexName"> Unique mutex name. </param>
    public static void WithMutexName(this BuilderSingleInstanceOptions options, string mutexName)
    {
        options.MutexName = _ => mutexName;
    }

    /// <summary>
    /// Enables forwarding of command-line arguments to an already running instance of the application.
    /// If the application is not already running, the current instance will handle the arguments as usual.
    /// Must be called with <see cref="WithMutexName"/>.
    /// </summary>
    /// <param name="options">The options of the single instance to add the namedPipeName to.</param>
    /// <param name="namedPipeName"> Unique pipe name. </param>
    public static void EnableArgumentForwarding(
        this BuilderSingleInstanceOptions options,
        string? namedPipeName = null
    )
    {
        options.NamedPipe = info => namedPipeName ?? info.Name;
    }
}
