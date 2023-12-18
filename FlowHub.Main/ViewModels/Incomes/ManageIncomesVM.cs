namespace FlowHub.Main.ViewModels.Incomes;

public partial class ManageIncomesVM : ObservableObject
{
    private readonly IIncomeRepository incomeService;
    private readonly IUsersRepository userService;

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

    //Search variables section
    [ObservableProperty]
    DateTime? searchStartDate = null;
    [ObservableProperty]
    DateTime? searchEndDate = null;
    [ObservableProperty]
    double? searchMinPrice;
    [ObservableProperty]
    double? searchMaxPrice;
    [ObservableProperty]
    string searchText;

    bool IsLoaded;
    [RelayCommand]
    public void PageLoaded()
    {
        try
        {
            if (!IsLoaded)
            {
                ApplyChanges();
                IsBusy = true;
                IsLoaded = true;
                IncTitle = "All Flow Ins";
                ActiveUser = userService.OfflineUser;
                UserPocketMoney = ActiveUser.PocketMoney;
                UserCurrency = ActiveUser.UserCurrency;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception When loading all Incomes; Message : {ex.Message}");
        }
        //FilterGetIncOfCurrentMonth();
        //await FilterGetAllIncomes();
    }

    private async void HandleIncomesListUpdated()
    {
        try
        {
            ApplyChanges();
        }
        catch (Exception ex)
        {
           await Shell.Current.DisplayAlert("Error incomes", ex.Message, "OK");
        }
    }

    private void ApplyChanges()
    {

        var IncList = incomeService.OfflineIncomesList
                    .Where(x => !x.IsDeleted)
                    .OrderByDescending(x => x.DateReceived);
        //ApplyFilters(IncList);       

        IncomesList = new ObservableCollection<IncomeModel>(IncList);
        OnPropertyChanged(nameof(IncomesList));

        RedoCountsAndAmountsCalculations(IncList);
    }

    void RedoCountsAndAmountsCalculations(IEnumerable<IncomeModel> IncList)
    {
        TotalAmount = IncList.AsParallel().Sum(x => x.AmountReceived);
        TotalIncomes = IncList.Count();
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
            var newIncome = new IncomeModel() { DateReceived = DateTime.Now };
            const string PageTitle = "Add New Income";
            const bool isAdd = true;

            await AddEditIncome(newIncome, PageTitle, isAdd);
        }
    }

    [RelayCommand]
    async Task ShowEditIncomePopUp(IncomeModel income)
    {
        await AddEditIncome(income, "Edit Flow In", false);
    }
    async Task AddEditIncome(IncomeModel newIncome, string pageTitle, bool isAdd)
    {
        var newUpserIncomeVM = new UpSertIncomeVM(incomeService, userService, newIncome, pageTitle, isAdd, ActiveUser);
        await Shell.Current.ShowPopupAsync(new UpSertIncomePopUp(newUpserIncomeVM));
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
    void SearchIncomes()
    {
        try
        {
            var orderIncCollection = incomeService.OfflineIncomesList
                .Where(inc => !inc.IsDeleted)
                .OrderByDescending(inc => inc.DateReceived);
            IEnumerable<IncomeModel> filteredCollectionOfInc = ApplyFilters(orderIncCollection);

            IncomesList = new ObservableCollection<IncomeModel>(filteredCollectionOfInc);
            RedoCountsAndAmountsCalculations(filteredCollectionOfInc);
        }
        catch (Exception ex)
        {

            Debug.WriteLine($"Search Flow IN Exception : {ex.Message}");
        }
    }

    [RelayCommand]
    void ClearFilters()
    {
        SearchText = string.Empty;
        SearchText = string.Empty;
        SearchStartDate = null;
        SearchEndDate = null;
        SearchMinPrice = null;
        SearchMaxPrice = null;
        ApplyChanges();

    }
    private IEnumerable<IncomeModel> ApplyFilters(IEnumerable<IncomeModel> incomeCollection)
    {
        IEnumerable<IncomeModel> filterIncCollection = incomeCollection;
        try
        {
            filterIncCollection = FilterByText(filterIncCollection);
            filterIncCollection = FilterByDateRange(filterIncCollection);
            filterIncCollection = FilterByAmountRange(filterIncCollection);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Filter Flow In Exception : {ex.Message}");
        }

        return filterIncCollection;
    }

    IEnumerable<IncomeModel> FilterByAmountRange(IEnumerable<IncomeModel> incomesCollection)
    {
        if (SearchMinPrice.HasValue && SearchMinPrice > 0)
        {
            incomesCollection = incomesCollection
                .Where(exp => exp.AmountReceived >= SearchMinPrice.Value);
        }
        if (SearchMaxPrice.HasValue && SearchMaxPrice > 0)
        {
            incomesCollection = incomesCollection
                .Where(exp => exp.AmountReceived <= SearchMaxPrice.Value);
        }

        return incomesCollection;
    }

    IEnumerable<IncomeModel> FilterByDateRange(IEnumerable<IncomeModel> incomesCollection)
    {

        // If both start and end dates are null, return the original list
        if (SearchStartDate == null && SearchEndDate == null)
        {
            return incomesCollection;
        }

        // Filter by start date if it's not null
        if (SearchStartDate != null)
        {
            incomesCollection = incomesCollection.Where(exp => exp.DateReceived >= SearchStartDate.Value);
        }

        // Filter by end date if it's not null
        if (SearchEndDate != null)
        {
            incomesCollection = incomesCollection.Where(exp => exp.DateReceived <= SearchEndDate.Value);
        }

        return incomesCollection;
    }

    IEnumerable<IncomeModel> FilterByText(IEnumerable<IncomeModel> incomesCollection)
    {
        if (string.IsNullOrEmpty(SearchText))
        {
            return incomesCollection.Where(inc => !inc.IsDeleted);
        }

        return incomesCollection
            .Where(inc => inc.Reason?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false)
            .Where(inc => !inc.IsDeleted);
    }

    [RelayCommand]
    public async Task PrintIncomesBtn()
    {
        if (IncomesList?.Count < 1)
        {
            await Shell.Current.ShowPopupAsync(new ErrorPopUpAlert("Cannot Save an Empty List to PDF"));
        }
        else
        {
            await Shell.Current.ShowPopupAsync(new ErrorPopUpAlert("Saved"));
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
    public void ShowFilterPopUpPage()
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
