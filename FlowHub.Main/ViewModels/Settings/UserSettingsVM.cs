﻿namespace FlowHub.Main.ViewModels.Settings;

public partial class UserSettingsVM(IUsersRepository usersRepository, IExpendituresRepository expendituresRepository,
    IIncomeRepository incomeRepository, IDebtRepository debtRepository,
    HomePageVM homePageVM) : ObservableObject
{
    private readonly CountryAndCurrencyCodes countryAndCurrency = new();

    LoginNavs NavFunctions = new();
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
    public double totalBorrowedCompletedAmount;
    [ObservableProperty]
    public double totalBorrowedPendingAmount;
    [ObservableProperty]
    public double totalLentCompletedAmount;
    [ObservableProperty]
    public double totalLentPendingAmount;
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
        ActiveUser = usersRepository.OfflineUser;
        PocketMoney = ActiveUser.PocketMoney;
        UserCurrency = ActiveUser.UserCurrency;
        UserCountry = ActiveUser.UserCountry;
        UserEmail = ActiveUser.Email;
        UserName = ActiveUser.Username;
        Taxes = ActiveUser.Taxes is not null ? new ObservableCollection<TaxModel>(ActiveUser.Taxes) : [];
        
        IsNotInEditingMode = true;
        SelectCountryCurrency = ActiveUser.UserCurrency;
        GetTotals();
    }

    private void GetTotals()
    {
        TotalExpendituresAmount = expendituresRepository.OfflineExpendituresList.Select(x => x.AmountSpent).Sum();
        TotalIncomeAmount = ActiveUser.TotalIncomeAmount;

        var filteredAndSortedDebts = debtRepository.OfflineDebtList
                        .Where(x => !x.IsDeleted)
                        .Distinct()
                        .ToList();
        var BorrowedCompletedList = new ObservableCollection<DebtModel>(filteredAndSortedDebts
            .Where(x => x.DebtType == DebtType.Borrowed && x.IsPaidCompletely)
            .OrderBy(x => x.AddedDateTime)); //total of all debts that were paid back to user completely

        var LentCompletedList = new ObservableCollection<DebtModel>(filteredAndSortedDebts
            .Where(x => x.DebtType == DebtType.Lent && x.IsPaidCompletely)
            .OrderBy(x => x.AddedDateTime));//total of all debts that were paid back FROM user completely

        var BorrowedPendingList = new ObservableCollection<DebtModel>(filteredAndSortedDebts
            .Where(x => x.DebtType == DebtType.Borrowed && !x.IsPaidCompletely)
            .OrderBy(x => x.AddedDateTime));//total of all debts that are still waiting to be paid back to user 

        var LentPendingList = new ObservableCollection<DebtModel>(filteredAndSortedDebts
            .Where(x => x.DebtType == DebtType.Lent && !x.IsPaidCompletely)
            .OrderBy(x => x.AddedDateTime)); //total of all debts that are still waiting to be paid back BY user 


        
        TotalBorrowedCompletedAmount = BorrowedCompletedList.Sum(x => x.Amount);
        TotalBorrowedPendingAmount = BorrowedPendingList.Sum(x => x.Amount);
        TotalLentCompletedAmount = LentCompletedList.Sum(x => x.Amount);
        TotalLentPendingAmount = LentPendingList.Sum(x => x.Amount);

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

            await usersRepository.LogOutUserAsync();
            await expendituresRepository.LogOutUserAsync();
            await incomeRepository.LogOutUserAsync();
            await debtRepository.LogOutUserAsync();
            homePageVM._isInitialized = false;

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

            await expendituresRepository.GetAllExpendituresAsync();
            if (await usersRepository.UpdateUserAsync(ActiveUser))
            {
                usersRepository.OfflineUser = ActiveUser;

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
    public void DeleteIdsCollection()
    {
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

    //section for themes in windows version. i'll revise this later
    [ObservableProperty]
    int selectedTheme;
    [ObservableProperty]
    bool isLightTheme;

    public void SetThemeConfig()
    {
        SelectedTheme = AppThemesSettings.ThemeSettings.Theme;
        IsLightTheme = SelectedTheme == 0;
    }
    [RelayCommand]
    public void ThemeToggler()
    {
        SelectedTheme = AppThemesSettings.ThemeSettings.SwitchTheme();
        IsLightTheme = !IsLightTheme;
    }
}
