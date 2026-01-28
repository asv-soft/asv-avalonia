# Recipe Book App

In this tutorial, we will build a **Recipe Book** application that allows users to add and edit recipes. You will learn how to implement features for editing cooking instructions and managing ingredient lists, complete with **Undo/Redo** support for text editing.

The goal of this guide is to demonstrate the core capabilities of the **Asv.Avalonia** framework through a practical example, covering:
- **Pages** (Application entry point and navigation).
- **Historical Properties** (Undo/Redo mechanics).
- **Dialogs** (Modal windows for data input).
- **Notifications** (Toast messages).
- **Layout Service** (Persisting and restoring UI state).

## Project Setup
We will skip the initial application initialization steps by using the boilerplate code from the [Get Started](project-setup.md) guide. For a more detailed explanation of Pages, please refer to the [Adding pages](pages.md) documentation.

### Project Structure

![project](recipe-book-app-initial.png)

### Running the Application

Let's launch the empty project to verify the setup.

![project](recipe-book-app-start.png)


## Adding a Page

Create the `RecipePageViewModel` and `RecipePageView`.

```c#
using System.Collections.Generic;
using System.Composition;
using Asv.Avalonia;
using Material.Icons;
using Microsoft.Extensions.Logging;

namespace RecipeBook.ViewModels;

public interface IRecipePageViewModel : IPage;

[ExportPage(PageId)]
public class RecipePageViewModel : PageViewModel<IRecipePageViewModel>, IRecipePageViewModel
{
	private readonly ILoggerFactory _loggerFactory;
	public const string PageId = "recipe_page";
	public const MaterialIconKind PageIcon = MaterialIconKind.ViewGallery;

	[ImportingConstructor]
	public RecipePageViewModel(ICommandService cmd, ILoggerFactory loggerFactory, IDialogService dialogService)
		: base(PageId, cmd, loggerFactory, dialogService)
	{
		_loggerFactory = loggerFactory;
	}

	public override IEnumerable<IRoutable> GetChildren()
	{
		return [];
	}

	protected override void AfterLoadExtensions() { }

	public override IExportInfo Source => SystemModule.Instance;
}
```

Define the XAML layout, dividing it into two main sections: a left sidebar for the recipe list and a right editor area
for the recipe description and ingredients.

```XML
<UserControl xmlns="https://github.com/avaloniaui"
	xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
	xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
	xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
	xmlns:vm="clr-namespace:RecipeBook.ViewModels"
	mc:Ignorable="d"
	d:DesignWidth="800"
	d:DesignHeight="450"
	x:Class="RecipeBook.Views.RecipePageView"
	x:DataType="vm:RecipePageViewModel">

	<Grid Margin="0,30,0,0">

		<Grid.ColumnDefinitions>
			<ColumnDefinition Width="300" />
			<ColumnDefinition Width="Auto" />
			<ColumnDefinition Width="*" />
		</Grid.ColumnDefinitions>

		<!-- Left panel (Sidebar) -->
		<Grid Grid.Column="0"
			RowDefinitions="Auto, *, Auto"
			Background="#252526">

		</Grid>

		<GridSplitter Grid.Column="1"
			Background="Black"
			Width="5" />

		<!-- Right panel (Editor) -->
		<Border Grid.Column="2"
			Background="#1E1E1E"
			Padding="30">
	
		</Border>

	</Grid>
</UserControl>
```

Add the necessary attributes for MEF2 registration in the code-behind file `RecipePageView.axaml.cs`.

```c#
using Asv.Avalonia;
using Avalonia.Controls;
using RecipeBook.ViewModels;

namespace RecipeBook.Views;

[ExportViewFor(typeof(RecipePageViewModel))]
public partial class RecipePageView : UserControl
{
	public RecipePageView()
	{
		InitializeComponent();
	}
}
```

Register the components using an extension method and add a command to open the page.

```c#
// HomePageRecipeExtension.cs

using System.Composition;
using Asv.Avalonia;
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;
using RecipeBook.ViewModels.Commands;

namespace RecipeBook.ViewModels;

[ExportExtensionFor<IHomePage>]
[method: ImportingConstructor]
public class HomePageRecipeExtension(ILoggerFactory loggerFactory)
	: AsyncDisposableOnce,
		IExtensionFor<IHomePage>
{
	public void Extend(IHomePage context, CompositeDisposable contextDispose)
	{
		context.Tools.Add(
			OpenRecipePageCommand
				.StaticInfo.CreateAction(
					loggerFactory,
					"Recipe Book",
					"Open recipes"
				)
				.DisposeItWith(contextDispose)
		);
	}
}
```

Implement the command to open the Recipe Book page.

```c#
// OpenRecipePageCommand.cs 

using System.Composition;
using Asv.Avalonia;

namespace RecipeBook.ViewModels.Commands;

[ExportCommand]
[method: ImportingConstructor]
public class OpenRecipePageCommand(INavigationService nav)
	: OpenPageCommandBase(RecipePageViewModel.PageId, nav)
{
	public override ICommandInfo Info => StaticInfo;

	public const string Id = $"{BaseId}.open.{RecipePageViewModel.PageId}";

	public static readonly ICommandInfo StaticInfo = new CommandInfo
	{
		Id = Id,
		Name = "Recipe Book",
		Description = "Open recipes",
		Icon = RecipePageViewModel.PageIcon,
		DefaultHotKey = null,
		Source = SystemModule.Instance,
	};
}
```

Project structure

![solution](recipe-book-app-solution-1.png)

### Run App

A corresponding menu item has appeared on the Home Page.

![page](recipe-book-app-page.png)

The application currently looks like this:

![dummy-page](recipe-book-app-recipe-page-dummy.png)

## Application Core

Let's create the fundamental components of the Recipe Book: the recipe and its ingredient list. 
We will utilize **Historical properties** to enable built-in **Undo/Redo** support.

Create the `IngredientViewModel`, which supports Undo/Redo operations for editing the ingredient name and amount.

```c#
// IngredientViewModel.cs

using System.Collections.Generic;
using Asv.Avalonia;
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace RecipeBook.ViewModels;

public class IngredientViewModel : RoutableViewModel
{
	private ReactiveProperty<string?> _name;
	private ReactiveProperty<string?> _amount;

	public IngredientViewModel(string name, string amount, ILoggerFactory loggerFactory)
		: base(NavigationId.GenerateRandom(), loggerFactory)
	{
		_name = new ReactiveProperty<string?>(name);
		Name = new HistoricalStringProperty(
				nameof(Name),
				_name,
				loggerFactory
			).SetRoutableParent(this)
			.DisposeItWith(Disposable);

		_amount = new ReactiveProperty<string?>(amount);
		Amount = new HistoricalStringProperty(
				nameof(Amount),
				_amount,
				loggerFactory
			).SetRoutableParent(this)
			.DisposeItWith(Disposable);
	}

	public HistoricalStringProperty Name { get; }
	public HistoricalStringProperty Amount { get; }

	public override IEnumerable<IRoutable> GetChildren()
	{
		yield return Name;
		yield return Amount;
	}
}
```

