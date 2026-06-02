using System.Windows.Input;
using FoodApp.Models;
using FoodApp.Services;

namespace FoodApp.ViewModels;

[QueryProperty(nameof(ItemId), "id")]
public class EditItemViewModel : BaseViewModel
{
    private readonly FoodService _foodService;
    private readonly FoodDatabaseService _databaseService;
    private FoodItem? _currentItem;
    private string _itemId = string.Empty;

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public string ItemId
    {
        get => _itemId;
        set
        {
            _itemId = value;
            _ = LoadItemAsync(value);
        }
    }

    public FoodItem? CurrentItem
    {
        get => _currentItem;
        set
        {
            _currentItem = value;
            OnPropertyChanged();
        }
    }

    public EditItemViewModel(FoodService foodService, FoodDatabaseService databaseService)
    {
        _foodService = foodService;
        _databaseService = databaseService;
        Title = "Edit Food Item";

        SaveCommand = new Command(async () => await SaveAsync());
        CancelCommand = new Command(async () => await CancelAsync());
    }

    private async Task LoadItemAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return;
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            if (_databaseService != null)
            {
                CurrentItem = await _databaseService.GetByIdAsync(id);
            }

            if (CurrentItem == null && _foodService != null)
            {
                CurrentItem = await _foodService.GetByIdAsync(id);
            }

            if (CurrentItem == null)
            {
                await ShowAlertAsync("Error", "Food item not found.");
                await Shell.Current.GoToAsync("..");
            }
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Error", $"Failed to load: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveAsync()
    {
        if (CurrentItem is null) return;
        if (IsBusy) return;

        // 验证
        if (string.IsNullOrWhiteSpace(CurrentItem.Name))
        {
            await ShowAlertAsync("Validation Error", "Please enter a food name.");
            return;
        }

        if (string.IsNullOrWhiteSpace(CurrentItem.Category))
        {
            await ShowAlertAsync("Validation Error", "Please select a category.");
            return;
        }

        try
        {
            IsBusy = true;

            // 更新时间戳
            CurrentItem.CreatedAt = DateTime.Now;

            // 更新数据库和内存服务
            if (_databaseService != null)
            {
                await _databaseService.UpdateAsync(CurrentItem);
            }

            if (_foodService != null)
            {
                await _foodService.UpdateAsync(CurrentItem);
            }

            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
            SemanticScreenReader.Announce($"{CurrentItem.Name} updated");

            await ShowAlertAsync("Success", $"'{CurrentItem.Name}' has been updated.");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Error", $"Failed to save: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}