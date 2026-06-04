# Codex Instructions

## Project Overview

- This repository contains `asv-avalonia`, a lightweight framework for building cross-platform Avalonia UI applications.
- The main source tree is under `src/`.
- The solution file is `src/Asv.Avalonia.slnx`.
- The project targets .NET 10 through `src/Directory.Build.props`; `src/global.json` allows SDK roll-forward to the latest major SDK.
- Core packages and feature areas include Avalonia, plugins, launcher support, IO, GeoMap, examples, and xUnit tests.

## Working Rules

- Keep changes surgical. Touch only files needed for the current request.
- Match the style already used in the nearby code, XAML, tests, and project files.
- Do not refactor adjacent code unless the task requires it.
- Do not remove unrelated dead code or generated files unless explicitly asked.
- Prefer the simplest implementation that fully satisfies the request.
- Add abstractions only when they clearly reduce real duplication or follow an existing local pattern.
- Preserve user changes in the worktree. Never revert work you did not make unless explicitly instructed.

## Language and Documentation

- Write code comments in English.
- Write XML documentation, Markdown documentation, README content, and other developer-facing docs in English.
- Use English names for public APIs, types, members, variables, files, and modules.
- Keep comments concise and useful. Prefer self-explanatory code over comments.
- Add comments only for intent, constraints, assumptions, tradeoffs, or non-obvious behavior.
- Update or remove comments when code changes make them inaccurate.

## Architecture

- Keep classes, services, and modules focused on a single responsibility.
- Follow SOLID principles where they improve clarity and maintainability.
- Prefer composition over inheritance unless inheritance is already the local pattern or clearly justified.
- Keep domain logic separate from UI, infrastructure, persistence, and framework-specific code.
- Depend on abstractions at system boundaries when this improves testability, extensibility, or clarity.
- Keep public APIs explicit, stable, and easy to understand.
- Avoid hidden side effects, unclear ownership, and overly broad service objects.

## Avalonia and UI

- Follow existing Avalonia patterns in `.axaml` and code-behind files.
- Keep view models separate from views and platform-specific behavior.
- Use existing services, mixins, shell/page patterns, resources, and localization files before introducing new mechanisms.
- Keep UI changes consistent with the current theme, resource dictionaries, and control structure.
- When adding user-facing text, update localization resources consistently when the surrounding feature is localized.

## PropertyEditor Notes

- `PropertyTextBoxViewModel` owns text apply/cancel behavior and can register text undo with its constructor `enableUndo` flag. `UndoController` already handles undo/redo execution state, so avoid local guard flags that duplicate that responsibility.
- `PropertyUnitViewModel` has its own undo entries for SI value changes and unit changes; keep base text undo disabled for unit properties unless the undo model is intentionally redesigned.
- `ExtendedPropertyEditorView` is a wide layout wrapper around the same property view models. Hide duplicated inner prefix icons through `PropertyEditorDisplayOptions.ShowPrefixIcon`, while keeping the row icon and action icons visible.
- Keep extended rows visually transparent unless explicitly asked otherwise; avoid hover/background row styling that competes with the inner property control.
- Example pages may show several editor instances over the same model, but each editor should receive its own property view model instances to avoid reusing one view model across visual trees.

## Testing and Verification

- For behavioral changes, prefer adding or updating focused xUnit tests in `src/Asv.Avalonia.Test`.
- Use Arrange/Act/Assert style when it matches existing tests.
- Run the narrowest useful verification first, then broaden when the change has wider impact.
- Common commands:
  - `dotnet test src/Asv.Avalonia.Test/Asv.Avalonia.Test.csproj`
  - `dotnet build src/Asv.Avalonia.slnx`
- If a command cannot be run in the current environment, report that clearly with the reason.

## Generated and Local Files

- Avoid editing `bin/`, `obj/`, logs, local navigation files, and IDE user files unless the request specifically targets them.
- Treat `.Designer.cs`, generated resources, and package artifacts carefully. Prefer changing the source resource files or project configuration that generates them.
- Do not commit or rely on local machine state from `.idea`, `.run`, user settings, or logs unless explicitly requested.

## Execution Style

- Start by reading the relevant existing code and tests.
- State assumptions when the request is ambiguous or multiple interpretations are likely.
- For multi-step work, define a short plan and verify each meaningful step.
- If a simpler or lower-risk approach exists, use it or explain the tradeoff.
- Finish with a concise summary of what changed and what verification was run.
