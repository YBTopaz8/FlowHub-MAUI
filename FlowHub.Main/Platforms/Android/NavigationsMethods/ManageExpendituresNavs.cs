
using FlowHub.Main.Views.Mobile.Expenditures;
using FlowHub.Main.Views.Mobile.Statistics;

namespace FlowHub.Main.Platforms.NavigationMethods;

public class ManageExpendituresNavs
{
    public async void FromManageExpToUpsertExpenditures(Dictionary<string, object> navParams)
    {
        await Shell.Current.GoToAsync(nameof(UpSertExpenditurePageM),true, navParams);
    }
    public async void FromUpsertExpToManageExpenditures(Dictionary<string, object> navParams)
    {
        await Shell.Current.GoToAsync(nameof(ManageExpendituresM), true, navParams);
    }

    public async void FromManageExpToSingleMonthStats(Dictionary<string, object> navParams)
    {
        await Shell.Current.GoToAsync(nameof(SingleMonthStatsPageM), true, navParams);
    }
    public async void ReturnOnce()
    {
        await Shell.Current.GoToAsync("..", true);
    }
}