Define the `RecipeViewModel` to represent a recipe, including its title, category, and ingredient list.

```c#
// RecipeViewModel.cs

using System.Collections.Generic;
using Asv.Avalonia;
using Asv.Common;
using Asv.IO;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace RecipeBook.ViewModels;

public class RecipeViewModel : RoutableViewModel
{
	private readonly ILoggerFactory _loggerFactory;
	public const string BaseId = "recipe";

	private readonly ReactiveProperty<string?> _title;
	private readonly ReactiveProperty<string?> _category;
	private readonly ReactiveProperty<string?> _instruction;

	private readonly ObservableList<IngredientViewModel> _ingredients = [];

	public RecipeViewModel(int id, string title, string? category, string? instruction, IEnumerable<IngredientViewModel> ingredients,
		ILoggerFactory loggerFactory)
		: base(new NavigationId(BaseId, id.ToString()), loggerFactory)
	{
		_loggerFactory = loggerFactory;

		_title = new ReactiveProperty<string?>(title).DisposeItWith(Disposable);
		Title = new HistoricalStringProperty(
				nameof(Title),
				_title,
				loggerFactory
			).SetRoutableParent(this)
			.DisposeItWith(Disposable);

		_category = new ReactiveProperty<string?>(category).DisposeItWith(Disposable);
		Category = new HistoricalStringProperty(
				nameof(Category),
				_category,
				loggerFactory
			).SetRoutableParent(this)
			.DisposeItWith(Disposable);

		_instruction = new ReactiveProperty<string?>(instruction).DisposeItWith(Disposable);
		Instruction = new HistoricalStringProperty(
				nameof(Instruction),
				_instruction,
				loggerFactory
			).SetRoutableParent(this)
			.DisposeItWith(Disposable);

		_ingredients.AddRange(ingredients);
		_ingredients.SetRoutableParent(this).DisposeItWith(Disposable);
		_ingredients.DisposeRemovedItems().DisposeItWith(Disposable);

		Ingredients = _ingredients.ToNotifyCollectionChangedSlim(SynchronizationContextCollectionEventDispatcher.Current)
			.DisposeItWith(Disposable);
	}

	public HistoricalStringProperty Title { get; }
	public HistoricalStringProperty Category { get; }
	public HistoricalStringProperty Instruction { get; }
	public NotifyCollectionChangedSynchronizedViewList<IngredientViewModel> Ingredients { get; }

	public override IEnumerable<IRoutable> GetChildren()
	{
		foreach (var ingredient in _ingredients)
		{
			yield return ingredient;
		}

		yield return Title;
		yield return Category;
		yield return Instruction;
	}
}
```

Update `RecipePageViewModel` to manage the recipe list.

Add a new field to store the collection of recipes.

```c#
public class RecipePageViewModel : PageViewModel<IRecipePageViewModel>, IRecipePageViewModel
{
	private readonly ILoggerFactory _loggerFactory;
	private readonly RecipeEditDialogPrefab _recipeEditDialog;
	public const string PageId = "recipe_page";
	public const MaterialIconKind PageIcon = MaterialIconKind.ViewGallery;
	
	private ObservableList<RecipeViewModel> _recipes { get; } = [];
```

Initialize the observable list in the RecipePageViewModel constructor and wrap it for XAML data binding.
We also need a property to track the currently selected recipe, which will display its cooking instructions and ingredient list.

```c#
	[ImportingConstructor]
	public RecipePageViewModel(ICommandService cmd, ILoggerFactory loggerFactory, IDialogService dialogService)
		: base(PageId, cmd, loggerFactory, dialogService)
	{
		_loggerFactory = loggerFactory;
		SelectedRecipe = new BindableReactiveProperty<RecipeViewModel?>();

		_recipes.SetRoutableParent(this).DisposeItWith(Disposable);
		_recipes.DisposeRemovedItems().DisposeItWith(Disposable);

		Recipes = _recipes.ToNotifyCollectionChanged().DisposeItWith(Disposable);

	}

	public NotifyCollectionChangedSynchronizedViewList<RecipeViewModel> Recipes { get; }
    
    public BindableReactiveProperty<RecipeViewModel?> SelectedRecipe { get; }


```

Finally, implement the framework's routing mechanism to ensure proper event propagation.

```c#
	public override IEnumerable<IRoutable> GetChildren()
	{
		foreach (var recipe in _recipes)
		{
			yield return recipe;
		}
	}
```

### Main Page Layout

Update the `RecipePageView` XAML to display the list of recipes and the selected recipe details.

Add the recipe list to the left sidebar.

```xml
<!-- Left panel (Sidebar) -->
<Grid Grid.Column="0"
	RowDefinitions="Auto, *, Auto"
	Background="#252526">

	<!-- Recipe list -->
	<ListBox Grid.Row="1"
		ItemsSource="{Binding Recipes}"
		SelectedItem="{Binding SelectedRecipe.Value}"
		Background="Transparent"
		SelectionMode="Single"
		Padding="0,0,0,10">
		<ListBox.ItemTemplate>
			<DataTemplate>
				<!-- Recipe description -->
				<StackPanel Margin="12,0,0,0"
					VerticalAlignment="Center">
					<TextBlock Text="{Binding Title.ViewValue.Value}"
						FontWeight="SemiBold"
						Foreground="#DDD"
						TextTrimming="CharacterEllipsis" />
					<TextBlock Text="{Binding Category.ViewValue.Value}"
						FontSize="11"
						Foreground="#888" />
				</StackPanel>
			</DataTemplate>
		</ListBox.ItemTemplate>
	</ListBox>
</Grid>
```

Recipe Title

```xml
<!-- Right panel (Editor) -->
<Border Grid.Column="2"
	Background="#1E1E1E"
	Padding="30">
	<Grid>

		<Grid RowDefinitions="Auto, *">

			<!-- Recipe title -->
			<StackPanel Grid.Row="0"
				Margin="0,0,0,25">
				<TextBlock Text="{Binding SelectedRecipe.Value.Title.ViewValue.Value}"
					FontSize="28"
					FontWeight="Bold"
					Foreground="White" />
				<StackPanel Orientation="Horizontal"
					Spacing="10"
					Margin="0,5,0,0">
					<Border Background="#333"
						CornerRadius="3"
						Padding="6,2">
						<TextBlock Text="{Binding SelectedRecipe.Value.Category.ViewValue.Value}"
							FontSize="12"
							Foreground="#AAA" />
					</Border>
				</StackPanel>

			</StackPanel>

		</Grid>

	</Grid>
</Border>
```

Recipe Description and Ingredients

