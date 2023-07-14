
namespace FlowHub.Main.Platforms.NavigationMethods;

public class LoginNavs
{
    public async Task GoToHomePage()
    {
        await AppShellMobile.Current.GoToAsync($"//HomePageM");
    }
    public async Task GoToLoginInPage()
    {
        await AppShellMobile.Current.GoToAsync($"//LoginM");
    }
}
