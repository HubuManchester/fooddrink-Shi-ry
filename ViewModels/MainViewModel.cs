using System.Collections.ObjectModel;
using System.Windows.Input;
using FoodApp.Models;
using FoodApp.Services;


namespace FoodApp.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly FoodService _foodService;
    private string _searchText = string.Empty;

    public ObservableCollection<FoodItem> Items { get; } = new();
    public ICommand LoadItemsCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand AddItemCommand { get; }
    public ICommand ViewDetailCommand { get; }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText == value) return;
            _searchText = value;
            OnPropertyChanged();
            SearchAsync();
        }
    }

    public MainViewModel(FoodService foodService)
    {
        _foodService = foodService;
        Title = "Food Explorer";

        LoadItemsCommand = new Command(async () => await LoadItemsAsync());
        SearchCommand = new Command(async () => await SearchAsync());
        AddItemCommand = new Command(async () => await GoToAddPage());
        ViewDetailCommand = new Command<FoodItem>(async (item) => await GoToDetailPage(item));

        Task.Run(async () => await LoadItemsAsync());
    }

    private async Task LoadItemsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var items = await _foodService.SearchAsync(null);
            Items.Clear();
            foreach (var item in items)
                Items.Add(item);
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Error", $"Failed to load items: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SearchAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var items = await _foodService.SearchAsync(_searchText);
            Items.Clear();
            foreach (var item in items)
                Items.Add(item);
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Error", $"Search failed: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task GoToAddPage()
    {
        await Shell.Current.GoToAsync(nameof(Views.AddItemPage));
    }

    private async Task GoToDetailPage(FoodItem item)
    {
        if (item == null) return;

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Shell.Current.GoToAsync($"{nameof(Views.DetailPage)}?id={item.Id}");
        });
    }
}