
namespace FlowHub.Main.Platforms.NavigationMethods;

public class MonthlyExpendituresNavs
{
    public void FromManageMonthlyPlannedToDetailsSingleMonthPlan(Dictionary<string, object> navParams)
    {
    }
    public void NavigateToUpsertMonthlyPlannedExpenditure(Dictionary<string, object> navParams)
    {
    }

    public async void ReturnOnce()
    {
        await Shell.Current.GoToAsync("..", true);
    }
}
