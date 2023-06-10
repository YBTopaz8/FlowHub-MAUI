
using FlowHub.Main.Views.Desktop.Expenditures;

namespace FlowHub.Main.Platforms.NavigationMethods;

public class HomePageNavs
{
    public async void FromHomePageToUpsertExpenditure(Dictionary<string, object> navParams)
    {
        await AppShell.Current.GoToAsync(nameof(UpSertExpenditurePageD), navParams);
    }
}
