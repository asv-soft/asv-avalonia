# Source code

```
Asv.Avalonia.Samples.RecipeBook
├── Dependencies/
├── Assets/
├── Commands/
│   └── Recipes/
│       └── OpenRecipePageCommand.cs
├── Pages/
│   ├── Events/
│   │   └── RemoveIngredientEvent.cs
│   ├── Ingredients/
│   │   └── IngredientViewModel.cs
│   └── Recipes/
│       ├── Dialog/
│       │   ├── RecipeEditDialogPrefab.cs
│       │   ├── RecipeEditDialogView.axaml
│       │   ├── RecipeEditDialogView.axaml.cs
│       │   └── RecipeEditDialogViewModel.cs
│       ├── HomePageRecipeExtension.cs
│       ├── RecipePageView.axaml
│       ├── RecipePageView.axaml.cs
│       ├── RecipePageViewModel.cs
│       └── RecipeViewModel.cs
├── App.axaml
├── App.axaml.cs
├── app.manifest
├── Asv.Avalonia.Samples.RecipeBook.csproj.DotSettings
└── Program.cs
```

```c#
using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;

namespace Asv.Avalonia.Samples.RecipeBook;

sealed class Program
{
	[STAThread]
	public static void Main(string[] args)
	{
		try
		{
			BuildAvaloniaApp()
				.StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
			AppHost.Instance.StopAsync().GetAwaiter().GetResult();
			Task.Factory.StartNew(AppHost.Instance.Dispose).GetAwaiter().GetResult();
		}
		catch (Exception e)
		{
			AppHost.HandleApplicationCrash(e);
		}
	}

	public static AppBuilder BuildAvaloniaApp()
		=> AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.WithInterFont()
			.LogToTrace()
			.UseAsv(builder =>
			{
				// Register recipe page
				builder.Shell.Pages.Register<RecipePageViewModel, RecipePageView>(RecipePageViewModel.PageId);

				// Register command
				builder.Commands.Register<OpenRecipePageCommand>();

				// Register home page extension
				builder.Extensions.Register<IHomePage, HomePageRecipeExtension>();

				// Register dialog prefab and its view
				builder.Dialogs.RegisterPrefab<RecipeEditDialogPrefab>();
				builder.ViewLocator.RegisterViewFor<RecipeEditDialogViewModel, RecipeEditDialogView>();

				builder
					.UseDefault()
					.UseOptionalLogViewer()
					.UseOptionalSoloRun(opt => opt.WithArgumentForwarding())
					.UseDesktopShell();
			});
}
```

{collapsible="true" collapsed-title="Program.cs"}

```c#
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia.Samples.RecipeBook;

public class App : AsvApplication
{
	public override void Initialize()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
```

{collapsible="true" collapsed-title="App.axaml.cs"}

```xml
<Application xmlns="https://github.com/avaloniaui"
			 xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
			 x:Class="Asv.Avalonia.Samples.RecipeBook.App"
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
	xmlns:avalonia="clr-namespace:Asv.Avalonia;assembly=Asv.Avalonia"
	xmlns:local="clr-namespace:Asv.Avalonia.Samples.RecipeBook"
	mc:Ignorable="d"
	d:DesignWidth="800"
	d:DesignHeight="450"
	x:Class="Asv.Avalonia.Samples.RecipeBook.RecipePageView"
	x:DataType="local:RecipePageViewModel">
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

namespace Asv.Avalonia.Samples.RecipeBook;

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
	xmlns:local="clr-namespace:Asv.Avalonia.Samples.RecipeBook"
	mc:Ignorable="d"
	d:DesignWidth="800"
	d:DesignHeight="450"
	x:Class="Asv.Avalonia.Samples.RecipeBook.RecipeEditDialogView"
	x:DataType="local:RecipeEditDialogViewModel">

	<StackPanel>
		<TextBlock Text="Recipe Title" />
		<TextBox Text="{CompiledBinding Title.Value}" />

		<TextBlock Text="Category" />
		<TextBox Text="{CompiledBinding Category.Value}" />
	</StackPanel>
</UserControl>
```

{collapsible="true" collapsed-title="RecipeEditDialogView.axaml"}

```c#
using Asv.Avalonia;
using Avalonia.Controls;

namespace Asv.Avalonia.Samples.RecipeBook;

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

namespace Asv.Avalonia.Samples.RecipeBook;

public sealed class RemoveIngredientEvent(IRoutable source) : AsyncRoutedEvent<IRoutable>(source, RoutingStrategy.Bubble);

public static class RemoveIngredientEventMixin
{
	public static ValueTask RequestRemoveIngredient(this IRoutable src, CancellationToken cancel = default)
	{
		return src.Rise(new RemoveIngredientEvent(src), cancel);
	}
}
```

{collapsible="true" collapsed-title="RemoveIngredientEvent.cs"}

```c#
using Asv.Avalonia;

namespace Asv.Avalonia.Samples.RecipeBook;

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
	};
}
```

{collapsible="true" collapsed-title="OpenRecipePageCommand.cs"}

