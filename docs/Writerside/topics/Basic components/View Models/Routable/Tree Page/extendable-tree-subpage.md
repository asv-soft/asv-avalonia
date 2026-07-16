# Extendable Tree Subpage

[`ExtendableTreeSubpage<TContext, TSubContext>`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Controls/TreePage/TreeSubpage/ExtendableTreeSubpage.cs)
is the extensible counterpart of [`TreeSubpage`](tree-subpage.md): both implement `ITreeSubpage` and expose the same
`Menu` / `MenuView` surface, but this one derives from [`ViewModel<TExtensionIfc>`](extendable-view-model.md) instead of
the plain [`ViewModel`](view-model.md) base, so extensions can be applied to it.

Like `TreeSubpage<TContext>`, it receives its `ITreeSubPageContext<TContext>` through the constructor, which forwards
the navigation arguments to the base view model.
