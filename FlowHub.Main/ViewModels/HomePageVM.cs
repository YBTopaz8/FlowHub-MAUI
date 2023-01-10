using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlowHub.DataAccess.IRepositories;
using FlowHub.Models;
using FlowHub.Main.Platforms.NavigationMethods;
using System.Diagnostics;

//This is the view model for the HOME PAGE
namespace FlowHub.Main.ViewModels;

[QueryProperty(nameof(TotalExp), nameof(TotalExp))]
public partial class HomePageVM : ObservableObject
{
    private readonly IExpendituresRepository _expendituresService;
    private readonly ISettingsServiceRepository settingsService;
    private readonly IUsersRepository userService;

    private readonly HomePageNavs NavFunction = new();

    public HomePageVM(IExpendituresRepository expendituresRepository, ISettingsServiceRepository settingsServiceRepo, IUsersRepository usersRepository)
    {
        _expendituresService = expendituresRepository;
        settingsService = settingsServiceRepo;
        userService = usersRepository;
    }

    [ObservableProperty]
    private ExpendituresModel _expendituresDetails = new() { DateSpent = DateTime.Now };

    [ObservableProperty]
    public int totalExp;

    [ObservableProperty]
    public string username;
    [ObservableProperty]
    public string userCurrency;
    [ObservableProperty]
    public double pocketMoney = 0;

    [ObservableProperty]
    private UsersModel activeUser = new ();

    [RelayCommand]
    public async void DisplayInfo()
    {
        string Id = await settingsService.GetPreference<string>("Id", "error");

        var user = userService.OfflineUser;
        ActiveUser = user;

        Username = ActiveUser.Username;
        PocketMoney = ActiveUser.PocketMoney;
        UserCurrency = ActiveUser.UserCurrency;
        var ListOfExp = await _expendituresService.GetAllExpendituresAsync();

        ExpendituresDetails = ListOfExp.Count != 0 ? ListOfExp.OrderByDescending(s => s.DateSpent).First() : (new() { DateSpent = DateTime.Now });
    }

    [RelayCommand]
    public void GetTotal()
    {
        try
        {
            var expList = _expendituresService.OfflineExpendituresList;
            TotalExp = expList.Count;

        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }
    [RelayCommand]
    public async void GoToAddExpenditurePage()
    {

        if (ActiveUser is null)
        {
            await Shell.Current.DisplayAlert("Wait", "Cannot go", "Ok");
        }
        else
        {
            Dictionary<string, object> navParam = new()
            {
                { "SingleExpenditureDetails", new ExpendituresModel { DateSpent = DateTime.Now } },
                { "PageTitle", new string("Add New Flow Out") },
                { "IsAdd", true },
                { "ActiveUser", ActiveUser }
            };
            NavFunction.FromHomePageToUpsertExpenditure(navParam);
        }
    }



    [RelayCommand]
    public void SaveToJSON()
    {

        /*
        var User = new UsersModel
        {
            Id = 1,
            Username = "Yvan"
            
        };
        if (  SessionService.SaveSessionAsync(User, true))
        {
            Debug.WriteLine("======>> SAVED");
        }
        else
        {

            Debug.WriteLine("======>> UNSAVED");
        }*/
    }

}
