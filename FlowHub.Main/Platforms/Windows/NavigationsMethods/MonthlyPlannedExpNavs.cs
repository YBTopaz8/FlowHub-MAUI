using FlowHub.Main.Views.Mobile.Expenditures.PlannedExpenditures.MonthlyPlannedExp;

namespace FlowHub.Main.Platforms.NavigationsMethods;

public class MonthlyPlannedExpNavs
{
    public async void ToUpSertMonthlyPlanned(Dictionary<string, object> navParams)
    {
        await Shell.Current.GoToAsync(nameof(UpSertMonthlyPlannedExpPageM), true, navParams);
    }

    public async void ToDetailsMonthlyPlanned(Dictionary<string, object> navParams)
    {
        await Shell.Current.GoToAsync("..", true, navParams);
    }
    public async void ReturnToDetailsMonthlyPlanned(Dictionary<string, object> navParams)
    {
        await Shell.Current.GoToAsync($"{nameof(DetailsOfMonthlyPlannedExpPageM)}", true, navParams);
    }
    public async void ReturnOnce()
    {
        await Shell.Current.GoToAsync("..");
    }
    public async void ReturnOnce(Dictionary<string, object> navParams)
    {
        await Shell.Current.GoToAsync("..", true, navParams);
    }
}
