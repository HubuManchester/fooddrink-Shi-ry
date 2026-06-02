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
        // 应用字体缩放
        AccessibilityService.ApplyFontScale(this);
        // 加载数据
        ((MainViewModel)BindingContext).LoadItemsCommand.Execute(null);
    }
}