
using FlowHub.Main.Views.Mobile.Expenditures;

namespace FlowHub.Main.Platforms.NavigationMethods;

public class HomePageNavs
{
    public async void FromHomePageToUpsertExpenditure(Dictionary<string, object> navParams)
    {
        await Shell.Current.GoToAsync(nameof(UpSertExpenditurePageM), navParams);
    }
}
