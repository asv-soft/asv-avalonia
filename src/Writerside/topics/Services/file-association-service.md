# File Association Service

## Overview

`IFileAssociationService` defines the application-level contract for querying supported file types
and opening or creating files.

The default `FileAssociationService` implementation uses registered `IFileHandler` instances. It
exposes their supported file types and dispatches each operation to one handler. Each handler can 
expose one or more `FileTypeInfo` entries. A descriptor contains the file type ID,
title, extension, icon, and its open and create capabilities.

## Default Implementation

The following dispatch rules belong to `FileAssociationService`. Another
`IFileAssociationService` implementation may handle the same contract differently.

Opening and creating use different selectors:

| Operation                    | Handler selector                                                                           | Failure                                                        |
|------------------------------|--------------------------------------------------------------------------------------------|----------------------------------------------------------------|
| `Open(path, cancel)`         | The first handler whose `CanOpen(path)` returns `true`, evaluated by ascending `Priority`. | `NotSupportedException` when no handler accepts the path.      |
| `Create(path, type, cancel)` | The handler that advertises a `FileTypeInfo` with the same `Id` as `type`.                 | `InvalidOperationException` when no handler advertises the ID. |

Both operations accept an optional `CancellationToken`. The dispatcher forwards it unchanged to
the selected handler.

### Opening a file

The default service sorts handlers once when it is created. Lower `Priority` values are evaluated
first. It calls `CanOpen(path)` in that order and delegates to `Open(path, cancel)` as soon as a
handler accepts the path. Later handlers are not evaluated.

### Creating a file

The caller selects a `FileTypeInfo` from `SupportedFiles` and passes it to `Create`. The service
matches only its `Id`, then forwards the path, descriptor, and cancellation token to the owning
handler.

## Registration

The file association service is included in the default core service registration. If an
application composes core services explicitly, register it through the services builder:

```C#
builder.RegisterCore(core =>
{
    core.RegisterServices(services =>
    {
        services.RegisterFileAssociation();
        // Register the other services required by the application.
    });
});
```

Register each application-specific handler through the file association builder:

```C#
builder.FileAssociation.Register<WorkspaceSnapshotFileHandler>();
```

Handlers are registered as singletons. Their dependencies and their `SupportedFiles` collections
should therefore be suitable for the application lifetime.

## Example: Workspace Snapshot Files

Suppose an application can save its current workspace and later restore it. A small custom file
format makes both operations available through `IFileAssociationService`.

The application-specific store contains the actual persistence logic:

```C#
public interface IWorkspaceSnapshotStore
{
    ValueTask Save(string path, CancellationToken cancel = default);
    ValueTask Restore(string path, CancellationToken cancel = default);
}
```

The handler describes the `.asvworkspace` type and delegates persistence to that store:

```C#
using Material.Icons;

public sealed class WorkspaceSnapshotFileHandler(
    IWorkspaceSnapshotStore snapshots
) : IFileHandler
{
    private static readonly FileTypeInfo SnapshotType = new(
        id: "workspace.snapshot",
        title: "Workspace snapshot",
        extension: "asvworkspace",
        canOpen: true,
        canCreate: true,
        icon: MaterialIconKind.FileOutline
    );

    private static readonly FileTypeInfo[] FileTypes = [SnapshotType];

    public int Priority => 0;

    public IEnumerable<FileTypeInfo> SupportedFiles => FileTypes;

    public bool CanOpen(string path)
    {
        return string.Equals(
            Path.GetExtension(path),
            $".{SnapshotType.Extension}",
            StringComparison.OrdinalIgnoreCase
        );
    }

    public ValueTask Open(string path, CancellationToken cancel = default)
    {
        return snapshots.Restore(path, cancel);
    }

    public ValueTask Create(
        string path,
        FileTypeInfo type,
        CancellationToken cancel = default
    )
    {
        if (type.Id != SnapshotType.Id)
        {
            throw new ArgumentException("Unsupported workspace file type", nameof(type));
        }

        return snapshots.Save(path, cancel);
    }
}
```

Register both the store and the handler during application setup:

```C#
builder.Services.AddSingleton<IWorkspaceSnapshotStore, WorkspaceSnapshotStore>();
builder.FileAssociation.Register<WorkspaceSnapshotFileHandler>();
```

After registration, the default service exposes the snapshot descriptor through `SupportedFiles`
and can dispatch open and create operations to `WorkspaceSnapshotFileHandler`.

## Direct Usage

Inject `IFileAssociationService` when application code needs to open or create a file:

