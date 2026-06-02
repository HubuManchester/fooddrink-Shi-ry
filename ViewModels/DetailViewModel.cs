using System.Windows.Input;
using FoodApp.Models;
using FoodApp.Services;

namespace FoodApp.ViewModels;

[QueryProperty(nameof(FoodItem), "foodItem")]
public class DetailViewModel : BaseViewModel
{
    private readonly FoodService _foodService;
    private FoodItem? _foodItem;

    public ICommand SpeakCommand { get; }
    public ICommand StopSpeechCommand { get; }
    public ICommand VibrateCommand { get; }
    public ICommand TakePhotoCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }

    public FoodItem? FoodItem
    {
        get => _foodItem;
        set
        {
            _foodItem = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayText));
            OnPropertyChanged(nameof(NutritionText));
        }
    }

    public string DisplayText => _foodItem is null ? string.Empty : $"{_foodItem.Name} - {_foodItem.Calories} kcal";
    public string NutritionText => _foodItem is null ? string.Empty : $"{_foodItem.NutritionSummary}";

    public DetailViewModel(FoodService foodService)
    {
        _foodService = foodService;
        Title = "Food Details";

        SpeakCommand = new Command(async () => await SpeakAsync());
        StopSpeechCommand = new Command(() => SpeechService.Stop());
        VibrateCommand = new Command(() => VibrateAsync());
        TakePhotoCommand = new Command(async () => await TakePhotoAsync());
        EditCommand = new Command(async () => await EditAsync());
        DeleteCommand = new Command(async () => await DeleteAsync());
    }

    private async Task SpeakAsync()
    {
        if (_foodItem is null) return;

        var text = $"{_foodItem.Name}. {_foodItem.Description}. " +
                   $"{_foodItem.Calories} calories, {_foodItem.Protein} grams protein, " +
                   $"{_foodItem.Carbs} grams carbs, {_foodItem.Fat} grams fat. " +
                   $"Allergy information: {_foodItem.AllergyInfo}";

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
                await ShowAlertAsync("Permission Denied", "Camera access is required to take photos.");
                return;
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo is null) return;

            var savedPath = Path.Combine(FileSystem.CacheDirectory, $"{Guid.NewGuid()}.jpg");
            using var stream = await photo.OpenReadAsync();
            using var fileStream = File.Create(savedPath);
            await stream.CopyToAsync(fileStream);

            SemanticScreenReader.Announce("Photo captured and saved.");
            await ShowAlertAsync("Photo Captured", $"Photo saved to: {savedPath}");
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Camera Error", ex.Message);
        }
    }

    private async Task EditAsync()
    {
        if (_foodItem is null) return;
        await Shell.Current.GoToAsync("..");
    }

    private async Task DeleteAsync()
    {
        if (_foodItem is null) return;

        var confirm = await Shell.Current.DisplayAlert("Confirm Delete",
            $"Are you sure you want to delete {_foodItem.Name}?", "Yes", "No");

        if (!confirm) return;

        try
        {
            await _foodService.DeleteAsync(_foodItem.Id);
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
            await ShowAlertAsync("Deleted", "Food item has been removed.");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Error", $"Failed to delete: {ex.Message}");
        }
    }
}