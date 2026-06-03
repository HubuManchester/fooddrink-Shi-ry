namespace FoodApp;

public partial class App : Application
{
    // Public attribute, for use in the configuration page
    public static AppTheme CurrentTheme { get; private set; } = AppTheme.Unspecified;

    public App()
    {
        InitializeComponent();

        // Load saved theme settings
        LoadTheme();

        MainPage = new AppShell();
    }

    private void LoadTheme()
    {
        var themePreference = Preferences.Get("AppTheme", "default");
        CurrentTheme = themePreference switch
        {
            "light" => AppTheme.Light,
            "dark" => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };
        UserAppTheme = CurrentTheme;
    }

    public static void SetTheme(AppTheme theme)
    {
        CurrentTheme = theme;
        var themeKey = theme switch
        {
            AppTheme.Light => "light",
            AppTheme.Dark => "dark",
            _ => "default"
        };
        Preferences.Set("AppTheme", themeKey);
        Current.UserAppTheme = theme;
    }
}