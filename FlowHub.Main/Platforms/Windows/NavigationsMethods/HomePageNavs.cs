namespace FlowHub.Main.Platforms.NavigationMethods;

public class HomePageNavs
{
    public async Task FromHomePageToUpsertExpenditure(Dictionary<string, object> navParams)
    {
        await AppShell.Current.GoToAsync(nameof(UpSertExpenditurePageD), navParams);
    }
}
