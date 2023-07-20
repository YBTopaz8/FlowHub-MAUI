namespace FlowHub.Main.Utilities;

public static class AppThemesSettings
{
    public static class ThemeSettings
    {
        // 0 = light, 1 = dark, 2 = black, etc...
        const int defaultTheme = 0;
        const int maxTheme = 1; // change this value based on the number of themes you have

        public static int Theme
        {
            get => Preferences.Default.Get(nameof(Theme), defaultTheme);
            set => Preferences.Default.Set(nameof(Theme), value);
        }

        public static void SetTheme()
        {
            switch (Theme)
            {
                case 0:
                    Application.Current.UserAppTheme = AppTheme.Light;
                    break;
                case 1:
                    Application.Current.UserAppTheme = AppTheme.Dark;
                break;
                default:
                    Application.Current.UserAppTheme = AppTheme.Light;
                    break;
            }
        }

        public static int GetTheme()
        {
            return Theme;
        }
        public static int SwitchTheme()
        {
            Theme = (Theme + 1) % (maxTheme + 1);
            SetTheme();
            return Theme;
        }
    }

    public enum Theme
    {
        Light,
        Dark
    }
}