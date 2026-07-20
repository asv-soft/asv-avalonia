# Layout Persistence

For user convenience, it would be great to remember the currently edited recipe and display it when opening the page.
For such tasks — saving user changes related to application page customization — every view model has a built-in
`Layout` controller. Unlike `IConfiguration` (which we used for the application data itself), layout data is stored
per page, in the `data/layout` subdirectory of the application working directory.

We'll only save the recipe ID, which we'll use to search among the recipes loaded from the configuration.

Declare `RecipePageViewModelLayoutConfig` in the `RecipePageViewModel.cs` file:

```C#
public class RecipePageViewModelLayoutConfig
{
	public string? SelectedRecipe { get; set; }
}
```

Implementing layout persistence involves three steps:

1. `Layout.Register` registers a named layout entry and returns a *sink* — an object with `SaveAsync` and
   `LoadAsync` methods for this entry. The callback passed to `Register` is invoked when the entry is loaded.
2. To save the layout, we call `SaveAsync` on the sink whenever the selected recipe changes.
3. `Layout.LoadWhenRootAttached` triggers the load of all registered entries as soon as the page is attached
   to the shell.

We place this logic in `AfterLoadExtensions` — at this point the constructor has finished, so the recipes are
already loaded from the configuration. Replace the empty `AfterLoadExtensions` in `RecipePageViewModel`:

```C#
	protected override void AfterLoadExtensions()
	{
		var layoutSink = Layout.Register<RecipePageViewModelLayoutConfig>(
			nameof(RecipePageViewModel),
			(config, _) =>
			{
				SelectedRecipe.Value = _recipes.FirstOrDefault(x => x.RecipeId == config.SelectedRecipe);
				return ValueTask.CompletedTask;
			}
		);
		layoutSink.DisposeItWith(Disposable);

		SelectedRecipe
			.Skip(1)
			.SubscribeAwait(
				(recipe, cancel) => layoutSink.SaveAsync(
					new RecipePageViewModelLayoutConfig { SelectedRecipe = recipe?.RecipeId },
					cancel
				),
				AwaitOperation.Drop
			)
			.DisposeItWith(Disposable);

		Layout.LoadWhenRootAttached(RootTracking).DisposeItWith(Disposable);
	}
```

When the load callback fires, we look up the saved recipe by its ID and select it.
The save subscription starts with `Skip(1)`: a `BindableReactiveProperty` pushes its current value
immediately on subscription, and without `Skip(1)` we would overwrite the stored layout with the initial
`null` selection before the load happens.

Now we see the recipe we were working with before closing the page.
