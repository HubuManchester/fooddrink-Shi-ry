using FoodApp.ViewModels;

namespace FoodApp.Views;

public partial class AddItemPage : ContentPage
{
    public AddItemPage(AddItemViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}