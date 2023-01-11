using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlowHub.DataAccess.IRepositories;
using FlowHub.Models;
using FlowHub.Main.Platforms.NavigationMethods;
using System.Diagnostics;
using FlowHub.Main.AdditionalResourcefulAPIClasses;

namespace FlowHub.Main.ViewModels;

public partial class LoginVM : ObservableObject
{
    private readonly ISettingsServiceRepository settingsService;
    private readonly IUsersRepository userService;
    private readonly IExpendituresRepository expenditureService;
    private readonly CountryAndCurrencyCodes countryAndCurrency = new();

    readonly LoginNavs NavFunctions = new();
    public LoginVM(ISettingsServiceRepository sessionServiceRepo, IUsersRepository userRepo, IExpendituresRepository expRepo)
    {
        settingsService = sessionServiceRepo;
        userService = userRepo;
        expenditureService = expRepo;
    }

    [ObservableProperty]
    public List<string> countryNamesList = new();

    [ObservableProperty]
    public string username;

    [ObservableProperty]
    public double pocketMoney;

    [ObservableProperty]
    public bool hasLoginRemembered = true;

    [ObservableProperty]
    public UsersModel currentUser;

    private string userCurrency;

    [ObservableProperty]
    private bool errorMessageVisible = false;

    [ObservableProperty]
    private bool errorMessagePicker = false;

    private string userId;

    [ObservableProperty]
    private bool isLoginFormVisible = false;

    [ObservableProperty]
    private bool isRegisterFormVisible = false;

    [ObservableProperty]
    private bool isQuickLoginVisible = false;

    [ObservableProperty]
    private bool registerAccountOnline = false;

    [ObservableProperty]
    private bool isBusy=false;

    [ObservableProperty]
    private bool showQuickLoginErrorMessage = false;

    [ObservableProperty]
    private bool isLoginOnlineButtonClicked = false;

    readonly string LoginDetectFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "QuickLogin.text");

    [RelayCommand]
    public async void PageLoaded()
    {
        if (IsQuickLoginDetectionFilePresent())
        {
            Username = await settingsService.GetPreference<string>("Username", null);
            userId = await settingsService.GetPreference<string>(nameof(CurrentUser.Id), null);
            IsQuickLoginVisible = true;
        }
        else
        {
            CurrentUser = new();

            HasLoginRemembered = false;
            CountryNamesList = countryAndCurrency.GetCountryNames();

            if (await userService.CheckIfAnyUserExists())
            {
                CurrentUser.Id = userId;
                if (userId is null)
                {
                    IsLoginFormVisible = true;
                    HasLoginRemembered = false;
                }
                IsLoginFormVisible = false;
            }
            else
            {
                await settingsService.ClearPreferences();
                HasLoginRemembered = false;
            }
        }
    }

    [RelayCommand]
    public void CurrencyFromCountryPicked(string countryName)
    {
        var dict = countryAndCurrency.LoadDictionaryWithCountryAndCurrency();
        CurrentUser.UserCountry = countryName;
        dict.TryGetValue(countryName, out userCurrency);

        Debug.WriteLine($"The Country Name is {countryName}, and its currency is {userCurrency}");
    }

    [RelayCommand]
    public async void GoToHomePageFromRegister()
    {
        if (userCurrency is null) //currency is null b/c the country was not chosen
        {
            ErrorMessagePicker = true;
        }
        else
        {
            CurrentUser.Id = Guid.NewGuid().ToString();
            CurrentUser.UserCurrency = userCurrency;
            CurrentUser.PocketMoney = PocketMoney;
            CurrentUser.RememberLogin = true;
            if (await userService.AddUserAsync(CurrentUser))
            {
                await settingsService.SetPreference(nameof(CurrentUser.Id), CurrentUser.Id);
                await settingsService.SetPreference("Username", CurrentUser.Username);
                await settingsService.SetPreference(nameof(CurrentUser.UserCurrency), CurrentUser.UserCurrency);

                if (!File.Exists(LoginDetectFile))
                {
                    File.Create(LoginDetectFile).Close();
                }

                if (RegisterAccountOnline && Connectivity.NetworkAccess.Equals(NetworkAccess.Internet))
                {
                    if (await userService.AddUserOnlineAsync(CurrentUser))
                    {
                        await Shell.Current.DisplayAlert("User Registration", "Online Account Created !", "Ok");
                        NavFunctions.GoToHomePage();
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("User Registration", "Online Account Exists Already !", "Ok");
                    }
                }

                IsQuickLoginVisible = false;
            }
            else
            {
                Debug.WriteLine("Failed to add user");
            }
        }
    }

    [RelayCommand]
    public async void GoToHomePageFromLogin()
    {

        ErrorMessageVisible = false;
        IsBusy = true;
        UsersModel User = new();
        if (IsLoginOnlineButtonClicked)
        {
            User = await userService.GetUserOnlineAsync(CurrentUser);
        }
        else
        {
            User = await userService.GetUserAsync(CurrentUser.Email.Trim(), CurrentUser.Password);
        }

        if (User is null)
        {
            IsBusy = false;
            ErrorMessageVisible = true;
        }
        else
        {
            if (!File.Exists(LoginDetectFile))
            {
                File.Create(LoginDetectFile).Close();
            }
            CurrentUser = User;
            userService.OfflineUser = await userService.GetUserAsync(CurrentUser.Id); //initialized user to be used by the entire app
            await settingsService.SetPreference<string>(nameof(CurrentUser.Id), CurrentUser.Id);
            await settingsService.SetPreference<string>("Username", CurrentUser.Username);
            await settingsService.SetPreference<string>(nameof(CurrentUser.UserCurrency), CurrentUser.UserCurrency);

            await expenditureService.SynchronizeExpendituresAsync(CurrentUser.Email, CurrentUser.Password);

            IsBusy = false;

            IsQuickLoginVisible = true;
            NavFunctions.GoToHomePage();
        }
    }

    [RelayCommand]
    public async void QuickLogin()
    {
        if (File.Exists(LoginDetectFile))
        {
            IsQuickLoginVisible = false;
            userService.OfflineUser = await userService.GetUserAsync(userId); //initialized user to be used by the entire app                                
            NavFunctions.GoToHomePage();
        }
        else
        {
            ShowQuickLoginErrorMessage = true;
        }
    }

    bool IsQuickLoginDetectionFilePresent()
    {
        return File.Exists(LoginDetectFile);
    }

    void DeletedLoginDetectFile()
    {
        File.Delete(LoginDetectFile);
    }
}
