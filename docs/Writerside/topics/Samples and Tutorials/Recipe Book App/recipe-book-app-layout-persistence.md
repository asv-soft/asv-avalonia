# Layout Persistence

For user convenience, it would be great to remember the currently edited recipe and display it when opening the page.
For such tasks as saving user changes related to application page customization in the framework,
there is a Layout service. Saving works similarly to `IConfiguration` in the **data** folder,
but to the **layouts.json** file.

We'll only save the `NavigationId`, which we'll use to search among the recipes loaded from config.

Declare `RecipePageViewModelLayoutConfig` in the **RecipePageViewModel.cs** file. In the `SelectedRecipe` property,
we'll save the `NavigationId` serialized as a string:

```c#
public class RecipePageViewModelLayoutConfig
{
	public string? SelectedRecipe { get; set; }
}
```

Add field with the layout config to `RecipePageViewModel`:
```c#
private RecipePageViewModelLayoutConfig? _layoutConfig;
```

The Layout service is available in any `IRoutable` inheritor via the `HandleLoadLayout<T>` extension method,
but how do we know when the user wants to save the page appearance? Fortunately, the application window already has
layout-saving functionality available, and all we need to do is handle two events: `SaveLayoutEvent` and `LoadLayoutEvent`.
We'll process them the same way we handled ingredient deletion.

Subscribe the `InternalCatchEvent` event handler in the `RecipePageViewModel` constructor:

```c#
	Events.Subscribe(InternalCatchEvent).DisposeItWith(Disposable);
```

Implement `InternalCatchEvent`. When receiving `SaveLayoutEvent`, we'll save the ID of the selected recipe from the
`SelectedRecipe` property, and when receiving `LoadLayoutEvent`, we'll read the saved ID and search among the recipes.

```c#
	private ValueTask InternalCatchEvent(IRoutable src, AsyncRoutedEvent<IRoutable> e)
	{
		switch (e)
		{
			case SaveLayoutEvent saveLayoutEvent:
				if (_layoutConfig is null)
				{
					break;
				}

				this.HandleSaveLayout(
					saveLayoutEvent,
					_layoutConfig,
					cfg => { cfg.SelectedRecipe = SelectedRecipe.Value?.Id.ToString(); }
				);
				break;
			case LoadLayoutEvent loadLayoutEvent:
				_layoutConfig = this.HandleLoadLayout<RecipePageViewModelLayoutConfig>(
					loadLayoutEvent,
					cfg =>
					{
						var t = _recipes.FirstOrDefault(x => x.Id.ToString() == (cfg.SelectedRecipe ?? string.Empty));
						SelectedRecipe.Value = t;
					}
				);
				break;
		}

		return default;
	}
```

Now we see the recipe we were working with before closing the page.

![save-layout](recipe-book-app-save-layout.png)