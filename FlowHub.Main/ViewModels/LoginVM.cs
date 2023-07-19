namespace FlowHub.Main.ViewModels;

public partial class LoginVM : ObservableObject
{
    private readonly ISettingsServiceRepository settingsRepo;
    private readonly IUsersRepository userRepo;
    private readonly IExpendituresRepository expenditureRepo;
    private readonly IIncomeRepository incomeRepo;
    private readonly IDebtRepository debtRepo;
    private readonly IDataAccessRepo dataAccessRepo;
    private readonly CountryAndCurrencyCodes countryAndCurrency = new();

    readonly LoginNavs NavFunctions = new();
    public LoginVM(ISettingsServiceRepository sessionServiceRepository, IUsersRepository userRepository, IExpendituresRepository expRepository,
                    IIncomeRepository incomeRepository, IDebtRepository debtRepository, IDataAccessRepo dataAccessRepo)
    {
        settingsRepo = sessionServiceRepository;
        userRepo = userRepository;
        expenditureRepo = expRepository;
        incomeRepo = incomeRepository;
        debtRepo = debtRepository;
        this.dataAccessRepo = dataAccessRepo;
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
            if (!await userRepo.CheckIfAnyUserExists())
            {
                File.Delete(LoginDetectFile);
                await PageLoaded();
            }
            Username = await settingsRepo.GetPreference<string>("Username", null);
            if (Username is null)
            {
                File.Delete(LoginDetectFile);
                await PageLoaded();
            }
            IsQuickLoginVisible = true;
            userId = await settingsRepo.GetPreference<string>(nameof(CurrentUser.Id), null);
            userRepo.OfflineUser = await userRepo.GetUserAsync(userId); //initialized user to be used by the entire app  
            //await Task.WhenAll(expenditureRepo.SynchronizeExpendituresAsync(), debtRepo.SynchronizeDebtsAsync(), incomeRepo.SynchronizeIncomesAsync());
        }
        else
        {
            CurrentUser = new();

            HasLoginRemembered = false;
            CountryNamesList = countryAndCurrency.GetCountryNames();

            if (await userRepo.CheckIfAnyUserExists())
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
                await settingsRepo.ClearPreferences();
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
        if (await userRepo.AddUserAsync(CurrentUser))
        {
            await settingsRepo.SetPreference(nameof(CurrentUser.Id), CurrentUser.Id);
            await settingsRepo.SetPreference("Username", CurrentUser.Username);
            await settingsRepo.SetPreference(nameof(CurrentUser.UserCurrency), CurrentUser.UserCurrency);

            if (!File.Exists(LoginDetectFile))
            {
                File.Create(LoginDetectFile).Close();
            }

            if (RegisterAccountOnline && Connectivity.NetworkAccess.Equals(NetworkAccess.Internet))
            {
                if (await userRepo.AddUserOnlineAsync(CurrentUser))
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
        const ToastDuration duration = ToastDuration.Short;
        const double fontSize = 14;

        if (IsLoginOnlineButtonClicked)
        {
            CurrentUser.Email = "8brunel@gmail.com";
            CurrentUser.Password = "Yvan";
            User = await userRepo.GetUserOnlineAsync(CurrentUser);
        }
        else
        {
            User = await userRepo.GetUserAsync(CurrentUser.Email.Trim(), CurrentUser.Password);
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
            userRepo.OfflineUser = await userRepo.GetUserAsync(CurrentUser.Id); //initialized user to be used by the entire app
            await settingsRepo.SetPreference<string>(nameof(CurrentUser.Id), CurrentUser.Id);
            await settingsRepo.SetPreference<string>("Username", CurrentUser.Username);
            await settingsRepo.SetPreference<string>(nameof(CurrentUser.UserCurrency), CurrentUser.UserCurrency);

            await SyncAndNotifyAsync();
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
            userRepo.OfflineUser = await userRepo.GetUserAsync(userId); //initialized user to be used by the entire app                                
            await NavFunctions.GoToHomePage();
        }
        else
        {
            ShowQuickLoginErrorMessage = true;
        }
    }

    private async Task SyncAndNotifyAsync()
    {
        try
        {
            CancellationTokenSource cts = new();
            const ToastDuration duration = ToastDuration.Short;
            const double fontSize = 14;
            string text = "All Synced Up !";
            var toast = Toast.Make(text, duration, fontSize);
            await toast.Show(cts.Token);
        }
        catch (AggregateException aEx)
        {
            foreach (var ex in aEx.InnerExceptions)
            {
                await Shell.Current.ShowPopupAsync(new ErrorPopUpAlert("Error when syncing " + ex.Message));
            }
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
