# Project Structure

Asv.Avalonia projects use a consistent layout. The top level separates application
features from their integration with the shell, while files inside each area are
grouped by purpose and feature. The framework, the `Asv.Avalonia.Example`
application, and the built-in modules follow the same convention.

## Standard layout

The main Asv.Avalonia project has the following structure:

```text
Asv.Avalonia/
├── Core/
│   ├── Services/
│   │   ├── Theme/
│   │   │   ├── IThemeService.cs
│   │   │   ├── ThemeService.cs
│   │   │   └── ThemeRegistrations.cs
│   │   └── ServicesRegistrations.cs
│   ├── Controls/
│   ├── ViewModel/
│   ├── Converters/
│   ├── Events/
│   └── CoreRegistrations.cs
├── Shell/
│   ├── Pages/
│   │   ├── Settings/
│   │   └── PagesRegistrations.cs
│   ├── MainMenu/
│   ├── Status/
│   ├── Window/
│   └── ShellRegistrations.cs
└── AppHostRegistrations.cs
```

- **`Core/`** contains application services, reusable controls, shared view models,
  converters, events, and dialogs.
- **`Core/Services/`** groups services by feature. A feature folder usually contains
  its interfaces, implementation, configuration, and registration file.
- **`Core/Controls/`** contains reusable controls together with their view models
  and related files.
- **`Shell/`** contains the application shell and the features displayed in it.
- **`Shell/Pages/`** groups pages by feature. A page folder contains its view, view
  model, extensions, and registration file.
- **`Shell/MainMenu/`**, **`Shell/Status/`**, and **`Shell/Window/`** contain the
  corresponding parts of the shell.

## Registration file placement

Registration files follow the folder structure. Common areas have an aggregate
registration file, such as `CoreRegistrations.cs`, `ServicesRegistrations.cs`, or
`PagesRegistrations.cs`. Feature-specific registration files are placed next to the
feature they register, for example `Core/Services/Theme/ThemeRegistrations.cs`.

See [](dependency-injection.md) for details on registration builders and application
composition.

## Application example

An application can use the same structure for its own services and shell features:

```text
MyApplication/
├── Core/
│   ├── Services/
│   │   ├── Reports/
│   │   │   ├── IReportService.cs
│   │   │   ├── ReportService.cs
│   │   │   └── ReportsRegistrations.cs
│   │   └── ServicesRegistrations.cs
│   └── CoreRegistrations.cs
├── Shell/
│   ├── Pages/
│   │   ├── Reports/
│   │   │   ├── ReportsPageView.axaml
│   │   │   ├── ReportsPageView.axaml.cs
│   │   │   ├── ReportsPageViewModel.cs
│   │   │   ├── HomePageReportsExtension.cs
│   │   │   └── ReportsRegistrations.cs
│   │   └── PagesRegistrations.cs
│   └── ShellRegistrations.cs
├── MyApplicationRegistrations.cs
├── App.axaml
├── App.axaml.cs
└── Program.cs
```

Keeping the same layout makes framework, module, and application code easier to
navigate. The `Asv.Avalonia.Example` project can be used as a reference when
organizing a new application.

The [](recipe-book-app.md) tutorial builds a complete application on Asv.Avalonia
using this architecture step by step.
