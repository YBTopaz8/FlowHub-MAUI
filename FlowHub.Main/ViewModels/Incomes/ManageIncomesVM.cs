namespace FlowHub.Main.ViewModels.Incomes;

public partial class ManageIncomesVM : ObservableObject
{
    private readonly IIncomeRepository incomeService;
    private readonly IUsersRepository userService;

    private readonly ManageIncomesNavs NavFunctions = new();

    public ManageIncomesVM(IIncomeRepository incomeRepository, IUsersRepository usersRepository)
    {
        incomeService = incomeRepository;
        userService = usersRepository;
        incomeRepository.OfflineIncomesListChanged += HandleIncomesListUpdated;
        usersRepository.OfflineUserDataChanged += HandleUserUpdated;
    }

    private void HandleUserUpdated()
    {
        ActiveUser = userService.OfflineUser;
        UserPocketMoney = ActiveUser.PocketMoney;
        UserCurrency = ActiveUser.UserCurrency;
    }

    [ObservableProperty]
    ObservableCollection<IncomeModel> incomesList;

    [ObservableProperty]
    double totalAmount;

    [ObservableProperty]
    int totalIncomes;

    [ObservableProperty]
    string userCurrency;

    [ObservableProperty]
    double userPocketMoney;

    [ObservableProperty]
    bool isBusy;

    [ObservableProperty]
    string incTitle;

    UsersModel ActiveUser = new();

    [RelayCommand]
    public void PageLoaded()
    {
        var user = userService.OfflineUser;
        ActiveUser = user;
        UserPocketMoney = ActiveUser.PocketMoney;
        UserCurrency = ActiveUser.UserCurrency;
        FilterGetAllIncomes();
        //FilterGetIncOfCurrentMonth();
        //await FilterGetAllIncomes();
    }

