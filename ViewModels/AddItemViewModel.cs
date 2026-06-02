using System.Windows.Input;
using FoodApp.Models;
using FoodApp.Services;

namespace FoodApp.ViewModels;

public class AddItemViewModel : BaseViewModel
{
    private readonly FoodService _foodService;
    private string _name = string.Empty;
    private string _category = string.Empty;
    private string _description = string.Empty;
    private string _calories = string.Empty;
    private string _protein = string.Empty;
    private string _carbs = string.Empty;
    private string _fat = string.Empty;
    private string _allergyInfo = string.Empty;

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    public string Category
    {
        get => _category;
        set { _category = value; OnPropertyChanged(); }
    }

    public string Description
    {
        get => _description;
        set { _description = value; OnPropertyChanged(); }
    }

    public string Calories
    {
        get => _calories;
        set { _calories = value; OnPropertyChanged(); }
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

    public AddItemViewModel(FoodService foodService)
    {
        _foodService = foodService;
        Title = "Add Food Item";

        SaveCommand = new Command(async () => await SaveAsync());
        CancelCommand = new Command(async () => await CancelAsync());
    }

    private async Task SaveAsync()
    {
        if (IsBusy) return;

        if (string.IsNullOrWhiteSpace(Name))
        {
            await ShowAlertAsync("Validation Error", "Please enter a food name.");
            return;
        }

        if (string.IsNullOrWhiteSpace(Category))
        {
            await ShowAlertAsync("Validation Error", "Please select a category.");
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
                AllergyInfo = string.IsNullOrWhiteSpace(AllergyInfo) ? "No allergy information provided." : AllergyInfo.Trim()
            };

            await _foodService.AddAsync(item);
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
            await ShowAlertAsync("Success", "Food item has been added successfully.");
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