```C#
public sealed class WorkspaceCommands(IFileAssociationService files)
{
    public ValueTask Open(string path, CancellationToken cancel = default)
    {
        return files.Open(path, cancel);
    }

    public ValueTask CreateSnapshot(string path, CancellationToken cancel = default)
    {
        var type = files.SupportedFiles.Single(x => x.Id == "workspace.snapshot");
        return files.Create(path, type, cancel);
    }
}
```

## API {collapsible="true" default-state="collapsed"}

### [IFileAssociationService](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Services/FileAssociation/IFileAssociationService.cs)

Defines application-level operations for querying supported file types and opening or creating
files.

| Property         | Type                        | Description                          |
|------------------|-----------------------------|--------------------------------------|
| `SupportedFiles` | `IEnumerable<FileTypeInfo>` | File types supported by the service. |

| Method                                                             | Return Type | Description                           |
|--------------------------------------------------------------------|-------------|---------------------------------------|
| `Open(string path, CancellationToken cancel)`                      | `ValueTask` | Opens the specified file.             |
| `Create(string path, FileTypeInfo type, CancellationToken cancel)` | `ValueTask` | Creates a file of the specified type. |

#### `IFileAssociationService.Open`

| Parameter | Type                | Description                         |
|-----------|---------------------|-------------------------------------|
| `path`    | `string`            | The path of the file to open.       |
| `cancel`  | `CancellationToken` | A token that cancels the operation. |

#### `IFileAssociationService.Create`

| Parameter | Type                | Description                           |
|-----------|---------------------|---------------------------------------|
| `path`    | `string`            | The path at which to create the file. |
| `type`    | `FileTypeInfo`      | The file type to create.              |
| `cancel`  | `CancellationToken` | A token that cancels the operation.   |

### [IFileHandler](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Services/FileAssociation/IFileAssociationService.cs)

Defines an application-specific handler for one or more file types.

| Property         | Type                        | Description                               |
|------------------|-----------------------------|-------------------------------------------|
| `Priority`       | `int`                       | Handler priority.                         |
| `SupportedFiles` | `IEnumerable<FileTypeInfo>` | File types supported by the handler.      |

| Method                                                             | Return Type | Description                                    |
|--------------------------------------------------------------------|-------------|------------------------------------------------|
| `CanOpen(string path)`                                             | `bool`      | Determines whether the handler accepts a path. |
| `Open(string path, CancellationToken cancel)`                      | `ValueTask` | Opens the specified file.                      |
| `Create(string path, FileTypeInfo type, CancellationToken cancel)` | `ValueTask` | Creates the requested file type.               |

#### `IFileHandler.CanOpen`

| Parameter | Type     | Description                      |
|-----------|----------|----------------------------------|
| `path`    | `string` | The file-system path to inspect. |

#### `IFileHandler.Open`

| Parameter | Type                | Description                         |
|-----------|---------------------|-------------------------------------|
| `path`    | `string`            | The path of the file to open.       |
| `cancel`  | `CancellationToken` | A token that cancels the operation. |

#### `IFileHandler.Create`

| Parameter | Type                | Description                             |
|-----------|---------------------|-----------------------------------------|
| `path`    | `string`            | The path at which to create the file.   |
| `type`    | `FileTypeInfo`      | The file type to create.                |
| `cancel`  | `CancellationToken` | A token that cancels the operation.     |

### [FileTypeInfo](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Services/FileAssociation/IFileAssociationService.cs)

Describes a file type advertised by a handler.

#### `FileTypeInfo` constructor

| Parameter   | Type                | Description                                                             |
|-------------|---------------------|-------------------------------------------------------------------------|
| `id`        | `string`            | File type identifier.                                                   |
| `title`     | `string`            | Human-readable title of the file type.                                  |
| `extension` | `string`            | File name extension, normally without a leading period.                 |
| `canOpen`   | `bool`              | Whether files of this type can be opened.                               |
| `canCreate` | `bool`              | Whether files of this type can be created.                              |
| `icon`      | `MaterialIconKind?` | Optional icon associated with the file type.                            |

| Property    | Type                | Description                                                             |
|-------------|---------------------|-------------------------------------------------------------------------|
| `Id`        | `string`            | File type identifier.                                                   |
| `Title`     | `string`            | Human-readable title of the file type.                                  |
| `Extension` | `string`            | File name extension, normally without a leading period.                 |
| `CanOpen`   | `bool`              | Whether files of this type can be opened.                               |
| `CanCreate` | `bool`              | Whether files of this type can be created.                              |
| `Icon`      | `MaterialIconKind?` | Optional icon associated with the file type.                            |
