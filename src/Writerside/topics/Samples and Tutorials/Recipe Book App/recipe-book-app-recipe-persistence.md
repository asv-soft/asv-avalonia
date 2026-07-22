# Recipe Persistence

Let's add recipe persistence so that users can close the page and restore their data after restarting the application.
We'll use `IConfiguration` to write the recipe list, together with its nested ingredients, and to read it back on startup.
The default implementation stores the configuration in the `profile.json` file
located in the `data` subdirectory of the application working directory.

First, add `IConfiguration` to the constructor arguments:

```C#
public RecipePageViewModel(
    IPageContext context,
    IConfiguration configuration,
    ILoggerFactory loggerFactory,
    IDialogService dialogService,
    IExtensionService ext
)
    : base(PageId, context, loggerFactory, dialogService, ext)
```

> `IConfiguration` comes from the `Asv.Cfg` namespace. The service is registered by `RegisterDefault()`,
> so it can be injected right away.
> {style="note"}

Add the configuration field to `RecipePageViewModel`:

```C#
private readonly IConfiguration _configuration;
```

Define `RecipePageViewModelConfig` to store the list of recipes with ingredients. Also create `RecipeDto` and
`IngredientDto`. Add the declarations to `RecipePageViewModel.cs`:

```C#
public class RecipePageViewModelConfig
{
    public IEnumerable<RecipeDto> Recipes { get; init; } = [];
}

public record RecipeDto
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public string? Category { get; init; }
    public string? Instruction { get; init; }
    public IEnumerable<IngredientDto> Ingredients { get; init; } = [];
}

public record IngredientDto(string Id, string Name, string? Amount);
```

Initialize the field and restore the stored configuration in the constructor:

```C#
_configuration = configuration;
var recipeConfig = configuration.Get<RecipePageViewModelConfig>();
Load(recipeConfig);
```

Add the `Save` method to `RecipePageViewModel`. This is where the `RecipeId` and `IngredientId` properties
we created earlier come into play — they identify recipes and ingredients in the stored configuration:

```C#
private void Save()
{
    var config = new RecipePageViewModelConfig
    {
        Recipes = _recipes.Select(r => new RecipeDto
        {
            Id = r.RecipeId,
            Title = r.Title.ViewValue.Value ?? string.Empty,
            Category = r.Category.ViewValue.Value,
            Instruction = r.Instruction.ViewValue.Value,
            Ingredients = r.Ingredients.Select(i =>
                new IngredientDto(
                    i.IngredientId,
                    i.Name.ViewValue.Value ?? string.Empty,
                    i.Amount.ViewValue.Value ?? string.Empty
                )
            )
        })
    };

    _configuration.Set(config);
}
```

Save recipes automatically when adding a new recipe to the list. Add the `Save` call to `CreateRecipeAsync`:

```C#
private async ValueTask CreateRecipeAsync(Unit unit, CancellationToken cancellationToken)
{
    // Create and add the recipe as shown in the previous chapter.
    // ...
    Save();
}
```

Add the `Load` method to restore saved recipes:

```C#
private void Load(RecipePageViewModelConfig recipeConfig)
{
    var recipeVms = recipeConfig.Recipes.Select(r =>
        new RecipeViewModel(
            r.Id,
            r.Title,
            r.Category,
            r.Instruction,
            r.Ingredients.Select(i =>
                new IngredientViewModel(
                    i.Id,
                    i.Name,
                    i.Amount ?? string.Empty,
                    _loggerFactory
                )
            ),
            _loggerFactory
        )
    );

    _recipes.AddRange(recipeVms);
}
```

Recipes now survive an application restart — but note exactly when the snapshot is written.
`Save` is called from `CreateRecipeAsync` only, so the file is updated at the moment a new recipe is created.
That write stores the current state of **all** recipes, so edits and ingredients you added earlier are captured by it.
Anything changed after the last recipe was created is lost on restart — with a single recipe in the book, that means
only its title and category are ever persisted.

> This keeps the example short. In a real application you would also save when recipe fields change and when
> ingredients are added or removed — for example, by subscribing to those changes and calling `Save` with a throttle.
> {style="note"}
