# Recipe Persistence

Let's add recipe persistence so that users can close the page and restore their data after restarting the application.
We'll use `IConfiguration` to save and restore the recipe list (including nested ingredients) to a `user_settings.json`
located in `data` subdirectory of the application executable.

First, add `IConfiguration` to the constructor arguments:

```c#
	public RecipePageViewModel(ICommandService cmd, IConfiguration configuration, ILoggerFactory loggerFactory, IDialogService dialogService)
		: base(PageId, cmd, loggerFactory, dialogService)
```

Add the configuration field to `RecipePageViewModel`:

```c#
	private readonly IConfiguration _configuration;
```

Initialize it in the constructor:

```c#
	_configuration = configuration;
```

Define `RecipePageViewModelConfig` to store the list of recipes with ingredients. Also create `RecipeDto` and `IngredientDto`.
Add the declarations to the `RecipePageViewModel.cs` file:

```c#
public class RecipePageViewModelConfig
{
	public IEnumerable<RecipeDto> Recipes { get; init; } = [];
}

public record RecipeDto
{
	public required string Id { get; init; }
	public required string Title { get; init;}
	public string? Category { get; init;}
	public  string? Instruction { get; init;}
	public IEnumerable<IngredientDto> Ingredients { get; init; } =  [];
}

public record IngredientDto(string Id, string Name, string? Amount);
```

Add the `Save` method to `RecipePageViewModel`:

```c#
	private void Save()
	{
		var config = new RecipePageViewModelConfig
        {
            Recipes = _recipes.Select(r => new RecipeDto
            {
                Id = r.Id.Args ?? r.Id.ToString(),
                Title = r.Title.ViewValue.Value ?? string.Empty,
                Category = r.Category.ViewValue.Value,
                Instruction = r.Instruction.ViewValue.Value,
                Ingredients = r.Ingredients.Select(i => 
                    new IngredientDto(
                        i.Id.Args ?? i.Id.ToString(), 
                        i.Name.ViewValue.Value ?? string.Empty, 
                        i.Amount.ViewValue.Value ?? string.Empty
                    )
                )
            })
        };

        _configuration.Set(config);
	}
```

Save recipes automatically when adding a new recipe to the list.

Add the `Save` call to `CreateRecipeAsync`:

```c#
...
	private async ValueTask CreateRecipeAsync(Unit unit, CancellationToken cancellationToken)
	{
		...
		Save();
	}
...
```

Add the `Load` method to restore saved recipes:

```c#
	private void Load(RecipePageViewModelConfig recipeConfig)
	{
		var recipeVms = recipeConfig.Recipes.Select(r =>
			new RecipeViewModel(
				r.Id,
				r.Title,
				r.Category,
				r.Instruction,
				r.Ingredients.Select(i => new IngredientViewModel(i.Id, i.Name, i.Amount ?? string.Empty, _loggerFactory)),
				_loggerFactory));

		_recipes.AddRange(recipeVms);
	}
```

Load recipes in the constructor. Add configuration retrieval and call `Load` to convert DTOs to corresponding ViewModels:

```c#
...
	_configuration = configuration;
	var recipeConfig = configuration.Get<RecipePageViewModelConfig>();
	Load(recipeConfig);
...
```

Now, when you close the page or restart the application, recipes will persist and reload correctly.

> Now you can save ingredients only when you add a new recipe.
> This will be fixed later.