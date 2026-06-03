using FoodApp.Services;
using FoodApp.ViewModels;

namespace FoodApp.Views;

public partial class DetailPage : ContentPage
{
    public DetailPage(DetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);

        if (BindingContext is DetailViewModel vm)
        {
            vm.InitShakeDetection();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (BindingContext is DetailViewModel vm)
        {
            vm.StopShakeDetection();
        }
    }
}