```xml
<!-- Right panel (Editor) -->
<Border Grid.Column="2"
	Background="#1E1E1E"
	Padding="30">
	<Grid>

		<Grid RowDefinitions="Auto, *">

			<!-- Recipe title -->

			<!-- Instructions (Left) | Ingredients (Right) -->
			<Grid Grid.Row="1"
				ColumnDefinitions="*, 320">

				<DockPanel Grid.Row="0"
					Margin="0,0,30,0">
					<TextBlock Text="INSTRUCTIONS"
						DockPanel.Dock="Top"
						FontSize="12"
						FontWeight="Bold"
						Foreground="#666"
						Margin="0,0,0,10" />
					<TextBox Text="{Binding SelectedRecipe.Value.Instruction.ViewValue.Value}"
						AcceptsReturn="True"
						TextWrapping="Wrap"
						Background="#252526"
						BorderThickness="0"
						Padding="15"
						CornerRadius="6"
						VerticalAlignment="Stretch"
						VerticalContentAlignment="Top"
						Foreground="#CCC" />
				</DockPanel>


				<DockPanel Grid.Column="1"
					Margin="0,0,30,0">
					<TextBlock Text="INGREDIENTS"
						DockPanel.Dock="Top"
						FontSize="12"
						FontWeight="Bold"
						Foreground="#666"
						Margin="0,0,0,10" />

					<ScrollViewer>
						<StackPanel Spacing="10">
							<!-- Ingredient list -->
							<ItemsControl ItemsSource="{Binding SelectedRecipe.Value.Ingredients}">
								<ItemsControl.ItemTemplate>
									<DataTemplate>
										<Border Background="#252526"
											CornerRadius="4"
											Margin="0,0,0,8"
											Padding="10">
											<Grid ColumnDefinitions="*, 10, 80, Auto">
												<!-- Name -->
												<TextBox Grid.Column="0"
													Text="{Binding Name.ViewValue.Value}"
													Background="Transparent"
													BorderThickness="0"
													Foreground="#DDD"
													Watermark="Name" />

												<!-- Amount -->
												<TextBox Grid.Column="2"
													Text="{Binding Amount.ViewValue.Value}"
													Background="#333"
													BorderThickness="0"
													CornerRadius="3"
													Foreground="#AAA"
													HorizontalContentAlignment="Center"
													Watermark="Qty" />

											</Grid>
										</Border>
									</DataTemplate>
								</ItemsControl.ItemTemplate>
							</ItemsControl>

						</StackPanel>
					</ScrollViewer>
				</DockPanel>
			</Grid>


		</Grid>

	</Grid>
</Border>
```

## Dialogs and Notifications

### Modal Window

We will use the `DialogPrefab` mechanism from **Asv.Avalonia** to create modal windows that can be invoked directly from the ViewModel.

Place the `RecipeEditDialogViewModel` in the `ViewModels` directory of your project.

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

	private ReactiveProperty<string?> _title;
	private ReactiveProperty<string?> _category;

	public RecipeEditDialogViewModel(ILoggerFactory loggerFactory)
		: base(DialogId, loggerFactory)
	{
		_title = new ReactiveProperty<string?>().DisposeItWith(Disposable);
		Title = new HistoricalStringProperty(
				nameof(Title),
				_title,
				loggerFactory
			)
			.SetRoutableParent(this)
			.DisposeItWith(Disposable);

		_category = new ReactiveProperty<string?>().DisposeItWith(Disposable);
		Category = new HistoricalStringProperty(
				nameof(Category),
				_category,
				loggerFactory
			)
			.SetRoutableParent(this)
			.DisposeItWith(Disposable);
	}

	public HistoricalStringProperty Title { get; }
	public HistoricalStringProperty Category { get; }

	public override IEnumerable<IRoutable> GetChildren()
	{
		yield return Title;
		yield return Category;
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

		vm.Title.ViewValue.Value = dialogPayload.Title;
		vm.Category.ViewValue.Value = dialogPayload.Category;

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
			Title = vm.Title.ViewValue.Value,
			Category = vm.Category.ViewValue.Value
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
		<TextBox Text="{CompiledBinding Title.ViewValue.Value}" />

		<TextBlock Text="Category" />
		<TextBox Text="{CompiledBinding Category.ViewValue.Value}" />
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

Initialize the command and retrieve `DialogPrefab` in the constructor.

```c#
	[ImportingConstructor]
	public RecipePageViewModel(ICommandService cmd, ILoggerFactory loggerFactory, IDialogService dialogService)
		: base(PageId, cmd, loggerFactory, dialogService)
	{
	

       	_recipeEditDialog = dialogService.GetDialogPrefab<RecipeEditDialogPrefab>();
		CreateRecipeCommand = new ReactiveCommand(CreateRecipeAsync).DisposeItWith(Disposable);
```

Define the handler for the recipe creation process.

```c#
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
			Random.Shared.Next(),
			createdRecipePayload.Title,
			createdRecipePayload.Category,
			string.Empty,
			[],
			_loggerFactory);

		_recipes.Add(recipeViewModel);
		SelectedRecipe.Value = recipeViewModel;
	}
```

Place the "Add Recipe" button immediately after the recipe list container within the sidebar.

```xml
<!-- Recipe list -->

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
```

Overview of recipe adding

![recipe-add](recipe-book-app-adding-recipe.png)

![recipe-dialog](recipe-book-app-dialog.png)

![added-recipe](recipe-book-app-overview-1.png)

### Notifications

Adding an Ingredient

The "Add Ingredient" button:

```xml
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
		Command="{Binding CreateIngredientCommand}"
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
```

Add the ingredient creation command to `RecipePageViewModel` 

```c#
    public ReactiveCommand CreateIngredientCommand { get; }
```

Initialization:

```c#
	public RecipePageViewModel(ICommandService cmd, ILoggerFactory loggerFactory, IDialogService dialogService)
		: base(PageId, cmd, loggerFactory, dialogService)
	{
        // ...
	
		CreateIngredientCommand = new ReactiveCommand(CreateIngredientAsync).DisposeItWith(Disposable);
```

Handler 

```c#
	private ValueTask CreateIngredientAsync(Unit unit, CancellationToken cancellationToken)
	{
		SelectedRecipe.Value?.AddIngredient();

		return default;
	}
```

Add the `AddIngredient` method to the `RecipeViewModel`, which sends a toast notification.

```c#
	public async Task AddIngredient()
	{
		var ingredient = new IngredientViewModel("Ingredient", string.Empty, _loggerFactory);
		_ingredients.Add(ingredient);

		var msg = new ShellMessage(
			"Added ingredient",
			"Ingredient was created",
			ShellErrorState.Normal,
			"This is description",
			MaterialIconKind.Info
		);
		
		await this.RaiseShellInfoMessage(msg, CancellationToken.None); 
	}
```

Notification on Recipe Creation

![notification](recipe-book-app-notification.png)

![huge-notification](recipe-book-app-huge-notification.png)

### Events

#### Removing Ingredients

We need to notify the parent viewmodel that an ingredient has been removed. 
To achieve this, create a `RemoveIngredientEvent` in the **Events** directory at the project root.

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
	public static ValueTask RemoveIngredient(this IRoutable src, CancellationToken cancel = default)
	{
		return src.Rise(new RemoveIngredientEvent(src), cancel);
	}
}
```

Add the `DeleteIngredientCommand` to the `IngredientViewModel`.

```c#
	public HistoricalStringProperty Name { get; }
	public HistoricalStringProperty Amount { get; }
    
	public ReactiveCommand DeleteIngredientCommand { get; }
