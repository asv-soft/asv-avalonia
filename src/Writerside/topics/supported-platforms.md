# Supported Platforms

Asv.Avalonia uses Avalonia for its UI and targets .NET 10. The repository includes a shared desktop
host and separate Android/iOS example projects. Availability of a host is not a guarantee that every
control, device transport, or plugin is supported on every platform.

| Platform | Repository coverage | Verification boundary |
|---|---|---|
| Windows | Desktop example and Windows release workflow | Validate application packaging, native dependencies, and hardware-specific transports for your deployment. |
| macOS | Desktop example; native gallery screenshots in these guides | The Home page, controls gallery, Markdown dialog, workspace, map, and plugin settings were opened for documentation capture on 26 October 2026. This does not certify hardware integration or every feature. |
| Linux | Shared desktop host with X11 options | Verify the target display server, fonts, file picker, window controls, and permissions on your distribution. |
| Android / iOS | Separate example host projects | No mobile runtime verification was performed for this documentation update. Desktop plugins, launchers, and transport APIs require separate platform evaluation. |

The documentation screenshots are real captures from the current example application, using demo
content and a separate local host to avoid interfering with an already running development instance.
Maps may depend on provider credentials and network access. An empty inspector or plugin list is an
initial UI state, rather than a successful physical-device or plugin-installation test.

To reproduce the desktop UI, follow [Project setup](project-setup.md) and the repository commands in
[For developers](for-developers.md). Start with the [controls gallery](property-editor.md) to check
layout and theme behavior before integrating your device or service.

Report platform-specific problems through the repository's issue tracker. Include the framework
version, operating system, architecture, display scaling, and the smallest reproducible scenario.
