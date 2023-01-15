

namespace FlowHub.Main.Platforms.NavigationMethods;

public class LoginNavs
{
    public async void GoToHomePage()
    {
        await AppShellMobile.Current.GoToAsync($"//HomePageM");
    }
    public async void GoToLoginInPage()
    {
        await AppShellMobile.Current.GoToAsync($"//LoginM");
    }
}