```

Initialize the command:

```c#
	public IngredientViewModel(string name, string amount, ILoggerFactory loggerFactory)
		: base(NavigationId.GenerateRandom(), loggerFactory)
	{
		// ...
	
		DeleteIngredientCommand = new ReactiveCommand(RiseRemoveEvent).DisposeItWith(Disposable);
	}
```

Raise the event using the extension method defined in `RemoveIngredientEvent`.

```c#
	private async void RiseRemoveEvent(Unit unit)
	{
		await this.RemoveIngredient();
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
	public RecipeViewModel(int id, string title, string? category, string? instruction, IEnumerable<IngredientViewModel> ingredients,
		ILoggerFactory loggerFactory)
		: base(new NavigationId(BaseId, id.ToString()), loggerFactory)
	{
	    // ...

		Events.Subscribe(InternalCatchEvent).DisposeItWith(Disposable);
	}
```

Add the delete control to **RecipePageView.axaml**.

```xml
						<!-- Ingredient list -->
									<ItemsControl ItemsSource="{Binding SelectedRecipe.Value.Ingredients}">
										<ItemsControl.ItemTemplate>
											<DataTemplate>
                                                
												<!-- Name -->

												<!-- Amount -->

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
```

## Layout Persistence

We will implement the ability to save the width of the recipe list sidebar using the **Layout Service**.

First, define the `RecipePageViewModelConfig` class, 
which will be used for serializing and storing the settings within `RecipePageViewModel`.

```c#
public class RecipePageViewModelConfig
{
	public double ColumnWidth { get; set; } = 250;
}
```

In `RecipePageViewModel`, declare properties to read/write configuration values and bind them to the sidebar column width.

```c#
	private RecipePageViewModelConfig? _config;
	private readonly SynchronizedReactiveProperty<double> _columnWidth;
    public BindableReactiveProperty<double> ColumnWidth { get; }
```

Initialize these properties and subscribe to the layout load/save events (the handler implementation will follow).

```c#
	public RecipePageViewModel(ICommandService cmd, ILoggerFactory loggerFactory, IDialogService dialogService)
		: base(PageId, cmd, loggerFactory, dialogService)
	{
	    // ...

		_columnWidth = new SynchronizedReactiveProperty<double>().DisposeItWith(Disposable);
		ColumnWidth = _columnWidth.ToBindableReactiveProperty().DisposeItWith(Disposable);
        
        Events.Subscribe(InternalCatchEvent).DisposeItWith(Disposable);
	}
```

Implement the event handler for `SaveLayoutEvent` and `LoadLayoutEvent`.

```c#
	private ValueTask InternalCatchEvent(IRoutable src, AsyncRoutedEvent<IRoutable> e)
	{
		switch (e)
		{
			case SaveLayoutEvent saveLayoutEvent:
				if (_config is null)
				{
					break;
				}

				this.HandleSaveLayout(
					saveLayoutEvent,
					_config,
					cfg => { cfg.ColumnWidth = ColumnWidth.Value; }
				);
				break;
			case LoadLayoutEvent loadLayoutEvent:
				_config = this.HandleLoadLayout<RecipePageViewModelConfig>(
					loadLayoutEvent,
					cfg => { _columnWidth.Value = cfg.ColumnWidth; }
				);
				break;
		}

		return default;
	}
```

Update the `RecipePageView` XAML to bind the column width to the ColumnWidth property.

```xml
<UserControl xmlns="https://github.com/avaloniaui"
			 xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
			 xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
			 xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
			 xmlns:vm="clr-namespace:RecipeBook.ViewModels"
			 xmlns:avalonia="clr-namespace:Asv.Avalonia;assembly=Asv.Avalonia"
			 mc:Ignorable="d"
			 d:DesignWidth="800"
			 d:DesignHeight="450"
			 x:Class="RecipeBook.Views.RecipePageView"
			 x:DataType="vm:RecipePageViewModel">

	<Grid Margin="0,30,0,0">

		<Grid.ColumnDefinitions>
			<ColumnDefinition Width="{Binding ColumnWidth.Value, Mode=TwoWay, 
                                 Converter={x:Static vm:GridSizeConverter.Instance}}" />
			<ColumnDefinition Width="Auto" />
			<ColumnDefinition Width="*" />
		</Grid.ColumnDefinitions>
```

You will need a `GridSizeConverter` to handle the conversion between `GridLength` and `double`.
Create a `GridSizeConverter.cs` file in the **ViewModels** directory.

```c#
// GridSizeConverter.cs

using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace RecipeBook.ViewModels;

public class GridSizeConverter : IValueConverter
{
	public static GridSizeConverter Instance { get; } = new();

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		return new GridLength((double)value!, GridUnitType.Pixel);
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		return ((GridLength)value!).Value;
	}
}
```

Now, when you adjust the layout, the column width will be persisted and restored upon restarting the application, 
instead of resetting to the default value.

![save-layout](recipe-book-app-save-layout.png)

## Recipe Search

We will utilize the framework's `SearchBoxViewModel` control to filter the list of recipes by name.
The `SearchBoxViewModel` simplifies search implementation by providing built-in features such as **debouncing**.

Add a view field to `RecipePageViewModel` to manage the filtered list.


```c#
	private readonly ISynchronizedView<RecipeViewModel, RecipeViewModel> _view;
    public SearchBoxViewModel Search { get; }
```

Update `RecipePageViewModel` constructor. 
The recipes will now be processed through a synchronized View and its attached filter, rather than exposing the raw list directly.

```c#
	public RecipePageViewModel(ICommandService cmd, ILoggerFactory loggerFactory, IDialogService dialogService)
		: base(PageId, cmd, loggerFactory, dialogService)
	{
	    // ...
        // Recipes = _recipes.ToNotifyCollectionChanged().DisposeItWith(Disposable);
        // ...

		Search = new SearchBoxViewModel(
				nameof(Search),
				loggerFactory,
				UpdateRecipeList,
				TimeSpan.FromMilliseconds(500)
			)
			.SetRoutableParent(this)
			.DisposeItWith(Disposable);
        
        	_view = _recipes.CreateView(x => x);
           	Recipes = _view.ToNotifyCollectionChanged().DisposeItWith(Disposable);
	}
