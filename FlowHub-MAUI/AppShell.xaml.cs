namespace FlowHub_MAUI
{
    public partial class AppShell : Shell
    {
        public AppShell(HomePageVM vm)
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(HomeD), typeof(HomeD));
            Routing.RegisterRoute(nameof(SettingsD), typeof(SettingsD));
        }
    }
}
