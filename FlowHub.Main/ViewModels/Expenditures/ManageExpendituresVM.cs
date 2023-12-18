
//This is the view model for the page that shows ALL expenditures

namespace FlowHub.Main.ViewModels.Expenditures;
public partial class ManageExpendituresVM : ObservableObject
{
    readonly IExpendituresRepository expendituresService;
    readonly IUsersRepository userRepo;
    private readonly UpSertExpenditureVM upSertExpenditureVM;

    public ManageExpendituresVM(IExpendituresRepository expendituresRepository, IUsersRepository usersRepository,
        UpSertExpenditureVM upSertExpenditureVM)
    {
        expendituresService = expendituresRepository;
        userRepo = usersRepository;
        this.upSertExpenditureVM = upSertExpenditureVM;

        ListOfExpenditureCategories = Enum.GetValues(typeof(ExpenditureCategory)).Cast<ExpenditureCategory>().ToList();
        //ExpendituresCat = ExpenditureCategoryDescriptions.Descriptions;
        expendituresService.OfflineExpendituresListChanged += HandleExpendituresListUpdated;
        userRepo.OfflineUserDataChanged += HandleUserDataChanged;
    }

    private void HandleUserDataChanged()
    {
        UserPocketMoney = userRepo.OfflineUser.PocketMoney;
    }

    [ObservableProperty]
    List<ExpenditureCategory> listOfExpenditureCategories;
    [ObservableProperty]
    ObservableCollection<ExpendituresModel> expendituresCollection;

    [ObservableProperty]
    ObservableCollection<DateGroup> groupedExpenditures;

    [ObservableProperty]
    double totalAmount;

    [ObservableProperty]
    int totalExpenditures;

    [ObservableProperty]
    string userCurrency;

    [ObservableProperty]
    double userPocketMoney;

    [ObservableProperty]
    bool isBusy = true;

    [ObservableProperty]
    string expTitle;

    [ObservableProperty]
    UsersModel activeUser;

    [ObservableProperty]
    bool activ;
    [ObservableProperty]
    bool showStatisticBtn;

    [ObservableProperty]
    bool isSyncing;

    //Search variables section
    [ObservableProperty]
    List<ExpenditureCategory> expendituresCatsFilters = [];
    [ObservableProperty]
    ObservableCollection<ExpenditureCategory> selectedExpCatsFilters = [];
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
    public void Pageloaded()
    {
        try
        {
            if (!IsLoaded)
            {                
                ApplyChanges();
                
                ExpTitle = "All Flow Outs";
                IsLoaded = true;
                IsBusy = false;
                ActiveUser = userRepo.OfflineUser;
                UserPocketMoney = ActiveUser.PocketMoney;
                UserCurrency = ActiveUser.UserCurrency;
                
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception When loading all expenditures; Message : {ex.Message}");
        }

    }

    [ObservableProperty]
    public int startAction;
    //[RelayCommand]
    //Function to show very single expenditure from DB

    public void ApplyChanges()
    {
        // Update expList
        var expList = expendituresService.OfflineExpendituresList
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.DateSpent);
            //.ToList();
        ApplyFilters(expList);
#if ANDROID
        // Update groupedData
        var groupedData = expList.GroupBy(e => e.DateSpent.Date)
            .Select(g => new DateGroup(g.Key, [.. g]))
            .ToList();

        // Update GroupedExpenditures
        GroupedExpenditures = new ObservableCollection<DateGroup>(groupedData);
        OnPropertyChanged(nameof(GroupedExpenditures));
#endif

#if WINDOWS
        // Update ExpendituresList
        ExpendituresCollection = new ObservableCollection<ExpendituresModel>(expList);
#endif

        RedoCountsAndAmountsCalculations(expList);
        
    }
    private async void HandleExpendituresListUpdated()
    {
        try
        {
            ApplyChanges();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error Exp", ex.Message, "OK");
        }
    }
    private void RedoCountsAndAmountsCalculations(IEnumerable<ExpendituresModel> expList)
    {
        // Update TotalAmount
        TotalAmount = expList.AsParallel().Sum(x => x.AmountSpent);

        // Update TotalExpenditures
        TotalExpenditures = expList.Count();

        // Update ShowStatisticBtn
        ShowStatisticBtn = expList.Count() >= 3;
    }

    [RelayCommand]
    public async Task ShowAddExpenditurePopUp()
    {
        if (ActiveUser is null)
        {
            Debug.WriteLine("Can't Open Add Exp PopUp user is null");
            await Shell.Current.DisplayAlert("Wait", "Cannot go", "Ok");
        }
        else
        {
            var newExpenditure = new ExpendituresModel() { DateSpent = DateTime.Now };
            
            upSertExpenditureVM.SingleExpenditureDetails = newExpenditure;
            upSertExpenditureVM.ClosePopUp = false;
            await AddEditExpediture();
        }
    }