```c#
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Samples.RecipeBook;

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
using System.Threading;
using System.Threading.Tasks;
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Samples.RecipeBook;

public class IngredientViewModel : RoutableViewModel
{
	public const string BaseId = "ingredient";
    
    private ReactiveProperty<string?> _name;
    private ReactiveProperty<string?> _amount;

    public IngredientViewModel(string id, string name, string amount, ILoggerFactory loggerFactory)
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
        DeleteIngredientCommand = new ReactiveCommand(RemoveIngredientAsync).DisposeItWith(Disposable);
    }

    public HistoricalStringProperty Name { get; }
    public HistoricalStringProperty Amount { get; }
    public ReactiveCommand DeleteIngredientCommand { get; }

    public override IEnumerable<IRoutable> GetChildren()
    {
        yield return Name;
        yield return Amount;
    }
    
    private async ValueTask RemoveIngredientAsync(Unit unit, CancellationToken cancellationToken)
    {
        await this.RequestRemoveIngredient(cancellationToken);
    }
}
```

{collapsible="true" collapsed-title="IngredientViewModel.cs"}

```c#
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Samples.RecipeBook;

public sealed class RecipeEditDialogPayload
{
	public required string Title { get; init; }
	public required string Category { get; init; }
}

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

{collapsible="true" collapsed-title="RecipeEditDialogPrefab.cs"}

```c#
using System.Collections.Generic;
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Samples.RecipeBook;

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

{collapsible="true" collapsed-title="RecipeEditDialogViewModel.cs"}

```c#
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Cfg;
using Asv.Common;
using Asv.IO;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.Samples.RecipeBook;

public interface IRecipePageViewModel : IPage;

public class RecipePageViewModelLayoutConfig
{
	public string? SelectedRecipe { get; set; }
}

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

public class RecipePageViewModel : PageViewModel<IRecipePageViewModel>, IRecipePageViewModel
{
	private readonly ILoggerFactory _loggerFactory;
	public const string PageId = "recipe_page";
	public const MaterialIconKind PageIcon = MaterialIconKind.ViewGallery;

	private ObservableList<RecipeViewModel> _recipes { get; } = [];

	private readonly RecipeEditDialogPrefab _recipeEditDialog;

	private readonly IConfiguration _configuration;
	private RecipePageViewModelLayoutConfig? _layoutConfig;
	private readonly ISynchronizedView<RecipeViewModel, RecipeViewModel> _view;

	public RecipePageViewModel(ICommandService cmd, IConfiguration configuration, ILoggerFactory loggerFactory,
		IDialogService dialogService, IExtensionService ext)
		: base(PageId, cmd, loggerFactory, dialogService, ext)
	{
		_loggerFactory = loggerFactory;

		SelectedRecipe = new BindableReactiveProperty<RecipeViewModel?>();

		_recipes.SetRoutableParent(this).DisposeItWith(Disposable);
		_recipes.DisposeRemovedItems().DisposeItWith(Disposable);

		_recipeEditDialog = dialogService.GetDialogPrefab<RecipeEditDialogPrefab>();
		CreateRecipeCommand = new ReactiveCommand(CreateRecipeAsync).DisposeItWith(Disposable);

		_configuration = configuration;
		var recipeConfig = configuration.Get<RecipePageViewModelConfig>();
		Load(recipeConfig);

		Events.Subscribe(InternalCatchEvent).DisposeItWith(Disposable);

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
	}

	public NotifyCollectionChangedSynchronizedViewList<RecipeViewModel> Recipes { get; }

	public BindableReactiveProperty<RecipeViewModel?> SelectedRecipe { get; }

	public ReactiveCommand CreateRecipeCommand { get; }

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
			Guid.NewGuid().ToString(),
			createdRecipePayload.Title,
			createdRecipePayload.Category,
			string.Empty,
			[],
			_loggerFactory);

		_recipes.Add(recipeViewModel);
		SelectedRecipe.Value = recipeViewModel;

		Save();
	}

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

	private Task UpdateRecipeList(string? text, IProgress<double> progress, CancellationToken cancel)
	{
		progress.Report(0);

		if (string.IsNullOrWhiteSpace(text))
		{
			_view.ResetFilter();
			return Task.CompletedTask;
		}

		_view.AttachFilter(x => x.Title.ViewValue.Value != null && x.Title.ViewValue.Value.Contains(text));

		progress.Report(1);

		return Task.CompletedTask;
	}

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

}
```

{collapsible="true" collapsed-title="RecipePageViewModel.cs"}


```c#
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia.InfoMessage;
using Asv.Common;
using Asv.IO;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.Samples.RecipeBook;

public class RecipeViewModel : RoutableViewModel
{
	private readonly ILoggerFactory _loggerFactory;
	public const string BaseId = "recipe";

	private readonly ReactiveProperty<string?> _title;
	private readonly ReactiveProperty<string?> _category;
	private readonly ReactiveProperty<string?> _instruction;

	private readonly ObservableList<IngredientViewModel> _ingredients = [];

	public RecipeViewModel(NavigationId id, string title, string? category, string? instruction,
		IEnumerable<IngredientViewModel> ingredients,
		ILoggerFactory loggerFactory)
		: base(id, loggerFactory)
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

		CreateIngredientCommand = new ReactiveCommand(AddIngredientAsync).DisposeItWith(Disposable);
	}

	public HistoricalStringProperty Title { get; }
	public HistoricalStringProperty Category { get; }
	public HistoricalStringProperty Instruction { get; }
	public NotifyCollectionChangedSynchronizedViewList<IngredientViewModel> Ingredients { get; }

	public ReactiveCommand CreateIngredientCommand { get; }

	public async ValueTask AddIngredientAsync(Unit unit, CancellationToken cancellationToken)
	{
		var ingredient = new IngredientViewModel(Guid.NewGuid().ToString(),
			"Ingredient", string.Empty, _loggerFactory);
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
