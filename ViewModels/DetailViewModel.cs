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

    // Shake detection
    private bool _isShakeDetectionActive = false;
    private DateTime _lastShakeTime = DateTime.MinValue;
    private const double ShakeThreshold = 2.5;
    private const int ShakeQuietPeriodMs = 500;

    // Commands
    public ICommand SpeakCommand { get; }
    public ICommand StopSpeechCommand { get; }
    public ICommand VibrateCommand { get; }
    public ICommand TakePhotoCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand AddToFavoriteCommand { get; }
    public ICommand RemoveFromFavoriteCommand { get; }
    public ICommand GetLocationCommand { get; }
    public ICommand OpenMapCommand { get; }

    // Properties
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
            OnPropertyChanged(nameof(DisplayText));
            OnPropertyChanged(nameof(NutritionText));
            OnPropertyChanged(nameof(HasPhoto));
            OnPropertyChanged(nameof(HasNoPhoto));
            OnPropertyChanged(nameof(IsFavorite));
            OnPropertyChanged(nameof(IsNotFavorite));
        }
    }

    public bool IsFavorite => _currentItem?.IsFavorite ?? false;
    public bool IsNotFavorite => !IsFavorite;
    public bool HasPhoto => _currentItem != null && !string.IsNullOrEmpty(_currentItem.PhotoPath);
    public bool HasNoPhoto => _currentItem != null && string.IsNullOrEmpty(_currentItem.PhotoPath);
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
        AddToFavoriteCommand = new Command(async () => await AddToFavoriteAsync());
        RemoveFromFavoriteCommand = new Command(async () => await RemoveFromFavoriteAsync());
        GetLocationCommand = new Command(async () => await GetCurrentLocationAsync());
        OpenMapCommand = new Command(async () => await OpenMapAsync());
    }

    private async Task LoadItemAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return;
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            // ALWAYS try to get from database first
            if (_databaseService != null)
            {
                CurrentItem = await _databaseService.GetByIdAsync(id);

                if (CurrentItem != null)
                {
                    System.Diagnostics.Debug.WriteLine($"=== Loaded from database: {CurrentItem.Name}, LocalId={CurrentItem.LocalId} ===");
                }
            }

            // If not in database, try memory service
            if (CurrentItem == null && _foodService != null)
            {
                CurrentItem = await _foodService.GetByIdAsync(id);

                if (CurrentItem != null)
                {
                    System.Diagnostics.Debug.WriteLine($"=== Loaded from memory: {CurrentItem.Name} ===");

                    // Save to database so it has a LocalId
                    await _databaseService.AddAsync(CurrentItem);
                    System.Diagnostics.Debug.WriteLine($"=== Saved to database: {CurrentItem.Name} ===");
                }
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
        if (_currentItem is null) return;

        try
        {
            var cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();
            if (cameraStatus != PermissionStatus.Granted)
            {
                await ShowAlertAsync("Permission Denied", "Camera permission is required.");
                return;
            }

            IsBusy = true;
            SemanticScreenReader.Announce("Opening camera...");

            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo is null)
            {
                await ShowAlertAsync("Cancelled", "Photo capture was cancelled.");
                return;
            }

            var fileName = $"{_currentItem.Id}_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
            var savedPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

            using var stream = await photo.OpenReadAsync();
            using var fileStream = File.Create(savedPath);
            await stream.CopyToAsync(fileStream);

            _currentItem.PhotoPath = savedPath;

            if (_databaseService != null)
            {
                await _databaseService.UpdateAsync(_currentItem);
            }

            if (_foodService != null)
            {
                await _foodService.UpdateAsync(_currentItem);
            }

            OnPropertyChanged(nameof(CurrentItem));
            OnPropertyChanged(nameof(HasPhoto));
            OnPropertyChanged(nameof(HasNoPhoto));

            SemanticScreenReader.Announce("Photo captured and saved.");
            await ShowAlertAsync("Photo Captured", "Photo has been saved.");
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Camera Error", $"Failed: {ex.Message}");
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
            $"Delete {_currentItem.Name}?", "Yes", "No");

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

    // ========== Favorite Methods ==========

    private async Task AddToFavoriteAsync()
    {
        if (_currentItem == null) return;

        if (_currentItem.IsFavorite)
        {
            await ShowAlertAsync("Already Favorite", $"{_currentItem.Name} is already in your favorites.");
            return;
        }

        var confirm = await Shell.Current.DisplayAlert("Add to Favorites",
            $"Add {_currentItem.Name} to your favorites?", "Yes", "No");

        if (!confirm) return;

        try
        {
            // Use the direct SQL update method
            if (_databaseService != null)
            {
                var result = await _databaseService.SetFavoriteAsync(_currentItem.Id, true);
                System.Diagnostics.Debug.WriteLine($"=== SetFavoriteAsync result: {result} rows affected ===");
            }

            _currentItem.IsFavorite = true;

            OnPropertyChanged(nameof(IsFavorite));
            OnPropertyChanged(nameof(IsNotFavorite));

            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
            SemanticScreenReader.Announce($"{_currentItem.Name} added to favorites");
            await ShowAlertAsync("Added!", $"❤️ {_currentItem.Name} has been added to your favorites.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== AddToFavoriteAsync error: {ex.Message} ===");
            await ShowAlertAsync("Error", $"Failed to add: {ex.Message}");
        }
    }

    private async Task RemoveFromFavoriteAsync()
    {
        if (_currentItem == null) return;

        if (!_currentItem.IsFavorite)
        {
            return;
        }

        var confirm = await Shell.Current.DisplayAlert("Remove from Favorites",
            $"Remove {_currentItem.Name} from favorites?", "Yes", "No");

        if (!confirm) return;

        try
        {
            _currentItem.IsFavorite = false;

            if (_databaseService != null)
            {
                await _databaseService.UpdateAsync(_currentItem);
            }

            if (_foodService != null)
            {
                await _foodService.UpdateAsync(_currentItem);
            }

            OnPropertyChanged(nameof(IsFavorite));
            OnPropertyChanged(nameof(IsNotFavorite));

            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
            SemanticScreenReader.Announce($"{_currentItem.Name} removed from favorites");
            await ShowAlertAsync("Removed", $"💔 {_currentItem.Name} removed from favorites.");
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Error", $"Failed to remove: {ex.Message}");
        }
    }

    // ========== Shake Detection ==========

    public void InitShakeDetection()
    {
        if (_isShakeDetectionActive) return;

        if (Accelerometer.Default.IsSupported)
        {
            Accelerometer.Default.ReadingChanged += OnAccelerometerReadingChanged;
            Accelerometer.Default.Start(SensorSpeed.Game);
            _isShakeDetectionActive = true;
            System.Diagnostics.Debug.WriteLine("=== Shake detection started ===");
        }
    }

    public void StopShakeDetection()
    {
        if (!_isShakeDetectionActive) return;

        if (Accelerometer.Default.IsSupported)
        {
            Accelerometer.Default.ReadingChanged -= OnAccelerometerReadingChanged;
            Accelerometer.Default.Stop();
            _isShakeDetectionActive = false;
            System.Diagnostics.Debug.WriteLine("=== Shake detection stopped ===");
        }
    }

    private void OnAccelerometerReadingChanged(object sender, AccelerometerChangedEventArgs e)
    {
        var data = e.Reading;
        var x = data.Acceleration.X;
        var y = data.Acceleration.Y;
        var z = data.Acceleration.Z;

        var acceleration = Math.Sqrt(x * x + y * y + z * z);

        if (acceleration > ShakeThreshold)
        {
            var now = DateTime.Now;
            if ((now - _lastShakeTime).TotalMilliseconds > ShakeQuietPeriodMs)
            {
                _lastShakeTime = now;

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    // 震动反馈表示检测到晃动
                    HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
                    await AddToFavoriteAsync();
                });
            }
        }
    }

    private async Task GetCurrentLocationAsync()
    {
        if (_currentItem == null)
        {
            await ShowAlertAsync("Error", "No food item loaded.");
            return;
        }

        try
        {
            // Request location permission
            var locationStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (locationStatus != PermissionStatus.Granted)
            {
                await ShowAlertAsync("Permission Denied", "Location permission is required to record your dining location.");
                return;
            }

            IsBusy = true;
            SemanticScreenReader.Announce("Getting current location...");

            // Get current location
            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(15));
            var location = await Geolocation.Default.GetLocationAsync(request);

            if (location == null)
            {
                await ShowAlertAsync("Location Failed", "Unable to get current location. Please try again.");
                return;
            }

            // Save location to current item
            _currentItem.Latitude = location.Latitude;
            _currentItem.Longitude = location.Longitude;

            // Try to get address from coordinates (reverse geocoding)
            try
            {
                var placemarks = await Geocoding.Default.GetPlacemarksAsync(location.Latitude, location.Longitude);
                var placemark = placemarks?.FirstOrDefault();
                if (placemark != null)
                {
                    var addressParts = new[]
                    {
                    placemark.CountryName,
                    placemark.AdminArea,
                    placemark.Locality,
                    placemark.Thoroughfare
                };
                    _currentItem.LocationName = string.Join(", ", addressParts.Where(p => !string.IsNullOrEmpty(p)));
                }
            }
            catch
            {
                // Geocoding failed, just use coordinates
                _currentItem.LocationName = $"{location.Latitude:F4}, {location.Longitude:F4}";
            }

            // Save to database
            if (_databaseService != null)
            {
                await _databaseService.UpdateAsync(_currentItem);
            }

            if (_foodService != null)
            {
                await _foodService.UpdateAsync(_currentItem);
            }

            // Show success message with location info
            var message = string.IsNullOrEmpty(_currentItem.LocationName)
                ? $"Location saved: {location.Latitude:F4}, {location.Longitude:F4}"
                : $"Location saved: {_currentItem.LocationName}";

            await ShowAlertAsync("Location Saved", message);
            SemanticScreenReader.Announce("Location saved");

            // Refresh UI
            OnPropertyChanged(nameof(CurrentItem));
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Location Error", $"Failed to get location: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
    private async Task OpenMapAsync()
    {
        if (_currentItem == null)
        {
            await ShowAlertAsync("Error", "No food item loaded.");
            return;
        }

        if (_currentItem.Latitude == null || _currentItem.Longitude == null)
        {
            await ShowAlertAsync("No Location", "This food item doesn't have a saved location. Tap 'Get Location' first.");
            return;
        }

        try
        {
            var latitude = _currentItem.Latitude.Value;
            var longitude = _currentItem.Longitude.Value;

            // Open in system maps app
            var options = new MapLaunchOptions
            {
                Name = _currentItem.Name ?? "Food Location",
                NavigationMode = NavigationMode.None
            };

            await Map.Default.OpenAsync(latitude, longitude, options);
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Map Error", $"Failed to open maps: {ex.Message}");
        }
    }
}