using FoodApp.Services;
using FoodApp.ViewModels;

namespace FoodApp.Views;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        //apply font scaling
        AccessibilityService.ApplyFontScale(this);
        //load data
        ((MainViewModel)BindingContext).LoadItemsCommand.Execute(null);
    }
}