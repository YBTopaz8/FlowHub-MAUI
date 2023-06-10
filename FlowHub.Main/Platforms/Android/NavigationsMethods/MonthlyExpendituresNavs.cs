
namespace FlowHub.Main.Platforms.NavigationMethods;

public class MonthlyExpendituresNavs
{
    //public async void FromManageMonthlyPlannedToDetailsSingleMonthPlan(Dictionary<string, object> navParams)
    //{

    //}
    //public async void NavigateToUpsertMonthlyPlannedExpenditure(Dictionary<string, object> navParams)
    //{

    //}

    public async void ReturnOnce()
    {
        await Shell.Current.GoToAsync("..", true);
    }
}
