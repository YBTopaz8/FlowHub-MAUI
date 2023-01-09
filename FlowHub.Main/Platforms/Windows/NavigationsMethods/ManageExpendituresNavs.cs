using FlowHub.Main.Views.Desktop.Expenditures;

namespace FlowHub.Main.Platforms.NavigationMethods;

public class ManageExpendituresNavs
{
    public async void FromManageExpToUpsertExpenditures(Dictionary<string, object> navParams)
    {
        await Shell.Current.GoToAsync(nameof(UpSertExpenditurePageD),true, navParams);
    }
    public async void FromUpsertExpToManageExpenditures(Dictionary<string, object> navParams)
    {
        await Shell.Current.GoToAsync(nameof(ManageExpendituresD), true, navParams);
    }
    public void FromManageExpToSingleMonthStats(Dictionary<string, object> navParams)
    {
        //await Shell.Current.GoToAsync(nameof(SingleMonthStatsPageM), true, navParams);
    }

    public async void ReturnOnce()
    {
        await Shell.Current.GoToAsync("..", true);
    }
}
