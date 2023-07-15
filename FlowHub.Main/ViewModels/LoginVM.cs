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
    private bool errorMessageVisible;

    private string userId;

    [ObservableProperty]
    private bool isLoginFormVisible;

    [ObservableProperty]
    private bool isRegisterFormVisible;

    [ObservableProperty]
    private bool isQuickLoginVisible;

    [ObservableProperty]
    private bool registerAccountOnline;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool showQuickLoginErrorMessage;

    [ObservableProperty]
    private bool isLoginOnlineButtonClicked;

    readonly string LoginDetectFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "QuickLogin.text");

    [RelayCommand]
    public async Task PageLoaded()
    {
        if (IsQuickLoginDetectionFilePresent())
        {
            Username = await settingsService.GetPreference<string>("Username", null);
            if (Username is null)
            {
                File.Delete(LoginDetectFile);
                await PageLoaded();
            }
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

    [ObservableProperty]
    string selectedCountry;

    [RelayCommand]
    public void CurrencyFromCountryPicked()
    {
        var dict = countryAndCurrency.LoadDictionaryWithCountryAndCurrency();
        CurrentUser.UserCountry = SelectedCountry;
        dict.TryGetValue(SelectedCountry, out userCurrency);

        Debug.WriteLine($"The Country Name is {SelectedCountry}, and its currency is {userCurrency}");
    }

    [RelayCommand]
    public async Task GoToHomePageFromRegister()
    {
        CurrentUser.Email = CurrentUser.Email.Trim();
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
                    await NavFunctions.GoToHomePage();
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

    [RelayCommand]
    public async Task GoToHomePageFromLogin()
    {
        ErrorMessageVisible = false;
        IsBusy = true;
        UsersModel User = new();

        CancellationTokenSource cancellationTokenSource = new();
        ToastDuration duration = ToastDuration.Short;
        double fontSize = 14;

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
            await NavFunctions.GoToHomePage();
        }
    }

    public async Task QuickLogin()
    {
        if (File.Exists(LoginDetectFile))
        {
            IsQuickLoginVisible = false;
            userService.OfflineUser = await userService.GetUserAsync(userId); //initialized user to be used by the entire app                                
            await NavFunctions.GoToHomePage();
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
