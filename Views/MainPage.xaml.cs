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
        ((MainViewModel)BindingContext).LoadItemsCommand.Execute(null);
    }
}