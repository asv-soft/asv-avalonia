# Recipe Book App

In this tutorial, we will build a **Recipe Book** application that allows users to add and edit recipes. 
You will learn how to implement features for editing cooking instructions and managing ingredient lists, 
complete with **Undo/Redo** support for text editing.

The goal of this guide is to demonstrate the core capabilities of the **Asv.Avalonia** framework through a practical 
example, covering:
- **Pages** (Application entry point and navigation).
- **Historical Properties** (Undo/Redo mechanics).
- **Dialogs** (Modal windows for data input).
- **Notifications** (Toast messages).
- **Layout Service** (Persisting and restoring UI state).

## Project Setup
We will skip the initial application initialization steps by using the boilerplate code from the [](project-setup.md) guide.
For a more detailed explanation of Pages, please refer to the [](pages.md) documentation.

### Running the Application

Let's launch the project to verify the setup.

![project](recipe-book-app-start.png)


## Adding a Page

Create the `RecipePageViewModel` and `RecipePageView` in `Pages` directory.

```c#
// RecipePageViewModel.cs

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

Define the XAML layout for `RecipePageView`, dividing it into two main sections: a left sidebar for the recipe list and 
a right editor area for the recipe description and ingredients.

```XML
<!-- RecipePageView.axaml -->

<UserControl xmlns="https://github.com/avaloniaui"
	xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
	xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
	xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
	xmlns:pages="clr-namespace:RecipeBook.Pages"
	mc:Ignorable="d"
	d:DesignWidth="800"
	d:DesignHeight="450"
	x:Class="RecipeBook.Views.RecipePageView"
	x:DataType="pages:RecipePageViewModel">

	<Grid Margin="0,30,0,0"
		ColumnDefinitions="Auto,Auto,*">

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
// RecipePageView.axaml.cs

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

Implement the command to open the Recipe Book page. Put **OpenRecipePageCommand.cs** into **Commands** folder.

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

![dummy-page](recipe-book-app-page-initial.png)

## Application Core

Let's create the fundamental components of the Recipe Book: the recipe and its ingredient list. 
We will utilize **Historical properties** to enable built-in **Undo/Redo** support. 

Create the `IngredientViewModel`, which supports Undo/Redo operations for editing 
the ingredient name and amount. Place this file in the **Pages** directory, alongside the other source files.

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
    public const string BaseId = "ingredient";
    
	private ReactiveProperty<string?> _name;
	private ReactiveProperty<string?> _amount;

	public IngredientViewModel(string id, string amount, ILoggerFactory loggerFactory)
		: base(new NavigationId(BaseId, id), loggerFactory)
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

Define the `RecipeViewModel` in the **Recipes** directory to represent a recipe, including its title, category, and ingredient list.

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
    public const string BaseId = "recipe";
	private readonly ILoggerFactory _loggerFactory;
	private readonly ReactiveProperty<string?> _title;
	private readonly ReactiveProperty<string?> _category;
	private readonly ReactiveProperty<string?> _instruction;

	private readonly ObservableList<IngredientViewModel> _ingredients = [];

	public RecipeViewModel(string id, string title, string? category, string? instruction, IEnumerable<IngredientViewModel> ingredients,
		ILoggerFactory loggerFactory)
		: base(new NavigationId(BaseId, id), loggerFactory)
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

Add a new field in `RecipePageViewModel` to store the collection of recipes.

```c#
	private ObservableList<RecipeViewModel> _recipes { get; } = [];
```

Initialize the observable list in the RecipePageViewModel constructor and wrap it for XAML data binding.
We also need a property to track the currently selected recipe, which will display its cooking instructions and ingredient list.

In the `RecipePageViewModel` constructor, add:

```c#
...
    SelectedRecipe = new BindableReactiveProperty<RecipeViewModel?>();
    _recipes.SetRoutableParent(this).DisposeItWith(Disposable);
    _recipes.DisposeRemovedItems().DisposeItWith(Disposable);
    Recipes = _recipes.ToNotifyCollectionChanged().DisposeItWith(Disposable);
...
```

Define properties:

```c#
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

Add recipe description and ingredient list on the right panel. Place it right after Recipe title XAML: 

```xml
<!-- Right panel (Editor) -->
<Border Grid.Column="2"
        Background="#1E1E1E"
        Padding="30">
    <Grid>
        <Grid RowDefinitions="Auto, *">
            <!-- Recipe title -->
            ...
        </Grid>
        
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
</Border>
```
