namespace FoodApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(Views.AddItemPage), typeof(Views.AddItemPage));
        Routing.RegisterRoute(nameof(Views.DetailPage), typeof(Views.DetailPage));
    }
}