namespace FlowHub.Main.Platforms.NavigationMethods;

public static class ManageExpendituresNavs
{
    public static async Task FromManageExpToUpsertExpenditures(Dictionary<string, object> navParams)
    {
        await Shell.Current.GoToAsync(nameof(UpSertExpenditurePageD), true, navParams);
    }
    public static async Task FromUpsertExpToManageExpenditures(Dictionary<string, object> navParams)
    {
        await Shell.Current.GoToAsync(nameof(ManageExpendituresPageD), true, navParams);
    }
    public static async Task FromManageExpToSingleMonthStats(Dictionary<string, object> navParams)
    {
        await Shell.Current.GoToAsync(nameof(StatisticsPageD), true, navParams);
    }

    public static async Task ReturnOnce()
    {
        await Shell.Current.GoToAsync("..", true);
    }
}
