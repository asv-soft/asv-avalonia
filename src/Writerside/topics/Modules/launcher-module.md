# Launcher

The launcher starts a desktop executable and waits for an explicit readiness signal over a named pipe. It has two sides: `Asv.Avalonia.Launcher` provides the launcher application, and `Asv.Avalonia.LauncherReady` integrates the target application. Shared protocol contracts live in `Asv.Avalonia.Launcher.Api`.

## Configure the target

Reference `Asv.Avalonia.LauncherReady` and import `Asv.Avalonia.Launcher.Ready`. After the ordinary application services and desktop shell have been registered, add:

```c#
builder.RegisterLauncher(configure => configure.IsOptional());
```

`IsOptional()` allows direct development launches without launcher endpoint arguments. Without that option, the notifier requires launcher arguments. The option is also bound from the `Launcher` configuration section through `LauncherFeatureOptions.IsOptional`.

`RegisterLauncher` registers the notifier and hosted `LauncherFeature`, and replaces `IAppRestartFeature` with `DesktopWithLauncherAppRestartFeature`. In the design-time environment it uses `NullLauncherNotifier`. Accessing `ModuleLauncherReady` alone exposes the configuration builder; it does not perform the complete registration.

The hosted feature waits through `IShellHost.ExecuteNowOrWhenShellLoaded` and attempts one READY notification. READY means that the shell has loaded; it does not establish that every device, network connection, background job, or plugin-specific operation has completed. Notification failures are logged.

## Configure the launcher application

Derive an Avalonia application from `LauncherApp`. Override `ConfigureLauncher(LauncherApplicationOptions options)` to customize its title, description, footer, icon, view factory, or window. The default view is `DefaultLauncherView` inside `LauncherWindow`.

On successful startup, the default options shut down the launcher after 700 milliseconds. Set `ShutdownOnSuccess` or `SuccessShutdownDelay` for another presentation. The launcher view model is disposed when the desktop application exits.

`Asv.Avalonia.Example.Launcher` is the executable example. Its entry point uses `LauncherCommandLineParser.WithDefaultTarget` to supply a default executable relative to `AppContext.BaseDirectory`. Its default filename is Windows-specific; supply the correct executable path for another deployment.

## Start a target

The command-line parser accepts:

```text
launcher --target /absolute/path/to/application --timeout-sec 30 -- application-argument
```

`--target` is required unless the host supplies a default. `--timeout-sec` must be a positive integer and defaults to 10 seconds. `--pipe` and `--token` can override the generated session endpoint values. Arguments after `--` are forwarded to the target.

The launcher validates options, starts the target process, then waits for READY, ERROR, process exit, or startup timeout. It injects `--launcher-pipe`, `--launcher-token`, and `--launcher-path` for target communication and restart. Progress signals update startup progress without completing the wait.

A READY signal produces a successful result. An ERROR signal produces a startup failure. If the target exits first, the result records that early exit. Startup timeout attempts to terminate the target process. Callers using `LauncherOrchestrator.RunAsync` directly must handle cancellation and inspect `LauncherRunResult`; cancellation propagates from the monitor.

## Restart and update boundaries

The replacement restart feature starts the launcher with the current target process and filtered application arguments. It needs the injected launcher path; a direct optional launch does not supply that path, and a missing path is logged instead of restarting through the launcher.

These projects implement process startup, readiness, and launcher-mediated restart. They do not download application versions, verify update packages, replace installed files, or provide rollback. Design any application updater as a separate component and define how it coordinates shutdown and executable replacement before integrating it with this startup flow. Plugin installation and plugin update behavior belongs to the plugins module.

The launcher packages have a separate `LauncherVersion` in `src/Directory.Build.props`; do not assume that their version matches the main framework's `ProductVersion`. Keep the launcher, readiness integration, and shared protocol contracts compatible when packaging a target application.
