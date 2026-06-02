using FoodApp.ViewModels;

namespace FoodApp.Views;

public partial class DetailPage : ContentPage
{
    public DetailPage(DetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}