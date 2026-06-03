using FoodApp.Services;
using System.Windows.Input;

namespace FoodApp.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
        LargeTextSwitch.IsToggled = AccessibilityService.LargeTextEnabled;
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
        UpdatePreview();
    }

    // switch brighrness
    public ICommand SetLightThemeCommand => new Command(() => SetTheme(AppTheme.Light));
    public ICommand SetDarkThemeCommand => new Command(() => SetTheme(AppTheme.Dark));
    public ICommand SetSystemThemeCommand => new Command(() => SetTheme(AppTheme.Unspecified));

    private void SetTheme(AppTheme theme)
    {
        App.SetTheme(theme);
        SemanticScreenReader.Announce($"Theme changed to {theme}");
    }

    private void OnLargeTextToggled(object? sender, ToggledEventArgs e)
    {
        AccessibilityService.LargeTextEnabled = e.Value;
        UpdatePreview();

        AccessibilityService.ApplyFontScale(this);

        SemanticScreenReader.Announce(e.Value ? "Large text mode enabled" : "Large text mode disabled");
    }

    private void UpdatePreview()
    {
        if (AccessibilityService.LargeTextEnabled)
        {
            PreviewLabel.Text = "✓ Large text mode is ON. Text size has been increased for better accessibility.";
            PreviewLabel.TextColor = Colors.Green;
        }
        else
        {
            PreviewLabel.Text = "Toggle the switch above to enable large text mode.";
            PreviewLabel.TextColor = Colors.Gray;
        }
    }
}