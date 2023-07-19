namespace FlowHub.Main.Platforms.NavigationsMethods;

public class MonthlyPlannedExpNavs
{
    public async Task ToUpSertMonthlyPlanned(Dictionary<string, object> NavigationParamaters)
    {
        await Shell.Current.GoToAsync(nameof(UpSertMonthlyPlannedExpPageM), true, NavigationParamaters);
    }

    public async Task ToDetailsMonthlyPlanned(Dictionary<string, object> NavigationParamaters)
    {
        await Shell.Current.GoToAsync(nameof(DetailsOfMonthlyPlannedExpPageM), true, NavigationParamaters);
    }
    public async Task ReturnToDetailsMonthlyPlanned(Dictionary<string, object> NavigationParamaters)
    {
        await Shell.Current.GoToAsync("../DetailsOfMonthlyPlannedExpPageM", true, NavigationParamaters);
    }

    public async Task ReturnOnce(Dictionary<string, object> NavigationParamaters)
    {
        await Shell.Current.GoToAsync("..", true, NavigationParamaters);
    }
    public async Task ReturnOnce()
    {
        await Shell.Current.GoToAsync("..");
    }
}
