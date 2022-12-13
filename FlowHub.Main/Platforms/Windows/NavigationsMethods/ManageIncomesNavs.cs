
namespace FlowHub.Main.Platforms.NavigationMethods;

public class ManageIncomesNavs
{
    public void FromManageIncToUpsertIncome(Dictionary<string, object> navParams)
    {
       // await Shell.Current.GoToAsync(nameof(UpSertIncomesM), true, navParams);
    }
    public void FromUpsertIncToManageIncome(Dictionary<string, object> navParams)
    {
       // await Shell.Current.GoToAsync(nameof(ManageIncomesM), true, navParams);
    }
    public async void ReturnOnce()
    {
        await Shell.Current.GoToAsync("..");
    }
}