```

Update `GetChildren` to include the search component in the routing tree, as `SearchBoxViewModel` implements `IRoutable`.

```c#
	public override IEnumerable<IRoutable> GetChildren()
	{
		yield return Search;

		foreach (var recipe in _recipes)
		{
			yield return recipe;
		}
	}
```

Implement `UpdateRecipeList` handler to apply the search filter.

```c#
	private Task UpdateRecipeList(string? text, IProgress<double> progress, CancellationToken cancel)
	{
		progress.Report(0);

		if (string.IsNullOrWhiteSpace(text))
		{
			_view.ResetFilter();
			return Task.CompletedTask;
		}

		_view.AttachFilter(x => x.Title.ViewValue.Value!.Contains(text));

		progress.Report(1);

		return Task.CompletedTask;
	}

```

Add the search control to the `RecipePageView` XAML.

```xml
		<!-- Left panel (Sidebar) -->
		<Grid Grid.Column="0"
			  RowDefinitions="Auto, *, Auto"
			  Background="#252526">

			<avalonia:SearchBoxView DockPanel.Dock="Right"
									Margin="5"
									DataContext="{Binding Search}" />
```

Additionally, hide the right-hand editor panel until a recipe is selected.

```xml
<!-- Right panel (Editor) -->
<Border Grid.Column="2"
	Background="#1E1E1E"
	Padding="30">
	<Grid>
		<Grid RowDefinitions="Auto, *"
			IsVisible="{Binding SelectedRecipe.Value, Converter={x:Static ObjectConverters.IsNotNull}}">

```
Recipe Filtering

![filtration-1](recipe-book-app-filter-1.png)

![filtration-2](recipe-book-app-filter-2.png)

UI Overview

![final](recipe-book-app-final.png)

## Source code

![final-structure](recipe-book-app-final-project-structure.png)

```c#
using Avalonia;
using System;
using System.IO;
using System.Reflection;
using Asv.Avalonia;
using Avalonia.Controls;

namespace RecipeBook;

sealed class Program
{
	public static void Main(string[] args)
	{
		var builder = AppHost.CreateBuilder(args);
		var dataFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

		builder
			.UseAvalonia(BuildAvaloniaApp)

			// This setting defines where all app data (like a JSON user config) will be stored
			.UseAppPath(opt => opt.WithRelativeFolder(Path.Combine(dataFolder, "data")))

			// Here you can define some JSON config settings. For example, we set autosave to 1 second
			.UseJsonUserConfig(opt => opt.WithAutoSave(TimeSpan.FromSeconds(1)))

			// This defines the source of app data (app name, version, etc.). We use the current assembly
			.UseAppInfo(opt => opt.FillFromAssembly(typeof(App).Assembly))

			// Here we set up the logging system
			.UseLogging(options =>
			{
				options.WithLogToFile();
				options.WithLogToConsole();

				// Optional: here you can enable Log viewer page
				options.WithLogViewer();
			});

		using var host = builder.Build();
		host.StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
	}

	// Avalonia configuration, don't remove; also used by visual designer.
	public static AppBuilder BuildAvaloniaApp() =>
		AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.WithInterFont()
			.LogToTrace()
			.UseR3();
}
```

{collapsible="true" collapsed-title="Program.cs"}

```c#
using System;
using System.Composition;
using System.Composition.Convention;
using System.Composition.Hosting;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Asv.Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using R3;

namespace RecipeBook;

public class App : Application, IContainerHost, IShellHost
{
	private readonly CompositionHost _container;
	private readonly Subject<IShell> _onShellLoaded = new();
	private IShell _shell;

	public App()
	{
		var conventions = new ConventionBuilder();
		var containerCfg = new ContainerConfiguration();

		containerCfg
			.WithDependenciesFromSystemModule()
			.WithDependenciesFromTheApp(this)
			.WithDefaultConventions(conventions);

		_container = containerCfg.CreateContainer();

		DataTemplates.Add(new CompositionViewLocator(_container));

		if (!Design.IsDesignMode)
			_container.GetExport<IAppStartupService>().AppCtor();
	}

	public T GetExport<T>()
		where T : IExportable
	{
		return _container.GetExport<T>();
	}

	public T GetExport<T>(string contract)
		where T : IExportable
	{
		return _container.GetExport<T>(contract);
	}

	public bool TryGetExport<T>(string id, out T value)
		where T : IExportable
	{
		return _container.TryGetExport(id, out value);
	}

	public void SatisfyImports(object value)
	{
		_container.SatisfyImports(value);
	}

	public IExportInfo Source => SystemModule.Instance;

	public IShell Shell
	{
		get => _shell;
		private set
		{
			_shell = value;
			_onShellLoaded.OnNext(value);
		}
	}

	public Observable<IShell> OnShellLoaded => _onShellLoaded;
	public TopLevel TopLevel { get; private set; }

	public override void Initialize()
	{
		AvaloniaXamlLoader.Load(this);
		if (!Design.IsDesignMode)
			_container.GetExport<IAppStartupService>().Initialize();
	}

	public override void OnFrameworkInitializationCompleted()
	{
		if (Design.IsDesignMode)
		{
			Shell = DesignTimeShellViewModel.Instance;
		}
		else if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			Shell = _container.GetExport<IShell>(DesktopShellViewModel.ShellId);
			if (desktop.MainWindow is TopLevel topLevel)
				TopLevel = topLevel;
		}
		else if (Current?.ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
		{
			Shell = _container.GetExport<IShell>(MobileShellViewModel.ShellId);
			if (singleViewPlatform.MainView is TopLevel topLevel)
				TopLevel = topLevel;
		}
		else
		{
			throw new Exception("Unknown platform");
		}

		base.OnFrameworkInitializationCompleted();
#if DEBUG
		this.AttachDevTools();
#endif
		if (!Design.IsDesignMode)
			_container.GetExport<IAppStartupService>().OnFrameworkInitializationCompleted();
	}
}

public static class ContainerConfigurationMixin
{
	public static ContainerConfiguration WithDependenciesFromTheApp(this ContainerConfiguration containerConfiguration,
		App app)
	{
		containerConfiguration.WithExport<IDataTemplateHost>(app).WithExport<IShellHost>(app);

		if (Design.IsDesignMode)
			containerConfiguration.WithExport(NullContainerHost.Instance);
		else
			containerConfiguration.WithExport<IContainerHost>(app);

		return containerConfiguration.WithAssemblies([app.GetType().Assembly]);
	}
}
```

{collapsible="true" collapsed-title="App.axaml.cs"}

```xml
<Application xmlns="https://github.com/avaloniaui"
			 xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
			 x:Class="RecipeBook.App"
			 RequestedThemeVariant="Default">
	<Application.Styles>
		<StyleInclude Source="avares://Asv.Avalonia/Styling/Theme.axaml" />
	</Application.Styles>
