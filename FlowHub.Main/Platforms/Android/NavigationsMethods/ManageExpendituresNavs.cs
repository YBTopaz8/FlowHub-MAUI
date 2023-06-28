
using FlowHub.Main.Views.Mobile.Expenditures;
using FlowHub.Main.Views.Mobile.Statistics;

namespace FlowHub.Main.Platforms.NavigationMethods;

public static class ManageExpendituresNavs
{
    public static async Task FromManageExpToUpsertExpenditures(Dictionary<string, object> navParams)
    {
        await Shell.Current.GoToAsync(nameof(UpSertExpenditurePageM),true, navParams);
    }
    public static async Task FromUpsertExpToManageExpenditures(Dictionary<string, object> navParams)
    {
        await Shell.Current.GoToAsync(nameof(ManageExpendituresM), true, navParams);
    }

    public static async Task FromManageExpToSingleMonthStats(Dictionary<string, object> navParams)
    {
        await Shell.Current.GoToAsync(nameof(StatisticsPageM), true, navParams);
    }
    public static async Task ReturnOnce()
    {
        await Shell.Current.GoToAsync("..", true);
    }
}
