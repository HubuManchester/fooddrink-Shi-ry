using FoodApp.Services;
using FoodApp.ViewModels;

namespace FoodApp.Views;

public partial class EditItemPage : ContentPage
{
    public EditItemPage(EditItemViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
    }
}