</Application>
```

{collapsible="true" collapsed-title="App.axaml"}

```xml
<UserControl xmlns="https://github.com/avaloniaui"
	xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
	xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
	xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
	xmlns:vm="clr-namespace:RecipeBook.ViewModels"
	xmlns:avalonia="clr-namespace:Asv.Avalonia;assembly=Asv.Avalonia"
	mc:Ignorable="d"
	d:DesignWidth="800"
	d:DesignHeight="450"
	x:Class="RecipeBook.Views.RecipePageView"
	x:DataType="vm:RecipePageViewModel">

	<Grid Margin="0,30,0,0">

		<Grid.ColumnDefinitions>
			<ColumnDefinition Width="{Binding ColumnWidth.Value, Mode=TwoWay, 
                                 Converter={x:Static vm:GridSizeConverter.Instance}}" />
			<ColumnDefinition Width="Auto" />
			<ColumnDefinition Width="*" />
		</Grid.ColumnDefinitions>

		<!-- Left panel (Sidebar) -->
		<Grid Grid.Column="0"
			RowDefinitions="Auto, *, Auto"
			Background="#252526">

			<avalonia:SearchBoxView DockPanel.Dock="Right"
				Margin="5"
				DataContext="{Binding Search}" />

			<!-- Recipe list -->
			<ListBox Grid.Row="1"
				ItemsSource="{Binding Recipes}"
				SelectedItem="{Binding SelectedRecipe.Value}"
				Background="Transparent"
				SelectionMode="Single"
				Padding="0,0,0,10">
				<ListBox.ItemTemplate>
					<DataTemplate>
						<!-- Recipe description -->
						<StackPanel Margin="12,0,0,0"
							VerticalAlignment="Center">
							<TextBlock Text="{Binding Title.ViewValue.Value}"
								FontWeight="SemiBold"
								Foreground="#DDD"
								TextTrimming="CharacterEllipsis" />
							<TextBlock Text="{Binding Category.ViewValue.Value}"
								FontSize="11"
								Foreground="#888" />
						</StackPanel>
					</DataTemplate>
				</ListBox.ItemTemplate>
			</ListBox>

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

		</Grid>
		<GridSplitter Grid.Column="1"
			Background="Black"
			Width="5" />

		<!-- Right panel (Editor) -->
		<Border Grid.Column="2"
			Background="#1E1E1E"
			Padding="30">
			<Grid>

				<Grid RowDefinitions="Auto, *"
					IsVisible="{Binding SelectedRecipe.Value, Converter={x:Static ObjectConverters.IsNotNull}}">

					<!-- Recipe title -->
					<StackPanel Grid.Row="0"
						Margin="0,0,0,25">
						<TextBlock Text="{Binding SelectedRecipe.Value.Title.ViewValue.Value}"
							FontSize="28"
							FontWeight="Bold"
							Foreground="White" />
						<StackPanel Orientation="Horizontal"
							Spacing="10"
							Margin="0,5,0,0">
							<Border Background="#333"
								CornerRadius="3"
								Padding="6,2">
								<TextBlock Text="{Binding SelectedRecipe.Value.Category.ViewValue.Value}"
									FontSize="12"
									Foreground="#AAA" />
							</Border>
						</StackPanel>
					</StackPanel>

					<!-- Instructions (Left) | Ingredients (Right) -->
					<Grid Grid.Row="1"
						ColumnDefinitions="*, 320">

						<DockPanel Grid.Row="0"
							Margin="0,0,30,0">
							<TextBlock Text="INSTRUCTIONS"
								DockPanel.Dock="Top"
								FontSize="12"
								FontWeight="Bold"
								Foreground="#666"
								Margin="0,0,0,10" />
							<TextBox Text="{Binding SelectedRecipe.Value.Instruction.ViewValue.Value}"
								AcceptsReturn="True"
								TextWrapping="Wrap"
								Background="#252526"
								BorderThickness="0"
								Padding="15"
								CornerRadius="6"
								VerticalAlignment="Stretch"
								VerticalContentAlignment="Top"
								Foreground="#CCC" />
						</DockPanel>


						<DockPanel Grid.Column="1"
							Margin="0,0,30,0">
							<TextBlock Text="INGREDIENTS"
								DockPanel.Dock="Top"
								FontSize="12"
								FontWeight="Bold"
								Foreground="#666"
								Margin="0,0,0,10" />

							<ScrollViewer>
								<StackPanel Spacing="10">
									<!-- Ingredient list -->
									<ItemsControl ItemsSource="{Binding SelectedRecipe.Value.Ingredients}">
										<ItemsControl.ItemTemplate>
											<DataTemplate>
												<Border Background="#252526"
													CornerRadius="4"
													Margin="0,0,0,8"
													Padding="10">
													<Grid ColumnDefinitions="*, 10, 80, Auto">
														<!-- Name -->
														<TextBox Grid.Column="0"
															Text="{Binding Name.ViewValue.Value}"
															Background="Transparent"
															BorderThickness="0"
															Foreground="#DDD"
															Watermark="Name" />

														<!-- Amount -->
														<TextBox Grid.Column="2"
															Text="{Binding Amount.ViewValue.Value}"
															Background="#333"
															BorderThickness="0"
															CornerRadius="3"
															Foreground="#AAA"
															HorizontalContentAlignment="Center"
															Watermark="Qty" />

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

									<!-- Add ingredient -->
									<Button HorizontalAlignment="Stretch"
										Background="Transparent"
										BorderBrush="#333"
										BorderThickness="1"
										CornerRadius="4"
										Command="{Binding CreateIngredientCommand}"
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
							</ScrollViewer>
						</DockPanel>
					</Grid>

				</Grid>

			</Grid>
		</Border>

	</Grid>
</UserControl>
```

{collapsible="true" collapsed-title="RecipePageView.axaml"}

```c#
using Asv.Avalonia;
using Avalonia.Controls;
using RecipeBook.ViewModels;

namespace RecipeBook.Views;

[ExportViewFor(typeof(RecipePageViewModel))]
public partial class RecipePageView : UserControl
{
	public RecipePageView()
	{
		InitializeComponent();
	}
}
```

{collapsible="true" collapsed-title="RecipePageView.axaml.cs"}

```xml
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
		<TextBox Text="{CompiledBinding Title.ViewValue.Value}" />

		<TextBlock Text="Category" />
		<TextBox Text="{CompiledBinding Category.ViewValue.Value}" />
	</StackPanel>
</UserControl>
```

{collapsible="true" collapsed-title="RecipeEditDialogView.axaml"}


```c#
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

{collapsible="true" collapsed-title="RecipeEditDialogView.axaml.cs"}

