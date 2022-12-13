using FlowHub.Main.Views.Mobile.Expenditures.PlannedExpenditures.MonthlyPlannedExp;
using System.Diagnostics;

namespace FlowHub.Main.Platforms.NavigationsMethods;

public class MonthlyPlannedExpNavs
{
    public async void ToUpSertMonthlyPlanned(Dictionary<string, object> NavigationParamaters)
    {
        await Shell.Current.GoToAsync(nameof(UpSertMonthlyPlannedExpPageM), true, NavigationParamaters);
    }

    public async void ToDetailsMonthlyPlanned(Dictionary<string, object> NavigationParamaters)
    {
        await Shell.Current.GoToAsync(nameof(DetailsOfMonthlyPlannedExpPageM), true, NavigationParamaters);
    }
    public async void ReturnToDetailsMonthlyPlanned(Dictionary<string, object> NavigationParamaters)
    {
       await Shell.Current.GoToAsync("../DetailsOfMonthlyPlannedExpPageM", true, NavigationParamaters);
    }

    public async void ReturnOnce(Dictionary<string, object> NavigationParamaters)
    {
        await Shell.Current.GoToAsync("..", true, NavigationParamaters);
    }
    public async void ReturnOnce()
    {
        await Shell.Current.GoToAsync("..");
    }
}
