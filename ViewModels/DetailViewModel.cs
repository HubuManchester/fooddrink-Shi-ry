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

    // 用于 UI 判断是否有照片
    public bool HasPhoto => _currentItem != null && !string.IsNullOrEmpty(_currentItem.PhotoPath);
    public bool HasNoPhoto => _currentItem != null && string.IsNullOrEmpty(_currentItem.PhotoPath);

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
            OnPropertyChanged(nameof(HasPhoto));      // 通知 UI 刷新
            OnPropertyChanged(nameof(HasNoPhoto));    // 通知 UI 刷新
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
        if (_currentItem is null)
        {
            await ShowAlertAsync("Error", "No food item loaded.");
            return;
        }

        try
        {
            // 请求相机权限
            var cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();
            if (cameraStatus != PermissionStatus.Granted)
            {
                await ShowAlertAsync("Permission Denied", "Camera permission is required to take photos.");
                return;
            }

            // 请求存储权限（Android 需要）
            var storageStatus = await Permissions.RequestAsync<Permissions.StorageWrite>();
            if (storageStatus != PermissionStatus.Granted)
            {
                await ShowAlertAsync("Permission Denied", "Storage permission is required to save photos.");
                return;
            }

            IsBusy = true;
            SemanticScreenReader.Announce("Opening camera...");

            // 拍照
            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo is null)
            {
                await ShowAlertAsync("Cancelled", "Photo capture was cancelled.");
                return;
            }

            // 保存照片到应用本地目录
            var fileName = $"{_currentItem.Id}_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
            var savedPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

            using var stream = await photo.OpenReadAsync();
            using var fileStream = File.Create(savedPath);
            await stream.CopyToAsync(fileStream);

            // 更新当前食物的照片路径
            _currentItem.PhotoPath = savedPath;

            // 同时保存到数据库和内存服务
            if (_databaseService != null)
            {
                await _databaseService.UpdateAsync(_currentItem);
            }

            if (_foodService != null)
            {
                await _foodService.UpdateAsync(_currentItem);
            }

            // 刷新 UI
            OnPropertyChanged(nameof(CurrentItem));
            OnPropertyChanged(nameof(HasPhoto));
            OnPropertyChanged(nameof(HasNoPhoto));

            SemanticScreenReader.Announce("Photo captured and saved.");
            await ShowAlertAsync("Photo Captured", "Photo has been saved to this food item.");
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Camera Error", $"Failed to take photo: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task EditAsync()
    {
        if (_currentItem is null) return;
        await Shell.Current.GoToAsync($"{nameof(Views.EditItemPage)}?id={_currentItem.Id}");
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

            if (!string.IsNullOrEmpty(_currentItem.PhotoPath) && File.Exists(_currentItem.PhotoPath))
            {
                File.Delete(_currentItem.PhotoPath);
            }

            if (_databaseService != null)
            {
                await _databaseService.DeleteAsync(_currentItem.Id);
            }

            if (_foodService != null)
            {
                await _foodService.DeleteAsync(_currentItem.Id);
            }

            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
            SemanticScreenReader.Announce($"{_currentItem.Name} deleted");
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