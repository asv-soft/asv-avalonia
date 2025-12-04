# What is Asv.Avalonia?

Asv.Avalonia is a framework built on [Avalonia UI](https://github.com/AvaloniaUI/Avalonia) designed to provide a robust
foundation for quickly creating complex desktop applications.

Rather than just a collection of components, it offers a structured environment and a set of architectural rules. It
handles the "heavy lifting" of application infrastructure—navigation, dependency injection, and modularity—so you can
focus on building business logic immediately.

## Core Concepts

To use Asv.Avalonia effectively, it is helpful to understand its fundamental architecture:

* **Hierarchical Tree of ViewModels:** This is the backbone of the framework. All logic is structured around a unified
  tree of ViewModels, ensuring a clear parent-child relationship across the entire application.
* **App Builder & Configuration:** The framework uses an ASP.NET Core-like builder pattern combined with
  `CompositionHost`. This allows for fine-grained configuration at startup—for example, you can easily disable standard
  pages or swap out core services.
* **Module System:** The application is composed of modules. You can write your own modules to extend functionality,
  separating your code into logical units.

## Key Capabilities

Asv.Avalonia provides essential features out of the box:

* **Advanced Shell:** A ready-to-use window structure with support for layout persistence (saving/restoring panel sizes
  and positions).
* **Navigation:** URI-based, page-oriented navigation system.
* **Command System:** A centralized manager for commands, shortcuts, and Undo/Redo operations.
* **File Picker:** A unified, cross-platform file picking mechanism.
* **Built-in Tools:** Ready-made pages for Settings, Logs, and UI theming.

## Detailed Features

Below is a deeper look at the main components included in the library.

### Shell & Layout

The shell is the central host for the application UI. It provides a standard visual hierarchy (toolbar, side panels,
main content area) and integrates with the **Layout Persistence** service. This means the application can remember the
state of windows and panels between sessions, providing a polished user experience.

![Shell page screenshot](shell-page.png)

### Navigation

Navigation in Asv.Avalonia is URI-like and page-oriented:

- You can register pages with keys/URIs and navigate programmatically.
- The shell keeps a consistent experience for back/forward, deep links, and restoring views.

### Command System

Asv.Avalonia includes a centralized manager for commands and shortcuts:

- Commands have metadata (name, description, default shortcut).
- Undo/redo works through undoable command wrappers integrated with a global stack.

![Commands page screenshot](commands-page.png)

### Modules & Plugins

The framework is designed to scale:

- **Modules:** You can split your application into discrete modules.
- **Plugins:** Using the base module capabilities, you can load external assemblies (plugins) dynamically. This allows
  you to ship optional features or maintain a modular codebase where features register their own pages, menus, and
  commands at startup.

### Built-in Pages

Out of the box, the framework provides standard pages:

- **Settings:** Centralized app settings UI with persistent storage helpers.
- **Logs:** A viewer for application logs with filtering and context.

![Logs page screenshot](logs-page.png)

### Controls & Tools

Asv.Avalonia ships with specialized controls and helpers:

- **File Picker:** A reliable abstraction for opening and saving files.
- **Toolkit Controls:** Lists, custom editors, and widgets used within the ASV ecosystem.
- **Map Control:** A map UI adapted for Avalonia (tile layers, markers), commonly used in drone ground-station apps.

## Ready to dive in?

Get your application running in minutes.

* **Quick Start**: follow our [step-by-step tutorial](install.md) to build your first shell.
* **Explore the Code**: check out
  the [Asv.Avalonia.Example](https://github.com/asv-soft/asv-avalonia/tree/main/src/Asv.Avalonia.Example) to see a fully
  functional app.