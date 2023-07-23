
namespace FlowHub.Main.Platforms.NavigationMethods;

public class LoginNavs
{
    public async Task GoToHomePage()
    {
        await Shell.Current.GoToAsync("//HomePageD");
    }
    public async Task GoToLoginInPage()
    {
        await Shell.Current.GoToAsync("//LoginD");
    }
}
