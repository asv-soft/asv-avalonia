# Dialogs and Notifications

## Modal Window

We will use the `DialogPrefab` mechanism from **Asv.Avalonia** to create modal windows that can be invoked directly from the ViewModel.

Place the `RecipeEditDialogViewModel` in the `Dialogs` directory of your project.

```c#
// RecipeEditDialogViewModel.cs

using System.Collections.Generic;
using Asv.Avalonia;
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace RecipeBook.ViewModels;

public class RecipeEditDialogViewModel : DialogViewModelBase
{
	public const string DialogId = $"{BaseId}.recipe_edit";

    public RecipeEditDialogViewModel(ILoggerFactory loggerFactory)
        : base(DialogId, loggerFactory)
    {
        Title = new BindableReactiveProperty<string?>().DisposeItWith(Disposable);
        Category = new BindableReactiveProperty<string?>().DisposeItWith(Disposable);
    }

    public BindableReactiveProperty<string?> Title { get; }
    public BindableReactiveProperty<string?> Category { get; }

    public override IEnumerable<IRoutable> GetChildren()
    {
        return [];
    }
}
```

Define `RecipeEditDialogPrefab` to handle data exchange with the dialog, enabling both payload injection and result retrieval.

```c#
// RecipeEditDialogPrefab.cs

using System.Composition;
using System.Threading.Tasks;
using Asv.Avalonia;
using Microsoft.Extensions.Logging;

namespace RecipeBook.ViewModels;

public sealed class RecipeEditDialogPayload
{
	public required string Title { get; init; }
	public required string Category { get; init; }
}

[ExportDialogPrefab]
[Shared]
[method: ImportingConstructor]
public sealed class RecipeEditDialogPrefab(INavigationService nav, ILoggerFactory loggerFactory)
	: IDialogPrefab<RecipeEditDialogPayload, RecipeEditDialogPayload?>
{
	public async Task<RecipeEditDialogPayload?> ShowDialogAsync(RecipeEditDialogPayload dialogPayload)
    {
        using var vm = new RecipeEditDialogViewModel(loggerFactory);

        vm.Title.Value = dialogPayload.Title;
        vm.Category.Value = dialogPayload.Category;

        var dialogContent = new ContentDialog(vm, nav)
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
			 xmlns:viewModels="clr-namespace:RecipeBook.ViewModels"
			 mc:Ignorable="d"
			 d:DesignWidth="800"
			 d:DesignHeight="450"
			 x:Class="RecipeBook.Views.RecipeEditDialogView"
			 x:DataType="viewModels:RecipeEditDialogViewModel">

	<StackPanel>
		<TextBlock Text="Recipe Title" />
		<TextBox Text="{CompiledBinding Title.Value}" />

		<TextBlock Text="Category" />
		<TextBox Text="{CompiledBinding Category.Value}" />
	</StackPanel>
</UserControl>
```

```c#
//RecipeEditDialogView.axaml.cs

using Asv.Avalonia;
using Avalonia.Controls;
using RecipeBook.ViewModels;

namespace RecipeBook.Views;

[ExportViewFor(typeof(RecipeEditDialogViewModel))]
public partial class RecipeEditDialogView : UserControl
{
	public RecipeEditDialogView()
	{
		InitializeComponent();
	}
}
```

Inside `RecipePageViewModel`, add a field to store the dialog prefab retrieved from the dialog service,
along with a command to create a new recipe.

```c#
    private readonly RecipeEditDialogPrefab _recipeEditDialog;
   	public ReactiveCommand CreateRecipeCommand { get; }
```

Initialize the command and retrieve `DialogPrefab` in `RecipePageViewModel` constructor.

```c#
      _recipeEditDialog = dialogService.GetDialogPrefab<RecipeEditDialogPrefab>();
	  CreateRecipeCommand = new ReactiveCommand(CreateRecipeAsync).DisposeItWith(Disposable);
```

Define the handler for the recipe creation process.

```c#
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

Overview of recipe adding

![recipe-add](recipe-book-app-adding-recipe.png)

![recipe-dialog](recipe-book-app-dialog.png)

![added-recipe](recipe-book-app-overview-1.png)

## Notifications

Let's implement the notification system, starting with a confirmation message when an ingredient is added.
But first, we need to ensure we can create recipes.

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

```c#
    public ReactiveCommand CreateIngredientCommand { get; }
```

Add an initialization in `RecipeViewModel` constructor:

```c#
	CreateIngredientCommand = new ReactiveCommand(AddIngredientAsync).DisposeItWith(Disposable);
```

Handle ingredient addition and send a toast notification:

```c#
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

		await this.RaiseShellInfoMessage(msg, cancellationToken);
	}
```

Notification on Recipe Creation

![notification](recipe-book-app-notification.png)

![huge-notification](recipe-book-app-huge-notification.png)

## Events

### Removing Ingredients

We need to notify the parent viewmodel that an ingredient has been removed.
To achieve this, create a `RemoveIngredientEvent` in the **Events** directory at **Pages** directory.

```c#
// RemoveIngredientEvent.cs

using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Common;

namespace RecipeBook.Events;

public sealed class RemoveIngredientEvent(IRoutable source) : AsyncRoutedEvent<IRoutable>(source, RoutingStrategy.Bubble);

public static class RemoveIngredientEventMixin
{
	public static ValueTask RequestRemoveIngredient(this IRoutable src, CancellationToken cancel = default)
	{
		return src.Rise(new RemoveIngredientEvent(src), cancel);
	}
}
```

Add the `DeleteIngredientCommand` to the `IngredientViewModel`.

```c#
	public ReactiveCommand DeleteIngredientCommand { get; }
```

Initialize the command in the constructor:

```c#
...
    public IngredientViewModel(string id, string name, string amount, ILoggerFactory loggerFactory)
		: base(new NavigationId(BaseId, id), loggerFactory)
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

```c#
	private async ValueTask RemoveIngredientAsync(Unit unit, CancellationToken cancellationToken)
	{
		await this.RequestRemoveIngredient(cancellationToken);
	}
```

Intercept the bubbling events by implementing `InternalCatchEvent` in the `RecipeViewModel`.

```c#
private ValueTask InternalCatchEvent(IRoutable src, AsyncRoutedEvent<IRoutable> e)
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

Subscribe to events in `RecipeViewModel` constructor.

```c#
...
	public RecipeViewModel(string id, string title, string? category, string? instruction, 
        IEnumerable<IngredientViewModel> ingredients, ILoggerFactory loggerFactory)
		: base(new NavigationId(BaseId, id), loggerFactory)
	{
        ...
	    CreateIngredientCommand = new ReactiveCommand(AddIngredientAsync).DisposeItWith(Disposable);
        
        // here we subscribe to events
		Events.Subscribe(InternalCatchEvent).DisposeItWith(Disposable);
	}
...
```

Add the delete button to **RecipePageView.axaml**.

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