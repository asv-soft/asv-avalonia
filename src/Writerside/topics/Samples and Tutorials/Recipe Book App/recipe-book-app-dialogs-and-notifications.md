# Dialogs and Notifications

## Modal Window

We will use the `DialogPrefab` mechanism from **Asv.Avalonia** to create modal windows that can be invoked directly from the ViewModel.

Place the dialog files in the `Shell/Pages/Recipes/Dialogs` directory of your project.

```C#
// RecipeEditDialogViewModel.cs

using Asv.Avalonia;
using Asv.Common;
using R3;

namespace AsvAvaloniaTest;

public class RecipeEditDialogViewModel : DialogViewModelBase
{
    public const string DialogId = $"{BaseId}.recipe_edit";

    public RecipeEditDialogViewModel()
        : base(DialogId)
    {
        Title = new BindableReactiveProperty<string?>().DisposeItWith(Disposable);
        Category = new BindableReactiveProperty<string?>().DisposeItWith(Disposable);
    }

    public BindableReactiveProperty<string?> Title { get; }
    public BindableReactiveProperty<string?> Category { get; }
}
```

Define `RecipeEditDialogPrefab` to handle data exchange with the dialog, enabling both payload injection and result retrieval.

```C#
// RecipeEditDialogPrefab.cs

using System.Threading.Tasks;
using Asv.Avalonia;

namespace AsvAvaloniaTest;

public sealed class RecipeEditDialogPayload
{
    public required string Title { get; init; }
    public required string Category { get; init; }
}

public sealed class RecipeEditDialogPrefab
    : IDialogPrefab<RecipeEditDialogPayload, RecipeEditDialogPayload?>
{
    public async Task<RecipeEditDialogPayload?> ShowDialogAsync(RecipeEditDialogPayload dialogPayload)
    {
        using var vm = new RecipeEditDialogViewModel();

        vm.Title.Value = dialogPayload.Title;
        vm.Category.Value = dialogPayload.Category;

        var dialogContent = new ContentDialog(vm)
        {
            Title = dialogPayload.Title,
            PrimaryButtonText = RS.DialogButton_Yes,
            SecondaryButtonText = RS.DialogButton_No,
            DefaultButton = ContentDialogButton.Primary,
        };

        var result = await dialogContent.ShowAsync();
        if (result != ContentDialogResult.Primary)
        {
            return null;
        }

        return new RecipeEditDialogPayload
        {
            Title = vm.Title.Value,
            Category = vm.Category.Value
        };
    }
}
```

Design the `RecipeEditDialogView` layout to allow users to input the recipe title and category during creation.

```xml
 <!-- RecipeEditDialogView.axaml -->

<UserControl xmlns="https://github.com/avaloniaui"
			 xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
			 xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
			 xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
			 xmlns:local="clr-namespace:AsvAvaloniaTest"
			 mc:Ignorable="d"
			 d:DesignWidth="800"
			 d:DesignHeight="450"
			 x:Class="AsvAvaloniaTest.RecipeEditDialogView"
			 x:DataType="local:RecipeEditDialogViewModel">

	<StackPanel>
		<TextBlock Text="Recipe Title" />
		<TextBox Text="{CompiledBinding Title.Value}" />

		<TextBlock Text="Category" />
		<TextBox Text="{CompiledBinding Category.Value}" />
	</StackPanel>
</UserControl>
```

```C#
// RecipeEditDialogView.axaml.cs

using Avalonia.Controls;

namespace AsvAvaloniaTest;

public partial class RecipeEditDialogView : UserControl
{
	public RecipeEditDialogView()
	{
		InitializeComponent();
	}
}
```

Keep the dialog wiring with the Recipe Page feature. Add these registrations to
`RecipePageRegistrations.RegisterRecipePage` after the page and Home Page extension registrations:

```C#
// Shell/Pages/Recipes/RecipePageRegistrations.cs

builder.AppBuilder.Dialogs.RegisterPrefab<RecipeEditDialogPrefab>();
builder.AppBuilder.ViewLocator.RegisterViewFor<
    RecipeEditDialogViewModel,
    RecipeEditDialogView
>();
```

Inside `RecipePageViewModel`, add a field to store the dialog prefab retrieved from the dialog service,
along with a command to create a new recipe.