    [RelayCommand]
    public async Task ShowEditExpenditurePopUp(ExpendituresModel expenditure)
    {
        upSertExpenditureVM.SingleExpenditureDetails = expenditure;
        upSertExpenditureVM.ClosePopUp = false;
        await AddEditExpediture();
    }
    private async Task AddEditExpediture()
    {
        var result = (PopUpCloseResult) await Shell.Current.ShowPopupAsync(new UpSertExpendituresPopUp(upSertExpenditureVM));
        if(result.Result == PopupResult.OK)
        {
            var resultingBalance = (double)result.Data;
            UserPocketMoney = resultingBalance;
        }
    }

    [RelayCommand]
    public async Task DeleteExpenditureBtn(ExpendituresModel expenditure)
    {
        CancellationTokenSource cancellationTokenSource = new();
        const ToastDuration duration = ToastDuration.Short;
        const double fontSize = 14;
        string text;
        bool response = (bool)(await Shell.Current.ShowPopupAsync(new AcceptCancelPopUpAlert("Confirm Delete ?")))!;
        if (response)
        {
            IsBusy = true;
            expenditure.UpdatedDateTime = DateTime.UtcNow;
            expenditure.PlatformModel = DeviceInfo.Current.Model;
            var deleteResponse = await expendituresService.DeleteExpenditureAsync(expenditure); //delete the expenditure from db

            if (deleteResponse)
            {
                text = "Flow Out Deleted Successfully";
                ActiveUser.TotalExpendituresAmount -= expenditure.AmountSpent;
                ActiveUser.PocketMoney += expenditure.AmountSpent;
                UserPocketMoney += expenditure.AmountSpent;
                await userRepo.UpdateUserAsync(ActiveUser);
            }
            else
            {
                text = "Flow Out Not Deleted";
            }
            var toast = Toast.Make(text, duration, fontSize);
            await toast.Show(cancellationTokenSource.Token); //toast a notification about exp deletion
            ApplyChanges();
            IsBusy = false;
        }
    }

