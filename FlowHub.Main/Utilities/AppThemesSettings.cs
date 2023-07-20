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
            Application.Current.UserAppTheme = Theme switch
            {
                0 => AppTheme.Light,
                1 => AppTheme.Dark,
                _ => AppTheme.Light,
            };
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