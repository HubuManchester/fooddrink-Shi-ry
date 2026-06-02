using System.Windows.Input;
using FoodApp.Models;
using FoodApp.Services;

namespace FoodApp.ViewModels;

[QueryProperty(nameof(ItemId), "id")]
public class DetailViewModel : BaseViewModel
{
    private readonly FoodService _foodService;
    private readonly FoodDatabaseService _databaseService;
    private FoodItem? _currentItem;
    private string _itemId = string.Empty;

    public ICommand SpeakCommand { get; }
    public ICommand StopSpeechCommand { get; }
    public ICommand VibrateCommand { get; }
    public ICommand TakePhotoCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }

    // 路由传参：接收字符串 ID
    public string ItemId
    {
        get => _itemId;
        set
        {
            _itemId = value;
            // 当 ID 传入时，加载数据
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
            OnPropertyChanged(nameof(DisplayText));
            OnPropertyChanged(nameof(NutritionText));
            OnPropertyChanged(nameof(HasItem));
        }
    }

    public bool HasItem => _currentItem != null;
    public string DisplayText => _currentItem is null ? string.Empty : $"{_currentItem.Name} - {_currentItem.Calories} kcal";
    public string NutritionText => _currentItem is null ? string.Empty : _currentItem.NutritionSummary;

    public DetailViewModel(FoodService foodService, FoodDatabaseService databaseService)
    {
        _foodService = foodService;
        _databaseService = databaseService;
        Title = "Food Details";

        SpeakCommand = new Command(async () => await SpeakAsync());
        StopSpeechCommand = new Command(() => SpeechService.Stop());
        VibrateCommand = new Command(() => VibrateAsync());
        TakePhotoCommand = new Command(async () => await TakePhotoAsync());
        EditCommand = new Command(async () => await EditAsync());
        DeleteCommand = new Command(async () => await DeleteAsync());
    }

    // 根据 ID 加载数据
    private async Task LoadItemAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return;
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            // 优先从数据库获取
            if (_databaseService != null)
            {
                CurrentItem = await _databaseService.GetByIdAsync(id);
            }

            // 如果数据库没有，从内存服务获取
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

    private async Task SpeakAsync()
    {
        if (_currentItem is null) return;

        var text = $"{_currentItem.Name}. {_currentItem.Description}. " +
                   $"{_currentItem.Calories} calories, {_currentItem.Protein} grams protein, " +
                   $"{_currentItem.Carbs} grams carbs, {_currentItem.Fat} grams fat. " +
                   $"Allergy information: {_currentItem.AllergyInfo}";

        try
        {
            await SpeechService.SpeakAsync(text);
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Speech Error", ex.Message);
        }
    }

    private void VibrateAsync()
    {
        try
        {
            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(300));
            HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
            SemanticScreenReader.Announce("Vibration feedback triggered.");
        }
        catch (Exception ex)
        {
            Task.Run(async () => await ShowAlertAsync("Vibration Error", ex.Message));
        }
    }

    private async Task TakePhotoAsync()
    {
        try
        {
            if (!await PermissionService.RequestCameraPermissionAsync())
            {
                await ShowAlertAsync("Permission Denied", "Camera access is required.");
                return;
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo is null) return;

            var savedPath = Path.Combine(FileSystem.CacheDirectory, $"{Guid.NewGuid()}.jpg");
            using var stream = await photo.OpenReadAsync();
            using var fileStream = File.Create(savedPath);
            await stream.CopyToAsync(fileStream);

            if (_currentItem != null)
            {
                _currentItem.PhotoPath = savedPath;
                if (_databaseService != null)
                {
                    await _databaseService.UpdateAsync(_currentItem);
                }
            }

            SemanticScreenReader.Announce("Photo captured and saved.");
            await ShowAlertAsync("Photo Captured", "Photo saved successfully.");
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Camera Error", ex.Message);
        }
    }

    private async Task EditAsync()
    {
        if (_currentItem is null) return;
        // 导航到编辑页面
        await Shell.Current.GoToAsync("..");
    }

    private async Task DeleteAsync()
    {
        if (_currentItem is null) return;

        var confirm = await Shell.Current.DisplayAlert("Confirm Delete",
            $"Are you sure you want to delete {_currentItem.Name}?", "Yes", "No");

        if (!confirm) return;

        try
        {
            IsBusy = true;

            if (_databaseService != null)
            {
                await _databaseService.DeleteAsync(_currentItem.Id);
            }

            if (_foodService != null)
            {
                await _foodService.DeleteAsync(_currentItem.Id);
            }

            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
            await ShowAlertAsync("Deleted", "Food item has been removed.");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Error", $"Failed to delete: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}