    [RelayCommand]
    void SearchExpenditures()
    {
        try
        {
            var orderedExpCollection = expendituresService.OfflineExpendituresList
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.DateSpent);

            IEnumerable<ExpendituresModel> filteredCollectionOfExp = ApplyFilters(orderedExpCollection);
            
#if WINDOWS
            ExpendituresCollection = new ObservableCollection<ExpendituresModel>(filteredCollectionOfExp);
#endif
            RedoCountsAndAmountsCalculations(filteredCollectionOfExp);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Search Flow Out Exception : {ex.Message}");
        }
    }

    [RelayCommand]
    void ClearFilters()
    {
        SearchText = string.Empty;
        SearchStartDate = null;
        SearchEndDate = null;
        SearchMinPrice = null;
        SearchMaxPrice = null;
        SelectedExpCatsFilters.Clear();
        ApplyChanges();
    }

    private IEnumerable<ExpendituresModel> ApplyFilters(IEnumerable<ExpendituresModel> expendituresCollection)
    {
        IEnumerable<ExpendituresModel> filteredExpCollection = expendituresCollection;
        try
        {
            filteredExpCollection = FilterByText(filteredExpCollection);
            filteredExpCollection = FilterByCategory(filteredExpCollection);
            filteredExpCollection = FilterByDateRange(filteredExpCollection);
            filteredExpCollection = FilterByAmountRange(filteredExpCollection);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Filter Flow Out Exception : {ex.Message}");
        }
        return filteredExpCollection;
    }

    private IEnumerable<ExpendituresModel> FilterByCategory(IEnumerable<ExpendituresModel> expendituresCollection)
    {
        if (SelectedExpCatsFilters is not null && SelectedExpCatsFilters.Count != 0)
        {
            expendituresCollection = expendituresCollection
                .Where(exp => SelectedExpCatsFilters.Contains(exp.Category));        
        }

        return expendituresCollection;
    }

    IEnumerable<ExpendituresModel> FilterByDateRange(IEnumerable<ExpendituresModel> expendituresCollection)
    {
        // If both start and end dates are null, return the original list
        if (SearchStartDate == null && SearchEndDate == null)
        {
            return expendituresCollection;
        }

        // Filter by start date if it's not null
        if (SearchStartDate != null)
        {
            expendituresCollection = expendituresCollection.Where(exp => exp.DateSpent >= SearchStartDate.Value);
        }

        // Filter by end date if it's not null
        if (SearchEndDate != null)
        {
            expendituresCollection = expendituresCollection.Where(exp => exp.DateSpent <= SearchEndDate.Value);
        }

        return expendituresCollection;
    }

    IEnumerable<ExpendituresModel> FilterByAmountRange(IEnumerable<ExpendituresModel> expendituresCollection)
    {
        if (SearchMinPrice.HasValue && SearchMinPrice > 0)
        {
            expendituresCollection = expendituresCollection
                .Where(exp => exp.AmountSpent >= SearchMinPrice.Value);
        }
        if (SearchMaxPrice.HasValue && SearchMaxPrice > 0)
        {
            expendituresCollection = expendituresCollection
                .Where(exp => exp.AmountSpent <= SearchMaxPrice.Value);
        }

        return expendituresCollection;
    }
    private IEnumerable<ExpendituresModel> FilterByText(IEnumerable<ExpendituresModel> expendituresCollection)
    {
        // If SearchText is null or empty, return the collection without filtering by text
        if (string.IsNullOrEmpty(SearchText))
        {
            return expendituresCollection.Where(exp => !exp.IsDeleted);
        }

        return expendituresCollection.
                Where(exp => exp.Reason?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false)
                .Where(exp => !exp.IsDeleted);
    }

    [RelayCommand]
    void ExpenditureCategoryFilterChanged(object sender)
    {
        if (sender is ExpenditureCategory selectedCategory && !SelectedExpCatsFilters.Contains(selectedCategory))
        {
            SelectedExpCatsFilters?.Add(selectedCategory);
        }

    }
    [RelayCommand]
    void ExpenditureCategoryChipDestroyed(object sender)
    {
        if (sender is UraniumUI.Material.Controls.Chip chip && chip.BindingContext is ExpenditureCategory category)
        {
            SelectedExpCatsFilters?.Remove(category);
        }
       // FilteredExpenditureCategories.Remove(expenditureCategory);
        //ApplyFilters(ExpendituresCollection);
    }

    [RelayCommand]
    public async Task GoToSpecificStatsPage()
    {
        if (GroupedExpenditures is null)
        {
            await Shell.Current.ShowPopupAsync(new ErrorPopUpAlert("No Data to visualize"));
            return;
        }

        var navParam = new Dictionary<string, object>
        {
            { "GroupedExpList", GroupedExpenditures }
        };

        await ManageExpendituresNavs.FromManageExpToSingleMonthStats(navParam);
    }
    public async Task PrintExpendituresBtn()
    {
        Activ = true;
#if ANDROID
        ExpendituresCollection = GroupedExpenditures.SelectMany(x => x).ToObservableCollection();
#endif

        if (ExpendituresCollection?.Count < 1 || ExpendituresCollection is null)
        {
            await Shell.Current.ShowPopupAsync(new ErrorPopUpAlert("Cannot save an Empty list to PDF"));
            return;
        }
        string dialogueResponse = (string)await Shell.Current.ShowPopupAsync(new InputCurrencyForPrintPopUpPage("Please Select Currency", UserCurrency));
        if (dialogueResponse is "Cancel")
        {
            return;
        }

        if (dialogueResponse != UserCurrency && !Connectivity.NetworkAccess.Equals(NetworkAccess.Internet))
        {
            await Shell.Current.ShowPopupAsync(new ErrorPopUpAlert("No Internet !\nPlease Connect to the Internet in order to save in other currencies"));
            return;
        }
        await PrintExpenditures.SaveExpenditureToPDF(ExpendituresCollection, ActiveUser.UserCurrency, dialogueResponse, ActiveUser.Username);
    }

    [RelayCommand]
    public static async Task CopyToClipboard(ExpendituresModel singlExp)
    {
        await Clipboard.SetTextAsync($"{singlExp.Reason} : {singlExp.AmountSpent}");
        CancellationTokenSource cancellationTokenSource = new();
        const ToastDuration duration = ToastDuration.Short;
        const double fontSize = 14;
        const string text = "Flow Out Details Copied to Clipboard";
        var toast = Toast.Make(text, duration, fontSize);
        await toast.Show(cancellationTokenSource.Token); //toast a notification about exp being copied to clipboard
    }

    [RelayCommand]
    public async Task DropCollection()
    {
        await expendituresService.DropExpendituresCollection();
    }

    
}

public class DateGroup : List<ExpendituresModel>
{
    public DateTime Date { get; set; }
    public double TotalAmount { get; set; }
    public int TotalCount { get; set; }
    public string Currency { get; }
    public DateGroup(DateTime date, List<ExpendituresModel> expenditures) : base(expenditures)
    {
        Date = date;
        TotalAmount = expenditures.Sum(x => x.AmountSpent);
        TotalCount = expenditures.Count;
        Currency = expenditures[0].Currency;
    }
}