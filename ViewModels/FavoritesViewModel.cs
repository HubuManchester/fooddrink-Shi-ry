using System.Collections.ObjectModel;
using System.Windows.Input;
using FoodApp.Models;
using FoodApp.Services;

namespace FoodApp.ViewModels;

public class FavoritesViewModel : BaseViewModel
{
    private readonly FoodDatabaseService _databaseService;
    private ObservableCollection<FoodItem> _favoriteItems;

    public ObservableCollection<FoodItem> FavoriteItems
    {
        get => _favoriteItems;
        set
        {
            _favoriteItems = value;
            OnPropertyChanged();
        }
    }

    public ICommand LoadFavoritesCommand { get; }
    public ICommand RemoveFavoriteCommand { get; }

    public FavoritesViewModel(FoodDatabaseService databaseService)
    {
        _databaseService = databaseService;
        Title = "Favorites";
        FavoriteItems = new ObservableCollection<FoodItem>();

        LoadFavoritesCommand = new Command(async () => await LoadFavoritesAsync());
        RemoveFavoriteCommand = new Command<FoodItem>(async (item) => await RemoveFavoriteAsync(item));

        // Auto load when created
        Task.Run(async () => await LoadFavoritesAsync());
    }

    public async Task LoadFavoritesAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            // Get all items first to see what's in the database
            var allItems = await _databaseService.GetAllAsync();
            System.Diagnostics.Debug.WriteLine($"=== Total items in database: {allItems?.Count ?? 0} ===");

            if (allItems != null)
            {
                foreach (var item in allItems)
                {
                    System.Diagnostics.Debug.WriteLine($"=== Item: {item.Name}, Id={item.Id}, IsFavorite={item.IsFavorite}, LocalId={item.LocalId} ===");
                }
            }

            // Then get favorites
            var favorites = await _databaseService.GetFavoritesAsync();

            System.Diagnostics.Debug.WriteLine($"=== Favorites loaded: {favorites?.Count ?? 0} items ===");

            FavoriteItems.Clear();

            if (favorites != null)
            {
                foreach (var item in favorites)
                {
                    FavoriteItems.Add(item);
                    System.Diagnostics.Debug.WriteLine($"=== Added to UI: {item.Name}");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== LoadFavoritesAsync error: {ex.Message} ===");
            await ShowAlertAsync("Error", $"Failed to load favorites: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RemoveFavoriteAsync(FoodItem item)
    {
        if (item == null) return;

        var confirm = await Shell.Current.DisplayAlert("Remove Favorite",
            $"Remove {item.Name} from favorites?", "Yes", "No");

        if (!confirm) return;

        try
        {
            IsBusy = true;

            // Use direct SQL update
            await _databaseService.SetFavoriteAsync(item.Id, false);

            // Also update the local object if it's in memory
            item.IsFavorite = false;

            // Remove from the displayed list
            FavoriteItems.Remove(item);

            SemanticScreenReader.Announce($"{item.Name} removed from favorites");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== RemoveFavoriteAsync error: {ex.Message} ===");
            await ShowAlertAsync("Error", $"Failed to remove: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}