﻿using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public interface IShellHost
{
    IShell Shell { get; }
}

public class NullShellHost : IShellHost
{
    public static IShellHost Instance { get; } = new NullShellHost();

    private NullShellHost() { }

    public IShell Shell => DesignTimeShellViewModel.Instance;
}

public enum ShellErrorState
{
    Normal,
    Warning,
    Error,
}

public interface IShell : IRoutable
{
    ObservableList<IMenuItem> MainMenu { get; }
    IReadOnlyObservableList<IPage> Pages { get; }
    BindableReactiveProperty<IPage?> SelectedPage { get; }
    BindableReactiveProperty<ShellErrorState> ErrorState { get; }
    BindableReactiveProperty<string> Title { get; }
}
