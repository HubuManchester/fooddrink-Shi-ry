using System.Windows.Input;
using FoodApp.Models;
using FoodApp.Services;

namespace FoodApp.ViewModels;

public class AddItemViewModel : BaseViewModel
{
    private readonly FoodService _foodService;
    private readonly FoodDatabaseService _databaseService;

    private string _name = string.Empty;
    private string _category = string.Empty;
    private string _description = string.Empty;
    private string _calories = string.Empty;
    private string _protein = string.Empty;
    private string _carbs = string.Empty;
    private string _fat = string.Empty;
    private string _allergyInfo = string.Empty;

    private string _nameError = string.Empty;
    private string _categoryError = string.Empty;
    private string _caloriesError = string.Empty;

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
            ValidateName(); 
        }
    }

    public string Category
    {
        get => _category;
        set
        {
            _category = value;
            OnPropertyChanged();
            ValidateCategory();
        }
    }

    public string Description
    {
        get => _description;
        set { _description = value; OnPropertyChanged(); }
    }

    public string Calories
    {
        get => _calories;
        set
        {
            _calories = value;
            OnPropertyChanged();
            ValidateCalories();
        }
    }

    public string Protein
    {
        get => _protein;
        set { _protein = value; OnPropertyChanged(); }
    }

    public string Carbs
    {
        get => _carbs;
        set { _carbs = value; OnPropertyChanged(); }
    }

    public string Fat
    {
        get => _fat;
        set { _fat = value; OnPropertyChanged(); }
    }

    public string AllergyInfo
    {
        get => _allergyInfo;
        set { _allergyInfo = value; OnPropertyChanged(); }
    }

    public string NameError
    {
        get => _nameError;
        set
        {
            _nameError = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasNameError));
            OnPropertyChanged(nameof(CanSave));
        }
    }

    public bool HasNameError => !string.IsNullOrEmpty(NameError);

    public string CategoryError
    {
        get => _categoryError;
        set
        {
            _categoryError = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasCategoryError));
            OnPropertyChanged(nameof(CanSave));
        }
    }

    public bool HasCategoryError => !string.IsNullOrEmpty(CategoryError);

    public string CaloriesError
    {
        get => _caloriesError;
        set
        {
            _caloriesError = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasCaloriesError));
            OnPropertyChanged(nameof(CanSave));
        }
    }

    public bool HasCaloriesError => !string.IsNullOrEmpty(CaloriesError);

    public bool CanSave => !HasNameError && !HasCategoryError && !HasCaloriesError &&
                           !string.IsNullOrWhiteSpace(Name) &&
                           !string.IsNullOrWhiteSpace(Category);

    public AddItemViewModel(FoodDatabaseService databaseService, FoodService foodService)
    {
        _databaseService = databaseService;
        _foodService = foodService;
        Title = "Add Food Item";

        SaveCommand = new Command(async () => await SaveAsync());
        CancelCommand = new Command(async () => await CancelAsync());
    }

    private void ValidateName()
    {
        if (string.IsNullOrWhiteSpace(Name))
            NameError = "Food name is required";
        else if (Name.Length < 2)
            NameError = "Name must be at least 2 characters";
        else
            NameError = string.Empty;
    }

    private void ValidateCategory()
    {
        if (string.IsNullOrWhiteSpace(Category))
            CategoryError = "Please select a category";
        else
            CategoryError = string.Empty;
    }

    private void ValidateCalories()
    {
        if (string.IsNullOrWhiteSpace(Calories))
            CaloriesError = "Calories are required";
        else if (!int.TryParse(Calories, out int cal) || cal < 0)
            CaloriesError = "Please enter a valid positive number";
        else
            CaloriesError = string.Empty;
    }

    private async Task SaveAsync()
    {
        if (IsBusy) return;

        ValidateName();
        ValidateCategory();
        ValidateCalories();

        if (!CanSave)
        {
            await ShowAlertAsync("Validation Error", "Please fix the errors before saving.");
            return;
        }

        if (!int.TryParse(Calories, out int calories) || calories < 0)
        {
            await ShowAlertAsync("Validation Error", "Please enter a valid calorie amount (0 or greater).");
            return;
        }

        if (!int.TryParse(Protein, out int protein) || protein < 0)
            protein = 0;

        if (!int.TryParse(Carbs, out int carbs) || carbs < 0)
            carbs = 0;

        if (!int.TryParse(Fat, out int fat) || fat < 0)
            fat = 0;

        try
        {
            IsBusy = true;

            var item = new FoodItem
            {
                Name = Name.Trim(),
                Category = Category.Trim(),
                Description = string.IsNullOrWhiteSpace(Description) ? "No description provided." : Description.Trim(),
                Calories = calories,
                Protein = protein,
                Carbs = carbs,
                Fat = fat,
                AllergyInfo = string.IsNullOrWhiteSpace(AllergyInfo) ? "No allergy information provided." : AllergyInfo.Trim(),
                CreatedAt = DateTime.Now
            };

            await _databaseService.AddAsync(item);  // Save to SQLite database
            await _foodService.AddAsync(item);       // Save to memory cache

            HapticFeedback.Default.Perform(HapticFeedbackType.Click);

            // Screen reader announcement (accessibility)
            SemanticScreenReader.Announce($"Added {item.Name} successfully");

            await ShowAlertAsync("Success", $"'{item.Name}' has been added to your collection.");
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