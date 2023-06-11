using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlowHub.DataAccess.IRepositories;
using FlowHub.Main.AdditionalResourcefulAPIClasses;
using FlowHub.Main.Platforms.NavigationMethods;
using FlowHub.Main.PopUpPages;
using FlowHub.Main.Views.Mobile.Settings;
using FlowHub.Models;
using System.Diagnostics;

namespace FlowHub.Main.ViewModels.Settings;

public partial class UserSettingsVM : ObservableObject
{
    private readonly IUsersRepository userService;
    private readonly IExpendituresRepository expService;
    private readonly CountryAndCurrencyCodes countryAndCurrency = new();

    LoginNavs NavFunctions = new();
    public UserSettingsVM(IUsersRepository usersRepository, IExpendituresRepository expendituresRepository)
    {
        userService = usersRepository;
        expService = expendituresRepository;
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
    public string selectCountryCurrency;

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
        TotalExpendituresAmount = expService.OfflineExpendituresList.Select(x => x.AmountSpent).Sum();

        SelectCountryCurrency = ActiveUser.UserCurrency;
    }

    public void GetCountryNamesList()
    {
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

            await userService.DropCollection();

            NavFunctions.GoToLoginInPage();
        }
    }

    [RelayCommand]
    public void CurrencyFromCountryPicked(string countryName)
    {
        Dictionary<string, string> dictOfCountry = countryAndCurrency.LoadDictionaryWithCountryAndCurrency();
        string curr;
        dictOfCountry.TryGetValue(countryName, out curr);
        SelectCountryCurrency = curr;
        ActiveUser.UserCurrency = curr;
        ActiveUser.UserCountry = countryName;

        Debug.WriteLine($"The Country Name is {countryName}, and its currency is {ActiveUser.UserCurrency}");
    }

    [RelayCommand]
    public async void GoToEditUserSettingsPage()
    {
        await Shell.Current.GoToAsync(nameof(EditUserSettingsPageM), true);
    }

    [RelayCommand]
    public async void UpdateUserInformation()
    {
        bool dialogResult = (bool)await Shell.Current.ShowPopupAsync(new AcceptCancelPopUpAlert("Save Profile?"));
        if (dialogResult)
        {
            ActiveUser.DateTimeOfPocketMoneyUpdate = DateTime.UtcNow;
            ActiveUser.TotalExpendituresAmount = 0;
            ActiveUser.PocketMoney = 0;

            await expService.GetAllExpendituresAsync();
            if (await userService.UpdateUserAsync(ActiveUser))
            {
                userService.OfflineUser = ActiveUser;

                CancellationTokenSource cancellationTokenSource = new();
                const ToastDuration duration = ToastDuration.Short;
                const double fontSize = 16;
                string text = "Profile Updated!";
                var toast = Toast.Make(text, duration, fontSize);
                await toast.Show(cancellationTokenSource.Token); //toast a notification about user profile updated
                await Shell.Current.GoToAsync("..", true);
            }
        }
    }
}
