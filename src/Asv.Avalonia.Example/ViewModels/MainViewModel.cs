using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Asv.Avalonia.Example.ViewModels;

public class MainViewModel : DisposableViewModel, INotifyPropertyChanged
{
    private string _searchQueryValue;
    private List<SearchableItem> _resultsList;

    public MainViewModel()
        : base("shell")
    {
        History = new CommandHistory(Id);
        Property1 = new HistoryProperty(History, "property1");
        Property2 = new HistoryProperty(History, "property2");

        SearchRepository = new SearchRepository();
        SearchEngine = new SearchEngine(SearchRepository);

        SearchRepository.AddItem(
            new SearchableItem
            {
                Id = "open_file",
                Name = "Open File",
                Description = "Command to open a file",
                Type = SearchableItemType.Command,
            }
        );
        SearchRepository.AddItem(
            new SearchableItem
            {
                Id = "settings",
                Name = "Settings",
                Description = "Application settings",
                Type = SearchableItemType.Setting,
            }
        );
        SearchRepository.AddItem(
            new SearchableItem
            {
                Id = "flight_page",
                Name = "Flight Page",
                Description = "One of the main views",
                Type = SearchableItemType.View,
            }
        );
        SearchRepository.AddItem(
            new SearchableItem
            {
                Id = "planning_page",
                Name = "Planning Page",
                Description = "One of the main views",
                Type = SearchableItemType.View,
            }
        );
        SearchRepository.AddItem(
            new SearchableItem
            {
                Id = "smthlikeanid",
                Name = "Продавайте деньги",
                Description = "idk",
                Type = SearchableItemType.Other,
            }
        );
        SearchRepository.AddItem(
            new SearchableItem
            {
                Id = "smthlikeanid",
                Name = "Покупайте деньги",
                Description = "idk",
                Type = SearchableItemType.Other,
            }
        );

        ResultsList = SearchRepository.GetItems().ToList();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ICommandHistory History { get; }
    public SearchRepository SearchRepository { get; set; }
    public SearchEngine SearchEngine { get; set; }
    public HistoryProperty Property2 { get; }
    public HistoryProperty Property1 { get; }
    public string SearchQueryValue
    {
        get => _searchQueryValue;
        set
        {
            if (_searchQueryValue != value)
            {
                _searchQueryValue = value;
                OnPropertyChanged(nameof(SearchQueryValue));
                Search(_searchQueryValue);
            }
        }
    }

    public List<SearchableItem> ResultsList
    {
        get => _resultsList;
        set
        {
            if (_resultsList != value)
            {
                _resultsList = value;
                OnPropertyChanged(nameof(ResultsList));
            }
        }
    }

    private void Search(string query)
    {
        ResultsList = string.IsNullOrWhiteSpace(query)
            ? SearchRepository.GetItems().ToList()
            : SearchEngine.Search(query).ToList();
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
