namespace FoodApp;

public partial class App : Application
{
    // 公开的属性，供设置页面使用
    public static AppTheme CurrentTheme { get; private set; } = AppTheme.Unspecified;

    public App()
    {
        InitializeComponent();

        // 加载保存的主题设置
        LoadTheme();

        MainPage = new AppShell();
    }

    private void LoadTheme()
    {
        // 使用 Preferences 保存用户的主题选择
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