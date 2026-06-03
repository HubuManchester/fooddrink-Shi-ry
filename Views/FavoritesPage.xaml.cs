using FoodApp.Models;
using FoodApp.Services;
using FoodApp.ViewModels;

namespace FoodApp.Views;

public partial class FavoritesPage : ContentPage
{
    private readonly FavoritesViewModel _viewModel;

    public FavoritesPage(FavoritesViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);

        // Force reload favorites every time page appears
        Task.Run(async () => await _viewModel.LoadFavoritesAsync());
    }

    private async void OnItemSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is FoodItem selectedItem)
        {
            ((CollectionView)sender).SelectedItem = null;
            await Shell.Current.GoToAsync($"{nameof(DetailPage)}?id={selectedItem.Id}");
        }
    }
}