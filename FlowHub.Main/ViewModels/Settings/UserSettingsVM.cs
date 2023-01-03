using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlowHub.DataAccess.IRepositories;
using FlowHub.Main.AdditionalResourcefulAPIClasses;
using FlowHub.Main.Platforms.NavigationMethods;
using FlowHub.Main.PopUpPages;
using FlowHub.Models;
using System.Diagnostics;

namespace FlowHub.Main.ViewModels.Settings;

public partial class UserSettingsVM : ObservableObject
{
    private readonly IUsersRepository userService;
    private readonly CountryAndCurrencyCodes countryAndCurrency = new();

    LoginNavs NavFunctions = new();
    public UserSettingsVM(IUsersRepository usersRepository)
    {
        userService = usersRepository;
    }
    [ObservableProperty]
    public List<string> countryNamesList = new();

    [ObservableProperty]
    public double pocketMoney;
    [ObservableProperty]
    public string userCurrency;
    [ObservableProperty]
    public string userCountry;
    [ObservableProperty]
    public string userName;
    [ObservableProperty]
    public string userEmail;
    [ObservableProperty]
    public double totalIncomeAmount;
    [ObservableProperty]
    public double totalExpendituresAmount;


    [ObservableProperty]
    private UsersModel activeUser = new();

    [RelayCommand]
    public void PageLoaded()
    {
        activeUser = userService.OfflineUser;
        PocketMoney = activeUser.PocketMoney;
        UserCurrency = activeUser.UserCurrency;
        UserCountry = activeUser.UserCountry;
        UserEmail = activeUser.Email;
        UserName = activeUser.Username;
        TotalExpendituresAmount = activeUser.TotalExpendituresAmount;
        TotalIncomeAmount = activeUser.TotalIncomeAmount;
        CountryNamesList = countryAndCurrency.GetCountryNames();
    }

    [RelayCommand]
    public async void LogOutUser()
    {
        bool response = (bool)await Shell.Current.ShowPopupAsync(new AcceptCancelPopUpAlert("Do You want to Log Out?"));
        if (response)
        {
            string LoginDetectFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "QuickLogin.text");
            File.Delete(LoginDetectFile);

            //await userService.DropCollection();
            
            NavFunctions.GoToLoginInPage();
        }
    }

    [RelayCommand]
    public void CurrencyFromCountryPicked(string countryName)
    {
        Dictionary<string, string> dictOfCountry = countryAndCurrency.LoadDictionaryWithCountryAndCurrency();
        dictOfCountry.TryGetValue(countryName, out var country);

        Debug.WriteLine($"The Country Name is {countryName}, and its currency is {country}");
    }
}
