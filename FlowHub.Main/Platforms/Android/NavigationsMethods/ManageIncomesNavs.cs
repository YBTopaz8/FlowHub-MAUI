
using FlowHub.Main.Views.Mobile.Incomes;

namespace FlowHub.Main.Platforms.NavigationMethods;

public class ManageIncomesNavs
{
    public async Task FromManageIncToUpsertIncome(Dictionary<string, object> navParams)
    {
        await Shell.Current.GoToAsync(nameof(UpSertIncomePageM), true, navParams);
    }
    public async Task FromUpsertIncToManageIncome(Dictionary<string, object> navParams)
    {
       await Shell.Current.GoToAsync(nameof(ManageIncomesM), true, navParams);
    }
    public async Task ReturnOnce()
    {
        await Shell.Current.GoToAsync("..");
    }
}
