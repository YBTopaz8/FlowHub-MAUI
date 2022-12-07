
using FlowHub.Main.Views.Mobile.Incomes;

namespace FlowHub.Main.Platforms.NavigationMethods;

public class ManageIncomesNavs
{
    public async void FromManageIncToUpsertIncome(Dictionary<string, object> navParams)
    {
        await Shell.Current.GoToAsync(nameof(UpSertIncomePageM), true, navParams);
    }
    public async void FromUpsertIncToManageIncome(Dictionary<string, object> navParams)
    {
       await Shell.Current.GoToAsync(nameof(ManageIncomesM), true, navParams);
    }
    public async void ReturnOnce()
    {
        await Shell.Current.GoToAsync("..");
    }
}