```c#
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Common;

namespace RecipeBook.Events;

public sealed class RemoveIngredientEvent(IRoutable source) : AsyncRoutedEvent<IRoutable>(source, RoutingStrategy.Bubble);

public static class RemoveIngredientEventMixin
{
	public static ValueTask RemoveIngredient(this IRoutable src, CancellationToken cancel = default)
	{
		return src.Rise(new RemoveIngredientEvent(src), cancel);
	}
}
```

{collapsible="true" collapsed-title="RemoveIngredientEvent.cs"}

```c#
using System.Composition;
using Asv.Avalonia;

namespace RecipeBook.ViewModels.Commands;

[ExportCommand]
[method: ImportingConstructor]
public class OpenRecipePageCommand(INavigationService nav)
	: OpenPageCommandBase(RecipePageViewModel.PageId, nav)
{
	public override ICommandInfo Info => StaticInfo;

	public const string Id = $"{BaseId}.open.{RecipePageViewModel.PageId}";

	public static readonly ICommandInfo StaticInfo = new CommandInfo
	{
		Id = Id,
		Name = "Recipe Book",
		Description = "Open recipes",
		Icon = RecipePageViewModel.PageIcon,
		DefaultHotKey = null,
		Source = SystemModule.Instance,
	};
}
```

{collapsible="true" collapsed-title="OpenRecipePageCommand.cs"}


```c#
using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace RecipeBook.ViewModels;

public class GridSizeConverter : IValueConverter
{
	public static GridSizeConverter Instance { get; } = new();

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		return new GridLength((double)value!, GridUnitType.Pixel);
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		return ((GridLength)value!).Value;
	}
}
```

{collapsible="true" collapsed-title="GridSizeConverter.cs"}


```c#
using System.Composition;
using Asv.Avalonia;
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;
using RecipeBook.ViewModels.Commands;

namespace RecipeBook.ViewModels;

[ExportExtensionFor<IHomePage>]
[method: ImportingConstructor]
public class HomePageRecipeExtension(ILoggerFactory loggerFactory)
	: AsyncDisposableOnce,
		IExtensionFor<IHomePage>
{
	public void Extend(IHomePage context, CompositeDisposable contextDispose)
	{
		context.Tools.Add(
			OpenRecipePageCommand
				.StaticInfo.CreateAction(
					loggerFactory,
					"Recipe Book",
					"Open recipes"
				)
				.DisposeItWith(contextDispose)
		);
	}
}
```

{collapsible="true" collapsed-title="HomePageRecipeExtension.cs"}


```c#
using System.Collections.Generic;
using Asv.Avalonia;
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;
using RecipeBook.Events;

namespace RecipeBook.ViewModels;

public class IngredientViewModel : RoutableViewModel
{
	private ReactiveProperty<string?> _name;
	private ReactiveProperty<string?> _amount;

	public IngredientViewModel(string name, string amount, ILoggerFactory loggerFactory)
		: base(NavigationId.GenerateRandom(), loggerFactory)
	{
		_name = new ReactiveProperty<string?>(name);
		Name = new HistoricalStringProperty(
				nameof(Name),
				_name,
				loggerFactory
			).SetRoutableParent(this)
			.DisposeItWith(Disposable);

		_amount = new ReactiveProperty<string?>(amount);
		Amount = new HistoricalStringProperty(
				nameof(Amount),
				_amount,
				loggerFactory
			).SetRoutableParent(this)
			.DisposeItWith(Disposable);

		DeleteIngredientCommand = new ReactiveCommand(RiseRemoveEvent).DisposeItWith(Disposable);
	}

	public HistoricalStringProperty Name { get; }
	public HistoricalStringProperty Amount { get; }

	public ReactiveCommand DeleteIngredientCommand { get; }

	public override IEnumerable<IRoutable> GetChildren()
	{
		yield return Name;
		yield return Amount;
	}

	private async void RiseRemoveEvent(Unit unit)
	{
		await this.RemoveIngredient();
	}
}
```

{collapsible="true" collapsed-title="IngredientViewModel.cs"}

```c#
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

		vm.Title.ViewValue.Value = dialogPayload.Title;
		vm.Category.ViewValue.Value = dialogPayload.Category;

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
			Title = vm.Title.ViewValue.Value,
			Category = vm.Category.ViewValue.Value
		};
	}
}
```

{collapsible="true" collapsed-title="RecipeEditDialogPrefab.cs"}


```c#
using System.Collections.Generic;
using Asv.Avalonia;
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace RecipeBook.ViewModels;

public class RecipeEditDialogViewModel : DialogViewModelBase
{
	public const string DialogId = $"{BaseId}.recipe_edit";

	private ReactiveProperty<string?> _title;
	private ReactiveProperty<string?> _category;

	public RecipeEditDialogViewModel(ILoggerFactory loggerFactory)
		: base(DialogId, loggerFactory)
	{
		_title = new ReactiveProperty<string?>().DisposeItWith(Disposable);
		Title = new HistoricalStringProperty(
				nameof(Title),
				_title,
				loggerFactory
			)
			.SetRoutableParent(this)
			.DisposeItWith(Disposable);

		_category = new ReactiveProperty<string?>().DisposeItWith(Disposable);
		Category = new HistoricalStringProperty(
				nameof(Category),
				_category,
				loggerFactory
			)
			.SetRoutableParent(this)
			.DisposeItWith(Disposable);
	}

	public HistoricalStringProperty Title { get; }
	public HistoricalStringProperty Category { get; }

	public override IEnumerable<IRoutable> GetChildren()
	{
		yield return Title;
		yield return Category;
	}
}
```

{collapsible="true" collapsed-title="RecipeEditDialogViewModel.cs"}


