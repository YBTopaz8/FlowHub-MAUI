

namespace FlowHub.Main.Platforms.NavigationMethods;

public class LoginNavs
{
    public async void GoToHomePage()
    {
        await Shell.Current.GoToAsync($"//HomePage");
    }
    public async void GoToLoginInPage()
    {
        await Shell.Current.GoToAsync($"//LoginD");
    }
}