```C#
    private readonly RecipeEditDialogPrefab _recipeEditDialog;
   	public ReactiveCommand CreateRecipeCommand { get; }
```

Initialize the command and retrieve `DialogPrefab` in the `RecipePageViewModel` constructor.

```C#
      _recipeEditDialog = dialogService.GetDialogPrefab<RecipeEditDialogPrefab>();
	  CreateRecipeCommand = new ReactiveCommand(CreateRecipeAsync).DisposeItWith(Disposable);
```

Define the handler for the recipe creation process.

```C#
// RecipePageViewModel.cs

	private async ValueTask CreateRecipeAsync(Unit unit, CancellationToken cancellationToken)
	{
		var payload = new RecipeEditDialogPayload
		{
			Title = "Recipe Title",
			Category = "Category",
		};

		var createdRecipePayload = await _recipeEditDialog.ShowDialogAsync(payload);

		if (createdRecipePayload == null)
		{
			return;
		}

		var recipeViewModel = new RecipeViewModel(
            Guid.NewGuid().ToString(),
			createdRecipePayload.Title,
			createdRecipePayload.Category,
			string.Empty,
			[],
			_loggerFactory
        );

		_recipes.Add(recipeViewModel);
		SelectedRecipe.Value = recipeViewModel;
	}
```

Place the "Add Recipe" button immediately after the recipe list container within the sidebar `RecipePageView`.

```xml
<!-- Recipe list -->
...
<!-- Add recipe -->
<Border Grid.Row="2"
	Padding="15"
	BorderBrush="#333"
	BorderThickness="0,1,0,0">
	<Button HorizontalAlignment="Stretch"
		Background="#3C3C3C"
		HorizontalContentAlignment="Center"
		Command="{Binding CreateRecipeCommand}"
		CornerRadius="4">
		<StackPanel Orientation="Horizontal"
			Spacing="8">
			<TextBlock Text="+"
				FontWeight="Bold"
				Foreground="#4CC2FF" />
			<TextBlock Text="Add Recipe"
				Foreground="#DDD" />
		</StackPanel>
	</Button>
</Border>
...
```

The recipe creation flow looks like this:

![recipe-add](recipe-book-app-adding-recipe.png)

![recipe-dialog](recipe-book-app-dialog.png)

![added-recipe](recipe-book-app-overview-1.png)

## Notifications

Let's implement the notification system, starting with a confirmation message when an ingredient is added.
But first, we need to ensure we can create ingredients.

The "Add Ingredient" button:

```xml
...
<StackPanel Spacing="10">
	<!-- Ingredient list -->
	<ItemsControl ItemsSource="{Binding SelectedRecipe.Value.Ingredients}">
    ...
	</ItemsControl>

	<!-- Add ingredient -->
	<Button HorizontalAlignment="Stretch"
		Background="Transparent"
		BorderBrush="#333"
		BorderThickness="1"
		CornerRadius="4"
	    Command="{Binding  SelectedRecipe.Value.CreateIngredientCommand}"
		Padding="10">
		<StackPanel Orientation="Horizontal"
			HorizontalAlignment="Center"
			Spacing="6">
			<TextBlock Text="+"
				Foreground="#555" />
			<TextBlock Text="Add Ingredient"
				Foreground="#666"
				FontSize="12" />
		</StackPanel>
	</Button>
</StackPanel>
...
```

Add the ingredient creation command to `RecipeViewModel`:

```C#
    public ReactiveCommand CreateIngredientCommand { get; }
```

Add an initialization in the `RecipeViewModel` constructor:

```C#
	CreateIngredientCommand = new ReactiveCommand(AddIngredientAsync).DisposeItWith(Disposable);
```

Handle ingredient addition and send a toast notification:

```C#
	public async ValueTask AddIngredientAsync(Unit unit, CancellationToken cancellationToken)
	{
		var ingredient = new IngredientViewModel(
            Guid.NewGuid().ToString(),
            "Ingredient",
            string.Empty,
            _loggerFactory
        );
		_ingredients.Add(ingredient);

		var msg = new ShellMessage(
			"Added ingredient",
			"Ingredient was created",
			ShellErrorState.Normal,
			"This is description",
			MaterialIconKind.Info
		);

		await this.RiseShellInfoMessage(msg, cancellationToken);
	}
```

