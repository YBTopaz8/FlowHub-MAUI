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
    private readonly CountryAndCurrencyCodes countryAndCurrency = new();

    LoginNavs NavFunctions = new();
    public LoginVM(ISettingsServiceRepository sessionServiceRepo, IUsersRepository userRepo)
    {
        settingsService = sessionServiceRepo;
        userService = userRepo;
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
    public UsersModel currentUser = new();

    private string userCurrency;

    [ObservableProperty]
    private bool errorMessage = false;
    
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
    private bool isBusy=false;

    [ObservableProperty]
    private bool showLoginMessage = false;

    readonly string LoginDetectFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "QuickLogin.text");
    [RelayCommand]
    public async void PageLoaded()
    {
        //deletedLoginDetectFile();
        //await userService.dropCollection();


        HasLoginRemembered = QuickLoginDetectionFile();
        CountryNamesList = countryAndCurrency.GetCountryNames();
        //if (Connectivity.Current.NetworkAccess.Equals(NetworkAccess.Internet))
        //{
        //    await onlineService.GetOnlineConnectionAsync();
        //}
        if (await userService.CheckIfAnyUserExists())
        {
            Username = await settingsService.GetPreference<string>("Username", null);
            userId = await settingsService.GetPreference<string>(nameof(CurrentUser.Id), null);
            CurrentUser.Id= userId;
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
        if (userCurrency is null)
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
                await settingsService.SetPreference(nameof(CurrentUser.Id), CurrentUser.Id.ToString());
                await settingsService.SetPreference("Username", CurrentUser.Username);
                await settingsService.SetPreference(nameof(CurrentUser.UserCurrency), CurrentUser.UserCurrency);
                if (HasLoginRemembered)
                {
                    if (!File.Exists(LoginDetectFile))
                    {
                        File.Create(LoginDetectFile).Close();
                    }
                }
                else
                {
                    if (File.Exists(LoginDetectFile))
                    {
                        File.Delete(LoginDetectFile);
                    }
                }
                IsQuickLoginVisible = false;
                NavFunctions.GoToHomePage();
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
        IsBusy = true;
        var checkedUser = await userService.GetUserAsync(CurrentUser.Email, CurrentUser.Password);
        if (checkedUser is null)
        {
            IsBusy = false;
            ErrorMessage = true;
        }
        else
        {
            IsBusy = false;
            if (checkedUser.RememberLogin)
            {
                if (!File.Exists(LoginDetectFile))
                {
                    File.Create(LoginDetectFile).Close();
                }
            }
            else
            {
                if(File.Exists(LoginDetectFile))
                {
                    File.Delete(LoginDetectFile);
                }
            }
            CurrentUser = checkedUser;

            // await userService.AddUserAsync(CurrentUser, false);
            userService.OfflineUser = await userService.GetUserAsync(CurrentUser.Id); //initialized user to be used by the entire app
            await settingsService.SetPreference<string>(nameof(CurrentUser.Id), CurrentUser.Id.ToString());
            await settingsService.SetPreference<string>("Username", CurrentUser.Username);
            await settingsService.SetPreference<string>(nameof(CurrentUser.UserCurrency), CurrentUser.UserCurrency);
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
            userService.OfflineUser = await userService.GetUserAsync(CurrentUser.Id); //initialized user to be used by the entire app            
                       
            NavFunctions.GoToHomePage();
        }
        else
        {
            ShowLoginMessage = true;
        }
    }

    bool QuickLoginDetectionFile()
    {
        if (File.Exists(LoginDetectFile))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void deletedLoginDetectFile()
    {
        File.Delete(LoginDetectFile);
    }
}