```c#
using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Common;
using Asv.IO;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace RecipeBook.ViewModels;

public interface IRecipePageViewModel : IPage;

public class RecipePageViewModelConfig
{
	public double ColumnWidth { get; set; } = 250;
}

[ExportPage(PageId)]
public class RecipePageViewModel : PageViewModel<IRecipePageViewModel>, IRecipePageViewModel
{
	private readonly ILoggerFactory _loggerFactory;
	public const string PageId = "recipe_page";
	public const MaterialIconKind PageIcon = MaterialIconKind.ViewGallery;

	private ObservableList<RecipeViewModel> _recipes { get; } = [];

	private readonly RecipeEditDialogPrefab _recipeEditDialog;

	private RecipePageViewModelConfig? _config;
	private readonly SynchronizedReactiveProperty<double> _columnWidth;
	private readonly ISynchronizedView<RecipeViewModel, RecipeViewModel> _view;

	[ImportingConstructor]
	public RecipePageViewModel(ICommandService cmd, ILoggerFactory loggerFactory, IDialogService dialogService)
		: base(PageId, cmd, loggerFactory, dialogService)
	{
		_loggerFactory = loggerFactory;

		SelectedRecipe = new BindableReactiveProperty<RecipeViewModel?>();

		_recipes.SetRoutableParent(this).DisposeItWith(Disposable);
		_recipes.DisposeRemovedItems().DisposeItWith(Disposable);

		Recipes = _recipes.ToNotifyCollectionChanged().DisposeItWith(Disposable);

		_recipeEditDialog = dialogService.GetDialogPrefab<RecipeEditDialogPrefab>();
		CreateRecipeCommand = new ReactiveCommand(CreateRecipeAsync).DisposeItWith(Disposable);
		CreateIngredientCommand = new ReactiveCommand(CreateIngredientAsync).DisposeItWith(Disposable);

		_columnWidth = new SynchronizedReactiveProperty<double>().DisposeItWith(Disposable);
		ColumnWidth = _columnWidth.ToBindableReactiveProperty().DisposeItWith(Disposable);

		Events.Subscribe(InternalCatchEvent).DisposeItWith(Disposable);

		Search = new SearchBoxViewModel(
				nameof(Search),
				loggerFactory,
				UpdateRecipeList,
				TimeSpan.FromMilliseconds(500)
			)
			.SetRoutableParent(this)
			.DisposeItWith(Disposable);

		_view = _recipes.CreateView(x => x);
		Recipes = _view.ToNotifyCollectionChanged().DisposeItWith(Disposable);
	}

	public NotifyCollectionChangedSynchronizedViewList<RecipeViewModel> Recipes { get; }

	public BindableReactiveProperty<RecipeViewModel?> SelectedRecipe { get; }

	public ReactiveCommand CreateRecipeCommand { get; }
	public ReactiveCommand CreateIngredientCommand { get; }

	public BindableReactiveProperty<double> ColumnWidth { get; }

	public SearchBoxViewModel Search { get; }

	public override IEnumerable<IRoutable> GetChildren()
	{
		yield return Search;

		foreach (var recipe in _recipes)
		{
			yield return recipe;
		}
	}

	protected override void AfterLoadExtensions() { }

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
			Random.Shared.Next(),
			createdRecipePayload.Title,
			createdRecipePayload.Category,
			string.Empty,
			[],
			_loggerFactory);

		_recipes.Add(recipeViewModel);
		SelectedRecipe.Value = recipeViewModel;
	}

	private ValueTask CreateIngredientAsync(Unit unit, CancellationToken cancellationToken)
	{
		SelectedRecipe.Value?.AddIngredient();

		return default;
	}

	private ValueTask InternalCatchEvent(IRoutable src, AsyncRoutedEvent<IRoutable> e)
	{
		switch (e)
		{
			case SaveLayoutEvent saveLayoutEvent:
				if (_config is null)
				{
					break;
				}

				this.HandleSaveLayout(
					saveLayoutEvent,
					_config,
					cfg => { cfg.ColumnWidth = ColumnWidth.Value; }
				);
				break;
			case LoadLayoutEvent loadLayoutEvent:
				_config = this.HandleLoadLayout<RecipePageViewModelConfig>(
					loadLayoutEvent,
					cfg => { _columnWidth.Value = cfg.ColumnWidth; }
				);
				break;
		}

		return default;
	}

	private Task UpdateRecipeList(string? text, IProgress<double> progress, CancellationToken cancel)
	{
		progress.Report(0);

		if (string.IsNullOrWhiteSpace(text))
		{
			_view.ResetFilter();
			return Task.CompletedTask;
		}

		_view.AttachFilter(x => x.Title.ViewValue.Value!.Contains(text));

		progress.Report(1);

		return Task.CompletedTask;
	}

	public override IExportInfo Source => SystemModule.Instance;
}
```

{collapsible="true" collapsed-title="RecipePageViewModel.cs"}


```c#
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Avalonia.InfoMessage;
using Asv.Common;
using Asv.IO;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;
using RecipeBook.Events;

namespace RecipeBook.ViewModels;

public class RecipeViewModel : RoutableViewModel
{
	private readonly ILoggerFactory _loggerFactory;
	public const string BaseId = "recipe";

	private readonly ReactiveProperty<string?> _title;
	private readonly ReactiveProperty<string?> _category;
	private readonly ReactiveProperty<string?> _instruction;

	private readonly ObservableList<IngredientViewModel> _ingredients = [];

	public RecipeViewModel(int id, string title, string? category, string? instruction, IEnumerable<IngredientViewModel> ingredients,
		ILoggerFactory loggerFactory)
		: base(new NavigationId(BaseId, id.ToString()), loggerFactory)
	{
		_loggerFactory = loggerFactory;

		_title = new ReactiveProperty<string?>(title).DisposeItWith(Disposable);
		Title = new HistoricalStringProperty(
				nameof(Title),
				_title,
				loggerFactory
			).SetRoutableParent(this)
			.DisposeItWith(Disposable);

		_category = new ReactiveProperty<string?>(category).DisposeItWith(Disposable);
		Category = new HistoricalStringProperty(
				nameof(Category),
				_category,
				loggerFactory
			).SetRoutableParent(this)
			.DisposeItWith(Disposable);

		_instruction = new ReactiveProperty<string?>(instruction).DisposeItWith(Disposable);
		Instruction = new HistoricalStringProperty(
				nameof(Instruction),
				_instruction,
				loggerFactory
			).SetRoutableParent(this)
			.DisposeItWith(Disposable);

		_ingredients.AddRange(ingredients);
		_ingredients.SetRoutableParent(this).DisposeItWith(Disposable);
		_ingredients.DisposeRemovedItems().DisposeItWith(Disposable);

		Ingredients = _ingredients.ToNotifyCollectionChangedSlim(SynchronizationContextCollectionEventDispatcher.Current)
			.DisposeItWith(Disposable);

		Events.Subscribe(InternalCatchEvent).DisposeItWith(Disposable);
	}

	public HistoricalStringProperty Title { get; }
	public HistoricalStringProperty Category { get; }
	public HistoricalStringProperty Instruction { get; }
	public NotifyCollectionChangedSynchronizedViewList<IngredientViewModel> Ingredients { get; }

	public async Task AddIngredient()
	{
		var ingredient = new IngredientViewModel("Ingredient", string.Empty, _loggerFactory);
		_ingredients.Add(ingredient);

		var msg = new ShellMessage(
			"Added ingredient",
			"Ingredient was created",
			ShellErrorState.Normal,
			"This is description",
			MaterialIconKind.Info
		);

		await this.RaiseShellInfoMessage(msg, CancellationToken.None);
	}

	public override IEnumerable<IRoutable> GetChildren()
	{
		foreach (var ingredient in _ingredients)
		{
			yield return ingredient;
		}

		yield return Title;
		yield return Category;
		yield return Instruction;
	}

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
}
```

{collapsible="true" collapsed-title="RecipeViewModel.cs"}