# What is Asv.Avalonia?

Asv.Avalonia is a framework built on [Avalonia UI](https://github.com/AvaloniaUI/Avalonia) designed to provide a robust
foundation for quickly creating complex desktop applications.

Rather than just a collection of components, it offers a structured environment and a set of architectural rules. It
handles the "heavy lifting" of application infrastructure—navigation, dependency injection and modularity—so you can
focus on building business logic immediately.

## Core Concepts

To use Asv.Avalonia effectively, it is helpful to understand its fundamental architecture:

* **Hierarchical Tree of ViewModels:** This is the backbone of the framework. All logic is structured around a unified
  tree of ViewModels, ensuring a clear parent-child relationship across the entire application.
* **App Builder & Configuration:** The framework uses an ASP.NET Core-like builder pattern combined with
  `CompositionHost`. This allows for fine-grained configuration at startup—for example, you can easily disable standard
  pages or swap out core services.
* **Module System:** You can extend the framework functionality by [using modules](what-is-a-module.md). You can create
  your own module or use an existing one.

## Key Features

Asv.Avalonia provides essential features out of the box:

* **Advanced Shell:** A ready-to-use window structure with support for layout persistence (saving/restoring panel sizes
  and positions).
* **Navigation:** URI-based, page-oriented navigation system.
* **Command System:** A centralized manager for commands, shortcuts and Undo/Redo operations.
* **File Picker:** A unified, cross-platform file picking mechanism.
* **Built-in Tools:** Ready-made pages for Settings, Logs and UI theming.

## Quick Tour

Below is a deeper look at the main components included in the library.

### Shell & Layout

The shell is the central host for the application UI. It provides a standard visual hierarchy (toolbar, side panels,
main content area) and integrates with the **Layout service**. This means the application can remember the state of
windows and panels between sessions — for example, the current search text, filtering settings, the selected item and
so on.

![Shell page screenshot](shell-page.png)

### Navigation

Navigation in Asv.Avalonia is URI-like and page-oriented:

- You can register pages with keys/URIs and navigate programmatically.
- The shell keeps a consistent experience for back/forward, deep links, and restoring views.

### Command System

Asv.Avalonia includes a centralized manager for commands and shortcuts:

- Commands have metadata (name, description, default shortcut).
- Commands implement the Command pattern, providing built-in support for undo and redo operations.

![Commands page screenshot](commands-page.png)

### Modules & Plugins

The framework is designed to scale:

- **Modules:** The framework’s functionality can be extended through modules. 
  You can either [develop your own module](how-to-create-module.md) or use existing ready-made modules.
- **Plugins:** plugins module allows you to dynamically load plugins (external assemblies). This allows
  you to ship optional features or maintain a modular codebase where features register their own pages, menus and
  commands at startup.

### Built-in Pages

Out of the box, the framework provides standard pages:

- **Settings:** Centralized app settings UI that can be extended with additional subpages.
- **Log Viewer:** A viewer for application's logs with filtering, pagination, and search functionality.

![Logs page screenshot](logs-page.png)

### Controls & Tools

Asv.Avalonia ships with specialized controls and helpers:

- **Unit service:** A service for working with different units of measurement. It lets you operate with values like
  temperature regardless of which unit the user chooses, automatically converts values to the SI system when needed and
  allows you to define unit names, descriptions, etc.
- **Useful Controls:** Dock, dialogs, info bars, indicators, etc.
- And more!

## Ready to dive in?

Get your application running in minutes.

* **Quick Start**: follow our [step-by-step tutorial](project-setup.md) to build your first shell.
* **Explore the Code**: check out
  the [Asv.Avalonia.Example](https://github.com/asv-soft/asv-avalonia/tree/main/src/Asv.Avalonia.Example) to see a fully
  functional app.