    [RelayCommand]
    public void FilterGetIncOfCurrentMonth()
    {
        try
        {
            IsBusy = true;
            double totalAmountFromList = 0;
            var incOfCurrentMonth = incomeService.OfflineIncomesList.FindAll(x => x.DateReceived.Month == DateTime.Today.Month)
                .ToList();
            if (incOfCurrentMonth?.Count > 0)
            {
                IsBusy = false;
                IncomesList.Clear();
                foreach (IncomeModel inc in incOfCurrentMonth)
                {
                    IncomesList.Add(inc);
                    totalAmountFromList += inc.AmountReceived;
                }
                TotalAmount = totalAmountFromList;
                TotalIncomes = IncomesList.Count;
                IncTitle = $"Flow In For {DateTime.Now:MMM - yyyy}";
            }
            else
            {
                IsBusy = false;
                IncomesList.Clear();
                TotalIncomes = IncomesList.Count;
                IncTitle = $"Flow Ins For {DateTime.Now:MMM - yyyy}";
                TotalAmount = 0;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception MESSAGE: {ex.Message}");
        }
    }

    bool IsLoaded;
    public void FilterGetAllIncomes()
    {
        try
        {
            if (!IsLoaded)
            {
                IsBusy = true;
                IncTitle = "All Flow Ins";
                var IncList = incomeService.OfflineIncomesList
                    .Where(x => !x.IsDeleted )
                    .OrderByDescending(x => x.DateReceived)
                    .ToList();

                IncomesList = new ObservableCollection<IncomeModel>(IncList);

                TotalAmount = IncList.AsParallel().Sum(x => x.AmountReceived);
                TotalIncomes = IncList.Count;

                IsLoaded = true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception MESSAGE: {ex.Message}");
        }
    }

    private void HandleIncomesListUpdated()
    {
        var IncList = incomeService.OfflineIncomesList
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.DateReceived)
            .ToList();
        IncomesList = new ObservableCollection<IncomeModel>(IncList);
        OnPropertyChanged(nameof(IncomesList));

        TotalAmount = IncList.AsParallel().Sum(x => x.AmountReceived);
        TotalIncomes = IncList.Count;
    }

    [RelayCommand]
    public void FilterGetIncOfToday()
    {
        try
        {
            IsBusy = true;
            double totalAmountFromList = 0;
            var incOfToday = incomeService.OfflineIncomesList.FindAll(x => x.DateReceived.Day == DateTime.Today.Day)
                .ToList();
            if (incOfToday?.Count > 0)
            {
                IsBusy = false;
                IncomesList.Clear();
                foreach (IncomeModel inc in incOfToday)
                {
                    IncomesList.Add(inc);
                    totalAmountFromList += inc.AmountReceived;
                }
                TotalAmount = totalAmountFromList;
                TotalIncomes = IncomesList.Count;
                IncTitle = "Today's Flow Ins";
            }
            else
            {
                IsBusy = false;
                IncomesList.Clear();
                TotalIncomes = IncomesList.Count;
                IncTitle = "Today's Flow Ins";
                TotalAmount = 0;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception MESSAGE: {ex.Message}");
        }
    }

    [RelayCommand]
    public void FilterGetIncOfSpecificMonth(DateTime specificDate)
    {
        try
        {
            IsBusy = true;
            double totalAmountFromList = 0;
            var incOfSpecificMonth = incomeService.OfflineIncomesList.FindAll(x => x.DateReceived.Month == specificDate.Month)
                .ToList();
            if (incOfSpecificMonth?.Count > 0)
            {
                IsBusy = false;
                IncomesList.Clear();
                foreach (IncomeModel inc in incOfSpecificMonth)
                {
                    IncomesList.Add(inc);
                    totalAmountFromList += inc.AmountReceived;
                }
                TotalAmount = totalAmountFromList;
                TotalIncomes = IncomesList.Count;
                IncTitle = $"Flow In For {DateTime.Now:MMM - yyyy}";
            }
            else
            {
                IsBusy = false;
                IncomesList.Clear();
                TotalIncomes = IncomesList.Count;
                IncTitle = $"Flow in For {DateTime.Now:MMM - yyyy}";
                TotalAmount = 0;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception MESSAGE: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task ShowAddIncomePopUp()
    {
        if (ActiveUser is null)
        {
            Debug.WriteLine("Can't Open because Active User is Null");
            await Shell.Current.DisplayAlert("Wait", "Please Wait", "OK");
        }
        else
        {
            var  newIncome = new IncomeModel(){DateReceived = DateTime.Now};
            const string PageTitle = "Add New Income";
            const bool isAdd = true;

            await AddEditIncome(newIncome, PageTitle, isAdd);
        }
    }

    private async Task AddEditIncome(IncomeModel newIncome, string pageTitle, bool isAdd)
    {
        var newUpserIncomeVM = new UpSertIncomeVM(incomeService, userService,newIncome, pageTitle, isAdd, ActiveUser);
        var UpSertResult = (PopUpCloseResult)await Shell.Current.ShowPopupAsync(new UpSertIncomePopUp(newUpserIncomeVM));
    }

    [RelayCommand]
    public async Task ShowEditIncomePopUp(IncomeModel income)
    {
        await AddEditIncome(income, "Edit Flow In", false);
    }

    [RelayCommand]
    public async Task DeleteIncomeBtn(IncomeModel income)
    {
        CancellationTokenSource cancellationTokenSource = new();
        const ToastDuration duration = ToastDuration.Short;
        const double fontSize = 14;
        const string text = "Income Deleted";
        var toast = Toast.Make(text, duration, fontSize);

        bool response = (bool)await Shell.Current.ShowPopupAsync(new AcceptCancelPopUpAlert("Confirm Deletion?"));
        if (response)
        {
            var updateDateTime = DateTime.UtcNow;
            income.UpdatedDateTime = updateDateTime;
            var deleteResponse = await incomeService.DeleteIncomeAsync(income);
            if (deleteResponse)
            {
                ActiveUser.TotalIncomeAmount -= income.AmountReceived;
                ActiveUser.PocketMoney -= income.AmountReceived;
                UserPocketMoney -= income.AmountReceived;
                ActiveUser.DateTimeOfPocketMoneyUpdate = updateDateTime;

                await userService.UpdateUserAsync(ActiveUser);
                IncomesList.Remove(income);

                await toast.Show(cancellationTokenSource.Token);
            }
        }
    }

    [RelayCommand]
    public async Task PrintIncomesBtn()
    {
        if (IncomesList?.Count < 1)
        {
            await Shell.Current.ShowPopupAsync(new ErrorNotificationPopUpAlert("Cannot Save an Empty List to PDF"));
        }
        else
        {
            await Shell.Current.ShowPopupAsync(new ErrorNotificationPopUpAlert("Saved"));
        }
    }

    [RelayCommand]
    public async Task ResetUserPocketMoney(double amount)
    {
        if (amount != 0)
        {
            ActiveUser.PocketMoney = amount;
            ActiveUser.DateTimeOfPocketMoneyUpdate = DateTime.UtcNow;
            userService.OfflineUser = ActiveUser;
            await userService.UpdateUserAsync(ActiveUser);

            CancellationTokenSource cancellationTokenSource = new();
            const ToastDuration duration = ToastDuration.Short;
            const double fontSize = 16;
            const string text = "User Balance Updated!";
            var toast = Toast.Make(text, duration, fontSize);
            await toast.Show(cancellationTokenSource.Token); //toast a notification about exp deletion

            PageLoaded();
        }
    }

    [RelayCommand]
    public  void ShowFilterPopUpPage()
    {
        //var filterOption = (string)await Shell.Current.ShowPopupAsync(new FilterOptionsPopUp("test"));
        //if (filterOption.Equals("Filter_All"))
        //{
        //    FilterGetAllIncomes();
        //}
        //else if (filterOption.Equals("Filter_Today"))
        //{
        //    FilterGetIncOfToday();
        //}
        //else if (filterOption.Equals("Filter_CurrMonth"))
        //{
        //    FilterGetIncOfCurrentMonth();
        //}
        //else
        //{
        //    //nothing was chosen
        //}

    }
}