> The `RiseShellInfoMessage` extension method lives in the `Asv.Avalonia.InfoMessage` namespace — don't forget
> the corresponding `using` directive. The snippets omit repeating `using` lists; you can always check the full
> files in the [source code](recipe-book-app-source-code.md).
> {style="note"}

Under the hood, `RiseShellInfoMessage` rises a routed event that bubbles up the view model tree to the shell,
which displays the message as a toast notification.

The ingredient creation notification appears in the lower-right corner:

![notification](recipe-book-app-notification.png)

## Events

### Removing Ingredients

We need to notify the parent viewmodel that an ingredient has been removed.
To achieve this, create `RemoveIngredientEvent.cs` in the
`Shell/Pages/Recipes/Events` directory.

```C#
// RemoveIngredientEvent.cs

using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Modeling;

namespace AsvAvaloniaTest;

public sealed class RemoveIngredientEvent(IViewModel source)
    : AsyncRoutedEvent<IViewModel>(source, RoutingStrategy.Bubble);

public static class RemoveIngredientEventMixin
{
    public static ValueTask RequestRemoveIngredient(
        this IViewModel src,
        CancellationToken cancel = default
    )
    {
        return src.Rise(new RemoveIngredientEvent(src), cancel);
    }
}
```

The event uses the `Bubble` routing strategy: it travels from the source up the view model tree, so any ancestor
can intercept it.

Add the `DeleteIngredientCommand` to the `IngredientViewModel`.

```C#
	public ReactiveCommand DeleteIngredientCommand { get; }
```

Initialize the command in the constructor:

```C#
...
    public IngredientViewModel(string id, string name, string amount, ILoggerFactory loggerFactory)
        : base(BaseId, new NavArgs(new KeyValuePair<string, string?>("id", id)))
	{
        ...
		Amount = new HistoricalStringProperty(
                nameof(Amount),
                _amount,
                loggerFactory
            ).SetRoutableParent(this)
            .DisposeItWith(Disposable);

        // new command
		DeleteIngredientCommand = new ReactiveCommand(RemoveIngredientAsync).DisposeItWith(Disposable);
	}
...
```

Raise the event using the extension method defined in `RemoveIngredientEvent`.

```C#
	private async ValueTask RemoveIngredientAsync(Unit unit, CancellationToken cancellationToken)
	{
		await this.RequestRemoveIngredient(cancellationToken);
	}
```

Intercept the bubbling events by implementing `InternalCatchEvent` in the `RecipeViewModel`.

```C#
	private ValueTask InternalCatchEvent(IViewModel src, AsyncRoutedEvent<IViewModel> e, CancellationToken cancel)
	{
		if (e is not RemoveIngredientEvent)
		{
			return default;
		}

		var vm = _ingredients.First(i => i.Id == e.Sender.Id);
		_ingredients.Remove(vm);

		return default;
	}
```

The `Sender` property of the event gives us the ingredient that requested its own removal, and we find it in the
list by its unique `NavId`.

Subscribe to events in the `RecipeViewModel` constructor.

```C#
...
	public RecipeViewModel(string id, string title, string? category, string? instruction,
        IEnumerable<IngredientViewModel> ingredients, ILoggerFactory loggerFactory)
        : base(BaseId, new NavArgs(new KeyValuePair<string, string?>("id", id)))
	{
        ...
	    CreateIngredientCommand = new ReactiveCommand(AddIngredientAsync).DisposeItWith(Disposable);

        // here we subscribe to events
		Events.Catch(InternalCatchEvent).DisposeItWith(Disposable);
	}
...
```

Add the delete button to `RecipePageView.axaml`.

```xml
...
<!-- Ingredient list -->
<ItemsControl ItemsSource="{Binding SelectedRecipe.Value.Ingredients}">
    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <Border Background="#252526"
                    CornerRadius="4"
                    Margin="0,0,0,8"
                    Padding="10">
                <Grid ColumnDefinitions="*, 10, 80, Auto">
                    ...
                    <!-- Delete -->
                    <Button Grid.Column="3"
                            Margin="5,0,0,0"
                            Background="Transparent"
                            Foreground="#666"
                            Command="{Binding DeleteIngredientCommand}"
                            Padding="5">
                        <TextBlock Text="×"
                                   FontSize="18"
                                   Margin="0,-3,0,0" />
                    </Button>
                </Grid>
            </Border>
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
...
```
