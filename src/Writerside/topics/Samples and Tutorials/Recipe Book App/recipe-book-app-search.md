# Recipe Search

We will use the framework's `SearchBoxViewModel` control to filter the list of recipes by name.
The **SearchBox** control is fully integrated into the **Asv.Avalonia** framework and provides the following features:

- **Progress indication** during search operations
- **Cancellation support** for long-running queries
- **Built-in Undo/Redo** for search input
- **Debouncing** to optimize performance while typing

Add a view field to `RecipePageViewModel` to manage the filtered list.

```C#
	private readonly ISynchronizedView<RecipeViewModel, RecipeViewModel> _view;
    public SearchBoxViewModel Search { get; }
```

Update the `RecipePageViewModel` constructor.
The recipes will now be processed through a synchronized View and its attached filter, rather than exposing the raw list directly.

```C#
...
		Search = new SearchBoxViewModel(
				nameof(Search),
				loggerFactory,
				UpdateRecipeList,
				TimeSpan.FromMilliseconds(500)
			)
			.SetRoutableParent(this)
			.DisposeItWith(Disposable);

        _view = _recipes.CreateView(x => x).DisposeItWith(Disposable);
        Recipes = _view.ToNotifyCollectionChanged().DisposeItWith(Disposable);
...
```

> The `Recipes` property was previously initialized with `_recipes.ToNotifyCollectionChanged(...)` —
> replace that line: the bindable collection is now produced from the filtered view.
> {style="note"}

Update `GetChildren` to include the search component in the routing tree — `SearchBoxViewModel` is a regular
view model, so its historical search text participates in Undo/Redo through the same tree.

```C#
	public override IEnumerable<IViewModel> GetChildren()
	{
		yield return Search;

		foreach (var recipe in _recipes)
		{
			yield return recipe;
		}
	}
```

Implement the `UpdateRecipeList` handler to apply the search filter.

```C#
	private Task UpdateRecipeList(string? text, IProgress<double> progress, CancellationToken cancel)
	{
		progress.Report(0);

		if (string.IsNullOrWhiteSpace(text))
		{
			_view.ResetFilter();
			return Task.CompletedTask;
		}

		_view.AttachFilter(x => x.Title.ViewValue.Value != null
			&& x.Title.ViewValue.Value.Contains(text, StringComparison.InvariantCultureIgnoreCase));

		progress.Report(1);

		return Task.CompletedTask;
	}
```

Add the search control to the `RecipePageView` XAML.

```xml
...
		<!-- Left panel (Sidebar) -->
		<Grid Grid.Column="0"
              RowDefinitions="Auto, *, Auto"
              Background="#252526">

			<avalonia:SearchBoxView DockPanel.Dock="Right"
									Margin="5"
									DataContext="{Binding Search}" />
        ...
        </Grid>
...
```

> The `avalonia` XML namespace must be declared in the `UserControl` element:
> `xmlns:avalonia="clr-namespace:Asv.Avalonia;assembly=Asv.Avalonia"`.
> {style="note"}

Additionally, hide the right-hand editor panel until a recipe is selected.

```xml
...
<!-- Right panel (Editor) -->
<Border Grid.Column="2"
        Background="#1E1E1E"
        Padding="30">
	<Grid>
		<Grid RowDefinitions="Auto, *"
			IsVisible="{Binding SelectedRecipe.Value, Converter={x:Static ObjectConverters.IsNotNull}}">
            ...
        </Grid>
    </Grid>
</Border>
...
```
The recipe list is filtered as the search query changes:

![filtration-1](recipe-book-app-filter-1.png)

![filtration-2](recipe-book-app-filter-2.png)

The completed Recipe Book page looks like this:

![final](recipe-book-app-final.png)

You can find the complete source code for the tutorial [here](recipe-book-app-source-code.md)
