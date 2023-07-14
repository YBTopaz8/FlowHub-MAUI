
namespace FlowHub.Main.Platforms.NavigationMethods;

public class MonthlyExpendituresNavs
{
    //public async Task FromManageMonthlyPlannedToDetailsSingleMonthPlan(Dictionary<string, object> navParams)
    //{

    //}
    //public async Task NavigateToUpsertMonthlyPlannedExpenditure(Dictionary<string, object> navParams)
    //{

    //}

    public async Task ReturnOnce()
    {
        await Shell.Current.GoToAsync("..", true);
    }
}
