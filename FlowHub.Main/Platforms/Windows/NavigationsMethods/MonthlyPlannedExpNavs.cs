namespace FlowHub.Main.Platforms.NavigationsMethods;

public class MonthlyPlannedExpNavs
{
    public async Task ToUpSertMonthlyPlanned(Dictionary<string, object> navParams)
    {
        await Shell.Current.GoToAsync(nameof(UpSertMonthlyPlannedExpPageM), true, navParams);
    }

    public async Task ToDetailsMonthlyPlanned(Dictionary<string, object> navParams)
    {
        await Shell.Current.GoToAsync("..", true, navParams);
    }
    public async Task ReturnToDetailsMonthlyPlanned(Dictionary<string, object> navParams)
    {
        await Shell.Current.GoToAsync($"{nameof(DetailsOfMonthlyPlannedExpPageM)}", true, navParams);
    }
    public async Task ReturnOnce()
    {
        await Shell.Current.GoToAsync("..");
    }
    public async Task ReturnOnce(Dictionary<string, object> navParams)
    {
        await Shell.Current.GoToAsync("..", true, navParams);
    }
}
