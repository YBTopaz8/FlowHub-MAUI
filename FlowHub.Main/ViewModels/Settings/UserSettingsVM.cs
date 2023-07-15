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

    [ObservableProperty]
    bool isNotInEditingMode;

    [ObservableProperty]
    private ObservableCollection<TaxModel> taxes;

    [RelayCommand]
    public void PageLoaded()
    {
        ActiveUser = userService.OfflineUser;
        PocketMoney = ActiveUser.PocketMoney;
        UserCurrency = ActiveUser.UserCurrency;
        UserCountry = ActiveUser.UserCountry;
        UserEmail = ActiveUser.Email;
        UserName = ActiveUser.Username;
        Taxes = ActiveUser.Taxes is not null ? new ObservableCollection<TaxModel>(ActiveUser.Taxes) : new ObservableCollection<TaxModel>();
        TotalExpendituresAmount = expService.OfflineExpendituresList.Select(x => x.AmountSpent).Sum();
        TotalIncomeAmount = ActiveUser.TotalIncomeAmount;
        IsNotInEditingMode = true;
        SelectCountryCurrency = ActiveUser.UserCurrency;
    }

    public void GetCountryNamesList()
    {
        CountryNamesList = countryAndCurrency.GetCountryNames();
    }

    [RelayCommand]
    public async Task LogOutUser()
    {
        bool response = (bool)await Shell.Current.ShowPopupAsync(new AcceptCancelPopUpAlert("Do You want to Log Out?"));
        if (response)
        {
            string LoginDetectFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "QuickLogin.text");
            File.Delete(LoginDetectFile);

            await userService.DropCollection();

            await NavFunctions.GoToLoginInPage();
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
    public async Task GoToEditUserSettingsPage()
    {
        await Shell.Current.GoToAsync(nameof(EditUserSettingsPageM), true);
    }

    [RelayCommand]
    public async Task UpdateUserInformation()
    {
        bool dialogResult = (bool)await Shell.Current.ShowPopupAsync(new AcceptCancelPopUpAlert("Save Profile?"));
        if (dialogResult)
        {
            ActiveUser.DateTimeOfPocketMoneyUpdate = DateTime.UtcNow;

            if (Taxes is not null)
            {
                ActiveUser.Taxes = Taxes.ToList();
            }

            await expService.GetAllExpendituresAsync();
            if (await userService.UpdateUserAsync(ActiveUser))
            {
                userService.OfflineUser = ActiveUser;

                CancellationTokenSource cancellationTokenSource = new();
                const ToastDuration duration = ToastDuration.Short;
                const double fontSize = 16;
                const string text = "Profile Updated!";
                var toast = Toast.Make(text, duration, fontSize);
                await toast.Show(cancellationTokenSource.Token); //toast a notification about user profile updated
                await Shell.Current.GoToAsync("..", true);
            }
        }
        IsNotInEditingMode = true;
    }

    [RelayCommand]
    public async Task DeleteIdsCollection()
    {
        await expService.DropCollectionIDsToDelete();
    }

    [RelayCommand]
    public async Task AddTax()
    {
        List<string> fieldTitles = new() { "Tax Name", "Tax Percentage" };
        PopUpCloseResult result = (PopUpCloseResult)await Shell.Current.ShowPopupAsync(new InputPopUpPage(InputType.Numeric | InputType.Text, fieldTitles, "Add Tax info", IsDeleteBtnVisible: false));
        if (result.Result == PopupResult.OK)
        {
            Dictionary<string, double> dict = (Dictionary<string, double>)result.Data;
            TaxModel tax = new() { Name = dict.Keys.First(), Rate = dict.Values.First() };

            Taxes ??= new ObservableCollection<TaxModel>();
            Taxes.Add(tax);
        }
    }

    [RelayCommand]
    public async Task ViewEditDeleteTax(TaxModel Selectedtax)
    {
        List<string> fieldTitles = new() { "Tax Name", "Tax Percentage" };
        PopUpCloseResult result = (PopUpCloseResult)await Shell.Current.ShowPopupAsync(new InputPopUpPage(InputType.Numeric | InputType.Text, fieldTitles, "Tax Info", Selectedtax, true));
        if (result.Result == PopupResult.OK)
        {
            int indexOfTax = Taxes.IndexOf(Selectedtax);
            if (indexOfTax != -1)
            {
                Dictionary<string, double> dict = (Dictionary<string, double>)result.Data;
                TaxModel tax = new() { Name = dict.Keys.First(), Rate = dict.Values.First() };
                Taxes[indexOfTax] = tax;
            }
        }
        else if (result.Result == PopupResult.Delete)
        {
            Taxes.Remove(Selectedtax);
        }
    }
}
