using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlowHub.DataAccess.IRepositories;
using FlowHub.Models;
using FlowHub.Main.Platforms.NavigationMethods;
using System.Diagnostics;
using CommunityToolkit.Maui.Views;
using FlowHub.Main.ViewModels.Expenditures;
using FlowHub.Main.Views;
using FlowHub.Main.Utilities;
using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;

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
    ObservableCollection<ExpendituresModel> _latestExpenditures;
    [ObservableProperty]
    ObservableCollection<IncomeModel> _latestIncomes;

    [ObservableProperty]
    public int totalExp;

    [ObservableProperty]
    public string username;
    [ObservableProperty]
    public string userCurrency;
    [ObservableProperty]
    public double pocketMoney;

    [ObservableProperty]
    private UsersModel activeUser = new ();
        
    public async Task DisplayInfo()
    {
        string Id = await settingsService.GetPreference<string>("Id", "error");

        var user = userService.OfflineUser;
        ActiveUser = user;

        Username = ActiveUser.Username;
        PocketMoney = ActiveUser.PocketMoney;
        UserCurrency = ActiveUser.UserCurrency;
        var ListOfExp = await _expendituresService.GetAllExpendituresAsync();

        LatestExpenditures = ListOfExp.Count != 0 
            ? ListOfExp.OrderByDescending(s => s.DateSpent).Take(5).ToObservableCollection() 
            : new ObservableCollection<ExpendituresModel>();

        
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
    public async Task GoToAddExpenditurePage()
    {
        if (ActiveUser is null)
        {
            Debug.WriteLine("Can't Open PopUp");
            await Shell.Current.DisplayAlert("Wait", "Cannot go", "Ok");
        }
        else
        {
            var newExpenditure = new ExpendituresModel() { DateSpent = DateTime.Now };
            string pageTitle = "Add New Flow Out";
            bool isAdd = true;

            var NewUpSertVM = new UpSertExpenditureVM(_expendituresService, userService, newExpenditure, pageTitle, isAdd, ActiveUser);
            var UpSertResult = (PopUpCloseResult)await Shell.Current.ShowPopupAsync(new UpSertExpendituresPopUp(NewUpSertVM));

            if (UpSertResult.Result == PopupResult.OK)
            {
                ExpendituresModel exp = (ExpendituresModel)UpSertResult.Data;
                //add logic if this exp is the latest in terms of datetime
                LatestExpenditures.Add(exp);
                LatestExpenditures = LatestExpenditures.OrderByDescending(s => s.DateSpent).Take(5).ToObservableCollection();
                PocketMoney -= exp.AmountSpent;
            }
        }
    }
}
