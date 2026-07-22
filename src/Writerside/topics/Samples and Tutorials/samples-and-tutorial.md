# Samples and Tutorials

This section contains samples and tutorials for users.
It will help you become more familiar with the framework.
If you want to learn the basics first, check out [Get Started](project-setup.md).

### Recipe Book application

Build a full-featured Recipe Book application on top of the project created in [Get Started](project-setup.md).
The tutorial covers the following topics:

- Creating pages, adding Home Page actions, and organizing feature registrations;
- Modeling recipes and ingredients with Undo/Redo support;
- Collecting user input with modal dialogs and handling routed events;
- Displaying notifications;
- Persisting recipe data and page layout;
- Filtering recipes with the Search Box.

Start the [tutorial here](recipe-book-app.md).

### How to create a module

The framework can be extended with custom [modules](what-is-a-module.md).
This guide shows how to create a module library and load it in a desktop demo application.
It covers the following topics:

- Creating a page and adding an action that opens it from the Home Page;
- Organizing module features with a hierarchical registration builder;
- Registering all default features or selecting individual features;
- Packaging the module into a NuGet package;
- Loading the module in compatible Asv.Avalonia applications.

This guide introduces the current module structure and registration pattern used by Asv.Avalonia.
Follow the [guide here](how-to-create-module.md).
