using FoodApp.Services;

namespace FoodApp.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
        LargeTextSwitch.IsToggled = AccessibilityService.LargeTextEnabled;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
        UpdatePreview();
    }

    private void OnLargeTextToggled(object? sender, ToggledEventArgs e)
    {
        AccessibilityService.LargeTextEnabled = e.Value;
        UpdatePreview();

        // 重新应用字体缩放到当前页面
        AccessibilityService.ApplyFontScale(this);

        // 通知用户
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

    // 主题切换命令可以在 ViewModel 中实现，或直接使用事件
    private void OnLightThemeClicked(object? sender, EventArgs e)
    {
        App.SetTheme(AppTheme.Light);
    }

    private void OnDarkThemeClicked(object? sender, EventArgs e)
    {
        App.SetTheme(AppTheme.Dark);
    }

    private void OnSystemThemeClicked(object? sender, EventArgs e)
    {
        App.SetTheme(AppTheme.Unspecified);
    }
}